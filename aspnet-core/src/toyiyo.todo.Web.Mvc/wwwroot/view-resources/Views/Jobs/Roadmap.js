(function ($) {
    // Add this check at the beginning of your script
    if (!$.fn.draggable) {
        console.error('jQuery UI draggable not loaded!');
        return;
    }

    var _jobService = abp.services.app.job;
    var _$roadmapContainer = $('#roadmapContainer');
    var _$monthlyViewBtn = $('#monthlyView');
    var _$quarterlyViewBtn = $('#quarterlyView');
    var _$startDate = $('#startDate');
    var _$endDate = $('#endDate');

    function initialize() {
        // Set default date range (6 months from now)
        var today = new Date();
        _$startDate.val(today.toISOString().split('T')[0]);
        today.setMonth(today.getMonth() + 6);
        _$endDate.val(today.toISOString().split('T')[0]);

        // Initialize view
        loadRoadmapData();

        // Bind events
        _$monthlyViewBtn.click(function() {
            _$monthlyViewBtn.addClass('active');
            _$quarterlyViewBtn.removeClass('active');
            loadRoadmapData();
        });

        _$quarterlyViewBtn.click(function() {
            _$quarterlyViewBtn.addClass('active');
            _$monthlyViewBtn.removeClass('active');
            loadRoadmapData();
        });

        _$startDate.change(loadRoadmapData);
        _$endDate.change(loadRoadmapData);
    }

    function loadRoadmapData() {
        var startDate = new Date(_$startDate.val());
        var endDate = new Date(_$endDate.val());
        var viewType = _$monthlyViewBtn.hasClass('active') ? 'Monthly' : 'Quarterly';

        abp.ui.setBusy(_$roadmapContainer);
        _jobService.getRoadmapView(startDate, endDate, viewType)
            .done(function (result) {
                renderRoadmap(result);
            })
            .fail(function (err) {
                abp.notify.error('Failed to load roadmap data');
                console.error(err);
            })
            .always(function() {
                abp.ui.clearBusy(_$roadmapContainer);
            });
    }

    function renderRoadmap(data) {
        _$roadmapContainer.empty();

        // Calculate timeline dimensions
        const timelineStart = new Date(data.startDate);
        const timelineEnd = new Date(data.endDate);
        const totalDays = Math.ceil((timelineEnd - timelineStart) / (1000 * 60 * 60 * 24));
        const dayWidth = 25;
        const timelineWidth = Math.max(totalDays * dayWidth, _$roadmapContainer.width());

        // Create timeline container
        var $timeline = $('<div>')
            .addClass('roadmap-timeline')
            .css('width', timelineWidth + 'px');

        // Add timeline grid
        var $timelineGrid = $('<div>').addClass('timeline-grid');
        var intervals = data.viewTypeValue === 'Monthly' ? 
            getMonthIntervals(timelineStart, timelineEnd) :
            getQuarterIntervals(timelineStart, timelineEnd);

        intervals.forEach(() => {
            $timelineGrid.append($('<div>').addClass('timeline-grid-line'));
        });
        $timeline.append($timelineGrid);

        // Add timeline scale
        var $timelineScale = $('<div>').addClass('timeline-scale');
        intervals.forEach(interval => {
            $timelineScale.append($('<div>')
                .addClass('timeline-scale-marker')
                .text(interval.label));
        });
        _$roadmapContainer.append($timelineScale);

        // Group jobs by parent ID
        const jobsMap = new Map();
        const rootJobs = [];
        
        // First, create a map of all jobs
        data.jobs.forEach(job => {
            jobsMap.set(job.id, {
                job: job,
                children: []
            });
        });

        // Then, organize into parent-child relationships
        data.jobs.forEach(job => {
            if (job.parentId && jobsMap.has(job.parentId)) {
                jobsMap.get(job.parentId).children.push(job);
            } else {
                rootJobs.push(job);
            }
        });

        // Track vertical position
        let currentRow = 0;

        // Render root jobs and their children
        rootJobs.forEach(rootJob => {
            const $rootElement = createJobElement(rootJob);
            positionJobElement($rootElement, rootJob, timelineStart, dayWidth, currentRow++ * 90);
            $timeline.append($rootElement);

            // Render children if any
            const children = jobsMap.get(rootJob.id)?.children || [];
            children.forEach(childJob => {
                const $childElement = createJobElement(childJob);
                positionJobElement($childElement, childJob, timelineStart, dayWidth, currentRow++ * 90);
                $childElement.addClass('task-indent');
                $timeline.append($childElement);
            });
        });

        _$roadmapContainer.append($timeline);

        // Initialize draggable
        initializeDraggable(dayWidth, timelineStart);
    }

    function positionJobElement($element, job, timelineStart, dayWidth, topOffset) {
        // If no start date, calculate backwards from due date
        let jobStart, jobEnd;
        
        if (job.startDate) {
            jobStart = new Date(job.startDate);
            jobEnd = new Date(job.dueDate || timelineStart);
        } else if (job.dueDate) {
            jobEnd = new Date(job.dueDate);
            // If only due date is set, assume 2 weeks duration
            jobStart = new Date(jobEnd);
            jobStart.setDate(jobEnd.getDate() - 14);
        } else {
            // If neither date is set, position at timeline start with default duration
            jobStart = new Date(timelineStart);
            jobEnd = new Date(timelineStart);
            jobEnd.setDate(jobEnd.getDate() + 14);
        }

        const daysFromStart = Math.ceil((jobStart - timelineStart) / (1000 * 60 * 60 * 24));
        const jobDuration = Math.ceil((jobEnd - jobStart) / (1000 * 60 * 60 * 24));
        
        $element.css({
            left: (daysFromStart * dayWidth) + 'px',
            width: Math.max((jobDuration * dayWidth), 200) + 'px',
            top: topOffset + 'px'    // Use provided top offset instead of nth-child
        });

        // Add visual indicator for items without start date
        if (!job.startDate) {
            $element.addClass('no-start-date');
        }

        $element.data('job-data', {
            startDate: jobStart,
            dueDate: jobEnd,
            duration: jobDuration,
            hasStartDate: !!job.startDate
        });
    }

    function initializeDraggable(dayWidth, timelineStart) {
        $('.roadmap-item').draggable({
            axis: 'x',
            containment: 'parent',
            grid: [dayWidth, 0],
            start: function(event, ui) {
                $(this).addClass('ui-draggable-dragging');
            },
            stop: function(event, ui) {
                $(this).removeClass('ui-draggable-dragging');
                const jobId = $(this).data('job-id');
                const daysOffset = Math.round(ui.position.left / dayWidth);
                const newStartDate = new Date(timelineStart);
                newStartDate.setDate(newStartDate.getDate() + daysOffset);
                
                updateJobDates(jobId, newStartDate);
            }
        });
    }

    function getMonthIntervals(start, end) {
        const intervals = [];
        let current = new Date(start);
        
        while (current <= end) {
            intervals.push({
                date: new Date(current),
                label: current.toLocaleString('default', { month: 'short', year: 'numeric' })
            });
            current.setMonth(current.getMonth() + 1);
        }
        
        return intervals;
    }

    function getQuarterIntervals(start, end) {
        const intervals = [];
        let current = new Date(start);
        
        while (current <= end) {
            intervals.push({
                date: new Date(current),
                label: 'Q' + (Math.floor(current.getMonth() / 3) + 1) + ' ' + current.getFullYear()
            });
            current.setMonth(current.getMonth() + 3);
        }
        
        return intervals;
    }

    function createTimelineScale(startDate, endDate, viewType) {
        var $scale = $('<div>').addClass('timeline-scale');
        var start = new Date(startDate);
        var end = new Date(endDate);
        var current = new Date(start);

        while (current <= end) {
            var label = viewType === 'Monthly' 
                ? current.toLocaleString('default', { month: 'short', year: 'numeric' })
                : 'Q' + (Math.floor(current.getMonth() / 3) + 1) + ' ' + current.getFullYear();
            
            $scale.append($('<div>')
                .addClass('timeline-scale-marker')
                .text(label));

            if (viewType === 'Monthly') {
                current.setMonth(current.getMonth() + 1);
            } else {
                current.setMonth(current.getMonth() + 3);
            }
        }

        return $scale;
    }

    function createJobElement(job) {
        var $jobElement = $('<div>')
            .addClass('roadmap-item')
            .addClass(job.level === 2 ? 'roadmap-epic' : 'roadmap-task')
            .data('job-id', job.id);

        var $header = $('<div>')
            .addClass('roadmap-item-header')
            .text(job.title);

        var $dates = $('<div>')
            .addClass('roadmap-item-dates');
        
        if (!job.startDate) {
            $dates.append($('<i class="fas fa-exclamation-triangle text-warning mr-1" title="No start date set"></i>'));
        }
        
        $dates.append(formatDate(job.startDate) + ' - ' + formatDate(job.dueDate));

        var $status = $('<div>')
            .addClass('roadmap-item-status')
            .addClass('status-' + job.jobStatus)
            .text(job.jobStatus);

        $jobElement
            .append($header)
            .append($dates)
            .append($status);

        $jobElement.draggable({
            axis: 'x',
            containment: 'parent',
            stop: function(event, ui) {
                // Handle date update based on position
                var offsetDays = Math.round(ui.position.left / 30); // Assuming 30px per day
                updateJobDates(job.id, offsetDays);
            }
        });

        return $jobElement;
    }

    function formatDate(dateString) {
        if (!dateString) return 'Not set';
        return new Date(dateString).toLocaleDateString();
    }

    function updateJobDates(jobId, newStartDate) {
        var job = _$roadmapContainer.find(`[data-job-id="${jobId}"]`).data('job-data');
        if (!job) return;

        const duration = job.dueDate - job.startDate;
        const newDueDate = new Date(newStartDate.getTime() + duration);

        _jobService.setStartDate(jobId, newStartDate)
            .then(() => _jobService.setDueDate(jobId, newDueDate))
            .then(() => {
                abp.notify.success('Job dates updated');
                loadRoadmapData();
            })
            .catch(err => {
                abp.notify.error('Failed to update job dates');
                console.error(err);
                loadRoadmapData();
            });
    }

    // Initialize when document is ready
    $(document).ready(function () {
        initialize();
    });
})(jQuery);
