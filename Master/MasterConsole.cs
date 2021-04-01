using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace Master
{

    /// <summary>
    /// Type of console message
    /// </summary>
    public enum CONSOLE_MSG
    {
        error,
        info,
        connectInfo,
        warning,
        success,
        cmdClient
    }

    /// <summary>
    /// Type of help to show
    /// </summary>
    public enum CONSOLE_HELP
    {
        console,
        exit,
        slaves,
        file,
        off,
        exec
    }

    public class MasterConsole
    {
        private static readonly List<string> CommandsList = new List<string>{"print","exit","slaves","off","exec","cls","file" };

        private static readonly string usr = "admin"; //! USER
        private static readonly string lvl = "master"; //! LEVEL
        public static Status sts; // Status Object

        /// <summary>
        /// Starting console control
        /// </summary>
        public static void StartConsole()
        {
            // Initialize Status
            sts = new Status();
            Loop();
        }

        private static void Loop()
        {
            while (true)
            {
                // Initialize server
                Connection.StartServer();
                ExecuteCommand(PrintUser());
            }
        }

        /// <summary>
        /// Obtains Useer info
        /// </summary>
        /// <returns>Formatted user and level</returns>
        private static string GetUserInfo()
        {
            return $"\n{usr}@{lvl}:";
        }

        /// <summary>
        /// Print in console "name"@"level":
        /// </summary>
        /// <returns> Input Text </returns>
        private static string PrintUser()
        {
            Console.CursorSize = 1;
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine(GetUserInfo());
            Console.ResetColor();
            Console.SetCursorPosition(GetUserInfo().Length, Console.CursorTop - 1);
            return Console.ReadLine();
        }

        /// <summary>
        /// Print line in console by type (color)
        /// </summary>
        /// <param name="msg">Message to print</param>
        /// <param name="type">Message type</param>
        public static void ConsoleMessage(string msg, CONSOLE_MSG type)
        {
            switch (type)
            {
                case CONSOLE_MSG.cmdClient:
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine($"\n{msg}");
                    Console.ResetColor();
                    break;
                case CONSOLE_MSG.success:
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"\n{msg}");
                    Console.ResetColor();
                    break;
                case CONSOLE_MSG.error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"\n{msg}");
                    Console.ResetColor();
                    break;
                case CONSOLE_MSG.info:
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine($"\n{msg}");
                    Console.ResetColor();
                    break;
                case CONSOLE_MSG.warning:
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.WriteLine($"\n{msg}");
                    Console.ResetColor();
                    break;
                case CONSOLE_MSG.connectInfo:
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine($"\n\n{msg}");
                    Console.ResetColor();
                    ExecuteCommand(PrintUser());
                    break;
            }
        }

        /// <summary>
        /// Execute console command
        /// </summary>
        /// <param name="cmd"> Command to execute </param>
        private static void ExecuteCommand(string cmd)
        {
            // Separate command in arguments
            string[] Args = cmd.Split(' ');

            // The first index in array is the head command
            switch (Args[0])
            {
                case "off":
                    Commands.OffCommand(Args);
                    break;
                case "file":
                    Commands.FileCommand(Args,cmd);
                    break;
                case "exec":
                    Commands.ExecCommand(Args, cmd);
                    break;
                case "slaves":
                    Commands.SlavesCommand(Args);
                    break;
                case "cls":
                    Console.Clear();
                    break;
                case "":
                    break;
                case "exit":
                    Commands.ExitCommand(Args);
                    break;
                case "help":
                    ShowHelp(CONSOLE_HELP.console);
                    break;
                default:
                    CommandMatching(Args[0]);
                    break;
            }
        }

        /// <summary>
        /// Check similarities with another command
        /// </summary>
        /// <param name="com">Command to check</param>
        private static void CommandMatching(string com)
        {
            bool sim = false;
            foreach(string cmd in CommandsList)
            {
                sim = Check.Similarity(com, cmd);
                if (sim)
                {
                    ConsoleMessage($"Did you mean: {cmd} ?", CONSOLE_MSG.error);
                    break;
                }
            }

            if (!sim)
            {
                ConsoleMessage("Unknown command", CONSOLE_MSG.error);
            }
        }

        /// <summary>
        /// Stop all task in execution and close win console
        /// </summary>
        public static void StopConsole()
        {
            Environment.Exit(0);
        }

        /// <summary>
        /// Show help about function
        /// </summary>
        /// <param name="chp">help to show</param>
        public static void ShowHelp(CONSOLE_HELP chp)
        {
            switch (chp)
            {
                case CONSOLE_HELP.off:

                    ConsoleMessage("===== Off command =====", CONSOLE_MSG.info);
                    ConsoleMessage("Off slave machine", CONSOLE_MSG.info);
                    ConsoleMessage("    -h show help", CONSOLE_MSG.info);
                    ConsoleMessage("    -r reboot slave", CONSOLE_MSG.info);
                    ConsoleMessage("    -s specified slave to off/reboot", CONSOLE_MSG.info);
                    ConsoleMessage("    -all off/reboot all slave machines", CONSOLE_MSG.info);
                    break;
                case CONSOLE_HELP.exec:
                    ConsoleMessage("===== Exec command =====", CONSOLE_MSG.info);
                    ConsoleMessage("Execute windows command in client", CONSOLE_MSG.info);
                    ConsoleMessage("Write command between [here]", CONSOLE_MSG.info);
                    ConsoleMessage("    -h show help", CONSOLE_MSG.info);
                    ConsoleMessage("    -s specified slave to executer command", CONSOLE_MSG.info);
                    ConsoleMessage("    -all execute windows command in all slaves", CONSOLE_MSG.info);
                    break;
                case CONSOLE_HELP.file:
                    ConsoleMessage("===== file command =====", CONSOLE_MSG.info);
                    ConsoleMessage("send file to default slave", CONSOLE_MSG.info);
                    ConsoleMessage("    -h show help", CONSOLE_MSG.info);
                    ConsoleMessage("    -f file path between quote marks \"here\"", CONSOLE_MSG.info);
                    ConsoleMessage("    -s specified slave to send", CONSOLE_MSG.info);
                    ConsoleMessage("    -all send file to all slaves", CONSOLE_MSG.info);

                    break;
                case CONSOLE_HELP.slaves:
                    ConsoleMessage("===== slaves command =====", CONSOLE_MSG.info);
                    ConsoleMessage("show all connected slaves", CONSOLE_MSG.info);
                    ConsoleMessage("    -h show help", CONSOLE_MSG.info);
                    ConsoleMessage("    -c show current slave", CONSOLE_MSG.info);
                    break;
                case CONSOLE_HELP.console:
                    ConsoleMessage($"Console version {Assembly.GetExecutingAssembly().GetName().Version.ToString()}", CONSOLE_MSG.info);
                    break;
                case CONSOLE_HELP.exit:
                    ConsoleMessage("===== exit command =====", CONSOLE_MSG.info);
                    ConsoleMessage("Stop all tasks in execution", CONSOLE_MSG.info);
                    ConsoleMessage("Close console", CONSOLE_MSG.info);
                    break;
            }
        }

        [DllImport("user32.dll")]
        public static extern bool ShowWindowAsync(HandleRef hWnd, int nCmdShow);
        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr WindowHandle);

        public const int SW_RESTORE = 9;

        /// <summary>
        /// Focus Window Console And close other consoles
        /// </summary>
        public static void FoccusOneConsole()
        {
            Process[] objProcesses = System.Diagnostics.Process.GetProcessesByName("Master");
            if (objProcesses.Length > 0)
            {
                // Get handle
                IntPtr hWnd = IntPtr.Zero;
                hWnd = objProcesses[0].MainWindowHandle;

                // Set Focus
                ShowWindowAsync(new HandleRef(null, hWnd), SW_RESTORE);
                SetForegroundWindow(objProcesses[0].MainWindowHandle);
            }
            
            // Kill this proccess
            Process.GetCurrentProcess().Kill();
        }
    }
}
