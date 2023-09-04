// See https://aka.ms/new-console-template for more information

using SpaceProfilerLogic;

var directory = Console.ReadLine();

var profiler = new Profiler();
var result = profiler.GetOrderedFiles(directory);

foreach (var fileInfo in result)
{
    Console.WriteLine($"{fileInfo.Length} {fileInfo.FullName}");
}