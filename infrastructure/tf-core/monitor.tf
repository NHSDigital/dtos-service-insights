module "monitor_action_group" {
  for_each = var.regions

  source = "./modules/monitor-action-group"

  name                = lower("example-monitor-action-group-${each.key}")
  resource_group_name = azurerm_resource_group.core[each.key].name
  location            = each.key
  short_name          = var.monitor_action_group.short_name

  email_receiver = local.email_receiver_list

  # webhook_receiver = [
  #   {
  #     name                    = "testing"
  #     service_uri             = "testing@testing.com"
  #     use_common_alert_schema = false
  #   }
  # ]
}

locals {

  email_receiver_list = flatten([
    for email_receiver_key, email_receiver_details in var.monitor_action_group.email_receiver : merge(
      # {
      #   region             = region             # 1st iterator
      #   email_receiver_key = email_receiver_key # 2nd iterator
      # },
      email_receiver_details # the rest of the key/value pairs for a specific event_grids
    )
  ])

  # ...then project the list of objects into a map with unique keys (combining the iterators), for consumption by a for_each meta argument
  # email_receiver_map = {
  #   for object in local.email_receiver_list : "${object.email_receiver_key}-${object.region}" => object
  # }


}
