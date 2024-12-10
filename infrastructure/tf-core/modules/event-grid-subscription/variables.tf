variable "subscription_name" {
  description = "The name of the Event Grid event subscription."
  type        = string
}

variable "resource_group_name" {
  type        = string
  description = "The name of the resource group in which to create the Event Grid. Changing this forces a new resource to be created."
}

variable "location" {
  type        = string
  description = "The location/region where the Event Grid is created."
}

variable "subscriber_function_endpoints" {
  type = list(object({
    function_endpoint = string
  }))
  default = []
}

variable "azurerm_eventgrid_id" {
  description = "The azurerm Event Grid id to link to."
  type        = string
}

variable "tags" {
  description = "A mapping of tags to assign to the Event Grid topic."
  type        = map(string)
  default     = {}
}

variable "dead_letter_storage_account_container_name" {
  description = "The name of storage account container for the Dead Letter queue."
  type        = string
}


variable "log_analytics_workspace_id" {
  type        = string
  description = "id of the log analytics workspace to send resource logging to via diagnostic settings"
}

variable "dead_letter_storage_account_name" {
  description = "The name of storage account for the Dead Letter queue."
  type        = string
}

variable "dead_letter_storage_account_id" {
  description = "The name of storage account container id for the Dead Letter queue."
  type        = string
}
