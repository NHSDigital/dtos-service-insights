variable "resource_group_name" {
  type        = string
  description = "The name of the resource group in which to create the Event Grid. Changing this forces a new resource to be created."
}

variable "location" {
  type        = string
  description = "The location/region where the Event Grid is created."
}

variable "topic_name" {
  description = "The name of the Event Grid topic."
  type        = string
}

# variable "dead_letter_storage_container_object" {
#   type = map(object({
#     container_id   = optional(string, "")
#     container_name = optional(string, "")
#   }))
#   description = "Storage container object to save blob dead letter destination data to"
#   default     = {}
# }

variable "inbound_ip_rules" {
  description = "inbound IP rules for the Event Grid topic. Each rule should be a map with keys: 'ip_mask' and 'action'."
  type = list(object({
    ip_mask = string
    action  = string
  }))
  default = []
}

variable "input_mapping_fields" {
  description = "Input mapping fields for the Event Grid subscription."
  type        = map(string)
  default = {
    subject    = "data.subject"
    id         = "data.id"
    event_time = "data.event_time"
  }
}

variable "identity_type" {
  description = "Type of identity for the Event Grid topic."
  type        = string
  default     = "SystemAssigned"
}

variable "log_analytics_workspace_id" {
  type        = string
  description = "id of the log analytics workspace to send resource logging to via diagnostic settings"
}

variable "dead_letter_storage_account_container_name" {
  description = "The name of storage account container for the Dead Letter queue."
  type        = string
}

variable "dead_letter_storage_account_name" {
  description = "The name of storage account for the Dead Letter queue."
  type        = string
}

variable "dead_letter_storage_account_id" {
  description = "The name of storage account container id for the Dead Letter queue."
  type        = string
}

variable "subscription_name" {
  description = "The name of the Event Grid event subscription."
  type        = string
}

variable "function_app_id" {
  description = "The function app id that the event subscription will send events to."
  type        = string
}

variable "public_network_access_enabled" {
  type        = bool
  description = "Controls whether data in the account may be accessed from public networks."
  default     = false
}

variable "tags" {
  description = "A mapping of tags to assign to the Event Grid topic."
  type        = map(string)
  default     = {}
}

# variable "private_endpoint_properties" {
#   description = "Consolidated properties for the Function App Private Endpoint."
#   type = object({
#     private_dns_zone_ids_sql             = optional(list(string), [])
#     private_endpoint_enabled             = optional(bool, false)
#     private_endpoint_subnet_id           = optional(string, "")
#     private_endpoint_resource_group_name = optional(string, "")
#     private_service_connection_is_manual = optional(bool, false)
#   })
# }
