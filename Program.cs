// See https://aka.ms/new-console-template for more information
using BenchmarkDotNet.Running;
using InlineArryBenchmark;



Console.WriteLine("Hello, World!");

var summary = BenchmarkRunner.Run<DatabaseBenchmarks>();
Console.WriteLine(summary);