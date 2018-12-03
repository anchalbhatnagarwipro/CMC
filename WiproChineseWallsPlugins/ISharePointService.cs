using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChineseWallsPlugins
{
    public interface ISharePointService
    {
        void CreateFolder(string siteUrl, string relativePath);
        void CreateFolder(string siteUrl, string relativePath, List<KeyValuePair<string, string>> userPermissionSet, ITracingService tracingService);
        void GrantFolderPermissions(string siteUrl, string relativePath, List<KeyValuePair<string, string>> userPermissionSet, ITracingService tracingService);
    }
}
