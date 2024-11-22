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
P12_FILE="mesh_$IDENTIFIER.p12"
PEM_FILE="foo_$IDENTIFIER.pem"
PFX_FILE="meshpfx_$IDENTIFIER.pfx"
CRT_FILE="mesh_$IDENTIFIER.crt"
KEY_ALIAS="meshclient_$IDENTIFIER"
CSR_FILE="mesh_$IDENTIFIER.csr"
LOG_FILE="verification_$IDENTIFIER.log"

# Ensure that password is provided
if [ -z "$PASSWORD" ]; then
    echo "Error: Key password not provided."
    echo "Usage: $0 <UniqueIdentifier> <KeyPassword>"
    exit 1
fi

# Ensure that mesh.crt and mesh.csr are available
if [ ! -f "$CRT_FILE" ]; then
    echo "Error: $CRT_FILE not found. Please make sure to save the signed certificate as $CRT_FILE."
    exit 1
fi

if [ ! -f "$CSR_FILE" ]; then
    echo "Error: $CSR_FILE not found. Please make sure to save the certificate signing request as $CSR_FILE."
    exit 1
fi

# Validate that the CRT matches the CSR
echo "Validating that $CRT_FILE matches $CSR_FILE..."
CRT_MODULUS=$(openssl x509 -noout -modulus -in $CRT_FILE | openssl md5)
CSR_MODULUS=$(openssl req -noout -modulus -in $CSR_FILE | openssl md5)

if [ "$CRT_MODULUS" != "$CSR_MODULUS" ]; then
    echo "Error: The certificate ($CRT_FILE) does not match the signing request ($CSR_FILE)." | tee -a $LOG_FILE
    echo "CSR Modulus: $CSR_MODULUS" >> $LOG_FILE
    echo "CRT Modulus: $CRT_MODULUS" >> $LOG_FILE
    exit 1
fi

echo "Validation successful: $CRT_FILE matches $CSR_FILE." | tee -a $LOG_FILE
echo "CSR Modulus: $CSR_MODULUS" >> $LOG_FILE
echo "CRT Modulus: $CRT_MODULUS" >> $LOG_FILE

# Extract DN from CSR and CRT
CSR_DN=$(openssl req -noout -subject -in "$CSR_FILE" | sed 's/subject= //')
CRT_DN=$(openssl x509 -noout -subject -in "$CRT_FILE" | sed 's/subject= //')

# Log and echo DN
echo "CSR DN: $CSR_DN" | tee -a $LOG_FILE
echo "CRT DN: $CRT_DN" | tee -a $LOG_FILE

# 5. Convert the keystore to PKCS12 format
echo "Converting keystore to PKCS12 format..."
keytool -importkeystore -srckeystore $KEYSTORE_NAME -srcstoretype JKS \
        -srcstorepass $PASSWORD \
        -destkeystore $P12_FILE -deststoretype PKCS12 \
        -deststorepass $PASSWORD
echo "PKCS12 keystore created as $P12_FILE"

# 6. Extract the private key
echo "Extracting private key to PEM format..."
openssl pkcs12 -in $P12_FILE -passin pass:$PASSWORD -out $PEM_FILE -nodes
echo "Private key extracted as $PEM_FILE"

# 7. Create the PFX file from the private key and certificate
echo "Creating PFX file..."
openssl pkcs12 -inkey $PEM_FILE -in $CRT_FILE -passin pass:$PASSWORD -export -out $PFX_FILE -passout pass:$PASSWORD
echo "PFX file created as $PFX_FILE"

echo "Certificate setup for $IDENTIFIER is complete."
