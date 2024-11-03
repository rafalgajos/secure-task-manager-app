from flask import Flask, jsonify, render_template
from flask_cors import CORS
from config import Config
from models import db
from tasks_api import tasks_api
from auth import auth

app = Flask(__name__)
app.config.from_object(Config)

# Database initialization
db.init_app(app)

# CORS support
CORS(app)

# Blueprint registration
app.register_blueprint(tasks_api)
app.register_blueprint(auth)

# Clickjacking protection headers
@app.after_request
def set_security_headers(response):
    response.headers['X-Frame-Options'] = 'DENY'
    response.headers['Content-Security-Policy'] = "frame-ancestors 'none';"
    return response

# Endpoint for application health
@app.route('/health', methods=['GET'])
def health_check():
    return jsonify({"status": "ok"}), 200

# Static route for registration form
@app.route('/register_form', methods=['GET'])
def register_form():
    return render_template("register.html")

if __name__ == '__main__':
    with app.app_context():
        db.create_all()  # Create database tables
    app.run(ssl_context='adhoc', port=8443)  # Running with HTTPS (SSL)
