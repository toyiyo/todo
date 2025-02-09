(function ($) {
  // Add this check at the beginning of your script
  if (!$.fn.draggable) {
    console.error("jQuery UI draggable not loaded!");
    return;
  }

  var _jobService = abp.services.app.job;
  var _$roadmapContainer = $("#roadmapContainer");
  var _$startDate = $("#startDate");
  var _$endDate = $("#endDate");

  function initialize() {
    // Set default date range (1 year from now)
    var today = new Date();
    _$startDate.val(today.toISOString().split("T")[0]);
    today.setFullYear(today.getFullYear() + 1);
    _$endDate.val(today.toISOString().split("T")[0]);

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
    _jobService
      .getRoadmapView(startDate, endDate)
      .done(function (result) {
        renderRoadmap(result);
      })
      .fail(function (err) {
        abp.notify.error("Failed to load roadmap data");
        console.error(err);
      })
      .always(function () {
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

    // Use dayWidth to calculate exact positions
    var dayWidth = 10; // Increased dayWidth

    // Calculate total quarters and timeline width
    var totalDays = Math.ceil(
      (timelineEnd - timelineStart) / (1000 * 60 * 60 * 24)
    );
    var timelineWidth = Math.ceil(totalDays * dayWidth);

    // Create timeline scale container
    var $timelineScaleContainer = $("<div>")
      .addClass("roadmap-timeline-scale-container")
      .css("width", timelineWidth + "px");

    // Create timeline container with calculated width
    var $timeline = $("<div>")
      .addClass("roadmap-timeline")
      .css("width", timelineWidth + "px");

    // Add timeline grid and scale
    addTimelineGridAndScale(
      $timelineScaleContainer,
      timelineStart,
      timelineEnd,
      dayWidth
    );

    // Append timeline scale container to roadmap container
    _$roadmapContainer.append($timelineScaleContainer);

    // Group jobs by project
    var projectGroups = {};
    data.jobs.forEach(function (job) {
      var projectId = job.project.id;
      if (!projectGroups[projectId]) {
        projectGroups[projectId] = {
          project: job.project,
          jobs: [],
        };
      }
      projectGroups[projectId].jobs.push(job);
    });

    // Track vertical position
    var currentRow = 0;

    // Render projects and their jobs
    Object.values(projectGroups).forEach(function (group) {
      // Add project header
      var $projectHeader = $("<div>")
        .addClass("roadmap-project-header")
        .text(group.project.title)
        .css("top", currentRow * 90 + "px");

      $timeline.append($projectHeader);
      currentRow++;

      // Render jobs for this project
      group.jobs.forEach(function (job) {
        var $jobElement = createJobElement(job, dayWidth, timelineStart); // Pass dayWidth here
        positionJobElement(
          $jobElement,
          job,
          timelineStart,
          dayWidth,
          currentRow * 90
        );
        $timeline.append($jobElement);
        currentRow++;
      });

      // Add spacing between projects
      currentRow++;
    });

    _$roadmapContainer.append($timeline);
    initializeDraggable(dayWidth, timelineStart);
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

  function positionJobElement(
    $element,
    job,
    timelineStart,
    dayWidth,
    topOffset
  ) {
    var jobStart = job.startDate
      ? new Date(job.startDate)
      : new Date(timelineStart);
    var jobEnd = job.dueDate ? new Date(job.dueDate) : new Date(jobStart);

    if (jobStart.getTime() === jobEnd.getTime()) {
      jobEnd = new Date(jobStart);
      jobEnd.setDate(jobEnd.getDate() + 1); // Minimum 1 day duration
    }

    // Calculate exact position and width based on days
    var daysFromStart = (jobStart - timelineStart) / (1000 * 60 * 60 * 24);
    var durationInDays = (jobEnd - jobStart) / (1000 * 60 * 60 * 24);

    // Calculate position ensuring dates align with actual calendar
    var leftPosition = Math.round(daysFromStart * dayWidth);
    var itemWidth = Math.round(durationInDays * dayWidth);

    $element.css({
      left: leftPosition + "px",
      width: itemWidth + "px",
      top: topOffset + "px",
    });

    // Store exact dates
    $element.data("job-data", {
      jobId: job.id,
      level: job.level,
      title: job.title,
      jobStatus: job.jobStatus,
      exactStartDate: jobStart,
      exactEndDate: jobEnd,
    });
  }

  function addTimelineGridAndScale(
    $timeline,
    timelineStart,
    timelineEnd,
    dayWidth
  ) {
    var intervals = getMonthIntervals(timelineStart, timelineEnd);

    // Add grid lines
    var $timelineGrid = $("<div>").addClass("timeline-grid");
    intervals.forEach(function (interval, index) {
      var daysFromStart =
        (interval.date - timelineStart) / (1000 * 60 * 60 * 24);
      var leftPos = Math.round(daysFromStart * dayWidth);
      $timelineGrid.append(
        $("<div>")
          .addClass("timeline-grid-line")
          .css("left", leftPos + "px")
      );
    });
    $timeline.append($timelineGrid);

    // Add month labels
    var $timelineScale = $("<div>").addClass("timeline-scale");
    intervals.forEach(function (interval, index) {
      var daysFromStart =
        (interval.date - timelineStart) / (1000 * 60 * 60 * 24);
      var leftPos = Math.round(daysFromStart * dayWidth);
      $timelineScale.append(
        $("<div>")
          .addClass("timeline-scale-marker")
          .css("left", leftPos + "px")
          .text(interval.label)
      );
    });
    $timeline.append($timelineScale);
  }

  function initializeDraggable(dayWidth, timelineStart) {
    $(".roadmap-item").draggable({
      axis: "x",
      containment: "parent",
      grid: [dayWidth, 0],
      start: function (event, ui) {
        $(this).addClass("ui-draggable-dragging");
      },
      stop: function (event, ui) {
        $(this).removeClass("ui-draggable-dragging");
        const $element = $(this);
        const jobId = $element.attr("data-job-id");
        const job = $element.data("job-data");

        // Calculate new dates
        const daysOffset = Math.round(ui.position.left / dayWidth);
        const newStartDate = new Date(timelineStart);
        newStartDate.setDate(newStartDate.getDate() + daysOffset);

        // Calculate end date based on job's current duration
        let newEndDate;
        if (job.exactStartDate && job.exactEndDate) {
          const originalDuration =
            job.exactEndDate.getTime() - job.exactStartDate.getTime();
          newEndDate = new Date(newStartDate.getTime() + originalDuration);
        } else {
          newEndDate = new Date(newStartDate);
          newEndDate.setMonth(newEndDate.getMonth() + 3); // Default to one quarter duration
        }

        updateJobDates(jobId, newStartDate, newEndDate);
      },
    });
  }

  function getMonthIntervals(start, end) {
    const intervals = [];
    let current = new Date(start);

    while (current <= end) {
      intervals.push({
        date: new Date(current),
        label: current.toLocaleDateString('en-US', {
          month: 'short',
          year: 'numeric'
        })
      });
      current.setMonth(current.getMonth() + 1);
    }

    return intervals;
  }

  function createJobElement(job, dayWidth, timelineStart) {
    var $jobElement = $("<div>")
      .addClass("roadmap-item")
      .addClass(job.level === 2 ? "roadmap-epic" : "roadmap-task")
      .attr("data-job-id", job.id) // Add this line to set the attribute
      .data("job-data", job);

    var $header = $("<div>").addClass("roadmap-item-header").text(job.title);

    var $dates = $("<div>").addClass("roadmap-item-dates");

    if (!job.startDate) {
      $dates.append(
        $(
          '<i class="fas fa-exclamation-triangle text-warning mr-1" title="No start date set"></i>'
        )
      );
    }

    $dates.append(formatDate(job.startDate) + " - " + formatDate(job.dueDate));

    var $status = $("<div>")
      .addClass("roadmap-item-status")
      .addClass("status-" + job.jobStatus)
      .text(job.jobStatus);

    $jobElement.append($header).append($dates).append($status);

    // Add resize handles
    $jobElement.append(
      $("<div>").addClass("roadmap-resize-handle-right")
    );

    // Create tooltip element
    var $tooltip = $('<div>')
      .addClass('resize-tooltip')
      .hide()
      .appendTo($jobElement); // Append to $jobElement

    // Initialize resizable
    $jobElement.resizable({
      handles: {
        e: '.roadmap-resize-handle-right', // Only right handle
      },
      grid: [dayWidth, 0],
      containment: 'parent',
      start: function (event, ui) {
        $(this).addClass("ui-resizable-resizing");
      },
      resize: function (event, ui) {
        const $element = $(this);
        const jobData = $element.data('job-data');
        const originalStartDate = jobData.exactStartDate;

        const width = ui.size.width;

        // Calculate exact dates based on days
        const durationDays = Math.round(width / dayWidth);

        const newEndDate = new Date(originalStartDate);
        newEndDate.setDate(originalStartDate.getDate() + durationDays);

        // Format dates for tooltip
        const tooltipContent = `${formatDate(originalStartDate)} - ${formatDate(newEndDate)}`;
        $tooltip
            .text(tooltipContent)
            .css({
                top: 0, // Position at the top of the roadmap-item
                left: width + 10, // Position to the right of the handle
            })
            .show();
      },
      stop: function (event, ui) {
        const $element = $(this);
        $(this).removeClass("ui-resizable-resizing");
        $tooltip.hide();

        const jobId = $(this).attr("data-job-id");
        const jobData = $element.data('job-data');
        const originalStartDate = jobData.exactStartDate;

        const width = ui.size.width;

        // Calculate exact dates for update
        const durationDays = Math.round(width / dayWidth);

        const newEndDate = new Date(originalStartDate);
        newEndDate.setDate(originalStartDate.getDate() + durationDays);

        updateJobDates(jobId, originalStartDate, newEndDate);
      },
    });

    return $jobElement;
  }

  function formatDate(date) {
    if (!date) return "Not set";
    return new Date(date).toLocaleDateString("en-US", {
      year: "numeric",
      month: "long",
      day: "numeric",
    });
  }

  function updateJobDates(jobId, newStartDate, newEndDate) {
    // Use new updateDates method
    _jobService
      .updateDates(jobId, newStartDate, newEndDate)
      .done(function () {
        refreshJobDateDisplay();
      })
      .fail(function (err) {
        abp.notify.error("Failed to update job dates");
        console.error(err);
        loadRoadmapData();
      });

    function refreshJobDateDisplay() {
      abp.notify.success("Job dates updated");

      // Find the corresponding epic element
      var $jobElement = $(`.roadmap-item[data-job-id="${jobId}"]`);

      if ($jobElement.length) {
        // Update the data-job-data attribute
        var jobData = $jobElement.data("job-data");
        jobData.exactStartDate = newStartDate;
        jobData.exactEndDate = newEndDate;
        $jobElement.data("job-data", jobData);

        // Update the displayed dates
        var formattedStartDate = formatDate(newStartDate);
        var formattedEndDate = formatDate(newEndDate);
        $jobElement
          .find(".roadmap-item-dates")
          .text(formattedStartDate + " - " + formattedEndDate);
      }
    }
  }

  // Initialize when document is ready
  $(document).ready(function () {
    initialize();
  });
})(jQuery);
