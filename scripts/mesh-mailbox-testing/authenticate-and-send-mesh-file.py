#!/usr/bin/env python3
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

def build_auth_header(mailbox_id_from: str, password: str, shared_key: str, nonce: str = None, nonce_count: int = 0):
    """ Generate MESH Authorization header for mailboxid. """
    if not nonce:
        nonce = str(uuid.uuid4())
    timestamp = datetime.datetime.now(datetime.timezone.utc).strftime("%Y%m%d%H%M")
    hmac_msg = mailbox_id_from + ":" + nonce + ":" + str(nonce_count) + ":" + password + ":" + timestamp
    hash_code = hmac.HMAC(shared_key.encode(), hmac_msg.encode(), sha256).hexdigest()
    return (
            AUTH_SCHEMA_NAME
            + mailbox_id_from + ":"
            + nonce + ":"
            + str(nonce_count) + ":"
            + timestamp + ":"
            + hash_code
    )

# Load values from environment variables
MAILBOX_ID_FROM = os.getenv("MAILBOX_ID_FROM")
MAILBOX_FROM_PASSWORD = os.getenv("MAILBOX_FROM_PASSWORD")
SHARED_KEY = os.getenv("SHARED_KEY")
FILE_PATH = os.getenv("FILE_PATH")
MAILBOX_ID_TO = os.getenv("MAILBOX_ID_TO")
CERT_PATH = os.getenv("CERT_PATH")
KEY_PATH = os.getenv("KEY_PATH")
WORKFLOW_ID = os.getenv("WORKFLOW_ID")

# Print the loaded values
print(f"MAILBOX_ID_FROM: {MAILBOX_ID_FROM}")
print(f"MAILBOX_FROM_PASSWORD: {MAILBOX_FROM_PASSWORD}")
print(f"SHARED_KEY: {SHARED_KEY}")
print(f"FILE_PATH: {FILE_PATH}")
print(f"MAILBOX_ID_TO: {MAILBOX_ID_TO}")
print(f"CERT_PATH: {CERT_PATH}")
print(f"KEY_PATH: {KEY_PATH}")
print(f"WORKFLOW_ID: {WORKFLOW_ID}")

auth_token = build_auth_header(MAILBOX_ID_FROM, MAILBOX_FROM_PASSWORD, SHARED_KEY)

# Print the authentication token
print(f"Authentication Token: {auth_token}")

# Extract the filename from the FILE_PATH
file_name = os.path.basename(FILE_PATH)

# Perform the first curl command and capture the response
curl_command_outbox = [
    "curl", "-k",
    "--request", "POST",
    "--cert", CERT_PATH,
    "--key", KEY_PATH,
    "--header", "accept: application/vnd.mesh.v2+json",
    "--header", f"authorization: {auth_token}",
    "--header", "content-type: application/octet-stream",
    "--header", f"mex-from: {MAILBOX_ID_FROM}",
    "--header", f"mex-to: {MAILBOX_ID_TO}",
    "--header", f"mex-workflowid: {WORKFLOW_ID}",
    "--header", f"mex-filename: {file_name}",
    "--header", "mex-localid: testing123",
    "--data", f"@{FILE_PATH}",
    f"https://msg.intspineservices.nhs.uk/messageexchange/{MAILBOX_ID_FROM}/outbox"
]

result_outbox = subprocess.run(curl_command_outbox, capture_output=True, text=True)
print(f"Mesh Mailbox Response (Outbox): {result_outbox.stdout.strip()}")
