using System;
using Microsoft.SPOT;
using System.Threading;

namespace Domotica
{
    /// <summary>
    ///  This class performs an important function.
    /// </summary>
    /// 
    class XanuraProtocolHandler
    {
        #region Uitgelezen data xanura
        //INCOMING  $<2800! A[1]012631A[1]012631 A01A01 AONAON59#
        //OUTGOING  $>2800 A[1]01A131A[1]01A131 A01A01 AONAON
        // -----------------------ON---------------- ---------BGT-------- --------------------OFF-------------
        //$<2800! A[1]010731A[1]010731 A01A01 AONAON A[1]010E31A[1]010E31 A[1]010031A[1]010031 A01A01 AOFFAOFFB9#
        // protocolformat|device type|addres type|CTX35 command|Letter Code| space | letter code | letter code| checksum| protocol format
        //      $>  $<      28          00              A           01        ' '       A             ON            CC            #
        //      $>  $<      28          00              A           01        ' '       A             ON            CC            #
        #endregion
        public bool debug = false;
        public event ReceivedDataEventHandler DataReceivedFromSerial;
        public static RS232 Serial = new RS232();
        public const string IncomingMessage = "$<";
        public const string OutgoingMessage = "$>";
        public const string DeviceType = "28";
        public const string AddressType = "001";
        public const string Acknowledged = "!";
        public const string NotAcknowledged = "?";
        public const string Space = " ";
        private const char space = ' ';
        private enum message {None =0, ShortMessage =1, LongMessage =2 };
        private ShortA10Message activeShortMessage;
        private string bufferString = "";
        public bool SDCardPresent = true;

        //public Timer OneHertzTimer;

        public XanuraProtocolHandler()
        {
            Serial.DataReceived += new ReceivedDataEventHandler(xph_DataReceived);
            //OneHertzTimer = new Timer(new TimerCallback(Query), null, 0, 1000);            
        }

        public void Query()
        {//request buffer dump   
            try
            {
                Serial.Write("$>2800008C#");
            }
            catch (Exception e)
            {
                Logging.LogMessageToFile(this.ToString() + "-" + e.Message, "ALL");
            }
        }


        private void xph_DataReceived(object sender, ReceivedDataEventArgs e)
        {
            try
            {
                if(e.ReceivedData!="$<2800!4B#")
                {
                    Logging.LogMessageToFile(this.ToString() + "- xph_DataReceived - Received => " + e.ReceivedData, "ALL");
                }
                RS232 com = (RS232)sender;
                string message = "";
                int maxNumberofWhiles = 100;
                int numberOfWhiles = 0;
                //pass message on to protocol translator and get translation
                //take into account that multiple submessages can exist in one transmission $<2800?69#$<2800?69#$<2800?69#
                // handle each # as a complete message from CTX35
                // find messages in one message that ends with #
                bufferString = bufferString + e.ReceivedData;
                do // loop until there's no end char in the message anymore
                {
                    if (bufferString.IndexOf("#") > 0)
                    {
                        //at least one message (doesn't have to be a complete one).
                        message = bufferString.Substring(0, bufferString.IndexOf("#") + 1);
                        TranslateIncomingMessage(message);
                        //There are more than one messages
                        if (bufferString.Length > bufferString.IndexOf("#") + 1)
                        {
                            //remove the first message and store the rest in bufferstring
                            bufferString = bufferString.Substring(bufferString.IndexOf("#") + 1, (bufferString.Length - (bufferString.IndexOf("#") + 1)));
                        }
                        else
                        {
                            //the message handled was the only message, clear bufferstring
                            bufferString = "";
                        }
                    }
                    else
                    {
                        return;
                    }
                    numberOfWhiles++;
                    if (numberOfWhiles >= maxNumberofWhiles)
                    {
                        Logging.LogMessageToFile(this.ToString() + "- Error in While searching for end of message # in " + bufferString, "ALL");
                    }
                }
                while (bufferString.IndexOf("#") > 0 && numberOfWhiles < maxNumberofWhiles);
            }
            catch (Exception ex)
            {
                Logging.LogMessageToFile(this.ToString() + " - xph_DataReceived => " + ex.Message, "ALL");
            }
        }

        public void SendMessage(string message)
        {
            try
            {
                message = OutgoingMessage + DeviceType + AddressType + message;
                message = message + CreateCheckSum(message) + "#";
                Serial.Write(message);
            }
            catch (Exception e)
            {
                Logging.LogMessageToFile(this.ToString() + "-" + e.Message, "ALL");
            }
        }

        private string CreateCheckSum(string txtLine)
        {
            try
            {
                int DecSum = 0;
                string CheckSum = "";
                foreach (char c in txtLine)
                {
                    int Dec = (int)c;
                    DecSum = DecSum + Dec;
                }
                CheckSum = DecSum.ToString("X");
                CheckSum = StringHandler.Right(CheckSum, 2);
                return CheckSum;
            }
            catch (Exception e)
            {
                Logging.LogMessageToFile(this.ToString() + "-" + e.Message, "ALL");
                return "";
            }
        }

        private void TranslateIncomingMessage(string A10Message)  // input is raw data (one message) from serial port, output is adres and command
        {

            # region protocolhandling
            /*hier moet worden gekeken of een bbericjt compleet is en eventueel wachten op een restant
             * 
             */
            # endregion
            try
            {
                //Logging.LogMessageToFile(this.ToString() + " - TranslateIncomingMessage - message => " + A10Message, "ALL");
                //escape if message is an empty acknowledge
                string message = "";
                string myAddress = "";
                string myCommand = "";
                int numberOfWhiles = 0;
                int maxNumberOfWhiles = 100;

                if (A10Message == "$<2800!4B#")
                {
                    activeShortMessage = null;                    
                    //DataReceivedFromSerial(this, new ReceivedDataEventArgs(""));
                    //activeShortMessage = null;
                    return;
                }

                //escape if message is an buffer full
                if (A10Message == "$<2800!S0CE#")
                {
                    Logging.LogMessageToFile(this.ToString() + ", buffer vol [" + A10Message + "]", "ALL");
                    return;
                }

                //escape if received message is a not acknowledged respons
                if (A10Message == "$<2800?69")
                {
                    Logging.LogMessageToFile(this.ToString() + ", ?huh? [" + A10Message + "]", "ALL");
                    return;
                }

                if (A10Message.Length < 9)
                {// message does not contain at least $<2800CC#
                    Logging.LogMessageToFile(this.ToString() + ", bericht incompleet lengte [" + A10Message + "]  <9", "ALL");
                    return;
                }

                //escape if the CheckSum is incorrect
                if (CreateCheckSum(StringHandler.Left(A10Message, 0, A10Message.Length - 3)) != StringHandler.Left(StringHandler.Right(A10Message, 3), 0, 2))
                {
                    Logging.LogMessageToFile(this.ToString() + ", checksum niet ok =[" + A10Message + "]", "ALL");
                    return;
                }

                #region new method
                if (isLongMessage(A10Message))
                {
                    Logging.LogMessageToFile(this.ToString() + " - TranslateIncomingMessage => is long message , " + A10Message, "ALL");
                    if (isLongMessageComplete(A10Message))
                    {
                        Logging.LogMessageToFile(this.ToString() + " - TranslateIncomingMessage => is complete long message , " + A10Message, "ALL");
                    }
                }
                else if (isShortMessage(A10Message) || activeShortMessage!=null) //When remainde of short message comes its possible it isnt seen as shortmessage
                {
                    Logging.LogMessageToFile(this.ToString() + " - TranslateIncomingMessage => is short message , " + A10Message, "ALL");
                    message = A10Message;
                    do
                    {
                        if (activeShortMessage != null)
                        {
                            activeShortMessage.analyzeData(removeProtocol(message)); // previous short message had submitted an address
                        }
                        else
                        {
                            activeShortMessage = new ShortA10Message(removeProtocol(message)); //create new short message and check is analyzed correct                                                
                        }

                        if (activeShortMessage.hasCompleteMessage())
                        {
                            Logging.LogMessageToFile(this.ToString() + " complete message is ok, " + activeShortMessage, "ALL");
                            myAddress = activeShortMessage.firstCompleteAddress;
                            myCommand = activeShortMessage.firstCompleteStatus;
                            message = activeShortMessage.remainder;                            
                            activeShortMessage = null;
                            if (isShortMessage(message))
                            {
                                Logging.LogMessageToFile(this.ToString() + " new message, " + message, "ALL");
                                activeShortMessage = new ShortA10Message(message); //remainder after a complete message will start a new message if short message
                            }
                            DataReceivedFromSerial(this, new ReceivedDataEventArgs(myAddress + " " + myCommand));
                            //                    return myAddress + " " + myCommand;
                        }
                        else
                        {
                            message = activeShortMessage.remainder; //write remainder to message to iterative find other messages.
                            //bufferString = message;
                            //Logging.LogMessageToFile(this.ToString() + " - TranslateIncomingMessage => remainder set to buffer , " + bufferString, "ALL");
                        }

                        //Debug.Print("remainder : " + activeShortMessage.remainder);
                        //} while (activeShortMessage != null && message.Length > 0); //return to begin when message is not complete and remainder is not empty
                        numberOfWhiles++;
                    } while (message.Length > 0 && numberOfWhiles < maxNumberOfWhiles); //return to begin when message is not complete and remainder is not empty

                }
                else
                {
                    Logging.LogMessageToFile(this.ToString() + " - TranslateIncomingMessage => no short/long message found , " + A10Message, "ALL");
                }

                #endregion
/*
                #region old method
                //translate this message, it can contain multiple status reports
                //  check if it is a short message or a long message | a short message = A01A01 AONAON | long message = A[1]yadayada
                if ((A10Message.IndexOf('[') > 0) || (A10Message.IndexOf(']') > 0))
                {
                    return;
                }
                message = A10Message;
                do
                {
                    if (activeShortMessage != null) // no remainder expect beginning of a new message, new message begins with space
                    {
                        activeShortMessage.analyzeData(removeProtocol(message)); // previous short message had submitted an address
                    }
                    else
                    {
                        if (message.IndexOf("[")<0 && message.IndexOf("]")<0)
                        {
                            activeShortMessage = new ShortA10Message(removeProtocol(message)); //create new short message and check is analyzed correct                    
                        }                        
                    }

                    if (activeShortMessage.hasCompleteMessage())
                    {
                        Logging.LogMessageToFile(this.ToString() + " complete message is ok, " + activeShortMessage, "ALL");
                        myAddress = activeShortMessage.firstCompleteAddress;
                        myCommand = activeShortMessage.firstCompleteStatus;
                        message = activeShortMessage.remainder;
                        activeShortMessage = null;
                        DataReceivedFromSerial(this, new ReceivedDataEventArgs(myAddress + " " + myCommand));
                        //                    return myAddress + " " + myCommand;
                    }
                    else
                    {
                        message = activeShortMessage.remainder; //write remainder to message to iterative find other messages.
                    }

                    //Debug.Print("remainder : " + activeShortMessage.remainder);
                    //} while (activeShortMessage != null && message.Length > 0); //return to begin when message is not complete and remainder is not empty
                    numberOfWhiles++;
                } while (message.Length > 0 && numberOfWhiles < maxNumberOfWhiles); //return to begin when message is not complete and remainder is not empty

            #endregion
 */
            }
            catch (Exception e)
            {
                Logging.LogMessageToFile(this.ToString() + " - TranslateIncomingMessage => " + e.Message, "ALL");
            }
      }

        private bool isLongMessageComplete(string message)
        {//$<2800! A[1]010731A[1]010731??#
            bool answ = false;
            if (message.Length >= 30)
            {
                if (message.Substring(8, 10) == message.Substring(18, 10))
                {
                    answ = true;
                }
            }
            return answ;
        }

        private bool isShortMessageComplete(string message)
        {//$<2800! A01A01 AONAON??#
            bool answ = false;
            if (message.Length >= 24)
            {
                if (message.Substring(8, 3) == message.Substring(11, 3) && message.Substring(15, 3) == message.Substring(18, 3))
                {
                    answ = true;
                }
            }
            return answ;
        }

        private bool isLongMessage(string message)
        {//$<2800! A[1]010731A[1]010731??#
            bool answ = false;
            if (message.Length > 9)
            {
                if (message.Substring(0, 8) == "$<2800! ")
                {
                    if (message.Substring(9, 1) == "[")
                    {
                        answ = true;
                    }
                }
            }
            return answ;
        }


        private bool isShortMessage(string message)
        {//$<2800! A01A01 AONAON??#
            bool answ = false;
            if (message.Length > 9)
            {
                if (message.Substring(0, 8) == "$<2800! ")
                {
                    if (message.Substring(9, 1) == "0" || message.Substring(9, 1) == "1")
                    {
                        answ = true;
                    }
                }
            }
            return answ;
        }

        private string removeProtocol(string A10Message)
        {
            try
            {
                //if (A10Message.Split(space).GetValue(0).ToString() == "$<2800!")
                if (A10Message.Length > 7 && A10Message.Substring(0, 7) == "$<2800!")
                {
                    if (A10Message[7] == space)
                    {
                        return A10Message.Substring(8, A10Message.Length - 11); // remove "$<2800! " and checksum# that is 8+3=11 chars
                    }
                    else
                    {
                        return A10Message.Substring(7, A10Message.Length - 10); // remove "$<2800!" and checksum# that is 7+3=10 chars..... remainder can start without space after protocol
                    }
                }
                else
                {
                    if (A10Message[0] == space)
                    {
                        return A10Message; //there can be multiple messages in one transmission, the protocol will be removed the first time
                    }
                    else
                    {
                        return "";
                    }
                }
            }
            catch (Exception e)
            {
                Logging.LogMessageToFile(this.ToString() + "-" + e.Message, "ALL");
                return "";
            }
        }

        private string HandleShortProtocolMessage(string A10Message)
        {
            try
            {
                string secondZeroCrossingRaw = A10Message.Split(space).GetValue(1).ToString();
                string secondZeroCrossing = StringHandler.Left(secondZeroCrossingRaw, 0, secondZeroCrossingRaw.Length);
                int group = secondZeroCrossing[0];

                if (group > 64 && group < 81) //First symbol is group address A->P
                {
                    int adresTens = secondZeroCrossing[1];
                    int adresOnes = secondZeroCrossing[2];
                    if (adresTens > 47 && adresTens < 58 && adresOnes > 47 && adresOnes < 58)
                    {
                        //Short message found
                        int addressNumber = Convert.ToInt16(StringHandler.Left(secondZeroCrossing, 1, 2));
                        string address = StringHandler.Left(secondZeroCrossing, 0, 3);
                        if (addressNumber > 16)
                        {
                            //address can not be greater then 16
                            return "";
                        }

                        //find command in thirdZeroCrossing, should be sometyhing like AONAON or AOFFAOFF

                    }
                    else
                    {
                        return "";
                    }
                }

                return "";
            }
            catch(Exception e)
            {
                Logging.LogMessageToFile(this.ToString() + "-" + e.Message, "ALL");
                return "";
            }
        }



        
    }
}