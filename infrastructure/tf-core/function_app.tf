module "functionapp" {
  for_each = local.function_app_map

  source = "../../../dtos-devops-templates/infrastructure/modules/function-app"

  function_app_name   = "${module.regions_config[each.value.region].names.function-app}-si-${lower(each.value.name_suffix)}"
  resource_group_name = azurerm_resource_group.core[each.value.region].name
  location            = each.value.region

  # app_settings = local.app_settings[each.value.region_key][each.value.function_key]
  app_settings = each.value.app_settings

  log_analytics_workspace_id                           = data.terraform_remote_state.audit.outputs.log_analytics_workspace_id[local.primary_region]
  monitor_diagnostic_setting_function_app_enabled_logs = local.monitor_diagnostic_setting_function_app_enabled_logs
  monitor_diagnostic_setting_function_app_metrics      = local.monitor_diagnostic_setting_function_app_metrics

  # public_network_access_enabled = length(keys(each.value.ip_restrictions)) > 0 ? true : var.features.public_network_access_enabled

  # vnet_integration_subnet_id = module.subnets["${module.regions_config[each.value.region].names.subnet}-apps"].id
  public_network_access_enabled = var.features.public_network_access_enabled
  vnet_integration_subnet_id    = var.features.private_endpoints_enabled ? module.subnets["${module.regions_config[each.value.region].names.subnet}-apps"].id : null

  # rbac_role_assignments = local.rbac_role_assignments[each.value.region]
  rbac_role_assignments = each.value.rbac_role_assignments

  asp_id = module.app-service-plan["${each.value.app_service_plan_key}-${each.value.region}"].app_service_plan_id

  # Use the storage account assigned identity for the Function Apps:
  storage_account_name          = module.storage["fnapp-${each.value.region}"].storage_account_name
  storage_account_access_key    = var.function_apps.storage_uses_managed_identity == true ? null : module.storage["fnapp-${each.value.region}"].storage_account_primary_access_key
  storage_uses_managed_identity = var.function_apps.storage_uses_managed_identity

  # Connection string for Application Insights:
  ai_connstring = data.azurerm_application_insights.ai.connection_string

  # Use the ACR assigned identity for the Function Apps:
  cont_registry_use_mi = var.function_apps.cont_registry_use_mi

  # Other Function App configuration settings:
  always_on    = var.function_apps.always_on
  worker_32bit = var.function_apps.worker_32bit

  # Use local ACR if present:
  acr_mi_client_id = var.acr != null ? module.acr[0].mi_client_id : data.azurerm_user_assigned_identity.acr_mi.client_id
  acr_login_server = var.acr != null ? module.acr[0].login_server : data.azurerm_container_registry.acr.login_server

  # acr_mi_client_id = data.azurerm_user_assigned_identity.acr_mi.client_id
  # acr_login_server = data.azurerm_container_registry.acr.login_server

  # Use the ACR assigned identity for the Function Apps too:
  assigned_identity_ids = var.function_apps.cont_registry_use_mi ? [data.azurerm_user_assigned_identity.acr_mi.id] : []

  image_tag  = var.function_apps.docker_env_tag
  image_name = "${var.function_apps.docker_img_prefix}-${lower(each.value.name_suffix)}"

  # Private Endpoint Configuration if enabled
  private_endpoint_properties = var.features.private_endpoints_enabled ? {
    private_dns_zone_ids                 = [data.terraform_remote_state.hub.outputs.private_dns_zones["${each.value.region}-app_services"].id]
    private_endpoint_enabled             = var.features.private_endpoints_enabled
    private_endpoint_subnet_id           = module.subnets["${module.regions_config[each.value.region].names.subnet}-pep"].id
    private_endpoint_resource_group_name = azurerm_resource_group.rg_private_endpoints[each.value.region].name
    private_service_connection_is_manual = var.features.private_service_connection_is_manual
  } : null

  ip_restrictions               = each.value.ip_restrictions
  ip_restriction_default_action = var.function_apps.ip_restriction_default_action

  function_app_slots = var.function_app_slots

  # To enable health checks for function apps
  health_check_path = var.function_apps.health_check_path

  # To enable app service log for function apps
  app_service_logs_retention_period_days = var.function_apps.app_service_logs_retention_period_days
  app_service_logs_disk_quota_mb         = var.function_apps.app_service_logs_disk_quota_mb

  tags = var.tags

  # depends_on = [module.storage[*].storage_account_id]
  depends_on = [module.storage]

}

locals {
  # Filter fa_config to only include those with event_grid_topic_producers
  event_grid_function_app_map = {
    for app_key, app_value in var.function_apps.fa_config :
    app_key => app_value
    if contains(keys(app_value), "event_grid_topic_producers")
  }

  # There are multiple maps
  # We cannot nest for loops inside a map, so first iterate all permutations as a list of objects...
  unified_event_grid_object_list = flatten([
    for event_key, event_value in local.event_grid_map : [
      for function_key, function_values in local.event_grid_function_app_map : merge({
        event_key    = event_key    # 1st iterator
        function_key = function_key # 2nd iterator
        event_value  = event_value
      }, function_values) # the block of key/value pairs for a specific collection
      if contains(keys(function_values), "event_grid_topic_producers")
      && (function_values.event_grid_topic_producers != null ? length(function_values.event_grid_topic_producers) > 0 : false)
    ]
  ])
  # ...then project them into a map with unique keys (combining the iterators), for consumption by a for_each meta argument
  unified_event_grid_object_map = {
    for item in local.unified_event_grid_object_list :
    "${item.event_key}-${item.function_key}" => item
  }
}

# Use the merged map in your resources
resource "azurerm_role_assignment" "data_sender" {
  for_each = local.unified_event_grid_object_map

  principal_id         = module.functionapp["${each.value.function_key}-${each.value.event_value.region}"].function_app_sami_id
  role_definition_name = "EventGrid Data Sender"
  # scope              = data.terraform_remote_state.hub.outputs.event_grid_topic["${each.value.event_key}"].id
  scope = var.features.event_grid_topic_enabled_in_project_vnet == true ? module.event_grid_topic["${each.value.event_key}"].id : data.terraform_remote_state.hub.outputs.event_grid_topic["${each.value.event_key}"].id

}

/* --------------------------------------------------------------------------------------------------
  Local variables used to create the Environment Variables for the Function Apps
-------------------------------------------------------------------------------------------------- */
locals {

  app_settings_common = {
    DOCKER_ENABLE_CI                    = var.function_apps.docker_CI_enable
    REMOTE_DEBUGGING_ENABLED            = var.function_apps.remote_debugging_enabled
    WEBSITES_ENABLE_APP_SERVICE_STORAGE = var.function_apps.enable_appsrv_storage
    WEBSITE_PULL_IMAGE_OVER_VNET        = var.features.private_endpoints_enabled

  }

  # There are multiple Function Apps and possibly multiple regions.
  # We cannot nest for loops inside a map, so first iterate all permutations of both as a list of objects...
  function_app_config_object_list = flatten([
    for region in keys(var.regions) : [
      for function, config in var.function_apps.fa_config : merge(
        {
          region   = region   # 1st iterator
          function = function # 2nd iterator
        },
        config, # the rest of the key/value pairs for a specific function
        {
          ip_restriction = config.ip_restrictions

          app_settings = merge(
            local.app_settings_common,
            config.env_vars_static,


            # # Dynamic env vars which cannot be stored in tfvars file
            # function == "example-function" ? {
            #   EXAMPLE_API_KEY = data.azurerm_key_vault_secret.example[region].versionless_id
            # } : {},

            # Dynamic references to other Function App URLs
            {
              for obj in config.app_urls : obj.env_var_name => format(
                "https://%s-si-%s.azurewebsites.net/api/%s",
                module.regions_config[region].names["function-app"],
                var.function_apps.fa_config[obj.function_app_key].name_suffix,
                length(obj.endpoint_name) > 0 ? obj.endpoint_name : var.function_apps.fa_config[obj.function_app_key].function_endpoint_name
              )
            },

            # Dynamic reference to Key Vault
            length(config.key_vault_url) > 0 ? {
              (config.key_vault_url) = module.key_vault[region].key_vault_url
            } : {},

            # Storage - The C# code should be updated to use System Managed Identity, rather than connection string
            length(config.storage_account_env_var_name) > 0 ? merge(
              {
                (config.storage_account_env_var_name) = module.storage["file_exceptions-${region}"].storage_account_primary_connection_string
              },
              var.features.private_endpoints_enabled ? {
                "${config.storage_account_env_var_name}__blobServiceUri"  = "https://${module.storage["file_exceptions-${region}"].storage_account_name}.blob.core.windows.net"
                "${config.storage_account_env_var_name}__queueServiceUri" = "https://${module.storage["file_exceptions-${region}"].storage_account_name}.queue.core.windows.net"
              } : {}
            ) : {},

            length(try(config.event_grid_topic_producers, [])) > 0 ? merge(
              {
                for idx, producer in coalescelist(config.event_grid_topic_producers, []) :
                "topicEndpoint${idx + 1}" => var.features.event_grid_topic_enabled_in_project_vnet == true ? module.event_grid_topic["${producer}-${region}"].topic_endpoint : data.terraform_remote_state.hub.outputs.event_grid_topic["${producer}-${region}"].topic_endpoint
              }
            ) : {},

            length(config.storage_containers) > 0 ? {
              for k, v in config.storage_containers :
              v.env_var_name => v.container_name
            } : {},

            # Database connection string
            length(config.db_connection_string) > 0 ? {
              (config.db_connection_string) = "Server=${module.regions_config[region].names.sql-server}.database.windows.net; Authentication=Active Directory Managed Identity; Database=${var.sqlserver.dbs.serins.db_name_suffix}"
            } : {}
          )

          # These RBAC assignments are for the Function Apps only
          rbac_role_assignments = flatten([

            # Key Vault
            var.key_vault != {} && length(config.key_vault_url) > 0 ? [
              for role in local.rbac_roles_key_vault : {
                role_definition_name = role
                scope                = module.key_vault[region].key_vault_id
              }
            ] : [],

            # Storage Accounts
            [
              for account in keys(var.storage_accounts) : [
                for role in local.rbac_roles_storage : {
                  role_definition_name = role
                  scope                = module.storage["${account}-${region}"].storage_account_id
                }
              ]
            ],

            # Database
            [
              for role in local.rbac_roles_database : {
                role_definition_name = role
                scope                = module.azure_sql_server[region].sql_server_id
              }
            ]

          ])

        }
      )
    ]
  ])

  # ...then project the list of objects into a map with unique keys (combining the iterators), for consumption by a for_each meta argument
  function_app_map = {
    for object in local.function_app_config_object_list : "${object.function}-${object.region}" => object
  }
}
