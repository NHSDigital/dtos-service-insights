locals {

  wip_rbac_roles_storage = [ # this is needed to use the updated storage module - will refactor the other vars here later when function_app.tf is revised
    "Storage Account Contributor",
    "Storage Blob Data Owner",
    "Storage Queue Data Contributor"
  ]

  rbac_roles_key_vault_officers = [
    "Key Vault Certificate Officer",
    "Key Vault Crypto Officer",
    "Key Vault Secrets Officer"
  ]

  rbac_roles_key_vault = [
    "Key Vault Certificate User",
    "Key Vault Crypto User",
    "Key Vault Secrets User"
  ]

  rbac_roles_storage = [
    "Storage Account Contributor",
    "Storage Blob Data Owner",
    "Storage Queue Data Contributor"
  ]

  rbac_roles_database = [
    "Contributor"
  ]
}
