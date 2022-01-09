data "azurerm_cosmosdb_account" "main" {
  name                = "${var.organization_name}-${var.environment_name}-common-cosmosdb"
  resource_group_name = "${var.organization_name}-${var.environment_name}-common"
}

data "azurerm_application_insights" "main" {
  name                = "${var.organization_name}-${var.environment_name}-common-ai"
  resource_group_name = "${var.organization_name}-${var.environment_name}-common"
}