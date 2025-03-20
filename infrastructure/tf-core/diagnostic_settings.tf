locals {
  #ACR
  monitor_diagnostic_setting_acr_enabled_logs = ["ContainerRegistryRepositoryEvents", "ContainerRegistryLoginEvents"]
  monitor_diagnostic_setting_acr_metrics      = ["AllMetrics"]

  #APPSERVICEPLAN
  monitor_diagnostic_setting_appserviceplan_metrics = ["AllMetrics"]

  #FUNCTIONAPP
  monitor_diagnostic_setting_function_app_enabled_logs = ["AppServiceAuthenticationLogs", "FunctionAppLogs"]
  monitor_diagnostic_setting_function_app_metrics      = ["AllMetrics"]

  # KEYVAULT
  monitor_diagnostic_setting_keyvault_enabled_logs = ["AuditEvent", "AzurePolicyEvaluationDetails"]
  monitor_diagnostic_setting_keyvault_metrics      = ["AllMetrics"]

  # LOG ANALYTICS WORKSPACE
  monitor_diagnostic_setting_log_analytics_workspace_enabled_logs = ["SummaryLogs", "Audit"]
  monitor_diagnostic_setting_log_analytics_workspace_metrics      = ["AllMetrics"]

  #SQL SERVER AND DATABASE
  monitor_diagnostic_setting_database_enabled_logs   = ["SQLSecurityAuditEvents", "SQLInsights", "QueryStoreWaitStatistics", "Errors", "DatabaseWaitStatistics", "Timeouts"]
  monitor_diagnostic_setting_database_metrics        = ["Basic", "InstanceAndAppAdvanced", "WorkloadManagement"]
  monitor_diagnostic_setting_sql_server_enabled_logs = ["SQLSecurityAuditEvents"]
  monitor_diagnostic_setting_sql_server_metrics      = ["Basic", "InstanceAndAppAdvanced", "WorkloadManagement"]

  #STORAGE ACCOUNT
  monitor_diagnostic_setting_storage_account_enabled_logs = ["StorageWrite", "StorageRead", "StorageDelete"]
  monitor_diagnostic_setting_storage_account_metrics      = ["Capacity", "Transaction"]

  #SUBNET
  monitor_diagnostic_setting_network_security_group_enabled_logs = ["NetworkSecurityGroupEvent", "NetworkSecurityGroupRuleCounter"]

  #VNET
  monitor_diagnostic_setting_vnet_enabled_logs = ["VMProtectionAlerts"]
  monitor_diagnostic_setting_vnet_metrics      = ["AllMetrics"]
}

