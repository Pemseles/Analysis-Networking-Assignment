using System;
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
        public static string settingsJsonPath = Path.GetFullPath(@"ClientServerConfig.json");
        public static string userJsonPath = Path.GetFullPath(@"Users.json");

        public SequentialServer()
        {
            // listen to client; 
            // if msgtype = Hello; reply w msg(Welcome, [content not important])
            // if msgtype = BookInquiry or UserInquiry; send exact message to BookHelper Server

            // listen to Helper Servers;
            // if msgtype = BookInquiryReply, UserInquiryReply or NotFound; send exact message to client

            // if client_id sending message = -1; end all communications & make Helper Servers end as well

            while (true) {
                int maxByte = newLibServerSock.Receive(buffer);
                data = Encoding.ASCII.GetString(buffer, 0, maxByte);
                Message recievedMsg = JsonConvert.DeserializeObject<Message>(data);

                try {
                    if (recievedMsg.Type == MessageType.Hello) {
                        // client makes first contact w server
                        byte[] welcomingMsg = AssembleMsg(recievedMsg);
                        newLibServerSock.Send(welcomingMsg);

                        // server waits for confirmation
                        int nextHandshake = newLibServerSock.Receive(buffer);
                        string dataNextHandshake = Encoding.ASCII.GetString(buffer, 0, nextHandshake);
                        Message recievedConfirmationMsg = JsonConvert.DeserializeObject<Message>(data);

                        if (recievedConfirmationMsg.Type == MessageType.BookInquiry || recievedConfirmationMsg.Type == MessageType.UserInquiry) {
                            // checks for types it needs to forward to Helper Servers
                            byte[] msgForward = AssembleMsg(recievedMsg);
                            newLibServerSock.Send(msgForward);
                            Console.WriteLine("Forwarded message to Helper Server.");
                        }
                        else if (recievedConfirmationMsg.Type == MessageType.BookInquiryReply || recievedConfirmationMsg.Type == MessageType.UserInquiryReply || recievedConfirmationMsg.Type == MessageType.NotFound) {
                            // checks for types it needs to send back to the client
                            byte[] msgForwardBack = AssembleMsg(recievedMsg);
                            newLibServerSock.Send(msgForwardBack);
                            Console.WriteLine("Forwarded message back to client.");
                        }
                    }
                    else {
                        // error occured
                        byte[] msgNew = AssembleMsg(recievedMsg);
                        newLibServerSock.Send(msgNew);
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
        }

        public void start()
        {
            string fullSettingsJsonStr = System.IO.File.ReadAllText(settingsJsonPath);
            Setting libServerSettings = JsonDeserialize<Setting>(fullSettingsJsonStr);
            
            byte[] buffer = new byte[1000];
            byte[] msg = Encoding.ASCII.GetBytes("From LibServerHelper server: Your message was delivered\n");
            string data = null;

            IPEndPoint localEndpoint = new IPEndPoint(libServerSettings.BookHelperIPAddress, libServerSettings.BookHelperPortNumber);
            Socket libServerSock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            libServerSock.bind(localEndpoint);
            libServerSock.Listen(libServerSettings.ServerListeningQueue);
            Console.WriteLine("\nWaiting for clients...");
            Socket newLibServerSock = libServerSock.Accept();

            SequentialHelper();
        }
    }

}



