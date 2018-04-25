using System;
using System.Collections.Generic;
using System.Text;

namespace NParser.Types
{
   public class Integer:NetLogoObject
    {
        public override object value { get { return val; } set { val = int.Parse(value.ToString()); } }
        public  int val { get; set; }
    }
}
