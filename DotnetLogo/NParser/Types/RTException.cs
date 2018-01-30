using System;
using System.Collections.Generic;
using System.Text;

namespace NParser.Types
{
   public class RTException:Exception
    {
        public RTException(string message)
        { RTMESSAGE = message; }
        private string RTMESSAGE { get; set; }
        public override string ToString()
        {
            return RTMESSAGE;
        }
    }
}
