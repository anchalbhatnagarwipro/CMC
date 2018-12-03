﻿using Microsoft.Xrm.Sdk;
using System;

namespace ErrorInspectorPlugins
{
    [CrmPluginRegistration(MessageNameEnum.Create, "wipro_executionlog", StageEnum.PostOperation, ExecutionModeEnum.Synchronous, "", "On wipro_executionlog create", 1, IsolationModeEnum.Sandbox)]
    public class AddUniqueToken : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            // Obtain the execution context from the service provider.
            Microsoft.Xrm.Sdk.IPluginExecutionContext context = (Microsoft.Xrm.Sdk.IPluginExecutionContext)
            serviceProvider.GetService(typeof(Microsoft.Xrm.Sdk.IPluginExecutionContext));

            IOrganizationServiceFactory servicefactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));

            IOrganizationService client = servicefactory.CreateOrganizationService(context.UserId);
            // The InputParameters collection contains all the data passed in the message request.
            if (context.InputParameters.Contains("Target") &&
                context.InputParameters["Target"] is Entity)
            {
                // Obtain the target entity from the input parameters.
                Entity entity = (Entity)context.InputParameters["Target"];
                //</snippetAccountNumberPlugin2>

                // Verify that the target entity represents an account.
                // If not, this plug-in was not registered correctly.
                if (entity.LogicalName == "wipro_executionlog")
                {

                    string result = string.Empty;
                    //extract user id                                                                    
                    string token = (string)entity.Attributes["wipro_uniquetoken"];

                    if (string.IsNullOrEmpty(token))
                    {
                        //string userIdPrefix = context.UserId.ToString().Substring(0, 7);
                        //Random random = new Random(DateTime.Now.Millisecond);
                        //result = string.Format("{0}{1}", userIdPrefix, random.Next(1000, 99999).ToString());
                        //entity.Attributes.Add("wipro_uniquetoken", result);
                        Entity image = context.PostEntityImages["image"];
                        Entity LogTemp = new Entity(entity.LogicalName);
                        LogTemp.Id = entity.Id;
                        LogTemp.Attributes["wipro_uniquetoken"] = result.ToString();
                        client.Update(entity);
                    }

                }
            }
        }
    }
}
