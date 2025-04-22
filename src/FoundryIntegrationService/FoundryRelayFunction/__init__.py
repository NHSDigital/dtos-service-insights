import logging
import json
import os
import requests
import azure.functions as func

def main(req: func.HttpRequest) -> func.HttpResponse:
    logging.info('FoundryRelayFunction triggered by HTTP POST request.')

    try:
        # Parse the JSON payload from the request
        payload = req.get_json()
        logging.info(f"Received Payload: {json.dumps(payload, indent=2)}")

        # Write to Foundry bucket via Foundry API
        foundry_url = os.getenv("FOUNDRY_API_URL")
        api_token = os.getenv("FOUNDRY_API_TOKEN")
        foundry_resource_id = os.getenv("FOUNDRY_RESOURCE_ID")

        # Log the Foundry API URL, token, and resource ID
        logging.info(f"FOUNDRY_API_URL: {foundry_url}")
        logging.info(f"FOUNDRY_API_TOKEN (first 10 chars): {api_token[:10]}")
        logging.info(f"FOUNDRY_RESOURCE_ID: {foundry_resource_id}")

        if not foundry_url or not api_token or not foundry_resource_id:
            logging.error("FOUNDRY_API_URL, FOUNDRY_API_TOKEN, or FOUNDRY_RESOURCE_ID is not set.")
            return func.HttpResponse(
                "FOUNDRY_API_URL, FOUNDRY_API_TOKEN, or FOUNDRY_RESOURCE_ID is not set.",
                status_code=500
            )

        headers = {
            "Authorization": f"Bearer {api_token}",
            "Content-Type": "application/json"
        }

        # Perform a basic API check
        try:
            logging.info("Performing API health check...")
            logging.info(f"Health check URL: {foundry_url}")
            logging.info(f"Health check Headers: {headers}")

            health_check_response = requests.get(foundry_url, headers=headers)
            if health_check_response.status_code != 200:
                logging.error(f"API health check failed: {health_check_response.status_code} - {health_check_response.text}")
                return func.HttpResponse(
                    f"API health check failed: {health_check_response.status_code} - {health_check_response.text}",
                    status_code=502
                )
            logging.info("API health check passed.")
        except requests.exceptions.RequestException as e:
            logging.error(f"API health check failed: {e}")
            return func.HttpResponse(
                f"API health check failed: {e}",
                status_code=502
            )

        # Include the resource ID in the payload or URL
        payload["resourceId"] = foundry_resource_id

        # Log the details of the request
        logging.info(f"Sending POST request to Palantir Foundry:")
        logging.info(f"URL: {foundry_url}")
        logging.info(f"Headers: {headers}")
        logging.info(f"Payload: {json.dumps(payload, indent=2)}")

        # The actual POST request to Foundry
        response = requests.post(foundry_url, headers=headers, json=payload)
        response.raise_for_status()
        logging.info(f"Successfully wrote event to Foundry bucket: {response.status_code}")

        return func.HttpResponse(
            "Successfully wrote event to Foundry bucket.",
            status_code=200
        )

    except ValueError:
        logging.error("Invalid JSON payload.")
        return func.HttpResponse(
            "Invalid JSON payload.",
            status_code=400
        )
    except requests.exceptions.RequestException as e:
        logging.error(f"Failed to write to Foundry bucket: {e}")
        return func.HttpResponse(
            f"Failed to write to Foundry bucket: {e}",
            status_code=500
        )
