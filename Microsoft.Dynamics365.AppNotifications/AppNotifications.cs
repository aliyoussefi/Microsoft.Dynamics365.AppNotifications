using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using System;
using System.Net;

namespace Microsoft.Dynamics365.AppNotifications
{
    public class AppNotifications
    {
        public readonly string tenantId;
        public readonly string orgUrl;
        public readonly string clientId;
        public readonly string clientSecret;
        public readonly string conn;
        public AppNotifications()
        {
            this.tenantId = System.Environment.GetEnvironmentVariable(
                                    "TENANT_ID", EnvironmentVariableTarget.Process);
            this.orgUrl = System.Environment.GetEnvironmentVariable(
                                    "ORG_URL", EnvironmentVariableTarget.Process);
            this.clientId = System.Environment.GetEnvironmentVariable(
                                    "CLIENT_ID", EnvironmentVariableTarget.Process);
            this.clientSecret = System.Environment.GetEnvironmentVariable(
                                    "CLIENT_SECRET", EnvironmentVariableTarget.Process);
            this.conn = System.Environment.GetEnvironmentVariable(
                        "CONNECTION_STRING", EnvironmentVariableTarget.Process);

        }
        [Function("BroadcastAppNotification")]
        public HttpResponseData BroadcastAppNotification([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req,
            FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("BroadcastAppNotification");

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

            string jsonContent = req.ReadAsStringAsync().Result;
            dynamic data = JsonConvert.DeserializeObject(jsonContent);
            string title = data?.title;
            string body = data?.body;
            string icontype = data?.icontype;
            string toasttype = data?.toasttype;

            DAL.Dataverse dataverse = new DAL.Dataverse(new DAL.Dataverse.ConnectionSettings()
            {
                clientId = clientId,
                clientSecret = clientSecret,
                organizationUrl = orgUrl,
                tenantID = tenantId,
                serviceClientConnectionString = conn
            });

            ServiceClient client = dataverse.GetNewCrmServiceClient(false);

            EntityCollection ec = dataverse.QuerySystemUsers(client);

            foreach (Entity user in ec.Entities)
            {
                Entity appNotification = new Entity("appnotification");
                appNotification.Attributes = new AttributeCollection();
                appNotification.Attributes.Add("title", title);
                appNotification.Attributes.Add("body", body + " " + user.Attributes["fullname"]);
                appNotification.Attributes.Add("ownerid", new EntityReference("systemuser", user.Id));
                appNotification.Attributes.Add("icontype", icontype);
                appNotification.Attributes.Add("toasttype", toasttype);
                try
                {
                    dataverse.CreateRecord(client, appNotification);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, ex.Message);
                }

            }
            return response;
        }


    }
}
