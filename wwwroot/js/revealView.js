let additionalHeaders = {};

window.loadRevealView = async function (viewId, dashboardName, headers) {
    let rvDashboard;

    if (dashboardName) {
        rvDashboard = await $.ig.RVDashboard.loadDashboard(dashboardName);
    }

    additionalHeaders = headers || {};

    console.log("additionalHeaders: " + JSON.stringify(additionalHeaders));

    $.ig.RevealSdkSettings.setAdditionalHeadersProvider(function (url) {
        return headers;
    });

    const revealView = createRevealView(viewId, dashboardName);
    //console.log("dashboardName: " + dashboardName);

    if (!rvDashboard) {
        revealView.startInEditMode = true;

        revealView.onDataSourcesRequested = (callback) => {
            const restDataSource = new $.ig.RVRESTDataSource();
            restDataSource.id = "RestDataSource";
            restDataSource.url = "https://excel2json.io/api/share/8bf0acfa-7fd8-468e-0478-08daa4a8d995";
            restDataSource.title = "Auto Users Data - Global";
            restDataSource.subtitle = "from Excel2Json";
            restDataSource.useAnonymousAuthentication = true;

            var sqlServerDataSource = new $.ig.RVAzureSqlDataSource();
            sqlServerDataSource.id="sqlServer";
            sqlServerDataSource.title = "SQL Server Data Source";
            sqlServerDataSource.subtitle = "Full Northwind Database";
    
            // SQL Server Data Source Item in Stored Procs
            var sqlDataSourceItem1 = new $.ig.RVAzureSqlDataSourceItem(sqlServerDataSource);
            sqlDataSourceItem1.id="CustomerOrders";
            sqlDataSourceItem1.title = "Customer Orders";
            sqlDataSourceItem1.subtitle = "Custom SQL Query (orderId)";
    
            var sqlDataSourceItem2 = new $.ig.RVAzureSqlDataSourceItem(sqlServerDataSource);
            sqlDataSourceItem2.id="CustOrderHist";
            sqlDataSourceItem2.title = "Customer Orders History";
            sqlDataSourceItem2.subtitle = "Stored Procedure (customerId)";
    
            var sqlDataSourceItem3 = new $.ig.RVAzureSqlDataSourceItem(sqlServerDataSource);
            sqlDataSourceItem3.id="CustOrdersOrders";
            sqlDataSourceItem3.title = "Customer Orders Orders";
            sqlDataSourceItem3.subtitle = "Stored Procedure  (customerId)";
    
            var sqlDataSourceItem4 = new $.ig.RVAzureSqlDataSourceItem(sqlServerDataSource);
            sqlDataSourceItem4.id="TenMostExpensiveProducts";
            sqlDataSourceItem4.title = "Ten Most Expensive Products";
            sqlDataSourceItem4.subtitle = "Stored Procedure";

            callback(new $.ig.RevealDataSources([restDataSource, sqlServerDataSource], 
                [sqlDataSourceItem1, sqlDataSourceItem2, sqlDataSourceItem3, sqlDataSourceItem4], false));
        };
    }

    revealView.onDashboardSelectorRequested = (args) => {
        openDialog(args.callback);
    };

    revealView.onLinkedDashboardProviderAsync = (dashboardId, title) => {
        return $.ig.RVDashboard.loadDashboard(dashboardId);
    };

    revealView.dashboard = rvDashboard;
}

window.createRevealView = function (viewId) {

    $.ig.RevealSdkSettings.theme = createRevealTheme(viewId);

    const revealView = new $.ig.RevealView("#" + viewId);

    //if (singleVisualizationMode) {
    //    revealView.singleVisualizationMode = true;
    //    revealView.showHeader = false;
    //    revealView.showMenu = false;
    //}

    revealView.onSave = (rv, args) => {
        if (args.saveAs) {
            console.log("i am saving as");
            DotNet.invokeMethodAsync('DashboardViewer', 'PromptForDashboardName')
                .then(newName => {
                    console.log("newName " + newName);
                    if (newName) {
                        isDuplicateName(newName).then(isDuplicate => {
                            if (isDuplicate === "true") {
                                console.log("I am saving dot net");
                                DotNet.invokeMethodAsync('DashboardViewer', 'ConfirmOverride', newName)
                                    .then(overrideConfirmed => {
                                        if (!overrideConfirmed) {
                                            return;
                                        }
                                        args.dashboardId = args.name = newName;
                                        args.saveFinished();
                                        console.log("SavedAs Finished " + newName);
                                        setTimeout(() => {
                                            DotNet.invokeMethodAsync('DashboardViewer', 'ReloadDashboardList');
                                        }, 250);
                                    });
                            } else {
                                args.dashboardId = args.name = newName;
                                args.saveFinished();
                                console.log("SavedAs Finished " + newName);
                                setTimeout(() => {
                                    DotNet.invokeMethodAsync('DashboardViewer', 'ReloadDashboardList');
                                }, 250);
                            }
                        });
                    }
                    else {
                        console.log("No name");
                    }
                });
        } else {
            args.saveFinished();
            console.log("Saved Finished " + args);
            setTimeout(() => {
                DotNet.invokeMethodAsync('DashboardViewer', 'ReloadDashboardList');
            }, 250);
        }
    }
    return revealView;
  }

window.createRevealTheme = function (viewId) {
    var theme = $.ig.RevealSdkSettings.theme.clone();
    theme.chartColors = ["#09B1A9", "#003B4A", "#93C569", "#FEB51E", "#FF780D", "#CA365B"];
    theme.regularFont = "Inter";
    theme.mediumFont = "Inter";
    theme.boldFont = "Inter";
    theme.useRoundedCorners = true;
    theme.accentColor = "#09B1A9";

    //if (singleVisualizationMode) {
    //    theme.dashboardBackgroundColor = "white";
    //}
    //if (viewId === "revealViewNew") {
    //    theme.dashboardBackgroundColor = "white";
    //}
    //else {
    //    theme.dashboardBackgroundColor = "#F5F5F5";
    //}
    return theme;
}

window.createDashboardThumbnail = function (id, info) {
    console.log("info " + info);
    $.ig.RevealSdkSettings.theme = createRevealTheme(id, "");
    let thumbnailView = new $.ig.RevealDashboardThumbnailView("#" + id);
    thumbnailView.dashboardInfo = info;
}

function downloadFile(url, filename) {
    var link = document.createElement('a');
    link.href = url;
    link.download = filename;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
}

window.selectedDashboardCallback = async function (selectedDashboard) {
    console.log("selectedDashboardCallback called with: " + selectedDashboard);
    if (window.dialogCallback) {
        console.log("window.dialogCallback is defined, calling it now.");
        await window.dialogCallback(selectedDashboard);
    } else {
        console.error("window.dialogCallback is not defined.");
    }
}

function openDialog(callback) {
    window.dialogCallback = callback;
    DotNet.invokeMethodAsync('DashboardViewer', 'ToggleDialog')
        .then(() => {
            console.log('OpenDialog - Dialog toggled from JavaScript');
        })
        .catch(error => {
            console.error('OpenDialog - Error toggling dialog from JavaScript', error);
        });
}

async function isDuplicateName(dashboardName) {
    const response = await fetch(`/dashboards/${encodeURIComponent(dashboardName)}/isduplicate`);
    if (!response.ok) {
        throw new Error('Network response was not ok');
    }
    return await response.text();
}
