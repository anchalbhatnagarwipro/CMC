using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ServiceModel;
using Microsoft.Xrm.Sdk;
using Microsoft.Crm.Sdk;
using Microsoft.Crm.Sdk.Messages;

using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Discovery;

//using Microsoft.Xrm.Client.Services;
using Microsoft.Xrm.Sdk.Query;

namespace ChineseWallsPlugins
{
    [CrmPluginRegistration(MessageNameEnum.Create, "email", StageEnum.PostOperation, ExecutionModeEnum.Synchronous, "", "On email create", 1, IsolationModeEnum.Sandbox)]
    [CrmPluginRegistration(MessageNameEnum.Create, "post", StageEnum.PostOperation, ExecutionModeEnum.Synchronous, "", "On post create", 1, IsolationModeEnum.Sandbox)]
    [CrmPluginRegistration(MessageNameEnum.Create, "task", StageEnum.PostOperation, ExecutionModeEnum.Synchronous, "", "On task create", 1, IsolationModeEnum.Sandbox)]
    [CrmPluginRegistration(MessageNameEnum.Create, "phonecall", StageEnum.PostOperation, ExecutionModeEnum.Synchronous, "", "On phonecall create", 1, IsolationModeEnum.Sandbox)]
    [CrmPluginRegistration(MessageNameEnum.Create, "wipro_dealsetup", StageEnum.PostOperation, ExecutionModeEnum.Synchronous, "", "On wipro_dealsetup create", 1, IsolationModeEnum.Sandbox)]
    [CrmPluginRegistration(MessageNameEnum.Create, "appointment", StageEnum.PostOperation, ExecutionModeEnum.Synchronous, "", "On appointment create", 1, IsolationModeEnum.Sandbox)]

    public class ConfigureCrmRecordAccess : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {

            Microsoft.Xrm.Sdk.IPluginExecutionContext context = (Microsoft.Xrm.Sdk.IPluginExecutionContext)serviceProvider.GetService(typeof(Microsoft.Xrm.Sdk.IPluginExecutionContext));
            ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
            {
                Entity entity = null;
                Entity postImage = null;
                Guid contextEntityGUID;
                Guid[] members = null;
                Guid memberGUID;
                String contextEntityID;
                try
                {
                    entity = (Entity)context.InputParameters["Target"];
                    contextEntityGUID = entity.Id;
                    contextEntityID = entity.Id.ToString();
                }
                catch (Exception ex)
                {
                    throw new InvalidPluginExecutionException("Target: " + ex.Message);
                }

                if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                {
                    IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                    IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);
                    if (entity.LogicalName == "wipro_dealsetup")
                    {
                        try
                        {
                            Entity entityDealSetup = service.Retrieve("wipro_dealsetup", contextEntityGUID, new ColumnSet("wipro_name", "wipro_accessteamname", "wipro_businessunit", "wipro_accessprivileges", "wipro_dealcreatestatus", "wipro_validfrom", "wipro_validtill", "wipro_prospect", "wipro_deal", "wipro_accessteamallocated"));
                            if (entityDealSetup.Contains("wipro_dealcreatestatus"))
                            {
                                if (((string)entityDealSetup["wipro_dealcreatestatus"]).ToString() == "1")
                                {
                                    //Create a deal with owner as Complaints team.
                                    Entity deal = new Entity("wipro_deal");
                                    deal["wipro_name"] = ((string)entityDealSetup["wipro_name"]).ToString();
                                    deal["ownerid"] = new EntityReference("team", new Guid("DA52AA1E-114D-E811-A95C-000D3A32890B")); //Complaints Team - here need to get the id dynamically.Need to change the code 
                                    deal["wipro_validfrom"] = ((DateTime)entityDealSetup["wipro_validfrom"]);
                                    deal["wipro_validtill"] = ((DateTime)entityDealSetup["wipro_validtill"]);
                                    EntityReference prospectLookup = ((Microsoft.Xrm.Sdk.EntityReference)entityDealSetup["wipro_prospect"]);
                                    String prospectGUID = prospectLookup.Id.ToString();
                                    deal["wipro_prospect"] = new EntityReference("wipro_prospect", new Guid(prospectGUID));
                                    Guid dealId = service.Create(deal);


                                    //Create a deal team - Access team
                                    Entity dealTeam = new Entity("team");
                                    dealTeam["name"] = ((string)entityDealSetup["wipro_accessteamname"]).ToString();
                                    OptionSetValue OpTeamType = new OptionSetValue(1);
                                    dealTeam["teamtype"] = OpTeamType;
                                    EntityReference businessUnitLookup = ((Microsoft.Xrm.Sdk.EntityReference)entityDealSetup["wipro_businessunit"]);
                                    String businessUnitGUID = businessUnitLookup.Id.ToString();
                                    dealTeam["businessunitid"] = new EntityReference("businessunit", new Guid(businessUnitGUID));
                                    dealTeam["administratorid"] = new EntityReference("systemuser", new Guid("DC75656E-F84D-E811-A97C-000D3A370778"));
                                    Guid teamId = service.Create(dealTeam);

                                    //Grant the team different privileges(read / write) to access deal.
                                    var entityReference = new EntityReference("wipro_deal", dealId);
                                    var teamReference = new EntityReference("team", teamId);
                                    String accessPrivileges = entityDealSetup.FormattedValues["wipro_accessprivileges"];
                                    string[] accessPrivilegesArray = accessPrivileges.Split(';');
                                    AddAccessRequest(entityReference, teamReference, accessPrivilegesArray, service);

                                    // Add uesrs to team
                                    string fetchXmluser = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                                                        <entity name='wipro_useraccessconfiguration'>  
                                                        <attribute name='wipro_user' />   
                                                        <attribute name='wipro_enablemanageraccess' />  
                                                        <filter type='and'>
                                                        <condition attribute='wipro_deal' operator='eq' value='" + contextEntityID + @"' />
                                                        </filter>
                                                         </entity>
                                                         </fetch>";
                                    var fetchUser = new FetchExpression(fetchXmluser);
                                    EntityCollection Users = service.RetrieveMultiple(fetchUser);
                                    var i = 0;
                                    if (Users.Entities.Count > 0)
                                    {
                                        foreach (Entity e in Users.Entities)
                                        {
                                            EntityReference userLookup = ((Microsoft.Xrm.Sdk.EntityReference)e["wipro_user"]);
                                            String userLookupGUID = userLookup.Id.ToString();
                                            memberGUID = new Guid(userLookupGUID);
                                            Boolean managerFlag = ((Boolean)e["wipro_enablemanageraccess"]);
                                            if (i == 0)
                                            {
                                                members = new[] { memberGUID };
                                            }
                                            else
                                            {
                                                members = members.Concat(new Guid[] { memberGUID }).ToArray();
                                            }
                                            if (managerFlag == true)
                                            {
                                                while (memberGUID != null && memberGUID != Guid.Empty)
                                                {
                                                    Entity entityManager = service.Retrieve("systemuser", memberGUID, new ColumnSet("parentsystemuserid"));

                                                    if (entityManager.Contains("parentsystemuserid"))
                                                    {
                                                        EntityReference managerRegardingLookup = ((Microsoft.Xrm.Sdk.EntityReference)entityManager["parentsystemuserid"]);
                                                        String managerLookupID = managerRegardingLookup.Id.ToString();
                                                        memberGUID = new Guid(managerLookupID);
                                                        if (memberGUID != null && memberGUID != Guid.Empty)
                                                        {
                                                            members = members.Concat(new Guid[] { memberGUID }).ToArray();
                                                        }
                                                    }
                                                    else
                                                    {
                                                        memberGUID = Guid.Empty;
                                                    }
                                                }
                                            }
                                            i = i + 1;
                                        }
                                        AddMembersToTeam(teamId, members, service);
                                    }
                                    Entity entityDealUpdate = service.Retrieve("wipro_dealsetup", contextEntityGUID, new ColumnSet("wipro_deal", "wipro_accessteamallocated"));
                                    entityDealUpdate.Attributes["wipro_deal"] = new EntityReference("wipro_deal", dealId);
                                    entityDealUpdate.Attributes["wipro_accessteamallocated"] = new EntityReference("team", teamId);
                                    service.Update(entityDealUpdate);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            //Log Exception
                            //throw new InvalidPluginExecutionException("An error occurred in the Update Deal Setup plug-in", ex);
                            throw new InvalidPluginExecutionException(ex.ToString(), ex);
                        }
                    }
                    else if (entity.LogicalName == "appointment" || entity.LogicalName == "email" || entity.LogicalName == "phonecall" || entity.LogicalName == "task")
                    {
                        try
                        {
                            //get the post image of the entity
                            //postImage = (Entity)context.PostEntityImages["PostCreateImage"];
                            string[] accessPrivilegesArray = null;
                            String activityRegardingLookupID;
                            String activityRegardingLogicalName = "";
                            String activityRegardingName = "";
                            Guid activityRegardingEntityGUID = Guid.Empty;
                            String contextEntityName = "";

                            if (entity.LogicalName == "appointment")
                            {
                                contextEntityName = "appointment";
                            }
                            else if (entity.LogicalName == "email")
                            {
                                contextEntityName = "email";
                            }
                            else if (entity.LogicalName == "phonecall")
                            {
                                contextEntityName = "phonecall";
                            }
                            else if (entity.LogicalName == "task")
                            {
                                contextEntityName = "task";
                            }

                            if (entity.Attributes.Contains("regardingobjectid"))
                            {
                                EntityReference activityRegardingLookup = ((Microsoft.Xrm.Sdk.EntityReference)entity["regardingobjectid"]);
                                activityRegardingLookupID = activityRegardingLookup.Id.ToString();
                                activityRegardingEntityGUID = new Guid(activityRegardingLookupID);
                                activityRegardingLogicalName = activityRegardingLookup.LogicalName;
                                activityRegardingName = activityRegardingLookup.Name;
                            }

                            if (activityRegardingLogicalName == "wipro_deal")
                            {
                                Entity entityActivityUpdate = service.Retrieve(contextEntityName, contextEntityGUID, new ColumnSet("ownerid"));
                                // entityActivityUpdate.Attributes["ownerid"] = new EntityReference("team", new Guid("DA52AA1E-114D-E811-A95C-000D3A32890B"));                                
                                var entityReference = new EntityReference(contextEntityName, contextEntityGUID);
                                EntityReference ownerLookup = ((Microsoft.Xrm.Sdk.EntityReference)entityActivityUpdate["ownerid"]);
                                Guid ownerGUID = ownerLookup.Id;
                                var systemUser2Ref = new EntityReference("systemuser", ownerGUID);
                                var revokeOwnerAccessReq = new RevokeAccessRequest
                                {
                                    Revokee = systemUser2Ref,
                                    Target = entityReference
                                };

                                service.Execute(revokeOwnerAccessReq);
                                //service.Update(entityActivityUpdate);
                                string fetchXmlDealSetup = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                                                    <entity name='wipro_dealsetup'>  
                                                    <attribute name='wipro_name' />   
                                                    <attribute name='wipro_accessteamname' />   
                                                    <attribute name='wipro_accessprivileges' /> 
                                                    <filter type='and'>
                                                    <condition attribute='wipro_name' operator='eq' value='" + activityRegardingName + @"' />
                                                    </filter>
                                                        </entity>
                                                        </fetch>";
                                var fetchDealSetup = new FetchExpression(fetchXmlDealSetup);
                                EntityCollection dealSetup = service.RetrieveMultiple(fetchDealSetup);
                                String teamName = "";
                                String teamPrivilege = "";
                                if (dealSetup.Entities.Count > 0)
                                {
                                    foreach (Entity e in dealSetup.Entities)
                                    {
                                        teamName = ((String)e.Attributes["wipro_accessteamname"]).ToString();
                                        teamPrivilege = e.FormattedValues["wipro_accessprivileges"];
                                    }
                                }
                                if (teamName != "")
                                {
                                    string fetchXmlTeam = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                                                    <entity name='team'>  
                                                    <attribute name='teamid' alias='teamguid' />
                                                    <filter type='and'>
                                                    <condition attribute='name' operator='eq' value='" + teamName + @"' />
                                                    </filter>
                                                        </entity>
                                                        </fetch>";
                                    var fetchTeam = new FetchExpression(fetchXmlTeam);
                                    EntityCollection team = service.RetrieveMultiple(fetchTeam);
                                    String teamId = "";
                                    Guid teamGuid = Guid.Empty;
                                    if (team.Entities.Count > 0)
                                    {
                                        foreach (Entity e in team.Entities)
                                        {
                                            teamId = ((AliasedValue)e.Attributes["teamguid"]).Value.ToString();
                                            teamGuid = new Guid(teamId);
                                        }
                                        //entityReference = new EntityReference(contextEntityName, contextEntityGUID);
                                        var teamReference = new EntityReference("team", teamGuid);
                                        accessPrivilegesArray = teamPrivilege.Split(';');
                                        AddAccessRequest(entityReference, teamReference, accessPrivilegesArray, service);
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            //Log Exception
                            //throw new InvalidPluginExecutionException("An error occurred in the Update Deal Setup plug-in", ex);
                            throw new InvalidPluginExecutionException(ex.ToString(), ex);
                        }
                    }
                }
            }
        }
        public void AddMembersToTeam(Guid teamId, Guid[] membersId, IOrganizationService service)
        {
            // Create the AddMembersTeamRequest object.
            AddMembersTeamRequest addRequest = new AddMembersTeamRequest();
            // Set the AddMembersTeamRequest TeamID property to the object ID of an existing team.
            addRequest.TeamId = teamId;
            // Set the AddMembersTeamRequest MemberIds property to an array of GUIDs that contains the object IDs of one or more system users.
            addRequest.MemberIds = membersId;
            // Execute the request.
            service.Execute(addRequest);
        }
        public void AddAccessRequest(EntityReference entityReference, EntityReference teamReference, string[] accessPrivilegesArray, IOrganizationService service)
        {
            AccessRights Access_Rights = new AccessRights();
            Access_Rights = AccessRights.None;
            foreach (var privilege in accessPrivilegesArray)
            {

                if (privilege.Trim() == "Read")
                {
                    if (Access_Rights == AccessRights.None)
                        Access_Rights = AccessRights.ReadAccess;
                    else
                        Access_Rights = Access_Rights | AccessRights.ReadAccess;
                }
                else if (privilege.Trim() == "Write")
                {
                    if (Access_Rights == AccessRights.None)
                        Access_Rights = AccessRights.WriteAccess;
                    else
                        Access_Rights = Access_Rights | AccessRights.WriteAccess;
                }
                else if (privilege.Trim() == "Share")
                {
                    if (Access_Rights == AccessRights.None)
                        Access_Rights = AccessRights.ShareAccess;
                    else
                        Access_Rights = Access_Rights | AccessRights.ShareAccess;
                }
                else if (privilege.Trim() == "Assign")
                {
                    if (Access_Rights == AccessRights.None)
                        Access_Rights = AccessRights.AssignAccess;
                    else
                        Access_Rights = Access_Rights | AccessRights.AssignAccess;
                }
                else if (privilege.Trim() == "Delete")
                {
                    if (Access_Rights == AccessRights.None)
                        Access_Rights = AccessRights.DeleteAccess;
                    else
                        Access_Rights = Access_Rights | AccessRights.DeleteAccess;
                }
                else if (privilege.Trim() == "Append")
                {
                    if (Access_Rights == AccessRights.None)
                        Access_Rights = AccessRights.AppendAccess;
                    else
                        Access_Rights = Access_Rights | AccessRights.AppendAccess;
                }
                else if (privilege.Trim() == "Append To")
                {
                    if (Access_Rights == AccessRights.None)
                        Access_Rights = AccessRights.AppendToAccess;
                    else
                        Access_Rights = Access_Rights | AccessRights.AppendToAccess;
                }
            }
            var grantAccessRequest = new GrantAccessRequest
            {
                PrincipalAccess = new PrincipalAccess
                {
                    AccessMask = Access_Rights,
                    Principal = teamReference
                },
                Target = entityReference
            };
            service.Execute(grantAccessRequest);
        }
    }
}