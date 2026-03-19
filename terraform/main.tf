# Terraform configuration for Azure deployment
# It provisions MySQL, Redis, and a Dockerized App Service to host the unified Blazor Web App.

resource "azurerm_resource_group" "main" {
  name     = var.resource_group_name
  location = var.location
}

# Azure Database for MySQL Flexible Server
resource "azurerm_mysql_flexible_server" "db" {
  name                   = "mysql-exchangepro"
  resource_group_name    = azurerm_resource_group.main.name
  location               = azurerm_resource_group.main.location
  administrator_login    = "myadmin"
  administrator_password = var.db_password
  sku_name               = "B_Standard_B1s"
  version                = "8.0.21"
}

resource "azurerm_mysql_flexible_database" "appdb" {
  name                = "ExchangePro"
  resource_group_name = azurerm_resource_group.main.name
  server_name         = azurerm_mysql_flexible_server.db.name
  charset             = "utf8mb4"
  collation           = "utf8mb4_unicode_ci"
}

# Azure Cache for Redis
resource "azurerm_redis_cache" "cache" {
  name                = "redis-exchangepro"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  capacity            = 0
  family              = "C"
  sku_name            = "Basic"
}

# App Service Plan
resource "azurerm_service_plan" "appserviceplan" {
  name                = "asp-exchangepro"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  os_type             = "Linux"
  sku_name            = "B1" # Update to a higher SKU if needed
}

# Linux Web App for Containers (Hosting the Unified Dockerized App)
resource "azurerm_linux_web_app" "app" {
  name                = "app-exchangepro-dashboard"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  service_plan_id     = azurerm_service_plan.appserviceplan.id

  site_config {
    application_stack {
      docker_image_name = "exchangepro/dashboard:latest" # Replace with your ACR/Docker Hub image
    }
    always_on = true
  }

  app_settings = {
    "WEBSITES_PORT"                        = "8080"
    "ASPNETCORE_ENVIRONMENT"               = "Production"
    "ConnectionStrings__DefaultConnection" = "Server=${azurerm_mysql_flexible_server.db.fqdn};Database=${azurerm_mysql_flexible_database.appdb.name};User=${azurerm_mysql_flexible_server.db.administrator_login};Password=${azurerm_mysql_flexible_server.db.administrator_password};Port=3306;SslMode=Required;"
    "ConnectionStrings__Redis"             = "${azurerm_redis_cache.cache.hostname}:6380,password=${azurerm_redis_cache.cache.primary_access_key},ssl=True,abortConnect=False"
    "ApiKey"                              = var.api_key
    "PollingIntervalMinutes"               = "60"
  }
}

output "app_url" {
  value = "https://${azurerm_linux_web_app.app.default_hostname}"
}

