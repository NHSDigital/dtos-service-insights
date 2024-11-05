#!/bin/bash

# Check if identifier is passed as an argument
if [ -z "$1" ]; then
    echo "Usage: $0 <UniqueIdentifier>"
    echo "Example: $0 X26OT023"
    exit 1
fi

# Variables
IDENTIFIER=$1
KEYSTORE_NAME="MESH_$IDENTIFIER.keystore"
P12_FILE="mesh_$IDENTIFIER.p12"
PEM_FILE="foo_$IDENTIFIER.pem"
PFX_FILE="meshpfx_$IDENTIFIER.pfx"
CRT_FILE="mesh_$IDENTIFIER.crt"
KEY_ALIAS="meshclient_$IDENTIFIER"

# Ensure that mesh.crt is available
if [ ! -f "$CRT_FILE" ]; then
    echo "Error: $CRT_FILE not found. Please make sure to save the signed certificate as $CRT_FILE."
    exit 1
fi

# 5. Convert the keystore to PKCS12 format
echo "Converting keystore to PKCS12 format..."
keytool -importkeystore -srckeystore $KEYSTORE_NAME -srcstoretype JKS -destkeystore $P12_FILE -deststoretype PKCS12
echo "PKCS12 keystore created as $P12_FILE"

# 6. Extract the private key
echo "Extracting private key to PEM format..."
openssl pkcs12 -in $P12_FILE -out $PEM_FILE -nodes
echo "Private key extracted as $PEM_FILE"

# 7. Create the PFX file from the private key and certificate
echo "Creating PFX file..."
openssl pkcs12 -inkey $PEM_FILE -in $CRT_FILE -export -out $PFX_FILE
echo "PFX file created as $PFX_FILE"

echo "Certificate setup for $IDENTIFIER is complete."
