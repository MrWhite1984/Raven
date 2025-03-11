import os
from http.client import HTTPException

from fastapi import FastAPI
from pydantic import BaseModel

from database import get_db_connection

from sklearn.preprocessing import MultiLabelBinarizer
from sklearn.feature_extraction.text import TfidfVectorizer

from sklearn.svm import SVC
from sklearn.multiclass import OneVsRestClassifier

from apscheduler.schedulers.background import BackgroundScheduler
from apscheduler.triggers.interval import IntervalTrigger

import pickle

app = FastAPI()

IS_UPDATING = False

MODEL = None
VECTORIZER = None
MLB = None

def load_model():
    global MODEL, VECTORIZER, MLB
    if not os.path.exists('model.pkl') or not os.path.exists('vectorizer.pkl') or not os.path.exists('mlb.pkl'):
        MODEL, VECTORIZER, MLB = (OneVsRestClassifier(SVC(probability=True)), TfidfVectorizer(), MultiLabelBinarizer())
        with open('model.pkl', 'wb') as f:
            pickle.dump(MODEL, f)

        with open('vectorizer.pkl', 'wb') as f:
            pickle.dump(VECTORIZER, f)

        with open('mlb.pkl', 'wb') as f:
            pickle.dump(MLB, f)
    else:
        with open('model.pkl', 'rb') as f:
            MODEL = pickle.load(f)
        with open('vectorizer.pkl', 'rb') as f:
            VECTORIZER = pickle.load(f)
        with open('mlb.pkl', 'rb') as f:
            MLB = pickle.load(f)
load_model()

class BodyPost(BaseModel):
    body:str

@app.post("/predict/")
async def predict_tags(bodyPost:BodyPost):
    if IS_UPDATING:
        raise HTTPException(status_code=503, detail="Сервер обновляется, попробуйте позже.")
    vectorized_body = VECTORIZER.transform([bodyPost.body])
    prediction = MODEL.predict(vectorized_body)
    predicted_tags = MLB.inverse_transform(prediction)
    return predicted_tags


@app.get("/retrain")
async  def retrain_model():
    print("Запущено переобучение", flush=True)
    data = await get_data_from_db()
    if data == {}:
        print("Нет данных", flush=True)
    else:
        bodies = list(data.keys())
        tags = list(data.values())

        mlb = MultiLabelBinarizer()
        y=mlb.fit_transform(tags)

        vectorizer = TfidfVectorizer()
        x = vectorizer.fit_transform(bodies)

        model = OneVsRestClassifier(SVC(probability=True))
        model.fit(x, y)

        with open('model.pkl', 'wb') as f:
            pickle.dump(model, f)

        with open('vectorizer.pkl', 'wb') as f:
            pickle.dump(vectorizer, f)

        with open('mlb.pkl', 'wb') as f:
            pickle.dump(mlb, f)

        update_params(model, vectorizer, mlb)

        print("Модель переобучена", flush=True)

def update_params(model, vectorizer, mlb):
    global  IS_UPDATING, MODEL, VECTORIZER, MLB
    IS_UPDATING = True
    MODEL = model
    VECTORIZER = vectorizer
    MLB = mlb
    IS_UPDATING = False

async def get_data_from_db():
    try:
        conn = await get_db_connection()
        with open("Request.sql", "r") as file:
            query = file.read()
        data = await conn.fetch(query)
        structured_data = {}
        for part_of_data in data:
            body = part_of_data["Body"]
            tags = part_of_data["Tag"]
            structured_data[body] = tags
        return structured_data

    except Exception as e:
        print(e)


def start_scheduler():
    scheduler = BackgroundScheduler()
    scheduler.add_job(retrain_model, IntervalTrigger(hours=12), id='retrain_job', name='Retrain model every 12 hours')
    scheduler.start()

start_scheduler()