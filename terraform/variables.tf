variable "resource_group_name" {
  description = "The name of the resource group"
  type        = string
  default     = "rg-exchangepro-prod"
}

variable "location" {
  description = "The Azure Region to deploy resources"
  type        = string
  default     = "East US"
}

variable "db_password" {
  description = "The administrator password for the MySQL server."
  type        = string
  sensitive   = true
}

variable "api_key" {
  description = "The API key for exchangerate.host"
  type        = string
  sensitive   = true
}

