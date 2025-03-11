from fastapi import FastAPI

app = FastAPI()

@app.get("/")
async def root():
    print("API is work", flush=True)