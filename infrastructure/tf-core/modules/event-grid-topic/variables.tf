variable "resource_group_name" {
  type        = string
  description = "The name of the resource group in which to create the Event Grid. Changing this forces a new resource to be created."
}

variable "location" {
  type        = string
  description = "The location/region where the Event Grid is created."
}

variable "inbound_ip_rules" {
  type = list(object({
    function_endpoint = string
  }))
  default = []
  description = "The list of IP address to allow."
}

variable "identity_type" {
  type        = string
  description = "The identity type of the Event Grid."
}

variable "topic_name" {
  description = "The name of the Event Grid topic."
  type        = string
}

variable "log_analytics_workspace_id" {
  type        = string
  description = "id of the log analytics workspace to send resource logging to via diagnostic settings"
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
