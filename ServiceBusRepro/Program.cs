using BOLD.EventCore.ServiceModel;
using Microsoft.ServiceBus.Messaging;
using NLog;
using ServiceBusRepro.TopicConfiguration;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceBusRepro
{
    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                DoProcessAsync().Wait();
            }
        }

        public static async Task DoProcessAsync()
        {
            var config = (ConfigurationManager.GetSection("TopicSettingSection") as TopicSettingSection);
            var events = new List<Event>();
            List<Task> tasks = new List<Task>();
            foreach (var topic in config.Instances.Cast<TopicSetting>().ToList())
            {
                var result = await DownloadEvent(topic);
                events.AddRange(result);
                LogManager.GetCurrentClassLogger().Info("DownloadDocEvent (" + topic.TopicName + ":" + topic.SubscriptionName + ") Total events downloaded : " + result.Count);
            }
        }

        private static async Task<List<Event>> DownloadEvent(TopicSetting topic)
        {
            ConcurrentBag<Event> events = new ConcurrentBag<Event>();
            IEnumerable<BrokeredMessage> messages = null;
            var subClient = await ServiceBusManager.GetSubscriptionClient(topic);
            messages = await subClient.ReceiveBatchAsync(topic.MessageCount);

            Parallel.ForEach(messages, message =>
            {
                var temp = message.GetBody<Event>();
                events.Add(temp);
            });

            foreach (var item in messages)
            {
                await item.CompleteAsync();
            }

            return events.ToList();
        }
    }


}
