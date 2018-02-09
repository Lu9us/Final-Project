using System;
using System.Collections.Generic;
using System.Text;

namespace NParser.Types
{
   public class NSString:NetLogoObject
    {
        public string val = "";
        public override object value { get { return val; } set { } }
    }
}
