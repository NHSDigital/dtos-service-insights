import pytest
from unittest.mock import patch, MagicMock
import json
from http import HTTPStatus
import azure.functions as func
from FoundryIntegrationService.FoundryRelayFunction import main

@pytest.fixture
def mock_request():
    """Fixture to create a mock HTTP request."""
    payload = {"key1": "value1", "key2": "value2"}
    return func.HttpRequest(
        method="POST",
        url="/api/FoundryRelayFunction",
        body=json.dumps(payload).encode("utf-8"),
        headers={}
    )

@patch("FoundryIntegrationService.FoundryRelayFunction.foundryRelayFunction.FoundryClient")
@patch("FoundryIntegrationService.FoundryRelayFunction.foundryRelayFunction.os.getenv")
def test_happy_path__pass_valid_payload_to_function_over_http(mock_getenv, mock_foundry_client, mock_request):
    """Test the main function for a successful file upload."""
    # Mock environment variables
    mock_getenv.side_effect = lambda key: {
        "FOUNDRY_API_URL": "https://foundry.example.com",
        "FOUNDRY_API_TOKEN": "mock-token",
        "FOUNDRY_RESOURCE_ID": "mock-dataset-id"
    }.get(key)

    # Mock FoundryClient behavior
    mock_client_instance = MagicMock()
    mock_foundry_client.return_value = mock_client_instance

    # Call the function
    response = main(mock_request)

    # Assertions
    assert response.status_code == HTTPStatus.OK
    assert "uploaded to Foundry dataset resource ID" in response.get_body().decode("utf-8")
    mock_client_instance.datasets.Dataset.File.upload.assert_called_once()

@patch("FoundryIntegrationService.FoundryRelayFunction.foundryRelayFunction.FoundryClient")
@patch("FoundryIntegrationService.FoundryRelayFunction.foundryRelayFunction.os.getenv")
def test_main_missing_env_vars(mock_getenv, mock_foundry_client, mock_request):
    """Test the main function when environment variables are missing."""
    # Mock environment variables to return None
    mock_getenv.side_effect = lambda key: None

    # Call the function
    response = main(mock_request)

    # Assertions
    assert response.status_code == HTTPStatus.BAD_REQUEST
    assert "Required environment variables are missing" in response.get_body().decode("utf-8")

@patch("FoundryIntegrationService.FoundryRelayFunction.foundryRelayFunction.FoundryClient")
@patch("FoundryIntegrationService.FoundryRelayFunction.foundryRelayFunction.os.getenv")
def test_main_invalid_payload(mock_getenv, mock_foundry_client):
    """Test the main function with an invalid payload."""
    # Mock environment variables
    mock_getenv.side_effect = lambda key: {
        "FOUNDRY_API_URL": "https://foundry.example.com",
        "FOUNDRY_API_TOKEN": "mock-token",
        "FOUNDRY_RESOURCE_ID": "mock-dataset-id"
    }.get(key)

    # Create a mock request with an invalid payload
    invalid_payload = "invalid_payload"
    mock_request = func.HttpRequest(
        method="POST",
        url="/api/FoundryRelayFunction",
        body=invalid_payload.encode("utf-8"),
        headers={}
    )

    # Call the function
    response = main(mock_request)

    # Assertions
    assert response.status_code == HTTPStatus.BAD_REQUEST
    assert "Invalid JSON payload" in response.get_body().decode("utf-8")

@patch("FoundryIntegrationService.FoundryRelayFunction.foundryRelayFunction.FoundryClient")
@patch("FoundryIntegrationService.FoundryRelayFunction.foundryRelayFunction.os.getenv")
def test_main_upload_failure(mock_getenv, mock_foundry_client, mock_request):
    """Test the main function when the file upload fails."""
    # Mock environment variables
    mock_getenv.side_effect = lambda key: {
        "FOUNDRY_API_URL": "https://foundry.example.com",
        "FOUNDRY_API_TOKEN": "mock-token",
        "FOUNDRY_RESOURCE_ID": "mock-dataset-id"
    }.get(key)

    # Mock FoundryClient behavior to raise an exception
    mock_client_instance = MagicMock()
    mock_client_instance.datasets.Dataset.File.upload.side_effect = Exception("Upload failed")
    mock_foundry_client.return_value = mock_client_instance

    # Call the function
    response = main(mock_request)

    # Assertions
    assert response.status_code == HTTPStatus.INTERNAL_SERVER_ERROR
    assert "An internal server error occurred" in response.get_body().decode("utf-8")
    mock_client_instance.datasets.Dataset.File.upload.assert_called_once()
