using Microsoft.ServiceBus.Messaging;
using System;
using System.Configuration;

namespace ServiceBusRepro.TopicConfiguration
{
    public class TopicSetting : ConfigurationElement
    {
        [ConfigurationProperty("TopicName", IsRequired = true, IsKey = true)]
        [StringValidator]
        public string TopicName
        {
            get { return (string)this["TopicName"]; }
            set { this["TopicName"] = value; }
        }

        [ConfigurationProperty("SubscriptionName", IsRequired = true)]
        [StringValidator]
        public string SubscriptionName
        {
            get { return (string)this["SubscriptionName"]; }
            set { this["SubscriptionName"] = value; }
        }

        [ConfigurationProperty("RecieveMode", IsRequired = true)]
        public ReceiveMode RecieveMode
        {
            get { return (ReceiveMode)Enum.Parse(typeof(ReceiveMode), Convert.ToString(this["RecieveMode"])); }
            set { this["RecieveMode"] = value; }
        }

        [ConfigurationProperty("PrefetchCount", IsRequired = true)]
        [IntegerValidator]
        public int PrefetchCount
        {
            get { return (int)this["PrefetchCount"]; }
            set { this["PrefetchCount"] = value; }
        }

        [ConfigurationProperty("MessageCount", IsRequired = true)]
        [IntegerValidator]
        public int MessageCount
        {
            get { return (int)this["MessageCount"]; }
            set { this["MessageCount"] = value; }
        }
    }
}