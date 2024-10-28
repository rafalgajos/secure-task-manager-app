from flask import Blueprint, request, jsonify

from config import Config
from models import db, User
import jwt # pip install PyJWT
import datetime

auth = Blueprint('auth', __name__)
SECRET_KEY = 'your_secret_key'

@auth.route('/register', methods=['POST'])
def register():
    data = request.json
    new_user = User(username=data['username'])
    new_user.set_password(data['password'])
    db.session.add(new_user)
    db.session.commit()
    return jsonify({"message": "User registered successfully"}), 201


@auth.route('/login', methods=['POST'])
def login():
    data = request.json
    # Find a user by name
    user = User.query.filter_by(username=data['username']).first()

    # Validate your password
    if user and user.check_password(data['password']):
        # Generation of JWT token with ‘user_id’ and expiry time
        token = jwt.encode({
            'user_id': user.id,
            'exp': datetime.datetime.utcnow() + datetime.timedelta(hours=1)  # Token will expire in one hour
        }, Config.SECRET_KEY, algorithm="HS256")

        # Return the token to the client
        return jsonify({'token': token}), 200

    # If login details are incorrect
    return jsonify({"message": "Invalid credentials"}), 401
