using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ChineseWallsPlugins
{
    public class SPService : ISharePointService
    {
        private string _username;
        private string _password;
        private string _siteUrl;
        private SpoAuthUtility _spo;
        public SPService(string username, string password)
        {
            _username = username;
            _password = password;

        }

        public void GrantFolderPermissions(string siteUrl, string relativePath, List<KeyValuePair<string, string>> userPermissionSet, ITracingService tracingService)
        {
            if (siteUrl != _siteUrl)
            {
                tracingService.Trace("1");
                _siteUrl = siteUrl;
                Uri spSite = new Uri(siteUrl);

                _spo = SpoAuthUtility.Create(spSite, _username, WebUtility.HtmlEncode(_password), false);
                tracingService.Trace("2");
            }

            string digest = _spo.GetRequestDigest();

            foreach (KeyValuePair<string, string> userRec in userPermissionSet)
            {
                tracingService.Trace(userRec.Key + "|" + userRec.Value);
                string odataQuery = String.Format("_api/web/getFolderByServerRelativeUrl('" + relativePath.TrimStart('/') + "')/ListItemAllFields/roleassignments/addroleassignment(principalid={0}, roleDefId={1})", userRec.Key, userRec.Value);
                Uri url = new Uri(String.Format("{0}/{1}", _spo.SiteUrl, odataQuery));
                tracingService.Trace("Url : " + url);
                //Uri url = new Uri("https://wipromscrmpractice.sharepoint.com/sites/Wipro365/_api/web/getFolderByServerRelativeUrl('wipro_deal/Anchal')/ListItemAllFields/roleassignments/addroleassignment(principalid=16,roleDefId=1073741830)");

                // Set X-RequestDigest
                var webRequest = (HttpWebRequest)HttpWebRequest.Create(url);
                webRequest.Headers.Add("X-RequestDigest", digest);
                tracingService.Trace("Request Start");
                // Send a json odata request to SPO rest services to fetch all list items for the list.
                byte[] result = HttpHelper.SendODataJsonRequest(
                  url,
                  "POST", // reading data from SP through the rest api usually uses the GET verb 
                  null,
                  webRequest,
                  _spo // pass in the helper object that allows us to make authenticated calls to SPO rest services
                  );
                tracingService.Trace("Request End");
                string response = Encoding.UTF8.GetString(result, 0, result.Length);
                tracingService.Trace("Response : " + response);
            }            
        }

        public void BreakRoleInheritance(string siteUrl, string relativePath)
        {
            if (siteUrl != _siteUrl)
            {
                _siteUrl = siteUrl;
                Uri spSite = new Uri(siteUrl);

                _spo = SpoAuthUtility.Create(spSite, _username, WebUtility.HtmlEncode(_password), false);
            }

            relativePath = relativePath.TrimStart('/');

            string odataQuery = "_api/web/getFolderByServerRelativeUrl('" + relativePath.TrimStart('/') + "')/ListItemAllFields/breakroleinheritance(copyRoleAssignments=false, clearSubscopes=true)";
            
            string digest = _spo.GetRequestDigest();

            Uri url = new Uri(String.Format("{0}/{1}", _spo.SiteUrl, odataQuery));

            //Uri url = new Uri("https://wipromscrmpractice.sharepoint.com/sites/Wipro365/_api/web/getFolderByServerRelativeUrl('wipro_deal/Test')/ListItemAllFields/breakroleinheritance(copyRoleAssignments=false,clearSubscopes=true)");


            // Set X-RequestDigest
            var webRequest = (HttpWebRequest)HttpWebRequest.Create(url);
            webRequest.Headers.Add("X-RequestDigest", digest);

            // Send a json odata request to SPO rest services to fetch all list items for the list.
            byte[] result = HttpHelper.SendODataJsonRequest(
              url,
              "POST", // reading data from SP through the rest api usually uses the GET verb 
              null,
              webRequest,
              _spo // pass in the helper object that allows us to make authenticated calls to SPO rest services
              );

            string response = Encoding.UTF8.GetString(result, 0, result.Length);
        }

        public void CreateFolder(string siteUrl, string relativePath, List<KeyValuePair<string, string>> userPermissionSet, ITracingService tracingService)
        {

            if (siteUrl != _siteUrl)
            {
                _siteUrl = siteUrl;
                Uri spSite = new Uri(siteUrl);

                _spo = SpoAuthUtility.Create(spSite, _username, WebUtility.HtmlEncode(_password), false);
            }

            string odataQuery = "_api/web/folders";

            byte[] content = ASCIIEncoding.ASCII.GetBytes(@"{ '__metadata': { 'type': 'SP.Folder' }, 'ServerRelativeUrl': '" + relativePath + "'}");


            string digest = _spo.GetRequestDigest();

            Uri url = new Uri(String.Format("{0}/{1}", _spo.SiteUrl, odataQuery));
            // Set X-RequestDigest
            var webRequest = (HttpWebRequest)HttpWebRequest.Create(url);
            webRequest.Headers.Add("X-RequestDigest", digest);

            // Send a json odata request to SPO rest services to fetch all list items for the list.
            byte[] result = HttpHelper.SendODataJsonRequest(
              url,
              "POST", // reading data from SP through the rest api usually uses the GET verb 
              content,
              webRequest,
              _spo // pass in the helper object that allows us to make authenticated calls to SPO rest services
              );

            string response = Encoding.UTF8.GetString(result, 0, result.Length);

            BreakRoleInheritance(siteUrl, relativePath);
            GrantFolderPermissions(siteUrl, relativePath, userPermissionSet, tracingService);
        }

        public void CreateFolder(string siteUrl, string relativePath)
        {

            if (siteUrl != _siteUrl)
            {
                _siteUrl = siteUrl;
                Uri spSite = new Uri(siteUrl);

                _spo = SpoAuthUtility.Create(spSite, _username, WebUtility.HtmlEncode(_password), false);
            }

            string odataQuery = "_api/web/folders";

            byte[] content = ASCIIEncoding.ASCII.GetBytes(@"{ '__metadata': { 'type': 'SP.Folder' }, 'ServerRelativeUrl': '" + relativePath + "'}");


            string digest = _spo.GetRequestDigest();

            Uri url = new Uri(String.Format("{0}/{1}", _spo.SiteUrl, odataQuery));
            // Set X-RequestDigest
            var webRequest = (HttpWebRequest)HttpWebRequest.Create(url);
            webRequest.Headers.Add("X-RequestDigest", digest);

            // Send a json odata request to SPO rest services to fetch all list items for the list.
            byte[] result = HttpHelper.SendODataJsonRequest(
              url,
              "POST", // reading data from SP through the rest api usually uses the GET verb 
              content,
              webRequest,
              _spo // pass in the helper object that allows us to make authenticated calls to SPO rest services
              );

            string response = Encoding.UTF8.GetString(result, 0, result.Length);
        }
    }
}
