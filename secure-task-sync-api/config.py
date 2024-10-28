import os
import secrets

BASE_DIR = os.path.abspath(os.path.dirname(__file__))

class Config:
    SECRET_KEY = os.environ.get("SECRET_KEY") or secrets.token_hex(16)
    if not os.environ.get("SECRET_KEY"):
        print("Warning: Using a fallback SECRET_KEY. Set SECRET_KEY as an environment variable for production.")
    SQLALCHEMY_DATABASE_URI = "sqlite:///" + os.path.join(BASE_DIR, "tasks.db")
    SQLALCHEMY_TRACK_MODIFICATIONS = False

