#!/usr/bin/env python3
import hmac
import uuid
import datetime
import subprocess
from hashlib import sha256
import os
import sys
import json

# Ensure the python-dotenv package is installed
try:
    from dotenv import load_dotenv
except ImportError:
    subprocess.check_call([sys.executable, "-m", "pip", "install", "python-dotenv"])
    from dotenv import load_dotenv

AUTH_SCHEMA_NAME = "NHSMESH "  # Keep the trailing space
ACCEPT_HEADER = "accept: application/vnd.mesh.v2+json"

def build_auth_header(mailbox_id: str, password: str, shared_key: str, nonce: str = None, nonce_count: int = 0):
    """Generate MESH Authorization header for the given mailbox ID."""
    if not nonce:
        nonce = str(uuid.uuid4())
    timestamp = datetime.datetime.now(datetime.timezone.utc).strftime("%Y%m%d%H%M")
    hmac_msg = f"{mailbox_id}:{nonce}:{nonce_count}:{password}:{timestamp}"
    hash_code = hmac.HMAC(shared_key.encode(), hmac_msg.encode(), sha256).hexdigest()
    return f"{AUTH_SCHEMA_NAME}{mailbox_id}:{nonce}:{nonce_count}:{timestamp}:{hash_code}"

def load_env_vars():
    """Load environment variables from .env file."""
    load_dotenv()

    env_vars = {
        "MAILBOX_ID_FROM": os.getenv("MAILBOX_ID_FROM"),
        "MAILBOX_FROM_PASSWORD": os.getenv("MAILBOX_FROM_PASSWORD"),
        "MAILBOX_ID_TO": os.getenv("MAILBOX_ID_TO"),
        "MAILBOX_TO_PASSWORD": os.getenv("MAILBOX_TO_PASSWORD"),
        "SHARED_KEY": os.getenv("SHARED_KEY"),
        "FILE_PATH": os.getenv("FILE_PATH"),
        "CERT_PATH": os.getenv("CERT_PATH"),
        "KEY_PATH": os.getenv("KEY_PATH"),
        "WORKFLOW_ID": os.getenv("WORKFLOW_ID")
    }

    missing_vars = [var for var, value in env_vars.items() if value is None]

    if missing_vars:
        print(f"\nError: The following required environment variables are not set: {', '.join(missing_vars)}")
        sys.exit(1)

    return env_vars

def send_to_outbox(env_vars):
    # Generate authentication token for outbox
    auth_token_outbox = build_auth_header(env_vars["MAILBOX_ID_FROM"], env_vars["MAILBOX_FROM_PASSWORD"], env_vars["SHARED_KEY"])

    # Print the authentication token for outbox
    print(f"\nGenerating Authentication Token (Outbox): {auth_token_outbox}")

    # Extract the filename from the FILE_PATH
    file_name = os.path.basename(env_vars["FILE_PATH"])

    # Determine if the file is gzipped
    is_gzipped = file_name.endswith(".gz")

    # Perform the curl command to send the file to the outbox and capture the response
    curl_command_outbox = [
        "curl", "-k",
        "--request", "POST",
        "--cert", env_vars["CERT_PATH"],
        "--key", env_vars["KEY_PATH"],
        "--header", ACCEPT_HEADER,
        "--header", f"authorization: {auth_token_outbox}",
        "--header", "content-type: text/csv",
        "--header", f"mex-from: {env_vars['MAILBOX_ID_FROM']}",
        "--header", f"mex-to: {env_vars['MAILBOX_ID_TO']}",
        "--header", f"mex-workflowid: {env_vars['WORKFLOW_ID']}",
        "--header", f"mex-filename: {file_name}",
        "--header", "mex-localid: testing123",
        "--data-binary", f"@{env_vars['FILE_PATH']}",
        f"https://msg.intspineservices.nhs.uk/messageexchange/{env_vars['MAILBOX_ID_FROM']}/outbox"
    ]

    # Add the content-encoding header if the file is gzipped
    if is_gzipped:
        curl_command_outbox.insert(9, "--header")
        curl_command_outbox.insert(10, "content-encoding: gzip")

    # Echo the curl command
    print(f"\nExecuting curl command: {' '.join(curl_command_outbox)}")

    result_outbox = subprocess.run(curl_command_outbox, capture_output=True, text=True)
    print(f"\nResult from sending to Mesh Mailbox {env_vars['MAILBOX_ID_FROM']} (Outbox): {result_outbox.stdout.strip()}")

def view_inbox(env_vars):
    # Generate a new authentication token for the inbox
    auth_token_inbox = build_auth_header(env_vars["MAILBOX_ID_TO"], env_vars["MAILBOX_TO_PASSWORD"], env_vars["SHARED_KEY"])

    # Print the authentication token for inbox
    print(f"\nCreating Authentication Token (Inbox): {auth_token_inbox}")

    # Perform the curl command to view the inbox and capture the response
    curl_command_inbox = [
        "curl", "-k",
        "--request", "GET",
        "--cert", env_vars["CERT_PATH"],
        "--key", env_vars["KEY_PATH"],
        "--header", ACCEPT_HEADER,
        "--header", f"authorization: {auth_token_inbox}",
        f"https://msg.intspineservices.nhs.uk/messageexchange/{env_vars['MAILBOX_ID_TO']}/inbox"
    ]

    # Echo the curl command
    print(f"\nExecuting curl command: {' '.join(curl_command_inbox)}")

    result_inbox = subprocess.run(curl_command_inbox, capture_output=True, text=True)
    print(f"\nViewing Mesh Mailbox {env_vars['MAILBOX_ID_TO']} (Inbox): {result_inbox.stdout.strip()}")

def empty_inbox(env_vars):
    # Generate authentication token for inbox
    auth_token_inbox = build_auth_header(env_vars["MAILBOX_ID_TO"], env_vars["MAILBOX_TO_PASSWORD"], env_vars["SHARED_KEY"])

    # Function to list messages in the inbox
    def list_messages():
        curl_command_inbox = [
            "curl", "-k",
            "--request", "GET",
            "--cert", env_vars["CERT_PATH"],
            "--key", env_vars["KEY_PATH"],
            "--header", ACCEPT_HEADER,
            "--header", f"authorization: {auth_token_inbox}",
            f"https://msg.intspineservices.nhs.uk/messageexchange/{env_vars['MAILBOX_ID_TO']}/inbox"
        ]

        # Echo the curl command
        print(f"\nExecuting curl command: {' '.join(curl_command_inbox)}")

        result_inbox = subprocess.run(curl_command_inbox, capture_output=True, text=True)
        print(f"\nViewing Mesh Mailbox {env_vars['MAILBOX_ID_TO']} (Inbox): {result_inbox.stdout.strip()}")
        return result_inbox.stdout.strip()

    # List messages and print each one
    messages = list_messages()
    # Assuming messages are in JSON format and contain a list of message IDs
    message_data = json.loads(messages)
    message_ids = message_data.get("messages", [])

    print("\nList of Messages in the Inbox:")
    for message_id in message_ids:
        print(f"Message ID: {message_id}")

    # Function to acknowledge a message
    def acknowledge_message(message_id):
        # Generate a new authentication token for acknowledging the message
        auth_token_ack = build_auth_header(env_vars["MAILBOX_ID_TO"], env_vars["MAILBOX_TO_PASSWORD"], env_vars["SHARED_KEY"])

        curl_command_ack = [
            "curl", "-k",
            "--request", "PUT",
            "--cert", env_vars["CERT_PATH"],
            "--key", env_vars["KEY_PATH"],
            "--header", "accept: */*",
            "--header", f"authorization: {auth_token_ack}",
            f"https://msg.intspineservices.nhs.uk/messageexchange/{env_vars['MAILBOX_ID_TO']}/inbox/{message_id}/status/acknowledged"
        ]

        # Echo the curl command
        print(f"\nExecuting curl command: {' '.join(curl_command_ack)}")

        result_ack = subprocess.run(curl_command_ack, capture_output=True, text=True)
        print(f"\nAcknowledging Message {message_id}: {result_ack.stdout.strip()}")

    # Acknowledge each message
    for message_id in message_ids:
        acknowledge_message(message_id)

def main():
    while True:
        print("\nChoose an option:")
        print("1. Send to Outbox")
        print("2. View Inbox")
        print("3. Empty Inbox")
        print("4. Quit")
        choice = input("Enter your choice (1, 2, 3 or 4): ")

        env_vars = load_env_vars()

        if choice == "1":
            send_to_outbox(env_vars)
        elif choice == "2":
            view_inbox(env_vars)
        elif choice == "3":
            empty_inbox(env_vars)
        elif choice == "4":
            print("Exiting the program.")
            break
        else:
            print("Invalid choice. Please enter 1, 2, 3 or 4.")

if __name__ == "__main__":
    main()
