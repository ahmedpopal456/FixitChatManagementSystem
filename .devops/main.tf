# deploy resource group 
resource "azurerm_resource_group" "main" {
  name     = "${var.organization_name}-${var.environment_name}-${var.service_abbreviation}"
  location = var.location_name
}

# deploy main storage account
resource "azurerm_storage_account" "main" {
  name                     = "${var.organization_name}${var.environment_name}${var.service_abbreviation}"
  resource_group_name      = azurerm_resource_group.main.name
  location                 = azurerm_resource_group.main.location
  account_tier             = var.storage_account_tier
  account_replication_type = var.storage_account_replication
}

resource "azurerm_storage_queue" "main" {
  for_each             = toset(var.queue_names)
  name                 = each.key
  storage_account_name = azurerm_storage_account.main.name
}

# deploy function apps storage accounts
resource "azurerm_storage_account" "apps" {
  for_each                 = toset(var.function_apps)
  name                     = "${var.organization_name}${var.environment_name}${var.service_abbreviation}${each.key}"
  resource_group_name      = azurerm_resource_group.main.name
  location                 = azurerm_resource_group.main.location
  account_tier             = var.storage_account_tier
  account_replication_type = var.storage_account_replication
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

# deploy service bus namespace
resource "azurerm_servicebus_namespace" "main" {
  name                = "${var.organization_name}-${var.environment_name}-${var.service_abbreviation}-servicebus"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  sku                 = var.servicebusnamespace_sku
}

# deploy service bus namespace authorization rules
resource "azurerm_servicebus_namespace_authorization_rule" "main" {
  name                = "${var.organization_name}-${var.environment_name}-${var.service_abbreviation}-sb-authrule"
  namespace_name      = azurerm_servicebus_namespace.main.name
  resource_group_name = azurerm_resource_group.main.name

  listen = true
  manage = true
  send   = true
}

# deploy service bus queues
resource "azurerm_servicebus_queue" "servicebusqueuenames" {
  for_each            = toset(var.service_bus_queue_names)
  name                = "${each.key}"
  resource_group_name = azurerm_resource_group.main.name
  namespace_name      = azurerm_servicebus_namespace.main.name

  lock_duration                         = "PT30S"
  requires_session                      = true
  default_message_ttl                   = "P14D"
  dead_lettering_on_message_expiration  = true
  max_delivery_count                    = 1000
  auto_delete_on_idle                   = "P10675199DT2H48M5.4775807S"
}

# deploy signal r service
resource "azurerm_signalr_service" "main" {
  name                = "${var.organization_name}-${var.environment_name}-${var.service_abbreviation}-signalr"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name

  sku {
    name     = var.azuresignalr_tier
    capacity = var.azuresignalr_size
  }
  cors {
    allowed_origins = ["*"]
  }
  features {
    flag  = "ServiceMode"
    value = "Serverless"
  }
}

# deploy azure functions
resource "azurerm_app_service_plan" "apps" {
  for_each            = toset(var.function_apps)
  name                = "${var.organization_name}-${var.environment_name}-${var.service_abbreviation}-func-${each.key}-sp"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  kind                = "FunctionApp"

  sku {
    tier = var.azurefunctions_sku_tier
    size = var.azurefunctions_sku_size
  }
}

resource "azurerm_function_app" "functionapps" {
  for_each                   = toset(var.function_apps)
  name                       = "${var.organization_name}-${var.environment_name}-${var.service_abbreviation}-func-${each.key}"
  location                   = azurerm_resource_group.main.location
  resource_group_name        = azurerm_resource_group.main.name
  app_service_plan_id        = azurerm_app_service_plan.apps[tostring(each.key)].id
  storage_account_name       = azurerm_storage_account.apps[each.key].name
  storage_account_access_key = azurerm_storage_account.apps[each.key].primary_access_key
  version                    = "~3"

  site_config {
    scm_type = "VSTSRM"
  }

  app_settings = {
    "APPINSIGHTS_INSTRUMENTATIONKEY"                     = data.azurerm_application_insights.main.instrumentation_key,
    "AzureSignalRConnectionString"                       = azurerm_signalr_service.main.primary_connection_string,
    "AzureSignalRServiceTransportType"                   = "Transient",
    "AzureWebJobsStorage"                                = azurerm_storage_account.apps[each.key].primary_connection_string,
    "FIXIT-CHMS-DB-CS"                                   = "AccountEndpoint=https://${data.azurerm_cosmosdb_account.main.name}.documents.azure.com:443/;AccountKey=${data.azurerm_cosmosdb_account.main.primary_key};",
    "FIXIT-CHMS-DB-KEY"                                  = data.azurerm_cosmosdb_account.main.primary_key,
    "FIXIT-CHMS-DB-NAME"                                 = azurerm_cosmosdb_sql_database.main.name,
    "FIXIT-CHMS-DB-URI"                                  = "https://${data.azurerm_cosmosdb_account.main.name}.documents.azure.com:443/",
    "FIXIT-CHMS-JOINGROUP-QUEUE-NAME"                    = var.service_bus_queue_names[0],
    "FIXIT-CHMS-SA-AK"                                   = azurerm_storage_account.main.primary_access_key,
    "FIXIT-CHMS-SA-AN"                                   = azurerm_storage_account.main.name,
    "FIXIT-CHMS-STORAGEACCOUNT-CS"                       = azurerm_storage_account.main.primary_connection_string,
    "FIXIT-CHMS-SA-EP"                                   = "core.windows.net",
    "FIXIT-CHMS-SB-CS"                                   = azurerm_servicebus_namespace.main.default_primary_connection_string,
    "FIXIT-CHMS-SENDMESSAGETOGROUP-QUEUE-NAME"           = var.service_bus_queue_names[1],
    "FIXIT-CHMS-TABLE-CONVERSATIONS-NAME"                = var.cosmosdb_tables["Conversations"],
    "FUNCTIONS_WORKER_RUNTIME"                           = "dotnet",
    "WEBSITE_RUN_FROM_PACKAGE"                           = "1",
    "WEBSITE_ENABLE_SYNC_UPDATE_SITE"                    = "true",
    "WEBSITE_NODE_DEFAULT_VERSION"                       = "10.14.1",
    "EMP-CHMS-ONCONVERSATIONACTIONDISPATCHER-QUEUE-NAME" = var.service_bus_queue_names[2],
  }
}
