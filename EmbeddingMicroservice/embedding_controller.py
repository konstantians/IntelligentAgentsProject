from typing import Dict
from fastapi import FastAPI
from sentence_transformers import SentenceTransformer
from pydantic import BaseModel, Field, field_validator

app = FastAPI()

model = SentenceTransformer('all-MiniLM-L6-V2')

class EmbeddingRequestModel(BaseModel):
    id_description_pairs: Dict[str, str] = Field(..., alias="idDescriptionPairs")

    @field_validator('id_description_pairs')
    def check_not_empty(cls, v):
        if not v:
            raise ValueError('idDescriptionPairs dictionary must not be empty')
        return v


@app.post("/createEmbeddings")
def create_embeddings(request : EmbeddingRequestModel):
    ids = list(request.id_description_pairs.keys())
    descriptions = list(request.id_description_pairs.values())
    embeddings = model.encode(descriptions).tolist()

    # Unreadable, but essentially this puts the embeddings, back with the ids, 
    # so it returns a Dictionary<string, float[]>, with keys being the ids and values being the embeddings
    response = {id_: embedding for id_, embedding in zip(ids, embeddings)}

    return response