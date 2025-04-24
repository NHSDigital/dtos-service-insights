resource "azurerm_servicebus_namespace" "this" {
  name                = var.service_bus_namespace_name
  location            = var.location
  resource_group_name = var.resource_group_name
  sku                 = var.sku_tier
  capacity            = var.capacity

  tags = var.tags
}