using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Text;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Slave
{
    /// <summary>
    /// Message reason in enum
    /// </summary>
    public enum MSG
    {
        TESTCONNECTION, //! Test connection
        none, //! None 
        EXEC, //! Execute
        FILE, //! FILE
        OFF, //! Shut down
        REBT, //! Reboot pc
        GTPTR //! Get Printers in client
    }

    class Program
    {
        private static byte[] bytes = new byte[1024]; // Buffer
        private static byte[] bytesFile; // Buffer for file
        private static Socket sender; // Server socket

        static void Main(string[] args)
        {

            StartClient(); // Start connection with server
        }

        /// <summary>
        /// Initialize connection with server and receive messages
        /// </summary>
        private static void StartClient()
        {

            try
            {
                IPHostEntry host = Dns.GetHostEntry("192.168.1.65"); // Server Ip
                IPAddress ipAddress = host.AddressList[0];
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, 11000); // Port 11000

                sender = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);


                try
                {
                    sender.Connect(remoteEP); // Connect with 192.168.1.65:11000

                    // Connected
                    Console.WriteLine($"Socket connected to {((IPEndPoint)sender.RemoteEndPoint).Address}");

                    while (true)
                    {
                        // Receive messages all time
                        Receive();
                    }
                }
                catch (SocketException e)
                {
                    // If there cant connnect
                    Console.WriteLine("Trying... -> {0}", e.SocketErrorCode);
                    // Try connect again
                    StartClient();
                }
            }
            catch (Exception e)
            {
                // Error in main try-catch
                Console.WriteLine($"Main try: {e}");
            }

        }

        /// <summary>
        /// Receive messages from socket server
        /// </summary>
        private static void Receive()
        {
            // Receive bytes
            int bytesRec = sender.Receive(bytes);
            ReceiveData(bytesRec, sender);
        }

        /// <summary>
        /// Receive file from server
        /// </summary>
        /// <param name="fileName">Filename with extension from server</param>
        private static void ReceiveFile(string fileName)
        {
            //? How do it works?
            //! 1) Receive message from server with: <FILE>[fileSize,filenameWithExtension]
            //! 2) Create byte array with size and store filenameWithExtension
            //! 3) Send to the client message when the arrival is ready
            //! 4) Client wait for file
            //! 5) Send message "true" when the file is ok

            sender.Receive(bytesFile);

            try
            {
                // Convert byte array in file
                using (var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
                {
                    // Write file
                    fs.Write(bytesFile, 0, bytesFile.Length);
                }

                // Send confirmation to server
                sender.Send(Encoding.ASCII.GetBytes("true"));
            }
            catch (Exception ex)
            {
                // Error on create the file
                Console.WriteLine("Exception caught in process: {0}", ex);
            }

        }

        /// <summary>
        /// Split information of the file received
        /// </summary>
        /// <param name="input">Information to split</param>
        /// <example>Received: <FILE>[3022,file.png]</example>
        /// <returns>String array: {3022,file.png}</returns>
        private static string[] SplitFileInfo(string input)
        {
            //! 1) Get text between '[]' 
            string preSplit = TextBetween(input, "[", "]");

            //! 2) Return split text by ','
            return preSplit.Split(',');
        }

        /// <summary>
        /// Receive data from server
        /// </summary>
        /// <param name="Rec">Count</param>
        /// <param name="sender">Socket server</param>
        public static void ReceiveData(int Rec, Socket sender)
        {
            //! 1) Get message stored in buffer 
            string recv = Encoding.ASCII.GetString(bytes, 0, Rec);

            //! 2) Get reason of this message
            switch (GetReason(recv))
            {
                //! 3) Depends with reason:

                case MSG.GTPTR:
                    string ptrs = string.Empty;

                    foreach(string p in System.Drawing.Printing.PrinterSettings.InstalledPrinters)
                    {
                        ptrs += $"[{p}]";

                    }

                    if (string.IsNullOrEmpty(ptrs))
                    {
                        sender.Send(Encoding.ASCII.GetBytes("<FALSE>"));
                    }
                    else
                    {
                        sender.Send(Encoding.ASCII.GetBytes("<TRUE>" + ptrs));

                    }
                    break;

                case MSG.REBT: // Reboot PC
                    var psiRebt = new ProcessStartInfo("shutdown", "/r /t 0")
                    {
                        CreateNoWindow = true,
                        UseShellExecute = false
                    };
                    sender.Send(Encoding.ASCII.GetBytes("true"));
                    Process.Start(psiRebt);
                    break;

                case MSG.OFF: // Shut down PC
                    var psi = new ProcessStartInfo("shutdown", "/s /t 0")
                    {
                        CreateNoWindow = true,
                        UseShellExecute = false
                    };
                    sender.Send(Encoding.ASCII.GetBytes("true"));
                    Process.Start(psi);

                    break;

                case MSG.FILE: // Get file (more info in ReceiveFile function)
                    Console.WriteLine("File preparation");
                    string[] inf = SplitFileInfo(recv);
                    bytesFile = new byte[int.Parse(inf[0])];
                    sender.Send(Encoding.ASCII.GetBytes("READY"));
                    ReceiveFile(inf[1]);
                    break;

                case MSG.EXEC: // Execute windows command
                    Console.WriteLine("Command Execution");
                    int indxSts = recv.IndexOf('>') + 1;
                    string command = recv.Substring(indxSts);

                    Process cmd = new Process();
                    cmd.StartInfo.FileName = "cmd.exe";
                    cmd.StartInfo.RedirectStandardInput = true;
                    cmd.StartInfo.RedirectStandardOutput = true;
                    cmd.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    cmd.StartInfo.UseShellExecute = false;
                    cmd.Start();

                    cmd.StandardInput.WriteLine(command);
                    cmd.StandardInput.Flush();
                    cmd.StandardInput.Close();
                    cmd.WaitForExit();

                    sender.Send(Encoding.ASCII.GetBytes("<CMDOUT>" + cmd.StandardOutput.ReadToEnd()));
                    break;

                case MSG.TESTCONNECTION: // Message to test connection
                    //! Return true to server
                    sender.Send(Encoding.ASCII.GetBytes("true"));
                    break;
            }

            bytes = new byte[1024]; //! Reset bytes
        }

        /// <summary>
        /// Convert string in enum MSG for reason
        /// </summary>
        /// <param name="input">Input to parse</param>
        /// <returns> Msg enum element </returns>
        public static MSG GetReason(string input)
        {
            // Write in console message reason
            Console.WriteLine(input.Split('<', '>')[0]);
            // Split message between '<>'
            string result = input.Split('<', '>')[1];

            MSG msg; // Create MSG enum to return

            switch (result)
            {
                case "GTPTR":
                    msg = MSG.GTPTR;
                    break;
                case "REBT":
                    msg = MSG.REBT;
                    break;
                case "OFF":
                    msg = MSG.OFF;
                    break;
                case "FILE":
                    msg = MSG.FILE;
                    break;
                case "TSCON":
                    msg = MSG.TESTCONNECTION;
                    break;
                case "EXEC":
                    msg = MSG.EXEC;
                    break;
                default:
                    msg = MSG.none;
                    break;
            }

            return msg;
        }

        /// <summary>
        /// Get Text between two chars
        /// </summary>
        /// <param name="input">Input to extract</param>
        /// <param name="start">Initial char</param>
        /// <param name="end">End char</param>
        /// <returns> Strign between two chars</returns>
        public static string TextBetween(string input, string start, string end)
        {
            int p1 = input.IndexOf(start) + start.Length; // Get index of first string 

            int p2 = input.IndexOf(end, p1); // Get index of second string

            if (end == "") return (input.Substring(p1)); // Return string if is ""
            else return input.Substring(p1, p2 - p1);
        }
    }
}
