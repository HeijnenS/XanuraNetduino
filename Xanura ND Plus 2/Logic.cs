using System;
using System.Threading;
using System.Collections;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;
using System.Net;

namespace mySecondtry
{
    class Logic
    {
        private bool debug = false;
        public Daix Woonkamerlamp;
        public Daix Badkamerlamp;
        public Daix VentilatieL1;
        //private Daix VentilatieL2;
        public Daix VentilatieL3;
        public Daix Afzuigkap;
        public Hashtable actuatorHashtable;
        private Daix LastHandledActuator;
        private OutputPort led = new OutputPort(Pins.ONBOARD_LED, false);
        private Timer onehundredHertzTimer;
        //private TimerCallback VentilationOff;


        private static void ZwaveSwitch(int level)
        {
            try
            {
               // WebRequest WebReq = WebRequest.Create("http://192.168.2.25:8083/ZWaveAPI/Run/devices[3].instances[0].commandClasses[0x20].Set('255')");
               // WebResponse WebResp = WebReq.GetResponse();

                //if (level > 0)
                //{
                //    WebRequest WebReq = WebRequest.Create("http://192.168.2.25:8083/ZWaveAPI/Run/devices[3].instances[0].commandClasses[0x20].Set(0)");
                    
                //    WebResponse WebResp = WebReq.GetResponse();
                //}
                //else
                //{
                //    WebRequest WebReq = WebRequest.Create("http://192.168.2.25:8083/ZWaveAPI/Run/devices[3].instances[0].commandClasses[0x20].Set(255)");
                //    WebResponse WebResp = WebReq.GetResponse();
                //    //Uri uriAddress = new Uri("http://192.168.2.25:8083/ZWaveAPI/Run/devices[3].instances[0].commandClasses[0x20].Set(255)");
                //}
            }
            catch (Exception e)
            {
                Logging.LogMessageToFile(e.Message, "ALL");
            }
            
            
        }


        private void controlAliveLed(object state)
        {
            try
            {
                led.Write(!led.Read());
            }
            catch (Exception e)
            {
                Logging.LogMessageToFile(this.ToString() + "-" + e.Message, "ALL");
            }
        }

        public Logic()
        {
            onehundredHertzTimer = new Timer(new TimerCallback(controlAliveLed), null, 0, 100);
            actuatorHashtable = new Hashtable();

            Woonkamerlamp = new Daix();
            Woonkamerlamp.location = "Woonkamer";
            Woonkamerlamp.actuator = "Woonkamerlamp";
            Woonkamerlamp.address = "A01";
            
            Afzuigkap = new Daix();
            Afzuigkap.location = "Keuken";
            Afzuigkap.actuator = "Afzuigkap";
            Afzuigkap.address = "A02";

            Badkamerlamp = new Daix();
            Badkamerlamp.location = "Badkamer";
            Badkamerlamp.actuator = "Badkamerlamp";
            Badkamerlamp.address = "B01";

            VentilatieL1 = new Daix();
            VentilatieL1.location = "Zolder";
            VentilatieL1.actuator = "VentilatieL1";
            VentilatieL1.address = "C01";

            VentilatieL3 = new Daix();
            VentilatieL3.location = "Zolder";
            VentilatieL3.actuator = "VentilatieL3";
            VentilatieL3.address = "C03";


            VentilatieL1.DirectlyLinkedOn_On[0] = Afzuigkap.address;
            VentilatieL1.DirectlyLinkedOn_On[1] = Badkamerlamp.address;
            VentilatieL3.DirectlyLinkedOn_On[0] = Afzuigkap.address;
            VentilatieL3.DirectlyLinkedOn_On[1] = Badkamerlamp.address;
            //Woonkamerlamp.DirectlyLinkedOn_On[0] = Afzuigkap.address;
            //Woonkamerlamp.DirectlyLinkedOff_Off[0] = Afzuigkap.address;

            actuatorHashtable.Add(Afzuigkap.address, Afzuigkap);
            actuatorHashtable.Add(Woonkamerlamp.address, Woonkamerlamp);
            actuatorHashtable.Add(Badkamerlamp.address, Badkamerlamp);
            actuatorHashtable.Add(VentilatieL1.address, VentilatieL1);
            actuatorHashtable.Add(VentilatieL3.address, VentilatieL3);
        }

        public void SetStatus(string message)
        {
            try
            {

                if (message == "")
                {
                    return;
                }
                string address = message.Split(' ').GetValue(0).ToString();
                string status = message.Split(' ').GetValue(1).ToString();

                foreach (DictionaryEntry actuator in actuatorHashtable)
                {
                    if (actuator.Key.ToString() == address)
                    {
                        Daix tmpDaix = (Daix)actuator.Value;
                        tmpDaix.status = status;
                        LastHandledActuator = tmpDaix;

                        if (debug)
                        {
                            Debug.Print(tmpDaix.actuator + " is turned " + tmpDaix.status);
                        }
                        return;
                    }
                }
            }
            catch (Exception e)
            {
                Logging.LogMessageToFile(this.ToString() + "-" + e.Message, "ALL");
            }

        }


        public string GetAction(string message)
        {
            try
            {
                if (message == "")
                {
                    return "";
                }
                string address = message.Split(' ').GetValue(0).ToString();
                string status = message.Split(' ').GetValue(1).ToString();
                string tempString = "";

                foreach (DictionaryEntry actuator in actuatorHashtable)
                {
                    Daix tmpDaix = (Daix)actuator.Value;
                    if (status == "ON" && (Array.IndexOf(tmpDaix.DirectlyLinkedOn_On, address) >= 0))
                    {
                        tempString = tempString + tmpDaix.address + tmpDaix.address + tmpDaix.Group() + "ON" + tmpDaix.Group() + "ON";
                        if (debug)
                        {
                            Debug.Print(tmpDaix.actuator + " is going to be turned on by " + LastHandledActuator.actuator + " = " + LastHandledActuator.status);
                        }
                    }
                    if (status == "ON" && (Array.IndexOf(tmpDaix.DirectlyLinkedOn_Off, address) >= 0))
                    {
                        tempString = tempString + tmpDaix.address + tmpDaix.address + tmpDaix.Group() + "OFF" + tmpDaix.Group() + "OFF";
                        if (debug)
                        {
                            Debug.Print(tmpDaix.actuator + " is going to be turned on by " + LastHandledActuator.actuator + " = " + LastHandledActuator.status);
                        }
                    }
                    if (status == "OFF" && (Array.IndexOf(tmpDaix.DirectlyLinkedOff_On, address) >= 0))
                    {
                        tempString = tempString + tmpDaix.address + tmpDaix.address + tmpDaix.Group() + "ON" + tmpDaix.Group() + "ON";
                        if (debug)
                        {
                            Debug.Print(tmpDaix.actuator + " is going to be turned off by " + LastHandledActuator.actuator + " = " + LastHandledActuator.status);
                        }
                    }
                    if (status == "OFF" && (Array.IndexOf(tmpDaix.DirectlyLinkedOff_Off, address) >= 0))
                    {
                        //create exception when bathroom light or cooking ventialtion is still on
                        tempString = tempString + tmpDaix.address + tmpDaix.address + tmpDaix.Group() + "OFF" + tmpDaix.Group() + "OFF";
                        if (debug)
                        {
                            Debug.Print(tmpDaix.actuator + " is going to be turned off by " + LastHandledActuator.actuator + " = " + LastHandledActuator.status);
                        }
                    }
                }

                if (((address == "A02") || (address == "B01")) && status == "ON")
                {
                    ZwaveSwitch(255);
                }

                if (((address == "A02") || (address == "B01")) && status == "OFF")
                {
                    if (Badkamerlamp.status == "OFF" && Afzuigkap.status == "OFF")
                    {
                        ZwaveSwitch(0);
                        if (debug)
                        {
                            Debug.Print(this.ToString() + " => Ventilatie mag uit");
                        }
                        //HumidityDelay = new Timer(new TimerCallback(VentilationOff), null, 0, 10000);
                        tempString = "C01C01COFFCOFFC03C03COFFCOFF";
                    }
                    else
                    {
                        if (debug)
                        {
                            if (Badkamerlamp.status == "ON")
                            {
                                Debug.Print(this.ToString() + " => " + Badkamerlamp.actuator + " is still " + Badkamerlamp.status);
                            }
                            if (Afzuigkap.status == "ON")
                            {
                                Debug.Print(this.ToString() + " => " + Afzuigkap.actuator + " is still " + Afzuigkap.status);
                            }
                        }
                    }
                }
                return tempString;
            }
            catch (Exception e)
            {
                Logging.LogMessageToFile(this.ToString() + "-" + e.Message, "ALL");
                return "";
            }
        }       
    }
}
