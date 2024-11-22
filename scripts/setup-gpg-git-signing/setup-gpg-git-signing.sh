#!/bin/bash

echo "Welcome to the Git and GPG setup script."
echo "This script will install necessary tools, create or use an existing ECDSA GPG key, configure Git, and set up commit signing."
echo

# Step 1: Install GPG and Pinentry-mac
echo "Step 1: Installing GPG and Pinentry-mac using Homebrew..."
if ! command -v brew &> /dev/null; then
  echo "Error: Homebrew is not installed. Please install Homebrew first: https://brew.sh/"
  exit 1
fi

brew install gpg pinentry-mac
if [ $? -ne 0 ]; then
  echo "Error: Failed to install GPG or Pinentry-mac."
  exit 1
fi

# Step 2: Configure Pinentry-mac for GPG
echo "Configuring pinentry-mac for GPG..."
echo "pinentry-program $(which pinentry-mac)" > ~/.gnupg/gpg-agent.conf
killall gpg-agent
echo "Pinentry-mac configuration completed."
echo

# Step 3: Retrieve the GitHub username from the global Git configuration
echo "Step 3: Retrieving your GitHub username from your Git configuration..."
USER_NAME=$(git config --global user.name)

if [ -z "$USER_NAME" ]; then
  echo "Error: No GitHub username found in your Git configuration."
  echo "Please set your GitHub username first using: git config --global user.name \"Your Name\""
  exit 1
fi

echo "GitHub username detected: $USER_NAME"

# Step 4: Retrieve the email address from the global Git configuration
echo "Step 4: Retrieving your GitHub email from your Git configuration..."
USER_EMAIL=$(git config --global user.email)

if [ -z "$USER_EMAIL" ]; then
  echo "Error: No email address found in your Git configuration."
  echo "Please set your email address first using: git config --global user.email \"your.email@example.com\""
  exit 1
fi

echo "GitHub email detected: $USER_EMAIL"

# Step 5: Check for an existing GPG key
echo
echo "Step 5: Checking for an existing GPG key..."
GPG_KEY_ID=$(gpg --list-secret-keys --keyid-format=long "$USER_EMAIL" | grep 'sec' | awk '{print $2}' | cut -d'/' -f2)

if [ -n "$GPG_KEY_ID" ]; then
  echo "An existing GPG key was found for this email: $GPG_KEY_ID"
else
  # Step 6: Generate a new GPG key if none exists
  echo "No existing GPG key found. Generating a new one..."
  gpg --batch --quick-gen-key "$USER_NAME <$USER_EMAIL>" future-default default 0
  if [ $? -ne 0 ]; then
    echo "Error: Failed to generate the GPG key. Please check your GPG setup."
    exit 1
  fi

  # Fetch the newly created GPG key ID
  GPG_KEY_ID=$(gpg --list-secret-keys --keyid-format=long "$USER_EMAIL" | grep 'sec' | awk '{print $2}' | cut -d'/' -f2)
  echo "Generated GPG key ID: $GPG_KEY_ID"
fi

# Step 6: Export the public key for GitHub
echo
echo "Step 6: Exporting the public key..."
gpg --armor --export "$GPG_KEY_ID" > gpg-public-key.asc
if [ $? -ne 0 ]; then
  echo "Error: Failed to export the public key."
  exit 1
fi

# Echo the public key to the terminal
echo "The public key has been saved to 'gpg-public-key.asc'. You can upload it to GitHub."
echo "Here is your public key:"
cat gpg-public-key.asc

# Step 7: Configure Git to use the GPG key
echo
echo "Step 7: Configuring Git to use the GPG key..."
echo "Unsetting gpg.format to avoid conflicts..."
git config --global --unset gpg.format
git config --global user.signingkey "$GPG_KEY_ID"
git config --global commit.gpgsign true

# Step 8: Set the correct GPG program for Git
echo "Ensuring Git uses the correct GPG program..."
GPG_PROGRAM=$(which gpg)
git config --global gpg.program "$GPG_PROGRAM"

# Step 9: Verify Git configuration
echo
echo "Git configurations set:"
echo "Signing Key: $(git config --global --get user.signingkey)"
echo "Commit Signing Enabled: $(git config --global --get commit.gpgsign)"
echo "GPG Program: $(git config --global --get gpg.program)"

# Step 10: Provide instructions for uploading the public key to GitHub
echo
echo "Step 8: Next steps:"
echo "1. Add your GPG key to GitHub:"
echo "   Open 'gpg-public-key.asc' and copy its contents."
echo "   Go to https://github.com/settings/keys and paste the key into the 'GPG keys' section."
echo
echo "2. Test your setup:"
echo "   Create a new commit and verify it is signed by running:"
echo "   git log --show-signature"
echo

echo "Done! Your Git and GPG setup is complete."
