using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net.Sockets;
using System.Text;

namespace ServerChatting
{
    class Program
    {
        
        //Class to handle each client request separatly
        public class user
        {
            string userName;
            TcpClient socket;
            int chat;


            public user(string n, TcpClient tc)
            {
                this.userName = n;
                this.socket = tc;
                this.chat = 0;
            }


           public int getChat()
            {
                return this.chat;
            }

            public void setChat(int n)
            {
                this.chat = n;
            }

            public string getName()
            {
                return this.userName;
            }

            public TcpClient getsocket()
            {
                return this.socket;
            }

        }

        static user[] userNames = new user[9];
        //static user[] ios = new user[9];

        public static void Main(string[] args)
        {
            TcpListener serverSock = new TcpListener(8888);
            TcpClient clientSock = default(TcpClient);

            


            serverSock.Start();
            Console.WriteLine(">> Server has started");

            while(true)
            {
                //Accepting clients
                clientSock = serverSock.AcceptTcpClient();
                //handleClinet client = new handleClinet();
                //client.startClient(clientSocket, Convert.ToString(counter));
                addUser(clientSock);


            }//end of while


            clientSock.Close();
            serverSock.Stop();
            Console.WriteLine(" >> " + "exit");
            Console.ReadLine();


        }//end of main

        //Adds user to the TCP connection.
        static void addUser(TcpClient tcp)
        {
            byte[] bytesFrom = new byte[10000];
            string dataFromClient = null;
            TcpClient clientTemp = tcp;
            Thread.Sleep(5000);
            NetworkStream networkStream = clientTemp.GetStream();
            networkStream.Read(bytesFrom, 0, (int)clientTemp.ReceiveBufferSize);
            dataFromClient = Encoding.ASCII.GetString(bytesFrom);
            //split by space
            string[] getName = dataFromClient.Split(' ');
            int userID = 0;
            for (int x = 0; x < userNames.Length; x++)
                if (userNames[x] == null)
                {
                    userNames[x] = new user(getName[0], tcp);
                    userID = x;
                    break;
                }

            //Console.WriteLine(dataFromClient); //debug statement

            //Lambda Thread for client
            Thread clientThread = new Thread(() => doChat(userID));
            clientThread.Start();
        }//end of addUser


        private static void doChat(int n)
        {
            byte[] bytesFrom = new byte[10025];
            string dataFromClient = null;
            Byte[] sendBytes = null;
            string serverResponse = null;
            string rCount = null;
            user client = userNames[n];

            while (true)
            {
                try
                {
                    
                    NetworkStream networkStream = client.getsocket().GetStream();
                    networkStream.Read(bytesFrom, 0, (int)client.getsocket().ReceiveBufferSize);
                    dataFromClient = Encoding.ASCII.GetString(bytesFrom);
                    
                    //Console.WriteLine(cleaned); //debug statement to see what's in there.

                    if (!String.IsNullOrEmpty(dataFromClient) && !(dataFromClient.Equals("\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0")))
                    {
                        var cleaned = dataFromClient.Replace("\0", string.Empty);
                        Console.WriteLine(cleaned); //debug statement to see what's in there.
                       // byte[] outStream = Encoding.ASCII.GetBytes(client.getName() + " says: " + cleaned); //+ "$");// + textBox2.Text);//"This was printed$");
                        //networkStream.Write(outStream, 0, outStream.Length);
                        //networkStream.Flush();
                        writeToUsers((string)cleaned,client);
                        dataFromClient = null;
                    }


                }
                catch (Exception e)
                {

                }//end of catch

            }//end of while loop

        }//end of doChat()

        public static string whichRoom(int n)
        {
            switch (n)
            {
                case 0:
                    return "switched to android";
                case 1:
                    return "switched to ios";
                case 2:
                    return "switched to global";
            }
            return null;
        }

        private static void changeRooms(string message, user cli,int room)
        {
            NetworkStream serverStream;
            //switching rooms
            if (room < 2)
            {
                for (int x = 0; x < userNames.Length; x++)
                {
                    if (userNames[x] == null)
                        break;

                    if (userNames[x] != null)
                        if (userNames[x].getName().Equals(cli.getName()))
                        {
                            userNames[x].setChat(room);
                            serverStream = userNames[x].getsocket().GetStream();
                            byte[] outStream = Encoding.ASCII.GetBytes(userNames[x].getName() + " has switched to: " + whichRoom(room));
                            serverStream.Write(outStream, 0, outStream.Length);
                            serverStream.Flush();
                            break;
                        }//end of if

                }//end of for loop

            }//if room isn't android or ios

            //sending global chat
            if(room == 2)
            {
                for(int x = 0; x < userNames.Length; x++)
                {
                    if (userNames[x] == null)
                        break;
                    else
                    {
                        serverStream = userNames[x].getsocket().GetStream();
                        byte[] outStream = Encoding.ASCII.GetBytes("[Global] "+cli.getName() + " says: " + message);
                        serverStream.Write(outStream, 0, outStream.Length);
                        serverStream.Flush();
                    }
                }
            }

        }//end of changeRooms

        private static void writeToUsers(string message, user cli)
        {
            int chat = cli.getChat();
            NetworkStream serverStream;
            bool argMessage = false;

            if (message.Contains(","))
            {
                string [] temp = message.Split(':');//splits the username and arg
                string [] argTemp = temp[1].Split(',');

                if(Convert.ToInt32(argTemp[0])<3)
                changeRooms(message, cli, Convert.ToInt32(argTemp[0]));

                //special case since we wont want to print again for global 0 = andorid, 1 = ios, 2 = global
                if (Convert.ToInt32(argTemp[0]) <= 2)
                    argMessage = true;

                //Only other thing would be to print all names to current user
                else
                {
                    //Will fire unless chat changes from 0 1 or 2
                    if (!argMessage)
                    {
                        //don't want to print other shit
                        argMessage = true;
                        string toNames = "";
                        for (int x = 0; x < userNames.Length; x++)
                        {
                            if (userNames[x] == null)
                                break;
                            if (userNames[x] != null)
                            {
                                toNames += userNames[x].getName() + " \n";
                            }//end of if

                        }//end of for

                        serverStream = cli.getsocket().GetStream();
                        byte[] outstream = Encoding.ASCII.GetBytes("Here are the names... " + toNames);
                        serverStream.Write(outstream, 0, outstream.Length);
                        serverStream.Flush();
                    }

                }//end of else

            }//end of arg

            if (!argMessage)
            {
                //Goes through all 9 users if avaliable and checks for users then will send a message
                for (int x = 0; x < userNames.Length; x++)
                {
                    if (userNames[x] == null)
                        break;
                    else
                    {
                        if (userNames[x].getChat() == chat)
                        {
                            serverStream = userNames[x].getsocket().GetStream();
                            byte[] outStream = Encoding.ASCII.GetBytes(cli.getName() + " says: " + message); //+ "$");// + textBox2.Text);//"This was printed$");
                            serverStream.Write(outStream, 0, outStream.Length);
                            serverStream.Flush();
                        }//end of same chat

                    }//end of else

                }//end of for loop
                Console.WriteLine("wrote to all users in certain channel");
            }//end of nonargMessage

        }//end of writeToUsers


    }
}
