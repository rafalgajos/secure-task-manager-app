from flask import Blueprint, request, jsonify
from config import Config
from models import db, User
import jwt
import datetime
import logging
from limiter_config import limiter

auth = Blueprint('auth', __name__)
SECRET_KEY = Config.SECRET_KEY

# Login configuration
logging.basicConfig(level=logging.DEBUG)

@auth.route('/auth/register', methods=['POST'])
@limiter.limit("5 per minute")
def register():
    logging.info("Received registration request")
    try:
        data = request.json
        logging.debug(f"Received registration data: {data}")

        if not data or 'username' not in data or 'password' not in data:
            logging.error("Missing username or password in registration data")
            return jsonify({"message": "Missing username or password"}), 400

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
@limiter.limit("10 per minute")
def login():
    try:
        data = request.json
        logging.debug(f"Received login data: {data}")

        user = User.query.filter_by(username=data['username']).first()
        if user and user.check_password(data['password']):
            token = jwt.encode({
                'user_id': user.id,
                'exp': datetime.datetime.now(datetime.timezone.utc) + datetime.timedelta(hours=1)
            }, SECRET_KEY, algorithm="HS256")

            logging.info(f"User logged in successfully: {data['username']}")
            logging.debug(f"Generated JWT token: {token}")

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
