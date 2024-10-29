#!/bin/bash

# Script to install Python if not already installed, add it to PATH, install pre-commit, and set up gitleaks in pre-commit hooks on macOS

# Step 1: Check if Python is installed
if command -v python3 &> /dev/null
then
    echo "Python is already installed. Version: $(python3 --version)"
else
    # Python is not installed, proceed to install via Homebrew
    echo "Python not found. Installing Python using Homebrew..."

    # Check if Homebrew is installed, and install it if not
    if ! command -v brew &> /dev/null
    then
        echo "Homebrew not found. Installing Homebrew..."
        /bin/bash -c "$(curl -fsSL https://raw.githubusercontent.com/Homebrew/install/HEAD/install.sh)"
    fi

    # Install Python using Homebrew
    brew install python
fi

# Step 2: Ensure Python and pip are accessible from PATH
export PATH="/usr/local/bin:/usr/local/sbin:$PATH"

# Verify Python and pip installation
echo "Python version:"
python3 --version
echo "Pip version:"
pip3 --version

# Step 3: Upgrade pip to the latest version
echo "Upgrading pip..."
python3 -m pip install --upgrade pip

# Step 4: Install the pre-commit package manager using pip
echo "Installing pre-commit..."
pip3 install pre-commit

# Step 5: Create a .pre-commit-config.yaml file with Gitleaks hook
echo "Creating .pre-commit-config.yaml file..."
cat <<EOT > .pre-commit-config.yaml
repos:
-   repo: https://github.com/gitleaks/gitleaks
    rev: v8.12.0  # Update to the latest stable version of gitleaks
    hooks:
    -   id: gitleaks
EOT

# Step 6: Install pre-commit hooks in the repository
echo "Installing pre-commit hooks..."
pre-commit install

# Step 7: Run pre-commit to verify gitleaks setup
echo "Running pre-commit to verify gitleaks setup..."
pre-commit run --all-files

echo "Pre-commit hooks set up with gitleaks successfully!"
