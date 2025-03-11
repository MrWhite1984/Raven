FROM python:3.8-slim

WORKDIR /app

COPY . /app


RUN pip install uvicorn
RUN pip install fastapi
RUN pip install pydantic
RUN pip install typing
RUN pip install asyncpg
RUN pip install scikit-learn
RUN pip install APScheduler



EXPOSE 8008

CMD ["uvicorn", "Raven-TagsPredictor:app", "--host", "0.0.0.0", "--port", "8008"]
