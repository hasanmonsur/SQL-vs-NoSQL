using BenchmarkDotNet.Attributes;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace InlineArryBenchmark
{
    public class DatabaseBenchmarks
    {
        private const int NumberOfOrders = 1000;
        private SqlDbContext _sqlContext;
        private IMongoDatabase _mongoDatabase;
        private int _randomId;
        private List<int> _postgresIds = new List<int>();
        private List<BsonValue> _mongoIds = new List<BsonValue>();
        private Random _random = new Random();


        [GlobalSetup]
        public void Setup()
        {
            // PostgreSQL setup
            var sqlOptions = new DbContextOptionsBuilder<SqlDbContext>()
                .UseNpgsql("Server=localhost;Database=postgres;User Id=postgres;Password=postgres;")
                .Options;

            _sqlContext = new SqlDbContext(sqlOptions);
            _sqlContext.Database.EnsureCreated();

            // MongoDB setup
            var mongoClient = new MongoClient("mongodb://admin:admin123@127.0.0.1:27017/?authSource=admin");
            _mongoDatabase = mongoClient.GetDatabase("salesdb");
            _mongoDatabase.DropCollection("orders"); // Clean start for each benchmark


            // Generate test data and store IDs
            var orders = GenerateOrders();
            _sqlContext.Orders.AddRangeAsync(orders);
            _sqlContext.SaveChangesAsync();
            _postgresIds = orders.Select(o => o.Id).ToList();

            // Insert same data into MongoDB and store IDs
            var mongoOrders = GenerateMongoOrders();
            var collection = _mongoDatabase.GetCollection<BsonDocument>("orders");
            collection.InsertManyAsync(mongoOrders);
            _mongoIds = mongoOrders.Select(o => o["_id"]).ToList();



        }

        [GlobalCleanup]
        public void Cleanup()
        {
            try
            {
                // Use a new context for cleanup to avoid disposed object issues
                using var cleanupContext = new SqlDbContext(
                    new DbContextOptionsBuilder<SqlDbContext>()
                        .UseNpgsql("Server=localhost;Database=postgres;User Id=postgres;Password=postgres;")
                        .Options);

                cleanupContext.Database.EnsureDeleted();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Cleanup error: {ex.Message}");
            }

            _sqlContext?.Dispose();
        }

        public void Dispose()
        {
            Cleanup();
        }


        
        [Benchmark]
        public async Task Sql_InsertOrders()
        {
            await _sqlContext.Orders.AddRangeAsync(GenerateOrders());
            await _sqlContext.SaveChangesAsync();
        }

        [Benchmark]
        public async Task NoSql_InsertOrders()
        {
            var collection = _mongoDatabase.GetCollection<BsonDocument>("orders");
            await collection.InsertManyAsync(GenerateMongoOrders());
        }


        [Benchmark]
        public async Task Sql_GetOrderById()
        {
            var randomIndex = _random.Next(_postgresIds.Count);
            var idToQuery = _postgresIds[randomIndex];

            using var context = new SqlDbContext();
            await context.Orders.FirstOrDefaultAsync(o => o.Id == idToQuery);
        }

        [Benchmark]
        public async Task NoSql_GetOrderById()
        {
            var randomIndex = _random.Next(_mongoIds.Count);
            var idToQuery = _mongoIds[randomIndex];

            var collection = _mongoDatabase.GetCollection<BsonDocument>("orders");
            await collection.Find(Builders<BsonDocument>.Filter.Eq("_id", idToQuery)).FirstOrDefaultAsync();
        }
        


        // SQL: Join orders with customers
        [Benchmark]
        public async Task Sql_GetOrdersWithCustomer()
        {
            using var context = new SqlDbContext();
            await context.Orders.Where(o => o.Amount > 100).ToListAsync();


        }

        // NoSQL: Embedded customer data in orders
        [Benchmark]
        public async Task NoSql_GetOrdersWithCustomer()
        {
            var collection = _mongoDatabase.GetCollection<BsonDocument>("orders");
            await collection.Find(Builders<BsonDocument>.Filter.Gt("total", 100)).ToListAsync();
        }


        private List<Order> GenerateOrders()
        {
            var orders = new List<Order>();
            var random = new Random();

            for (int i = 0; i < NumberOfOrders; i++)
            {
                orders.Add(new Order(
                    customerName: $"Customer {i}",
                    orderDate: DateTime.UtcNow.AddDays(-random.Next(365)),
                    amount: random.Next(100, 10000),
                    isShipped: random.NextDouble() > 0.5,
                    Items: Enumerable.Range(1, random.Next(1, 6))
                        .Select(n => new OrderItem(
                            productName: $"Product {n}",
                            quantity: random.Next(1, 5),
                            price: random.Next(10, 500)))
                        .ToList()  // This should be after the Select, not on OrderItem
                ));
            }

            return orders;
        }

        private List<BsonDocument> GenerateMongoOrders()
        {
            var orders = new List<BsonDocument>();
            var random = new Random();

            for (int i = 0; i < NumberOfOrders; i++)
            {
                var order = new BsonDocument
            {
                { "orderDate", DateTime.Now.AddDays(-random.Next(365)) },
                { "customerName", $"Customer {i}" },
                { "amount", random.Next(100, 10000) },
                { "isShipped", random.NextDouble() > 0.5 },
                { "items", new BsonArray(
                    Enumerable.Range(1, random.Next(1, 6))
                        .Select(n => new BsonDocument
                        {
                            { "productName", $"Product {n}" },
                            { "quantity", random.Next(1, 5) },
                            { "price", random.Next(10, 500) }
                        }))
                }
            };
                orders.Add(order);
            }

            return orders;
        }
    }
}
