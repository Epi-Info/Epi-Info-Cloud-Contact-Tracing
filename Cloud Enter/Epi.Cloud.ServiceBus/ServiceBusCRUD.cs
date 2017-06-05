
using System;
using System.Configuration;
using System.Threading;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using System.Collections.Generic;
using Newtonsoft.Json;
using Epi.DataPersistence.DataStructures;
using Epi.Cloud.Common.Configuration;
using Epi.Cloud.Common.Constants;
namespace Epi.Cloud.ServiceBus
{
    public class ServiceBusCRUD
    {

        public TopicClient topicClient;
        public string TopicName = ConfigurationManager.AppSettings["ServiceBusTopicName"];
        public string SubscriptionName = ConfigurationManager.AppSettings["ServiceBusSubscriptionName"];
        const Int16 maxTrials = 4;

        public ServiceBusCRUD()
        {
            if (!VerifyConfiguration())
            {
                Console.ReadLine();
                return;
            }
        }


        private static bool VerifyConfiguration()
        {
            bool configOK = true;
            var ResourceKey = ConfigurationHelper.GetEnvironmentKey("ServiceBusConnectionString", AppSettings.Key.Environment, false);
            var connectionString = ConfigurationHelper.GetConnectionStringByResourceKey(ResourceKey);

            return configOK;

        }

        #region Create Topic if topic is not exist
        private bool CreateTopic()
        {
            NamespaceManager namespaceManager = NamespaceManager.Create();
            try
            {
                if (namespaceManager.TopicExists(TopicName))
                {
                    return true;
                }
                else
                {
                    TopicDescription myTopic = namespaceManager.CreateTopic(TopicName);
                    SubscriptionDescription subscription = namespaceManager.CreateSubscription(myTopic.Path, SubscriptionName);
                    return true;
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
                throw;
            }
        }
        #endregion

        #region Send message to Topic
        public bool SendMessagesToTopic(FormResponseDetail hierarchicalResponse)
        {
            //var ResourceKey = ConfigurationHelper.GetEnvironmentKey("ServiceBusConnectionString", AppSettings.Key.Environment, false);
            //var connectionString = ConfigurationHelper.GetConnectionStringByResourceKey(ResourceKey);

            //Create Topic
            CreateTopic();

            var responseProperties = new Dictionary<string, object>();
            responseProperties.Add("ResponseId", hierarchicalResponse.RootResponseId);
            responseProperties.Add("FormId", hierarchicalResponse.RootFormId);
            responseProperties.Add("FormName", hierarchicalResponse.RootFormName);

            var hierarchicalResponseJson = JsonConvert.SerializeObject(hierarchicalResponse, Formatting.None, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });


            //Send message
            SendMessage(hierarchicalResponseJson, responseProperties);

            return false;

        }
        #endregion

        #region Read Message from Topic
        public MessagePayload ReceiveMessages()
        {
            // For PeekLock mode (default) where applications require "at least once" delivery of messages 
            SubscriptionClient agentSubscriptionClient = SubscriptionClient.Create(TopicName, SubscriptionName);
            MessagePayload messagePayload = null;
            try
            {
                //receive messages from Agent Subscription
                var message = agentSubscriptionClient.Receive(TimeSpan.FromSeconds(5));
                if (message != null)
                {
                    //Console.WriteLine(string.Format("Message received: Id = {0}, Body = {1}", message.MessageId, message.GetBody<string>()));
                    messagePayload = new MessagePayload
                    {
                        Properties = message.Properties,
                        Body = message.GetBody<string>()
                    };

                    message.Complete();
                }
                else
                {
                    return null;
                }
            }
            catch (MessagingException e)
            {
                if (!e.IsTransient)
                {
                    Console.WriteLine(e.Message);
                    throw;
                }
                else
                {
                    HandleTransientErrors(e);
                }
            }

            return messagePayload;

        }
        #endregion

        #region Created message and send message to queue
        public void SendMessage(string body, IDictionary<string, object> responseProperties = null)
        {
            topicClient = TopicClient.Create(TopicName);
            BrokeredMessage message = CreateMessage(body, responseProperties);
            try
            {
                topicClient.Send(message);
            }
            catch (MessagingException e)
            {
                if (!e.IsTransient)
                {
                    Console.WriteLine(e.Message);
                    throw;
                }
                else
                {
                    HandleTransientErrors(e);
                }
            }
            Console.WriteLine(string.Format("Message sent: Id = {0}, Body = {1}", message.MessageId, message.GetBody<string>()));
            topicClient.Close();
        }

        #endregion

        #region Create message
        public BrokeredMessage CreateMessage(string messageBody, IDictionary<string, object> responseProperties = null)
        {
            BrokeredMessage message = new BrokeredMessage(messageBody);
            if (responseProperties != null)
            {
                foreach (var kvp in responseProperties)
                {
                    message.Properties.Add(kvp);
                }
            }
            return message;
        }
        #endregion

        #region Handel TransientError
        private static void HandleTransientErrors(MessagingException e)
        {
            //If transient error/exception, let's back-off for 2 seconds and retry
            Console.WriteLine("Will retry sending the message in 2 seconds");
            Thread.Sleep(2000);
        }
        #endregion


    }
}
