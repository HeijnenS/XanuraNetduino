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
        private static XanuraProtocolHandler XPH = new XanuraProtocolHandler();
        private static Logic Zichtakker17Logic = new Logic();
        private static WebServer webServer = new WebServer();
        private static Timer TwoSecTimer = null;
        private static Timer fiveminTimer = null;
        //private static TimerCallback timerCallBack = null;
        //private static bool StatusBathroomMovement = false;
        //private static Queue tempQ;
        //private static Queue lumQ;
        //private static Queue humQ;
        //private static Queue moveQ;
        //private event ReceivedDataEventHandler XanuraWebRequests;
        public bool SDCardPresent = false;
        private bool switchAirUnit = false;

        /*
        private void button_OnInterrupt(uint data1, uint data2, DateTime time)
        {
            if (switchAirUnit)
            {
                XPH.SendMessage("C01C01COFFCOFFC03C03COFFCOFF");
            }
            else
            {
                XPH.SendMessage("C01C01CONCONC03C03CONCON");
            }
            switchAirUnit = !switchAirUnit;
            Logging.LogMessageToFile("Onboard buttonn pressed", "ALL");
        }
        */

        public static void Main()
        {
            Program program = new Program();
            Microsoft.SPOT.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces()[0].EnableDhcp();
            fiveminTimer = new Timer(fiveminActions, null, 0, 60000);
            TwoSecTimer = new Timer(TwoSecActions, null, 0, 2000);            
            webServer.DataReceived += new ReceivedDataEventHandler(webServer_DataReceived);
            XPH.DataReceivedFromSerial += new ReceivedDataEventHandler(logic_DataReceived);
            RemovableMedia.Insert += new InsertEventHandler(RemovableMedia_Insert);
            RemovableMedia.Eject += new EjectEventHandler(RemovableMedia_Eject);

            //InterruptPort button = new InterruptPort(SecretLabs.NETMF.Hardware.Netduino.Pins.ONBOARD_BTN, true, Port.ResistorMode.Disabled, Port.InterruptMode.InterruptEdgeHigh);
            //button.OnInterrupt += new NativeEventHandler(program.button_OnInterrupt);
            //////WebServer init
            ////new Thread(secondThread).Start();         
            //tempQ = new Queue();
            //lumQ = new Queue();
            //humQ = new Queue();
            //moveQ = new Queue();

            while (true)
            {
                Thread.Sleep(1000);
            }
            //string s = "{\"id\": \"ZWayVDev_20:0:49:1\"\"deviceType\": \"probe\"\"metrics\": {  \"probeTitle\": \"Temperature\"  \"scaleTitle\": \"°C\"  \"level\": 7.599999904632568  \"title\": \"Temperature Sensor\"  \"iconBase\": \"zwave\"  }\"tags\": []\"location\": null\"updateTime\": 1387882443}";
            //ZWave.TestJDom(s);
        }

        //static void secondThread()
        //{
        //    WebServer webServer = new WebServer();
        //}

        ~Program()
        {
            //webServer.Dispose();
        }



        private static void fiveminActions(object state)
        {
            //ZwaveLogic();
            try
            {
                Microsoft.SPOT.Hardware.Utility.SetLocalTime(SetDatetime.NTPTime("time-a.nist.gov", +1));
            }
            catch (Exception ex)
            {
                Logging.LogMessageToFile("Program" + " - TwoSecActions NTPTime- " + "error =>" + ex.Message.ToString(), "ALL");
            }
        }

        private static void TwoSecActions(object state)
        {
            //ZwaveLogic();
            Logging.LogMessageToFile("Program" + " - TwoSecActions ?Query", "ALL");
            XPH.Query();
        }
/*
        private static void ZwaveLogic()
        {
            double temperature;
            int luminiscence;
            int humidity;
            bool movement;

            try
            {

                ZWave.GetSensorBathRoom(out temperature, out luminiscence, out humidity, out movement);


                tempQ.Enqueue(temperature);
                if (tempQ.Count > 100)
                {
                    tempQ.Dequeue();
                }
                lumQ.Enqueue(luminiscence);
                if (lumQ.Count > 100)
                {
                    lumQ.Dequeue();
                }
                humQ.Enqueue(humidity);
                if (humQ.Count > 100)
                {
                    humQ.Dequeue();
                }
                moveQ.Enqueue(movement);
                if (moveQ.Count > 100)
                {
                    moveQ.Dequeue();
                }


                if (movement == true && StatusBathroomMovement == false)
                {
                    Logging.LogMessageToFile("Bathroom light on by movement", "Logic");
                    Debug.Print("Light on by movement");
                    StatusBathroomMovement = movement;
                    XPH.SendMessage("B01B01BONBON");
                }
                if (movement == false && StatusBathroomMovement == true)
                {
                    Logging.LogMessageToFile("Bathroom light off by movement off", "Logic");
                    Debug.Print("Light off by movement");
                    StatusBathroomMovement = movement;
                    XPH.SendMessage("B01B01BOFFBOFF");
                }

                // Logging.LogMessageToFile("Sensor5 Log," + temperature + "," + luminiscence + "," + humidity,"Bathroom");
                if (humidity > 80 + 10) //humidity is bigger then 70% start ventilation
                //if (humQ.Count && humQ[99]>(humQ[0]+10))
                {
                    if (Zichtakker17Logic.VentilatieL3.status == "OFF")
                    {
                        Logging.LogMessageToFile("Ventilation started by humidity sensor >80", "Logic");
                        Debug.Print("Started by humidity");
                        XPH.SendMessage("C01C01CONCON");
                        XPH.SendMessage("C03C03CONCON");
                        //ventilatie hoog
                    }
                }
                if (humidity <= 85)
                {
                    // humidity lower then 70% and A02OFF then ventilation OFF
                    if (Zichtakker17Logic.Badkamerlamp.status == "OFF" && Zichtakker17Logic.Afzuigkap.status == "OFF" && Zichtakker17Logic.VentilatieL3.status == "ON")
                    //if (true)
                    {
                        Logging.LogMessageToFile("Ventilation stopped by humidity sensor <85", "Logic");
                        Debug.Print("stopped by humidity");
                        XPH.SendMessage("C01C01COFFCOFF");
                        XPH.SendMessage("C03C03COFFCOFF");
                    }
                }
            }
            catch (Exception e)
            {
                Logging.LogMessageToFile(e.Message, "All");
            }
        }
*/
        private static void logic_DataReceived(object sender, ReceivedDataEventArgs e)
        {
            try
            {
            string data = "";
            XanuraProtocolHandler ph = (XanuraProtocolHandler)sender;
            data = e.ReceivedData;
            //Debug.Print(DateTime.UtcNow.ToString() + ", --------------RECEIVED from ----------------" + ph.ToString() + " - " + data);
            Zichtakker17Logic.SetStatus(data);
            string sendData = Zichtakker17Logic.GetAction(data);
            if (sendData != "")
            {
                XPH.SendMessage(sendData);
            }
            }
            catch(Exception ex)
            {
                Logging.LogMessageToFile("Program - logic_DataReceived => " + ex.Message, "All");
            }

        }

        private static void webServer_DataReceived(object sender, ReceivedDataEventArgs e)
        {
            try
            {
                switch (e.ReceivedData)
                {
                    case "VENTILATION_OFF":
                        XPH.SendMessage("C01C01COFFCOFFC03C03COFFCOFF");
                        break;
                    case "VENTILATION_1":
                        XPH.SendMessage("C01C01CONCONC03C03COFFCOFF");
                        break;
                    case "VENTILATION_2":
                        break;
                    //XPH.SendMessage("");
                    case "VENTILATION_3":
                        XPH.SendMessage("C01C01CONCONC03C03CONCON");
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


        //private static bool IsZwaveSwitchIsOn()
        //{
        //    //this allready works!!!!! jiiiiiiiiiiiiiii haaaaaaaaaaaaaaaa
        //    WebRequest WebReq = WebRequest.Create("http://192.168.2.25:8083/ZWaveAPI/Run/devices[3].instances[0].commandClasses[0x20].data.level.value");
        //    WebResponse WebResp = WebReq.GetResponse();
        //     //WebResponseStream WebResp = WebReq.GetResponse();
        //    using (var reader = new StreamReader(WebResp.GetResponseStream()))
        //    {
        //        string result = reader.ReadToEnd().ToString(); // do something fun... 
        //        if (result == "255")
        //        {
        //            return true;
        //        }
        //        else
        //        {
        //            return false;
        //        }

        //    }
        //}


    //    private static void ZwaveSwitch(int level)
    //    {
    //        //this allready works!!!!! jiiiiiiiiiiiiiii haaaaaaaaaaaaaaaa
    //        WebRequest WebReq = WebRequest.Create("http://192.168.2.25:8083/ZWaveAPI/Run/devices[3].instances[0].commandClasses[0x20].Set(" + level.ToString() + ")");
    //    }

    }
}
