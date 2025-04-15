module "monitor_action_group" {
  for_each = var.regions

  source = "./modules/monitor-action-group"

  name                = lower("example-monitor-action-group-${each.key}")
  resource_group_name = azurerm_resource_group.core[each.key].name
  location            = each.key
  short_name          = var.monitor_action_group.short_name

  email_receiver   = local.email_receiver_list
  webhook_receiver = local.webhook_receiver_list
}

locals {

  email_receiver_list = flatten([
    for _, email_receiver_details in var.monitor_action_group.email_receiver : merge(
      email_receiver_details
    )
  ])

  webhook_receiver_list = flatten([
    for _, webhook_receiver_details in var.monitor_action_group.webhook_receiver : merge(
      webhook_receiver_details
    )
  ])

}
