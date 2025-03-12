import uuid

import orjson
from datetime import datetime

import redis

LOGS_KEY = "logs"

async def get_redis_connection():
    r = redis.Redis(host='redis-logs', port='6379')
    return r

async  def log(log_level, message):
    redis = await get_redis_connection()
    data = {"Id":str(uuid.UUID(int=0)),
            "LogSender": "Raven.TagsPredictor",
            "LogLevel":log_level,
            "DateTime":datetime.utcnow(),
            "Message":message
            }
    redis.rpush(LOGS_KEY, orjson.dumps(data).decode())