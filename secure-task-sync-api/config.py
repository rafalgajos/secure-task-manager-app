import os
import secrets

BASE_DIR = os.path.abspath(os.path.dirname(__file__))

class Config:
    SECRET_KEY = os.environ.get("SECRET_KEY") or secrets.token_hex(16)
    SQLALCHEMY_DATABASE_URI = "sqlite:///" + os.path.join(BASE_DIR, "tasks.db")
    SQLALCHEMY_TRACK_MODIFICATIONS = False

    # Konfiguracja ciasteczek sesji
    SESSION_COOKIE_SECURE = True  # Dostępne tylko przez HTTPS
    SESSION_COOKIE_HTTPONLY = True  # Niedostępne przez JavaScript
    SESSION_COOKIE_SAMESITE = 'Lax'  # Ciasteczka są dostępne tylko na tej samej witrynie

    # Wymuś HTTPS
    PREFERRED_URL_SCHEME = 'https'
