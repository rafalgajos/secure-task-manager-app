import os
import secrets

BASE_DIR = os.path.abspath(os.path.dirname(__file__))

class Config:
    SECRET_KEY = os.environ.get("SECRET_KEY") or secrets.token_hex(16)
    SQLALCHEMY_DATABASE_URI = "sqlite:///" + os.path.join(BASE_DIR, "tasks.db")
    SQLALCHEMY_TRACK_MODIFICATIONS = False

    # Configure session cookies
    SESSION_COOKIE_SECURE = True        # Only accessible via HTTPS
    SESSION_COOKIE_HTTPONLY = True      # Not accessible via JavaScript
    SESSION_COOKIE_SAMESITE = 'Lax'     # Cookies are only available on the same site

    # Enforce HTTPS
    PREFERRED_URL_SCHEME = 'https'
