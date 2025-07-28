from pydantic import BaseModel
from .generation_input import GenerationInput

class GenerationRequest(BaseModel):
    input: GenerationInput