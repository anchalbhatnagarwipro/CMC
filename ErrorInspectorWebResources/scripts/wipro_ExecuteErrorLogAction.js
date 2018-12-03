function CreateTestRecord(executionContext) {
    try {
        var sampleName = Xrm.Page.getAttribute("name").getvalue();
        //Module Name
        //wipro_module
    }
    catch (ex) {
        debugger;
        var formType = Xrm.Page.ui.getFormType();
        alert(ex.message);
             
        if (formType != 1) {
                var moduleReferenceId=retireveModule();
                var guId = Xrm.Page.data.entity.getId().replace("{", "").replace("}", "");
                var organizationUrl = Xrm.Page.context.getClientUrl();
                var entityName = Xrm.Page.data.entity.getEntityName();
                var objectTypeCode = "";               
                var recordURL = organizationUrl + "/main.aspx?etn="+entityName+"&id=%7b"+guId+"%7d&pagetype=entityrecord";
               
                var query = "new_CreateErrorLog";
                var dataDescription = ex.message;
                var errorType = ex.name;
                var errorDetails = ex.stack;
                var getFormMode = window.getFormMode;
               
                var data = {
                    "Log_url": recordURL,
                    "Name": dataDescription,
                    "Error_Type": errorType,
                    "ErrorLogDetails": errorDetails,
                    "Source_Entity": entityName,
                    "Source_Reference": guId,                  
                    "SeverityValue": 1,
                    "Module": { "new_moduleid": "18CD1D02-3ADB-E711-A94F-000D3AF28A0D", "@data.type":"Microsoft.Dynamics.CRM.new_moduleSet"}                   
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
function retireveModule() {
    debugger;
    var moduleId = "";
 
    var req = new XMLHttpRequest();
    var organizationUrl = Xrm.Page.context.getClientUrl();
    var columnset = "?$select=new_moduleid";
    var Query = "new_moduleid"+columnset; 
    organizationUrl + "/api/data/v8.0/" + Query,
   // req.open("GET", Xrm.Page.context.getClientUrl() + "/api/data/v8.2/new_moduleSet?$select=new_moduleid&$filter=new_name eq 'ERP' ", true);
        req.open("GET", organizationUrl + "/api/data/v8.2/" + Query, true);  
    req.setRequestHeader("OData-MaxVersion", "4.0");
    req.setRequestHeader("OData-Version", "4.0");
    req.setRequestHeader("Accept", "application/json");
    req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
    req.onreadystatechange = function () {
        if (this.readyState === 4) {
            req.onreadystatechange = null;
            if (this.status === 200) {
                var results = JSON.parse(this.response);
                for (var i = 0; i < results.value.length; i++) {
                    moduleId = results.value[i]["new_moduleid"];
                }
            }
            else {
                alert(this.statusText);
            }
        }
    };
    req.send();
    return moduleId;
    
}

