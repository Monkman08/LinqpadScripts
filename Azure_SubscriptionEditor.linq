<Query Kind="Program">
  <Reference Relative="..\..\Assemblies\Microsoft.ServiceBus.dll">C:\Assemblies\Microsoft.ServiceBus.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Runtime.Serialization.dll</Reference>
  <Namespace>Microsoft.ServiceBus</Namespace>
  <Namespace>Microsoft.ServiceBus.Messaging</Namespace>
  <Namespace>System.Xml.Serialization</Namespace>
</Query>

private const string ConnectionString = "Endpoint=sb://";


private const string SubscriptionName = "Production";
private const string TopicName = "ProdutionTopicName";

#region Setup Factory / Clients

private static TopicClient _topicClient;
private static MessagingFactory _messageFactory;
private static NamespaceManager _nsManager;

private static MessagingFactory MessageFactory {
	get { return _messageFactory 
		?? (_messageFactory = MessagingFactory.CreateFromConnectionString(ConnectionString)); } 
}

private static TopicClient DriverMetricsClient {
	get { return _topicClient
		?? (_topicClient = MessageFactory.CreateTopicClient(TopicName)); }
}

private static NamespaceManager NsManager {
	get { return _nsManager
		?? (_nsManager = NamespaceManager.CreateFromConnectionString(ConnectionString)); }
}

#endregion

void Main()
{
	#region Track Start
	var startTime = DateTime.Now;
	string.Format("Process Started at {0}", startTime).Dump();
	#endregion
	
	//----------------------------------------------------------------------
	// SETUP YOUR NEW RULES HERE
	//----------------------------------------------------------------------
	var newRule = new RuleDescription()
	{
		//Action = new SqlRuleAction("set SpeedOverAlert=’Red’"), /* you can dynamically set flags on FILTER true */
		Filter = new SqlFilter("StoreNo = '12345'"),
		Name = "StoreFilterRule1"
	};
	
	//----------------------------------------------------------------------
	
	#region Question
	string view = string.Empty;
	do {
		Console.WriteLine("Do you want to view all existing rules or add a new rule [v/a]? ");
		view = Console.ReadLine().ToLower();
	} while (view != "v" && view != "a");	
	view.Dump();
	#endregion
		
	var rules = NsManager.GetRules(TopicName, SubscriptionName);
	rules.Dump("Before Configuration");
	
	var subClient = MessageFactory.CreateSubscriptionClient(
		TopicName, 
		SubscriptionName);	
	
	#region Question
	string delete = string.Empty;
	if (view != "v")
	{
		do {
			Console.WriteLine("Do you want to delete all existing rules [y/n]? ");
			delete = Console.ReadLine().ToLower();
		} while (delete != "y" && delete != "n");
		delete.Dump();
	}
	#endregion
	
	// use the subscription client to remove all current rules, we don't have to do this
	if (delete == "y")
		foreach (var rule in rules)
			subClient.RemoveRule(rule.Name);

	// create the new rule
	if (view == "a")
	{
		subClient.AddRule(newRule);

		rules = NsManager.GetRules(TopicName, SubscriptionName);
		rules.Dump("After Configuration");
	}

	#region  Track End
	var endTime = DateTime.Now;
	string.Format("Process Ended at {0}", endTime).Dump();
	string.Format("Process Completed in {0}ms", Math.Round((endTime - startTime).TotalMilliseconds)).Dump();
	#endregion
}

// Define other methods and classes here