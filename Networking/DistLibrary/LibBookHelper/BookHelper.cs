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
        public static string settingsJsonPath = Path.GetFullPath(@"ClientServerConfig.json");
        public static string booksJsonPath = Path.GetFullPath(@"Books.json");

        public static void SequentialHelper()
        {
            while (true) {
                int maxByte = newBookSock.Receive(buffer);
                data = Encoding.ASCII.GetString(buffer, 0, maxByte);

                try {
                    string fullBooksJsonStr = System.IO.File.ReadAllText(booksJsonPath);
                    var tempBookList = JsonConvert.DeserializeObject<List<BookData>>(fullBooksJsonStr);
                    
                    if (data.Type == MessageType.BookInquiryReply && fullBooksJsonStr.Contains(requestedBook.Title.ToLower())) {
                        // book was found; does not matter if borrowed or not, handled on client-side
                        foreach (BookData book in tempBookList)
                        {
                            if (book.Title == data.Content) {
                                BookData requestedBook = new BookData(book.Title, book.Author, book.Status, book.BorrowedBy, book.ReturnDate);
                            }
                        }

                        byte[] msgNew = AssembleMsg(data.Type);
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
                };
                string replyBookNotFound = JsonSerializer.Serialize(replyJsonData);
                byte[] msgNew = Encoding.ASCII.GetBytes(replyBookNotFound);
                return msgNew;
                
            }
            else if (messageTypeEnum == MessageType.BookInquiryReply) {
                // build message if book found
                Message replyJsonData = new Message {
                    Type = MessageType.BookInquiryReply,
                    Content = requestedBook
                };
                string replyBookFound = JsonSerializer.Serialize(replyJsonData);
                byte[] msgNew = Encoding.ASCII.GetBytes(replyBookFound);
                return msgNew;  
            }
            else if (messageTypeEnum == MessageType.Error) {
                // build message if error occured
                Message replyJsonData = new Message {
                    Type = MessageType.Error,
                    Content = "From Bookhelper server: error occured during booksearch\n"
                };
                string replyError = JsonSerializer.Serialize(replyJsonData);
                byte[] msgNew = Encoding.ASCII.GetBytes(replyError);
                return msgNew;
            }
        }

        public static void start()
        {
            string fullSettingsJsonStr = System.IO.File.ReadAllText(settingsJsonPath);
            Setting bookHelperSettings = JsonDeserialize<Setting>(fullSettingsJsonStr);
            
            byte[] buffer = new byte[1000];
            byte[] msg = Encoding.ASCII.GetBytes("From BookHelper server: Your message was delivered\n");
            string data = null;

            IPEndPoint localEndpoint = new IPEndPoint(bookHelperSettings.BookHelperIPAddress, bookHelperSettings.BookHelperPortNumber);
            Socket bookSock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            bookSock.bind(localEndpoint);
            bookSock.Listen(bookHelperSettings.ServerListeningQueue);
            Console.WriteLine("\nWaiting for main server...");
            Socket newBookSock = bookSock.Accept();

            SequentialHelper();
        }
    }
}