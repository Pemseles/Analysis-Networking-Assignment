using System;
using System.Text.Json;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Collections.Generic;
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
        public static string settingsJsonPath = Path.GetFullPath(@"ClientServerConfig.json");
        public static string userJsonPath = Path.GetFullPath(@"Users.json");

        public void SequentialHelper()
        {
            while (true) {
                int maxByte = newUserSock.Receive(buffer);
                data = Encoding.ASCII.GetString(buffer, 0, maxByte);
                Message recievedMsg = JsonConvert.DeserializeObject<Message>(data);

                try {
                    string fullUserJsonStr = System.IO.File.ReadAllText(userJsonPath);
                    var tempUserList = JsonConvert.DeserializeObject<List<UserData>>(fullUserJsonStr);

                    if (recievedMsg.Type == MessageType.UserInquiryReply && fullUserJsonStr.Contains(data)) {
                        // message was delivered properly; send back only data of user
                        foreach (UserData user in tempUserList)
                        {
                            if (user.User_id == recievedMsg.Content) {
                                UserData requestedUser = new UserData(user.User_id, user.Name, user.Email, user.Phone);
                            }
                        }

                        byte[] msgNew = AssembleMsg(MessageType.UserInquiryReply);
                        newUserSock.Send(msgNew);
                    }
                }
                catch {
                    // error occured during message thingy
                    byte[] msgNew = AssembleMsg(MessageType.Error);
                    newUserSock.Send(msgNew);
                    Console.WriteLine("Error");
                }
            }
        }

        public byte[] AssembleMsg(MessageType messageTypeEnum) {
            if (messageTypeEnum == MessageType.UserInquiryReply) {
                // build message of requested user info
                Message replyJsonData = new Message {
                    Type = MessageType.UserInquiryReply,
                    Content = requestedUser;
                };
                string replyUserFound = JsonSerializer.Serialize(replyJsonData);
                byte[] msgNew = Encoding.ASCII.GetBytes(replyUserFound);
                return msgNew;
            }
            else if (messageTypeEnum == MessageType.Error) {
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

        public void start()
        {
            string fullSettingsJsonStr = System.IO.File.ReadAllText(settingsJsonPath);
            Setting userHelperSettings = JsonDeserialize<Setting>(fullSettingsJsonStr);

            byte[] buffer = new byte[1000];
            string data = null;

            IPEndPoint localEndpoint = new IPEndPoint(userHelperSettings.UserHelperIPAddress, userHelperSettings.UserHelperPortNumber);
            Socket userSock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            userSock.bind(localEndpoint);
            userSock.Listen(userHelperSettings.ServerListeningQueue);
            Console.WriteLine("\nWaiting for main server...");
            Socket newUserSock = userSock.Accept();

            SequentialHelper();
        }
    }
}
