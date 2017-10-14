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
                
                case ActivityTypes.Message:
                    CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                    CloudConfigurationManager.GetSetting("AzureWebJobsStorage"));
                    
                    CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
                    CloudTable table = tableClient.GetTableReference("dinners");
                    table.CreateIfNotExists();
                    
                    DinnerEntity dinner = new DinnerEntity("Soup", DateTime.Now.Ticks);
                    TableOperation insertOperation = TableOperation.Insert(dinner);
                    table.Execute(insertOperation);
                    
                    
                    dinner = new DinnerEntity("Salad", DateTime.Now.Ticks);
                     insertOperation = TableOperation.Insert(dinner);
                    table.Execute(insertOperation);
                    
                    
                     dinner = new DinnerEntity("Baked Potatoe", DateTime.Now.Ticks);
                     insertOperation = TableOperation.Insert(dinner);
                    table.Execute(insertOperation);
                    
                    
                    await Conversation.SendAsync(activity, () => new BasicProactiveEchoDialog());
                    break;
                    /*
                case ActivityTypes.ConversationUpdate:
                    log.Info("Conversation Update");
                    var client = new ConnectorClient(new Uri(activity.ServiceUrl));
                    IConversationUpdateActivity update = activity;
                    if (update.MembersAdded.Any())
                    {
                        var reply = activity.CreateReply();
                        var newMembers = update.MembersAdded?.Where(t => t.Id != activity.Recipient.Id);
                        foreach (var newMember in newMembers)
                        {
                            reply.Text = "Welcome";
                            if (!string.IsNullOrEmpty(newMember.Name))
                            {
                                reply.Text += $" {newMember.Name}";
                            }
                            reply.Text += "!";
                            await client.Conversations.ReplyToActivityAsync(reply);
                        }
                    }
                    break;
                    */
                case ActivityTypes.Event:
                    // handle proactive Message from function
                    log.Info("Trigger start");
                    IEventActivity triggerEvent = activity;
                    var message = JsonConvert.DeserializeObject<Message>(((JObject) triggerEvent.Value).GetValue("Message").ToString());
                    var messageactivity = (Activity)message.RelatesTo.GetPostToBotMessage();
                    
                    var client = new ConnectorClient(new Uri(messageactivity.ServiceUrl));
                    var triggerReply = messageactivity.CreateReply();
                    triggerReply.Text = $"Here's the deal";
                    await client.Conversations.ReplyToActivityAsync(triggerReply);
                    log.Info("Trigger end");
                    break;
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
}
