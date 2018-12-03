﻿using Microsoft.Uii.Common.Entities;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Tooling.PackageDeployment.CrmPackageExtentionBase;
using System;
using System.ComponentModel.Composition;
using System.Configuration;

namespace CMCPackageDeployment
{
    /// <summary>
    /// Import package starter frame.
    /// </summary>
    [Export(typeof(IImportExtensions))]
    public class PackageTemplate : ImportExtension
    {
        /// <summary>
        /// Called When the package is initialized.
        /// </summary>
        public override void InitializeCustomExtension()
        {
            // Do nothing.
        }

        /// <summary>
        /// Called Before Import Completes.
        /// </summary>
        /// <returns></returns>
        public override bool BeforeImportStage()
        {
            return true; // do nothing here.
        }

        /// <summary>
        /// Called for each UII record imported into the system
        /// This is UII Specific and is not generally used by Package Developers
        /// </summary>
        /// <param name="app">App Record</param>
        /// <returns></returns>
        public override ApplicationRecord BeforeApplicationRecordImport(ApplicationRecord app)
        {
            return app;  // do nothing here.
        }

        /// <summary>
        /// Called during a solution upgrade while both solutions are present in the target CRM instance.
        /// This function can be used to provide a means to do data transformation or upgrade while a solution update is occurring.
        /// </summary>
        /// <param name="solutionName">Name of the solution</param>
        /// <param name="oldVersion">version number of the old solution</param>
        /// <param name="newVersion">Version number of the new solution</param>
        /// <param name="oldSolutionId">Solution ID of the old solution</param>
        /// <param name="newSolutionId">Solution ID of the new solution</param>
        public override void RunSolutionUpgradeMigrationStep(string solutionName, string oldVersion, string newVersion, Guid oldSolutionId, Guid newSolutionId)
        {

            base.RunSolutionUpgradeMigrationStep(solutionName, oldVersion, newVersion, oldSolutionId, newSolutionId);
        }


        /// <summary>
        /// Called after Import completes.
        /// </summary>
        /// <returns></returns>
        public override bool AfterPrimaryImport()
        {
            var version = CrmSvc.ConnectedOrgVersion;

            string assemblyNames = ConfigurationManager.AppSettings["assemblyNames"];

            if (string.IsNullOrEmpty(assemblyNames))
            {
                return true;
            }

            string[] assemblyNameSet = assemblyNames.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);

            //Enable all the steps in the plugin assembly
            if (CrmSvc != null && CrmSvc.IsReady)
            {
                foreach (var assemblyname in assemblyNameSet)
                {
                    // Create the QueryExpression object to retrieve plug-in type
                    var query = new QueryExpression();
                    query.EntityName = "plugintype";
                    query.Criteria.AddCondition("assemblyname", ConditionOperator.Equal, assemblyname);
                    var retrievedPluginType = CrmSvc.RetrieveMultiple(query)[0];

                    var pluginTypeId = (Guid)retrievedPluginType.Attributes["plugintypeid"];

                    query = new QueryExpression();

                    // Set the properties of the QueryExpression object.
                    query.EntityName = "sdkmessageprocessingstep";
                    query.ColumnSet = new ColumnSet(new[] { "sdkmessageprocessingstepid", "statecode" });
                    query.Criteria.AddCondition(new ConditionExpression("plugintypeid", ConditionOperator.Equal, pluginTypeId));
                    var retrievedSteps = CrmSvc.RetrieveMultiple(query);

                    foreach (var step in retrievedSteps.Entities)
                    {
                        // Enable the step by setting it's state code
                        step.Attributes["statecode"] = new OptionSetValue(0); // 0 = Enabled
                        step.Attributes["statuscode"] = new OptionSetValue(1); // 1 = Enabled
                        CrmSvc.Update(step);
                    }
                }

                return true; // Do nothing here/
            }

            return false;
        }

        #region Properties

        /// <summary>
        /// Name of the Import Package to Use
        /// </summary>
        /// <param name="plural">if true, return plural version</param>
        /// <returns></returns>
        public override string GetNameOfImport(bool plural)
        {
            return "Wipro CMC";
        }

        /// <summary>
        /// Folder Name for the Package data.
        /// </summary>
        public override string GetImportPackageDataFolderName
        {
            get
            {
                // WARNING this value directly correlates to the folder name in the Solution Explorer where the ImportConfig.xml and sub content is located.
                // Changing this name requires that you also change the correlating name in the Solution Explorer
                return "PkgFolder";
            }
        }

        /// <summary>
        /// Description of the package, used in the package selection UI
        /// </summary>
        public override string GetImportPackageDescriptionText
        {
            get { return "Package Description"; }
        }

        /// <summary>
        /// Long name of the Import Package.
        /// </summary>
        public override string GetLongNameOfImport
        {
            get { return "Wipro CMC"; }
        }


        #endregion

    }
}
