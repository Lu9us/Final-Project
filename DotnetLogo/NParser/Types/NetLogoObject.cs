using System;
using System.Collections.Generic;
using System.Text;

namespace NParser.Types
{
    public class NetLogoObject
    {
         public virtual object value { get { return ptrID; } set { } }
         internal string ptrID;
        public override string ToString()
        {
            return value.ToString();
        }
    }
}

 
