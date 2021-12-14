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

                                if (recievedConfirmationMsg.Type == MessageType.BookInquiry) {
                                    // checks for types it needs to forward to Helper Servers
                                    // exchanges info w helper servers & back to client
                                    byte[] msgForward = AssembleMsg(recievedConfirmationMsg, false);

                                    // establish connection w BookHelper Server
                                    IPEndPoint bookhelperEndpoint = new IPEndPoint(IPAddress.Parse(libServerSettings.BookHelperIPAddress), libServerSettings.BookHelperPortNumber);
                                    forwardingSock1 = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                                    forwardingSock1.Connect(bookhelperEndpoint);

                                    // forwards msg to bookhelper
                                    forwardingSock1.Send(msgForward);
                                    
                                    int replyInt = forwardingSock1.Receive(buffer);
                                    string dataInquiryReply = Encoding.ASCII.GetString(buffer, 0, replyInt);
                                    Message inquiryReplyMsg = JsonSerializer.Deserialize<Message>(dataInquiryReply);
                                    BookData bookInquiryData = null;

                                    if (inquiryReplyMsg.Type != MessageType.NotFound) {
                                        bookInquiryData = JsonSerializer.Deserialize<BookData>(inquiryReplyMsg.Content);
                                    }
                                    
                                    byte[] msgForwardBack = AssembleMsg(inquiryReplyMsg, false);
                                    newLibServerSock.Send(msgForwardBack);
                                    if (inquiryReplyMsg.Type == MessageType.NotFound || bookInquiryData.Status == "Available") {
                                        // client will not send userinquiry
                                        break;
                                    }
                                    else {
                                        // client needs to send userinquiry, expecting msg
                                        int userInquiryFromClientInt = newLibServerSock.Receive(buffer);
                                        string dataUserInquiry = Encoding.ASCII.GetString(buffer, 0, userInquiryFromClientInt);
                                        Message userInquiryMsg = JsonSerializer.Deserialize<Message>(dataUserInquiry);

                                        // establish connection w userhelper
                                        IPEndPoint userhelperEndpoint = new IPEndPoint(IPAddress.Parse(libServerSettings.UserHelperIPAddress), libServerSettings.UserHelperPortNumber);
                                        forwardingSock2 = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                                        forwardingSock2.Connect(userhelperEndpoint);

                                        // make msg to forward to userhelper & forward it
                                        byte[] msgForwardToUserhelper = AssembleMsg(userInquiryMsg, false);
                                        forwardingSock2.Send(msgForwardToUserhelper);

                                        // recieve msgreply to userinquiry from userhelper
                                        int replyInt2 = forwardingSock2.Receive(buffer);
                                        string userInquiryReplyData = Encoding.ASCII.GetString(buffer, 0, replyInt2);
                                        Message userInquiryReply = JsonSerializer.Deserialize<Message>(userInquiryReplyData);

                                        // forward reply to userinquiry back to client;
                                        byte[] msgForwardBackAgain = AssembleMsg(userInquiryReply, false);
                                        newLibServerSock.Send(msgForwardBackAgain);
                                        
                                        forwardingSock2.Close();            
                                        break;
                                    }
                                }
                                else if (recievedConfirmationMsg.Type == MessageType.EndCommunication) {
                                    // turns off entire system, first send ending msg to helper servers then close itself
                                    byte[] msgEndCommunications = AssembleMsg(recievedMsg, true);
                                    byte[] buffer = new byte[1000];
                                    
                                    // establish connection & send ending msg to bookhelper
                                    IPEndPoint BookhelperEndpoint = new IPEndPoint(IPAddress.Parse(libServerSettings.BookHelperIPAddress), libServerSettings.BookHelperPortNumber);
                                    Socket endingSock1 = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                                    endingSock1.Connect(BookhelperEndpoint);
                                    endingSock1.Send(msgEndCommunications);
                                    endingSock1.Close();

                                    // establish connection & send ending msg to userhelper
                                    IPEndPoint UserhelperEndpoint = new IPEndPoint(IPAddress.Parse(libServerSettings.UserHelperIPAddress), libServerSettings.UserHelperPortNumber);
                                    Socket endingSock2 = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                                    endingSock2.Connect(UserhelperEndpoint);
                                    endingSock2.Send(msgEndCommunications);
                                    endingSock2.Close();

                                    // end libserver
                                    newLibServerSock.Close();
                                    libServerSock.Close();
                                    break;
                                }
                                else {
                                    // go here if client sends wrong msg type
                                    newLibServerSock.Send(Encoding.ASCII.GetBytes("Message type rejected by server, please try again."));
                                }
                            }
                            if (recievedConfirmationMsg.Type != MessageType.EndCommunication) {
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
                        Console.WriteLine("Error occured in code, this message is for debugging purposes only");
                        newLibServerSock.Close();
                        libServerSock.Close();
                        break;
                    }
                }
                catch {
                    Console.WriteLine("Failed to make connection with client.");
                } 
            }
        }
    }
}