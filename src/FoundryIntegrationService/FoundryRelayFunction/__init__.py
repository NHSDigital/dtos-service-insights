import logging
import json
import os
from datetime import datetime
from http import HTTPStatus
from uuid import uuid4
import azure.functions as func
from foundry_sdk import FoundryClient, UserTokenAuth

logger = logging.getLogger(__name__)

def main(req: func.HttpRequest) -> func.HttpResponse:
    logger.info('Foundry file upload function triggered.')

    try:
        payload = req.get_json()
        foundry_url = os.getenv("FOUNDRY_API_URL")
        api_token = os.getenv("FOUNDRY_API_TOKEN")
        dataset_rid = os.getenv("FOUNDRY_RESOURCE_ID")

        if not foundry_url or not api_token or not dataset_rid:
            raise EnvironmentError("Required environment variables are missing.")

        if not isinstance(payload, dict):
                return func.HttpResponse(
                    "Invalid payload format. Expected a JSON object.",
                    status_code=HTTPStatus.BAD_REQUEST
                )

        client = FoundryClient(
            auth=UserTokenAuth(api_token),
            hostname=foundry_url
        )

        file_name = generate_file_name()
        content = json.dumps(payload)

        logger.info(f"Uploading file '{file_name}' to Foundry dataset resource ID: {dataset_rid}...")

        client.datasets.Dataset.File.upload(
            dataset_rid=dataset_rid,
            file_path=file_name,
            body=content.encode("utf-8")
        )

        logger.info(f"File '{file_name}' uploaded successfully.")

        return func.HttpResponse(
            f"File '{file_name}' uploaded to Foundry dataset resource ID '{dataset_rid}' successfully.",
            status_code=HTTPStatus.OK
        )

    except EnvironmentError as env_err:
        logger.error(f"Environment configuration error: {env_err}")
        return func.HttpResponse(
            str(env_err),
            status_code=HTTPStatus.BAD_REQUEST
        )

    except Exception as e:
        logger.error(f"An error occurred: {e}", exc_info=True)
        return func.HttpResponse(
            f"An internal server error occurred: {str(e)}",
            status_code=HTTPStatus.INTERNAL_SERVER_ERROR
        )

def generate_file_name() -> str:
    current_time = datetime.now().strftime("%Y-%m-%d_%H-%M-%S")
    unique_suffix = uuid4().hex[:8]
    return f"{current_time}_{unique_suffix}.json"
