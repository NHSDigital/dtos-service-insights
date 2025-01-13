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

# Generate authentication token for outbox
auth_token_outbox = build_auth_header(MAILBOX_ID_FROM, MAILBOX_FROM_PASSWORD, SHARED_KEY)

# Print the authentication token for outbox
print(f"\nGenerating Authentication Token (Outbox): {auth_token_outbox}")

# Extract the filename from the FILE_PATH
file_name = os.path.basename(FILE_PATH)

# Perform the first curl command to the outbox and capture the response
curl_command_outbox = [
    "curl", "-k",
    "--request", "POST",
    "--cert", CERT_PATH,
    "--key", KEY_PATH,
    "--header", "accept: application/vnd.mesh.v2+json",
    "--header", f"authorization: {auth_token_outbox}",
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
print(f"\nSending to Mesh Mailbox {MAILBOX_ID_FROM} (Outbox): {result_outbox.stdout.strip()}")

# Now lets view the inbox of the receiving mailbox
print(f"\nNext lets view the inbox of the receiving mailbox: {MAILBOX_ID_TO}")

# Generate a new authentication token for the inbox
auth_token_inbox = build_auth_header(MAILBOX_ID_TO, MAILBOX_TO_PASSWORD, SHARED_KEY)

# Print the authentication token for inbox
print(f"\nCreating Authentication Token (Inbox): {auth_token_inbox}")

# Perform the second curl command to the inbox and capture the response
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
