import logging
import json
import os
from datetime import datetime
from uuid import uuid4
import azure.functions as func
from foundry_sdk import FoundryClient, UserTokenAuth

def main(req: func.HttpRequest) -> func.HttpResponse:
    logging.info('Foundry file upload function triggered.')

    try:
        payload = req.get_json()
        foundry_url = os.getenv("FOUNDRY_API_URL")
        api_token = os.getenv("FOUNDRY_API_TOKEN")
        dataset_rid = os.getenv("FOUNDRY_RESOURCE_ID")

        if not foundry_url or not api_token or not dataset_rid:
            raise EnvironmentError("Required environment variables are missing.")

        client = FoundryClient(
            auth=UserTokenAuth(api_token),
            hostname=foundry_url
        )

        # Log available methods in the SDK
        logging.info(f"Available methods in DatasetClient: {dir(client.datasets)}")
        logging.info(f"Available methods in client.datasets.Dataset: {dir(client.datasets.Dataset)}")
        logging.info(f"Available methods in client.datasets.Dataset.File: {dir(client.datasets.Dataset.File)}")

        file_name = generate_file_name()
        content = json.dumps(payload)

        logging.info(f"Uploading file '{file_name}' to dataset {dataset_rid}...")

        client.datasets.Dataset.File.upload(
            dataset_rid=dataset_rid,
            file_path=file_name,
            body=content.encode("utf-8")
        )

        logging.info(f"File '{file_name}' uploaded successfully.")

        return func.HttpResponse(
            f"File '{file_name}' uploaded to Foundry dataset successfully.",
            status_code=200
        )

    except Exception as e:
        logging.error(f"An error occurred: {e}", exc_info=True)
        return func.HttpResponse(
            f"An error occurred: {str(e)}",
            status_code=500
        )

def generate_file_name():
    current_time = datetime.now().strftime("%Y-%m-%d_%H-%M-%S")
    unique_suffix = uuid4().hex[:8]
    return f"{current_time}_{unique_suffix}.json"
