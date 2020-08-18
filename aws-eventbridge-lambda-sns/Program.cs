using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Amazon;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Amazon.EventBridge;
using Amazon.EventBridge.Model;
using Amazon.Lambda;
using Amazon.Lambda.Model;

namespace SnsPublish
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Create Eventbridge & Lambda Client
            var EBclient = new AmazonEventBridgeClient(awsAccessKeyId: "Your-Access-Key", awsSecretAccessKey: "Your-Secret-Access-Key", region: Amazon.RegionEndpoint.APSoutheast1);
            var lambdaClient = new AmazonLambdaClient(awsAccessKeyId: "Your-Access-Key", awsSecretAccessKey: "Your-Secret-Access-Key", region: Amazon.RegionEndpoint.APSoutheast1);

            // Create a Scheduled Rule on EventBridge
            var putRuleRequest = new PutRuleRequest();
            putRuleRequest.Name = "Rule2";
            putRuleRequest.ScheduleExpression = "cron(*/1 * * * ? *)"; // Cron Expression (UTC Time): Minute Hour (DayofMonth) Month (DayofWeek) Year
            putRuleRequest.EventBusName = "default";
            putRuleRequest.State = RuleState.ENABLED;

            var putRuleResponse = await EBclient.PutRuleAsync(putRuleRequest);
            Console.WriteLine(putRuleResponse.HttpStatusCode);
            Console.WriteLine(putRuleResponse.RuleArn);

            // Add Permission of the ScheduledRule to invoke function in Lambda
            var addPermissionRequest = new Amazon.Lambda.Model.AddPermissionRequest();
            addPermissionRequest.Action = "lambda:InvokeFunction";
            addPermissionRequest.Principal = "events.amazonaws.com";
            addPermissionRequest.StatementId = "lambda-rule-2";
            addPermissionRequest.SourceArn = putRuleResponse.RuleArn;
            addPermissionRequest.FunctionName = "TestEBSNS";

            var addPermissionResponse = await lambdaClient.AddPermissionAsync(addPermissionRequest);
            Console.WriteLine(addPermissionResponse.HttpStatusCode);

            // Set Target of the Schedule Rule to Lambda Function
            var putTargetRequest = new PutTargetsRequest();
            putTargetRequest.Rule = "Rule2";
            putTargetRequest.EventBusName = "default";

            var eventTarget = new Target();
            eventTarget.Arn = "arn:aws:lambda:your-region:your-account:function:TestEBSNS";
            eventTarget.Input = "{ \"Key1\":\"Rule1\", \"Key2\":\"Rule2\" }";
            eventTarget.Id = "1";

            List<Target> eventTargets = new List<Target>();
            eventTargets.Add(eventTarget);
            putTargetRequest.Targets = eventTargets;

            var putTargetResponse = await EBclient.PutTargetsAsync(putTargetRequest);
            Console.WriteLine(putTargetResponse.HttpStatusCode);
        }
    }
}