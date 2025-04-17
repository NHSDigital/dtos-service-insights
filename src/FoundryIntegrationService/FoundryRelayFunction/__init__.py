import logging
import json
import os
import requests
import azure.functions as func

def main(event: func.EventGridEvent):
    logging.info('FoundryRelayFunction triggered by Event Grid event.')

    # Log the event details
    logging.info(f"Raw Event Data: {event.get_json()}")
    logging.info(f"Event Subject: {event.subject}")
    logging.info(f"Event Type: {event.event_type}")
    logging.info(f"Event Data: {event.get_json()}")

    # Write to Foundry bucket via Foundry API
    foundry_url = os.getenv("FOUNDRY_API_URL")
    api_token = os.getenv("FOUNDRY_API_TOKEN")

    # Log the Foundry API URL and token
    logging.info(f"FOUNDRY_API_URL: {foundry_url}")
    logging.info(f"FOUNDRY_API_TOKEN (first 10 chars): {api_token[:10]}")

    if not foundry_url or not api_token:
        logging.error("FOUNDRY_API_URL or FOUNDRY_API_TOKEN is not set.")
        return

    try:
        headers = {
            "Authorization": f"Bearer {api_token}",
            "Content-Type": "application/json"
        }
        payload = event.get_json()

        # Log the details of the request
        logging.info(f"Would send POST request to Palantir Foundry:")
        logging.info(f"URL: {foundry_url}")
        logging.info(f"Headers: {headers}")
        logging.info(f"Payload: {json.dumps(payload, indent=2)}")

        # The actual POST request to Foundry
        response = requests.post(foundry_url, headers=headers, json=payload)
        response.raise_for_status()
        logging.info(f"Successfully wrote event to Foundry bucket: {response.status_code}")

    except requests.exceptions.RequestException as e:
        logging.error(f"Failed to write to Foundry bucket: {e}")
