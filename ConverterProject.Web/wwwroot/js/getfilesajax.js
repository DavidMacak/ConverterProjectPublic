
function fetchData() {
    $.ajax({
        url: '/Pdf/GetFiles',
        type: 'GET',
        success: function (data) {
            console.log('Získaná data:', data);
            createTable(data);
        },
        error: function (error) {
            console.error('Chyba při získávání dat:', error);
        }
    });
}

fetchData();
setInterval(fetchData, 5000);

function createTable(data) {
    var table = $("<table>").addClass("table table-bordered");
    var thead = $("<thead>");
    thead.append(CreateFirstRow());
    table.append(thead);
    var tbody = $("<tbody>");
    for (var i = 0; i < data.length; i++) {
        tbody.append(CreateRow(data[i]));
    }
    table.append(tbody);
    $("#fileTable").html(table);
};

function GetDeleteButton(temporaryFileName) {
    // Vytvoření odkazu
    var link = $("<a>").attr({
        "type": "submit",
        "class": "btn btn-submit-red",
        "href": "/Pdf/DeleteFile?filename=" + temporaryFileName,
    }).text("X");

    return link;
}


function CreateFirstRow() {
    var row = $("<tr>");
    var cell = $("<th>");
    row.append(cell);
    cell = $("<th>").text("Soubor");
    row.append(cell);
    cell = $("<th>");
    row.append(cell);
    return row
}

function CreateRow(data) {
    var row = $("<tr>");

    var cell = $("<td>");
    if (data.statusType == 4 || data.statusType == 6) {
        cell.append(GetDeleteButton(data.temporaryFileName));
    }
    row.append(cell);

    var cell = $("<td>").text(data.originalFileName);
    row.append(cell);

    var cell = $("<td>");
    if (data.statusType == 1) {
        cell.append("Soubor úspěšně nahrán");
    }
    else if (data.statusType == 2) {
        cell.append("Soubor je zařazen do fronty");
    }
    else if (data.statusType == 3) {
        cell.append("Soubor se konvertuje");
    }
    else if (data.statusType == 4 || data.statusType == 6) {
        cell.append(GetDownloadButton(data.temporaryFileName));
    }
    else if (data.statusType == 5) {
        cell.append("Vyskytl se problém");
    }
    row.append(cell);
    return row;
}
function GetDownloadButton(temporaryFileName) {
    var btn = $("<a>").attr({
        "type": "submit",
        "class": "btn btn-submit",
        "href": "/Pdf/DownloadPdf?filename=" + temporaryFileName,
    }).text("Stáhnout");

    return btn;
}