using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace ChineseWallsPlugins
{
    [CrmPluginRegistration(MessageNameEnum.Create, "wipro_deal", StageEnum.PostOperation, ExecutionModeEnum.Synchronous, "", "On wipro_deal create", 1, IsolationModeEnum.Sandbox)]
    public class ConfigureSharepointFolderAccess : IPlugin
    {
        private ITracingService tracingService;
        private IPluginExecutionContext context;
        private IOrganizationServiceFactory serviceFactory;
        private IOrganizationService service;
        private IOrganizationService privService;
        private ChineseWallsContext cwContext;


        public void Execute(IServiceProvider serviceProvider)
        {
            // Initialization of CRM Services and Context Objects 

            tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            service = serviceFactory.CreateOrganizationService(context.UserId);
            privService = serviceFactory.CreateOrganizationService(null);
            cwContext = new ChineseWallsContext(service);

            tracingService.Trace("Plugin Started...");

            // Find which entity is calling

            if ((context.MessageName == "Create") && context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
            {
                Entity targetEntity = (Entity)context.InputParameters["Target"];

                if (targetEntity.LogicalName != wipro_deal.EntityLogicalName)
                {
                    return;
                }

                try
                {
                    wipro_deal dealRec = targetEntity.ToEntity<wipro_deal>();
                    List<KeyValuePair<string, string>> userPermissionSet = GetUserPermissionSet(dealRec);

                    if(userPermissionSet == null)
                    {
                        return;
                    }
                    
                    string config = GetSecureConfigValue(privService, "PrivSharePointUser"); ;
                    string[] user = config.Split(';');
                    
                    // Create a new sharepoint service using the given priv sharepoint user credentials
                    SPService spService = new SPService(user[0], user[1]);
                    var docLocation = new DocumentLocationHelper(privService, spService);

                    // Get the site passed into the workflow activity

                    SharePointDocumentLocation parentDocLoc = cwContext.SharePointDocumentLocationSet.FirstOrDefault(rec => rec.RelativeUrl == wipro_deal.EntityLogicalName);
                    var site = cwContext.SharePointSiteSet.Where(a => a.Id == parentDocLoc.ParentSiteOrLocation.Id).FirstOrDefault();
                    
                    if (site != null)
                    {
                        var documentLocation = docLocation.CreateDocumentLocation(site, "Deals", dealRec, userPermissionSet, tracingService);
                    }
                    else
                        throw new InvalidPluginExecutionException("The specified Sharepoint site record could not be found.");
                }
                catch (Exception ex)
                {
                    tracingService.Trace(ex.Message + Environment.NewLine + ex.StackTrace);
                    throw new InvalidPluginExecutionException(ex.Message + Environment.NewLine + ex.StackTrace);
                }
                finally
                {
                    cwContext.Dispose();
                }
            }
        }

        private List<KeyValuePair<string, string>> GetUserPermissionSet(wipro_deal dealRec)
        {
            List<KeyValuePair<string, string>> userPermissionSet = new List<KeyValuePair<string, string>>();
            wipro_dealsetup dealSetupRec = cwContext.wipro_dealsetupSet.Where(a => a.wipro_name == dealRec.wipro_name).FirstOrDefault();
            var spSetupRecColl = cwContext.wipro_sharepointaccesssetupSet.Where(a => a.wipro_deal.Id == dealSetupRec.Id).ToList();

            if(spSetupRecColl == null || spSetupRecColl.Count < 1)
            {
                return null;
            }

            foreach(wipro_sharepointaccesssetup spSetupRec in spSetupRecColl)
            {
                List<string> principalIdSet = new List<string>();
                OptionSetValueCollection permissionSet = spSetupRec.wipro_accessprivileges as OptionSetValueCollection;

                SystemUser userRec = cwContext.SystemUserSet.Where(a => a.Id == spSetupRec.wipro_user.Id).FirstOrDefault();
                principalIdSet.Add(userRec.wipro_sharepointprincipalid);

                if(spSetupRec.wipro_enablemanageraccess == true)
                {
                    while (userRec.ParentSystemUserId != null)
                    {
                        userRec = cwContext.SystemUserSet.Where(a => a.Id == userRec.ParentSystemUserId.Id).FirstOrDefault();
                        principalIdSet.Add(userRec.wipro_sharepointprincipalid);
                    }
                }
               
                foreach(string principalId in principalIdSet)
                {
                    foreach(OptionSetValue opValue in permissionSet)
                    {
                        KeyValuePair<string, string> userPermissionItem = new KeyValuePair<string, string>(principalId, opValue.Value.ToString());
                        userPermissionSet.Add(userPermissionItem);
                    }
                }
            }

            return userPermissionSet;
        }

        /// <summary>
        /// Get a config value - using your chosen technique!
        /// </summary>
        /// <param name="privService"></param>
        /// <param name="configName"></param>
        /// <returns></returns>
        private static string GetSecureConfigValue(IOrganizationService privService, string configName)
        {

            // TODO: Add you chosen method of storing secure config!
            switch (configName)
            {
                case "PrivSharePointUser":
                    // Username and password separated by ;
                    return "admin@wipromscrmpractice.onmicrosoft.com;wipro@123";

                default:
                    return null;
            }
        }
    }
}
