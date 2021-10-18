using System;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Text.Json;
using LibData;


namespace LibClient
{
    // Note: Do not change this class 
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

    // Note: Do not change this class 
    public class Output
    {
        public string Client_id { get; set; } // the id of the client that requests the book
        public string BookName { get; set; } // the name of the book to be reqyested
        public string Status { get; set; } // final status received from the server
        public string BorrowerName { get; set; } // the name of the borrower in case the status is borrowed, otherwise null
        public string BorrowerEmail { get; set; } // the email of the borrower in case the status is borrowed, otherwise null
    }

    // Note: Complete the implementation of this class. You can adjust the structure of this class.
    public class SimpleClient
    {
        // some of the fields are defined. 
        public Output result;
        public Socket clientSocket;
        public IPEndPoint serverEndPoint;
        public IPAddress ipAddress;
        public Setting settings;
        public string client_id;
        private string bookName;
        // all the required settings are provided in this file
        public string configFile = @"../ClientServerConfig.json";
        //public string configFile = @"../../../../ClientServerConfig.json"; // for debugging

        // todo: add extra fields here in case needed 

        /// <summary>
        /// Initializes the client based on the given parameters and seeting file.
        /// </summary>
        /// <param name="id">id of the clients provided by the simulator</param>
        /// <param name="bookName">name of the book to be requested from the server, provided by the simulator</param>
        public SimpleClient(int id, string bookName)
        {
            //todo: extend the body if needed.
            this.bookName = bookName;
            this.client_id = "Client " + id.ToString();
            this.result = new Output();
            result.BookName = bookName;
            result.Client_id = this.client_id;
            // read JSON directly from a file
            try
            {
                string configContent = File.ReadAllText(configFile);
                this.settings = JsonSerializer.Deserialize<Setting>(configContent);
                this.ipAddress = IPAddress.Parse(settings.ServerIPAddress);
                this.serverEndPoint = new IPEndPoint(this.ipAddress, this.settings.ServerPortNumber);
            }
            catch (Exception e)
            {
                Console.Out.WriteLine("[Client Exception] {0}", e.Message);
            }
        }

        /// <summary>
        /// Establishes the connection with the server and requests the book according to the specified protocol.
        /// Note: The signature of this method must not change.
        /// </summary>
        /// <returns>The result of the request</returns>
        public Output start()
        {
            // todo: implement the body to communicate with the server and requests the book. Return the result as an Output object.
            // Adding extra methods to the class is permitted. The signature of this method must not change.

            clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            clientSocket.Connect(serverEndPoint);
            byte[] buffer = new byte[1000];
            string data = "";

            try {
                Console.WriteLine("line 94, socket is connected");
                // first handshake to LibServer
                byte[] firstMsg = AssembleMsg(MessageType.Hello, null);
                clientSocket.Send(firstMsg);
                Console.WriteLine("line 98, Hello msg is sent");

                int welcomingMsgInt = clientSocket.Receive(buffer);
                data = Encoding.ASCII.GetString(buffer, 0, welcomingMsgInt);
                Message recievedWelcomingMsg = JsonSerializer.Deserialize<Message>(data);
                Console.WriteLine("line 103, Message recieved: {0}", recievedWelcomingMsg.Content);

                if (recievedWelcomingMsg.Type == MessageType.Welcome) {
                    if (client_id == "Client -1") {
                        // end communication
                        Console.WriteLine("line 109, ending msg being made & client_id = -1");
                        byte[] endingMsg = AssembleMsg(MessageType.EndCommunication, null);
                        clientSocket.Send(endingMsg);
                        clientSocket.Close();
                        Console.WriteLine("line 113; ending msg is sent & socket is closed");
                    }
                    else {
                        // main function; get requested book into message & bookdata obj & check if book was found
                        Console.WriteLine("line 117; main body of function (client_id is not -1 but {0})", this.client_id);
                        byte[] mainMsg = AssembleMsg(MessageType.BookInquiry, null);
                        clientSocket.Send(mainMsg);
                        Console.WriteLine("line 120; bookinquiry sent to libserver");

                        int bookInquiryReplyMsgInt = clientSocket.Receive(buffer);
                        data = Encoding.ASCII.GetString(buffer, 0, bookInquiryReplyMsgInt);
                        Message bookInquiryMsg = JsonSerializer.Deserialize<Message>(data);
                        Console.WriteLine("line 124; message recieved from libserver as response to bookinquiry: {0}", bookInquiryMsg.Type);
                        Console.WriteLine("Content of recieved bookinquiry reply: {0}", bookInquiryMsg.Content);

                        if (bookInquiryMsg.Type == MessageType.NotFound) {
                            // book was not found
                            Console.WriteLine("line 128; Book: {0} was not found in Library, building output...", this.bookName);
                            this.result = AssembleOutputObj(this.client_id, this.bookName, null, null, null);
                        }
                        else {
                            // book was found; check if currently being borrowed
                            Console.WriteLine("line 133; book was found, will check if available");
                            BookData recievedBookData = JsonSerializer.Deserialize<BookData>(bookInquiryMsg.Content);
                            if (recievedBookData.Status == "Available") {
                                // book is available; assemble output
                                Console.WriteLine("line 137; book is available, building output");
                                this.result = AssembleOutputObj(this.client_id, this.bookName, recievedBookData.Status, null, null);
                            }
                            else {
                                // book is being borrowed; send userInquiry w borrower's user_id
                                Console.WriteLine("line 142; book is being borrowed, sending userinquiry");
    	                        byte[] userInquiryMsg = AssembleMsg(MessageType.UserInquiry, recievedBookData);
                                clientSocket.Send(userInquiryMsg);
                                Console.WriteLine("line 145; userinquiry sent");
                                
                                int UserInquiryReplyMsgInt = clientSocket.Receive(buffer);
                                data = Encoding.ASCII.GetString(buffer, 0, UserInquiryReplyMsgInt);
                                Message userInquiryMsgObj = JsonSerializer.Deserialize<Message>(data);
                                UserData borrowerData = JsonSerializer.Deserialize<UserData>(userInquiryMsgObj.Content);
                                Console.WriteLine("line 151; reply to userinquiry recieved & put into obj: name of borrower: {0}", borrowerData.Name);
                                Console.WriteLine("line 152; buidling output based on reply to userinquiry");

                                this.result = AssembleOutputObj(this.client_id, this.bookName, recievedBookData.Status, borrowerData.Name, borrowerData.Email);
                            }
                        }
                    }
                    Console.WriteLine("line 158; operations done, closing socket & returning ouput");
                    clientSocket.Close();
                    return result;
                }
                else {
                    // server sent wrong msgType back
                    Console.WriteLine("line 164; Server sent wrong msgtype, must be Welcome");
                    clientSocket.Close();
                }
                // return type: return result
                return null;
            }
            catch {
                // error
                this.result = AssembleOutputObj(null, null, null, null, null);
                Console.WriteLine("Error occured somewhere in function, check if LibInput.json is in order and try again.");
                return null;
            }
        }

        public byte[] AssembleMsg(MessageType desiredMsgType, BookData recievedBookData) {
            if (desiredMsgType == MessageType.Hello) {
                // make first handshake message, expected reply is msgType: Welcome
                Console.WriteLine("line 181; assembling hello msg to send to libserver");
                Message replyJsonData = new Message {
                    Type = MessageType.Hello,
                    Content = this.client_id
                };
                string helloMsg = JsonSerializer.Serialize(replyJsonData);
                byte[] msgNew = Encoding.ASCII.GetBytes(helloMsg);
                return msgNew;
            }
            else if (desiredMsgType == MessageType.BookInquiry) {
                // make msg when Welcome msgType is recieved
                Console.WriteLine("line 192; assembling bookinquiry msg to send to libserver");
                Message replyJsonData = new Message {
                    Type = MessageType.BookInquiry,
                    Content = this.bookName
                };
                string bookMsg = JsonSerializer.Serialize(replyJsonData);
                byte[] msgNew = Encoding.ASCII.GetBytes(bookMsg);
                return msgNew;
            }
            else if (desiredMsgType == MessageType.UserInquiry) {
                // msg for userInquiries
                Console.WriteLine("line 203; assembling msg userinquiry to send to libserver");
                Message replyJsonData = new Message {
                    Type = MessageType.UserInquiry,
                    Content = recievedBookData.BorrowedBy
                };
                string userMsg = JsonSerializer.Serialize(replyJsonData);
                byte[] msgNew = Encoding.ASCII.GetBytes(userMsg);
                return msgNew;
            }
            else if (desiredMsgType == MessageType.EndCommunication) {
                // make ending message for closing all communication
                Console.WriteLine("line 214; assembling closing msg to send to libserver");
                Message replyJsonData = new Message {
                    Type = MessageType.EndCommunication,
                    Content = "Content is not important pls don't look :("
                };
                string endingMsg = JsonSerializer.Serialize(replyJsonData);
                byte[] msgNew = Encoding.ASCII.GetBytes(endingMsg);
                return msgNew;
            }
            else {
                // error message
                Console.WriteLine("line 255; assembling error msg");
                Message replyJsonData = new Message {
                    Type = MessageType.Error,
                    Content = "Error occured somewhere"
                };
                string errorMsg = JsonSerializer.Serialize(replyJsonData);
                byte[] msgNew = Encoding.ASCII.GetBytes(errorMsg);
                return msgNew;
            }
        }

        public Output AssembleOutputObj(string client_idNo, string bookTitle, string statusOfBook, string nameOfBorrower, string emailOfBorrower) {
            if (statusOfBook == "Available") {
                // output if book is available
                Console.WriteLine("line 239; assembling output based on book being available");
                Output outputObj = new Output {
                    Client_id = client_idNo,
                    BookName = bookTitle,
                    Status = statusOfBook,
                    BorrowerName = null,
                    BorrowerEmail = null
                };
                return outputObj;
            }
            else if (statusOfBook == "Borrowed") {
                // output if book is being borrowed
                Console.WriteLine("line 251; assembling output based on book being borrowed");
                Output outputObj = new Output {
                    Client_id = client_idNo,
                    BookName = bookTitle,
                    Status = statusOfBook,
                    BorrowerName = nameOfBorrower,
                    BorrowerEmail = emailOfBorrower
                };
                return outputObj;
            }
            else if (client_idNo != null && statusOfBook == null) {
                // output if book is not in Books.json
                Console.WriteLine("line 263; assembling output based on book being not found");
                Output outputObj = new Output {
                    Client_id = client_idNo,
                    BookName = bookTitle,
                    Status = "NotFound",
                    BorrowerName = null,
                    BorrowerEmail = null
                };
                return outputObj;
            }
            else {
                // output if error occured
                Console.WriteLine("line 275; assembling output error occured");
                Output outputobj = new Output {
                    Client_id = null,
                    BookName = null,
                    Status = null,
                    BorrowerName = null,
                    BorrowerEmail = null
                };
                return outputobj;
            }
        }
    }
}