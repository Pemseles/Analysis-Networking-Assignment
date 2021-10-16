using System;
using System.Text;
using System.Text.Json;
using System.Net;
using System.Net.Sockets;
using System.IO;
using LibData;


namespace LibServer
{
    // Note: Do not change this class.
    public class Setting
    {
        public int ServerPortNumber { get; set; }
        public int BookHelperPortNumber { get; set; }
        public int UserHelperPortNumber { get; set; }
        public string ServerIPAddress { get; set; }
        public string BookHelperIPAddress { get; set; }
        public string UserHelperIPAddress { get; set; }
        public int ServerListeningQueue { get; set; }
    }


    // Note: Complete the implementation of this class. You can adjust the structure of this class. 
    public class SequentialServer
    {
        private string settingsJsonPath;
        private string userJsonPath;
        private string fullSettingsJsonStr;
        private Setting libServerSettings;
        private byte[] buffer;
        private string data;
        private IPEndPoint localEndpoint;
        private Socket libServerSock;

        public SequentialServer()
        {
            settingsJsonPath = Path.GetFullPath(@"ClientServerConfig.json");
            userJsonPath = Path.GetFullPath(@"Users.json");
            fullSettingsJsonStr = System.IO.File.ReadAllText(settingsJsonPath);
            libServerSettings = JsonSerializer.Deserialize<Setting>(fullSettingsJsonStr);
            buffer = new byte[1000];
            data = null;
            localEndpoint = new IPEndPoint((long)Convert.ToDouble(libServerSettings.ServerIPAddress), libServerSettings.ServerPortNumber);
            libServerSock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        public byte[] AssembleMsg(Message msgObj) {
            if (msgObj.Type == MessageType.Hello) {
                // build welcoming message
                Message replyJsonData = new Message {
                    Type = MessageType.Welcome,
                    Content = "not important don't look :("
                };
                string welcomeMsg = JsonSerializer.Serialize(replyJsonData);
                byte[] msgNew = Encoding.ASCII.GetBytes(welcomeMsg);
                return msgNew;
            }
            else if (msgObj.Type == MessageType.BookInquiry || msgObj.Type == MessageType.UserInquiry || msgObj.Type == MessageType.BookInquiryReply || msgObj.Type == MessageType.UserInquiryReply || msgObj.Type == MessageType.NotFound) {
                // remake same message
                Message replyJsonData = new Message {
                    Type = msgObj.Type,
                    Content = msgObj.Content
                };
                string forwardMsg = JsonSerializer.Serialize(replyJsonData);
                byte[] msgNew = Encoding.ASCII.GetBytes(forwardMsg);
                return msgNew;
            }
            else {
                // catch error
                Message replyJsonData = new Message {
                    Type = MessageType.Error,
                    Content = "Error occured"
                };
                string errorMsg = JsonSerializer.Serialize(replyJsonData);
                byte[] msgNew = Encoding.ASCII.GetBytes(errorMsg);
                return msgNew;
            }
        }

        public void start()
        {
            // listen to client; 
            // if msgtype = Hello; reply w msg(Welcome, [content not important])
            // if msgtype = BookInquiry or UserInquiry; send exact message to BookHelper Server

            // listen to Helper Servers;
            // if msgtype = BookInquiryReply, UserInquiryReply or NotFound; send exact message to client

            // if client_id sending message = -1; end all communications & make Helper Servers end as well

            libServerSock.Bind(localEndpoint);
            libServerSock.Listen(libServerSettings.ServerListeningQueue);
            Console.WriteLine("\nWaiting for clients...");
            Socket newLibServerSock = libServerSock.Accept();

            while (true) {
                int maxByte = newLibServerSock.Receive(buffer);
                data = Encoding.ASCII.GetString(buffer, 0, maxByte);
                Message recievedMsg = JsonSerializer.Deserialize<Message>(data);

                try {
                    Socket forwardingSock = null;

                    if (recievedMsg.Type == MessageType.Hello) {
                        // client makes first contact w server
                        byte[] welcomingMsg = AssembleMsg(recievedMsg);
                        newLibServerSock.Send(welcomingMsg);

                        // server waits for confirmation
                        int nextHandshake = newLibServerSock.Receive(buffer);
                        string dataNextHandshake = Encoding.ASCII.GetString(buffer, 0, nextHandshake);
                        Message recievedConfirmationMsg = JsonSerializer.Deserialize<Message>(data);

                        if (recievedConfirmationMsg.Type == MessageType.BookInquiry || recievedConfirmationMsg.Type == MessageType.UserInquiry) {
                            // checks for types it needs to forward to Helper Servers
                            byte[] msgForward = AssembleMsg(recievedMsg);
                            string fullSettingsJsonStr = System.IO.File.ReadAllText(settingsJsonPath);
                            Setting libServerSettings = JsonSerializer.Deserialize<Setting>(fullSettingsJsonStr);

                            byte[] buffer = new byte[1000];

                            if (recievedConfirmationMsg.Type == MessageType.BookInquiry) {
                                // establish connection w BookHelper Server
                                IPEndPoint BookhelperEndpoint = new IPEndPoint((long)Convert.ToDouble(libServerSettings.BookHelperIPAddress), libServerSettings.BookHelperPortNumber);
                                forwardingSock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                                forwardingSock.Connect(BookhelperEndpoint);
                            }
                            else if (recievedConfirmationMsg.Type == MessageType.UserInquiry) {
                                // establish connection w UserHelper Server
                                IPEndPoint UserhelperEndpoint = new IPEndPoint((long)Convert.ToDouble(libServerSettings.UserHelperIPAddress), libServerSettings.UserHelperPortNumber);
                                forwardingSock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                                forwardingSock.Connect(UserhelperEndpoint);
                            }

                            forwardingSock.Send(msgForward);
                            Console.WriteLine("Forwarded message to Helper Server.");
                        }
                        else if (recievedConfirmationMsg.Type == MessageType.BookInquiryReply || recievedConfirmationMsg.Type == MessageType.UserInquiryReply || recievedConfirmationMsg.Type == MessageType.NotFound) {
                            // checks for types it needs to send back to the client
                            byte[] msgForwardBack = AssembleMsg(recievedMsg);
                            forwardingSock.Send(msgForwardBack);
                            Console.WriteLine("Forwarded message back to client.");
                        }
                    }
                    else {
                        // error occured
                        byte[] msgNew = AssembleMsg(recievedMsg);
                        forwardingSock.Send(msgNew);
                        Console.WriteLine("Error");
                    }
                }
                catch {
                    // error occured
                    byte[] msgNew = AssembleMsg(recievedMsg);
                    newLibServerSock.Send(msgNew);
                    Console.WriteLine("Error");
                }
            }
        }
    }
}