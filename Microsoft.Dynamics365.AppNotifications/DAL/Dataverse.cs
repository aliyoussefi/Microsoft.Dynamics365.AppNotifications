using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Dynamics365.AppNotifications.DAL
{
    public class Dataverse
    {
        private ConnectionSettings _conn;
        public class ConnectionSettings
        {
            public string organizationUrl;
            public string clientId;
            public string clientSecret;
            public string aadInstance = "https://login.microsoftonline.com/";
            public string tenantID;
            public string serviceClientConnectionString;
        }
        public Dataverse(ConnectionSettings settings)
        {
            _conn = settings;
        }

        private string ConnectToDynamics(ConnectionSettings authenticationSettings)
        {
            ClientCredential clientcred = new ClientCredential(authenticationSettings.clientId, authenticationSettings.clientSecret);
            AuthenticationContext authenticationContext = new AuthenticationContext(authenticationSettings.aadInstance + authenticationSettings.tenantID);
            var authenticationResult = authenticationContext.AcquireTokenAsync(authenticationSettings.organizationUrl, clientcred).Result;
            return authenticationResult.AccessToken;

        }

        public ServiceClient GetNewCrmServiceClient(bool useServicePrincipal)
        {
            if (useServicePrincipal)
            {
                PowerPlatform.Dataverse.Client.ServiceClient client = new PowerPlatform.Dataverse.Client.ServiceClient(new Uri(_conn.organizationUrl), _conn.clientId, _conn.clientSecret, true);
                return client;
            }
            else
            {
                PowerPlatform.Dataverse.Client.ServiceClient client = new PowerPlatform.Dataverse.Client.ServiceClient(_conn.serviceClientConnectionString);
                return client;
            }
        }

        public EntityCollection QuerySystemUsers(ServiceClient client)
        {
            QueryExpression qx = new QueryExpression("systemuser");
            qx.ColumnSet.AllColumns = true;//.AddColumn("fullname");

            //qx.TopCount = top;
            //qx.NoLock = nolock;
            //qx.Distinct = distinct;
            qx.Criteria = new FilterExpression();
            qx.Criteria.AddCondition("accessmode", ConditionOperator.Equal, 0);
            EntityCollection ec = client.RetrieveMultiple(qx);
            return ec;
        }

        public Guid CreateRecord(ServiceClient client, Entity appNotification)
        {
            return client.Create(appNotification);
        }
    }
}
