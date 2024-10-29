# Script to install Python if not already installed, add it to PATH, install pre-commit, and set up gitleaks in pre-commit hooks on Windows

# Step 1: Check if Python is installed
$pythonVersion = python --version 2>$null

if ($pythonVersion) {
    Write-Host "Python is already installed. Version: $pythonVersion"
} else {
    # Python is not installed, proceed to install it
    Write-Host "Python not found. Installing Python..."

    # Define Python download URL
    $pythonInstallerUrl = "https://www.python.org/ftp/python/3.13.0/python-3.13.0-amd64.exe"  # Replace with the latest version if needed
    $installerPath = "$env:TEMP\python-installer.exe"

    # Download Python installer
    Invoke-WebRequest -Uri $pythonInstallerUrl -OutFile $installerPath

    # Install Python silently (it will install to the default directory and add to PATH)
    Start-Process -FilePath $installerPath -ArgumentList "/quiet PrependPath=1" -Wait

    # Refresh the environment to ensure Python is in the PATH
    $env:Path += ";$env:LOCALAPPDATA\Programs\Python\Python313;$env:LOCALAPPDATA\Programs\Python\Python313\Scripts"
}

# Step 2: Verify Python and pip installation
Write-Host "Python version:"
python --version
Write-Host "Pip version:"
pip --version

# Step 3: Upgrade pip to the latest version
Write-Host "Upgrading pip..."
python -m pip install --upgrade pip

# Step 4: Install the pre-commit package manager using pip
Write-Host "Installing pre-commit..."
pip install pre-commit

# Step 5: Create a .pre-commit-config.yaml file with Gitleaks hook
Write-Host "Creating .pre-commit-config.yaml file..."
@"
repos:
-   repo: https://github.com/gitleaks/gitleaks
    rev: v8.12.0  # Update to the latest stable version of gitleaks
    hooks:
    -   id: gitleaks
"@ | Out-File -FilePath ".pre-commit-config.yaml" -Encoding utf8

# Step 6: Install pre-commit hooks in the repository
Write-Host "Installing pre-commit hooks..."
pre-commit install

# Step 7: Run pre-commit to verify gitleaks setup
Write-Host "Running pre-commit to verify gitleaks setup..."
pre-commit run --all-files

Write-Host "Pre-commit hooks set up with gitleaks successfully!"
