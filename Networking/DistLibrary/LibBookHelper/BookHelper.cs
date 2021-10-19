using System;
using System.Text;
using System.Text.Json;
using System.Net;
using System.Net.Sockets;
using System.IO;
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
        public string settingsJsonPath;
        public string booksJsonPath;
        public string fullSettingsJsonStr;
        public Setting bookHelperSettings;
        public byte[] buffer;
        public string data;
        public IPEndPoint localEndpoint;
        public Socket bookSock;

        public SequentialHelper()
        {
            settingsJsonPath = @"../ClientServerConfig.json";
            booksJsonPath = Path.GetFullPath(@"Books.json");
            fullSettingsJsonStr = System.IO.File.ReadAllText(settingsJsonPath);
            bookHelperSettings = JsonSerializer.Deserialize<Setting>(fullSettingsJsonStr);
            buffer = new byte[1000];
            data = null;
            localEndpoint = new IPEndPoint(IPAddress.Parse(bookHelperSettings.BookHelperIPAddress), bookHelperSettings.BookHelperPortNumber);
            bookSock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }
            
        private BookData GetBookData(Message recievedMsg, string bookJsonStr) {
            BookData[] tempBookArray = JsonSerializer.Deserialize<BookData[]>(bookJsonStr);
            Console.WriteLine("searching for: {0}, full jsonstring: {1}", recievedMsg.Content, bookJsonStr);
            foreach (BookData book in tempBookArray)
            {
                Console.WriteLine("current book: {0}", book.Title);
                if (book.Title == recievedMsg.Content) {
                    BookData requestedBook = new BookData {
                        Title = book.Title,
                        Author = book.Author,
                        Status = book.Status,
                        BorrowedBy = book.BorrowedBy,
                        ReturnDate = book.ReturnDate
                    };
                    return requestedBook;
                }
            }
            return null;
        }

        public byte[] AssembleMsg(Message msgObj, BookData requestedBook) {
            if (requestedBook == null && msgObj != null) {
                // build message if book not found
                Message replyJsonData = new Message {
                    Type = MessageType.NotFound,
                    Content = "From BookHelper server: Book not found\n"
                };
                string replyBookNotFound = JsonSerializer.Serialize(replyJsonData);
                byte[] msgNew = Encoding.ASCII.GetBytes(replyBookNotFound);
                return msgNew;
                
            }
            else if (requestedBook != null) {
                // build message if book found
                Message replyJsonData = new Message {
                    Type = MessageType.BookInquiryReply,
                    Content = JsonSerializer.Serialize(requestedBook)
                };
                string replyBookFound = JsonSerializer.Serialize(replyJsonData);
                byte[] msgNew = Encoding.ASCII.GetBytes(replyBookFound);
                return msgNew;  
            }
            else {
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

        public void start()
        {
            while (true) {
                try {
                    byte[] msgNew = null;

                    bookSock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    bookSock.Bind(localEndpoint);
                    bookSock.Listen(bookHelperSettings.ServerListeningQueue);
                    Console.WriteLine("\nWaiting for main server...");
                    Socket newBookSock = bookSock.Accept();
                    try {  
                        BookData requestedBook = null;

                        int maxByte = newBookSock.Receive(buffer);
                        data = Encoding.ASCII.GetString(buffer, 0, maxByte);
                        Message recievedMsg = JsonSerializer.Deserialize<Message>(data);
                        string fullBooksJsonStr = System.IO.File.ReadAllText(booksJsonPath);
                        Console.WriteLine("line 114; msg recieved from libserver, type: {0}", recievedMsg.Type);
                        if (recievedMsg.Type == MessageType.BookInquiry) {
                            requestedBook = GetBookData(recievedMsg, fullBooksJsonStr);

                            msgNew = AssembleMsg(recievedMsg, requestedBook);
                            newBookSock.Send(msgNew);

                            // DEBUGGING; REMOVE WHEN DONE
                            string temp = Encoding.Default.GetString(msgNew);
                            Console.WriteLine("reply sent back to libserver, containing: {0}", temp);
                        }
                        else if (recievedMsg.Type == MessageType.EndCommunication) {
                            // end socket & program
                            Console.WriteLine("Closing BookHelper...");
                            newBookSock.Close();
                            bookSock.Close();
                            break;
                        }
                        Console.WriteLine("reply msg sent back to libserver");
                        newBookSock.Close();
                        bookSock.Close();
                    }
                    catch {
                    // error during message recieving (not serialised etc)
                    msgNew = AssembleMsg(null, null);
                    newBookSock.Send(msgNew);
                    Console.WriteLine("error occured somewhere idk");
                    newBookSock.Close();
                    bookSock.Close();
                    break;
                    }
                }
                catch {
                    Console.WriteLine("failed to make connection with libserver");
                    break;
                }
            }
        }
    }
}
