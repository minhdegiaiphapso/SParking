//using Cirrious.CrossCore;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Net;
//using System.Net.Sockets;
//using System.Text;
//using System.Threading.Tasks;

//namespace SP.Parking.Terminal.Core.Services
//{
//    public interface IBugSenderService
//    {
//        void Start();
//    }

//    public class BugSenderService : IBugSenderService
//    {
//        byte[] bytes = new byte[1024];
//        Socket senderSock;
//        IHostSettings _hostSettings;
//        string _path;

//        public BugSenderService()
//        {
//            _hostSettings = Mvx.Resolve<IHostSettings>();
//            _path = Path.Combine(_hostSettings.StoragePath, "logs", "unhandled_crash");
//            //if (!Directory.Exists(_path))
//            //    Directory.CreateDirectory(_path);
//            Start();
//        }

//        public void Start()
//        {
//            Random rand = new Random();

//            Task.Factory.StartNew(() => {
//                Connect();
//                while (true)
//                {
//                    //int d = rand.Next(0, 12);
//                    //if (d != 4)
//                        System.Threading.Thread.Sleep(500);
//                    //else
//                    //{
//                    //    System.Threading.Thread.Sleep(2500);
//                    //    //var obj = new ClassLibrary1.Class1();
//                    //    //obj.foo();
//                    //}
//                    Send(_path);
//                }
//            }, TaskCreationOptions.LongRunning);
//        }

//        private void Connect()
//        {
//            try
//            {
//                // Create one SocketPermission for socket access restrictions 
//                SocketPermission permission = new SocketPermission(
//                    NetworkAccess.Connect,    // Connection permission 
//                    TransportType.Tcp,        // Defines transport types 
//                    "",                       // Gets the IP addresses 
//                    SocketPermission.AllPorts // All ports 
//                    );

//                // Ensures the code to have permission to access a Socket 
//                permission.Demand();

//                // Resolves a host name to an IPHostEntry instance
//                IPHostEntry ipHost = Dns.GetHostEntry("");

//                // Gets first IP address associated with a localhost 
//                IPAddress ipAddr = ipHost.AddressList.Where(add => add.AddressFamily == AddressFamily.InterNetwork).FirstOrDefault();
                
//                // Creates a network endpoint 
//                IPEndPoint ipEndPoint = new IPEndPoint(ipAddr, 4510);

//                // Create one Socket object to setup Tcp connection 
//                senderSock = new Socket(
//                    ipAddr.AddressFamily,// Specifies the addressing scheme 
//                    SocketType.Stream,   // The type of socket  
//                    ProtocolType.Tcp     // Specifies the protocols  
//                    );

//                senderSock.NoDelay = false;   // Using the Nagle algorithm 

//                // Establishes a connection to a remote host 
//                senderSock.Connect(ipEndPoint);
//            }
//            catch (Exception exc) { }
//        }

//        private void Send(string str)
//        {
//            try
//            {
//                // Sending message 
//                //<Client Quit> is the sign for end of data 
                
//                byte[] msg = Encoding.ASCII.GetBytes(str);

//                // Sends data to a connected Socket. 
//                int bytesSend = senderSock.Send(msg);

//                //ReceiveDataFromServer();
//            }
//            catch (Exception exc) { }
//        }
//    }
//}
