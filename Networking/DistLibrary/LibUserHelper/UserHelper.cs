using System;
using System.Text;
using System.Text.Json;
using System.Net;
using System.Net.Sockets;
using System.IO;
using LibData;

namespace UserHelper
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
        public string userJsonPath;
        public string fullSettingsJsonStr;
        public Setting userHelperSettings;
        public byte[] buffer;
        public string data;
        public IPEndPoint localEndpoint;
        public Socket userSock;

        public SequentialHelper()
        {
            settingsJsonPath = @"../ClientServerConfig.json";
            userJsonPath = Path.GetFullPath(@"Users.json");
            fullSettingsJsonStr = System.IO.File.ReadAllText(settingsJsonPath);
            userHelperSettings = JsonSerializer.Deserialize<Setting>(fullSettingsJsonStr);
            buffer = new byte[1000];
            data = null;
            localEndpoint = new IPEndPoint(IPAddress.Parse(userHelperSettings.UserHelperIPAddress), userHelperSettings.UserHelperPortNumber);
            userSock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        private UserData GetUserData(Message recievedMsg, string userJsonStr) {
            UserData[] tempUserArray = JsonSerializer.Deserialize<UserData[]>(userJsonStr);

            foreach (UserData user in tempUserArray)
            {
                if (user.User_id == recievedMsg.Content) {
                    UserData requestedUser = new UserData {
                        User_id = user.User_id,
                        Name = user.Name,
                        Email = user.Email,
                        Phone = null
                    };
                    return requestedUser;
                }
            }
            return null;
        }

        public byte[] AssembleMsg(Message msgObj, UserData requestedUser) {
            if (requestedUser != null) {
                // build message of requested user info
                Message replyJsonData = new Message {
                    Type = MessageType.UserInquiryReply,
                    Content = JsonSerializer.Serialize(requestedUser)
                };
                string replyUserFound = JsonSerializer.Serialize(replyJsonData);
                byte[] msgNew = Encoding.ASCII.GetBytes(replyUserFound);
                return msgNew;
            }
            else if (requestedUser == null && msgObj != null) {
                // build message if user not found
                Message replyJsonData = new Message {
                    Type = MessageType.NotFound,
                    Content = "From UserHelper server: user not found\n"
                };
                string replyUserNotFound = JsonSerializer.Serialize(replyJsonData);
                byte[] msgNew = Encoding.ASCII.GetBytes(replyUserNotFound);
                return msgNew;
            }
            else {
                // build message if error occured
                Message replyJsonData = new Message {
                    Type = MessageType.Error,
                    Content = "From UserHelper server: error occured during usersearch\n"
                };
                string replyError = JsonSerializer.Serialize(replyJsonData);
                byte[] msgNew = Encoding.ASCII.GetBytes(replyError);
                return msgNew;
            }
        }

        public void start() {
            while (true) {
                try {
                    userSock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    userSock.Bind(localEndpoint);
                    userSock.Listen(userHelperSettings.ServerListeningQueue);
                    Console.WriteLine("\nWaiting for main server...");
                    Socket newUserSock = userSock.Accept();

                    try {
                        int maxByte = newUserSock.Receive(buffer);
                        data = Encoding.ASCII.GetString(buffer, 0, maxByte);
                        Message recievedMsg = JsonSerializer.Deserialize<Message>(data);
                        string fullUserJsonStr = System.IO.File.ReadAllText(userJsonPath);
                        Console.WriteLine("line 112; msg recieved from libserver, type: {0}", recievedMsg.Type);

                        if (recievedMsg.Type == MessageType.UserInquiry && fullUserJsonStr.Contains(data)) {
                            // message was delivered properly; send back only data of user
                            UserData desiredUser = GetUserData(recievedMsg, fullUserJsonStr);
                            byte[] msgNew = AssembleMsg(recievedMsg, desiredUser);
                            newUserSock.Send(msgNew);
                        }
                        else if (recievedMsg.Type == MessageType.UserInquiry && !fullUserJsonStr.Contains(data)) {
                            // user was not found
                            byte[] msgNew = AssembleMsg(recievedMsg, null);
                            newUserSock.Send(msgNew);
                        }
                        else if (recievedMsg.Type == MessageType.EndCommunication) {
                            Console.WriteLine("Closing UserHelper...");
                            newUserSock.Close();
                            userSock.Close();
                            break;
                        }
                        newUserSock.Close();
                        userSock.Close();
                    }
                    catch (Exception e) {
                        // error occured during message thingy
                        byte[] msgNew = AssembleMsg(null, null);
                        newUserSock.Send(msgNew);
                        Console.Out.WriteLine("Error: ", e.Message);
                    }
                }
                catch {
                    Console.WriteLine("failed to make connection with libserver");
                } 
            }
        }
    }
}