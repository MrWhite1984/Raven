FROM python:3.8-slim

WORKDIR /app

COPY . /app

RUN pip install uvicorn
RUN pip install fastapi

EXPOSE 8008

CMD ["uvicorn", "Raven-TagsPredictor:app", "--host", "0.0.0.0", "--port", "8008"]
