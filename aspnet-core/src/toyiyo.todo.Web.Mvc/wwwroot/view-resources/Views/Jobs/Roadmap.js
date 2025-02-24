(function ($) {
  if (!$.fn.draggable) {
    console.error("jQuery UI draggable not loaded!");
    return;
  }

  let _jobService = abp.services.app.job;
  let _$roadmapContainer = $("#roadmapContainer");
  let _$startDate = $("#startDate");
  let _$endDate = $("#endDate");

  function initialize() {
    let today = new Date();
    _$startDate.val(today.toISOString().split("T")[0]);
    today.setFullYear(today.getFullYear() + 1);
    _$endDate.val(today.toISOString().split("T")[0]);

    loadRoadmapData();

    _$startDate.change(loadRoadmapData);
    _$endDate.change(loadRoadmapData);
  }

  function loadRoadmapData() {
    let startDate = new Date(_$startDate.val());
    let endDate = new Date(_$endDate.val());

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

    let timelineStart = getQuarterStart(new Date(data.startDate));
    let timelineEnd = getQuarterEnd(new Date(data.endDate));
    let dayWidth = 10;
    let totalDays = Math.ceil((timelineEnd - timelineStart) / (1000 * 60 * 60 * 24));
    let timelineWidth = Math.ceil(totalDays * dayWidth);

    let $timelineScaleContainer = $("<div>")
      .addClass("roadmap-timeline-scale-container")
      .css("width", timelineWidth + "px");

    let $timeline = $("<div>")
      .addClass("roadmap-timeline")
      .css("width", timelineWidth + "px");

    addTimelineGridAndScale($timelineScaleContainer, timelineStart, timelineEnd, dayWidth);
    _$roadmapContainer.append($timelineScaleContainer);

    let projectGroups = groupJobsByProject(data.jobs);
    let currentRow = 0;

    Object.values(projectGroups).forEach(function (group) {
      let $projectHeader = createProjectHeader(group.project, currentRow);
      $timeline.append($projectHeader);
      currentRow++;

      group.jobs.forEach(function (job) {
        let $jobElement = createJobElement(job, dayWidth, timelineStart);
        positionJobElement($jobElement, job, timelineStart, dayWidth, currentRow * 90);
        $timeline.append($jobElement);
        currentRow++;
      });

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
    return new Date(date.getFullYear(), quarterMonth + 1, 0);
  }

  function groupJobsByProject(jobs) {
    let projectGroups = {};
    jobs.forEach(function (job) {
      let projectId = job.project.id;
      if (!projectGroups[projectId]) {
        projectGroups[projectId] = {
          project: job.project,
          jobs: [],
        };
      }
      projectGroups[projectId].jobs.push(job);
    });
    return projectGroups;
  }

  function createProjectHeader(project, row) {
    let $projectHeader = $("<div>")
      .addClass("roadmap-project-header")
      .text(project.title)
      .css("top", row * 90 + "px");

    let $collapseButton = $("<button>")
      .addClass("roadmap-collapse-button")
      .text("-")
      .on("click", function () {
        let $button = $(this);
        let $projectJobs = $(`.roadmap-item[data-project-id="${project.id}"]`);

        if ($button.text() === "-") {
          $button.text("+");
          $projectJobs.hide();
        } else {
          $button.text("-");
          $projectJobs.show();
        }
      });

    $projectHeader.append($collapseButton);
    return $projectHeader;
  }

  function positionJobElement($element, job, timelineStart, dayWidth, topOffset) {
    let { leftPosition, itemWidth } = calculatePositionAndWidth(job, timelineStart, dayWidth);

    $element.css({
      left: leftPosition + "px",
      width: itemWidth + "px",
      top: topOffset + "px",
    });

    $element.data("job-data", {
      jobId: job.id,
      level: job.level,
      title: job.title,
      jobStatus: job.jobStatus,
      exactStartDate: new Date(job.startDate),
      exactEndDate: new Date(job.dueDate),
    });
  }

  function calculatePositionAndWidth(job, timelineStart, dayWidth) {
    let jobStart = job.startDate ? new Date(job.startDate) : new Date(timelineStart);
    let jobEnd = job.dueDate ? new Date(job.dueDate) : new Date(jobStart);

    if (jobStart.getTime() === jobEnd.getTime()) {
      jobEnd.setDate(jobEnd.getDate() + 1);
    }

    let daysFromStart = (jobStart - timelineStart) / (1000 * 60 * 60 * 24);
    let durationInDays = (jobEnd - jobStart) / (1000 * 60 * 60 * 24);

    let leftPosition = Math.round(daysFromStart * dayWidth);
    let itemWidth = Math.round(durationInDays * dayWidth);

    return { leftPosition, itemWidth };
  }

  function addTimelineGridAndScale($timeline, timelineStart, timelineEnd, dayWidth) {
    let intervals = getMonthIntervals(timelineStart, timelineEnd);

    let $timelineGrid = $("<div>").addClass("timeline-grid");
    intervals.forEach(function (interval) {
      let daysFromStart = (interval.date - timelineStart) / (1000 * 60 * 60 * 24);
      let leftPos = Math.round(daysFromStart * dayWidth);
      $timelineGrid.append(
        $("<div>")
          .addClass("timeline-grid-line")
          .css("left", leftPos + "px")
      );
    });
    $timeline.append($timelineGrid);

    let $timelineScale = $("<div>").addClass("timeline-scale");
    intervals.forEach(function (interval) {
      let daysFromStart = (interval.date - timelineStart) / (1000 * 60 * 60 * 24);
      let leftPos = Math.round(daysFromStart * dayWidth);
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
        showTooltip($(this), ui.position.left, dayWidth, timelineStart);
      },
      drag: function (event, ui) {
        updateTooltip($(this), ui.position.left, dayWidth, timelineStart);
      },
      stop: function (event, ui) {
        $(this).removeClass("ui-draggable-dragging");
        hideTooltip($(this));
        updateJobDatesOnDragStop($(this), ui.position.left, dayWidth, timelineStart);
      },
    });
  }

  function showTooltip($element, leftPosition, dayWidth, timelineStart) {
    const job = $element.data("job-data");
    const $tooltip = $element.find('.resize-tooltip');
    const { newStartDate, newEndDate } = calculateNewDates(leftPosition, dayWidth, timelineStart, job);

    const tooltipContent = `${formatDate(newStartDate)} - ${formatDate(newEndDate)}`;
    $tooltip
      .text(tooltipContent)
      .css({
        top: 0,
        left: leftPosition + 10,
      })
      .show();
  }

  function updateTooltip($element, leftPosition, dayWidth, timelineStart) {
    const job = $element.data("job-data");
    const $tooltip = $element.find('.resize-tooltip');
    const { newStartDate, newEndDate } = calculateNewDates(leftPosition, dayWidth, timelineStart, job);

    const tooltipContent = `${formatDate(newStartDate)} - ${formatDate(newEndDate)}`;
    $tooltip
      .text(tooltipContent)
      .css({
        top: 0,
        left: 10,
      });
  }

  function hideTooltip($element) {
    const $tooltip = $element.find('.resize-tooltip');
    $tooltip.hide();
  }

  function calculateNewDates(leftPosition, dayWidth, timelineStart, job) {
    // Calculate new start date based on position
    const daysOffset = Math.round(leftPosition / dayWidth);
    const newStartDate = new Date(timelineStart);
    newStartDate.setDate(newStartDate.getDate() + daysOffset);

    // Initialize end date
    let newEndDate;
    
    if (job.exactStartDate && job.exactEndDate && 
        !isNaN(job.exactStartDate.getTime()) && !isNaN(job.exactEndDate.getTime())) {
        // If we have both valid dates, maintain the original duration
        const originalDuration = job.exactEndDate.getTime() - job.exactStartDate.getTime();
        newEndDate = new Date(newStartDate.getTime() + originalDuration);
    } else {
        // For items with no dates or partial dates, set end date to 1 month after start
        newEndDate = new Date(newStartDate);
        newEndDate.setMonth(newStartDate.getMonth() + 1);
    }

    // Ensure dates are in the current year or later
    const currentYear = new Date().getFullYear();
    if (newStartDate.getFullYear() < currentYear) {
        newStartDate.setFullYear(currentYear);
    }
    if (newEndDate.getFullYear() < currentYear) {
        newEndDate.setFullYear(currentYear);
    }

    // Ensure end date is always after start date by at least one month
    if (newEndDate <= newStartDate || 
        (newEndDate.getTime() - newStartDate.getTime()) < (30 * 24 * 60 * 60 * 1000)) {
        newEndDate = new Date(newStartDate);
        newEndDate.setMonth(newStartDate.getMonth() + 1);
    }

    return { newStartDate, newEndDate };
  }

  function updateJobDatesOnDragStop($element, leftPosition, dayWidth, timelineStart) {
    const jobId = $element.attr("data-job-id");
    const job = $element.data("job-data");
    const { newStartDate, newEndDate } = calculateNewDates(leftPosition, dayWidth, timelineStart, job);

    updateJobDates(jobId, newStartDate, newEndDate);
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
    let $jobElement = $("<div>")
      .addClass("roadmap-item")
      .addClass(job.level === 2 ? "roadmap-epic" : "roadmap-task")
      .attr("data-job-id", job.id)
      .data("job-data", job);

    let $header = $("<div>").addClass("roadmap-item-header").text(job.title);
    let $dates = $("<div>").addClass("roadmap-item-dates");

    if (!job.startDate) {
      $dates.append(
        $('<i class="fas fa-exclamation-triangle text-warning mr-1" title="No start date set"></i>')
      );
    }

    $dates.append(formatDate(job.startDate) + " - " + formatDate(job.dueDate));
    $jobElement.append($header).append($dates);

    $jobElement.append($("<div>").addClass("roadmap-resize-handle-right"));

    let $tooltip = $('<div>')
      .addClass('resize-tooltip')
      .hide()
      .appendTo($jobElement);

    initializeResizable($jobElement, dayWidth, timelineStart, $tooltip);

    return $jobElement;
  }

  function initializeResizable($jobElement, dayWidth, timelineStart, $tooltip) {
    $jobElement.resizable({
      handles: {
        e: '.roadmap-resize-handle-right',
      },
      grid: [dayWidth, 0],
      containment: 'parent',
      start: function () {
        $(this).addClass("ui-resizable-resizing");
      },
      resize: function (event, ui) {
        const jobData = $jobElement.data('job-data');
        const originalStartDate = jobData.exactStartDate;
        const width = ui.size.width;
        const durationDays = Math.round(width / dayWidth);
        const newEndDate = new Date(originalStartDate);
        newEndDate.setDate(originalStartDate.getDate() + durationDays);

        const tooltipContent = `${formatDate(originalStartDate)} - ${formatDate(newEndDate)}`;
        $tooltip
          .text(tooltipContent)
          .css({
            top: 0,
            left: width + 10,
          })
          .show();
      },
      stop: function (event, ui) {
        $(this).removeClass("ui-resizable-resizing");
        $tooltip.hide();

        const jobId = $(this).attr("data-job-id");
        const jobData = $jobElement.data('job-data');
        const originalStartDate = jobData.exactStartDate;
        const width = ui.size.width;
        const durationDays = Math.round(width / dayWidth);
        const newEndDate = new Date(originalStartDate);
        newEndDate.setDate(originalStartDate.getDate() + durationDays);

        updateJobDates(jobId, originalStartDate, newEndDate);
      },
    });
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
    _jobService
      .updateDates(jobId, newStartDate, newEndDate)
      .done(function () {
        refreshJobDateDisplay(jobId, newStartDate, newEndDate);
      })
      .fail(function (err) {
        abp.notify.error("Failed to update job dates");
        console.error(err);
        loadRoadmapData();
      });
  }

  function refreshJobDateDisplay(jobId, newStartDate, newEndDate) {
    abp.notify.success("Job dates updated");

    let $jobElement = $(`.roadmap-item[data-job-id="${jobId}"]`);

    if ($jobElement.length) {
      let jobData = $jobElement.data("job-data");
      jobData.exactStartDate = newStartDate;
      jobData.exactEndDate = newEndDate;
      $jobElement.data("job-data", jobData);

      let formattedStartDate = formatDate(newStartDate);
      let formattedEndDate = formatDate(newEndDate);
      $jobElement
        .find(".roadmap-item-dates")
        .text(formattedStartDate + " - " + formattedEndDate);
    }
  }

  $(document).ready(function () {
    initialize();
  });
})(jQuery);
