using System;
using System.Text;
using System.Text.Json;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using LibData;
using Microsoft.Extensions.Configuration;

namespace LibServerSolution
{
    public struct Setting
    {
        public int ServerPortNumber { get; set; }
        public string ServerIPAddress { get; set; }
        public int BookHelperPortNumber { get; set; }
        public string BookHelperIPAddress { get; set; }
        public int ServerListeningQueue { get; set; }
    }

    abstract class AbsSequentialServer
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
            Console.Out.WriteLine("[Server] {0} : {1}", type, msg);
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
                settings.BookHelperIPAddress = Config.GetSection("BookHelperIPAddress").Value;
                settings.BookHelperPortNumber = Int32.Parse(Config.GetSection("BookHelperPortNumber").Value);
                settings.ServerListeningQueue = Int32.Parse(Config.GetSection("ServerListeningQueue").Value);
                // Console.WriteLine( settings.ServerIPAddress, settings.ServerPortNumber );
            }
            catch (Exception e) { report("[Exception]", e.Message); }
        }

        protected abstract void createSocketAndConnectHelpers();

        public abstract void handelListening();

        protected abstract Message processMessage(Message message);
    
        protected abstract Message requestDataFromHelpers(string msg);
    }

    class SequentialServer : AbsSequentialServer
    {
        // check all the required parameters for the server. How are they initialized? 
        Socket serverSocket;
        IPEndPoint listeningPoint;
        Socket bookHelperSocket;

        public SequentialServer() : base()
        {
            GetConfigurationValue();
        }
        
        /// <summary>
        /// Connect socket settings and connec
        /// </summary>
        protected override void createSocketAndConnectHelpers()
        {
            // todo: To meet the assignment requirement, finish the implementation of this method.
            // Extra Note: If failed to connect to helper. Server should retry 3 times.
            // After the 3d attempt the server starts anyway and listen to incoming messages to clients
           
            try
            {
                // init values
                GetConfigurationValue();
                Console.WriteLine("in createSocketAndConnectHelpers()");

                this.listeningPoint = new IPEndPoint(IPAddress.Parse(settings.ServerIPAddress), settings.ServerPortNumber);
                this.serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                IPEndPoint bookHelperEndpoint = new IPEndPoint(IPAddress.Parse(settings.BookHelperIPAddress), settings.BookHelperPortNumber);
                this.bookHelperSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                // connection w LibClient
                this.serverSocket.Bind(this.listeningPoint);
                this.serverSocket.Listen(settings.ServerListeningQueue);

                // connection w LibBookHelper
                try {
                    this.bookHelperSocket.Connect(bookHelperEndpoint);
                }
                catch {
                    try {
                        Console.WriteLine("Connection with LibBookHelper unsuccessful, tries left: 2");
                        Thread.Sleep(2000);
                        this.bookHelperSocket.Connect(bookHelperEndpoint);
                    }
                    catch {
                        try {
                            Console.WriteLine("Connection with LibBookHelper unsuccessful, tries left: 1");
                            Thread.Sleep(2000);
                            this.bookHelperSocket.Connect(bookHelperEndpoint);
                        }
                        catch (Exception ee) {
                            Console.WriteLine("Connection to LibBookHelper unsuccessful, reason={0}", ee.Message);
                            Thread.Sleep(1000);
                            Console.WriteLine("LibServer starting anyways");
                        }
                    }
                }
            }
            catch (Exception e){
                Console.WriteLine("error; createSocketAndConnectHelpers :< was: {0}", e.Message);
            }
        }

        /// <summary>
        /// This method starts the socketserver after initializion and listents to incoming connections. 
        /// It tries to connect to the book helpers. If it failes to connect to the helper. Server should retry 3 times. 
        /// After the 3d attempt the server starts any way. It listen to clients and waits for incoming messages from clients
        /// </summary>
        public override void handelListening()
        {
            createSocketAndConnectHelpers();
            //todo: To meet the assignment requirement, finish the implementation of this method.

            try
            {
                // setup
                Console.WriteLine("inside handleListening()");
                Console.WriteLine("LibBookHelper status={0}", this.bookHelperSocket.Connected);
                byte[] buffer = new byte[1000];
                string data = "";
                Socket newServerSocket = this.serverSocket.Accept();
                
                // recieve Hello msg from LibClient
                int recievedInt = newServerSocket.Receive(buffer);
                data = Encoding.ASCII.GetString(buffer, 0, recievedInt);
                Message receivedMsg = JsonSerializer.Deserialize<Message>(data);

                Console.WriteLine("recieved Hello msg; type={0} content={1}", receivedMsg.Type, receivedMsg.Content);

                // process Hello msg & send Welcome msg to LibClient
                Message welcomeMsg = processMessage(receivedMsg);
                byte[] welcomeMsgSend = Encoding.ASCII.GetBytes(JsonSerializer.Serialize(welcomeMsg));
                newServerSocket.Send(welcomeMsgSend);

                Console.WriteLine("Welcome msg sent :)");

                // recieve BookInquiry msg from Libclient
                recievedInt = newServerSocket.Receive(buffer);
                data = Encoding.ASCII.GetString(buffer, 0, recievedInt);
                receivedMsg = JsonSerializer.Deserialize<Message>(data);

                Console.WriteLine("recieved BookInquiry msg; type={0} content={1}", receivedMsg.Type, receivedMsg.Content);

                // send BookInquiry msg to LibBookHelper; if not connected: sends error msg instead
                if (this.bookHelperSocket.Connected == true) {
                    byte[] bookInqMsgSend = Encoding.ASCII.GetBytes(JsonSerializer.Serialize(receivedMsg));
                    this.bookHelperSocket.Send(bookInqMsgSend);

                    Console.WriteLine("BookInquiry msg forwarded to LibBookHelper :)");

                    // recieve BookInquiryReply msg from LibBookHelper
                    recievedInt = this.bookHelperSocket.Receive(buffer);
                    data = Encoding.ASCII.GetString(buffer, 0, recievedInt);
                    receivedMsg = JsonSerializer.Deserialize<Message>(data);

                    Console.WriteLine("recieved BookInquiryReply msg; type={0} content={1}", receivedMsg.Type, receivedMsg.Content);

                    // send BookInquiryReply msg to LibClient
                    bookInqMsgSend = Encoding.ASCII.GetBytes(JsonSerializer.Serialize(receivedMsg));
                    newServerSocket.Send(bookInqMsgSend);

                    Console.WriteLine("BookInquiryReply msg forwarded to LibClient :)");
                }
    	        else {
                    // makes & sends Error msg to libClient
                    Console.WriteLine("not connected to LibBookHelper; send Error msg to LibClient instead");
                    Message errorMsg = new Message();
                    errorMsg.Type = MessageType.Error;
                    errorMsg.Content = "not an error technically, just not connected to LibBookHelper so it can't get the requested book :(";
                    byte[] errorMsgSend = Encoding.ASCII.GetBytes(JsonSerializer.Serialize(errorMsg));
                    newServerSocket.Send(errorMsgSend);
                    
                    Console.WriteLine("Error msg sent to LibClient :)");
                }
            }
            catch (Exception e) {
                Console.WriteLine("error; handleListening ;> was: {0}", e.Message);
            }
        }

        /// <summary>
        /// Process the message of the client. Depending on the logic and type and content values in a message it may call 
        /// additional methods such as requestDataFromHelpers().
        /// </summary>
        /// <param name="message"></param>
        protected override Message processMessage(Message message)
        {
            Message pmReply = new Message();
            //todo: To meet the assignment requirement, finish the implementation of this method.
            try
            {
                if (message.Type == MessageType.Hello) {
                    pmReply.Type = MessageType.Welcome;
                    pmReply.Content = "This is a welcome message, Content is not important so don't look :(";
                }
            }
            catch (Exception e) {
                Console.WriteLine("error; processMessage >:( was: {0}", e.Message);
            }

            return pmReply;
        }

        /// <summary>
        /// When data is processed by the server, it may decide to send a message to a book helper to request more data. 
        /// </summary>
        /// <param name="content">Content may contain a different values depending on the message type. For example "a book title"</param>
        /// <returns>Message</returns>
        protected override Message requestDataFromHelpers(string content)
        {
            Message HelperReply = new Message();
            //todo: To meet the assignment requirement, finish the implementation of this method .

            // try
            // {

               
            // }
            // catch () { }
            
            return HelperReply;
        }

        public void delay()
        {
            int m = 10;
            for (int i = 0; i <= m; i++)
            {
                Console.Out.Write("{0} .. ", i);
                Thread.Sleep(200);
            }
            Console.WriteLine("\n");
            //report("round:","next to start");
        }

    }
}

