using System;
using System.Collections.Generic;
using System.Threading;
using Epi.Cloud.Common.Configuration;
using Epi.Cloud.Common.Constants;
using Epi.DataPersistence.Constants;
using Epi.DataPersistence.DataStructures;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;

namespace Epi.Cloud.ServiceBus
{
    public class ServiceBusCRUD
    {
        public TopicClient topicClient;
        public string SBconnectionString = ConnectionStrings.GetConnectionString(ConnectionStrings.Key.ServiceBusConnectionString);
        public string TopicName = AppSettings.GetStringValue(AppSettings.Key.ServiceBusTopicName);
        public string SubscriptionName = AppSettings.GetStringValue(AppSettings.Key.ServiceBusSubscriptionName);

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
            var connectionString = ConnectionStrings.GetConnectionString(ConnectionStrings.Key.ServiceBusConnectionString);

            return configOK;

        }

        #region Create Topic if topic is not exist
        private bool CreateTopic()
        {
            NamespaceManager namespaceManager = NamespaceManager.CreateFromConnectionString(SBconnectionString);
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
            //Create Topic
            CreateTopic();

            var responseProperties = new Dictionary<string, object>();
            responseProperties.Add(MessagePropertyKeys.ResponseId, hierarchicalResponse.RootResponseId);
            responseProperties.Add(MessagePropertyKeys.FormId, hierarchicalResponse.RootFormId);
            responseProperties.Add(MessagePropertyKeys.FormName, hierarchicalResponse.RootFormName);
            responseProperties.Add(MessagePropertyKeys.UserOrgId, hierarchicalResponse.UserOrgId);
            responseProperties.Add(MessagePropertyKeys.IsDeleted, hierarchicalResponse.RecStatus == RecordStatus.Deleted
                                                               || hierarchicalResponse.RecStatus == RecordStatus.PhysicalDelete);

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
            SubscriptionClient agentSubscriptionClient = SubscriptionClient.CreateFromConnectionString(SBconnectionString, TopicName, SubscriptionName);
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
            var SBconnectionString = ConnectionStrings.GetConnectionString(ConnectionStrings.Key.ServiceBusConnectionString);
            topicClient = TopicClient.CreateFromConnectionString(SBconnectionString, TopicName);
            //topicClient = TopicClient.Create(TopicName);
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
