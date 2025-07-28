from pydantic import BaseModel

class FileNotification(BaseModel):
    job_id: str
    filename: str
    success: bool