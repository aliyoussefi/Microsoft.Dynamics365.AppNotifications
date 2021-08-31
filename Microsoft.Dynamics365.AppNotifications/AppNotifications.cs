using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Query;
using Newtonsoft.Json;

namespace Microsoft.Dynamics365.AppNotifications
{
    public class AppNotifications
    {
        public readonly string tenantId;
        public readonly string orgUrl;
        public readonly string clientId;
        public readonly string clientSecret;
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

        }
        [Function("CreateAppNotification")]
        public HttpResponseData CreateAppNotification([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req,
            FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("Function1");
            logger.LogInformation("C# HTTP trigger function processed a request.");

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

            response.WriteString("Welcome to Azure Functions!");

            foreach (var header in req.Headers)
            {
                //log.LogInformation(header.Key, header.Value);
            }

            //var content = req..Body;
            string jsonContent = req.ReadAsStringAsync().Result;
            dynamic data = JsonConvert.DeserializeObject(jsonContent);
            string title = data?.title;
            DateTimeOffset timestamp = DateTime.Now;
            string body = data?.body;
            string icontype = data?.icontype;
            string toasttype = data?.toasttype;

            string token = ConnectToDynamics(new S2SAuthenticationSettings()
            {
                clientId = clientId,
                clientSecret = clientSecret,
                tenantID = tenantId,
                organizationUrl = orgUrl
            });
            string conn = System.Environment.GetEnvironmentVariable(
                                    "CONNECTION_STRING", EnvironmentVariableTarget.Process);

            ServiceClient client = GetNewCrmServiceClient(conn);


            List<Entity> rtnObject = new List<Entity>();
            QueryExpression qx = new QueryExpression("systemuser");
            qx.ColumnSet.AllColumns = true;//.AddColumn("fullname");

            //qx.TopCount = top;
            //qx.NoLock = nolock;
            //qx.Distinct = distinct;
            qx.Criteria = new FilterExpression();
            qx.Criteria.AddCondition("accessmode" , ConditionOperator.Equal , 0);
            EntityCollection ec = client.RetrieveMultiple(qx);
            foreach (Entity user in ec.Entities)
            {
                Entity appNotification = new Entity("appnotification");
                appNotification.Attributes = new AttributeCollection();
                appNotification.Attributes.Add("title", title);
                appNotification.Attributes.Add("body", body + " " + user.Attributes["fullname"]);
                appNotification.Attributes.Add("ownerid", new EntityReference("systemuser", user.Id));
                appNotification.Attributes.Add("icontype", icontype);
                appNotification.Attributes.Add("toasttype", toasttype);
                client.Create(appNotification);
            }



            //RequestModel objRequestModel = new RequestModel();
            //objRequestModel.correlationid = data?.correlationId ?? exCtx.InvocationId.ToString();
            //HttpResponseMessage rtnObject = req.CreateResponse(HttpStatusCode.OK, objRequestModel);
            //rtnObject.Headers.Add("InvocationId", exCtx.InvocationId.ToString());
            //return rtnObject;

            return response;
        }

        private ServiceClient GetNewCrmServiceClient(string conn)
        {
            OrganizationServiceContext orgContext;
            PowerPlatform.Dataverse.Client.ServiceClient client = new PowerPlatform.Dataverse.Client.ServiceClient(conn);
            //CrmServiceClient client = new CrmServiceClient(conn);
            //if (client == null || client.OrganizationServiceProxy == null)
            //{
            //    client = new CrmServiceClient(conn);
            //}

            orgContext = new OrganizationServiceContext(client);


            return client;
        }

        private string ConnectToDynamics(S2SAuthenticationSettings authenticationSettings)
        {
            ClientCredential clientcred = new ClientCredential(authenticationSettings.clientId, authenticationSettings.clientSecret);
            AuthenticationContext authenticationContext = new AuthenticationContext(authenticationSettings.aadInstance + authenticationSettings.tenantID);
            var authenticationResult = authenticationContext.AcquireTokenAsync(authenticationSettings.organizationUrl, clientcred).Result;
            return authenticationResult.AccessToken;

        }
        public class S2SAuthenticationSettings
        {
            public string organizationUrl;
            public string clientId;
            public string clientSecret;
            public string aadInstance = "https://login.microsoftonline.com/";
            public string tenantID;
        }
    }
}
