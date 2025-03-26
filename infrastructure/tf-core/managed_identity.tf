resource "azurerm_user_assigned_identity" "mi" {
  for_each = var.features.smoke_test_managed_identity_created ? var.regions : {}

  name                = "${module.regions_config[each.key].names.managed-identity}-smoke-tests"
  resource_group_name = azurerm_resource_group.core[each.key].name
  location            = each.key

  tags = var.tags
}

module "rbac_assignments" {
  for_each = var.features.smoke_test_managed_identity_created ? local.rbac_map : {}

  source = "../../../dtos-devops-templates/infrastructure/modules/rbac-assignment"

  principal_id         = azurerm_user_assigned_identity.mi[each.value.region].principal_id
  role_definition_name = each.value.rbac_role
  scope                = module.storage["fnapp-${each.value.region}"].storage_account_id
}


locals {
  rbac_flatlist = flatten([
    for region in keys(var.regions) : [
      for rbac_role in local.rbac_roles_storage : merge(
        {
          region    = region
          rbac_role = rbac_role
        },
      )
    ]
  ])

  # ...then project the list of objects into a map with unique keys (combining the iterators), for consumption by a for_each meta argument
  rbac_map = {
    for object in local.rbac_flatlist : "${object.rbac_role}-${object.region}" => object
  }

}
