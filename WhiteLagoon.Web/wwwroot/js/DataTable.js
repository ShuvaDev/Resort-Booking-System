var dataTable;
$(document).ready(function () {
    const urlParams = new URLSearchParams(window.location.search);
    const status = urlParams.get('status');
    loadDataTable(status);
});

function loadDataTable(status) {
    dataTable = $('#myTable').DataTable({
        "ajax": { url: '/Booking/GetBookingList?status=' + status },
        "columns": [
            { data: 'id', "width": "5%" },
            { data: 'name', "width": "15%" },
            { data: 'email', "width": "15%" },
            { data: 'phone', "width": "15%" },
            { data: 'status', "width": "10%" },
            { data: 'checkInDate', "width": "10%" },
            { data: 'nights', "width": "10%" },
            { data: 'totalCost', "width": "10%", render: $.fn.dataTable.render.number(',', '.', 2) },
            {
                data: 'id',
                "render": function (data) {
                    return `<div class="btn-group" role="group">
                	        <a href="/Booking/BookingDetails?bookingId=${data}" class="btn btn-primary mx-2">
                            	<i class="bi bi-pencil-square"></i> Details
                        	</a>
                	</div>`
                },
                "width": "10%"
            }
        ],
        "bDestroy": true
    });
}
