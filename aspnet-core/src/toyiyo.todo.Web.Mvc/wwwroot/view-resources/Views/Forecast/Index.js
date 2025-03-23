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
        _forecastService.getForecast(projectId, parseInt(level))
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

        // Combine all unique dates from actual and forecast data
        const allDates = new Set([
            ...result.actualProgress.map(p => p.date),
            ...result.forecastProgress.map(p => p.date),
            ...result.optimisticProgress.map(p => p.date),
            ...result.conservativeProgress.map(p => p.date)
        ].sort());

        // Create a sorted array of all dates
        const sortedDates = Array.from(allDates).sort((a, b) => new Date(a) - new Date(b));

        _forecastChart = new Chart(ctx, {
            type: 'line',
            data: {
                labels: sortedDates.map(d => moment(d).format('LL')),
                datasets: [
                    {
                        label: 'Actual Progress',
                        data: generateDataPoints(sortedDates, result.actualProgress),
                        backgroundColor: '#1f77b4',
                        borderColor: '#1f77b4',
                        borderWidth: 2,
                        fill: false
                    },
                    {
                        label: 'Forecast',
                        data: generateDataPoints(sortedDates, result.forecastProgress),
                        backgroundColor: '#ff7f0e',
                        borderColor: '#ff7f0e',
                        borderDash: [5, 5],
                        borderWidth: 2,
                        fill: false
                    },
                    {
                        label: 'Optimistic (P10)',
                        data: generateDataPoints(sortedDates, result.optimisticProgress),
                        backgroundColor: '#2ca02c',
                        borderColor: '#2ca02c',
                        borderDash: [10, 5],
                        borderWidth: 2,
                        fill: false
                    },
                    {
                        label: 'Conservative (P90)',
                        data: generateDataPoints(sortedDates, result.conservativeProgress),
                        backgroundColor: '#d62728',
                        borderColor: '#d62728',
                        borderDash: [3, 3],
                        borderWidth: 2,
                        fill: false
                    }
                ]
            },
            options: {
                responsive: true,
                scales: {
                    x: {
                        type: 'category',
                        title: {
                            display: true,
                            text: 'Dates'
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
                                return context.dataset.label + ': ' + context.parsed.y + '%';
                            }
                        }
                    }
                }
            }
        });
    }

    function generateDataPoints(dates, progressData) {
        const progressMap = new Map(progressData.map(p => [p.date, p.completionPercentage]));
        return dates.map(date => {
            // Find the closest previous date with data
            const progressDate = [...progressMap.keys()]
                .filter(d => new Date(d) <= new Date(date))
                .sort((a, b) => new Date(b) - new Date(a))[0];
            
            return progressDate ? progressMap.get(progressDate) : null;
        });
    }

    $(document).ready(function () {
        initialize();
    });
})(jQuery);
