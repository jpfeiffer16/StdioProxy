// #define DBG_LOG

using System;
using System.Diagnostics;
#if DBG_LOG
using System.IO;
#endif
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
#if DBG_LOG
        private static string OUT_LOG_FILE = $"{HomeFolder}/stdioproxy.stdout.log";
        private static string IN_LOG_FILE = $"{HomeFolder}/stdioproxy.stdin.log";
        private static string ERR_LOG_FILE = $"{HomeFolder}/stdioproxy.stderr.log";
#endif
        private static string OMNISHARP_EXECUTABLE = IsWindows
            ? $"{HomeFolder}/.omnisharp/omnisharp-roslyn/omnisharp/OmniSharp.exe"
            : $"{HomeFolder}/.cache/omnisharp-vim/omnisharp-roslyn/omnisharp/OmniSharp.exe";

        static void Main(string[] args)
        {
#if DBG_LOG
            if (File.Exists(OUT_LOG_FILE))
            {
                File.Delete(OUT_LOG_FILE);
            }
            if (File.Exists(IN_LOG_FILE))
            {
                File.Delete(IN_LOG_FILE);
            }
            using (var stdoutLogStream = new StreamWriter(File.Create(OUT_LOG_FILE)))
            using (var stdinLogStream = new StreamWriter(File.Create(IN_LOG_FILE)))
            using (var stderrLogStream = new StreamWriter(File.Create(ERR_LOG_FILE)))
#endif
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
#if DBG_LOG
                            stdoutLogStream.Write(output);
#endif
                        }
                    }
                });

                var stdErrTask = Task.Run(() =>
                {
                    using (var stderr = proc.StandardError)
                    {
                        var buffer = new char[MAX_BUFFER_SIZE];
                        int bytesRead;
                        while((bytesRead = stderr.Read(buffer, 0, 1)) > 0)
                        {
                            var output = new String(buffer.Take(bytesRead).ToArray());
#if DBG_LOG
                            stderrLogStream.Write(output);
#endif
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
#if DBG_LOG
                            stderrLogStream.Write(((char)byteRead).ToString());
#endif
                        }
                    }
                });

                Task.WaitAll(stdoutTask, stdinTask);
            }
        }
    }
}
