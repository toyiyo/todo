(function ($) {
    // Add this check at the beginning of your script
    if (!$.fn.draggable) {
        console.error('jQuery UI draggable not loaded!');
        return;
    }

    var _jobService = abp.services.app.job;
    var _$roadmapContainer = $('#roadmapContainer');
    var _$startDate = $('#startDate');
    var _$endDate = $('#endDate');

    function initialize() {
        // Set default date range (1 year from now)
        var today = new Date();
        _$startDate.val(today.toISOString().split('T')[0]);
        today.setFullYear(today.getFullYear() + 1);
        _$endDate.val(today.toISOString().split('T')[0]);

        // Initialize view
        loadRoadmapData();

        // Bind events
        _$startDate.change(loadRoadmapData);
        _$endDate.change(loadRoadmapData);
    }

    function loadRoadmapData() {
        var startDate = new Date(_$startDate.val());
        var endDate = new Date(_$endDate.val());

        abp.ui.setBusy(_$roadmapContainer);
        _jobService.getRoadmapView(startDate, endDate)
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
        var timelineStart = new Date(data.startDate);
        var timelineEnd = new Date(data.endDate);
        
        // Set consistent start/end points at quarter boundaries
        timelineStart = getQuarterStart(timelineStart);
        timelineEnd = getQuarterEnd(timelineEnd);
        
        // Constants for timeline scaling
        const DAYS_PER_QUARTER = 91;
        const QUARTER_WIDTH = 272; // Width in pixels for each quarter
        const DAY_WIDTH = QUARTER_WIDTH / DAYS_PER_QUARTER;

        // Calculate total quarters and timeline width
        var totalDays = Math.ceil((timelineEnd - timelineStart) / (1000 * 60 * 60 * 24));
        var timelineWidth = Math.ceil(totalDays * DAY_WIDTH);

        // Create timeline container with calculated width
        var $timeline = $('<div>')
            .addClass('roadmap-timeline')
            .css('width', timelineWidth + 'px');

        // Add timeline grid and scale
        addTimelineGridAndScale($timeline, timelineStart, timelineEnd, QUARTER_WIDTH);

        // Group jobs by project
        var projectGroups = {};
        data.jobs.forEach(function(job) {
            var projectId = job.project.id;
            if (!projectGroups[projectId]) {
                projectGroups[projectId] = {
                    project: job.project,
                    jobs: []
                };
            }
            projectGroups[projectId].jobs.push(job);
        });

        // Track vertical position
        var currentRow = 0;

        // Render projects and their jobs
        Object.values(projectGroups).forEach(function(group) {
            // Add project header
            var $projectHeader = $('<div>')
                .addClass('roadmap-project-header')
                .text(group.project.title)
                .css('top', (currentRow * 90) + 'px');
            
            $timeline.append($projectHeader);
            currentRow++;

            // Render jobs for this project
            group.jobs.forEach(function(job) {
                var $jobElement = createJobElement(job, DAY_WIDTH, timelineStart); // Pass DAY_WIDTH here
                positionJobElement($jobElement, job, timelineStart, DAY_WIDTH, currentRow * 90);
                $timeline.append($jobElement);
                currentRow++;
            });

            // Add spacing between projects
            currentRow++;
        });

        _$roadmapContainer.append($timeline);
        initializeDraggable(DAY_WIDTH, timelineStart);
    }

    function getQuarterStart(date) {
        const quarterMonth = Math.floor(date.getMonth() / 3) * 3;
        return new Date(date.getFullYear(), quarterMonth, 1);
    }

    function getQuarterEnd(date) {
        const quarterMonth = Math.floor(date.getMonth() / 3) * 3 + 2;
        const endDate = new Date(date.getFullYear(), quarterMonth + 1, 0);
        return endDate;
    }

    function positionJobElement($element, job, timelineStart, dayWidth, topOffset) {
        var jobStart = job.startDate ? new Date(job.startDate) : new Date(timelineStart);
        var jobEnd = job.dueDate ? new Date(job.dueDate) : new Date(jobStart);

        if (jobStart.getTime() === jobEnd.getTime()) {
            jobEnd = new Date(jobStart);
            jobEnd.setDate(jobEnd.getDate() + 1); // Minimum 1 day duration
        }

        // Calculate exact position and width based on days
        var daysFromStart = Math.max(0, (jobStart - timelineStart) / (1000 * 60 * 60 * 24));
        var durationInDays = Math.ceil((jobEnd - jobStart) / (1000 * 60 * 60 * 24));
        
        // Calculate position ensuring dates align with actual calendar
        var leftPosition = Math.floor(daysFromStart * dayWidth);
        var itemWidth = Math.ceil(durationInDays * dayWidth);

        $element.css({
            left: leftPosition + 'px',
            width: itemWidth + 'px',
            top: topOffset + 'px'
        });

        // Store exact dates
        $element.data('job-data', {
            ...job,
            exactStartDate: jobStart,
            exactEndDate: jobEnd
        });
    }

    function addTimelineGridAndScale($timeline, timelineStart, timelineEnd, quarterWidth) {
        var intervals = getQuarterIntervals(timelineStart, timelineEnd);
        
        // Add grid lines
        var $timelineGrid = $('<div>').addClass('timeline-grid');
        intervals.forEach(function(interval, index) {
            $timelineGrid.append(
                $('<div>')
                    .addClass('timeline-grid-line')
                    .css('left', (index * quarterWidth) + 'px')
            );
        });
        $timeline.append($timelineGrid);

        // Add quarter labels
        var $timelineScale = $('<div>').addClass('timeline-scale');
        intervals.forEach(function(interval, index) {
            $timelineScale.append(
                $('<div>')
                    .addClass('timeline-scale-marker')
                    .css('left', (index * quarterWidth) + 'px')
                    .text(interval.label)
            );
        });
        $timeline.append($timelineScale);
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
                const $element = $(this);
                const jobId = $element.attr('data-job-id');
                const job = $element.data('job-data');
                
                // Calculate new dates
                const daysOffset = Math.round(ui.position.left / dayWidth);
                const newStartDate = new Date(timelineStart);
                newStartDate.setDate(newStartDate.getDate() + daysOffset);
                
                // Calculate end date based on job's current duration or default to one quarter
                let newEndDate;
                if (job.startDate && job.dueDate) {
                    const originalDuration = new Date(job.dueDate) - new Date(job.startDate);
                    newEndDate = new Date(newStartDate.getTime() + originalDuration);
                } else {
                    newEndDate = new Date(newStartDate);
                    newEndDate.setMonth(newEndDate.getMonth() + 3); // Default to one quarter duration
                }
                
                updateJobDates(jobId, newStartDate, newEndDate);
            }
        });
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

    function createJobElement(job, dayWidth, timelineStart) {
        console.log('Creating element for job:', job); // Debug log

        var $jobElement = $('<div>')
            .addClass('roadmap-item')
            .addClass(job.level === 2 ? 'roadmap-epic' : 'roadmap-task')
            .attr('data-job-id', job.id) // Add this line to set the attribute
            .data('job-data', job);

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

        // Add resize handles
        $jobElement.append(
            $('<div>').addClass('roadmap-resize-handle-left'),
            $('<div>').addClass('roadmap-resize-handle-right')
        );

        // Create tooltip element
        var $tooltip = $('<div>')
            .addClass('resize-tooltip')
            .hide()
            .appendTo('body');

        // Initialize resizable
        $jobElement.resizable({
            handles: {
                e: '.roadmap-resize-handle-right',
                w: '.roadmap-resize-handle-left'
            },
            grid: [dayWidth, 0],
            containment: 'parent',
            start: function(event, ui) {
                $(this).addClass('ui-resizable-resizing');
            },
            resize: function(event, ui) {
                const position = ui.position.left;
                const width = ui.size.width;
                
                // Calculate exact dates based on days
                const daysFromStart = Math.round(position / dayWidth);
                const durationDays = Math.round(width / dayWidth);
                
                const startDate = new Date(timelineStart);
                startDate.setDate(startDate.getDate() + daysFromStart);
                
                const endDate = new Date(timelineStart);
                endDate.setDate(endDate.getDate() + daysFromStart + durationDays);
                
                // Format dates for tooltip
                const tooltipContent = `${formatDate(startDate)} - ${formatDate(endDate)}`;
                $tooltip
                    .text(tooltipContent)
                    .css({
                        top: ui.position.top - 25,
                        left: ui.position.left + (ui.size.width / 2) - ($tooltip.width() / 2)
                    })
                    .show();
            },
            stop: function(event, ui) {
                $(this).removeClass('ui-resizable-resizing');
                $tooltip.hide();
                
                const jobId = $(this).attr('data-job-id');
                const position = ui.position.left;
                const width = ui.size.width;
                
                // Calculate exact dates for update
                const daysFromStart = Math.round(position / dayWidth);
                const durationDays = Math.round(width / dayWidth);
                
                const newStartDate = new Date(timelineStart);
                newStartDate.setDate(newStartDate.getDate() + daysFromStart);
                
                const newEndDate = new Date(timelineStart);
                newEndDate.setDate(newEndDate.getDate() + daysFromStart + durationDays);
                
                updateJobDates(jobId, newStartDate, newEndDate);
            }
        });

        return $jobElement;
    }

    function formatDate(date) {
        if (!date) return 'Not set';
        return new Date(date).toLocaleDateString('en-US', {
            year: 'numeric',
            month: 'long',
            day: 'numeric'
        });
    }


    function updateJobDates(jobId, newStartDate, newEndDate) {
        console.log('Updating dates for job:', jobId, newStartDate, newEndDate);
        
        // Use new updateDates method
        _jobService.updateDates(jobId, newStartDate, newEndDate)
            .done(function() {
                abp.notify.success('Job dates updated');
                loadRoadmapData();
            })
            .fail(function(err) {
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
