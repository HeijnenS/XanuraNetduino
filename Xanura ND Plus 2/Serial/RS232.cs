using System;
using System.IO;
using System.IO.Ports;
using Microsoft.SPOT;

namespace Domotica
{
    class RS232
    {
        public event ReceivedDataEventHandler DataReceived;
        SerialPort sp = new SerialPort(Serial.COM1, 19200, Parity.None, 8, StopBits.One);
        public bool debug = false;

        public RS232()
        {
            sp.BaudRate = 19200;
            sp.DataBits = 8;
            sp.Parity = Parity.None;
            sp.StopBits = StopBits.One;
            try
            {
                sp.Open();
                sp.DataReceived += new SerialDataReceivedEventHandler(sp_DataReceived);
            }
            catch (Exception e)
            {
                Logging.LogMessageToFile(this.ToString() + "-" + e.Message, "RS232");
            }
        }

        ~RS232()
        {
            sp.Close();
        }

        private void sp_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                string tempString = "";
                SerialPort port = (SerialPort)sender;
                byte[] bytes = new byte[port.BytesToRead];
                if (bytes.Length == 0) //after every query transmit, follow 4 answers (only the first one has data)
                {
                    return;
                }
                port.Read(bytes, 0, bytes.Length);
                try
                {
                    tempString = new string(System.Text.Encoding.UTF8.GetChars(bytes));
                    Logging.LogMessageToFile(tempString, "RS232");
                    DataReceived(this, new ReceivedDataEventArgs(tempString));
                }
                catch (Exception excep)
                {
                    tempString = "";
                    Logging.LogMessageToFile("error in tempString = new string(System.Text.Encoding.UTF8.GetChars(bytes)); " + excep.Message.ToString(), "RS232");
                }
            }
            catch (Exception ex)
            {
                Logging.LogMessageToFile(this.ToString() + "-" + ex.Message, "RS232");
            }
        }

        public void Write(string txtLine)
        {
            try
            {
                Logging.LogMessageToFile(this.ToString() + "-" + txtLine, "RS232");
                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(txtLine);
                //Debug.Print(DateTime.UtcNow.ToString() + ", SENDED " + txtLine);
                sp.Write(buffer, 0, buffer.Length);
                if (debug)
                {
                    Debug.Print("Write to gateway - " + DateTime.Now.ToString() + " - " + txtLine);
                }
            }
            catch (Exception e)
            {
                Logging.LogMessageToFile(this.ToString() + "-" + e.Message, "RS232");
            }
        }
    }
}



