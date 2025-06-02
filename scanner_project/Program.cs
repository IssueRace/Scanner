using System;
using System.Collections.Generic;
using System.IO;

namespace ScannerA
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("ScannerA: Enter directory path:");
            string directoryPath = Console.ReadLine();

            if (!Directory.Exists(directoryPath))
            {
                Console.WriteLine("Directory does not exist!");
                return;
            }

            var wordIndex = IndexFiles(directoryPath);
            foreach (var fileEntry in wordIndex)
            {
                foreach (var wordEntry in fileEntry.Value)
                {
                    Console.WriteLine($"{fileEntry.Key}:{wordEntry.Key}:{wordEntry.Value}");
                }
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
    }
}