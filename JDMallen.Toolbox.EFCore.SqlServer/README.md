# JDMallen.Toolbox.EFCore.SqlServer

SQL Server provider package for JDMallen.Toolbox.EFCore.

## Purpose

This package exists solely to bring in the `Microsoft.EntityFrameworkCore.SqlServer` dependency for projects that need SQL Server support. By separating the provider into its own package, consumers can choose alternative database providers like PostgreSQL, SQLite, or Cosmos DB without being forced to reference SQL Server.

## Usage

Install this package when you're using SQL Server:

```bash
dotnet add package JDMallen.Toolbox.EFCore.SqlServer
```

For other databases, use the appropriate provider package:

- **PostgreSQL**: `Npgsql.EntityFrameworkCore.PostgreSQL`
- **SQLite**: `Microsoft.EntityFrameworkCore.Sqlite`
- **MySQL**: `Pomelo.EntityFrameworkCore.MySql`
- **Cosmos DB**: `Microsoft.EntityFrameworkCore.Cosmos`

All core EF functionality from `JDMallen.Toolbox.EFCore` works with any provider.
