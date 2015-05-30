using System;

using Microsoft.SPOT;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using System.IO;

namespace Domotica
{
    public class WebServer : IDisposable
    {
        private Socket socket = null;
        public Timer OneHertzTimer;

        private Thread listenThread;
        public event ReceivedDataEventHandler DataReceived;


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
            listenThread = new Thread(new ThreadStart(ListenForRequest));
            Debug.Print(listenThread.ManagedThreadId.ToString() + " = listenthread");
            listenThread.Start();
            //ListenForRequest();



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
                //Debug.Print("Listen request");
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
                            //Thread.Sleep(150);
                            //led.Write(false);
                        }
                    }
                }
                catch (Exception e)
                {
                    Logging.LogMessageToFile(this.ToString() + "-" + e.Message, "WebserverThread");
                }
                Debug.Print("Exit Listen request");
            }
        }


        #region webserver requests
        private string HandleWebRequests( string request)
        {
            
            if (request.IndexOf('/') <= 0)
            {
                return "";
            }
            try
            {
                string req = request.Split('/')[1];
                if (request.IndexOf(' ') <= 0)
                {
                    return "";
                }
                req = req.Split(' ')[0];
                Logging.LogMessageToFile(this.ToString() + " - HandleWebRequests => " + req, "WebserverThread");
                switch (req)
                {
                    //case "GET / HTTP/1.1\r\nAccept: text/html, application/xhtml+xml, */*\r\nAccept-Language: nl-NL\r\nUser-Agent: Mozilla/5.0 (Windows NT 6.1; WOW64; Trident/7.0; rv:11.0) like Gecko\r\nAccept-Encoding: gzip, deflate\r\nHost: 192.168.2.19\r\nConnection: Keep-Alive\r\n\r\n":
                    //    ;
                    case "GetLogAll":
                        return Logging.ReadActiveLogFile("ALL");
                    case "GetLogRS232":
                        return Logging.ReadActiveLogFile("RS232");
                    case "ClearLogAll":
                        Logging.ClearFile("ALL");
                        return "Log file ALL cleared";
                    case "ClearLogRS232":
                        Logging.ClearFile("RS232");
                        return "Log file RS232 cleared";
                    case "ACTION-VENTILATION_OFF":
                        DataReceived(this, new ReceivedDataEventArgs("VENTILATION_OFF"));
                        return "Turning ventilation Off";
                    case "ACTION-VENTILATION_1":
                        DataReceived(this, new ReceivedDataEventArgs("VENTILATION_1"));
                        return "Turning ventilation to power mode 1";
                    case "ACTION-VENTILATION_2":
                        DataReceived(this, new ReceivedDataEventArgs("VENTILATION_2"));
                        return "Turning ventilation to power mode 2";
                    case "ACTION-VENTILATION_3":
                        DataReceived(this, new ReceivedDataEventArgs("VENTILATION_3"));
                        return "Turning ventilation to power mode 3";
                    case "Test2": return "Test";
                    default:
                        return "IP / ClearLogAll GetLogAll ClearLogRS232 GetLogRS232 ACTION-VENTILATION_OFF ACTION-VENTILATION_1 ACTION-VENTILATION_2 ACTION-VENTILATION_3";
                }
            }
            catch (Exception e)
            {
                Logging.LogMessageToFile(e.Message, "WebserverThread");
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
