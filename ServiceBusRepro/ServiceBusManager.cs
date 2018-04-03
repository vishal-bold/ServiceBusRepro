using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using NLog;
using ServiceBusRepro.TopicConfiguration;
using System;
using System.Collections.Concurrent;
using System.Configuration;
using System.Threading.Tasks;

namespace ServiceBusRepro
{
    public static class ServiceBusManager
    {
        static NamespaceManager _ns;
        static MessagingFactorySettings _mfs;
        static ServiceBusManager()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["Microsoft.ServiceBus.ConnectionString"].ConnectionString;
            _ns = NamespaceManager.CreateFromConnectionString(connectionString);
            _mfs = new MessagingFactorySettings();
            _mfs.TransportType = TransportType.Amqp;
            _mfs.TokenProvider = TokenProvider.CreateSharedAccessSignatureTokenProvider(ConfigurationManager.AppSettings["Microsoft.ServiceBus.TokenKey"], ConfigurationManager.AppSettings["Microsoft.ServiceBus.Token"]);
        }
        static ConcurrentDictionary<string, Tuple<MessagingFactory, SubscriptionClient>> _messagingFactories = new ConcurrentDictionary<string, Tuple<MessagingFactory, SubscriptionClient>>();
        private static MessagingFactory GetMessageFactory()
        {
            MessagingFactory messagingFactory = MessagingFactory.Create(_ns.Address, _mfs);
            return messagingFactory;
        }


        public static async Task<SubscriptionClient> GetSubscriptionClient(TopicSetting topicDetail)
        {
            SubscriptionClient subscriptionClient = null;
            try
            {
                var key = topicDetail.TopicName + Constants.COLON + topicDetail.SubscriptionName;
                if (!_messagingFactories.ContainsKey(key))
                {
                    key = topicDetail.TopicName + Constants.COLON + topicDetail.SubscriptionName;
                    if (!_messagingFactories.ContainsKey(key))
                    {
                        var messagingFactory = GetMessageFactory();
                        if (!await _ns.SubscriptionExistsAsync(topicDetail.TopicName, topicDetail.SubscriptionName)) // TODO :
                        {
                            SubscriptionDescription subscriptionDescription = new SubscriptionDescription(topicDetail.TopicName, topicDetail.SubscriptionName);
                            subscriptionDescription.EnableBatchedOperations = true;
                            await _ns.CreateSubscriptionAsync(subscriptionDescription);
                        }

                        subscriptionClient = messagingFactory.CreateSubscriptionClient(topicDetail.TopicName, topicDetail.SubscriptionName, topicDetail.RecieveMode);
                        subscriptionClient.PrefetchCount = topicDetail.PrefetchCount;
                        _messagingFactories[key] = Tuple.Create<MessagingFactory, SubscriptionClient>(messagingFactory, subscriptionClient);
                        LogManager.GetCurrentClassLogger().Info("ServiceBusManager created SubscriptionClient:{@SubscriptionName} for TopicName:{@TopicName} in RecieveMode:{@ReceiveMode} with key:{@Key}", topicDetail.SubscriptionName, topicDetail.TopicName, topicDetail.RecieveMode, key);
                    }
                }
                subscriptionClient = _messagingFactories[key].Item2;
                //Logger.GetInstance().Information("Subscriptionclient return for key:{@Key}", key);
            }
            catch (Exception ex)
            {
                LogManager.GetCurrentClassLogger().Error(ex, "ServiceBusManager error creating SubscriptionClient:{@SubscriptionName} for TopicName:{@TopicName} in ReceiveMode:{@ReceiveMode}", topicDetail.SubscriptionName, topicDetail.TopicName, topicDetail.RecieveMode);
            }
            return subscriptionClient;
        }

        internal static void CloseConnection()
        {
            foreach (var item in _messagingFactories.Keys)
            {
                try
                {
                    _messagingFactories[item].Item2.Close();
                    _messagingFactories[item].Item1.Close();
                }
                catch (Exception ex)
                {
                    LogManager.GetCurrentClassLogger().Error(ex, "ServiceBusManager error closing subscriptionClient for Key : {Key}", item);
                }
            }
        }
    }
}
