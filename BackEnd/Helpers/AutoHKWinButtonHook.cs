using System;
using System.IO.Pipes;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;

namespace StartifyBackend.Mechanics
{
    class NamedPipeServer
    {
        public event EventHandler<EventArgs> StartTriggered;

        async Task Main()
        {
            using (NamedPipeServerStream pipeServer = new NamedPipeServerStream("StartifyAHK", PipeDirection.In))
            {
                Console.WriteLine("Waiting for connection to ahk pipe server...");
                pipeServer.WaitForConnection();

                using (StreamReader reader = new StreamReader(pipeServer))
                {
                    while (true)
                    {
                        string message = await reader.ReadLineAsync();
                        Debug.WriteLine("Received message from Startify AHK script: " + message);

                        // Raise the StartTriggered event when a message is received
                        StartTriggered?.Invoke(this, EventArgs.Empty);
                    }
                }
            }
        }
    }
}
