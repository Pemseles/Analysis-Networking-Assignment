using System.Linq;
using System;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading;
// using LibData;
using Microsoft.Extensions.Configuration;

// made by: 1008331 - INF2E

namespace LibClient
{
    public struct Setting
    {
        public int ServerPortNumber { get; set; }
        public string ServerIPAddress { get; set; }
    }

    public class Output
    {
        public string Client_id { get; set; } // the id of the client that requests the book
        public string BookName { get; set; } // the name of the book to be reqyested
        public string Status { get; set; } // final status received from the server
        public string Error { get; set; } // True if errors received from the server
        public string BorrowerName { get; set; } // the name of the borrower in case the status is borrowed, otherwise null
        public string ReturnDate { get; set; } // the email of the borrower in case the status is borrowed, otherwise null
    }

    abstract class AbsSequentialClient
    {
        protected Setting settings;

        /// <summary>
        /// Report method can be used to print message to console in standaard formaat. 
        /// It is not mandatory to use it, but highly recommended.
        /// </summary>
        /// <param name="type">For example: [Exception], [Error], [Info] etc</param>
        /// <param name="msg"> In case of [Exception] the message of the exection can be passed. Same is valud for other types</param>

        protected void report(string type, string msg)
        {
            // Console.Clear();
            Console.Out.WriteLine(">>>>>>>>>>>>>>>>>>>>>>>>>");
            if (!String.IsNullOrEmpty(msg))
            {
                msg = msg.Replace(@"\u0022", " ");
            }

            Console.Out.WriteLine("[Client] {0} : {1}", type, msg);
        }

        /// <summary>
        /// This methid loads required settings.
        /// </summary>
        protected void GetConfigurationValue()
        {
            settings = new Setting();
            try
            {
                string path = AppDomain.CurrentDomain.BaseDirectory;
                IConfiguration Config = new ConfigurationBuilder()
                    .SetBasePath(Path.GetFullPath(Path.Combine(path, @"../../../../")))
                    .AddJsonFile("appsettings.json")
                    .Build();

                settings.ServerIPAddress = Config.GetSection("ServerIPAddress").Value;
                settings.ServerPortNumber = Int32.Parse(Config.GetSection("ServerPortNumber").Value);
                // settings.ServerListeningQueue = Int32.Parse(Config.GetSection("ServerListeningQueue").Value);
            }
            catch (Exception e) { report("[Exception]", e.Message); }
        }

        protected abstract void createSocketAndConnect();
        public abstract Output handleConntectionAndMessagesToServer();
        protected abstract Message processMessage(Message message);

    }

    class SequentialClient : AbsSequentialClient
    {
        public Output result;
        public Socket clientSocket;
        public IPEndPoint serverEndPoint;
        public IPAddress ipAddress;

        public string client_id;
        private string bookName;

        //This field is optional to use. 
        private int delayTime;
        /// <summary>
        /// Initializes the client based on the given parameters and seeting file.
        /// </summary>
        /// <param name="id">id of the clients provided by the simulator</param>
        /// <param name="bookName">name of the book to be requested from the server, provided by the simulator</param>
        public SequentialClient(int id, string bookName)
        {
            GetConfigurationValue();

            this.delayTime = 100;
            this.bookName = bookName;
            this.client_id = "Client " + id.ToString();
            this.result = new Output();
            result.Client_id = this.client_id;
        }


        /// <summary>
        /// Optional method. Can be used for testing to delay the output time.
        /// </summary>
        public void delay()
        {
            int m = 10;
            for (int i = 0; i <= m; i++)
            {
                Console.Out.Write("{0} .. ", i);
                Thread.Sleep(delayTime);
            }
            Console.WriteLine("\n");
        }

        /// <summary>
        /// Connect socket settings and connect to the helpers.
        /// </summary>
        protected override void createSocketAndConnect()
        {
            //todo: To meet the assignment requirement, finish the implementation of this method.
            try
            {
                // init of values
                GetConfigurationValue();
                Console.WriteLine("in createSocketAndConnect()");
                this.serverEndPoint = new IPEndPoint(IPAddress.Parse(settings.ServerIPAddress), settings.ServerPortNumber);
                this.clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                // connection w LibServer 
                this.clientSocket.Connect(this.serverEndPoint);
            }
            catch (Exception e)
            {
                Console.WriteLine("error occured; createSocketAndConnect :) was: {0}", e.Message);
            }
        }

        /// <summary>
        /// This method starts the socketserver after initializion and handles all the communications with the server. 
        /// Note: The signature of this method must not change.
        /// </summary>
        /// <returns>The final result of the request that will be written to output file</returns>
        public override Output handleConntectionAndMessagesToServer()
        {
            this.report("starting:", this.client_id + " ; " + this.bookName);
            createSocketAndConnect();

            //todo: To meet the assignment requirement, finish the implementation of this method.
            try
            {
                Console.WriteLine("inside HandleConnection");
                byte[] buffer = new byte[1000];
                string data = "";

                // first handshake; sending Hello msg
                Message helloMsg = new Message();
                helloMsg.Type = MessageType.Hello;
                helloMsg.Content = this.client_id;

                byte[] helloMsgSend = Encoding.ASCII.GetBytes(JsonSerializer.Serialize(helloMsg));
                this.clientSocket.Send(helloMsgSend);
                
                Console.WriteLine("Hello msg sent :)");

                // recieve Welcome msg from LibServer
                int recievedInt = this.clientSocket.Receive(buffer);
                data = Encoding.ASCII.GetString(buffer, 0, recievedInt);
                Message recievedMsg = JsonSerializer.Deserialize<Message>(data);

                Console.WriteLine("recieved Welcome msg; type={0} content={1}", recievedMsg.Type, recievedMsg.Content);

                // process Welcome msg & send BookInquiry to LibServer
                Message bookInqMsg = processMessage(recievedMsg);
                byte[] bookInqMsgSend = Encoding.ASCII.GetBytes(JsonSerializer.Serialize(bookInqMsg));
                this.clientSocket.Send(bookInqMsgSend);

                Console.WriteLine("BookInq msg sent :))");
            }
            catch (Exception e) {
                Console.WriteLine("error; handleConnectionAndMessagesToServer ;-; was: {0}", e.Message);
            }
            return this.result;
        }

        /// <summary>
        /// Process the messages of the server. Depending on the logic, type and content of a message the client may return different message values.
        /// </summary>
        /// <param name="message">Received message to be processed</param>
        /// <returns>The message that needs to be sent back as the reply.</returns>
        protected override Message processMessage(Message message)
        {
            Message processedMsgResult = new Message();
            //todo: To meet the assignment requirement, finish the implementation of this method.
            try
            {
                if (message.Type == MessageType.Welcome) {
                    processedMsgResult.Type = MessageType.BookInquiry;
                    processedMsgResult.Content = this.bookName;
                }
            }
            catch (Exception e) {
                Console.WriteLine("error; processMessage >:( was: {0}", e.Message);
            }
            return processedMsgResult;
        }
    }
}

