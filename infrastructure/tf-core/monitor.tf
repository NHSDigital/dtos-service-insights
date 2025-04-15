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
  email_receiver_list = values(var.monitor_action_group.email_receiver)
  webhook_receiver_list = values(var.monitor_action_group.webhook_receiver)

}
