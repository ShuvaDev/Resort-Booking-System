$(document).ready(function () {
    loadCustomerBookingPieChart();
});

function loadCustomerBookingPieChart() {
    $('.chart-spinner').show();

    $.ajax({
        url: "/dashboard/GetTotalBookingPieChartData",
        type: "GET",
        dataType: "json",

        success: function (data) {
            
            loadPieChart("customerBookingPieChart", data);

            $('.chart-spinner').hide();
        }
    });
}


function loadPieChart(id, data) {
    var chartColors = getChartColorsArray(id);
    var options = {
        series: data.series,
        labels: data.labels,
        colors: chartColors,
        chart: {
            type: 'pie',
            width : 380
        },
        legend: {
            position: 'bottom',
            horizontalAlign: 'center',
            labels: {
                colors: "#fff",
                useSeriesColors: true
            }
        }
    }

    var chart = new ApexCharts(document.querySelector("#" + id), options);

    chart.render();
}

function getChartColorsArray(id) {
    if (document.getElementById(id) != null) {
        var colors = document.getElementById(id).getAttribute("data-colors");
        if (colors) {
            colors = JSON.parse(colors);
            return colors.map(function (value) {
                var newValue = value.replace(" ", "");
                if (newValue.indexOf(",") === -1) {
                    var color = getComputedStyle(document.documentElement).getPropertyValue(newValue);
                    if (color) return color;
                    else return newValue;;
                }
            });
        }
    }
}

