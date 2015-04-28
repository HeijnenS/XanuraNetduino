using System;
using Microsoft.SPOT;

namespace mySecondtry
{
    class ShortA10Message
    {
        //INCOMING  A01A01 AONAON
        // public method analyze to start writing to private local data
        // public methods to retreive private local data
        public const string Space = " ";

        public string A10Command { get; set; }
        public string remainder { get; set; }
        public string commando { get; set; }
        public string address { get; set; }

        public string firstCompleteStatus = "";
        public string firstCompleteAddress = "";
        public string firstStatus = "";
        public string firstAddress = "";
        private char firstDeviceGroup = ' ';
        private int firstDeviceNumber = 0;

        public string secondCompleteStatus = "";
        public string secondCompleteAddress = "";
        public string secondStatus = "";
        public string secondAddress = "";
        private char secondDeviceGroup = ' ';
        private int secondDeviceNumber = 0;

        //private Boolean IsNumeric (System.Object Expression)
        //{
        //    if(Expression == null || Expression is DateTime)
        //        return false;

        //    if(Expression is Int16 || Expression is Int32 || Expression is Int64 || Expression is Decimal || Expression is Single || Expression is Double || Expression is Boolean)
        //        return true;

        //    try 
        //    {
        //        if(Expression is string)
        //            Double.Parse(Expression as string);
        //        else
        //            Double.Parse(Expression.ToString());
        //            return true;
        //        } catch {} // just dismiss errors but return false
        //        return false;
        //    }
        //}

        public string saveStatus(string message, ref string status)
        {//"AONAON" or "AOFFAOFF" or "AON" or "AOFF" 
            //statusMessage = first occurance of "AON"
            try
            {
                if (message.Length < 3)
                {
                    Logging.LogMessageToFile(this.ToString() + "-" + "message not okay => " + message, "ALL");
                    return message;
                }

                string commandOn = firstDeviceGroup.ToString() + "ON";
                string commandOff = firstDeviceGroup.ToString() + "OFF";

                if (StringHandler.Left(message, 0, 3) == commandOn)
                {
                    status = "ON";
                    //Debug.Print("Status = " + status);
                    return message.Substring(3, message.Length - 3);
                }
                if (StringHandler.Left(message, 0, 4) == commandOff)
                {
                    status = "OFF";
                    //Debug.Print("Status = " + status);
                    return message.Substring(4, message.Length - 4);
                }
                Logging.LogMessageToFile(this.ToString() + " - saveStatus - " + "error in message " + message + "[Should be A01A01AONAON for example]", "ALL");
                return "";
            }
            catch (Exception e)
            {
                Logging.LogMessageToFile(this.ToString() + " saveStatus - Catch " + e.Message, "ALL");
                return "";
            }
        }


        private string saveAddress(string message, ref string completeAddress) //receives full message without protocol => A01........
        {
            try
            {
                if (message.Length > 2)
                {
                    int adresGroup = message[0];
                    int adresTens = message[1];
                    int adresOnes = message[2];
                    if (adresTens > 47 && adresTens < 58 && adresOnes > 47 && adresOnes < 58 && adresGroup > 64 && adresGroup < 81)
                    {//format checks out [A-P][0-1][0-9]
                        firstDeviceNumber = Convert.ToInt16(StringHandler.Left(message, 1, 2));
                        if (firstDeviceNumber < 16)
                        {
                            firstDeviceGroup = message[0]; //to be used with status analyzing
                            completeAddress = StringHandler.Left(message, 0, 3);
                            //Debug.Print("Address = " + completeAddress);
                        }
                    }
                    if (message.Length > 3)
                    {
                        return message.Substring(3, message.Length - 3); //return the remainder of the message
                    }
                    else
                    {
                        return "";
                    }
                }
                else
                {
                    Logging.LogMessageToFile(this.ToString() + "-" + "message is not correct => " + message, "ALL");
                    return "";
                }
            }
            catch (Exception e)
            {
                Logging.LogMessageToFile(this.ToString() + "-" + e.Message, "ALL");
                return "";
            }
        }


        public void analyzeData(string A10Message)
        {//without protocolformat and checksum => "A01" or "A01A01 AONAON" or "A01A01" or "AON" or "AONAON" or "AONAON A02A02 AOFFAOFF"
            Logging.LogMessageToFile(this.ToString() + " - analyzeData - " + "Incoming Message =>" + A10Message, "ALL");
            string message = "";
            try
            {
                if (A10Message.Length == 0)
                {
                    return;
                }

                if (A10Message[0].ToString() == Space)
                {
                    message = A10Message.Substring(1, A10Message.Length - 1);
                }
                else
                {
                    message = A10Message;
                }
                //Check which parts are allready analyzed in a previous message that was not complete
                // 1 analyze first address (pass full message wtithout protocol)
                // 2 analyze second address (pass message without first adddress)
                // 3 analyze first status   (pass message without firstr/second address)
                // 4 analyze second status  (pass message ......)
                // 5 write reaminder 

                if (message[1] == '0' || message[1] == '1')//A01 or A11 and not AON 
                {
                    if (firstCompleteAddress == "")
                    {
                        Logging.LogMessageToFile(this.ToString() + " - analyzeData - " + "First address to be filled with =>" + message, "ALL");
                        message = saveAddress(message, ref firstCompleteAddress);
                    }
                    if (secondCompleteAddress == "" && (message.Length > 0))
                    {
                        Logging.LogMessageToFile(this.ToString() + " - analyzeData - " + "Second address to be filled with =>" + message, "ALL");
                        message = saveAddress(message, ref secondCompleteAddress);
                    }
                }
                message = message.Trim();
                if (message[1] == 'O')
                {
                    if (firstCompleteStatus == "" && (message.Length > 0))
                    {
                        Logging.LogMessageToFile(this.ToString() + " - analyzeData - " + "First status to be filled with =>" + message, "ALL");
                        message = saveStatus(message, ref firstCompleteStatus);
                    }
                    if (secondCompleteStatus == "" && (message.Length > 0))
                    {
                        Logging.LogMessageToFile(this.ToString() + " - analyzeData - " + "Second status to be filled with =>" + message, "ALL");
                        message = saveStatus(message, ref secondCompleteStatus);
                    }
                }
                remainder = message;
            }
            catch (Exception e)
            {
                Logging.LogMessageToFile(this.ToString() + " - analyzeData - Catch =>" + e.Message, "ALL");
            }
        }

        public bool hasDeviceGroup()
        {
            return (firstDeviceGroup != ' ' && secondDeviceGroup != ' ');
        }

        public bool hasDeviceNumber()
        {
            return (firstDeviceNumber != 0 && secondDeviceNumber != 0);
        }

        public bool hasStatus()
        {
            return ((firstStatus == "ON" || firstStatus == "OFF") && (secondStatus == "ON" || secondStatus == "OFF"));
        }

        public bool hasCompleteMessage()
        {
            //Debug.Print(hasCompleteAddress() + "[" + firstCompleteAddress + "]," + hasCompleteStatus() +  "[" + firstCompleteStatus + "]");
            return hasCompleteAddress() && hasCompleteStatus();
        }

        public bool hasCompleteAddress()
        {
            return firstCompleteAddress != "" && secondCompleteAddress != "";
        }

        public bool hasCompleteStatus()
        {
            return firstCompleteStatus != "" && secondCompleteStatus != "";
        }

        public string getCompleteMessage()
        {
            return firstDeviceGroup.ToString() + firstDeviceNumber.ToString() + firstStatus;
        }

        public ShortA10Message(string A10Message)
        {
            remainder = "";
            analyzeData(A10Message);
        }

        public bool hasRemainder()
        {
            return remainder != "";
        }


    }
}
