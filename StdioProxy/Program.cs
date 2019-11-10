using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace stdioproxy
{
    class Program
    {
        private static string HomeFolder = Environment.GetEnvironmentVariable(RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "USERPROFILE" : "HOME");
        private const int MAX_BUFFER_SIZE = 2048;
        private static string LOG_FILE = $"{HomeFolder}/stdioproxy.log";
        private static string OMNISHARP_EXECUTABLE = $"{HomeFolder}/.omnisharp/omnisharp-roslyn/OmniSharp.exe";
        // private static string OMNISHARP_EXECUTABLE = "/home/jpfeiffer/Source/Razor.VSCode/src/Microsoft.AspNetCore.Razor.LanguageServer/bin/Debug/netcoreapp2.2/publish/linux-x64/rzls";
        // private static string OMNISHARP_EXECUTABLE = "/home/jpfeiffer/.vscode/extensions/ms-vscode.csharp-1.21.0/.razor/rzls";

        static void Main(string[] args)
        {
            // var listener = new TcpListener(IPAddress.Loopback, 1234);
            // listener.Start();
            using (var logStream = new StreamWriter(File.Create(LOG_FILE)))
            using (var proc = Process.Start(new ProcessStartInfo
            {
                FileName = OMNISHARP_EXECUTABLE,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                Arguments = string.Join(' ', args)
            }))
            {
                var stdoutTask = Task.Run(() =>
                {
                    using (var stdout = proc.StandardOutput)
                    {
                        var buffer = new char[MAX_BUFFER_SIZE];
                        int bytesRead;
                        while((bytesRead = stdout.Read(buffer, 0, 1)) > 0)
                        {
                            var output = new String(buffer.Take(bytesRead).ToArray());
                            Console.Write(output);
                            logStream.Write(output);
                        }
                    }
                });

                var stdinTask = Task.Run(() =>
                {
                    using (var thisStdin = Console.OpenStandardInput())
                    {
                        var buffer = new char[MAX_BUFFER_SIZE];
                        int byteRead;
                        while ((byteRead = thisStdin.ReadByte()) > 0)
                        {
                            proc.StandardInput.Write((char)byteRead);
                        }
                    }
                });

                Task.WaitAll(stdoutTask, stdinTask);
            }
        }
    }
}
