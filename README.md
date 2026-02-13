# Pagination POC - Offset vs Cursor Strategy Benchmarking

A comprehensive proof-of-concept demonstrating two pagination strategies with performance benchmarking in a .NET 10.0 ASP.NET Core API. This project compares **Offset-based pagination** and **Cursor-based pagination** to help you choose the right approach for your application.


## 📋 Table of Contents

- [Overview](#-overview)
- [Quick Start](#-quick-start)
- [Running the API](#-running-the-api)
- [Running the Benchmarks](#-running-the-benchmarks)
- [Configuration](#️-configuration)
- [Technologies & Dependencies](#️-technologies--dependencies)
- [Performance Insights](#-performance-insights)


## 🎯 Overview

This project showcases two distinct pagination approaches with their respective trade-offs:

| Aspect | Offset Pagination | Cursor Pagination |
|--------|-------------------|-------------------|
| **Use Case** | Traditional page browsing | Large datasets, infinite scroll |
| **Performance** | Degrades with deep pages | Consistent regardless of position |
| **Jump to Page** | ✅ Easy | ❌ Difficult |
| **Real-time Data** | ❌ Problematic | ✅ Safe |
| **UI Complexity** | Simple (page numbers) | Moderate (cursor tokens) |

The project includes:
- **100,000 seeded user records** for realistic benchmarking
- **Repository pattern** with separate implementations per strategy
- **Comprehensive benchmarks** measuring performance across 10 scenarios
- **OpenAPI/Scalar** documentation for API exploration


## 🚀 Quick Start

### Prerequisites

- **.NET 10.0 SDK** or later
- **SQL Server 2019+** (or SQL Server Express LocalDB)
- **Visual Studio 2022+** or VS Code with C# extensions

### Setup

1. **Clone and navigate to the project:**
   ```bash
   cd pagination
   ```

2. **Set up the SQL Server connection string:**
   
   Create an `appsettings.Development.json` file or use User Secrets:
   
   ```bash
   dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Database=PaginationDb;Trusted_Connection=true;"
   ```

   Or modify `appsettings.json` directly.

3. **Apply database migrations:**
   ```bash
   dotnet ef database update
   ```

4. **Run the API:**
   ```bash
   dotnet run
   ```
   
   API will be available at: `http://localhost:5152`


## 🌐 Running the API

### Development Mode

```bash
cd pagination
dotnet run
```

### Example API Calls

**Offset pagination - Get page 1:**
```bash
curl "http://localhost:5152/api/User/getusers?PaginationType=1&offsetPagination.Page=1&offsetPagination.PageSize=50"
```

**Response:**
```json
{
  "data": [...],
  "totalCount": 100000,
  "page": 1,
  "pageSize": 50,
  "totalPages": 2000,
  "hasNextPage": true,
  "hasPreviousPage": false
}
```

**Cursor pagination - Get first page:**
```bash
curl "http://localhost:5152/api/User/getusers?PaginationType=2&cursorPagination.Cursor=0&cursorPagination.IsQueryPreviousPage=false&cursorPagination.IsIncludeTotalCount=false&cursorPagination.PageSize=50"
```

**Response:**
```json
{
  "data": [...],
  "totalCount": 100000,
  "nextCursor": 50,
  "previousCursor": 0,
  "hasNextPage": true,
  "hasPreviousPage": true
}
```

**Docker:**
```bash
docker build -f pagination/Dockerfile -t pagination-api .
docker run -p 8080:8080 -p 8081:8081 pagination-api
```


## ⚡ Running the Benchmarks

The BenchmarkSuite uses **BenchmarkDotNet** to measure performance across 10 scenarios (5 for each pagination strategy).

### Run All Benchmarks

```bash
cd BenchmarkSuite
dotnet run -c Release --no-build
```

### Key Benchmark Scenarios

1. **Offset - First Page**: Retrieve page 1 (baseline)
2. **Offset - Last Page**: Retrieve last page (~page 2000)
3. **Offset - Next Page**: Retrieve page 2 (early pagination)
4. **Offset - Previous Page**: Retrieve page 1995 (deep pagination)
5. **Offset - Random Page**: Retrieve random page near end (stress test)

6. **Cursor - First Page**: Retrieve first page (cursor = 0)
7. **Cursor - Last Page**: Retrieve last page (cursor near end)
8. **Cursor - Next Page**: Retrieve next page
9. **Cursor - Previous Page**: Retrieve previous page
10. **Cursor - Random Page**: Retrieve random page (stress test)

### Benchmark Output

Benchmarks generate:
- **Console output** with detailed metrics
- **Markdown report** in `BenchmarkDotNet.Artifacts/`
- Memory diagnostics and allocation statistics
- Min/Max/Mean execution times


## ⚙️ Configuration

### Connection String

Update `appsettings.json` or use User Secrets:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=PaginationDb;Trusted_Connection=true;"
  }
}
```

For SQL Server Express:
```
Server=.\\SQLEXPRESS;Database=PaginationDb;Trusted_Connection=true;
```

For Azure SQL:
```
Server=tcp:your-server.database.windows.net,1433;Initial Catalog=PaginationDb;Persist Security Info=False;User ID=YourId;Password=YourPassword;Encrypt=True;Connection Timeout=30;
```

### Database Seeding

The seed migration creates **100,000 test users**. To adjust the count:

1. Modify [Seed_Initial_Data.cs](pagination/Migrations/20260212063553_Seed_Initial_Data.cs):
   ```csharp
   int totalUsers = 100000; // Change this value
   ```

2. Create a new migration:
   ```bash
   dotnet ef migrations add Update_Seed_Count
   dotnet ef database update
   ```


## 🛠️ Technologies & Dependencies

### Core Framework
- **.NET 10.0** - Latest .NET runtime
- **ASP.NET Core 10.0** - Web API framework
- **Entity Framework Core 10.0.3** - ORM

### Database
- **SQL Server 2019+** - Primary database
- **Entity Framework Core SQL Server** - DB provider

### APIs & Documentation
- **OpenAPI 10.0.3** - API specification
- **Scalar 2.12.38** - Interactive API documentation

### Benchmarking
- **BenchmarkDotNet 0.15.8** - Performance benchmarking framework
- **Moq 4.20.72** - Mocking library (for test support)

### Development
- **NuGet Packages**:
  - Microsoft.EntityFrameworkCore.Design
  - Microsoft.EntityFrameworkCore.Tools
  - Microsoft.VisualStudio.Azure.Containers.Tools.Targets (Docker support)


## 📈 Performance Insights

Based on the benchmark design:

- **Offset pagination degrades** as page number increases (deep pagination requires scanning more rows)
- **Cursor pagination remains consistent** regardless of position in dataset
- **Memory allocation is comparable** for both approaches with the same page size
- **Query complexity differs**: Offset uses SKIP, Cursor uses WHERE

### Recommendations

**Use Offset Pagination when:**
- Users need to browse specific pages (e.g., "Show page 5")
- SEO is important (clean page numbers)
- Small to medium datasets (< 100k rows)
- Traditional pagination UI is required

**Use Cursor Pagination when:**
- Building infinite scroll interfaces
- Handling large datasets (100k+ rows)
- Real-time data where consistency matters
- Mobile apps with "load more" patterns
- Deep pagination is common in your use case

---
