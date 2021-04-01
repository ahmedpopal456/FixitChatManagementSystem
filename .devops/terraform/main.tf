resource "azurerm_resource_group" "main" {
  name     = "${var.organization_name}-${var.environment_name}-${var.service_abbreviation}"
  location = var.location_name
}

resource "azurerm_storage_account" "main" {
  name                     = "${var.organization_name}${var.environment_name}${var.service_abbreviation}"
  resource_group_name      = azurerm_resource_group.main.name
  location                 = azurerm_resource_group.main.location
  account_tier             = "Standard"
  account_replication_type = "LRS"
}

resource "azurerm_storage_account" "app" {
  for_each                 = toset(var.function_apps)
  name                     = "${var.organization_name}${var.environment_name}${var.service_abbreviation}${each.key}"
  resource_group_name      = azurerm_resource_group.main.name
  location                 = azurerm_resource_group.main.location
  account_tier             = "Standard"
  account_replication_type = "LRS"
}

resource "azurerm_storage_queue" "main" {
  for_each             = toset(var.queue_names)
  name                 = each.key
  storage_account_name = azurerm_storage_account.main.name
}

resource "azurerm_app_service_plan" "main" {
  name                = "${var.organization_name}-${var.environment_name}-${var.service_abbreviation}-service-plan"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  kind                = "FunctionApp"

  sku {
    tier = "Dynamic"
    size = "Y1"
  }
}

resource "azurerm_function_app" "main" {
  for_each                   = toset(var.function_apps)
  name                       = "${var.organization_name}-${var.environment_name}-${var.service_abbreviation}-${each.key}"
  location                   = azurerm_resource_group.main.location
  resource_group_name        = azurerm_resource_group.main.name
  app_service_plan_id        = azurerm_app_service_plan.main.id
  storage_account_name       = azurerm_storage_account.app[each.key].name
  storage_account_access_key = azurerm_storage_account.app[each.key].primary_access_key
  version                    = "~3"

  site_config {
    scm_type = "VSTSRM"
  }

  app_settings = {
    "AzureWebJobsStorage"          = azurerm_storage_account.main.primary_connection_string,
    "AzureSignalRConnectionString" = data.azurerm_signalr_service.main.primary_connection_string,

    "WEBSITE_ENABLE_SYNC_UPDATE_SITE" = "true",
    "WEBSITE_RUN_FROM_PACKAGE"        = "1",
    "APPINSIGHTS_INSTRUMENTATIONKEY"  = data.azurerm_application_insights.main.instrumentation_key,
    "WEBSITE_NODE_DEFAULT_VERSION"    = "10.14.1"
    "FUNCTIONS_WORKER_RUNTIME"        = "dotnet",

    "FIXIT-CMS-DB-EP"                 = data.azurerm_cosmosdb_account.main.endpoint,
    "FIXIT-CMS-DB-KEY"                = data.azurerm_cosmosdb_account.main.primary_key,
    "FIXIT-CMS-DB-NAME"               = azurerm_cosmosdb_sql_database.main.name,
    "FIXIT-CMS-DB-CONVERSATIONSTABLE" = azurerm_cosmosdb_sql_container.main["conversations"].name,
    "FIXIT-CMS-DB-MESSAGESTABLE"      = azurerm_cosmosdb_sql_container.main["messages"].name,

    "FIXIT-CMS-STORAGEACCOUNT-CS"       = azurerm_storage_account.main.primary_connection_string,
    "FIXIT-CMS-CONVERSATIONSQUEUE-NAME" = "createconversationsqueue",
    "FIXIT-CMS-MESSAGESQUEUE-NAME"      = "handlesendtouserqueue"
  }
}

resource "azurerm_cosmosdb_sql_database" "main" {
  name                = var.organization_name
  resource_group_name = data.azurerm_cosmosdb_account.main.resource_group_name
  account_name        = data.azurerm_cosmosdb_account.main.name
  throughput          = 400
}

resource "azurerm_cosmosdb_sql_container" "main" {
  for_each            = var.cosmosdb_tables
  name                = each.value
  resource_group_name = data.azurerm_cosmosdb_account.main.resource_group_name
  account_name        = data.azurerm_cosmosdb_account.main.name
  database_name       = azurerm_cosmosdb_sql_database.main.name
  partition_key_path  = "/EntityId"

  indexing_policy {
    indexing_mode = "Consistent"

    included_path {
      path = "/*"
    }

    excluded_path {
      path = "/\"_etag\"/?"
    }
  }
}
