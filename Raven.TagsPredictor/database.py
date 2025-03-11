import asyncpg

async def get_db_connection():
    conn = await asyncpg.connect(
        user="admin",
        password = "p",
        database = "TrainingDb",
        host = "postgres-training-data",
        port="5432"
    )
    return  conn