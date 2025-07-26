from fastapi.staticfiles import StaticFiles
from fastapi import FastAPI

app = FastAPI()

@app.get("/api/generate")
async def generate():
    return {"message": "HELLO WORLD!"}

app.mount("/", StaticFiles(directory="build", html=True), name="static")