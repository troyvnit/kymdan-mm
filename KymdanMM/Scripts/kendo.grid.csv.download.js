

var toCSV = function (gridId, ignoredTemplates) {
    var csv = '';

    // Get access to basic grid data
    var grid = $("#" + gridId).data("kendoGrid"),
        datasource = grid.dataSource,
        multiSelect = $("#" + gridId + "Columns").data("kendoMultiSelect");

    // Increase page size to cover all the data and get a reference to that data
    //datasource.pageSize(datasource.total());
    var data = datasource.view();

    //add the header row
    for (var i = 0; i < grid.columns.length; i++) {
        var title = grid.columns[i].title,
            field = grid.columns[i].field;
        if (typeof (field) === "undefined" || $.inArray(field, multiSelect.value()) == -1) { continue; /* no data! */ }
        if (typeof (title) === "undefined") { title = field }

        title = title.replace(/"/g, '""');
        csv += '"' + title + '"';
        if (i < grid.columns.length - 1) {
            csv += ",";
        }
    }
    csv += "\r\n";
	
    //add each row of data
    for (var row in data) {
        var tr = grid.content.find('tr[role=row]') != undefined ? grid.content.find('tr[role=row]')[row] : undefined;
        console.log(tr);
        if (tr != undefined) {
            for (var i = 0; i < grid.columns.length; i++) {
                var fieldName = grid.columns[i].field;

                if (typeof (fieldName) === "undefined" || $.inArray(fieldName, multiSelect.value()) == -1) { continue; }
                var td = tr.cells != undefined ? tr.cells[i] : undefined;
                console.log(td);
                var text = td != undefined ? td.innerText : undefined;
                var value = text;

                if (!value) {
                    value = "";
                } else {
                    value = value.toString();
                }

                value = value.replace(/"/g, '""');
                csv += '"' + value + '"';
                if (i < grid.columns.length - 1) {
                    csv += ",";
                }
            }
        }
        csv += "\r\n";
    }

    // Reset datasource
    //datasource.pageSize(originalPageSize);

    return csv;
};

var toTable = function (gridId, ignoredTemplates) {
    var table = '<table><tr>';

    // Get access to basic grid data
    var grid = $("#" + gridId).data("kendoGrid"),
        datasource = grid.dataSource,
        multiSelect = $("#" + gridId + "Columns").data("kendoMultiSelect");

    // Increase page size to cover all the data and get a reference to that data
    //datasource.pageSize(datasource.total());
    var data = datasource.view();

    //add the header row
    for (var i = 0; i < grid.columns.length; i++) {
        var title = grid.columns[i].title,
            field = grid.columns[i].field;
        if (typeof (field) === "undefined" || $.inArray(field, multiSelect.value()) == -1) { continue; /* no data! */ }
        if (typeof (title) === "undefined") { title = field }

        table += '<th>' + title + '</th>';
    }
    table += "</tr>";

    //add each row of data
    for (var row in data) {
        var tr = grid.content.find('tr[role=row]') != undefined ? grid.content.find('tr[role=row]')[row] : undefined;
        console.log(tr);
        if (tr != undefined) {
            table += "<tr>";
            for (var i = 0; i < grid.columns.length; i++) {
                var fieldName = grid.columns[i].field;

                if (typeof (fieldName) === "undefined" || $.inArray(fieldName, multiSelect.value()) == -1) { continue; }
                var td = tr.cells != undefined ? tr.cells[i] : undefined;
                console.log(td);
                var text = td != undefined ? td.innerText : undefined;
                var value = text;

                if (!value) {
                    value = "";
                } else {
                    value = value.toString();
                }

                table += '<td>' + value + '</td>';
            }
            table += "</tr>";
        }
    }

    // Reset datasource
    //datasource.pageSize(originalPageSize);

    return table;
};
