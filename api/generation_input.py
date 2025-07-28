from pydantic import BaseModel

class GenerationInput(BaseModel):
    message: str
    use_ssml: bool = False
    voice_id: str