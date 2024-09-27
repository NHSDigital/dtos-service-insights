USE master;
GO

IF NOT EXISTS (
        SELECT name
        FROM sys.databases
        WHERE name = N'ServiceInsightsDB'
        )
    CREATE DATABASE [ServiceInsightsDB];
GO

IF SERVERPROPERTY('ProductVersion') > '12'
    ALTER DATABASE [ServiceInsightsDB] SET QUERY_STORE = ON;
GO
