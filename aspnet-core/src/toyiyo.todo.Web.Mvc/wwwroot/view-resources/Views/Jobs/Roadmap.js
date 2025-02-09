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
        var timelineStart = new Date(data.startDate);
        var timelineEnd = new Date(data.endDate);
        var totalDays = Math.ceil((timelineEnd - timelineStart) / (1000 * 60 * 60 * 24));
        var dayWidth = 25;
        var timelineWidth = Math.max(totalDays * dayWidth, _$roadmapContainer.width());

        // Create timeline container
        var $timeline = $('<div>')
            .addClass('roadmap-timeline')
            .css('width', timelineWidth + 'px');

        // Add timeline grid and scale
        addTimelineGridAndScale($timeline, timelineStart, timelineEnd, data.viewTypeValue, dayWidth);

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
                var $jobElement = createJobElement(job);
                positionJobElement($jobElement, job, timelineStart, dayWidth, currentRow * 90);
                $timeline.append($jobElement);
                currentRow++;
            });

            // Add spacing between projects
            currentRow++;
        });

        _$roadmapContainer.append($timeline);
        initializeDraggable(dayWidth, timelineStart);
    }

    function addTimelineGridAndScale($timeline, timelineStart, timelineEnd, viewType, dayWidth) {
        // Add timeline grid
        var $timelineGrid = $('<div>').addClass('timeline-grid');
        var intervals = viewType === 'Monthly' ? 
            getMonthIntervals(timelineStart, timelineEnd) :
            getQuarterIntervals(timelineStart, timelineEnd);

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
        var jobStart, jobEnd;
        
        // Handle date calculation
        if (job.startDate && job.dueDate) {
            // Both dates set
            jobStart = new Date(job.startDate);
            jobEnd = new Date(job.dueDate);
        } else if (job.startDate) {
            // Only start date - assume 1 month duration
            jobStart = new Date(job.startDate);
            jobEnd = new Date(jobStart);
            jobEnd.setMonth(jobEnd.getMonth() + 1);
        } else if (job.dueDate) {
            // Only end date - position 1 month before
            jobEnd = new Date(job.dueDate);
            jobStart = new Date(jobEnd);
            jobStart.setMonth(jobStart.getMonth() - 1);
        } else {
            // No dates set - position at timeline start
            jobStart = new Date(timelineStart);
            jobEnd = new Date(timelineStart);
            jobEnd.setMonth(jobEnd.getMonth() + 1);
        }

        // Ensure dates are valid
        if (jobStart == 'Invalid Date' || jobEnd == 'Invalid Date') {
            jobStart = new Date(timelineStart);
            jobEnd = new Date(timelineStart);
            jobEnd.setMonth(jobEnd.getMonth() + 1);
            console.warn('Invalid dates for job:', job.title, 'Using default dates');
        }

        // Calculate position
        var daysFromStart = Math.max(0, Math.ceil((jobStart - timelineStart) / (1000 * 60 * 60 * 24)));
        var jobDuration = Math.max(30, Math.ceil((jobEnd - jobStart) / (1000 * 60 * 60 * 24)));
        
        // Position the element
        $element.css({
            left: (daysFromStart * dayWidth) + 'px',
            width: Math.max((jobDuration * dayWidth), 200) + 'px',
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
            width: jobDuration * dayWidth
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
        console.log('Updating dates for job:', jobId);
        var $element = _$roadmapContainer.find('div[data-job-id="' + jobId + '"]');
        var job = $element.data('job-data');
        
        if (!job) {
            console.error('Job data not found for id:', jobId);
            return;
        }

        // Calculate new due date
        var originalStartDate = new Date(job.startDate || job.dueDate);
        var originalDueDate = new Date(job.dueDate);
        var originalDuration = originalDueDate - originalStartDate;
        var newDueDate = new Date(newStartDate.getTime() + originalDuration);

        // Use new updateDates method
        _jobService.updateDates(jobId, newStartDate, newDueDate)
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
