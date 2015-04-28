using System;
using Microsoft.SPOT;
using System.Net;
using System.IO;


namespace mySecondtry
{
    static class ZWave
    {

        public static void GetSensorBathRoom(out double temperature, out int luminiscence, out int humidity, out bool movement)
        {
            HttpWebRequest WebReq;
            HttpWebResponse WebResp;

            Debug.GC(true);

            temperature = 0;
            luminiscence = 0;
            humidity = 0;
            movement = false;
            try
            {
                var request = System.Net.WebRequest.Create("http://192.168.2.25:8083/ZAutomation/api/v1/devices/") as System.Net.HttpWebRequest;
                request.KeepAlive = true;

                request.Method = "PUT";

                request.ContentType = "application/json";
                byte[] byteArray = System.Text.Encoding.UTF8.GetBytes("{\"id\":\"ZWayVDev_20:0:49:1\",\"deviceType\":\"probe\",\"metrics\":{\"probeTitle\":\"Temperature\",\"scaleTitle\":\"°C\",\"level\":7.599999904632568,\"title\":\"Temperature Sensor\",\"iconBase\":\"zwave\"},\"tags\":[],\"location\":null,\"updateTime\":1387882443}");
                request.ContentLength = byteArray.Length;
                using (var writer = request.GetRequestStream()) { writer.Write(byteArray, 0, byteArray.Length); }

                string responseContent = null;
                using (var response = request.GetResponse() as System.Net.HttpWebResponse)
                {
                    using (var reader = new System.IO.StreamReader(response.GetResponseStream()))
                    {
                        responseContent = reader.ReadToEnd();
                    }
                }

                WebReq = (HttpWebRequest)WebRequest.Create("http://192.168.2.25:8083/ZAutomation/api/v1/devices/ZWayVDev_zway_5-0-48-1/command/update");
                WebReq.KeepAlive = true;
                WebReq.Method = "GET";
                WebReq.Timeout = 1000;
                WebReq.ReadWriteTimeout = 1000;
                WebResp = (HttpWebResponse)WebReq.GetResponse();
                WebResp.Close();
                WebReq = (HttpWebRequest)WebRequest.Create("http://192.168.2.25:8083/ZAutomation/api/v1/devices/ZWayVDev_zway_5-0-49-1/command/update");
                WebResp = (HttpWebResponse)WebReq.GetResponse();
                WebResp.Close();
                WebReq = (HttpWebRequest)WebRequest.Create("http://192.168.2.25:8083/ZAutomation/api/v1/devices/ZWayVDev_zway_5-0-49-3/command/update");
                WebResp = (HttpWebResponse)WebReq.GetResponse();
                WebResp.Close();
                WebReq = (HttpWebRequest)WebRequest.Create("http://192.168.2.25:8083/ZAutomation/api/v1/devices/ZWayVDev_zway_5-0-49-5/command/update");
                WebResp = (HttpWebResponse)WebReq.GetResponse();
                WebResp.Close();

                WebReq = (HttpWebRequest)WebRequest.Create("http://192.168.2.25:8083/ZWaveAPI/Run/devices[5].instances[0].commandClasses[0x31].data[0x01].val.value");
                WebResp = (HttpWebResponse)WebReq.GetResponse();
                using (var reader = new StreamReader(WebResp.GetResponseStream()))
                {
                    double result2 = double.Parse(reader.ReadToEnd().ToString())*10;
                    int result1 = (int)result2;
                    double result = (double)result1/10; 
                    Logging.LogMessageToFile("Main - Temperature = [" + result.ToString() + "]", "SENSOR");
                    temperature = result;
                    reader.Close();                    
                }
                WebResp.Close();
                WebReq = (HttpWebRequest)WebRequest.Create("http://192.168.2.25:8083/ZWaveAPI/Run/devices[5].instances[0].commandClasses[0x31].data[0x03].val.value");
                WebResp = (HttpWebResponse)WebReq.GetResponse();
                using (var reader = new StreamReader(WebResp.GetResponseStream()))
                {
                    int result = Int32.Parse(reader.ReadToEnd().ToString()); // do something fun...
                    Logging.LogMessageToFile("Main - Luminiscence = [" + result.ToString() + "]", "SENSOR");
                    luminiscence = result;
                    reader.Close();
                }
                WebResp.Close();
                WebReq = (HttpWebRequest)WebRequest.Create("http://192.168.2.25:8083/ZWaveAPI/Run/devices[5].instances[0].commandClasses[0x31].data[0x05].val.value");
                WebResp = (HttpWebResponse)WebReq.GetResponse();
                using (var reader = new StreamReader(WebResp.GetResponseStream()))
                {
                    int result = Int32.Parse(reader.ReadToEnd().ToString()); // do something fun...
                    Logging.LogMessageToFile("Main - Humidity = [" + result.ToString() + "]", "SENSOR");
                    humidity = result;
                    reader.Close();
                }
                WebResp.Close();

                WebReq = (HttpWebRequest)WebRequest.Create("http://192.168.2.25:8083/ZWaveAPI/Run/devices[5].instances[0].commandClasses[0x30].data[1].level.value");
                WebResp = (HttpWebResponse)WebReq.GetResponse();
                using (var reader = new StreamReader(WebResp.GetResponseStream()))
                {
                    if (reader.ReadToEnd().ToString().ToUpper() == "FALSE")
                    {
                        Logging.LogMessageToFile("Main - Movement = [FALSE]", "SENSOR");
                        movement = false;
                    }
                    else
                    {
                        Logging.LogMessageToFile("Main - Movement = [TRUE]", "SENSOR");
                        movement = true;
                    }
                    reader.Close();
                }
                WebResp.Close();
                Debug.Print(DateTime.Now + "," + temperature.ToString().Substring(0,4) + "," + humidity.ToString() + "," + luminiscence.ToString() + "," + movement.ToString());
            }
            catch (Exception e)
            {
                Debug.Print("*********" + e.Message);
            }
        }



        public static double GetTemperature()
        {
            try
            {
                WebRequest WebReq = WebRequest.Create("http://192.168.2.25:8083/ZWaveAPI/Run/devices[5].instances[0].SensorMultilevel.Get()");
                WebReq = WebRequest.Create("http://192.168.2.25:8083/ZWaveAPI/Run/devices[5].instances[0].commandClasses[0x31].data[0x01].val.value");
                WebResponse WebResp = WebReq.GetResponse();
                using (var reader = new StreamReader(WebResp.GetResponseStream()))
                {
                    double result = double.Parse(reader.ReadToEnd().ToString()); // do something fun...
                    Logging.LogMessageToFile("Main - Temperature = [" + result.ToString() + "]", "SENSOR");
                    return result;
                }
            }
            catch
            {
                return -999;
            }
        }

        public static int GetLuminiscence()
        {
            try
            {
                WebRequest WebReq = WebRequest.Create("http://192.168.2.25:8083/ZWaveAPI/Run/devices[5].instances[0].SensorMultilevel.Get()");
                WebReq = WebRequest.Create("http://192.168.2.25:8083/ZWaveAPI/Run/devices[5].instances[0].commandClasses[0x31].data[0x03].val.value");
                WebResponse WebResp = WebReq.GetResponse();
                using (var reader = new StreamReader(WebResp.GetResponseStream()))
                {
                    int result = Int32.Parse(reader.ReadToEnd().ToString()); // do something fun...
                    Logging.LogMessageToFile("Main - Luminiscence = [" + result.ToString() + "]", "SENSOR");
                    return result;
                }
            }
            catch
            {
                return -999;
            }
        }

        public static int GetHumidity()
        {
            try
            {
                WebRequest WebReq = WebRequest.Create("http://192.168.2.25:8083/ZWaveAPI/Run/devices[5].instances[0].SensorMultilevel.Get()");
                WebReq = WebRequest.Create("http://192.168.2.25:8083/ZWaveAPI/Run/devices[5].instances[0].commandClasses[0x31].data[0x05].val.value");
                WebResponse WebResp = WebReq.GetResponse();
                using (var reader = new StreamReader(WebResp.GetResponseStream()))
                {
                    int result = Int32.Parse(reader.ReadToEnd().ToString()); // do something fun...
                    Logging.LogMessageToFile("Main - Humidity = [" + result.ToString() + "]", "SENSOR");
                    return result;
                }
            }
            catch
            {
                return -999;
            }
        }

        public static bool ZwaveServerStatusOK()
        {
            // Common testing requirement. If you are consuming an API in a sandbox/test region, uncomment this line of code ONLY for non production uses.
            // System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

            var request = System.Net.WebRequest.Create("http://private-anon-12841c194-zwayhomeautomation.apiary-proxy.com/ZAutomation/api/v1/status") as System.Net.HttpWebRequest;
            request.KeepAlive = true;

            request.Method = "GET";

            request.Accept = "application/json";
            request.ContentLength = 0;

            string responseContent = null;
            using (var response = request.GetResponse() as System.Net.HttpWebResponse)
            {
                using (var reader = new System.IO.StreamReader(response.GetResponseStream()))
                {
                    responseContent = reader.ReadToEnd();
                }
            }
            return true;
        }

        public static void TestJDom(string s)
        {
            var request = System.Net.WebRequest.Create("http://private-anon-12841c194-zwayhomeautomation.apiary-proxy.com/ZAutomation/api/v1/devices/1") as System.Net.HttpWebRequest;
            request.KeepAlive = true;

            request.Method = "PUT";

            request.ContentType = "application/json";
            byte[] byteArray = System.Text.Encoding.UTF8.GetBytes("{\"id\":\"ZWayVDev_20:0:49:1\",\"deviceType\":\"probe\",\"metrics\":{\"probeTitle\":\"Temperature\",\"scaleTitle\":\"°C\",\"level\":7.599999904632568,\"title\":\"Temperature Sensor\",\"iconBase\":\"zwave\"},\"tags\":[],\"location\":null,\"updateTime\":1387882443}");
            request.ContentLength = byteArray.Length;
            using (var writer = request.GetRequestStream()) { writer.Write(byteArray, 0, byteArray.Length); }

            string responseContent = null;
            using (var response = request.GetResponse() as System.Net.HttpWebResponse)
            {
                using (var reader = new System.IO.StreamReader(response.GetResponseStream()))
                {
                    responseContent = reader.ReadToEnd();
                }
            }
        }
    }
}
