﻿using System;
using System.Collections.Generic;
using System.Text;

namespace NParser.Types
{
  public class Number:NetLogoObject
    {
        public float val;

        public override object value { get { return val; } set { } }
        public override string ToString()
        {
            return val.ToString();
        }
    }
}
