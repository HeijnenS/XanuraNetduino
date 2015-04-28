using System;
using Microsoft.SPOT;

namespace mySecondtry
{
    class Daix
    {
        public string address = "";
        public string status = "OFF";
        public string actuator = "";
        public string location = "";
        public string[] DirectlyLinkedOn_On = new string[5];
        public string[] DirectlyLinkedOff_Off = new string[5];
        public string[] DirectlyLinkedOn_Off = new string[5];
        public string[] DirectlyLinkedOff_On = new string[5];


        public string Group()
        {
            try
            {
                if (address.Length > 0)
                {
                    return StringHandler.Left(address, 0, 1);
                }
                else
                {
                    return "";
                }
            }
            catch
            {
                return "";
            }
        }
    }
}
