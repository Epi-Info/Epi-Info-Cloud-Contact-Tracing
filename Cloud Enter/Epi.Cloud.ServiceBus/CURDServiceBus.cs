
using System;
using Microsoft.ServiceBus.Messaging;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceBus;

public class CURDServiceBus
{

    private static QueueClient queueClient;
    private static string QueueName = "FormInfoQueue";
    const Int16 maxTrials = 4;

    public CURDServiceBus()
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

    private static void CreateQueue()
    {
        NamespaceManager namespaceManager = NamespaceManager.Create();
        // Delete if exists
        if (namespaceManager.QueueExists(QueueName))
        {
            namespaceManager.DeleteQueue(QueueName);
        }

        namespaceManager.CreateQueue(QueueName);
    }

    public bool SendMessagesToQueue(string id, string formProperties)
    {
        var connectionString = ConfigurationManager.AppSettings["Microsoft.ServiceBus.ConnectionString"];
        queueClient = QueueClient.CreateFromConnectionString(connectionString, QueueName);
        BrokeredMessage message = new BrokeredMessage();
        message = SendFormInfoToQueue(id, formProperties);
        try
        {
            queueClient.Send(message);
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
                //HandleTransientErrors(e);
            }
        }


        //try
        //{
        //    //receive messages from Queue
        //    message = queueClient.Receive(TimeSpan.FromSeconds(5));
        //    if (message != null)
        //    {
        //        Console.WriteLine(string.Format("Message received: Id = {0}, Body = {1}", message.MessageId, message.GetBody<string>()));
        //        // Further custom message processing could go here...
        //        message.Complete();
        //    }
        //}
        //catch (Exception ex)
        //{

        //}
        return false;

    }

    public bool ReceiveMessagesFromQueue()
    {
        Console.WriteLine("\nReceiving message from Queue...");
        BrokeredMessage message = null;
        while (true)
        {
            try
            {
                //receive messages from Queue
                message = queueClient.Receive(TimeSpan.FromSeconds(5));
                if (message != null)
                {
                    Console.WriteLine(string.Format("Message received: Id = {0}, Body = {1}", message.MessageId, message.GetBody<string>()));
                    // Further custom message processing could go here...
                    message.Complete();
                }
                else
                {
                    //no more messages in the queue
                    break;
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
        }
        queueClient.Close();
        return true;
    }

    public BrokeredMessage SendFormInfoToQueue(string messageId, string messageBody)
    {
        BrokeredMessage message = new BrokeredMessage(messageBody);
        message.MessageId = messageId;
        return message;
    }

    private static void HandleTransientErrors(MessagingException e)
    {
        //If transient error/exception, let's back-off for 2 seconds and retry
        Console.WriteLine(e.Message);
        Console.WriteLine("Will retry sending the message in 2 seconds");
        Thread.Sleep(2000);
    }


    //    public static async Task<QueueClient> GetQueueClientAsync()
    //    {
    //        QueueClient client;
    //        string connectionString =CloudConfigurationManager.GetSetting("Microsoft.ServiceBus.ConnectionString");
    //        NamespaceManager namespaceManager = NamespaceManager.Create();
    //        //Create the queue if it does not exists
    //        if (!(await namespaceManager.QueueExistsAsync(QueueName)))
    //        {
    //            await namespaceManager.CreateQueueAsync(QueueName);
    //        }
    //        // Initialize the connection to Service Bus Queue
    //        client = QueueClient.CreateFromConnectionString
    //            (connectionString, QueueName);
    //        return client;
    //    }
    //}

}