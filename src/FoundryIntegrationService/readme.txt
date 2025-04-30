# Foundry Relay Function - Setup and Usage Guide

## 1. Install Foundry SDK and Dependencies
Ensure you have Python 3.10 installed, then install the required dependencies:
```bash
pip install -r requirements.txt
```

## 2. Update Local Environment Settings
Configure your local environment by updating the `local.settings.json` file:
- Add your **Foundry API Token**, **Dataset Resource ID**, and **Foundry API URL**.
- Use the `local.settings.json.template` as a reference.

Example:
```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "python",
    "FOUNDRY_API_URL": "https://your-foundry-url",
    "FOUNDRY_API_TOKEN": "your-foundry-token",
    "FOUNDRY_RESOURCE_ID": "your-dataset-resource-id"
  }
}
```

## 3. Run the Azure Function Locally
Navigate to the `FoundryIntegrationService` directory and start the Azure Function:
```bash
cd src/FoundryIntegrationService
func start
```

## 4. Test the Function
Use `curl` to send a POST request to the function with a sample payload:
```bash
curl -X POST http://localhost:7071/api/FoundryRelayFunction \
-H "Content-Type: application/json" \
--data @payload.json
```

### Notes:
- Ensure the `payload.json` file contains the data you want to send to the function.
- Example `payload.json`:
  ```json
  {
    "key1": "value1",
    "key2": "value2"
  }
  ```

## 5. Run Unit Tests
To ensure the function behaves as expected, run the unit tests using `pytest`:
```bash
pytest tests/FoundryIntegrationService/test_foundry_relay_function.py
```

## Troubleshooting
- If the function does not start, ensure all dependencies are installed and environment variables are correctly configured.
- Check the logs for errors when running the function:
  ```bash
  func start
  ```

## Additional Information
- For Foundry SDK usage, refer to the [Foundry SDK Documentation](https://www.palantir.com/docs/foundry/api/v1/datasets-resources/files/upload-file).
