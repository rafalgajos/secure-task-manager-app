import subprocess
import time
import json
import os

# Path where we will save the generated URL (original path and additional local path)
output_file = os.path.join("..", "secure-task-manager-app", "Resources", "Raw", "ngrok_url.txt")
local_output_file = os.path.join(os.getcwd(), "ngrok_url.txt")  # Save in the current directory

# Run ngrok in HTTPS tunneling mode on port 8443
ngrok_process = subprocess.Popen(
    ["ngrok", "http", "https://127.0.0.1:8443"],
    stdout=subprocess.PIPE,
    stderr=subprocess.STDOUT,
    text=True
)

# Increase waiting time to give ngrok more time to run
time.sleep(5)

# Download the list of tunnels from the ngrok API
try:
    print("Attempting to retrieve ngrok URL from API...")
    result = subprocess.run(["curl", "-s", "http://127.0.0.1:4040/api/tunnels"], capture_output=True, text=True)
    output = result.stdout
    print("API response:", output)  # Display full response for diagnostics

    # Parsing JSON from API response
    data = json.loads(output)
    if "tunnels" in data and len(data["tunnels"]) > 0:
        # Get the first tunnel and its public URL
        ngrok_url = data["tunnels"][0].get("public_url", None)

        if ngrok_url:
            # Save the URL to the original file path
            with open(output_file, "w") as file:
                file.write(ngrok_url)
            print(f"ngrok URL saved to {output_file}: {ngrok_url}")

            # Save the URL to the local file path
            with open(local_output_file, "w") as local_file:
                local_file.write(ngrok_url)
            print(f"ngrok URL also saved to {local_output_file}: {ngrok_url}")

        else:
            print("No public URL found in the API response.")
    else:
        print("No tunnels found in the API response.")
except json.JSONDecodeError as e:
    print(f"Failed to parse JSON: {e}")
except Exception as e:
    print(f"Error occurred: {e}")

# View ngrok process logs in real time
try:
    for line in ngrok_process.stdout:
        print(line, end="")  # Display each line of ngrok output
except KeyboardInterrupt:
    print("\nNgrok process interrupted.")

# When finished, close the ngrok process
ngrok_process.terminate()
