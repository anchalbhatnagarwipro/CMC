﻿using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Workflow;
using System;
using System.Activities;

namespace ErrorInspectorPlugins
{
    [CrmPluginRegistration("CalculateRecordCount", "CalculateRecordCount", "Calculates record count", "", IsolationModeEnum.Sandbox)]
    public class CalculateRecordCount : CodeActivity
    {
        [Output("RecordCount")]
        public OutArgument<String> RecordCount { get; set; }

        [Input("EntityName")]
        public InArgument<String> EntityName { get; set; }

        [Input("ColumnName")]
        public InArgument<String> ColumnName { get; set; }

        protected override void Execute(CodeActivityContext context)
        {

            IWorkflowContext workflowContext = context.GetExtension<IWorkflowContext>();
            IOrganizationServiceFactory factory = context.GetExtension<IOrganizationServiceFactory>();
            IOrganizationService service = factory.CreateOrganizationService(workflowContext.InitiatingUserId);

            try
            {

                string PluginExceptionMessage = string.Empty;
                QueryExpression qe = new QueryExpression();
                qe.EntityName = this.EntityName.Get<string>(context);
                qe.ColumnSet = new ColumnSet();
                qe.ColumnSet.Columns.Add(this.ColumnName.Get<string>(context));
                EntityCollection retrieved = service.RetrieveMultiple(qe);
                if (retrieved.Entities.Count > 0)
                {
                    this.RecordCount.Set(context, retrieved.Entities.Count);
                }

            }
            catch (Exception ex)
            {
                throw new Exception("workflow error");
            }
        }
    }
}
