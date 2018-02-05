﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NParser.Utils
{
   public static class StringUtilities
    {
        /// <summary>
        /// functionally simlar to string.split however it keeps the delimiters in the string
        /// </summary>
        /// <param name="delims"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string[] split(char[] delims, string data)
        {
            List<string> split = new List<string>();
            string workingString = new string(data.ToCharArray());
           
            int i = 0;

            while(i<workingString.Length)
            {
                if (delims.Contains(workingString[i]))
                {


                    if (!string.IsNullOrEmpty(workingString.Substring(0, i)))
                    {
                        split.Add(workingString.Substring(0, i));  
                    }
                    string s = "";
                    s += workingString[i];
                    split.Add(s);


                    if (i < workingString.Length - 1)
                    {
                        string temp = "";
                        for (int x = i + 1; x < workingString.Length; x++)
                        {
                            temp += workingString[x];
                        }
                        workingString = temp;
                    }
                    else
                    { break; }
                    i = 0;

                }
                else
                {
                    i++;
                }  
            }
            if (workingString.Length > 0)
            {
                split.Add(workingString);
            }

            return split.ToArray();
        }



    }
}
