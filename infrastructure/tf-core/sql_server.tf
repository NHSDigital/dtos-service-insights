module "azure_sql_server" {
  for_each = var.sqlserver != {} ? var.regions : {}

  source = "../../../dtos-devops-templates/infrastructure/modules/sql-server"

  # Azure SQL Server
  name                = module.regions_config[each.key].names.sql-server
  resource_group_name = azurerm_resource_group.core[each.key].name
  location            = each.key

  sqlversion = var.sqlserver.server.sqlversion
  tlsver     = var.sqlserver.server.tlsversion
  kv_id      = module.key_vault[each.key].key_vault_id

  sql_uai_name         = var.sqlserver.sql_uai_name
  sql_admin_group_name = var.sqlserver.sql_admin_group_name
  sql_admin_object_id  = data.azuread_group.sql_admin_group.object_id
  ad_auth_only         = var.sqlserver.ad_auth_only

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
    private_dns_zone_ids_sql             = [data.terraform_remote_state.hub.outputs.private_dns_zone_azure_sql[each.key].private_dns_zone.id]
    private_endpoint_enabled             = var.features.private_endpoints_enabled
    private_endpoint_subnet_id           = module.subnets["${module.regions_config[each.key].names.subnet}-pep"].id
    private_endpoint_resource_group_name = azurerm_resource_group.rg_private_endpoints[each.key].name
    private_service_connection_is_manual = var.features.private_service_connection_is_manual
  } : null

  tags = var.tags
}