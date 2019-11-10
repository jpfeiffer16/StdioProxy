using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Reflection;
using System.Text;

namespace StdioProxy.Debugger
{
    class Program
    {
        private static int _seq = 999;
        static void Main(string[] args)
        {
            var client = new TcpClient("localhost", 1234);
            var tmp = Path.Join(Path.GetTempPath(), "stdioproxy");
            if (!Directory.Exists(tmp))
            {
                Directory.CreateDirectory(tmp);
            }
            var tmpFile = Path.Join(tmp, Guid.NewGuid().ToString());
            using (var file = File.Create(tmpFile))
            {
                File.Open(
                    Path.Join(
                        Path.GetDirectoryName(
                            new Uri(Assembly.GetExecutingAssembly().CodeBase).AbsolutePath),
                            "template.txt"), FileMode.Open).CopyTo(file);
            }
            Console.WriteLine(tmpFile);

            var editorProc = Process.Start(new ProcessStartInfo
            {
                FileName = "gnome-terminal",
                // FileName = "/bin/bash",
                Arguments = $"-- vim {tmpFile}",
                RedirectStandardError = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true
            });
            while (true)
            {
                Console.WriteLine("Press a key to send a request");
                Console.ReadKey();
                var text = File.ReadAllText(tmpFile);
                var contentLength = text.Length;
                client.Client.BeginSend(Encoding.UTF8.GetBytes(text), 0, contentLength, SocketFlags.Broadcast);
            }
            // Application.Init();
            // var top = Application.Top;
            // var win = new App();
            // top.Add(win);
            // var requestPathLabel = new Label("Request Path:") { X = 3, Y = 2 };
            // win.Add(requestPathLabel);
            // var requestPathTextBox = new TextField("")
            // {
            //     X = Pos.Right(requestPathLabel),
            //     Y = Pos.Top(requestPathLabel),
            //     Width = 40
            // };
            // win.Add(requestPathTextBox);
            // var requestBodyTextBox = new TextField("")
            // {
            //     X = Pos.Left(requestPathTextBox),
            //     Y = Pos.Bottom(requestPathTextBox),
            //     Width = 40
            // };
            // win.Add(requestBodyTextBox);
            // Application.Run();
        }
    }
}
