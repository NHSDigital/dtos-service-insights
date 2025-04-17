module "monitor_action_group" {
  for_each = local.monitor_action_group_map

  source = "../../../dtos-devops-templates/infrastructure/modules/monitor-action-group"

  name                = module.regions_config[each.value.region].names.monitor-action-group
  resource_group_name = azurerm_resource_group.core[each.value.region].name
  location            = each.value.region
  short_name          = "test"
  # short_name          = each.value.short_name
  email_receiver      = each.value.email_receiver

  # email_receiver   = local.email_receiver_list
  # webhook_receiver = local.webhook_receiver_list
}

locals {
  # email_receiver_list = values(var.monitor_action_group.email_receiver)
  # webhook_receiver_list = values(var.monitor_action_group.webhook_receiver)

  monitor_action_group_object_list = flatten([
    for region in keys(var.regions) : [
      for action_group_key, action_group_details in var.monitor_action_group : merge(
        {
          region           = region
          action_group_key = action_group_key
        },
        action_group_details
      )
      # for _, email_receiver_details in var.monitor_action_group.email_receiver : merge(
      #   email_receiver_details
      # )
      # for _, email_receiver_details in var.monitor_action_group.email_receiver : merge(
      #   email_receiver_details
      # )
    ]
  ])

  # ...then project the list of objects into a map with unique keys (combining the iterators), for consumption by a for_each meta argument
  monitor_action_group_map = {
    for object in local.monitor_action_group_object_list : "${object.action_group_key}-${object.region}" => object
  }
}
