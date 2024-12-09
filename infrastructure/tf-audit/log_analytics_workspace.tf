# module "log_analytics_workspace_audit" {
#   for_each = { for key, val in var.regions : key => val if val.is_primary_region }

#   source = "../../../dtos-devops-templates/infrastructure/modules/log-analytics-workspace"

#   name     = module.regions_config[each.key].names.log-analytics-workspace
#   location = each.key

#   law_sku        = var.law.law_sku
#   retention_days = var.law.retention_days

#   monitor_diagnostic_setting_log_analytics_workspace_enabled_logs = local.monitor_diagnostic_setting_log_analytics_workspace_enabled_logs
#   monitor_diagnostic_setting_log_analytics_workspace_metrics      = local.monitor_diagnostic_setting_log_analytics_workspace_metrics

#   resource_group_name = azurerm_resource_group.audit[each.key].name

#   tags = var.tags
# }

module "log_analytics_workspace_audit" {
  for_each = var.regions

  source = "../../../dtos-devops-templates/infrastructure/modules/log-analytics-workspace"

  name     = module.regions_config[each.key].names.log-analytics-workspace
  location = each.key

  law_sku        = var.law.law_sku
  retention_days = var.law.retention_days

  monitor_diagnostic_setting_log_analytics_workspace_enabled_logs = local.monitor_diagnostic_setting_log_analytics_workspace_enabled_logs
  monitor_diagnostic_setting_log_analytics_workspace_metrics      = local.monitor_diagnostic_setting_log_analytics_workspace_metrics

  resource_group_name = azurerm_resource_group.audit[each.key].name

  tags = var.tags
}

/*--------------------------------------------------------------------------------------------------
  RBAC Assignments
--------------------------------------------------------------------------------------------------*/
/*
For sending events to the Event Hub:
* Azure Event Hubs Data Sender: Grants permissions to send events to the Event Hub.  
* For receiving events from the Event Hub:

For receiving events from the Event Hub (i.e. remote resource):
* Azure Event Hubs Data Receiver: Grants permissions to receive events from the Event Hub.
*/
# module "rbac_assignments" {
#   for_each = var.regions

#   source = "../../../dtos-devops-templates/infrastructure/modules/rbac-assignment"

#   principal_id         = module.log_analytics_workspace_audit[each.key].0.principal_id
#   role_definition_name = "Azure Event Hubs Data Sender"
#   scope                = data.terraform_remote_state.hub.outputs.eventhub_law_export_id["dtos-hub-${each.key}"]
# }

output "log_analytics_workspace_audit" {
  value = module.log_analytics_workspace_audit
}
