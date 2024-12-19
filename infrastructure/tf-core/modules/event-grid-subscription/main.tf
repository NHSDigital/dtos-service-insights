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
data "azurerm_client_config" "current" {}



resource "azurerm_role_assignment" "eventgrid_function_permission" {
  for_each = var.subscriber_function_endpoints
  # for_each = zipmap(
  #   [for i, endpoint in var.subscriber_function_endpoints : i],
  #   var.subscriber_function_endpoints
  # )

  scope                = each.value.function_endpoint
  role_definition_name = "Azure Event Grid System Topic Event Subscription Contributor"
  principal_id         = data.azurerm_client_config.current.object_id
}
