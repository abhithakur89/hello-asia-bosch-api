const connection = new signalR.HubConnectionBuilder()
	.withUrl("https://hello-asia-bosch-api.azurewebsites.net/boschapihub")
    .build();

async function start() {
    try {
        await connection.start();
        console.log("connected");
    } catch (err) {
        console.log(err);
        setTimeout(() => start(), 5000);
    }
};

connection.onclose(async () => {
    await start();
});

connection.on("NewIn", (cameraId) => {
    console.log("NewIn: " + cameraId);
	PopupEntry();
});

connection.on("NewOut", (cameraId) => {
    console.log("NewOut: " + cameraId);
	PopupExit();
});

async function PopupExit() {
	fetch("https://hello-asia-bosch-api.azurewebsites.net/gettodayexits?cameraid=1&recordcount=50000")
		.then(response => {
			return response.json();
		})
		.then(data => {
			var table = document.getElementById("ExitTable");

			while (table.rows.length > 1) {
				table.deleteRow(1);
			}

			for (var i = 0; i < data.records.length; i++) {

				var tr = table.insertRow(-1);
				if (i >= 20) {
					tr.style.display = 'none'
				}

				for (var j = 0; j < 2; j++) {
					var tabCell = tr.insertCell(-1);

					if (j == 0) {
						tabCell.innerHTML = data.records[i].timestamp;
					}
					else {
						tabCell.style.textAlign = "center";
						tabCell.innerHTML = data.records[i].count;
					}
				}
			}
			var label = document.getElementById("exitCount");
			label.innerHTML = data.count;
		})
};

async function PopupEntry() {
	fetch("https://hello-asia-bosch-api.azurewebsites.net/gettodayentries?cameraid=1&recordcount=50000")
		.then(response => {
			return response.json();
		})
		.then(data => {
			var table = document.getElementById("EntryTable");

			while (table.rows.length > 1) {
				table.deleteRow(1);
			}

			for (var i = 0; i < data.records.length; i++) {

				var tr = table.insertRow(-1);

				if (i >= 20) {
					tr.style.display = 'none'
				}

				for (var j = 0; j < 2; j++) {
					var tabCell = tr.insertCell(-1);

					if (j == 0) {
						tabCell.innerHTML = data.records[i].timestamp;
					}
					else {
						tabCell.style.textAlign = "center";
						tabCell.innerHTML = data.records[i].count;
					}
				}
			}
			var label = document.getElementById("entryCount");
			label.innerHTML = data.count;
		})
};



connection.on("CrowdDensityChanged", (cameraId, density) => {
	console.log("CrowdDensityChanged: " + cameraId + ", " + density);
	var today = new Date();
	DisplayAlarm(density, formatDate(today));
});

function DisplayAlarm(density, dt) {
	var label = document.getElementById("alertTimestamp");
	label.innerHTML = dt;

	var alramimage = document.getElementById("alramimage");
	var crowdimage = document.getElementById("crowdimage");
	var goodimage = document.getElementById("goodimage");

	if (density == 1) {
		alramimage.style.display = 'none';
		crowdimage.style.display = 'none';
		goodimage.style.display = '';
	} else if (density == 2) {
		alramimage.style.display = 'none';
		crowdimage.style.display = '';
		goodimage.style.display = 'none';
	} else if (density == 3) {
		alramimage.style.display = '';
		crowdimage.style.display = 'none';
		goodimage.style.display = 'none';
	}

}

// Quick and simple export target #table_id into a csv
function download_table_as_csv(table_id) {
    // Select rows from table_id
    var rows = document.querySelectorAll('table#' + table_id + ' tr');
    // Construct csv
    var csv = [];
    for (var i = 0; i < rows.length; i++) {
        var row = [], cols = rows[i].querySelectorAll('td, th');
        for (var j = 0; j < cols.length; j++) {
            // Clean innertext to remove multiple spaces and jumpline (break csv)
            var data = cols[j].innerText.replace(/(\r\n|\n|\r)/gm, '').replace(/(\s\s)/gm, ' ')
            // Escape double-quote with double-double-quote (see https://stackoverflow.com/questions/17808511/properly-escape-a-double-quote-in-csv)
            data = data.replace(/"/g, '""');
            // Push escaped string
            row.push('"' + data + '"');
        }
        csv.push(row.join(','));
    }
    var csv_string = csv.join('\n');
    // Download it
    var filename = 'export_' + table_id + '_' + new Date().toLocaleDateString() + '.csv';
    var link = document.createElement('a');
    link.style.display = 'none';
    link.setAttribute('target', '_blank');
    link.setAttribute('href', 'data:text/csv;charset=utf-8,' + encodeURIComponent(csv_string));
    link.setAttribute('download', filename);
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
}

function formatDate(dateVal) {
	var newDate = new Date(dateVal);

	var sMonth = padValue(newDate.getMonth() + 1);
	var sDay = padValue(newDate.getDate());
	var sYear = newDate.getFullYear();
	var sHour = newDate.getHours();
	var sMinute = padValue(newDate.getMinutes());
	var sAMPM = "AM";

	var iHourCheck = parseInt(sHour);

	if (iHourCheck > 12) {
		sAMPM = "PM";
		sHour = iHourCheck - 12;
	}
	else if (iHourCheck === 0) {
		sHour = "12";
	}

	sHour = padValue(sHour);

	return sMonth + "-" + sDay + "-" + sYear + " " + sHour + ":" + sMinute + " " + sAMPM;
}

function padValue(value) {
	return (value < 10) ? "0" + value : value;
}

function getLatestAlarms() {
	fetch("https://hello-asia-bosch-api.azurewebsites.net/gettodayalarmlevel?cameraid=1&recordcount=50000")
		.then(response => {
			return response.json();
		})
		.then(data => {
			var table = document.getElementById("alarmTable");

			table.innerHTML = "";

			var hr = table.insertRow(-1);
			var hc1 = hr.insertCell(-1);
			hc1.innerHTML = "Timestamp";
			var hc2 = hr.insertCell(-1);
			hc2.innerHTML = "Level";

			for (var i = 0; i < data.records.length; i++) {

				var tr = table.insertRow(-1);
				for (var j = 0; j < 2; j++) {
					var tabCell = tr.insertCell(-1);

					if (j == 0) {
						tabCell.innerHTML = data.records[i].timestamp;
					}
					else {
						tabCell.innerHTML = data.records[i].level;
					}
				}
			}

			download_table_as_csv('alarmTable');
		});
}

async function PopupInitialAlarm() {
	fetch("https://hello-asia-bosch-api.azurewebsites.net/getlatestalarmlevel?cameraid=1")
		.then(response => {
			return response.json();
		})
		.then(data => {
			DisplayAlarm(data.level, data.timestamp)
		})
};


start();
PopupExit();
PopupEntry();
PopupInitialAlarm();