import logging
from datetime import datetime, timezone
from functools import wraps
import jwt
from flask import Blueprint, request, jsonify
from auth import SECRET_KEY
from models import db, Task, User

tasks_api = Blueprint('tasks_api', __name__)

# Improved decorator to handle token authentication with detailed error handling
def token_required(f):
    @wraps(f)
    def decorated(*args, **kwargs):
        token = request.headers.get('Authorization')
        logging.debug(f"Authorization Header: {token}")

        if not token:
            return jsonify({"error": "Authorization header missing"}), 401

        try:
            token = token.split(" ")[1]
            data = jwt.decode(token, SECRET_KEY, algorithms=["HS256"])
            current_user = User.query.get(data['user_id'])
            if not current_user:
                return jsonify({"error": "User not found"}), 404
        except IndexError:
            return jsonify({"error": "Token format is invalid"}), 401
        except jwt.ExpiredSignatureError:
            return jsonify({"error": "Token has expired"}), 401
        except jwt.InvalidTokenError:
            return jsonify({"error": "Invalid token"}), 401

        return f(current_user, *args, **kwargs)

    return decorated

# Get all tasks for the current user
@tasks_api.route('/tasks', methods=['GET'])
@token_required
def get_tasks(current_user):
    try:
        tasks = Task.query.filter_by(user_id=current_user.id).all()
        return jsonify([{
            "id": task.id,
            "title": task.title,
            "description": task.description,
            "due_date": task.due_date.isoformat() if task.due_date else None,
            "completed": task.completed,
            "last_sync_date": task.last_sync_date.isoformat(),
            "location": task.location  # Include location in the response
        } for task in tasks])
    except Exception as e:
        logging.error(f"Error fetching tasks: {str(e)}")
        return jsonify({"error": "Failed to fetch tasks"}), 500

# Add a new task for the current user
@tasks_api.route('/tasks', methods=['POST'])
@token_required
def add_task(current_user):
    data = request.json
    if not data or 'title' not in data:
        return jsonify({"error": "Title is required"}), 400

    try:
        # Check if the task with the same title, description, due date, and completed status already exists for this user
        existing_task = Task.query.filter_by(
            title=data['title'],
            description=data.get('description'),
            due_date=datetime.fromisoformat(data['due_date']) if data.get('due_date') else None,
            completed=data.get('completed', False),
            user_id=current_user.id
        ).first()

        if existing_task:
            return jsonify({"message": "Task with similar properties already exists"}), 200

        # Add new task if not found
        due_date = datetime.fromisoformat(data.get('due_date')) if data.get('due_date') else None
        new_task = Task(
            title=data['title'],
            description=data.get('description'),
            due_date=due_date,
            completed=data.get('completed', False),
            location=data.get('location'),
            user_id=current_user.id
        )
        db.session.add(new_task)
        db.session.commit()
        return jsonify({"message": "Task created successfully", "id": new_task.id}), 201
    except Exception as e:
        logging.error(f"Error adding task: {str(e)}")
        return jsonify({"error": "Failed to create task"}), 500

# Update an existing task
@tasks_api.route('/tasks/<int:task_id>', methods=['PUT'])
@token_required
def update_task(current_user, task_id):
    task = Task.query.filter_by(id=task_id, user_id=current_user.id).first()
    if not task:
        return jsonify({"error": "Task not found"}), 404

    data = request.json
    try:
        task.title = data.get('title', task.title)
        task.description = data.get('description', task.description)
        task.due_date = datetime.fromisoformat(data.get('due_date')) if data.get('due_date') else task.due_date
        task.completed = data.get('completed', task.completed)
        task.location = data.get('location', task.location)  # Update location if provided
        task.last_sync_date = datetime.now(timezone.utc)  # Use timezone-aware UTC datetime
        db.session.commit()
        return jsonify({"message": "Task updated successfully"})
    except Exception as e:
        logging.error(f"Error updating task: {str(e)}")
        return jsonify({"error": "Failed to update task"}), 500

# Delete a task
@tasks_api.route('/tasks/<int:task_id>', methods=['DELETE'])
@token_required
def delete_task(current_user, task_id):
    task = Task.query.filter_by(id=task_id, user_id=current_user.id).first()
    if not task:
        return jsonify({"error": "Task not found"}), 404

    try:
        db.session.delete(task)
        db.session.commit()
        return jsonify({"message": "Task deleted successfully"})
    except Exception as e:
        logging.error(f"Error deleting task: {str(e)}")
        return jsonify({"error": "Failed to delete task"}), 500
