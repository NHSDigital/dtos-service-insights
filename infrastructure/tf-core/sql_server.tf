locals {
  primary_region = [for k, v in var.regions : k if v.is_primary_region][0]

  ## Overrides mainly used by dynamically created Temporary environments:
  sql_admin_group_name = var.sqlserver.sql_admin_group_name != "" ? var.sqlserver.sql_admin_group_name : "sqlsvr_${var.application}_${lower(var.environment)}_${local.primary_region}_admin"

}

module "azure_sql_server" {
  for_each = var.sqlserver != {} ? var.regions : {}
  # for_each = var.sqlserver

  source = "../../../dtos-devops-templates/infrastructure/modules/sql-server"

  # Azure SQL Server
  name                = module.regions_config[each.key].names.sql-server
  resource_group_name = azurerm_resource_group.core[each.key].name
  location            = each.key

  sqlversion = var.sqlserver.server.sqlversion
  tlsver     = var.sqlserver.server.tlsversion
  kv_id      = module.key_vault[each.key].key_vault_id

  # Diagnostic Settings
  log_analytics_workspace_id                         = data.terraform_remote_state.audit.outputs.log_analytics_workspace_id[local.primary_region]
  primary_blob_endpoint_name                         = data.terraform_remote_state.audit.outputs.storage_account_audit["sqllogs-${local.primary_region}"].primary_blob_endpoint_name
  storage_account_name                               = data.terraform_remote_state.audit.outputs.storage_account_audit["sqllogs-${local.primary_region}"].name
  storage_account_id                                 = data.terraform_remote_state.audit.outputs.storage_account_audit["sqllogs-${local.primary_region}"].id
  storage_container_id                               = data.terraform_remote_state.audit.outputs.storage_account_audit["sqllogs-${local.primary_region}"].containers["vulnerability-assessment"].id
  monitor_diagnostic_setting_database_enabled_logs   = local.monitor_diagnostic_setting_database_enabled_logs
  monitor_diagnostic_setting_database_metrics        = local.monitor_diagnostic_setting_database_metrics
  monitor_diagnostic_setting_sql_server_enabled_logs = local.monitor_diagnostic_setting_sql_server_enabled_logs
  monitor_diagnostic_setting_sql_server_metrics      = local.monitor_diagnostic_setting_sql_server_metrics
  log_monitoring_enabled                             = true

  sql_server_alert_policy_state = "Enabled"

  sql_uai_name         = var.sqlserver.sql_uai_name
  sql_admin_group_name = local.sql_admin_group_name

  sql_admin_object_id                  = data.azuread_group.sql_admin_group.object_id
  ad_auth_only                         = var.sqlserver.ad_auth_only
  security_alert_policy_retention_days = var.sqlserver.security_alert_policy_retention_days
  auditing_policy_retention_in_days    = var.sqlserver.auditing_policy_retention_in_days

  public_network_access_enabled = var.sqlserver.public_network_access_enabled

  # Default database
  db_name_suffix = var.sqlserver.dbs.serins.db_name_suffix
  collation      = var.sqlserver.dbs.serins.collation
  licence_type   = var.sqlserver.dbs.serins.licence_type
  max_gb         = var.sqlserver.dbs.serins.max_gb
  read_scale     = var.sqlserver.dbs.serins.read_scale
  sku            = var.sqlserver.dbs.serins.sku

  # FW Rules
  firewall_rules = var.sqlserver.fw_rules

  # Private Endpoint Configuration if enabled
  private_endpoint_properties = var.features.private_endpoints_enabled ? {
    private_dns_zone_ids_sql             = [data.terraform_remote_state.hub.outputs.private_dns_zones["${each.key}-azure_sql"].id]
    private_endpoint_enabled             = var.features.private_endpoints_enabled
    private_endpoint_subnet_id           = module.subnets["${module.regions_config[each.key].names.subnet}-pep"].id
    private_endpoint_resource_group_name = azurerm_resource_group.rg_private_endpoints[each.key].name
    private_service_connection_is_manual = var.features.private_service_connection_is_manual
  } : null

  tags = var.tags

  depends_on = values(module.storage)[*].storage_account_id
}
