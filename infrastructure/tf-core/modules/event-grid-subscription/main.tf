resource "azurerm_eventgrid_event_subscription" "eventgrid_event_subscription" {
  name  = var.subscription_name
  scope = var.azurerm_eventgrid_id

  dynamic "azure_function_endpoint" {
    for_each = var.subscriber_function_endpoints
    content {
      function_id = azure_function_endpoint.value.function_endpoint
    }
  }

  storage_blob_dead_letter_destination {
    storage_account_id          = var.dead_letter_storage_account_id
    storage_blob_container_name = var.dead_letter_storage_account_container_name
  }

  # tags = var.tags
}
