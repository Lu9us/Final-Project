﻿
using System;
using System.Collections.Generic;
using System.Text;

namespace NParser.Types
{
    public class NSString : NetLogoObject
    {

        types type;
        enum types
        {
            Black = 0,
            DarkRed = 1,
            Red = 3
        }
    }
}
