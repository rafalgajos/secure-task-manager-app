from flask import Flask, jsonify, render_template, redirect, url_for, request
from flask_cors import CORS
from config import Config
from models import db
from tasks_api import tasks_api
from auth import auth
import os
from limiter_config import limiter  # Import limiter z limiter_config

app = Flask(__name__)
app.config.from_object(Config)

# Initialize limiter with app (app initialization happens after limiter is imported)
limiter.init_app(app)

# Database initialization
db.init_app(app)

# CORS support (restricted to specific domains for security)
CORS(app, resources={
    r"/*": {"origins": ["https://127.0.0.1:8443"]}
})

# Blueprint registration
app.register_blueprint(tasks_api)
app.register_blueprint(auth)

# Global variables to toggle protections
clickjacking_protection_enabled = True
js_clickjacking_protection_enabled = True


# Clickjacking protection headers and security improvements
@app.after_request
def set_security_headers(response):
    global clickjacking_protection_enabled, js_clickjacking_protection_enabled
    if clickjacking_protection_enabled:
        response.headers['X-Frame-Options'] = 'DENY'
        response.headers['Content-Security-Policy'] = "frame-ancestors 'none';"
    else:
        response.headers.pop('X-Frame-Options', None)
        response.headers.pop('Content-Security-Policy', None)

    # Add JavaScript clickjacking protection if enabled
    if js_clickjacking_protection_enabled:
        response_data = response.get_data(as_text=True)
        response_data = response_data.replace(
            '</head>',
            '<script>if (window.top !== window.self) { window.top.location = window.self.location; }</script></head>'
        )
        response.set_data(response_data)

    # Additional security headers
    response.headers['Strict-Transport-Security'] = 'max-age=31536000; includeSubDomains'
    response.headers['X-Content-Type-Options'] = 'nosniff'
    response.headers['Cache-Control'] = 'no-store, no-cache, must-revalidate, max-age=0'
    response.headers['Pragma'] = 'no-cache'
    response.headers['Server'] = 'SecureServer'

    return response


# Endpoint to toggle clickjacking protection headers
@app.route('/toggle_clickjacking_protection', methods=['POST'])
def toggle_clickjacking_protection():
    global clickjacking_protection_enabled
    clickjacking_protection_enabled = not clickjacking_protection_enabled
    status = "enabled" if clickjacking_protection_enabled else "disabled"
    return jsonify({"message": f"Clickjacking protection {status}."})


# Endpoint to toggle JavaScript clickjacking protection
@app.route('/toggle_js_clickjacking_protection', methods=['POST'])
def toggle_js_clickjacking_protection():
    global js_clickjacking_protection_enabled
    js_clickjacking_protection_enabled = not js_clickjacking_protection_enabled
    status = "enabled" if js_clickjacking_protection_enabled else "disabled"
    return jsonify({"message": f"JavaScript clickjacking protection {status}."})


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
