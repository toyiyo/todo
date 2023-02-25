$(function () {

    'use strict';

    /* ChartJS
     * -------
     * Here we will create a few charts using ChartJS
     */

    //-----------------------
    //- MONTHLY SALES CHART -
    //-----------------------

    // Get context with jQuery - using jQuery's .get() method.
    var salesChartCanvas = $('#salesChart').get(0).getContext('2d');
    // This will get the first returned node in the jQuery collection.

    const MONTHS = [
        'January',
        'February',
        'March',
        'April',
        'May',
        'June',
        'July',
        'August',
        'September',
        'October',
        'November',
        'December'
    ];

    var completedPerMonth = JSON.parse(document.querySelector('#home-page-stats-data').dataset.completedMonthly);

    function filterAndGroupByMonth(data) {
        // Get the current year
        const currentYear = new Date().getFullYear();

        // Filter the data by year and sort it by month
        const filteredData = data
            .filter(item => item.Year === currentYear)
            .sort((a, b) => {
                // Compare the month names to sort them in chronological order
                return MONTHS.indexOf(a.Month) - MONTHS.indexOf(b.Month);
            });

        // Group the data by month and get the counts
        const groupedData = Array(12).fill(0); // Initialize an array with 12 zeros
        filteredData.forEach(item => {
            const monthIndex = new Date(`${item.Month} 1, 2000`).getMonth(); // Get the month index (0-11) from the month name
            groupedData[monthIndex] += item.Count; // Add the count to the corresponding month index
        });

        // Return the grouped data as an array of integers
        return groupedData;
    }

    var salesChartData = {
        labels: MONTHS,
        datasets: [
            {
                label: 'Completed',
                fill: '#dee2e6',
                borderColor: '#ced4da',
                pointBackgroundColor: '#ced4da',
                pointBorderColor: '#c1c7d1',
                pointHoverBackgroundColor: '#fff',
                pointHoverBorderColor: 'rgb(220,220,220)',
                spanGaps: true,
                data: filterAndGroupByMonth(completedPerMonth)
            },
        ]
    };

    var salesChartOptions = {
        //Boolean - If we should show the scale at all
        showScale: true,
        //Boolean - Whether grid lines are shown across the chart
        scaleShowGridLines: false,
        //String - Colour of the grid lines
        scaleGridLineColor: 'rgba(0,0,0,.05)',
        //Number - Width of the grid lines
        scaleGridLineWidth: 1,
        //Boolean - Whether to show horizontal lines (except X axis)
        scaleShowHorizontalLines: true,
        //Boolean - Whether to show vertical lines (except Y axis)
        scaleShowVerticalLines: true,
        //Boolean - Whether the line is curved between points
        bezierCurve: true,
        //Number - Tension of the bezier curve between points
        bezierCurveTension: 0.3,
        //Boolean - Whether to show a dot for each point
        pointDot: false,
        //Number - Radius of each point dot in pixels
        pointDotRadius: 4,
        //Number - Pixel width of point dot stroke
        pointDotStrokeWidth: 1,
        //Number - amount extra to add to the radius to cater for hit detection outside the drawn point
        pointHitDetectionRadius: 20,
        //Boolean - Whether to show a stroke for datasets
        datasetStroke: true,
        //Number - Pixel width of dataset stroke
        datasetStrokeWidth: 2,
        //Boolean - Whether to fill the dataset with a color
        datasetFill: true,
        //String - A legend template
        legendTemplate: '<ul class="<%=name.toLowerCase()%>-legend"><% for (var i=0; i<datasets.length; i++){%><li><span style="background-color:<%=datasets[i].lineColor%>"></span><%=datasets[i].label%></li><%}%></ul>',
        //Boolean - whether to maintain the starting aspect ratio or not when responsive, if set to false, will take up entire container
        maintainAspectRatio: false,
        //Boolean - whether to make the chart responsive to window resizing
        responsive: true
    };

    //Create the line chart
    var salesChart = new Chart(salesChartCanvas, {
        type: 'line',
        data: salesChartData,
        options: salesChartOptions
    });

    //---------------------------
    //- END MONTHLY SALES CHART -
    //---------------------------
});
