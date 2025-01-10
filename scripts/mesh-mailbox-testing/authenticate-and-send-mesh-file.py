import hmac
import uuid
import datetime
import subprocess
from hashlib import sha256
import os
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
MAILBOX_ID = os.getenv("MAILBOX_ID")
MAILBOX_PASSWORD = os.getenv("MAILBOX_PASSWORD")
SHARED_KEY = os.getenv("SHARED_KEY")
FILE_PATH = os.getenv("FILE_PATH")

# Print the loaded values
print(f"MAILBOX_ID: {MAILBOX_ID}")
print(f"MAILBOX_PASSWORD: {MAILBOX_PASSWORD}")
print(f"SHARED_KEY: {SHARED_KEY}")
print(f"FILE_PATH: {FILE_PATH}")

auth_token = build_auth_header(MAILBOX_ID, MAILBOX_PASSWORD, SHARED_KEY)

# Print the authentication token
print(f"Authentication Token: {auth_token}")

# Extract the filename from the FILE_PATH
file_name = os.path.basename(FILE_PATH)

# Perform the first curl command and capture the response
curl_command_outbox = [
    "curl", "-k",
    "--request", "POST",
    "--cert", "meshcertnew.crt",
    "--key", "meshnewkey.pem",
    "--header", "accept: application/vnd.mesh.v2+json",
    "--header", f"authorization: {auth_token}",
    "--header", "content-type: application/octet-stream",
    "--header", f"mex-from: {MAILBOX_ID}",
    "--header", f"mex-to: {MAILBOX_ID}",
    "--header", "mex-workflowid: BSS DtoS Extract",
    "--header", f"mex-filename: {file_name}",
    "--header", "mex-localid: testing123",
    "--data", f"@{FILE_PATH}",
    f"https://msg.intspineservices.nhs.uk/messageexchange/{MAILBOX_ID}/outbox"
]

result = subprocess.run(curl_command_outbox, capture_output=True, text=True)
print(f"Mesh Mailbox Response: {result.stdout.strip()}")
