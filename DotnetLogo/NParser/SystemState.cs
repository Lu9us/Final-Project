using NParser.Types.Agents;
using System;
using System.Collections.Generic;
using System.Text;

namespace NParser
{
   public class SystemState
    {
        public SystemState()
        {
            patches = new Patch[50,50];

            for (int i = 0; i < 50; i++)
            {
                for (int j = 0; j < 50; j++)
                {
                    patches[i, j] = new Patch(i, j);

                }

            }


        }

        public Patch[,] patches;

       


    }
}
