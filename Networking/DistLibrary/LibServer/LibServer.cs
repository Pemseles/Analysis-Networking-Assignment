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
        private string fullSettingsJsonStr;
        private Setting libServerSettings;
        private byte[] buffer;
        private string data;
        private IPEndPoint localEndpoint;
        private Socket libServerSock;

        public SequentialServer()
        {
            settingsJsonPath = @"../ClientServerConfig.json";
            fullSettingsJsonStr = System.IO.File.ReadAllText(settingsJsonPath);
            libServerSettings = JsonSerializer.Deserialize<Setting>(fullSettingsJsonStr);
            buffer = new byte[1000];
            data = null;
            localEndpoint = new IPEndPoint(IPAddress.Parse(libServerSettings.ServerIPAddress), libServerSettings.ServerPortNumber);
            libServerSock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        public byte[] AssembleMsg(Message msgObj, bool nullCheck) {
            if (msgObj.Type == MessageType.Hello && nullCheck == false) {
                // build welcoming message
                Message replyJsonData = new Message {
                    Type = MessageType.Welcome,
                    Content = "not important don't look :("
                };
                string welcomeMsg = JsonSerializer.Serialize(replyJsonData);
                byte[] msgNew = Encoding.ASCII.GetBytes(welcomeMsg);
                return msgNew;
            }
            else if ((msgObj.Type == MessageType.BookInquiry || msgObj.Type == MessageType.UserInquiry || msgObj.Type == MessageType.BookInquiryReply || msgObj.Type == MessageType.UserInquiryReply || msgObj.Type == MessageType.NotFound) && nullCheck == false) {
                // remake same message
                Message replyJsonData = new Message {
                    Type = msgObj.Type,
                    Content = msgObj.Content
                };
                string forwardMsg = JsonSerializer.Serialize(replyJsonData);
                byte[] msgNew = Encoding.ASCII.GetBytes(forwardMsg);
                return msgNew;
            }
            else if (nullCheck == true) {
                // make ending msg
                Console.WriteLine("trying to make ending msg");
                Message replyJsonData = new Message {
                    Type = MessageType.EndCommunication,
                    Content = "not important don't look part 2 the remix :("
                };
                string endingMsg = JsonSerializer.Serialize(replyJsonData);
                byte[] msgNew = Encoding.ASCII.GetBytes(endingMsg);
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
            while (true) {
                try {
                    libServerSock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    libServerSock.Bind(localEndpoint);
                    libServerSock.Listen(libServerSettings.ServerListeningQueue);
                    Console.WriteLine("\nWaiting for clients...");
                    Socket newLibServerSock = libServerSock.Accept();

                    int maxByte = newLibServerSock.Receive(buffer);
                    data = Encoding.ASCII.GetString(buffer, 0, maxByte);
                    Message recievedMsg = JsonSerializer.Deserialize<Message>(data);
                    Console.WriteLine("line 110; recieved msg from client, type = {0}", recievedMsg.Type);

                    try {
                        Socket forwardingSock1 = null;
                        Socket forwardingSock2 = null;
                        Message recievedConfirmationMsg = null;

                        if (recievedMsg.Type == MessageType.Hello) {
                            // client makes first contact w server
                            byte[] welcomingMsg = AssembleMsg(recievedMsg, false);
                            newLibServerSock.Send(welcomingMsg);

                            while (true) {
                                // server waits for confirmation
                                int nextHandshake = newLibServerSock.Receive(buffer);
                                string dataNextHandshake = Encoding.ASCII.GetString(buffer, 0, nextHandshake);
                                recievedConfirmationMsg = JsonSerializer.Deserialize<Message>(dataNextHandshake);
                                Console.WriteLine("recieved 2nd msg from client, type: {0}", recievedConfirmationMsg.Type);

                                if (recievedConfirmationMsg.Type == MessageType.BookInquiry) {
                                    // checks for types it needs to forward to Helper Servers
                                    // exchanges info w helper servers & back to client
                                    byte[] msgForward = AssembleMsg(recievedConfirmationMsg, false);

                                    // establish connection w BookHelper Server
                                    IPEndPoint bookhelperEndpoint = new IPEndPoint(IPAddress.Parse(libServerSettings.BookHelperIPAddress), libServerSettings.BookHelperPortNumber);
                                    forwardingSock1 = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                                    forwardingSock1.Connect(bookhelperEndpoint);
                                    Console.WriteLine("line 137; connected to bookhelper");

                                    // forwards msg to bookhelper
                                    forwardingSock1.Send(msgForward);
                                    Console.WriteLine("Forwarded message to bookHelper Server, awaiting reply");
                                    
                                    int replyInt = forwardingSock1.Receive(buffer);
                                    Console.WriteLine("replyint gotten");
                                    string dataInquiryReply = Encoding.ASCII.GetString(buffer, 0, replyInt);
                                    Console.WriteLine("replyjsonstring gotten: {0}", dataInquiryReply);
                                    Message inquiryReplyMsg = JsonSerializer.Deserialize<Message>(dataInquiryReply);
                                    Console.WriteLine("msg obj deserialized, type = {0} & content = {1}", inquiryReplyMsg.Type, inquiryReplyMsg.Content);
                                    BookData bookInquiryData = null;
                                    Console.WriteLine("bookdata made null");

                                    if (inquiryReplyMsg.Type != MessageType.NotFound) {
                                        bookInquiryData = JsonSerializer.Deserialize<BookData>(inquiryReplyMsg.Content);
                                        Console.WriteLine("content of new msg obj deserialised into bookdata obj");
                                        Console.WriteLine("Recieved reply from bookhelper server, type: {0} & status = {1}", inquiryReplyMsg.Type, bookInquiryData.Status);
                                    }
                                    else {
                                        // REMOVE ELSE AFTER ERROR IS TRACED
                                        Console.WriteLine("recieved reply; book was not found");
                                    }
                                    
                                    
                                    byte[] msgForwardBack = AssembleMsg(inquiryReplyMsg, false);
                                    newLibServerSock.Send(msgForwardBack);
                                    Console.WriteLine("reply sent back to client");

                                    if (inquiryReplyMsg.Type == MessageType.NotFound || bookInquiryData.Status == "Available") {
                                        // client will not send userinquiry
                                        Console.WriteLine("client does not need to send userinquiry, ending relevant sockets");
                                        break;
                                    }
                                    else {
                                        // client needs to send userinquiry, expecting msg
                                        int userInquiryFromClientInt = newLibServerSock.Receive(buffer);
                                        string dataUserInquiry = Encoding.ASCII.GetString(buffer, 0, userInquiryFromClientInt);
                                        Message userInquiryMsg = JsonSerializer.Deserialize<Message>(dataUserInquiry);
                                        Console.WriteLine("Recieved user inquiry from client, establishing & forwarding to userhelper");

                                        // establish connection w userhelper
                                        IPEndPoint userhelperEndpoint = new IPEndPoint(IPAddress.Parse(libServerSettings.UserHelperIPAddress), libServerSettings.UserHelperPortNumber);
                                        forwardingSock2 = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                                        forwardingSock2.Connect(userhelperEndpoint);
                                        Console.WriteLine("connected to userhelper, forwarding & recieving msg");

                                        // make msg to forward to userhelper & forward it
                                        byte[] msgForwardToUserhelper = AssembleMsg(userInquiryMsg, false);
                                        forwardingSock2.Send(msgForwardToUserhelper);
                                        Console.WriteLine("sent userinquiry to userhelper, expecting reply");

                                        // recieve msgreply to userinquiry from userhelper
                                        int replyInt2 = forwardingSock2.Receive(buffer);
                                        string userInquiryReplyData = Encoding.ASCII.GetString(buffer, 0, replyInt2);
                                        Message userInquiryReply = JsonSerializer.Deserialize<Message>(userInquiryReplyData);
                                        Console.WriteLine("recieved reply from userhelper; type {0}", userInquiryReply.Type);

                                        // forward reply to userinquiry back to client
                                        byte[] msgForwardBackAgain = AssembleMsg(userInquiryReply, false);
                                        newLibServerSock.Send(msgForwardBackAgain);  
                                        forwardingSock2.Close();            
                                        break;
                                    }
                                }
                                else if (recievedConfirmationMsg.Type == MessageType.EndCommunication) {
                                    // turns off entire system, first send ending msg to helper servers then close itself
                                    Console.WriteLine("User ended communication; proceeding to close all sockets");
                                    byte[] msgEndCommunications = AssembleMsg(recievedMsg, true);
                                    byte[] buffer = new byte[1000];
                                    Console.WriteLine("assembled ending msg for helper servers");
                                    
                                    // establish connection & send ending msg to bookhelper
                                    Console.WriteLine("establishing connection w bookhelper");
                                    IPEndPoint BookhelperEndpoint = new IPEndPoint(IPAddress.Parse(libServerSettings.BookHelperIPAddress), libServerSettings.BookHelperPortNumber);
                                    Socket endingSock1 = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                                    endingSock1.Connect(BookhelperEndpoint);
                                    endingSock1.Send(msgEndCommunications);
                                    Console.WriteLine("Ending message sent to BookHelper.");
                                    endingSock1.Close();

                                    // establish connection & send ending msg to userhelper
                                    Console.WriteLine("establishing connection w userhelper");
                                    IPEndPoint UserhelperEndpoint = new IPEndPoint(IPAddress.Parse(libServerSettings.UserHelperIPAddress), libServerSettings.UserHelperPortNumber);
                                    Socket endingSock2 = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                                    endingSock2.Connect(UserhelperEndpoint);
                                    endingSock2.Send(msgEndCommunications);
                                    Console.WriteLine("Ending message sent to UserHelper.");
                                    endingSock2.Close();

                                    // end libserver
                                    Console.WriteLine("Operations complete, ending LibServer...");
                                    newLibServerSock.Close();
                                    libServerSock.Close();
                                    break;
                                }
                                else {
                                    // go here if client sends wrong msg type
                                    Console.WriteLine("Client sent wrong msg Type, should be an inquiry/ending msg, not: {0}", recievedConfirmationMsg.Type);
                                    newLibServerSock.Send(Encoding.ASCII.GetBytes("Message type rejected by server, please try again."));
                                }
                            }
                            if (recievedConfirmationMsg.Type != MessageType.EndCommunication) {
                                Console.WriteLine("Finished operations for current client");
                                forwardingSock1.Close();
                            }
                            else {
                                break;
                            }
                        }
                        else {
                            // error occured
                            byte[] msgNew = AssembleMsg(recievedMsg, false);
                            forwardingSock1.Send(msgNew);
                            Console.WriteLine("Error, client did not send type hello, instead sent type {0}", recievedMsg.Type);
                        }
                        if (recievedConfirmationMsg.Type != MessageType.EndCommunication) {
                            newLibServerSock.Close();
                            libServerSock.Close();
                        }
                    }
                    catch {
                        // error occured
                        byte[] msgNew = AssembleMsg(recievedMsg, false);
                        newLibServerSock.Send(msgNew);
                        Console.WriteLine("error Something went wrong somewhere idk where lmao");
                        newLibServerSock.Close();
                        libServerSock.Close();
                        break;
                    }
                }
                catch {
                    Console.WriteLine("failed to make connection with client");
                } 
            }
        }
    }
}