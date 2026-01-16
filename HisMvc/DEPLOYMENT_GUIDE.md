# HIS MVC - Deployment Guide

## ?? M?c L?c
1. [Yêu C?u H? Th?ng](#yêu-c?u-h?-th?ng)
2. [Chu?n B? Môi Tr??ng](#chu?n-b?-môi-tr??ng)
3. [C?u Hình Database](#c?u-hình-database)
4. [Deploy Lên IIS](#deploy-lên-iis)
5. [Deploy Lên Azure](#deploy-lên-azure)
6. [Deploy B?ng Docker](#deploy-b?ng-docker)
7. [Monitoring & Maintenance](#monitoring--maintenance)

---

## ??? Yêu C?u H? Th?ng

### Minimum Requirements
- **OS**: Windows Server 2019+ / Windows 10+ / Linux (Ubuntu 20.04+)
- **CPU**: 2 cores
- **RAM**: 4 GB
- **Disk**: 20 GB SSD
- **Database**: SQL Server 2019+ ho?c SQL Server Express

### Recommended Requirements
- **OS**: Windows Server 2022 / Ubuntu 22.04
- **CPU**: 4 cores
- **RAM**: 8 GB
- **Disk**: 50 GB SSD
- **Database**: SQL Server 2022

### Software Requirements
- **.NET 10 SDK** (cho build)
- **.NET 10 Runtime** (cho production)
- **SQL Server** (2019+)
- **IIS 10+** (Windows) ho?c **Nginx** (Linux)

---

## ?? Chu?n B? Môi Tr??ng

### B??c 1: Cài ??t .NET 10

**Windows:**
```powershell
# Download t?: https://dotnet.microsoft.com/download/dotnet/10.0
# Ho?c dùng winget
winget install Microsoft.DotNet.SDK.10

# Verify
dotnet --version
```

**Linux (Ubuntu):**
```bash
wget https://dot.net/v1/dotnet-install.sh
chmod +x dotnet-install.sh
./dotnet-install.sh --channel 10.0

# Verify
dotnet --version
```

### B??c 2: Cài ??t SQL Server

**Windows:**
```powershell
# Download SQL Server Express t?:
# https://www.microsoft.com/en-us/sql-server/sql-server-downloads

# Ho?c SQL Server Developer Edition (mi?n phí)
```

**Linux:**
```bash
# SQL Server 2022 on Ubuntu
sudo apt-get update
sudo apt-get install -y mssql-server

sudo /opt/mssql/bin/mssql-conf setup

# Verify
systemctl status mssql-server
```

---

## ?? C?u Hình Database

### B??c 1: T?o Database

```sql
-- Connect to SQL Server using SSMS or Azure Data Studio

CREATE DATABASE HIS_MVC_DB;
GO

USE HIS_MVC_DB;
GO
```

### B??c 2: T?o User (Production)

```sql
-- Create login and user
CREATE LOGIN his_app_user WITH PASSWORD = 'YourStrongPassword123!';
GO

USE HIS_MVC_DB;
GO

CREATE USER his_app_user FOR LOGIN his_app_user;
GO

-- Grant permissions
ALTER ROLE db_datareader ADD MEMBER his_app_user;
ALTER ROLE db_datawriter ADD MEMBER his_app_user;
ALTER ROLE db_ddladmin ADD MEMBER his_app_user; -- For migrations
GO
```

### B??c 3: Update Connection String

**appsettings.Production.json:**
```json
{
  "ConnectionStrings": {
    "Default": "Server=YOUR_SERVER;Database=HIS_MVC_DB;User Id=his_app_user;Password=YourStrongPassword123!;TrustServerCertificate=True;Encrypt=True"
  }
}
```

### B??c 4: Apply Migrations

```bash
# Development
dotnet ef database update

# Production (t? published folder)
cd /path/to/published/app
dotnet HisMvc.dll --apply-migrations
```

---

## ?? Deploy Lên IIS

### B??c 1: Cài ??t IIS và ASP.NET Core Hosting Bundle

```powershell
# Enable IIS
Enable-WindowsOptionalFeature -Online -FeatureName IIS-WebServerRole
Enable-WindowsOptionalFeature -Online -FeatureName IIS-WebServer
Enable-WindowsOptionalFeature -Online -FeatureName IIS-ApplicationDevelopment
Enable-WindowsOptionalFeature -Online -FeatureName IIS-ASPNET45

# Download ASP.NET Core Hosting Bundle
# https://dotnet.microsoft.com/permalink/dotnetcore-current-windows-runtime-bundle-installer
```

### B??c 2: Publish Application

```bash
# T? project directory
dotnet publish -c Release -o ./publish

# Ho?c v?i runtime c? th?
dotnet publish -c Release -r win-x64 --self-contained false -o ./publish
```

### B??c 3: T?o Application Pool

```powershell
# Import IIS module
Import-Module WebAdministration

# Create App Pool
New-WebAppPool -Name "HisMvcAppPool"
Set-ItemProperty IIS:\AppPools\HisMvcAppPool -name "managedRuntimeVersion" -value ""

# Configure App Pool
Set-ItemProperty IIS:\AppPools\HisMvcAppPool -name processModel -value @{identitytype="ApplicationPoolIdentity"}
```

### B??c 4: T?o Website

```powershell
# Create website
New-Website -Name "HisMvc" -Port 80 -PhysicalPath "C:\inetpub\wwwroot\HisMvc" -ApplicationPool "HisMvcAppPool"

# Ho?c dùng IIS Manager
# 1. M? IIS Manager
# 2. Right-click Sites ? Add Website
# 3. Site name: HisMvc
# 4. Physical path: C:\inetpub\wwwroot\HisMvc
# 5. Binding: http, port 80
# 6. Application pool: HisMvcAppPool
```

### B??c 5: Copy Files

```powershell
# Copy published files
Copy-Item -Path "./publish/*" -Destination "C:\inetpub\wwwroot\HisMvc" -Recurse -Force

# Set permissions
icacls "C:\inetpub\wwwroot\HisMvc" /grant "IIS AppPool\HisMvcAppPool:(OI)(CI)F" /T
```

### B??c 6: Configure HTTPS (Optional)

```powershell
# Generate self-signed certificate (development)
New-SelfSignedCertificate -DnsName "localhost" -CertStoreLocation "cert:\LocalMachine\My"

# Add HTTPS binding
New-WebBinding -Name "HisMvc" -Protocol "https" -Port 443 -IPAddress "*" -SslFlags 0
```

---

## ?? Deploy Lên Azure

### Option 1: Azure App Service

```bash
# Login to Azure
az login

# Create resource group
az group create --name his-mvc-rg --location southeastasia

# Create App Service Plan
az appservice plan create --name his-mvc-plan --resource-group his-mvc-rg --sku B1 --is-linux

# Create Web App
az webapp create --resource-group his-mvc-rg --plan his-mvc-plan --name his-mvc-app --runtime "DOTNETCORE:10.0"

# Deploy
az webapp deployment source config-zip --resource-group his-mvc-rg --name his-mvc-app --src ./publish.zip
```

### Option 2: Azure SQL Database

```bash
# Create SQL Server
az sql server create --name his-mvc-sql --resource-group his-mvc-rg --location southeastasia --admin-user sqladmin --admin-password YourPassword123!

# Create Database
az sql db create --resource-group his-mvc-rg --server his-mvc-sql --name HIS_MVC_DB --service-objective S0

# Configure firewall
az sql server firewall-rule create --resource-group his-mvc-rg --server his-mvc-sql --name AllowAzureServices --start-ip-address 0.0.0.0 --end-ip-address 0.0.0.0
```

### Update Connection String

```bash
# Set connection string in App Service
az webapp config connection-string set --resource-group his-mvc-rg --name his-mvc-app --connection-string-type SQLAzure --settings Default="Server=tcp:his-mvc-sql.database.windows.net,1433;Database=HIS_MVC_DB;User ID=sqladmin;Password=YourPassword123!;Encrypt=True;TrustServerCertificate=False;"
```

---

## ?? Deploy B?ng Docker

### Dockerfile

```dockerfile
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["HisMvc/HisMvc.csproj", "HisMvc/"]
RUN dotnet restore "HisMvc/HisMvc.csproj"
COPY . .
WORKDIR "/src/HisMvc"
RUN dotnet build "HisMvc.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "HisMvc.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HisMvc.dll"]
```

### docker-compose.yml

```yaml
version: '3.8'

services:
  web:
    build: .
    ports:
      - "8080:80"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__Default=Server=sql;Database=HIS_MVC_DB;User Id=sa;Password=YourPassword123!;TrustServerCertificate=True
    depends_on:
      - sql
    restart: unless-stopped

  sql:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=YourPassword123!
    ports:
      - "1433:1433"
    volumes:
      - sqldata:/var/opt/mssql
    restart: unless-stopped

volumes:
  sqldata:
```

### Deploy Commands

```bash
# Build và run
docker-compose up -d

# View logs
docker-compose logs -f

# Stop
docker-compose down
```

---

## ?? Monitoring & Maintenance

### Health Check Endpoint

Thêm vào `Program.cs`:

```csharp
app.MapHealthChecks("/health");
```

### Logging

**appsettings.Production.json:**
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Warning"
    },
    "File": {
      "Path": "logs/hismvc-.log",
      "RollingInterval": "Day"
    }
  }
}
```

### Backup Database

```sql
-- Full backup
BACKUP DATABASE HIS_MVC_DB
TO DISK = 'C:\Backups\HIS_MVC_DB_Full.bak'
WITH FORMAT, INIT, NAME = 'Full Backup of HIS_MVC_DB';

-- Differential backup
BACKUP DATABASE HIS_MVC_DB
TO DISK = 'C:\Backups\HIS_MVC_DB_Diff.bak'
WITH DIFFERENTIAL, FORMAT, INIT, NAME = 'Differential Backup of HIS_MVC_DB';
```

### Automated Backup Script (PowerShell)

```powershell
# backup-database.ps1
$server = "localhost"
$database = "HIS_MVC_DB"
$backupPath = "C:\Backups"
$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
$backupFile = "$backupPath\HIS_MVC_DB_$timestamp.bak"

$query = @"
BACKUP DATABASE [$database]
TO DISK = '$backupFile'
WITH FORMAT, INIT, COMPRESSION
"@

Invoke-Sqlcmd -ServerInstance $server -Query $query
Write-Host "Backup completed: $backupFile"
```

### Windows Task Scheduler (Automated Backup)

```powershell
# Create scheduled task
$action = New-ScheduledTaskAction -Execute 'PowerShell.exe' -Argument '-File C:\Scripts\backup-database.ps1'
$trigger = New-ScheduledTaskTrigger -Daily -At 2am
Register-ScheduledTask -Action $action -Trigger $trigger -TaskName "HIS MVC DB Backup" -Description "Daily backup of HIS MVC database"
```

---

## ?? Security Checklist

- [ ] Change default passwords
- [ ] Use HTTPS/TLS
- [ ] Enable firewall
- [ ] Restrict database access
- [ ] Regular security updates
- [ ] Implement rate limiting
- [ ] Enable audit logging
- [ ] Use strong password policy
- [ ] Regular backup & disaster recovery plan
- [ ] Monitor suspicious activities

---

## ?? Support

- **Email**: support@his-mvc.local
- **Documentation**: [README.md](README.md)
- **API Docs**: [API_COMPLETE_DOCUMENTATION.md](API_COMPLETE_DOCUMENTATION.md)

---

**Version:** 1.0.0  
**Last Updated:** January 2026
