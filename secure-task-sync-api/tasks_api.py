import logging
from datetime import datetime
from functools import wraps
import jwt
from flask import Blueprint, request, jsonify
from auth import SECRET_KEY
from models import db, Task, User

tasks_api = Blueprint('tasks_api', __name__)


def token_required(f):
    @wraps(f)
    def decorated(*args, **kwargs):
        token = request.headers.get('Authorization')

        # Logowanie wartości nagłówka Authorization
        logging.debug(f"Authorization Header: {token}")

        if not token:
            return jsonify({"error": "Authorization header missing"}), 401

        try:
            token = token.split(" ")[1]
            data = jwt.decode(token, SECRET_KEY, algorithms=["HS256"])
            current_user = User.query.get(data['user_id'])
        except IndexError:
            return jsonify({"error": "Token format is invalid"}), 401
        except jwt.ExpiredSignatureError:
            return jsonify({"error": "Token has expired"}), 401
        except jwt.InvalidTokenError:
            return jsonify({"error": "Invalid token"}), 401

        return f(current_user, *args, **kwargs)

    return decorated


@tasks_api.route('/tasks', methods=['GET'])
@token_required
def get_tasks(current_user):
    tasks = Task.query.filter_by(user_id=current_user.id).all()
    return jsonify([{
        "id": task.id,
        "title": task.title,
        "description": task.description,
        "due_date": task.due_date,
        "completed": task.completed,
        "last_sync_date": task.last_sync_date
    } for task in tasks])


@tasks_api.route('/tasks', methods=['POST'])
@token_required
def add_task(current_user):
    data = request.json
    due_date = datetime.fromisoformat(data.get('due_date')) if data.get('due_date') else None
    new_task = Task(
        title=data.get('title'),
        description=data.get('description'),
        due_date=due_date,
        completed=data.get('completed', False),
        user_id=current_user.id
    )
    db.session.add(new_task)
    db.session.commit()
    return jsonify({"message": "Task created successfully", "id": new_task.id}), 201


@tasks_api.route('/tasks/<int:id>', methods=['PUT'])
@token_required
def update_task(current_user, id):
    task = Task.query.get_or_404(id)
    data = request.json
    task.title = data.get('title', task.title)
    task.description = data.get('description', task.description)
    task.due_date = datetime.fromisoformat(data.get('due_date')) if data.get('due_date') else task.due_date
    task.completed = data.get('completed', task.completed)
    task.last_sync_date = datetime.utcnow()
    db.session.commit()
    return jsonify({"message": "Task updated successfully"})



@tasks_api.route('/tasks/<int:id>', methods=['DELETE'])
@token_required
def delete_task(current_user, id):
    task = Task.query.get_or_404(id)
    db.session.delete(task)
    db.session.commit()
    return jsonify({"message": "Task deleted successfully"})
