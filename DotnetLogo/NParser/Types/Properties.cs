using System;
using System.Collections.Generic;
using System.Text;

namespace NParser.Types
{
  public  class AgentProperties
    {
        internal delegate NetLogoObject OnPropertyChange(NetLogoObject newVal);

        internal AgentProperties()
        {
            properties = new Dictionary<string, NetLogoObject>();
            protectedType = new Dictionary<string, Type>();
            protectedValue = new List<string>();
            Events = new Dictionary<string, OnPropertyChange>();
        }
        internal Dictionary<string, NetLogoObject> properties { get; set; }
        internal Dictionary<string, Type> protectedType { get; set; }
        internal List<string> protectedValue { get; set; }
        internal Dictionary<string, OnPropertyChange> Events { get; set; }
        public bool valueChanged = false;
        public NetLogoObject GetProperty(string name)
        {
            try
            {
                if (properties.ContainsKey(name))
                {
                    return properties[name];
                }
                else
                {
                    return null;
                }
            }
            catch (Exception e)
            {
                return null;
            }

        }
        public void SetProperty(string name, NetLogoObject value)
        {
            if (protectedValue.Contains(name))
            {
                throw new RTException("property " + name + " value is protected");
            }
            if (protectedType.ContainsKey(name) && protectedType[name] != value.GetType())
            {
                throw new RTException("property " + name + " type protected ");
            }
            try
            {
             
                properties[name] = value;
                if (Events.ContainsKey(name))
                { Events[name].Invoke(value); }
                valueChanged = true;
            }
            catch (Exception e)
            {
                throw new RTException(name + " PROPERTY NOT FOUND");
            }
        }

        public void AddProperty(string name, NetLogoObject value)
        {
            if (!properties.ContainsKey(name))
            {
                properties.Add(name, value);
            }
        }
    }
}
