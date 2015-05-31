using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;
using SecretLabs.NETMF.Hardware.NetduinoPlus;
using System.IO;
using System.Collections;
using Microsoft.SPOT.IO;


namespace Domotica
{
    public class Program
    {
        private static WebServer webServer = new WebServer();
        private static Timer fiveminTimer = null;
        public bool SDCardPresent = false;

        public static void Main()
        {
            Program program = new Program();
            Microsoft.SPOT.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces()[0].EnableDhcp();
            fiveminTimer = new Timer(fiveminActions, null, 0, 60000);          
            webServer.DataReceived += new ReceivedDataEventHandler(webServer_DataReceived);
            RemovableMedia.Insert += new InsertEventHandler(RemovableMedia_Insert);
            RemovableMedia.Eject += new EjectEventHandler(RemovableMedia_Eject);
            while (true)
            {
                Thread.Sleep(1000);
            }
        }


        ~Program()
        {
        }



        private static void fiveminActions(object state)
        {
            try
            {
                Microsoft.SPOT.Hardware.Utility.SetLocalTime(SetDatetime.NTPTime("time-a.nist.gov", +1));
            }
            catch (Exception ex)
            {
                //Logging.LogMessageToFile("Program" + " - TwoSecActions NTPTime- " + "error =>" + ex.Message.ToString(), "ALL");
            }
        }


        private static void webServer_DataReceived(object sender, ReceivedDataEventArgs e)
        {
            try
            {
                switch (e.ReceivedData)
                {
                    case "VENTILATION_OFF":
                        Debug.Print("VENTILATION_OFF");
                        break;
                    case "VENTILATION_1":
                        Debug.Print("VENTILATION_1");
                        break;
                    case "VENTILATION_2":
                        break;
                    case "VENTILATION_3":
                        Debug.Print("VENTILATION_3");
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.Print("error webServer_DataReceived => " + ex.Message.ToString());
            }
        }

        static void RemovableMedia_Eject(object sender, MediaEventArgs e)
        {
            Debug.Print("Ejected");
        }

        static void RemovableMedia_Insert(object sender, MediaEventArgs e)
        {
            Debug.Print("Inserted");
        }
    }
}
