using System;
using System.Collections.Generic;
using System.Text;

namespace NParser.Types
{
  public abstract class AgentProperties
    {
        protected delegate NetLogoObject OnPropertyChange(NetLogoObject newVal);

        protected AgentProperties()
        {
            properties = new Dictionary<string, NetLogoObject>();
            protectedType = new Dictionary<string, Type>();
            protectedValue = new List<string>();
            Events = new Dictionary<string, OnPropertyChange>();
        }
        protected Dictionary<string, NetLogoObject> properties { get; set; }
        protected Dictionary<string, Type> protectedType { get; set; }
        protected List<string> protectedValue { get; set; }
        protected Dictionary<string, OnPropertyChange> Events { get; set; }
        public object GetProperty(string name)
        {
            try
            {

                return properties[name];
            }
            catch (Exception e)
            {
                throw new RTException(name + " PROPERTY NOT FOUND");
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
