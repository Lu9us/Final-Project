using System;
using System.Collections.Generic;
using System.Text;

namespace NParser.Types.Agents
{
    public class Patch : MetaAgent
    {
        public Patch(int x, int y)
        {
            this.x = x;
            this.y = y;
            Integer nx = new Integer(), ny = new Integer();
            nx.val = x;
            ny.val = y;
            properties.properties["X"] = nx;
            properties.properties["Y"] = ny;
            properties.protectedValue.Add("X");
            properties.protectedValue.Add("Y");
            properties.AddProperty("p-color", new NSString());
            properties.protectedType.Add("p-color",typeof(NSString));
            properties.properties["p-color"] = new NSString() { val = "black" };


        }
        public Patch()
        {}
    }
}
