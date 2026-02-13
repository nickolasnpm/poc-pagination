using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using pagination.Application;
using pagination.Controllers;
using pagination.Domain;
using pagination.Infrastructure;
using System;
using System.IO;
using System.Threading.Tasks;

namespace BenchmarkSuite
{
    [MemoryDiagnoser]
    [Orderer(SummaryOrderPolicy.FastestToSlowest)]
    [RankColumn]
    [MinColumn, MaxColumn]
    [MarkdownExporter]
    public class PaginationBenchmarks
    {
        private UserController _controller;
        private IOffsetRepository _offsetRepository;
        private ICursorRepository _cursorRepository;
        private ILogger<UserController> _logger;
        private UserDbContext _dbContext;
        private ServiceProvider _serviceProvider;

        private const int PageSize = 50;
        private int _totalRecords;
        private int _totalPages;
        private Random _random;

        [GlobalSetup]
        public async Task Setup()
        {
            _random = new Random(42);

            // Build configuration with user secrets support
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                .AddUserSecrets<PaginationBenchmarks>()
                .Build();

            // Setup dependency injection
            var services = new ServiceCollection();

            // Add DbContext
            services.AddDbContext<UserDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    sqlOptions =>
                    {
                        sqlOptions.EnableRetryOnFailure(
                            maxRetryCount: 3,
                            maxRetryDelay: TimeSpan.FromSeconds(5),
                            errorNumbersToAdd: null
                        );
                    }
                )
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
            );

            // Add repositories
            services.AddScoped<IOffsetRepository, OffsetRepository>();
            services.AddScoped<ICursorRepository, CursorRepository>();

            // Add logging
            services.AddLogging(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Warning);
            });

            _serviceProvider = services.BuildServiceProvider();

            // Get services
            var scope = _serviceProvider.CreateScope();
            _dbContext = scope.ServiceProvider.GetRequiredService<UserDbContext>();
            _offsetRepository = scope.ServiceProvider.GetRequiredService<IOffsetRepository>();
            _cursorRepository = scope.ServiceProvider.GetRequiredService<ICursorRepository>();
            _logger = scope.ServiceProvider.GetRequiredService<ILogger<UserController>>();

            // Get actual record count from database
            _totalRecords = await _dbContext.Users.CountAsync();
            _totalPages = (int)Math.Ceiling((double)_totalRecords / PageSize);

            Console.WriteLine($"Database has {_totalRecords} records.");
            Console.WriteLine($"Total pages: {_totalPages}");
            Console.WriteLine($"Page size: {PageSize}");

            _controller = new UserController(_offsetRepository, _cursorRepository, _logger);
        }

        [GlobalCleanup]
        public void Cleanup()
        {
            _dbContext?.Dispose();
            _serviceProvider?.Dispose();
        }

        #region Offset Pagination Benchmarks

        [Benchmark(Description = "Offset: First Page")]
        public async Task<IActionResult> Offset_RetrieveFirstPage()
        {
            var request = new DefaultPaginationRequest
            {
                PaginationType = (int)PaginationType.Offset,
                offsetPagination = new OffsetPaginationRequest
                {
                    Page = 1,
                    PageSize = PageSize
                }
            };
            return await _controller.DefaultPaginationAsync(request);
        }

        [Benchmark(Description = "Offset: Last Page")]
        public async Task<IActionResult> Offset_RetrieveLastPage()
        {
            var request = new DefaultPaginationRequest
            {
                PaginationType = (int)PaginationType.Offset,
                offsetPagination = new OffsetPaginationRequest
                {
                    Page = _totalPages,
                    PageSize = PageSize
                }
            };
            return await _controller.DefaultPaginationAsync(request);
        }

        [Benchmark(Description = "Offset: Next Page")]
        public async Task<IActionResult> Offset_RetrieveNextPage()
        {
            var request = new DefaultPaginationRequest
            {
                PaginationType = (int)PaginationType.Offset,
                offsetPagination = new OffsetPaginationRequest
                {
                    Page = 2,
                    PageSize = PageSize
                }
            };
            return await _controller.DefaultPaginationAsync(request);
        }

        [Benchmark(Description = "Offset: Previous Page")]
        public async Task<IActionResult> Offset_RetrievePreviousPage()
        {
            var request = new DefaultPaginationRequest
            {
                PaginationType = (int)PaginationType.Offset,
                offsetPagination = new OffsetPaginationRequest
                {
                    Page = _totalPages - 1,
                    PageSize = PageSize
                }
            };
            return await _controller.DefaultPaginationAsync(request);
        }

        [Benchmark(Description = "Offset: Random Page (around 99500)")]
        public async Task<IActionResult> Offset_RetrieveRandomPage()
        {
            // Random page near the end (last 10 pages)
            int randomPage = _random.Next(Math.Max(1, _totalPages - 10), _totalPages + 1);
            var request = new DefaultPaginationRequest
            {
                PaginationType = (int)PaginationType.Offset,
                offsetPagination = new OffsetPaginationRequest
                {
                    Page = randomPage,
                    PageSize = PageSize
                }
            };
            return await _controller.DefaultPaginationAsync(request);
        }

        #endregion

        #region Cursor Pagination Benchmarks

        [Benchmark(Description = "Cursor: First Page")]
        public async Task<IActionResult> Cursor_RetrieveFirstPage()
        {
            var request = new DefaultPaginationRequest
            {
                PaginationType = (int)PaginationType.Cursor,
                cursorPagination = new CursorPaginationRequest
                {
                    Cursor = 0,
                    PageSize = PageSize
                }
            };
            return await _controller.DefaultPaginationAsync(request);
        }

        [Benchmark(Description = "Cursor: Last Page")]
        public async Task<IActionResult> Cursor_RetrieveLastPage()
        {
            long lastPageCursor = _totalRecords - PageSize;
            var request = new DefaultPaginationRequest
            {
                PaginationType = (int)PaginationType.Cursor,
                cursorPagination = new CursorPaginationRequest
                {
                    Cursor = lastPageCursor,
                    PageSize = PageSize
                }
            };
            return await _controller.DefaultPaginationAsync(request);
        }

        [Benchmark(Description = "Cursor: Next Page")]
        public async Task<IActionResult> Cursor_RetrieveNextPage()
        {
            var request = new DefaultPaginationRequest
            {
                PaginationType = (int)PaginationType.Cursor,
                cursorPagination = new CursorPaginationRequest
                {
                    Cursor = PageSize,
                    PageSize = PageSize
                }
            };
            return await _controller.DefaultPaginationAsync(request);
        }

        [Benchmark(Description = "Cursor: Previous Page")]
        public async Task<IActionResult> Cursor_RetrievePreviousPage()
        {
            var request = new DefaultPaginationRequest
            {
                PaginationType = (int)PaginationType.Cursor,
                cursorPagination = new CursorPaginationRequest
                {
                    Cursor = 0,
                    PageSize = PageSize
                }
            };
            return await _controller.DefaultPaginationAsync(request);
        }

        [Benchmark(Description = "Cursor: Random Page (around 99500)")]
        public async Task<IActionResult> Cursor_RetrieveRandomPage()
        {
            // Random cursor near the end (last 500 records)
            long randomCursor = _random.Next(Math.Max(0, _totalRecords - 500), _totalRecords - PageSize);
            var request = new DefaultPaginationRequest
            {
                PaginationType = (int)PaginationType.Cursor,
                cursorPagination = new CursorPaginationRequest
                {
                    Cursor = randomCursor,
                    PageSize = PageSize
                }
            };
            return await _controller.DefaultPaginationAsync(request);
        }

        #endregion
    }
}