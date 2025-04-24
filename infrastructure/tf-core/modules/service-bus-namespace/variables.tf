variable "resource_group_name" {
  type        = string
  description = "The name of the resource group in which to create the Event Grid. Changing this forces a new resource to be created."
}

variable "location" {
  description = "The location/region where the Service Bus namespace will be created."
  type        = string
  default     = "uksouth"
  validation {
    condition     = contains(["uksouth", "ukwest"], var.location)
    error_message = "The location must be either uksouth or ukwest."
  }
}

variable "service_bus_namespace_name" {
  description = "The name of the Service Bus namespace."
  type        = string
}

variable "sku_tier" {
  description = "The tier of the SKU."
  type        = string
  default     = "Standard"
  validation {
    condition     = contains(["Basic", "Standard", "Premium"], var.sku_tier)
    error_message = "The SKU name must be either Basic, Standard or Premium."
  }
}

variable "tags" {
  description = "A mapping of tags to assign to the resource."
  type        = map(string)
  default     = {}
}

variable "capacity" {
  description = "When sku is Premium, capacity can be 1, 2, 4, 8 or 16. When sku is Basic or Standard, capacity must be 0."
  type        = number
  default     = 1
  validation {
    condition = (
      (var.sku_tier == "Premium" && contains([1, 2, 4, 8, 16], var.capacity)) ||
      ((var.sku_tier == "Basic" || var.sku_tier == "Standard") && var.capacity == 0)
    )
    error_message = "Invalid capacity: Premium allows 1, 2, 4, 8 or 16. Basic and Standard must have capacity 0."
  }
}