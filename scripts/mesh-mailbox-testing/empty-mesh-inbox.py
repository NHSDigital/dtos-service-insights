import json
import hmac
import uuid
import datetime
import subprocess
from hashlib import sha256
import os
import sys

# Ensure the python-dotenv package is installed
try:
    from dotenv import load_dotenv
except ImportError:
    subprocess.check_call([sys.executable, "-m", "pip", "install", "python-dotenv"])
    from dotenv import load_dotenv

# Load environment variables from .env file
load_dotenv()

AUTH_SCHEMA_NAME = "NHSMESH " # keep the trailing space

def build_auth_header(mailbox_id: str, password: str, shared_key: str, nonce: str = None, nonce_count: int = 0):
    """ Generate MESH Authorization header for mailboxid. """
    if not nonce:
        nonce = str(uuid.uuid4())
    timestamp = datetime.datetime.now(datetime.timezone.utc).strftime("%Y%m%d%H%M")
    hmac_msg = mailbox_id + ":" + nonce + ":" + str(nonce_count) + ":" + password + ":" + timestamp
    hash_code = hmac.HMAC(shared_key.encode(), hmac_msg.encode(), sha256).hexdigest()
    return (
            AUTH_SCHEMA_NAME
            + mailbox_id + ":"
            + nonce + ":"
            + str(nonce_count) + ":"
            + timestamp + ":"
            + hash_code
    )

# Load values from environment variables
MAILBOX_ID_FROM = os.getenv("MAILBOX_ID_FROM")
MAILBOX_FROM_PASSWORD = os.getenv("MAILBOX_FROM_PASSWORD")
MAILBOX_ID_TO = os.getenv("MAILBOX_ID_TO")
MAILBOX_TO_PASSWORD = os.getenv("MAILBOX_TO_PASSWORD")
SHARED_KEY = os.getenv("SHARED_KEY")
FILE_PATH = os.getenv("FILE_PATH")
CERT_PATH = os.getenv("CERT_PATH")
KEY_PATH = os.getenv("KEY_PATH")
WORKFLOW_ID = os.getenv("WORKFLOW_ID")

# Check if any required environment variables are not set
required_vars = {
    "MAILBOX_ID_FROM": MAILBOX_ID_FROM,
    "MAILBOX_FROM_PASSWORD": MAILBOX_FROM_PASSWORD,
    "MAILBOX_ID_TO": MAILBOX_ID_TO,
    "MAILBOX_TO_PASSWORD": MAILBOX_TO_PASSWORD,
    "SHARED_KEY": SHARED_KEY,
    "FILE_PATH": FILE_PATH,
    "CERT_PATH": CERT_PATH,
    "KEY_PATH": KEY_PATH,
    "WORKFLOW_ID": WORKFLOW_ID
}

missing_vars = [var for var, value in required_vars.items() if value is None]

if missing_vars:
    print(f"\nError: The following required environment variables are not set: {', '.join(missing_vars)}")
    sys.exit(1)

# Print the loaded values
print(f"\nMAILBOX_ID_FROM: {MAILBOX_ID_FROM}")
print(f"\nMAILBOX_FROM_PASSWORD: {MAILBOX_FROM_PASSWORD}")
print(f"\nMAILBOX_ID_TO: {MAILBOX_ID_TO}")
print(f"\nMAILBOX_TO_PASSWORD: {MAILBOX_TO_PASSWORD}")
print(f"\nSHARED_KEY: {SHARED_KEY}")
print(f"\nFILE_PATH: {FILE_PATH}")
print(f"\nCERT_PATH: {CERT_PATH}")
print(f"\nKEY_PATH: {KEY_PATH}")
print(f"\nWORKFLOW_ID: {WORKFLOW_ID}")

# Generate authentication token for inbox
auth_token_inbox = build_auth_header(MAILBOX_ID_TO, MAILBOX_TO_PASSWORD, SHARED_KEY)

# Function to list messages in the inbox
def list_messages():
    curl_command_inbox = [
        "curl", "-k",
        "--request", "GET",
        "--cert", CERT_PATH,
        "--key", KEY_PATH,
        "--header", "accept: application/vnd.mesh.v2+json",
        "--header", f"authorization: {auth_token_inbox}",
        f"https://msg.intspineservices.nhs.uk/messageexchange/{MAILBOX_ID_TO}/inbox"
    ]

    result_inbox = subprocess.run(curl_command_inbox, capture_output=True, text=True)
    print(f"\nViewing Mesh Mailbox {MAILBOX_ID_TO} (Inbox): {result_inbox.stdout.strip()}")
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
    auth_token_ack = build_auth_header(MAILBOX_ID_TO, MAILBOX_TO_PASSWORD, SHARED_KEY)

    curl_command_ack = [
        "curl", "-k",
        "--request", "PUT",
        "--cert", CERT_PATH,
        "--key", KEY_PATH,
        "--header", "accept: */*",
        "--header", f"authorization: {auth_token_ack}",
        f"https://msg.intspineservices.nhs.uk/messageexchange/{MAILBOX_ID_TO}/inbox/{message_id}/status/acknowledged"
    ]

    result_ack = subprocess.run(curl_command_ack, capture_output=True, text=True)
    print(f"\nAcknowledging Message {message_id}: {result_ack.stdout.strip()}")

# Acknowledge each message
for message_id in message_ids:
    acknowledge_message(message_id)
