#r "Newtonsoft.Json"
#load "BasicProactiveEchoDialog.csx"

using System;
using System.Net;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Microsoft.Bot.Builder.Azure;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

using Microsoft.Azure; // Namespace for CloudConfigurationManager
using Microsoft.WindowsAzure.Storage; // Namespace for CloudStorageAccount
using Microsoft.WindowsAzure.Storage.Table; // Namespace for Table storage types

// Bot Storage: Register the optional private state storage for your bot. 

// For Azure Table Storage, set the following environment variables in your bot app:
// -UseTableStorageForConversationState set to 'true'
// -AzureWebJobsStorage set to your table connection string

// For CosmosDb, set the following environment variables in your bot app:
// -UseCosmosDbForConversationState set to 'true'
// -CosmosDbEndpoint set to your cosmos db endpoint
// -CosmosDbKey set to your cosmos db key

public class BotMessage
{
    public string Source { get; set; }
    public string Message { get; set; }
}

public class DinnerEntity : TableEntity
{
    public DinnerEntity(string meal, long lastEatten)
    {
        this.PartitionKey = "dinner";
        this.RowKey = meal;
        this.LastEatten = lastEatten;
    }

    public DinnerEntity() { }
    
    public long LastEatten {get; set;}
}

public static async Task<object> Run(HttpRequestMessage req, TraceWriter log)
{
    log.Info($"Webhook was triggered!");
/*
    // Initialize the azure bot
    using (BotService.Initialize())
    {
        // Deserialize the incoming activity
        string jsonContent = await req.Content.ReadAsStringAsync();
        log.Info(jsonContent);
        var activity = JsonConvert.DeserializeObject<Activity>(jsonContent);
        
        // authenticate incoming request and add activity.ServiceUrl to MicrosoftAppCredentials.TrustedHostNames
        // if request is authenticated
        if (!await BotService.Authenticator.TryAuthenticateAsync(req, new [] {activity}, CancellationToken.None))
        {
            return BotAuthenticator.GenerateUnauthorizedResponse(req);
        }
    
        if (activity != null)
        {
            // one of these will have an interface and process it
            switch (activity.GetActivityType())
            {
                // Message: to the bot?
                case ActivityTypes.Message:
                    log.Info(activity.Text);
                    log.Info(activity.From.Name);

                    CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                    CloudConfigurationManager.GetSetting("AzureWebJobsStorage"));
                    
                    CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
                    CloudTable table = tableClient.GetTableReference("dinners");
                    table.CreateIfNotExists();
                    
                    // Add an item
                    //DinnerEntity dinner = new DinnerEntity("Soup", DateTime.Now.Ticks);
                    //TableOperation insertOperation = TableOperation.Insert(dinner);
                    //table.Execute(insertOperation);

                    TableQuery<DinnerEntity> query = new TableQuery<DinnerEntity>().Where(
                            TableQuery.GenerateFilterCondition("PartitionKey",
                            QueryComparisons.Equal, "dinner"));
                    
                    var meal = table.ExecuteQuery(query).OrderByDescending(x => x.LastEatten).Last();

                    var reply = activity.CreateReply();
                    reply.Text = meal.RowKey;

                    var clientone = new ConnectorClient(new Uri(activity.ServiceUrl));
                    await clientone.Conversations.ReplyToActivityAsync(reply);



                    // Update item
                    TableOperation retrieveOperation = TableOperation.Retrieve<DinnerEntity>("dinner", meal.RowKey);
                    TableResult retrievedResult = table.Execute(retrieveOperation);
                    DinnerEntity updateEntity = (DinnerEntity)retrievedResult.Result;

                    if (updateEntity != null)
                    {
                        meal.LastEatten = DateTime.Now.Ticks;
                        TableOperation updateOperation = TableOperation.Replace(meal);
                        //TableOperation deleteOperation = TableOperation.Delete(deleteEntity);

                        table.Execute(updateOperation);
                    }

                    //await Conversation.SendAsync(activity, () => new BasicProactiveEchoDialog());
                    break;
                case ActivityTypes.Event:
                    // handle proactive Message from function
                    log.Info("Trigger start");

                    IEventActivity triggerEvent = activity;
                    //log.Info(triggerEvent.Name);
                    //log.Info(activity.From.Name);

                    var message = ((JObject) triggerEvent.Value).GetValue("Message").ToString();
                    log.Info(message);


                    //var message = JsonConvert.DeserializeObject<Message>(((JObject) triggerEvent.Value).GetValue("Message").ToString());
                    //var messageactivity = (Activity)message.RelatesTo.GetPostToBotMessage();
                    
                    //var client = new ConnectorClient(new Uri(messageactivity.ServiceUrl));
                    //var triggerReply = messageactivity.CreateReply();
                    //triggerReply.Text = $"Here's the deal";
                    //await client.Conversations.ReplyToActivityAsync(triggerReply);

                    log.Info("Trigger end");
                    break;
                case ActivityTypes.ConversationUpdate:
                case ActivityTypes.ContactRelationUpdate:
                case ActivityTypes.Typing:
                case ActivityTypes.DeleteUserData:
                case ActivityTypes.Ping:
                default:
                    log.Error($"Unknown activity type ignored: {activity.GetActivityType()}"); 
                    break;
            }
        }
        return req.CreateResponse(HttpStatusCode.Accepted);
    }    
*/
        return req.CreateResponse(HttpStatusCode.Accepted);
}
