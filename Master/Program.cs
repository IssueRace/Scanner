using System;
using System.IO.Pipes;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Master
{
    class Program
    {
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
                    break;
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
                            }
                        }
                    }
                }
            }
        }
    }
}