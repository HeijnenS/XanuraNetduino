using System;
using Microsoft.SPOT;

namespace Domotica
{
    public class ReceivedDataEventArgs:EventArgs
    {
        public string ReceivedData { get; set; }
        public ReceivedDataEventArgs(string receivedData)
        {
            ReceivedData = receivedData;
        }
    }
}
