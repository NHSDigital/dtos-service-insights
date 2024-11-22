#!/bin/bash

# Check if identifier and password are passed as arguments
if [ -z "$1" ]; then
    echo "Usage: $0 <UniqueIdentifier> [KeyPassword]"
    echo "Example: $0 X26OT023 mypassword"
    exit 1
fi

# Variables
IDENTIFIER=$1
PASSWORD=$2
KEYSTORE_NAME="MESH_$IDENTIFIER.keystore"
KEY_ALIAS="meshclient_$IDENTIFIER"
CSR_FILE="mesh_$IDENTIFIER.csr"
DN="CN=$IDENTIFIER.x26.api.mesh-client.nhs.uk"

# Ensure that password is provided
if [ -z "$PASSWORD" ]; then
    echo "Error: Key password not provided."
    echo "Usage: $0 <UniqueIdentifier> <KeyPassword>"
    exit 1
fi

# 1. Create a private key
echo "Creating private key for Mesh Mailbox $IDENTIFIER..."
keytool -genkey -alias $KEY_ALIAS -keyalg RSA -keysize 2048 -keystore $KEYSTORE_NAME -dname "$DN" \
        -storepass $PASSWORD -keypass $PASSWORD
echo "Private key created and stored in $KEYSTORE_NAME"

# 2. Create a CSR from the private key
echo "Creating CSR from private key..."
keytool -certreq -alias $KEY_ALIAS -keystore $KEYSTORE_NAME -file $CSR_FILE \
        -storepass $PASSWORD -keypass $PASSWORD
echo "CSR created as $CSR_FILE"

# 3. Prompt to send CSR to the NHS Service Desk
echo "Please send the CSR file ($CSR_FILE) to the NHS Service Desk to get a signed certificate."
echo "After receiving the certificate, save it as 'mesh_$IDENTIFIER.crt' and run the next script."
