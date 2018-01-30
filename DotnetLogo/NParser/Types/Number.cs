using System;
using System.Collections.Generic;
using System.Text;

namespace NParser.Types
{
  public class Number:NetLogoObject
    {
        public float val;
        public override string ToString()
        {
            return val.ToString();
        }
    }
}
