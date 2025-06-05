using System;
using System.IO.Pipes;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections.Concurrent;

namespace Master
{
    class Program
    {
        static ConcurrentDictionary<string, ConcurrentDictionary<string, int>> wordIndex = new();

        static void Main(string[] args)
        {
            Process.GetCurrentProcess().ProcessorAffinity = new IntPtr(1 << 2);

            Console.WriteLine("Master: Starting... Press ESC to exit.");

            Task[] tasks = new Task[]
            {
                Task.Run(() => ListenToAgent("agent1")),
                Task.Run(() => ListenToAgent("agent2"))
            };

            while (true)
            {
                if (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Escape)
                {
                    PrintAggregatedIndex();
                    Console.WriteLine("Press any key to exit...");
                    Console.ReadKey(); 
                    break;
                }
            }

            Console.WriteLine("Master: Shutting down...");
        }

        static void ListenToAgent(string pipeName)
        {
            while (true)
            {
                using (NamedPipeServerStream pipeServer = new NamedPipeServerStream(pipeName, PipeDirection.In))
                {
                    Console.WriteLine($"Master: Waiting for connection on pipe {pipeName}...");
                    pipeServer.WaitForConnection();
                    Console.WriteLine($"Master: Connected to pipe {pipeName}.");

                    using (StreamReader reader = new StreamReader(pipeServer))
                    {
                        while (!reader.EndOfStream)
                        {
                            string line = reader.ReadLine();
                            if (!string.IsNullOrEmpty(line))
                            {
                                Console.WriteLine($"Master: Received: {line}");
                                ProcessReceivedData(line);
                            }
                        }
                    }
                }
            }
        }

        static void ProcessReceivedData(string line)
        {
            string[] parts = line.Split(':');
            if (parts.Length == 3 && int.TryParse(parts[2], out int count))
            {
                string fileName = parts[0];
                string word = parts[1];

                var fileDict = wordIndex.GetOrAdd(fileName, _ => new ConcurrentDictionary<string, int>());
                fileDict.AddOrUpdate(word, count, (_, oldCount) => oldCount + count);
            }
        }

        static void PrintAggregatedIndex()
        {
            Console.WriteLine("\nMaster: Aggregated Word Index:");
            foreach (var fileEntry in wordIndex)
            {
                foreach (var wordEntry in fileEntry.Value)
                {
                    Console.WriteLine($"{fileEntry.Key}:{wordEntry.Key}:{wordEntry.Value}");
                }
            }
        }
    }
}