module "event_grid_topic" {
  for_each = var.features.event_grid_topic_enabled_in_project_vnet ? local.event_grid_map : {}

  source = "../../../dtos-devops-templates/infrastructure/modules/event-grid-topic"

  topic_name          = each.value.event_grid_subscription_key
  resource_group_name = azurerm_resource_group.core[each.value.region].name
  location            = each.value.region
  # identity_type       = each.value.identity_type
  identity_type    = "SystemAssigned"
  inbound_ip_rules = each.value.inbound_ip_rules

  # Private Endpoint Configuration if enabled
  private_endpoint_properties = null

  tags = var.tags
}

module "event_grid_subscription" {
  for_each = local.event_grid_map

  source = "../../../dtos-devops-templates/infrastructure/modules/event-grid-subscription"

  subscription_name   = each.value.event_grid_subscription_key
  resource_group_name = azurerm_resource_group.core[each.value.region].name
  # azurerm_eventgrid_id = data.terraform_remote_state.hub.outputs.event_grid_topic["${each.value.event_grid_subscription_key}-${each.value.region}"].id
  azurerm_eventgrid_id = var.features.event_grid_topic_enabled_in_project_vnet == true ? module.event_grid_topic["${each.value.event_grid_subscription_key}-${each.value.region}"].id : data.terraform_remote_state.hub.outputs.event_grid_topic["${each.value.event_grid_subscription_key}-${each.value.region}"].id

  function_endpoint = format("%s/functions/%s", module.functionapp["${each.value.subscriber_functionName}-${each.value.region}"].id, each.value.subscriber_functionName)
  principal_id      = module.functionapp["${each.value.subscriber_functionName}-${each.value.region}"].function_app_sami_id

  dead_letter_storage_account_container_name = "deadletterqueue"
  dead_letter_storage_account_id             = module.storage["eventgrid-${each.value.region}"].storage_account_id

  tags = var.tags
}

locals {

  event_grid_config_object_list = flatten([
    for region in keys(var.regions) : [
      for event_grid_subscription_key, event_grid_subscription_details in var.event_grid_subscriptions.subscriber_config : merge(
        {
          region                      = region                      # 1st iterator
          event_grid_subscription_key = event_grid_subscription_key # 2nd iterator
          inbound_ip_rules            = []
        },
        event_grid_subscription_details # the rest of the key/value pairs for a specific event_grids
      )
    ]
  ])

  # ...then project the list of objects into a map with unique keys (combining the iterators), for consumption by a for_each meta argument
  event_grid_map = {
    for object in local.event_grid_config_object_list : "${object.event_grid_subscription_key}-${object.region}" => object
  }
}
