application           = "serins"
application_full_name = "service-insights"
#environment           = "TEMP01"   #This comes from the pipeline


features = {
  private_endpoints_enabled              = false
  private_service_connection_is_manual   = false
  public_network_access_enabled          = true
  log_analytics_data_export_rule_enabled = false
}

tags = {
  Project = "Service-Insights"
}

regions = {
  uksouth = {
    is_primary_region = true
    address_space     = "10.255.0.0/16"
    connect_peering   = true
    subnets           = {}
  }
}

app_insights = {
  appinsights_type = "web"
}

law = {
  law_sku        = "PerGB2018"
  retention_days = 30
}

storage_accounts = {
  sqllogs = {
    name_suffix                   = "sqllogs"
    account_tier                  = "Standard"
    replication_type              = "LRS"
    public_network_access_enabled = true
    containers = {
      vulnerability-assessment = {
        container_name        = "vulnerability-assessment"
        container_access_type = "private"
      }
    }
  }
}
