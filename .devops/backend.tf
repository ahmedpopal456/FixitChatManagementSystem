terraform {
  backend "azurerm" {
    storage_account_name = "__terraformstorageaccount__"
    container_name       = "tfstate"
    key                  = "fixit-chms.terraform.tfstate"
    access_key           = "__storagekey__"
  }

  required_version = "=0.14.5"

  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm",
      version = "2.45.1"
    }
  }
}
