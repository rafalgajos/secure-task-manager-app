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
        token = request.headers.get('Authorization').split(" ")[1]
        try:
            data = jwt.decode(token, SECRET_KEY, algorithms=["HS256"])
            current_user = User.query.get(data['user_id'])
        except:
            return jsonify({'message': 'Token is invalid!'}), 403
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
    new_task = Task(
        title=data.get('title'),
        description=data.get('description'),
        due_date=datetime.fromisoformat(data.get('due_date')) if data.get('due_date') else None,
        completed=data.get('completed', False),
        user_id=current_user.id
    )
    db.session.add(new_task)
    db.session.commit()
    return jsonify({"message": "Task created successfully"}), 201

@tasks_api.route('/tasks/<int:id>', methods=['PUT'])
def update_task(id):
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
def delete_task(id):
    task = Task.query.get_or_404(id)
    db.session.delete(task)
    db.session.commit()
    return jsonify({"message": "Task deleted successfully"})
