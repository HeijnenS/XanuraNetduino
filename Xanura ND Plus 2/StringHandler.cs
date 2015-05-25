using System;
using Microsoft.SPOT;

namespace Domotica
{
    class StringHandler
    {

        public static string Left(string txtLine, int start, int length)
        {
            try
            {
                //we start at 0 since we want to get the characters starting from the
                //left and with the specified lenght and assign it to a variable
                string result = "";
                if (txtLine.Length > length)
                {
                    result = txtLine.Substring(start, length);
                    //return the result of the operation
                }
                else
                {
                    result = txtLine;
                }

                return result;
            }
            catch (Exception e)
            {
                Logging.LogMessageToFile(e.Message, "ALL");
                return "";
            }
        }

        public static int CountCharInstr(string txtLine, char findChar)
        {
            try
            {
                Char[] myChars;
                int count = 0;
                myChars = txtLine.ToCharArray();
                foreach (char myChar in myChars)
                {
                    if (myChar == findChar)
                    {
                        count = count + 1;
                    }
                }
                return count;
            }
            catch (Exception e)
            {
                Logging.LogMessageToFile(e.Message, "ALL");
                return 0;
            }
        }

        public static string Right(string txtLine, int Length)
        {
            try
            {
                if (Length < txtLine.Length)
                {
                    //we start at 0 since we want to get the characters starting from the
                    //left and with the specified lenght and assign it to a variable
                    string result = txtLine.Substring(txtLine.Length - Length, Length);
                    //return the result of the operation
                    return result;
                }
                else
                {
                    return txtLine;
                }
            }
            catch (Exception e)
            {
                Logging.LogMessageToFile(e.Message, "ALL");
                return "";
            }

        }
    }
}
