from fastapi.staticfiles import StaticFiles
from fastapi import FastAPI, HTTPException
import uuid, re, json, time, queue, threading, socket
from .generation_request import GenerationRequest
from .file_notification import FileNotification
from lxml import etree

HOST = 'host.docker.internal'
PORT = 8765
RETRY_DELAY = 4

CHARACTER_LIMIT = 2000

app = FastAPI()

message_queue = queue.Queue()

job_statuses = {} # key: job_id, value: { "status": "queued"/"complete"/"error", "filename": str }

def validate_ssml_message(message: str) -> bool:
    try:
        etree.fromstring(message)
        return True
    except etree.XMLSyntaxError:
        return False

def socket_worker():
    s = None
    while True:
        # Ensure connection
        while s is None:
            try:
                print(f"Connecting to {HOST}:{PORT}...")
                s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
                s.connect((HOST, PORT))
                print("Connected.")
            except (ConnectionRefusedError, socket.timeout) as e:
                print(f"{e}. Retrying in {RETRY_DELAY}s...")
                time.sleep(RETRY_DELAY)
            except Exception as e:
                print(f"Unexpected error: {e}")
                time.sleep(RETRY_DELAY)

        # Get a job from the queue
        input, job_id = message_queue.get()
        job_statuses[job_id] = {"status": "queued", "filename": None}

        try:
            # Prepare and send message
            message_dict = input.dict()
            message_dict["job_id"] = job_id
            jsonMessage = json.dumps(message_dict) + "\n"

            print(f"Sending: {jsonMessage.strip()}")
            s.sendall(jsonMessage.encode())

            # Receive response
            data = s.recv(1024)
            print(f"Received from server: {data.decode()}")

        except (ConnectionResetError, BrokenPipeError, socket.timeout) as e:
            print(f"Connection lost: {e}. Will reconnect.")
            s.close()
            s = None  # Force reconnect next loop
            time.sleep(RETRY_DELAY)
            message_queue.put((input, job_id))  # Requeue the job
        except Exception as e:
            print(f"Unexpected error while sending job: {e}")
            time.sleep(RETRY_DELAY)
            message_queue.put((input, job_id))  # Requeue the job
        else:
            message_queue.task_done()


@app.post("/api/generate")
async def generate(input: GenerationRequest):
    message_length = len(input.input.message)

    if (message_length > CHARACTER_LIMIT):
        warning_message = str(message_length) + " inputted, " + str(CHARACTER_LIMIT) + " max"
        return { "status": "invalid_input", "job_id": "-1", "message": "Too many characters! " + warning_message }
    
    if input.input.use_ssml:
        if not validate_ssml_message(input.input.message):
            return {
                "status": "invalid_input",
                "job_id": "-1",
                "message": "Invalid format for SSML"
                }
    
    unique_id = get_job_id(input)
    print("Queuing request with ID " + unique_id)
    message_queue.put((input, unique_id))
    job_statuses[unique_id] = { "status": "queued", "filename": None }
    return { "status": "queued", "job_id": unique_id, "message": "Queued!" }

def get_job_id(request: GenerationRequest):
    base = request.input.message[:16]

    base = re.sub(r'[^a-zA-Z0-9_-]', '_', base).strip('_')

    unique_suffix = uuid.uuid4().hex[:16]

    return f"{base}_{unique_suffix}"

@app.post("/api/notify-file-ready")
async def notify_file_ready(data: FileNotification):
    print("Raw data received:", data)
    if data.job_id not in job_statuses:
        print(f"Unknown job ID: {data.job_id}")
        raise HTTPException(status_code=404, detail="Unknown job ID")

    if data.success:
        print(f"File ready: {data.job_id} -> {data.filename}")
        job_statuses[data.job_id] = {
            "status": "ready",
            "filename": data.filename
        }
        return { "status": "received" }
    else:
        print("Received a failed generation")
        job_statuses[data.job_id] = {
            "status": "error",
            "filename": None
        }
        return { "status": "error" }


@app.get("/api/status/{job_id}")
async def check_status(job_id: str):
    job = job_statuses.get(job_id)
    if not job:
        raise HTTPException(status_code=404, detail="Job ID not found")

    status = job["status"]
    
    if status == "ready":
        return {
            "status": "ready",
            "url": f"/files/{job['filename']}"
        }
    elif status == "error":
        return {
            "status": "error"
        }
    else:
        return {
            "status": "queued"
        }

# START SERVER

app.mount("/files", StaticFiles(directory="output", html=False), name="files")
app.mount("/", StaticFiles(directory="build", html=True), name="static")

threading.Thread(target=socket_worker, daemon=True).start()