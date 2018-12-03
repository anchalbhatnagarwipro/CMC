function CreateTestRecord(executionContext) {
    try {
        var sampleName = Xrm.Page.getAttribute("wipro_name").getvalue();
        //Module Name
        //wipro_module
    }
    catch (ex) {
        debugger;
        var formType = Xrm.Page.ui.getFormType();
        alert(ex.message);
             
            if (formType != 1) {
                var guId = Xrm.Page.data.entity.getId().replace("{", "").replace("}", "");
                var organizationUrl = Xrm.Page.context.getClientUrl();
                var entityName = Xrm.Page.data.entity.getEntityName();
                var objectTypeCode = "";               
                var recordURL = organizationUrl + "/main.aspx?etn="+entityName+"&id=%7b"+guId+"%7d&pagetype=entityrecord";
               
                var query = "wipro_CreateErrorLog";
                var dataDescription = ex.message;
                var errorType = ex.name;
                var errorDetails = ex.stack;
                var getFormMode = window.getFormMode;
               
                var data = {
                    "Log_url": recordURL,
                    "DescriptionInDetails": dataDescription,
                    "Error_Type": errorType,
                    "ErrorLogDetails": errorDetails,
                    "Source_Entity": entityName,
                    "Source_Reference": guId,
                    "Event_Details": getFormMode,
                    "SeverityValue": 1,
                    "Module": { "wipro_moduleid": "bF4548D51-55DA-E711-8179-E0071B65E251", "@data.type":"Microsoft.Dynamics.CRM.wipro_module"}                   
                };
                var req = new XMLHttpRequest();
                req.open("POST", organizationUrl + "/api/data/v8.2/" + query, true);
                req.setRequestHeader("Accept", "application/json");
                req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
                req.setRequestHeader("OData-MaxVersion", "4.0");
                req.setRequestHeader("OData-Version", "4.0");
                req.onreadystatechange = function () {
                    if (this.readyState == 4) {
                        req.onreadystatechange = null;
                        if (this.status == 200) {
                            alert("Action called successfully");
                        }
                        else {
                            var error = JSON.parse(this.response).error;
                            alert(error.message);
                        }
                    }
                };
                req.send(window.JSON.stringify(data));             
        }
        else {
            
            executionContext.getEventArgs().preventDefault();               
            }       
    }
}

