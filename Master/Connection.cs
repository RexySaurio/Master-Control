using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Master
{
    /// <summary>
    /// Status and information about connection
    /// </summary>
    public class Status
    {
        // Random Object
        public static Random rand = new Random();

        // Client
        private static bool Instance = false;
        public const int BufferSize = 1024;
        public static byte[] buffer = new byte[BufferSize];
        public StringBuilder sb = new StringBuilder();
        public static Socket workSocket = null;
        public static TcpListener Listener { get { return listener; } }
        private static TcpListener listener = null;
        public static List<TcpClient> ClientsConnected { get { UpdateConnectedClients(); return ctsConn; } set { ctsConn = value; } }
        private static List<TcpClient> ctsConn = new List<TcpClient>();
        public static TcpClient CurrentClient { get; set; }

        public Status()
        {
            // Check if there is no more than 1 instance
            if (Instance)
            {
                // If there is, then, send error
                throw new InvalidOperationException("There cannot be more than one status instance");
            }
            else
            {
                Instance = true;
            }

            // Prepare connection
            IPHostEntry ipHostInfo = Dns.GetHostEntry("192.168.1.65");
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 11000);

            // Tcp Socket
            listener = new TcpListener(localEndPoint.Address, localEndPoint.Port);
            //! Try for check only one instance of the console
            try
            {
                // Start listen
                listener.Start();
            }
            catch (SocketException sE)
            {
                if (sE.ErrorCode == sE.ErrorCode)
                {
                    MasterConsole.FoccusOneConsole();
                }
            }
        }

        public static void UpdateConnectedClients()
        {
            for (int i = 0; i < ctsConn.Count; i++)
            {
                if (!CheckConnectedClient(ctsConn[i].Client))
                {
                    ctsConn.Remove(ctsConn[i]);
                }
            }
        }

        public static bool CheckConnectedClient(Socket client)
        {
            bool result = false;

            try
            {
                string recv = Connection.ClientMessage("<TSCON>", client);
                if (recv.Contains("true"))
                {
                    result = true;
                }
            }
            catch (SocketException sE)
            {
                if (sE.SocketErrorCode == SocketError.ConnectionAborted)
                {
                    result = false;
                }
            }

            return result;
        }

        public static string GetClientIp(Socket client)
        {
            return ((IPEndPoint)client.RemoteEndPoint).Address.ToString();
        }
    }

    class Connection
    {

        public static void StartServer()
        {
            Status.Listener.BeginAcceptTcpClient(AcceptCallback, Status.Listener);
        }

        private static void AcceptCallback(IAsyncResult ar)
        {
            TcpClient client = Status.Listener.EndAcceptTcpClient(ar);
            Status.ClientsConnected.Add(client);
            Status.CurrentClient = client;
        }

        /// <summary>
        /// Send and Receive message from client specified
        /// </summary>
        /// <param name="msg"> Message To send </param>
        /// <param name="client"> Client to send message </param>
        /// <returns> Message received from client </returns>
        public static string ClientMessage(string msg, Socket client)
        {
            client.Send(Encoding.ASCII.GetBytes(msg));
            client.Receive(Status.buffer);
            string recv = Encoding.ASCII.GetString(Status.buffer);

            return recv;
        }

        public static void SendFile(byte[] bytes, Socket client, string fpath)
        {

            string filename = Path.GetFileName(fpath);

            // Send message to client as <MSG>[bytes size by int,filename]
            string res = ClientMessage($"<FILE>[{bytes.Length},{filename}]", client);

            //? If client returns "READY"
            if (res.Contains("READY"))
            {
                client.Send(bytes);
                client.Receive(Status.buffer);
                string recv = Encoding.ASCII.GetString(Status.buffer);
                if (recv.Contains("true"))
                {
                    MasterConsole.ConsoleMessage("Sucess", CONSOLE_MSG.success);

                }
            }
            else
            {
                MasterConsole.ConsoleMessage("Error in slave, try again.", CONSOLE_MSG.error);
            }
        }
    }

    public class Slaves
    {
        /// <summary>
        /// Update Connected clients and print actives
        /// </summary>
        public static void PrintSlaves()
        {


            //? If there are clients connected?
            if (Status.ClientsConnected.Count > 0)
            {
                int cnt = Status.ClientsConnected.Count;
                for (int i = 0; i < cnt; i++)
                {
                    string ip = Status.GetClientIp(Status.ClientsConnected[i].Client);
                    MasterConsole.ConsoleMessage($"{i}) {ip}", CONSOLE_MSG.info);
                }
            }
            else
            {
                MasterConsole.ConsoleMessage("No slaves connected", CONSOLE_MSG.info);

            }
        }

        /// <summary>
        /// Print current client selcted and
        /// check if is connected
        /// </summary>
        public static void PrintCurrentSlave()
        {
            if (Status.ClientsConnected.Count > 0)
            {
                // Check if current client is desconnected
                if (!Status.CheckConnectedClient(Status.CurrentClient.Client))
                {
                    // Is disconnected \\

                    Status.CurrentClient = Status.ClientsConnected[0]; // Set client "0" as current client
                    PrintCurrentSlave(); // Try print
                }
                else
                {
                    // Is connected \\

                    string ip = Status.GetClientIp(Status.CurrentClient.Client); // Get Ip
                    int indx = Status.ClientsConnected.IndexOf(Status.CurrentClient); // Get index in list
                    MasterConsole.ConsoleMessage($"{indx}) {ip}", CONSOLE_MSG.info); // Print id) ip
                }

            }
            else
            {
                MasterConsole.ConsoleMessage("No slaves connected", CONSOLE_MSG.info);
            }


        }

        /// <summary>
        /// Set current client by id
        /// and check if is connected and exists
        /// </summary>
        /// <param name="id">ID of client</param>
        public static void SetCurrentSlave(int id)
        {
            int cnt = Status.ClientsConnected.Count;

            //? Check if is valid id
            if (id < cnt && id >= 0)
            {
                //? check if client selected is connected
                if (Status.CheckConnectedClient(Status.ClientsConnected[id].Client))
                {
                    //! Set new connected client
                    Status.CurrentClient = Status.ClientsConnected[id];
                    MasterConsole.ConsoleMessage($"Current slave changed to: {id}) {Status.GetClientIp(Status.CurrentClient.Client)}", CONSOLE_MSG.success);
                }
                else
                {
                    MasterConsole.ConsoleMessage($"Client {id} is disconnected", CONSOLE_MSG.error);
                    PrintSlaves();
                }

            }
            else
            {
                MasterConsole.ConsoleMessage("Invalid id.", CONSOLE_MSG.error);
            }
        }
    }
}
