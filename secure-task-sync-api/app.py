from flask import Flask, jsonify, render_template, redirect, url_for, request
from flask_cors import CORS
from config import Config
from models import db
from tasks_api import tasks_api
from auth import auth
import os

app = Flask(__name__)
app.config.from_object(Config)

# Database initialization
db.init_app(app)

# CORS support
CORS(app)

# Blueprint registration
app.register_blueprint(tasks_api)
app.register_blueprint(auth)

# Global variable to toggle clickjacking protection
clickjacking_protection_enabled = True

# Clickjacking protection headers
@app.after_request
def set_security_headers(response):
    global clickjacking_protection_enabled
    if clickjacking_protection_enabled:
        response.headers['X-Frame-Options'] = 'DENY'
        response.headers['Content-Security-Policy'] = "frame-ancestors 'none';"
    else:
        response.headers.pop('X-Frame-Options', None)
        response.headers.pop('Content-Security-Policy', None)
    return response

# Endpoint to toggle clickjacking protection
@app.route('/toggle_clickjacking_protection', methods=['POST'])
def toggle_clickjacking_protection():
    global clickjacking_protection_enabled
    clickjacking_protection_enabled = not clickjacking_protection_enabled
    status = "enabled" if clickjacking_protection_enabled else "disabled"
    return jsonify({"message": f"Clickjacking protection {status}."})

# Static route for registration form
@app.route('/register_form', methods=['GET'])
def register_form():
    global clickjacking_protection_enabled
    # Jeśli ochrona przed clickjackingiem jest wyłączona, przekieruj na clickjacking_attack
    if not clickjacking_protection_enabled:
        return redirect(url_for('clickjacking_attack'))
    return render_template("register.html")

# Endpoint to render register_html
@app.route('/register_html', methods=['GET'])
def register_render():
    return render_template("register.html")

# Endpoint for the clickjacking attack
@app.route('/clickjacking_attack', methods=['GET'])
def clickjacking_attack():
    # Wczytaj URL z pliku ngrok_url.txt
    try:
        with open("ngrok_url.txt", "r") as file:
            ngrok_url = file.read().strip()
    except FileNotFoundError:
        ngrok_url = "URL not found"  # Wartość domyślna, jeśli plik nie istnieje

    # Przekaż URL do szablonu
    return render_template("clickjacking_attack.html", ngrok_url=ngrok_url)

if __name__ == '__main__':
    with app.app_context():
        db.create_all()  # Create database tables
    app.run(ssl_context='adhoc', port=8443)  # Running with HTTPS (SSL)
