
using System;
using System.Configuration;
using System.Threading;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;

namespace Epi.Cloud.ServiceBus
{
    public class ServiceBusCRUD
    {

        public TopicClient topicClient;
        public string TopicName = "ResponseInfoTopic";
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
            var connectionString = ConfigurationManager.AppSettings["Microsoft.ServiceBus.ConnectionString"];

            return configOK;

        }

        #region Create Topic if topic is not exist
        private bool CreateTopic()
        {
            NamespaceManager namespaceManager = NamespaceManager.Create();
            try
            {
                // Delete if exists
                if (namespaceManager.TopicExists(TopicName))
                {
                    return true;
                }
                else
                {
                    TopicDescription myTopic = namespaceManager.CreateTopic(TopicName);
                    SubscriptionDescription subscription = namespaceManager.CreateSubscription(myTopic.Path, "ReadFormInfoSubscription");
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
		public bool SendMessagesToTopic(string id, string responseProperties)
        {
            var connectionString = ConfigurationManager.AppSettings["Microsoft.ServiceBus.ConnectionString"];

            //Create Topic
            CreateTopic();

            //Send message
            SendMessages(id, responseProperties);

            return false;

        }
        #endregion

        #region Read Message from Topic
        public string ReceiveMessages()
        {
            // For PeekLock mode (default) where applications require "at least once" delivery of messages 
            SubscriptionClient agentSubscriptionClient = SubscriptionClient.Create(TopicName, "ReadFormInfoSubscription");
            BrokeredMessage message = null;
            string msg = string.Empty;

            try
            {
                //receive messages from Agent Subscription
                message = agentSubscriptionClient.Receive(TimeSpan.FromSeconds(5));
                if (message != null)
                {
                    //Console.WriteLine(string.Format("Message received: Id = {0}, Body = {1}", message.MessageId, message.GetBody<string>()));
                    msg = message.GetBody<string>();
                    
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

            return msg;

        }
        #endregion

        #region Created message and send message to queue
        public void SendMessages(string id, string body)
        {
            topicClient = TopicClient.Create(TopicName);
            BrokeredMessage message = new BrokeredMessage();
            message = CreateMessage(id, body);
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
        public BrokeredMessage CreateMessage(string messageId, string messageBody)
        {
            BrokeredMessage message = new BrokeredMessage(messageBody);
            message.MessageId = messageId;
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
