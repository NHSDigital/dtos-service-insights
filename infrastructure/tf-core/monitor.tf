module "monitor_action_group" {
  for_each = local.monitor_action_group_map

  source = "./modules/monitor-action-group"

  name                = lower("example-monitor-action-group-${each.key}")
  resource_group_name = azurerm_resource_group.core[each.key].name
  location            = each.key
  short_name          = "testing123"

  email_receiver = [
    {
      name                    = each.value.name
      email_address           = each.value.email_address
      use_common_alert_schema = false
    }
  ]

  # webhook_receiver = [
  #   {
  #     name                    = "testing"
  #     service_uri             = "testing@testing.com"
  #     use_common_alert_schema = false
  #   }
  # ]
}

locals {

  monitor_action_group_list = flatten([
    for region in keys(var.regions) : [
      for email_receiver_key, email_receiver_details in var.monitor_action_group.email_receiver : merge(
        {
          region             = region             # 1st iterator
          email_receiver_key = email_receiver_key # 2nd iterator
        },
        email_receiver_details # the rest of the key/value pairs for a specific event_grids
      )
    ]
  ])

  # ...then project the list of objects into a map with unique keys (combining the iterators), for consumption by a for_each meta argument
  monitor_action_group_map = {
    for object in local.monitor_action_group_list : "${object.email_receiver_key}-${object.region}" => object
  }


}
