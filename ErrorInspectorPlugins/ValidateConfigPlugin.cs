using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;

namespace ErrorInspectorPlugins
{
    [CrmPluginRegistration(MessageNameEnum.Create, "wipro_executionlog", StageEnum.PreValidation, ExecutionModeEnum.Synchronous, "", "On wipro_executionlog create", 1, IsolationModeEnum.Sandbox)]
    public class ValidateConfigPlugin : IPlugin
    {
        /// <summary>
        /// Prevalidation
        /// </summary>
        /// <param name="serviceProvider"></param>
        public void Execute(IServiceProvider serviceProvider)
        {
            Microsoft.Xrm.Sdk.IPluginExecutionContext context = (Microsoft.Xrm.Sdk.IPluginExecutionContext)
                serviceProvider.GetService(typeof(Microsoft.Xrm.Sdk.IPluginExecutionContext));

            IOrganizationServiceFactory servicefactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));

            IOrganizationService crmService = servicefactory.CreateOrganizationService(context.UserId);
            string PluginExceptionMessage = string.Empty;
            QueryExpression qe = new QueryExpression();
            qe.EntityName = "wipro_logconfiguration";
            qe.ColumnSet = new ColumnSet();
            qe.ColumnSet.Columns.Add("wipro_pluginmessagesource");

            EntityCollection retrieved = crmService.RetrieveMultiple(qe);
            if (retrieved.Entities.Count == 0)
            {
                throw new NotImplementedException("Please configuration into wipro_logconfiguration");
            }
        }
    }
}
