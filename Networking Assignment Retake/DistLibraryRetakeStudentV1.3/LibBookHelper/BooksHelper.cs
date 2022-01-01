using System;
using System.Text;
using System.Text.Json;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using LibData;
using Microsoft.Extensions.Configuration;

namespace BookHelperSolution
{
    public struct Setting
    {
        public int BookHelperPortNumber { get; set; }
        public string BookHelperIPAddress { get; set; }
        public int ServerListeningQueue { get; set; }
    }

    abstract class AbsSequentialServerHelper
    {
        protected Setting settings;
        protected string booksDataFile;

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
            Console.Out.WriteLine("[Server Helper] {0} : {1}", type, msg);
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
                settings.BookHelperIPAddress = Config.GetSection("BookHelperIPAddress").Value;
                settings.BookHelperPortNumber = Int32.Parse(Config.GetSection("BookHelperPortNumber").Value);
                settings.ServerListeningQueue = Int32.Parse(Config.GetSection("ServerListeningQueue").Value);
            }
            catch (Exception e) { report("[Exception]", e.Message); }
        }

        protected abstract void loadDataFromJson();
        protected abstract void createSocket();
        public abstract void handelListening();
        protected abstract Message processMessage(Message message);
    }

    class SequentialServerHelper : AbsSequentialServerHelper
    {
        // check all the required parameters for the server. How are they initialized? 
        public Socket listener;
        public IPEndPoint listeningPoint;
        public IPAddress ipAddress;
        public List<BookData> booksList;

        public SequentialServerHelper() : base()
        {
            booksDataFile = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"../../../") + "Books.json");
            GetConfigurationValue();
            loadDataFromJson();
        }

        /// <summary>
        /// This method loads data items provided in booksDataFile into booksList.
        /// </summary>
        protected override void loadDataFromJson()
        {
            //todo: To meet the assignment requirement, implement this method 
            try
            {
                // get path & read file -> into string
                string booksDataFile = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"../../../") + "Books.json");
                string fullBookDataJsonString = System.IO.File.ReadAllText(booksDataFile);

                // load string into BookData object
                this.booksList = JsonSerializer.Deserialize<List<BookData>>(fullBookDataJsonString);
            }
            catch (Exception e){
                Console.WriteLine("error; loadDataFromJson :)( was: {0}", e.Message);
            }
        }

        /// <summary>
        /// This method establishes required socket: listener.
        /// </summary>
        protected override void createSocket()
        {
            //todo: To meet the assignment requirement, implement this method
            try
            {
                // init values
                GetConfigurationValue();
                Console.WriteLine("in createSocket()");

                this.listeningPoint = new IPEndPoint(IPAddress.Parse(settings.BookHelperIPAddress), settings.BookHelperPortNumber);
                this.listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                // connection w LibServer
                this.listener.Bind(this.listeningPoint);
                this.listener.Listen(settings.ServerListeningQueue);
            }
            catch (Exception e){
                Console.WriteLine("error; createSocket :(] was: {0}", e.Message);
            }
        }

        /// <summary>
        /// This method is optional. It delays the execution for a short period of time.
        /// Note: Can be used only for testing purposes.
        /// </summary>
        void delay()
        {
            int m = 10;
            for (int i = 0; i <= m; i++)
            {
                Console.Out.Write("{0} .. ", i);
                Thread.Sleep(200);
            }
            Console.WriteLine("\n");
        }

        /// <summary>
        /// This method handles all the communications with the LibServer.
        /// </summary>
        public override void handelListening()
        {
            //todo: To meet the assignment requirement, finish the implementation of this method 
            while (true) {
                Socket newHelperSocket = null;
                try 
                {
                    createSocket();
                    // setup
                    Console.WriteLine("inside handleListening()");
                    byte[] buffer = new byte[1000];
                    string data = "";
                    newHelperSocket = this.listener.Accept();

                    // recieve BookInquiry msg from LibServer
                    int recievedInt = newHelperSocket.Receive(buffer);
                    data = Encoding.ASCII.GetString(buffer, 0, recievedInt);
                    Message recievedMsg = JsonSerializer.Deserialize<Message>(data);

                    Console.WriteLine("recieved BookInquiry msg; type={0} content={1}", recievedMsg.Type, recievedMsg.Content);

                    // process BookInquiry msg & send BookInquiryReply msg back to LibServer
                    Message inqReplyMsg = processMessage(recievedMsg);
                    byte[] inqReplyMsgSend = Encoding.ASCII.GetBytes(JsonSerializer.Serialize(inqReplyMsg));
                    newHelperSocket.Send(inqReplyMsgSend);

                    Console.WriteLine("BookInquiryReply msg sent :)");

                    // closing socket instance
                    this.listener.Close();
                    newHelperSocket.Close();
                }
                catch(Exception e) {
                    Console.WriteLine("error; handleListening :] was: {0}", e.Message);

                    // closing socket instance
                    this.listener.Close();
                    newHelperSocket.Close();
                }
                Thread.Sleep(1000);
            }
        }

        /// <summary>
        /// Given the message received from the Server, this method processes the message and returns a reply.
        /// </summary>
        /// <param name="message">The message received from the LibServer.</param>
        /// <returns>The message that needs to be sent back as the reply.</returns>
        protected override Message processMessage(Message message)
        {
            Message reply = new Message();
            //todo: To meet the assignment requirement, finish the implementation of this method .
            try
            {
                if (message.Type == MessageType.BookInquiry) {
                    loadDataFromJson();
                    foreach (BookData book in this.booksList) {
                        if (book.Title == message.Content) {
                            // book was found; build BookInquiryReply msg
                            reply.Type = MessageType.BookInquiryReply;
                            reply.Content = JsonSerializer.Serialize(new BookData {
                                Title = book.Title,
                                Author = book.Author,
                                Status = book.Status,
                                BorrowedBy = book.BorrowedBy,
                                ReturnDate = book.ReturnDate
                            });
                            return reply;
                        }
                    }
                    // book was NOT found; build NotFound msg
                    reply.Type = MessageType.NotFound;
                    reply.Content = "Book wasn't found, so Content is not important stop looking immediately :(";
                }
            }
            catch (Exception e) {
                Console.WriteLine("error; processMessage :[] was: {0}", e.Message);
            }
            return reply;
        }
    }
}
