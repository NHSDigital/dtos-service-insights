
# Mesh Mailbox Certificate Generation

## Overview

This repository helps streamline the process of creating SSL certificates for Mesh Mailboxes. SSL certificates are essential for securing communication between clients and servers, ensuring data privacy and integrity.

The SSL certificates and Mesh Mailbox details will be required by the Cloud Engineering Team when they create the Azure Environments to host the Azure functions.

The scripts generate certificate files named according to the unique identifier of the Mesh Mailbox, making it easy to manage multiple certificates.

In summary, this repository simplifies the process of obtaining and setting up SSL certificates for Mesh Mailboxes by automating the creation of CSRs and completing the certificate setup after receiving the signed certificate from the NHS Service Desk. This ensures secure communication between clients and servers, protecting sensitive data and maintaining data integrity.

This project contains two scripts, `mesh_step_1_generate_cert_signing_request.sh` and `mesh_step_2_generate_actual_certificate.sh`, to create certificates for each unique Mesh Mailbox. Each certificate and associated files are named according to the specific identifier of the Mesh Mailbox (e.g., `X26OT023`), making it easy to manage multiple certificates.

## Prerequisites

To use these scripts, you’ll need `keytool` and `openssl`.

### Installing Prerequisites on macOS

#### 1. **Install Java Development Kit (JDK) for `keytool`**

You can install the JDK via [Homebrew](https://brew.sh) (if Homebrew is not installed, follow the instructions on [brew.sh](https://brew.sh)):

```bash
brew install openjdk
```

#### 2. **Install `openssl`**

You can also install `openssl` using Homebrew:

```bash
brew install openssl
```

## Instructions stage 1

### `mesh_step_1_generate_cert_signing_request.sh`

This script creates the private key and generates a CSR (Certificate Signing Request) for the specified Mesh Mailbox. The generated CSR file can then be sent to the NHS Service Desk to obtain a signed certificate.

#### How to Run

To run this script, use the following command, replacing `<UniqueIdentifier>` with your specific Mesh Mailbox identifier:

```bash
sh ./mesh_step_1_generate_cert_signing_request.sh <UniqueIdentifier>
```

**Example**:

```bash
sh ./mesh_step_1_generate_cert_signing_request.sh X26OT023
```

#### Keystore Password

You will be prompted to enter the keystore password. This password will be used to protect the keystore.
Remember this keystore password, as it will be needed later on.

#### Sending CSR to NHS Service Desk

Send the CSR (`mesh_X26OT023.csr`) to the NHS Service Desk via email.

## Instructions stage 2 (after receiving signed certificate from NHS Service Desk)

### `mesh_step_2_generate_actual_certificate.sh`

Once you receive the signed certificate from the NHS Service Desk, save it as `mesh_<UniqueIdentifier>.crt` and run the following script to complete the setup. This script converts the keystore to PKCS12 format, extracts the private key in PEM format, and creates a PFX file with the private key and certificate.

#### How to Run

To run this script, use the following command, replacing `<UniqueIdentifier>` with your specific Mesh Mailbox identifier:

```bash
sh ./mesh_step_2_generate_actual_certificate.sh <UniqueIdentifier>
```

**Example**:

```bash
sh ./mesh_step_2_generate_actual_certificate.sh X26OT023
```

Each generated file is named according to the unique identifier provided for the Mesh Mailbox.

## Summary Workflow

Here’s a full example of the process for a Mesh Mailbox with identifier `X26OT023`:

1. **Generate the CSR** by running the first script:

   ```bash
   sh ./mesh_step_1_generate_cert_signing_request.sh X26OT023
   ```

2. **Send the CSR** (`mesh_X26OT023.csr`) to the NHS Service Desk.

3. **Receive the signed certificate** and save it as `mesh_X26OT023.crt`.

4. **Complete the certificate setup** by running the second script:

   ```bash
   sh ./mesh_step_2_generate_actual_certificate.sh X26OT023
   ```

Following these steps will produce the necessary certificate files for the Mesh Mailbox.
