using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Diagnostics;
using System.Threading.Tasks;

namespace ScannerB
{
    class Program
    {
        static void Main(string[] args)
        {
            Process.GetCurrentProcess().ProcessorAffinity = new IntPtr(1 << 1);

            while (true)
            {
                Console.WriteLine("ScannerB: Enter directory path or 'exit' to quit:");
                string directoryPath = Console.ReadLine();

                if (directoryPath.ToLower() == "exit")
                    break;

                if (!Directory.Exists(directoryPath))
                {
                    Console.WriteLine("Directory does not exist!");
                    continue;
                }

                var txtFiles = Directory.GetFiles(directoryPath, "*.txt");
                if (txtFiles.Length == 0)
                {
                    Console.WriteLine("No .txt files found in directory!");
                    continue;
                }

                Task<Dictionary<string, Dictionary<string, int>>> indexTask = Task.Run(() => IndexFiles(directoryPath));

                Task.Run(async () =>
                {
                    var wordIndex = await indexTask;
                    await SendDataToMaster(wordIndex);
                }).Wait();
            }
        }

        static Dictionary<string, Dictionary<string, int>> IndexFiles(string directoryPath)
        {
            var wordIndex = new Dictionary<string, Dictionary<string, int>>();

            foreach (string file in Directory.GetFiles(directoryPath, "*.txt"))
            {
                string fileName = Path.GetFileName(file);
                var wordCounts = new Dictionary<string, int>();

                string content = File.ReadAllText(file);
                string[] words = content.Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (string word in words)
                {
                    string cleanedWord = word.ToLower().Trim();
                    if (!string.IsNullOrEmpty(cleanedWord))
                    {
                        if (wordCounts.ContainsKey(cleanedWord))
                            wordCounts[cleanedWord]++;
                        else
                            wordCounts[cleanedWord] = 1;
                    }
                }

                wordIndex[fileName] = wordCounts;
            }

            return wordIndex;
        }

        static async Task SendDataToMaster(Dictionary<string, Dictionary<string, int>> wordIndex)
        {
            using (NamedPipeClientStream pipeClient = new NamedPipeClientStream(".", "agent2", PipeDirection.Out))
            {
                try
                {
                    Console.WriteLine("ScannerB: Connecting to Master...");
                    await pipeClient.ConnectAsync(5000);
                    Console.WriteLine("ScannerB: Connected to Master.");

                    using (StreamWriter writer = new StreamWriter(pipeClient))
                    {
                        writer.AutoFlush = true;
                        foreach (var fileEntry in wordIndex)
                        {
                            foreach (var wordEntry in fileEntry.Value)
                            {
                                await writer.WriteLineAsync($"{fileEntry.Key}:{wordEntry.Key}:{wordEntry.Value}");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ScannerB: Error - {ex.Message}");
                }
            }
        }
    }
}