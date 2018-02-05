using NParser.Types.Agents;
using System;
using System.Collections.Generic;
using System.Text;

namespace NParser.Runtime
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
        private Dictionary<string,Function> registeredFunctions = new Dictionary<string, Function>();
       
        public void AddFunction(Function function)
        {
            registeredFunctions.Add(function.name, function);
        }


    }
}
