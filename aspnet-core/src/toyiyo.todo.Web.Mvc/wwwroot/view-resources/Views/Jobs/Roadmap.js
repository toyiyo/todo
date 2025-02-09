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
        
        // Constants for timeline scaling
        const QUARTER_WIDTH = 272; // Width in pixels for each quarter
        const DAYS_PER_QUARTER = 91; // ~91 days per quarter
        const DAY_WIDTH = QUARTER_WIDTH / DAYS_PER_QUARTER; // Calculate day width from quarter width

        // Calculate total timeline width based on quarters
        var totalQuarters = Math.ceil(((timelineEnd - timelineStart) / (1000 * 60 * 60 * 24)) / DAYS_PER_QUARTER);
        var timelineWidth = totalQuarters * QUARTER_WIDTH;

        // Create timeline container with calculated width
        var $timeline = $('<div>')
            .addClass('roadmap-timeline')
            .css('width', timelineWidth + 'px');

        // Add timeline grid and scale
        addTimelineGridAndScale($timeline, timelineStart, timelineEnd, data.viewTypeValue, DAY_WIDTH);

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

    function addTimelineGridAndScale($timeline, timelineStart, timelineEnd, viewType, dayWidth) {
        // Add timeline grid
        var $timelineGrid = $('<div>').addClass('timeline-grid');
        var intervals = getQuarterIntervals(timelineStart, timelineEnd);

        intervals.forEach(function() {
            $timelineGrid.append($('<div>').addClass('timeline-grid-line'));
        });
        $timeline.append($timelineGrid);

        // Add timeline scale
        var $timelineScale = $('<div>').addClass('timeline-scale');
        intervals.forEach(function(interval) {
            $timelineScale.append($('<div>')
                .addClass('timeline-scale-marker')
                .text(interval.label));
        });
        _$roadmapContainer.append($timelineScale);
    }

    function positionJobElement($element, job, timelineStart, dayWidth, topOffset) {
        const QUARTER_WIDTH = 272; // Width in pixels for each quarter
        const DAYS_PER_QUARTER = 91; // ~91 days per quarter
        var jobStart, jobEnd;

        // Handle date calculation
        if (job.startDate && job.dueDate) {
            jobStart = new Date(job.startDate);
            jobEnd = new Date(job.dueDate);
            
            // If same day, set to one quarter duration
            if (jobStart.getTime() === jobEnd.getTime()) {
                jobEnd = new Date(jobStart);
                jobEnd.setMonth(jobEnd.getMonth() + 3);
            }
        } else {
            // Default to timeline start with one quarter duration
            jobStart = new Date(timelineStart);
            jobEnd = new Date(timelineStart);
            jobEnd.setMonth(jobEnd.getMonth() + 3);
        }

        // Calculate position
        var daysFromStart = Math.max(0, Math.ceil((jobStart - timelineStart) / (1000 * 60 * 60 * 24)));
        
        // Calculate duration in quarters (rounded up)
        var durationInDays = Math.ceil((jobEnd - jobStart) / (1000 * 60 * 60 * 24));
        var quarters = Math.ceil(durationInDays / DAYS_PER_QUARTER);
        
        // Set width to exact number of quarters
        var width = quarters * QUARTER_WIDTH;
        
        // Position the element
        $element.css({
            left: (daysFromStart * dayWidth) + 'px',
            width: width + 'px',
            top: topOffset + 'px'
        });

        // Visual indicators for incomplete dates
        $element
            .toggleClass('no-start-date', !job.startDate)
            .toggleClass('no-end-date', !job.dueDate)
            .toggleClass('no-dates', !job.startDate && !job.dueDate);

        // Store the job data
        $element.data('job-data', job);

        // Debug info
        console.log('Positioned job:', {
            title: job.title,
            start: jobStart,
            end: jobEnd,
            left: daysFromStart * dayWidth,
            width: width
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
            stop: function(event, ui) {
                $(this).removeClass('ui-resizable-resizing');
                const jobId = $(this).attr('data-job-id');
                const job = $(this).data('job-data');
                
                // Calculate new dates based on resize
                const daysFromStart = Math.round(ui.position.left / dayWidth);
                const widthInDays = Math.round(ui.size.width / dayWidth);
                
                // Calculate new start and end dates
                const newStartDate = new Date(timelineStart);
                newStartDate.setDate(newStartDate.getDate() + daysFromStart);
                
                const newEndDate = new Date(timelineStart);
                newEndDate.setDate(newEndDate.getDate() + daysFromStart + widthInDays);

                // Update the job dates
                updateJobDates(jobId, newStartDate, newEndDate);
            }
        });

        return $jobElement;
    }

    function formatDate(dateString) {
        if (!dateString) return 'Not set';
        return new Date(dateString).toLocaleDateString();
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
