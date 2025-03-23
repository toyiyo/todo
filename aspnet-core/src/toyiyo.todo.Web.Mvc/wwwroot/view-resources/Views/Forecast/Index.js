(function ($) {
    const _forecastService = abp.services.app.forecast;
    let _forecastChart = null;

    function initialize() {
        $('#levelSelect').on('change', loadForecast);
        loadForecast();
    }

    function loadForecast() {
        const level = $('#levelSelect').val();

        abp.ui.setBusy($('body'));
        _forecastService.getForecast(projectId, parseInt(level))  // Use projectId from view
            .done(renderForecast)
            .always(() => abp.ui.clearBusy($('body')));
    }

    function renderForecast(result) {
        renderDates(result);
        renderChart(result);
    }

    function renderDates(result) {
        $('#optimisticDate').text(moment(result.optimisticCompletionDate).format('LL'));
        $('#estimatedDate').text(moment(result.estimatedCompletionDate).format('LL'));
        $('#conservativeDate').text(moment(result.conservativeCompletionDate).format('LL'));
    }

    function renderChart(result) {
        const ctx = document.getElementById('forecastChart').getContext('2d');
        
        if (_forecastChart) {
            _forecastChart.destroy();
        }

        _forecastChart = new Chart(ctx, {
            type: 'line',
            data: {
                datasets: [
                    {
                        label: 'Actual Progress',
                        data: result.actualProgress.map(p => ({
                            x: moment(p.date).toDate(),
                            y: p.completionPercentage
                        })),
                        borderColor: '#1f77b4',
                        fill: false
                    },
                    {
                        label: 'Forecast',
                        data: result.forecastProgress.map(p => ({
                            x: moment(p.date).toDate(),
                            y: p.completionPercentage
                        })),
                        borderColor: '#ff7f0e',
                        borderDash: [5, 5],
                        fill: false
                    }
                ]
            },
            options: {
                responsive: true,
                scales: {
                    x: {
                        type: 'time',
                        time: {
                            unit: 'week'
                        }
                    },
                    y: {
                        beginAtZero: true,
                        max: 100,
                        title: {
                            display: true,
                            text: 'Completion %'
                        }
                    }
                },
                plugins: {
                    tooltip: {
                        callbacks: {
                            label: function(context) {
                                return `${context.dataset.label}: ${Math.round(context.parsed.y)}%`;
                            }
                        }
                    }
                }
            }
        });
    }

    $(document).ready(function () {
        initialize();
    });
})(jQuery);
