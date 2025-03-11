from fastapi import FastAPI
from pyexpat.errors import messages

app = FastAPI()

@app.get("/")
async def root():
    print("API is work", flush=True)
    return {"Response"}