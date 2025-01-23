application           = "serins"
application_full_name = "service-insights"
environment           = "DEV"

features = {
  acr_enabled                          = false
  api_management_enabled               = false
  event_grid_enabled                   = false
  private_endpoints_enabled            = true
  private_service_connection_is_manual = false
  public_network_access_enabled        = false
}

tags = {
  Project = "Service-Insights"
}

regions = {
  uksouth = {
    is_primary_region = true
    address_space     = "10.113.0.0/16"
    connect_peering   = true
    subnets = {
      apps = {
        cidr_newbits               = 8
        cidr_offset                = 2
        delegation_name            = "Microsoft.Web/serverFarms"
        service_delegation_name    = "Microsoft.Web/serverFarms"
        service_delegation_actions = ["Microsoft.Network/virtualNetworks/subnets/action"]
      }
      pep = {
        cidr_newbits = 8
        cidr_offset  = 1
      }
      sql = {
        cidr_newbits = 8
        cidr_offset  = 3
      }
    }
  }
}

routes = {
  uksouth = {
    firewall_policy_priority = 100
    application_rules        = []
    nat_rules                = []
    network_rules = [
      {
        name                  = "AllowSerinsToAudit"
        priority              = 800
        action                = "Allow"
        rule_name             = "SerinsToAudit"
        source_addresses      = ["10.113.0.0/16"] # will be populated with the serins manager subnet address space
        destination_addresses = ["10.114.0.0/16"] # will be populated with the audit subnet address space
        protocols             = ["TCP", "UDP"]
        destination_ports     = ["443"]
      },
      {
        name                  = "AllowAuditToSerins"
        priority              = 810
        action                = "Allow"
        rule_name             = "AuditToSerins"
        source_addresses      = ["10.114.0.0/16"]
        destination_addresses = ["10.113.0.0/16"]
        protocols             = ["TCP", "UDP"]
        destination_ports     = ["443"]
      }
    ]
    route_table_routes_to_audit = [
      {
        name                   = "SerinsToAudit"
        address_prefix         = "10.114.0.0/16"
        next_hop_type          = "VirtualAppliance"
        next_hop_in_ip_address = "" # will be populated with the Firewall Private IP address
      }
    ]
    route_table_routes_from_audit = [
      {
        name                   = "AuditToSerins"
        address_prefix         = "10.113.0.0/16"
        next_hop_type          = "VirtualAppliance"
        next_hop_in_ip_address = "" # will be populated with the Firewall Private IP address
      }
    ]
  }
}

app_service_plan = {
  os_type                  = "Linux"
  sku_name                 = "P2v3"
  vnet_integration_enabled = true

  autoscale = {
    memory_percentage = {
      metric = "MemoryPercentage"

      capacity_min = "1"
      capacity_max = "5"
      capacity_def = "1"

      time_grain       = "PT1M"
      statistic        = "Average"
      time_window      = "PT10M"
      time_aggregation = "Average"

      inc_operator        = "GreaterThan"
      inc_threshold       = 70
      inc_scale_direction = "Increase"
      inc_scale_type      = "ChangeCount"
      inc_scale_value     = 1
      inc_scale_cooldown  = "PT5M"

      dec_operator        = "LessThan"
      dec_threshold       = 25
      dec_scale_direction = "Decrease"
      dec_scale_type      = "ChangeCount"
      dec_scale_value     = 1
      dec_scale_cooldown  = "PT5M"
    }
  }

  instances = {
    Default = {}
    # BIAnalyticsDataService       = {}
    # BIAnalyticsService           = {}
    # DemographicsService          = {}
    # EpisodeDataService           = {}
    # EpisodeIntegrationService    = {}
    # EpisodeManagementService     = {}
    # MeshIntegrationService       = {}
    # ParticipantManagementService = {}
    # ReferenceDataService         = {}
  }
}

diagnostic_settings = {
  metric_enabled = true
}

event_grid_defaults = {
  identity_ids                  = []
  identity_type                 = "SystemAssigned"
  inbound_ip_rules              = []
  input_schema                  = {}
  local_auth_enabled            = true
  public_network_access_enabled = false
}

event_grid_configs = {
  event-grid-topic-1 = {
    identity_type                = "SystemAssigned"
    subscription_name            = "dev1234"
    subscriber_functionName_list = ["CreateParticipantScreeningEpisode"]
  }
  # event-grid-topic-2 = {
  #   identity_type                = "SystemAssigned"
  #   subscription_name            = "sub2"
  #   subscriber_list_functionName = []
  #   publisher_list               = []
  # }
}

function_apps = {
  acr_mi_name = "dtos-service-insights-acr-push"
  acr_name    = "acrukshubdevserins"
  acr_rg_name = "rg-hub-dev-uks-serins"

  app_insights_name    = "appi-dev-uks-serins"
  app_insights_rg_name = "rg-serins-dev-uks-audit"

  always_on = true

  cont_registry_use_mi = true

  docker_CI_enable  = "true"
  docker_env_tag    = "development"
  docker_img_prefix = "service-insights"

  enable_appsrv_storage         = "false"
  ftps_state                    = "Disabled"
  https_only                    = true
  remote_debugging_enabled      = false
  storage_uses_managed_identity = null
  worker_32bit                  = false

  fa_config = {

    CreateParticipantScreeningEpisodeData = {
      name_suffix            = "create-ps-episode-data"
      function_endpoint_name = "CreateParticipantScreeningEpisodeData"
      app_service_plan_key   = "Default"
      db_connection_string   = "ServiceInsightsDbConnectionString"
    }

    CreateParticipantScreeningProfileData = {
      name_suffix            = "create-ps-profile-data"
      function_endpoint_name = "CreateParticipantScreeningProfileData"
      app_service_plan_key   = "Default"
      db_connection_string   = "ServiceInsightsDbConnectionString"
    }

    GetParticipantScreeningProfile = {
      name_suffix            = "get-ps-profile"
      function_endpoint_name = "GetParticipantScreeningProfile"
      app_service_plan_key   = "Default"
      db_connection_string   = "ServiceInsightsDbConnectionString"
      app_urls = [
        {
          env_var_name     = "GetProfilesUrl"
          function_app_key = "GetParticipantScreeningProfileData"
        }
      ]
    }

    GetParticipantScreeningProfileData = {
      name_suffix            = "get-ps-profile-data"
      function_endpoint_name = "GetParticipantScreeningProfileData"
      app_service_plan_key   = "Default"
      db_connection_string   = "ServiceInsightsDbConnectionString"
    }

    GetParticipantScreeningEpisode = {
      name_suffix            = "get-ps-episode"
      function_endpoint_name = "GetParticipantScreeningEpisode"
      app_service_plan_key   = "Default"
      db_connection_string   = "ServiceInsightsDbConnectionString"
    }

    GetParticipantScreeningEpisodeData = {
      name_suffix            = "get-ps-episode-data"
      function_endpoint_name = "GetParticipantScreeningEpisodeData"
      app_service_plan_key   = "Default"
      db_connection_string   = "ServiceInsightsDbConnectionString"
    }

    CreateParticipantScreeningEpisode = {
      name_suffix            = "create-ps-episode"
      function_endpoint_name = "CreateParticipantScreeningEpisode"
      app_service_plan_key   = "Default"
      app_urls = [
        {
          env_var_name     = "GetEpisodeUrl"
          function_app_key = "GetEpisode"
        },
        {
          env_var_name     = "CreateParticipantScreeningEpisodeUrl"
          function_app_key = "GetParticipantScreeningEpisodeData"
        },
        {
          env_var_name     = "GetScreeningDataUrl"
          function_app_key = "GetScreeningData"
        },
        {
          env_var_name     = "GetReferenceDataUrl"
          function_app_key = "GetOrganisationData"
        }
      ]
      ip_restrictions = {
        "AllowEventGrid" : {
          name        = "AllowEventGrid"
          priority    = 300
          action      = "Allow"
          service_tag = "AzureEventGrid"
        },
        "DenyAll" : {
          name        = "DenyAll"
          priority    = 2147483647
          action      = "Deny"
        }
      }
    }

    CreateParticipantScreeningProfile = {
      name_suffix            = "create-ps-profile"
      function_endpoint_name = "CreateParticipantScreeningProfile"
      app_service_plan_key   = "Default"
      app_urls = [
        {
          env_var_name     = "GetParticipantUrl"
          function_app_key = "GetParticipant"
        },
        {
          env_var_name     = "CreateParticipantScreeningProfileUrl"
          function_app_key = "CreateParticipantScreeningProfile"
        },
        {
          env_var_name     = "DemographicsServiceUrl"
          function_app_key = "GetDemographicsData"
        }
      ]
    }

    GetParticipantScreeningEpisodeData = {
      name_suffix            = "get-ps-episode-data"
      function_endpoint_name = "GetParticipantScreeningEpisodeData"
      app_service_plan_key   = "Default"
      db_connection_string   = "ServiceInsightsDbConnectionString"
    }

    GetDemographicsData = {
      name_suffix            = "get-demographics-data"
      function_endpoint_name = "GetDemographicsData"
      app_service_plan_key   = "Default"
    }

    CreateEpisode = {
      name_suffix               = "create-episode"
      function_endpoint_name    = "CreateEpisode"
      app_service_plan_key      = "Default"
      db_connection_string      = "ServiceInsightsDbConnectionString"
      event_grid_topic_producer = "event-grid-topic-1"
    }

    GetEpisode = {
      name_suffix            = "get-episode"
      function_endpoint_name = "GetEpisode"
      app_service_plan_key   = "Default"
      db_connection_string   = "ServiceInsightsDbConnectionString"
    }

    UpdateEpisode = {
      name_suffix               = "update-episode"
      function_endpoint_name    = "UpdateEpisode"
      app_service_plan_key      = "Default"
      db_connection_string      = "ServiceInsightsDbConnectionString"
      event_grid_topic_producer = "event-grid-topic-1"
    }

    ReceiveData = {
      name_suffix            = "receive-data"
      function_endpoint_name = "ReceiveData"
      app_service_plan_key   = "Default"
      app_urls = [
        {
          env_var_name     = "EpisodeManagementUrl"
          function_app_key = "CreateUpdateEpisode"
        },
        {
          env_var_name     = "ParticipantManagementUrl"
          function_app_key = "UpdateParticipant"
        }
      ]
    }

    CreateUpdateEpisode = {
      name_suffix            = "create-update-episode"
      function_endpoint_name = "CreateUpdateEpisode"
      app_service_plan_key   = "Default"
      app_urls = [
        {
          env_var_name     = "CreateEpisodeUrl"
          function_app_key = "CreateEpisode"
        },
        {
          env_var_name     = "GetEpisodeUrl"
          function_app_key = "GetEpisode"
        },
        {
          env_var_name     = "UpdateEpisodeUrl"
          function_app_key = "UpdateEpisode"
        }
      ]
    }

    GetEpisodeMgmt = {
      name_suffix            = "get-episode-mgmt"
      function_endpoint_name = "GetEpisodeMgmt"
      app_service_plan_key   = "Default"
      app_urls = [
        {
          env_var_name     = "GetEpisodeUrl"
          function_app_key = "GetEpisode"
        }
      ]
    }

    RetrieveMeshFile = {
      name_suffix            = "retrieve-mesh-file"
      function_endpoint_name = "RetrieveMeshFile"
      app_service_plan_key   = "Default"
      key_vault_url          = "KeyVaultConnectionString"
      env_vars_static = {
        TimerExpression  = "*/5 * * * *"
        BSSContainerName = "inbound"
      }
    }

    GetParticipant = {
      name_suffix            = "get-participant"
      function_endpoint_name = "GetParticipant"
      app_service_plan_key   = "Default"
    }

    UpdateParticipant = {
      name_suffix            = "update-participant"
      function_endpoint_name = "UpdateParticipant"
      app_service_plan_key   = "Default"
    }

    GetOrganisationData = {
      name_suffix            = "get-organisation-data"
      function_endpoint_name = "GetOrganisationData"
      app_service_plan_key   = "Default"
      db_connection_string   = "ServiceInsightsDbConnectionString"
    }

    GetScreeningData = {
      name_suffix            = "get-screening-data"
      function_endpoint_name = "GetScreeningData"
      app_service_plan_key   = "Default"
      db_connection_string   = "ServiceInsightsDbConnectionString"
    }
  }

}

function_app_slots = []

key_vault = {
  disk_encryption   = true
  soft_del_ret_days = 7
  purge_prot        = false
  sku_name          = "standard"
}

sqlserver = {
  sql_uai_name                         = "dtos-service-insight-sql-adm"
  sql_admin_group_name                 = "sqlsvr_serins_dev_uks_admin"
  ad_auth_only                         = true
  auditing_policy_retention_in_days    = 30
  security_alert_policy_retention_days = 30

  server = {
    sqlversion                    = "12.0"
    tlsversion                    = 1.2
    azure_services_access_enabled = true
  }

  # serins database
  dbs = {
    serins = {
      db_name_suffix = "ServiceInsightsDB"
      collation      = "SQL_Latin1_General_CP1_CI_AS"
      licence_type   = "LicenseIncluded"
      max_gb         = 5
      read_scale     = false
      sku            = "S0"
    }
  }

  fw_rules = {}
}

storage_accounts = {
  fnapp = {
    name_suffix                   = "fnappstor"
    account_tier                  = "Standard"
    replication_type              = "LRS"
    public_network_access_enabled = false
    containers = {
      config = {
        container_name = "config"
      }
      inbound = {
        container_name = "inbound"
      }
      inbound-poison = {
        container_name = "inbound-poison"
      }
    }
  }

  eventgrid = {
    name_suffix                   = "eventgrid"
    account_tier                  = "Standard"
    replication_type              = "LRS"
    public_network_access_enabled = false
    containers = {
      config = {
        container_name = "deadletterqueue"
      }
    }
  }
}
