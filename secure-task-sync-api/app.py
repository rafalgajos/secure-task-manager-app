from flask import Flask, jsonify
from flask_cors import CORS
from config import Config
from models import db
from tasks_api import tasks_api
from auth import auth  # Import blueprint auth

app = Flask(__name__)
app.config.from_object(Config)

# Database initialisation
db.init_app(app)

# CORS support
CORS(app)

# Blueprint registration
app.register_blueprint(tasks_api)
app.register_blueprint(auth)  # Blueprint auth registration

# Clickjacking protection headers
@app.after_request
def set_security_headers(response):
    response.headers['X-Frame-Options'] = 'DENY'
    response.headers['Content-Security-Policy'] = "frame-ancestors 'none';"
    return response

# Endpoint application health
@app.route('/health', methods=['GET'])
def health_check():
    return jsonify({"status": "ok"}), 200

if __name__ == '__main__':
    with app.app_context():
        db.create_all()  # Create a database table
    app.run(ssl_context='adhoc', port=8443)  # Running the application with HTTPS (SSL)
