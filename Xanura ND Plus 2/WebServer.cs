using System;

using Microsoft.SPOT;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using System.IO;

namespace mySecondtry
{
    public class WebServer : IDisposable
    {
        private Socket socket = null;
        public Timer OneHertzTimer;
        //open connection to onbaord led so we can blink it with every request
        //private OutputPort led = new OutputPort(Pins.ONBOARD_LED, false);
        public WebServer()
        {
            //Initialize Socket class
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //Request and bind to an IP from DHCP server
            socket.Bind(new IPEndPoint(IPAddress.Any, 80));
            //Debug print our IP address
            Debug.Print(Microsoft.SPOT.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces()[0].IPAddress);
            //Start listen for web requests
            socket.Listen(10);
            ListenForRequest();
            //OneHertzTimer = new Timer(new TimerCallback(ListenForRequest), null, 0, 1000);
            //Thread webServerThread = new Thread(ListenForRequest);
            //webServerThread.Start();   
        }

        public void testGC()
        {
        }

        public void ListenForRequest()
        {
            while (true)
            {
                Debug.Print("Listen request");
                try
                {
                    using (Socket clientSocket = socket.Accept())
                    {
                        //Get clients IP
                        IPEndPoint clientIP = clientSocket.RemoteEndPoint as IPEndPoint;
                        EndPoint clientEndPoint = clientSocket.RemoteEndPoint;
                        //int byteCount = cSocket.Available;
                        int bytesReceived = clientSocket.Available;
                        if (bytesReceived > 0)
                        {
                            //Get request
                            byte[] buffer = new byte[bytesReceived];
                            int byteCount = clientSocket.Receive(buffer, bytesReceived, SocketFlags.None);
                            string request = new string(Encoding.UTF8.GetChars(buffer));
                            //Compose a response
                            string response = HandleWebRequests(request);
                            string header = "HTTP/1.0 200 OK\r\nContent-Type: text; charset=utf-8\r\nContent-Length: " + response.Length.ToString() + "\r\nConnection: close\r\n\r\n";
                            clientSocket.Send(Encoding.UTF8.GetBytes(header), header.Length, SocketFlags.None);
                            clientSocket.Send(Encoding.UTF8.GetBytes(response), response.Length, SocketFlags.None);
                            //Blink the onboard LED
                            //led.Write(true);
                            Thread.Sleep(150);
                            //led.Write(false);
                        }
                    }
                }
                catch (Exception e)
                {
                    Logging.LogMessageToFile(this.ToString() + "-" + e.Message, "ALL");
                }
                Debug.Print("Exit Listen request");
            }
        }


        #region webserver requests
        private string HandleWebRequests( string request)
        {
            try
            {
                switch (request)
                {
                    case "GET / HTTP/1.1\r\nAccept: text/html, application/xhtml+xml, */*\r\nAccept-Language: nl-NL\r\nUser-Agent: Mozilla/5.0 (Windows NT 6.1; WOW64; Trident/7.0; rv:11.0) like Gecko\r\nAccept-Encoding: gzip, deflate\r\nHost: 192.168.2.19\r\nConnection: Keep-Alive\r\n\r\n":
                        return Logging.ReadActiveLogFile();
                        ;
                    case "Test2": return "Test";
                    default:
                        return "Hello World";
                }
            }
            catch (Exception e)
            {
                Logging.LogMessageToFile(e.Message, "All");
                return "";
            }
        }

        #endregion

        #region IDisposable Members
        ~WebServer()
        {
            Dispose();
        }
        public void Dispose()
        {
            if (socket != null)
                socket.Close();
        }
        #endregion
    }
}
