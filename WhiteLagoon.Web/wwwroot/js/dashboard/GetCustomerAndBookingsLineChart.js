$(document).ready(function () {
    loadNewMembersAndBookingLineChart();
});

function loadNewMembersAndBookingLineChart() {
    $('.chart-spinner').show();

    $.ajax({
        url: "/dashboard/GetMemberAndBookingLineChartData",
        type: "GET",
        dataType: "json",

        success: function (data) {

            loadLineChart("newMembersAndBookingLineChart", data);

            $('.chart-spinner').hide();
        }
    });
}


function loadLineChart(id, data) {
    var chartColors = getChartColorsArray(id);
    var options = {
        series: data.series,
        colors: chartColors,

        chart: {
            type: 'line',
            height : 280
        },
        markers: {
            size: 3,
            strokeWidth: 0,
            hover: {
                size: 7
            }
        },
        stroke: {
            curve: 'smooth',
            width : 3
        },
        xaxis: {
            categories: data.categories
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

