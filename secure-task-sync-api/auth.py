from flask import Blueprint, request, jsonify
from config import Config
from models import db, User
import jwt  # pip install PyJWT
import datetime
import logging

auth = Blueprint('auth', __name__)
SECRET_KEY = Config.SECRET_KEY

# Konfiguracja logowania
logging.basicConfig(level=logging.DEBUG)

@auth.route('/auth/register', methods=['POST'])
def register():
    logging.info("Received registration request")
    try:
        data = request.json
        logging.debug(f"Received registration data: {data}")

        if not data or 'username' not in data or 'password' not in data:
            logging.error("Missing username or password in registration data")
            return jsonify({"message": "Missing username or password"}), 400

        # Check if username already exists
        if User.query.filter_by(username=data['username']).first():
            logging.error("Username already exists")
            return jsonify({"message": "Username already exists"}), 400

        new_user = User(username=data['username'])
        new_user.set_password(data['password'])

        db.session.add(new_user)
        db.session.commit()

        logging.info(f"User registered successfully: {data['username']}")
        return jsonify({"message": "User registered successfully"}), 201
    except Exception as e:
        logging.error(f"Error during registration: {str(e)}")
        return jsonify({"message": f"Registration failed", "error": str(e)}), 500


@auth.route('/login', methods=['POST'])
def login():
    try:
        data = request.json
        logging.debug(f"Received login data: {data}")

        # Znajdź użytkownika na podstawie nazwy użytkownika
        user = User.query.filter_by(username=data['username']).first()

        # Sprawdź poprawność hasła
        if user and user.check_password(data['password']):
            # Generowanie tokena JWT z 'user_id' i czasem wygaśnięcia
            token = jwt.encode({
                'user_id': user.id,
                'exp': datetime.datetime.utcnow() + datetime.timedelta(hours=1)
            }, SECRET_KEY, algorithm="HS256")

            logging.info(f"User logged in successfully: {data['username']}")
            logging.debug(f"Generated JWT token: {token}")

            # Zwróć token do klienta
            return jsonify({'token': token}), 200

        logging.warning("Invalid credentials provided")
        return jsonify({"message": "Invalid credentials"}), 401

    except jwt.ExpiredSignatureError:
        logging.error("Token expired")
        return jsonify({"message": "Token has expired"}), 401
    except jwt.InvalidTokenError as e:
        logging.error(f"Invalid token error: {str(e)}")
        return jsonify({"message": "Invalid token"}), 401
    except Exception as e:
        logging.error(f"Error during login: {str(e)}")
        return jsonify({"message": "Login failed", "error": str(e)}), 500
