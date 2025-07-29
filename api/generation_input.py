from pydantic import BaseModel, StrictBool

class GenerationInput(BaseModel):
    message: str
    use_ssml: StrictBool
    voice_id: str