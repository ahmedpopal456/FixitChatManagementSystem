variable "subscription_name" {
  type = string
}

variable "subscription_id" {
  type = string
}

variable "environment_name" {
  type = string
}

variable "location_name" {
  type = string
}

variable "location_name_linux" {
  type = string
}

variable "servicebusnamespace_sku" {
  type    = string
  default = "Standard"
}

variable "azurefunctions_sku_tier" {
  type    = string
  default = "Dynamic"
}

variable "azurefunctions_sku_size" {
  type    = string
  default = "Y1"
}

variable "azuresignalr_tier" {
  type    = string
  default = "Free_F1"
}

variable "azuresignalr_size" {
  type    = string
  default = "1"
}

variable "storage_account_tier" {
  type    = string
  default = "Standard"
}

variable "storage_account_replication" {
  type    = string
  default = "LRS"
}

variable "organization_name" {
  type    = string
  default = "fixit"
}

variable "tenant_id" {
  type    = string
  default = "ccc68497-f4c0-4c2c-b499-78c30c54b52c"
}

variable "service_abbreviation" {
  type    = string
  default = "chms"
}

variable "function_apps" {
  type = list(string)
  default = [
    "api",
    "trigger"
  ]
}

variable "cosmosdb_tables" {
  type = map(string)
  default = {
    Conversations = "FixConversations"
  }
}

variable "service_bus_queue_names" {
  type = list(string)
  default = [
    "joingroupqueue",
    "sendmessagetogroupqueue",
    "onconversationactiondispatcher"
  ]
}

variable "queue_names" {
  type = list(string)
  default = [
    "createconversationsqueue",
  ]
}