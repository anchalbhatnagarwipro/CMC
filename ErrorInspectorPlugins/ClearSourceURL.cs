﻿using Microsoft.Xrm.Sdk;
using System;

namespace ErrorInspectorPlugins
{
    [CrmPluginRegistration(MessageNameEnum.Create, "wipro_executionlog", StageEnum.PreOperation, ExecutionModeEnum.Synchronous, "", "On wipro_executionlog create", 2, IsolationModeEnum.Sandbox)]
    public class ClearSourceURL : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            // Obtain the execution context from the service provider.
            Microsoft.Xrm.Sdk.IPluginExecutionContext context = (Microsoft.Xrm.Sdk.IPluginExecutionContext)
                serviceProvider.GetService(typeof(Microsoft.Xrm.Sdk.IPluginExecutionContext));

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
                    // An accountnumber attribute should not already exist because
                    // it is system generated.
                    if (entity.Attributes.Contains("wipro_recordurl") == true)
                    {
                        string wipro_event = (string)entity.Attributes["wipro_event"];
                        int value = ((OptionSetValue)entity["wipro_errorsource"]).Value;
                        if (value == 2 && wipro_event == "Create")
                        {
                            entity.Attributes["wipro_recordurl"] = string.Empty;
                        }
                    }
                    else
                    {

                        throw new InvalidPluginExecutionException("The error Tracker Having problem.");
                    }
                }
            }
        }
    }
}