using Microsoft.ServiceBus.Messaging;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceBusRepro.TopicConfiguration
{
    public class TopicSettingSection : ConfigurationSection
    {
        [ConfigurationProperty("", IsRequired = true, IsDefaultCollection = true)]
        public TopicSettingCollection Instances
        {
            get { return (TopicSettingCollection)this[""]; }
            set { this[""] = value; }
        }
    }
    public class TopicSettingCollection : ConfigurationElementCollection
    { 
        protected override ConfigurationElement CreateNewElement()
        {
            return new TopicSetting();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            //set to whatever Element Property you want to use for a key
            return ((TopicSetting)element).TopicName;
        }

        public new TopicSetting this[string elementName]
        {
            get
            {
                return this.OfType<TopicSetting>().FirstOrDefault(item => item.TopicName == elementName);
            }
        }
       
    }
}
