<Query Kind="Program">
  <Reference Relative="..\Assemblies\Microsoft.ServiceBus.dll">D:\Dropbox\Shared Programs\LinqPad\Assemblies\Microsoft.ServiceBus.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Runtime.Serialization.dll</Reference>
  <Namespace>Microsoft.ServiceBus</Namespace>
  <Namespace>Microsoft.ServiceBus.Messaging</Namespace>
</Query>

static ManualResetEvent _quitEvent = new ManualResetEvent(false);

private const string ConnectionString = "Endpoint=sb://";
private const string SubscriptionName = "Testing";
private const string TopicName = "TestTopic";

void Main()
{
    Console.CancelKeyPress += (sender, eArgs) => {
        _quitEvent.Set();
        eArgs.Cancel = true;
    };

    // kick off asynchronous stuff 

	var namespaceManager = NamespaceManager.CreateFromConnectionString(ConnectionString);
	
	if (!namespaceManager.TopicExists(TopicName))
	{
		var topicDescription = new TopicDescription(TopicName) { DefaultMessageTimeToLive = TimeSpan.FromMinutes(10) };
		namespaceManager.CreateTopic(topicDescription);
	}
	
	if (!namespaceManager.SubscriptionExists(TopicName, SubscriptionName))
	{
		//var filter = new SqlFilter("StoreNo = 12345");
		namespaceManager.CreateSubscription(TopicName, SubscriptionName);//, filter);
	}
			
	var subscriptionClient = SubscriptionClient.CreateFromConnectionString(ConnectionString, TopicName, SubscriptionName);	
		
	var options = new OnMessageOptions {AutoComplete = true, MaxConcurrentCalls = 1};
	options.ExceptionReceived += OnMessageException; 
	
	subscriptionClient.OnMessage(msg => {
		//msg.Dump();
		msg.GetBody<string>().Dump();
	}, options);

	// Indefinite Wait
	 
    _quitEvent.WaitOne();

    // cleanup/shutdown and quit
	
}

// Define other methods and classes here

private void OnMessageException(object sender, ExceptionReceivedEventArgs e)
{
	e.Dump();
}

public class TestMessage
{
	public string MessageId {get; set;}
	public string Message {get; set;}
}