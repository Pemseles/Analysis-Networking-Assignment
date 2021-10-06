using System;
using System.Text.Json;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Collections.Generic;
using LibData;

namespace BookHelper
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
    public class SequentialHelper
    {
        public static string booksJsonPath = Path.GetFullPath(@"Books.json");

        public void SequentialHelper()
        {
            while (true) {
                int maxByte = newBookSock.Receive(buffer);
                data = Encoding.ASCII.GetString(buffer, 0, maxByte);

                try {
                    string fullBooksJsonStr = System.IO.File.ReadAllText(booksJsonPath);
                    BookData requestedBook = JsonDeserialize<BookData>(data);

                    if (fullBooksJsonStr.Contains(requestedBook.Title.ToLower())) {
                        // book was found; does not matter if borrowed or not, handled on client-side
                        byte[] msgNew = AssembleMsg(MessageType.BookInquiryReply);
                        newBookSock.Send(msgNew);
                    }
                    else {
                        // book was not found; sends back message 
                        byte[] msgNew = AssembleMsg(MessageType.NotFound);
                        newBookSock.Send(msgNew);
                    }
                }
                catch {
                    // error during message recieving (not serialised etc)
                    byte[] msgNew = AssembleMsg(MessageType.Error);
                    newBookSock.Send(msgNew);
                    Console.WriteLine("Error");
                }
            }
        }

        public static byte[] AssembleMsg(enum messageTypeEnum) {
            if (messageTypeEnum == MessageType.NotFound) {
                // build message if book not found
                Message replyJsonData = new Message {
                    Type = MessageType.NotFound,
                    Content = "From BookHelper server: Book not found\n"
                }
                string replyBookNotFound = JsonSerializer.Serialize(replyJsonData);
                byte[] msgNew = Encoding.ASCII.GetBytes(replyBookNotFound);
                return msgNew;
                
            }
            else if (messageTypeEnum == MessageType.BookInquiryReply) {
                // build message if book found
                Message replyJsonData = new Message {
                    Type = MessageType.BookInquiryReply,
                    Content = requestedBook
                }
                string replyBookFound = JsonSerializer.Serialize(replyJsonData);
                byte[] msgNew = Encoding.ASCII.GetBytes(replyBookFound);
                return msgNew;
            }
            else if (messageTypeEnum == MessageType.Error) {
                // build message if error occured
                Message replyJsonData = new Message {
                    Type = MessageType.Error,
                    Content = "From Bookhelper server: error occured during booksearch\n"
                }
                string replyError = JsonSerializer.Serialize(replyJsonData);
                byte[] msgNew = Encoding.ASCII.GetBytes(replyError);
                return msgNew;
            }
        }

        public void start()
        {
            string ip = "127.0.0.1";
            int port = 32000;
            
            byte[] buffer = new byte[1000];
            byte[] msg = Encoding.ASCII.GetBytes("From BookHelper server: Your message was delivered\n");
            string data = null;

            IPAddress ipAddress = IPAddress.Parse(ip);
            IPEndPoint localEndpoint = new IPEndPoint(ipAddress, port);

            Socket bookSock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            bookSock.bind(localEndpoint);
            sock.Listen(5);
            Console.WriteLine("\nWaiting for main server...");
            Socket newBookSock = bookSock.Accept();

            SequentialHelper();
        }
    }
}