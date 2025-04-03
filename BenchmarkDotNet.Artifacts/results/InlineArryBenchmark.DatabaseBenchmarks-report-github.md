```

BenchmarkDotNet v0.14.0, Windows 11 (10.0.26100.3624)
Intel Core i7-8750H CPU 2.20GHz (Coffee Lake), 1 CPU, 12 logical and 6 physical cores
.NET SDK 9.0.102
  [Host]     : .NET 9.0.1 (9.0.124.61010), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.1 (9.0.124.61010), X64 RyuJIT AVX2


```
| Method                      | Mean         | Error       | StdDev      |
|---------------------------- |-------------:|------------:|------------:|
| Sql_GetOrdersWithCustomer   | 6,711.446 ms | 151.2858 ms | 441.3080 ms |
| NoSql_GetOrdersWithCustomer |     1.488 ms |   0.0394 ms |   0.1123 ms |
