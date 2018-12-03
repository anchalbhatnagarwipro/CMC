var wipro_dealcreatestatus;

function OnExecuteButtonClick() {

    if (Xrm.Page.ui.getFormType() == 2) {

        var allfieldspopulated = CheckMandatoryFieldsPopulated();

        if (allfieldspopulated !== true) {
            alert("Please ensure all of the mandatory fields on the form are populated.");
            return;
        }

        var dealsetupId = Xrm.Page.data.entity.getId().slice(1, -1);
        var dealName = Xrm.Page.getAttribute("wipro_name").getValue();
        GetDealCreationStatus(dealsetupId);

        if (wipro_dealcreatestatus !== "1") {
            var accessTeamMemberCount = Xrm.Page.getControl("UserAccess").getGrid().getTotalRecordCount();

            if (accessTeamMemberCount < 1) {
                alert("It looks like you haven't added members to your Access Team. Please add User Access Configuration records before executing the deal setup.");
                return;
            }

            var createspdoc = Xrm.Page.getAttribute("wipro_createsharepointfolder").getValue();
            var spMemberCount = Xrm.Page.getControl("sharepointaccess").getGrid().getTotalRecordCount();

            if (createspdoc == true && spMemberCount < 1) {
                alert("As you have opted for creating document folder on Sharepoint, please add members to the Sharepoint Access Configuration section. If you do not wish to add any member, please set the 'Sharepoint Folder Creation Required?' field to 'No'");
                return;
            }

            UpdateDealCreationStatus(dealsetupId, dealName);
            Xrm.Page.data.refresh();
        }
    }
}

function GetDealCreationStatus(dealsetupId) {
    
    var req = new XMLHttpRequest();
    req.open("GET", Xrm.Page.context.getClientUrl() + "/api/data/v9.0/wipro_dealsetups(" + dealsetupId + ")?$select=wipro_dealcreatestatus", false);
    req.setRequestHeader("OData-MaxVersion", "4.0");
    req.setRequestHeader("OData-Version", "4.0");
    req.setRequestHeader("Accept", "application/json");
    req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
    req.setRequestHeader("Prefer", "odata.include-annotations=\"*\"");
    req.onreadystatechange = function () {
        if (this.readyState === 4) {
            req.onreadystatechange = null;
            if (this.status === 200) {
                var result = JSON.parse(this.response);
                wipro_dealcreatestatus = result["wipro_dealcreatestatus"];
            } else {
                Xrm.Utility.alertDialog(this.statusText);
            }
        }
    };
    req.send();
}

function UpdateDealCreationStatus(dealsetupId, dealName) {
    
    var entity = {};
    entity.wipro_dealcreatestatus = "1";

    var req = new XMLHttpRequest();
    req.open("PATCH", Xrm.Page.context.getClientUrl() + "/api/data/v9.0/wipro_dealsetups(" + dealsetupId + ")", false);
    req.setRequestHeader("OData-MaxVersion", "4.0");
    req.setRequestHeader("OData-Version", "4.0");
    req.setRequestHeader("Accept", "application/json");
    req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
    req.onreadystatechange = function () {
        if (this.readyState === 4) {
            req.onreadystatechange = null;
            if (this.status === 204) {
                Xrm.Utility.alertDialog("Deal record with name '" + dealName + "' has been created successfully!");
            } else {
                Xrm.Utility.alertDialog(this.statusText);
            }
        }
    };
    req.send(JSON.stringify(entity));
}

function OnDealSetupFormSave(context) {
    
    var createspdoc = Xrm.Page.getAttribute("wipro_createsharepointfolder").getValue();
    var spMemberCount = Xrm.Page.getControl("sharepointaccess").getGrid().getTotalRecordCount();

    if (createspdoc == true && spMemberCount < 1) {
        alert("As you have opted for creating document folder on Sharepoint, please add members to the Sharepoint Access Configuration section. If you do not wish to add any member, please set the 'Sharepoint Folder Creation Required?' field to 'No'");
        context.getEventArgs().preventDefault();
        return false;
    }
}

function OnDealSetupFormLoad() {
    
    if (Xrm.Page.ui.getFormType() == 2) {
        var dealsetupId = Xrm.Page.data.entity.getId().slice(1, -1);
        GetDealCreationStatus(dealsetupId);
        
        if (wipro_dealcreatestatus == "1") {
            Xrm.Page.getControl('wipro_createsharepointfolder').setDisabled(true);
        }
    }

    //SetSharepointAccessGridVisibility();
}

function SetSharepointAccessGridVisibility() {
    
    var createspdoc = Xrm.Page.getAttribute("wipro_createsharepointfolder").getValue();

    if (createspdoc !== true) {
        Xrm.Page.ui.tabs.get("sharepointaccess").setVisible(false);
    }
    else {
        Xrm.Page.ui.tabs.get("sharepointaccess").setVisible(true);
    }
}

function SetExecuteButtonVisibility() {
    
    if (Xrm.Page.ui.getFormType() == 2) {
        var dealsetupId = Xrm.Page.data.entity.getId().slice(1, -1);
        GetDealCreationStatus(dealsetupId);

        if (wipro_dealcreatestatus == "1") {
            return false;
        }
        else {
            return true;
        }
    }
    else {
        return false;
    }
}

function CheckMandatoryFieldsPopulated() {
    
    var allfieldspopulated = true;
    var fields = ["wipro_name", "wipro_prospect", "wipro_validfrom", "wipro_validtill", "wipro_accessteamname", "wipro_businessunit", "wipro_accessprivileges", "wipro_createsharepointfolder"];

    for (var i = 0; i < fields.length; i++) {

        if (Xrm.Page.getAttribute(fields[i]).getValue() == null) {
            allfieldspopulated = false;
            break;
        }
    }
    return allfieldspopulated;
}