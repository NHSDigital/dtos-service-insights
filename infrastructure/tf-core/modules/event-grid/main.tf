resource "azurerm_eventgrid_topic" "azurerm_eventgrid" {
  name                = var.topic_name
  resource_group_name = var.resource_group_name
  location            = var.location

  identity {
    type = var.identity_type
  }

  dynamic "inbound_ip_rule" {
    for_each = var.inbound_ip_rules
    content {
      ip_mask = inbound_ip_rule.value["ip_mask"]
      action  = inbound_ip_rule.value["action"]
    }
  }

  tags = var.tags

}

# resource "azurerm_storage_queue" "storage_queue" {
#   name                 = "${var.subscription_name}-storage-queue"
#   storage_account_name = var.dead_letter_storage_account_name
# }


resource "azurerm_eventgrid_event_subscription" "eventgrid_event_subscription" {
  name                      = var.subscription_name
  scope                     = azurerm_eventgrid_topic.azurerm_eventgrid.id
  # storage_blob_dead_letter_destination {
  #   storage_account_id  = dead_letter_storage_account_container_id
  #   blob_container_name = dead_letter_storage_account_container_name
  # }

  # storage_queue_endpoint {
  #   storage_account_id  = var.dead_letter_storage_account_id
  #   queue_name          = azurerm_storage_queue.storage_queue.name
  # }

  # HTTP endpoint for the second function app (create participant screening episode)
  webhook_endpoint {
    url = var.function_app_endpoint  # URL of the second function app that will process the event
  }

  storage_blob_dead_letter_destination {
    storage_account_id          = var.dead_letter_storage_account_id
    storage_blob_container_name = var.dead_letter_storage_account_container_name
  }

}
