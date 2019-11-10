using Terminal.Gui;

namespace StdioProxy.Debugger
{
    public class App : Window
    {
        public App() : base("Stdio Debugger", 0)
        {
            // Application.Init();
            // // var top = Application.Top;
            // // var win = new Window("Stdio Debugger");
            // // top.Add(this);
            // Application.Run();
        }

        public override bool ProcessKey(KeyEvent keyEvent)
        {
            if (keyEvent.Key == Key.Esc)
            {
                Application.RequestStop();
                return true;
            }
            return base.ProcessKey(keyEvent);
        }
    }
}
