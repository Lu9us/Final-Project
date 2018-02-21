using System;
using System.Collections.Generic;
using System.Text;

namespace NParser.Types
{
  public class Boolean  : NetLogoObject
    {
        public bool val;

        public override object value { get { return val; } set { val = bool.Parse((string)value); } }
        public override string ToString()
        {
            return val.ToString();
        }

    }
}
