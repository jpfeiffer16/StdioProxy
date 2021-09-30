using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace stdioproxy
{
    class Program
    {
        // It's a sad day. Someone is on Windows...
        private static bool IsWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        private static string HomeFolder = Environment.GetEnvironmentVariable(IsWindows ? "USERPROFILE" : "HOME");

        private const int MAX_BUFFER_SIZE = 2048;
        private static string LOG_FILE = $"{HomeFolder}/stdioproxy.log";
        private static string OMNISHARP_EXECUTABLE = IsWindows
            ? $"{HomeFolder}/.omnisharp/omnisharp-roslyn/omnisharp/OmniSharp.exe"
            : $"{HomeFolder}/.cache/omnisharp-vim/omnisharp-roslyn/omnisharp/OmniSharp.exe";

        static void Main(string[] args)
        {
            File.Exists(LOG_FILE) && File.Delete(LOG_FILE);
            using (var logStream = new StreamWriter(File.Create(LOG_FILE)))
            using (var proc = Process.Start(new ProcessStartInfo
            {
                FileName = IsWindows ? OMNISHARP_EXECUTABLE : "/usr/bin/mono",
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                Arguments = (IsWindows ? "" : $"--assembly-loader=strict {OMNISHARP_EXECUTABLE} ") + string.Join(' ', args)
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
