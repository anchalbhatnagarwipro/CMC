////Created by Abhijeet Kore

var wipro_Sdk = new Object({
    logException: function (ex, priorityCode) {
        var req = new XMLHttpRequest();
        debugger;
        var entityName = "";
        if (Xrm.Page.data != null) {
            entityName = Xrm.Page.data.entity.getEntityName();
        }
        else {
            entityName = "WebResource";
        }
        var friendlyMessageInput = "";
        var url = "/api/data/v8.1/wipro_moduleusersettings?$select=_wipro_user_value&$filter=contains(wipro_name, '" + entityName + "') ";
        req.open("GET", Xrm.Page.context.getClientUrl() + url, true);
        req.setRequestHeader("OData-MaxVersion", "4.0");
        req.setRequestHeader("OData-Version", "4.0");
        req.setRequestHeader("Accept", "application/json");
        req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
        req.onreadystatechange = function () {
            if (this.readyState === 4) {
                //req.onreadystatechange = null;
                if (this.status === 200) {
                    var result = JSON.parse(this.response);
                    for (var i = 0; i < result.value.length; i++) {
                        var userID = result.value[i]["_wipro_user_value"];
                        var organizationUrl = Xrm.Page.context.getClientUrl();
                        var objectTypeCode = "";
                        var recordURL = "";
                        var guId = "";
                        var formType = "";
                        var formName = "";
                        var event = "";
                        //   var EXCEPTION_USER_MSG = "An unexpected error has occured, please notify your system administrator. Error message: {0}";
                        //      var safeStackString = exception.stack.toString().replace(/</g, "&lt;").replace(/>/g, "&gt;").replace(/&/g, "&amp;");
                        if (Xrm.Page.data != null) {

                            guId = Xrm.Page.data.entity.getId().replace("{", "").replace("}", "");
                            formName = Xrm.Page.ui.formSelector.getCurrentItem().getLabel();
                            formType = Xrm.Page.ui.getFormType();
                            recordURL = organizationUrl + "/main.aspx?etn=" + entityName + "&id=%7b" + guId + "%7d&pagetype=entityrecord";
                            if (formType == 1)
                                event = "Create";
                            else if (formType == 2)
                                event = "Update";
                            else if (formType == 5)
                                event = "Quick Create";
                        }
                        else {
                            recordURL = window.location.href;
                            entityName = "WebResource";
                        }
                        friendlyMessageInput = wipro_Sdk.retriveFriendlyMessage();
                        var query = "wipro_LogException";
                        var dataDescription = ex.message;
                        var errorType = ex.name;
                        var errorDetails = ex.stack;
                        var getFormMode = window.getFormMode;
                        var data = {
                            "Name": ex.name,
                            "RecordURL": recordURL,
                            "errorLogDetails": dataDescription,
                            //"errorType": errorType,
                            "errordescription": errorDetails,
                            "UserFriendlyMessage": friendlyMessageInput,
                            "entityname": entityName,
                            "eventName": getFormMode,
                            "priority": priorityCode,
                            "errorSource": 1,
                            "eventName": event,
                            "UserReference": { "systemuserid": userID, "@data.type": "Microsoft.Dynamics.CRM.systemuser" }

                        };

                        var req = new XMLHttpRequest();
                        req.open("POST", organizationUrl + "/api/data/v8.2/" + query, false);
                        req.setRequestHeader("Accept", "application/json");
                        req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
                        req.setRequestHeader("OData-MaxVersion", "4.0");
                        req.setRequestHeader("OData-Version", "4.0");
                        req.onreadystatechange = function () {
                            if (this.readyState == 4) {
                                //req.onreadystatechange = null;
                                if (this.status == 200) {
                                    var friendlyMsg = JSON.parse(this.response);
                                    alert(friendlyMsg.friendlyMessage);
                                }
                                else {
                                    var error = JSON.parse(this.response).error;
                                    alert(error.message);
                                }
                            }
                        };
                        req.send(window.JSON.stringify(data));
                    }
                }
                else {
                    alert("Error retrieving Accounts – " + error.message);
                }
            }
        };
        req.send(null);
    },

    retriveFriendlyMessage: function () {
        debugger;
        var friendlyMessageInput = "";
        var reqConfig = new XMLHttpRequest();
        var url = "/api/data/v8.2/wipro_logconfigurations?$select=wipro_javascriptmessagesource";
        reqConfig.open("GET", Xrm.Page.context.getClientUrl() + url, false);
        reqConfig.setRequestHeader("OData-MaxVersion", "4.0");
        reqConfig.setRequestHeader("OData-Version", "4.0");
        reqConfig.setRequestHeader("Accept", "application/json");
        reqConfig.setRequestHeader("Content-Type", "application/json; charset=utf-8");
        reqConfig.onreadystatechange = function () {
            if (this.readyState === 4) {
                //req.onreadystatechange = null;
                if (this.status === 200) {
                    var resultset = JSON.parse(this.response);
                    friendlyMessageInput = resultset.value[0]["wipro_javascriptmessagesource"];
                }
            }
        }
        reqConfig.send();
        return friendlyMessageInput;
    }
});

wipro_Sdk.PriorityLevel = {
    LOW: 1,
    MEDIUM: 2,
    HIGH: 3,
    CRITICAL: 4
}




