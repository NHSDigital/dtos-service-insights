module "monitor_action_group" {
  for_each = var.regions

  source = "modules/monitor_action_group"

  name                = lower("example-monitor-action-group-${each.key}")
  resource_group_name = azurerm_resource_group.core[each.key].name
  location            = each.key
  short_name          = "testing123"

  email_receiver = [
    {
      name                    = "testing"
      email_address           = "testing@testing.com"
      use_common_alert_schema = false
    }
  ]

}
