import os
import logging
from azure.storage.blob import BlobServiceClient
from azure.core.exceptions import ResourceExistsError, AzureError

# Configure logging
logging.basicConfig(level=logging.INFO, format='%(asctime)s - %(levelname)s - %(message)s')

def setup_azurite():
    try:
        connect_str = os.getenv("AZURITE_CONNECTION_STRING")
        container_names = [
            os.getenv("AZURITE_CONTAINER_NAME"),
            os.getenv("AZURITE_POISON_CONTAINER_NAME")
        ]

        if not connect_str:
            logging.error("AZURITE_CONNECTION_STRING is not set.")
            return

        if not all(container_names):
            logging.error("One or more container names are not set.")
            return

        # Log the full connection string and container names
        logging.info(f"AZURITE_CONNECTION_STRING: {connect_str}")
        logging.info(f"AZURITE_CONTAINER_NAMES: {container_names}")

        # Establish connection to Azurite
        try:
            blob_service_client = BlobServiceClient.from_connection_string(connect_str)
            logging.info("Successfully connected to Azurite.")

            # Log the Blob Service URL
            logging.info(f"Blob Service URL: {blob_service_client.url}")
        except AzureError as e:
            logging.error(f"Failed to connect to Azurite: {str(e)}")
            return

        # Create blob containers
        for container_name in container_names:
            if not container_name:
                logging.error("Container name is empty.")
                continue

            try:
                blob_service_client.create_container(container_name)
                logging.info(f"Blob container '{container_name}' created successfully.")
            except ResourceExistsError:
                logging.info(f"Blob container '{container_name}' already exists.")
            except AzureError as e:
                logging.error(f"Error while creating blob container '{container_name}': {str(e)}")

        # List all blob containers
        logging.info("Listing Blob containers:")
        try:
            containers = blob_service_client.list_containers()
            for container in containers:
                logging.info(f"Container Name: {container['name']}, Last Modified: {container['last_modified']}")
        except AzureError as e:
            logging.error(f"Error while listing blob containers: {str(e)}")

    except Exception as e:
        logging.error(f"An unexpected error occurred: {str(e)}")

# Run the setup
setup_azurite()
