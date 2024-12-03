module "event_grid" {
  for_each = local.event_grid_map

  source = "./modules/event-grid"

  # topic_name          = "${module.regions_config[each.value.region].names.topic_name}-${lower(each.value.name_suffix)}"
  topic_name            = each.value.event_topic_name
  subscription_name     = each.value.subscription_name
  resource_group_name   = azurerm_resource_group.core[each.value.region].name
  location              = each.value.region
  function_app_endpoint = each.value.function_app_endpoint

  log_analytics_workspace_id = data.terraform_remote_state.audit.outputs.log_analytics_workspace_id[local.primary_region]
  # monitor_diagnostic_setting_keyvault_enabled_logs = local.monitor_diagnostic_setting_keyvault_enabled_logs
  # monitor_diagnostic_setting_keyvault_metrics      = local.monitor_diagnostic_setting_keyvault_metrics

  # dead_letter_storage_account_container_name = module.storage["eventgrid-${each.value.region}"].storage_account_name
  dead_letter_storage_account_container_name = "deadletterqueue"
  dead_letter_storage_account_id             = module.storage["eventgrid-${each.value.region}"].storage_account_id
  dead_letter_storage_account_name           = module.storage["eventgrid-${each.value.region}"].storage_account_name
  # Private Endpoint Configuration if enabled
  # private_endpoint_properties = var.features.private_endpoints_enabled ? {
  #   private_dns_zone_ids_keyvault        = [data.terraform_remote_state.hub.outputs.private_dns_zones["${each.key}-event_grid"].id]
  #   private_endpoint_enabled             = var.features.private_endpoints_enabled
  #   private_endpoint_subnet_id           = module.subnets["${module.regions_config[each.key].names.subnet}-pep"].id
  #   private_endpoint_resource_group_name = azurerm_resource_group.rg_private_endpoints[each.key].name
  #   private_service_connection_is_manual = var.features.private_service_connection_is_manual
  # } : null

  tags = var.tags
}

locals {

  event_grids = {
    for event_grid_key, event_grid_details in var.event_grid_configs :
    event_grid_key => merge(var.event_grid_defaults,
    {
      event_topic_name   = "event-grid-${event_grid_key}"

    }, event_grid_details) # event_grid_details will win merge conflicts
  }

  event_grid_config_object_list = flatten([
    for region in keys(var.regions) : [
      for event_grid_key, event_grid_details in local.event_grids : merge(
        {
          region         = region   # 1st iterator
          event_grid_key = event_grid_key # 2nd iterator
        },
        event_grid_details # the rest of the key/value pairs for a specific event_grids
      )
    ]
  ])

  # ...then project the list of objects into a map with unique keys (combining the iterators), for consumption by a for_each meta argument
  event_grid_map = {
    for object in local.event_grid_config_object_list : "${object.event_grid_key}-${object.region}" => object
  }
}
