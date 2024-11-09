(function ($) {
    var _jobService = abp.services.app.job,
        l = abp.localization.getSource('todo'),
        _$JobCreateModal = $('#JobCreateModal'),
        _$JobEditModal = $('#JobEditModal'),
        _$deleteModal = $('#JobDeleteModal'),
        _$form = $('#JobCreateForm'),
        _$table = $('#JobsTable');

    const backlogFavicon = `<svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" class="bi bi-dash-circle-dotted" viewBox="0 0 16 16">
                                <path d="M8 0c-.176 0-.35.006-.523.017l.064.998a7.117 7.117 0 0 1 .918 0l.064-.998A8.113 8.113 0 0 0 8 0zM6.44.152c-.346.069-.684.16-1.012.27l.321.948c.287-.098.582-.177.884-.237L6.44.153zm4.132.271a7.946 7.946 0 0 0-1.011-.27l-.194.98c.302.06.597.14.884.237l.321-.947zm1.873.925a8 8 0 0 0-.906-.524l-.443.896c.275.136.54.29.793.459l.556-.831zM4.46.824c-.314.155-.616.33-.905.524l.556.83a7.07 7.07 0 0 1 .793-.458L4.46.824zM2.725 1.985c-.262.23-.51.478-.74.74l.752.66c.202-.23.418-.446.648-.648l-.66-.752zm11.29.74a8.058 8.058 0 0 0-.74-.74l-.66.752c.23.202.447.418.648.648l.752-.66zm1.161 1.735a7.98 7.98 0 0 0-.524-.905l-.83.556c.169.253.322.518.458.793l.896-.443zM1.348 3.555c-.194.289-.37.591-.524.906l.896.443c.136-.275.29-.54.459-.793l-.831-.556zM.423 5.428a7.945 7.945 0 0 0-.27 1.011l.98.194c.06-.302.14-.597.237-.884l-.947-.321zM15.848 6.44a7.943 7.943 0 0 0-.27-1.012l-.948.321c.098.287.177.582.237.884l.98-.194zM.017 7.477a8.113 8.113 0 0 0 0 1.046l.998-.064a7.117 7.117 0 0 1 0-.918l-.998-.064zM16 8a8.1 8.1 0 0 0-.017-.523l-.998.064a7.11 7.11 0 0 1 0 .918l.998.064A8.1 8.1 0 0 0 16 8zM.152 9.56c.069.346.16.684.27 1.012l.948-.321a6.944 6.944 0 0 1-.237-.884l-.98.194zm15.425 1.012c.112-.328.202-.666.27-1.011l-.98-.194c-.06.302-.14.597-.237.884l.947.321zM.824 11.54a8 8 0 0 0 .524.905l.83-.556a6.999 6.999 0 0 1-.458-.793l-.896.443zm13.828.905c.194-.289.37-.591.524-.906l-.896-.443c-.136.275-.29.54-.459.793l.831.556zm-12.667.83c.23.262.478.51.74.74l.66-.752a7.047 7.047 0 0 1-.648-.648l-.752.66zm11.29.74c.262-.23.51-.478.74-.74l-.752-.66c-.201.23-.418.447-.648.648l.66.752zm-1.735 1.161c.314-.155.616-.33.905-.524l-.556-.83a7.07 7.07 0 0 1-.793.458l.443.896zm-7.985-.524c.289.194.591.37.906.524l.443-.896a6.998 6.998 0 0 1-.793-.459l-.556.831zm1.873.925c.328.112.666.202 1.011.27l.194-.98a6.953 6.953 0 0 1-.884-.237l-.321.947zm4.132.271a7.944 7.944 0 0 0 1.012-.27l-.321-.948a6.954 6.954 0 0 1-.884.237l.194.98zm-2.083.135a8.1 8.1 0 0 0 1.046 0l-.064-.998a7.11 7.11 0 0 1-.918 0l-.064.998zM4.5 7.5a.5.5 0 0 0 0 1h7a.5.5 0 0 0 0-1h-7z"/>
                            </svg>`
    const inProgressFavicon = `<svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" class="bi bi-arrow-clockwise" viewBox="0 0 16 16">
                                    <path fill-rule="evenodd" d="M8 3a5 5 0 1 0 4.546 2.914.5.5 0 0 1 .908-.417A6 6 0 1 1 8 2v1z"/>
                                    <path d="M8 4.466V.534a.25.25 0 0 1 .41-.192l2.36 1.966c.12.1.12.284 0 .384L8.41 4.658A.25.25 0 0 1 8 4.466z"/>
                                </svg>`
    const doneFavicon = `<svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" class="bi bi-check2-circle" viewBox="0 0 16 16">
                            <path d="M2.5 8a5.5 5.5 0 0 1 8.25-4.764.5.5 0 0 0 .5-.866A6.5 6.5 0 1 0 14.5 8a.5.5 0 0 0-1 0 5.5 5.5 0 1 1-11 0z"/>
                            <path d="M15.354 3.354a.5.5 0 0 0-.708-.708L8 9.293 5.354 6.646a.5.5 0 1 0-.708.708l3 3a.5.5 0 0 0 .708 0l7-7z"/>
                        </svg>`;

    const epicFavicon = `<svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" class="bi bi-window-stack" viewBox="0 0 16 16">
                            <path d="M4.5 6a.5.5 0 1 0 0-1 .5.5 0 0 0 0 1ZM6 6a.5.5 0 1 0 0-1 .5.5 0 0 0 0 1Zm2-.5a.5.5 0 1 1-1 0 .5.5 0 0 1 1 0Z"/>
                            <path d="M12 1a2 2 0 0 1 2 2 2 2 0 0 1 2 2v8a2 2 0 0 1-2 2H4a2 2 0 0 1-2-2 2 2 0 0 1-2-2V3a2 2 0 0 1 2-2h10ZM2 12V5a2 2 0 0 1 2-2h9a1 1 0 0 0-1-1H2a1 1 0 0 0-1 1v8a1 1 0 0 0 1 1Zm1-4v5a1 1 0 0 0 1 1h10a1 1 0 0 0 1-1V8H3Zm12-1V5a1 1 0 0 0-1-1H4a1 1 0 0 0-1 1v2h12Z"/>
                        </svg>`;

    const storyFavicon = `<svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" class="bi bi-window" viewBox="0 0 16 16">
                            <path d="M2.5 4a.5.5 0 1 0 0-1 .5.5 0 0 0 0 1zm2-.5a.5.5 0 1 1-1 0 .5.5 0 0 1 1 0zm1 .5a.5.5 0 1 0 0-1 .5.5 0 0 0 0 1z"/>
                            <path d="M2 1a2 2 0 0 0-2 2v10a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V3a2 2 0 0 0-2-2H2zm13 2v2H1V3a1 1 0 0 1 1-1h12a1 1 0 0 1 1 1zM2 14a1 1 0 0 1-1-1V6h14v7a1 1 0 0 1-1 1H2z"/>
                        </svg>`;

    const subTaskFavicon = `<svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" class="bi bi-window-split" viewBox="0 0 16 16">
                            <path d="M2.5 4a.5.5 0 1 0 0-1 .5.5 0 0 0 0 1Zm2-.5a.5.5 0 1 1-1 0 .5.5 0 0 1 1 0Zm1 .5a.5.5 0 1 0 0-1 .5.5 0 0 0 0 1Z"/>
                            <path d="M2 1a2 2 0 0 0-2 2v10a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V3a2 2 0 0 0-2-2H2Zm12 1a1 1 0 0 1 1 1v2H1V3a1 1 0 0 1 1-1h12ZM1 13V6h6.5v8H2a1 1 0 0 1-1-1Zm7.5 1V6H15v7a1 1 0 0 1-1 1H8.5Z"/>
                        </svg>`;

    const getStatusDropdown = function (jobStatus, id, favicon) {
        return `<div class="dropdown show">
        <div class="dropdown-toggle" href="#" role="button" id="dropdownMenuLink" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false" data-job-id="${id}" data-job-status="${jobStatus}">
            ${favicon} 
        </div>
      
        <div class="dropdown-menu" aria-labelledby="dropdownMenuLink">
            <a class="dropdown-item job-status-selector" selected-job-status=0 href="#">${backlogFavicon} Backlog</a>
            <a class="dropdown-item job-status-selector" selected-job-status=1 href="#">${inProgressFavicon} In Progress </a>
            <a class="dropdown-item job-status-selector" selected-job-status=2 href="#">${doneFavicon} Done</a>
        </div>
      </div>`
    }

    const getStatusButton = function (jobStatus, jobId) {
        if (jobStatus === 2) {
            return getStatusDropdown(jobStatus, jobId, doneFavicon);
        } else if (jobStatus === 1) {
            return getStatusDropdown(jobStatus, jobId, inProgressFavicon);
        } else if (jobStatus === 0) {
            return getStatusDropdown(jobStatus, jobId, backlogFavicon);
        }
    }

    var _$jobsTable = _$table.DataTable({
        rowReorder: {
            dataSrc: 'orderByDate',
            update: false
        },
        responsive: {
            details: {
                type: 'column',
                target: 2
            }
        },
        paging: true,
        serverSide: true,
        select: true,
        listAction: {
            ajaxFunction: _jobService.getAll,
            inputFilter: function () {
                return {
                    keyword: $('#JobsSearchForm input[type=search]').val(),
                    jobStatus: $('#SelectedJobStatus').val(),
                    projectId: $('#ProjectId').val(),
                    level: 0,
                    //include parentJobId only if the value of ParentJobId is not null
                    parentJobId: $('#SelectedEpicId').val(),
                    sorting: 'OrderByDate DESC',
                }
            },
            dataFilter: function (data) {
                var json = jQuery.parseJSON(data);
                json.recordsTotal = json.TotalCount;
                json.recordsFiltered = json.Items.length;
                json.data = json.list;
                return JSON.stringify(json);
            }
        },
        buttons: [],
        columnDefs: [
            // { className: 'dtr-control', orderable: false, targets: 0 },
            { orderable: true, className: 'reorder', targets: 0 },
            { orderable: false, targets: '_all' },
            {
                targets: 0,
                data: 'lastModificationTime',
                width: '1em',
                className: 'reorder',
                render: (data, type, row, meta) => {
                    return [
                        `<div data-order-datetime="${row.lastModificationTime}">`,
                        `<svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" class="bi bi-grip-vertical" viewBox="0 0 16 16">`,
                        `<path d="M7 2a1 1 0 1 1-2 0 1 1 0 0 1 2 0zm3 0a1 1 0 1 1-2 0 1 1 0 0 1 2 0zM7 5a1 1 0 1 1-2 0 1 1 0 0 1 2 0zm3 0a1 1 0 1 1-2 0 1 1 0 0 1 2 0zM7 8a1 1 0 1 1-2 0 1 1 0 0 1 2 0zm3 0a1 1 0 1 1-2 0 1 1 0 0 1 2 0zm-3 3a1 1 0 1 1-2 0 1 1 0 0 1 2 0zm3 0a1 1 0 1 1-2 0 1 1 0 0 1 2 0zm-3 3a1 1 0 1 1-2 0 1 1 0 0 1 2 0zm3 0a1 1 0 1 1-2 0 1 1 0 0 1 2 0z"/>`,
                        `</svg>`,
                        `</div>`,
                    ].join('');
                }
            },
            {
                targets: 1,
                data: null,
                defaultContent: '',
                width: '1em',
                render: (data, type, row, meta) => {
                    return getStatusButton(row.jobStatus, row.id);
                }
            },
            {
                targets: 2,
                data: 'title',
                className: 'title',
                defaultContent: '',
            },
            {
                targets: 3,
                data: 'dueDate',
                defaultContent: '',
                width: '8em',
                render: (data, type, row, meta) => {
                    const friendlyDueOnDate = moment(row.dueDate).fromNow();
                    if (moment(row.dueDate).year() > 2000) { return `<span title="due ${friendlyDueOnDate}">${friendlyDueOnDate}</span>` }
                    else { return `` }
                }
            },
            {
                targets: 4,
                data: null,
                autoWidth: false,
                defaultContent: '',
                width: '1em',
                render: (data, type, row, meta) => {
                    return [
                        `<button type="button" class="btn btn-sm bg-default edit-job" data-job-id="${row.id}" data-job-status="${row.jobStatus}" data-toggle="modal" data-target="#JobEditModal">
                            <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" class="bi bi-pencil" viewBox="0 0 16 16">
                                 <path d="M12.146.146a.5.5 0 0 1 .708 0l3 3a.5.5 0 0 1 0 .708l-10 10a.5.5 0 0 1-.168.11l-5 2a.5.5 0 0 1-.65-.65l2-5a.5.5 0 0 1 .11-.168l10-10zM11.207 2.5 13.5 4.793 14.793 3.5 12.5 1.207 11.207 2.5zm1.586 3L10.5 3.207 4 9.707V10h.5a.5.5 0 0 1 .5.5v.5h.5a.5.5 0 0 1 .5.5v.5h.293l6.5-6.5zm-9.761 5.175-.106.106-1.528 3.821 3.821-1.528.106-.106A.5.5 0 0 1 5 12.5V12h-.5a.5.5 0 0 1-.5-.5V11h-.5a.5.5 0 0 1-.468-.325z"/>
                             </svg>
                         </button>`,
                    ].join('');
                }
            },
        ]
    });

    //Initializers
    $(window).on('load', function () {
        const jobId = $('#JobId').val();
        //if we have a job id, we are loading the job details, let's show the user the modal
        if (jobId) {
            loadJobDetailsModal(jobId);
        }

        //due date initializer - do not allow dates in the past
        document.getElementById("due-date-button").setAttribute("min", new Date().toJSON().split('T')[0]);
        //filter button content
        document.getElementById('backlog-icon').innerHTML = backlogFavicon;
        document.getElementById('in-progress-icon').innerHTML = inProgressFavicon;
        document.getElementById('done-icon').innerHTML = doneFavicon;

    });


    //handle filtering by job status
    $(document).on('click', '.job-status-filter', function (_e) {
        $('#SelectedJobStatus').val($(this).attr('data-job-status-filter'));
        _$jobsTable.ajax.reload();
    });

    //Job creation
    _$form.submit((e) => {
        e.preventDefault();

        if (!_$form.valid()) {
            return;
        }

        var job = _$form.serializeFormToObject();
        job.projectId = $('#ProjectId').val();
        job.dueDate = moment($(".due-date-button").val()).endOf('day').utc();
        job.parentId = $('#SelectedEpicId').val();

        abp.ui.setBusy(_$JobCreateModal);
        _jobService
            .create(job)
            .done(function () {
                _$JobCreateModal.modal('hide');
                _$form[0].reset();
                abp.notify.info(l('SavedSuccessfully'));
                _$jobsTable.ajax.reload();
            })
            .always(function () {
                abp.ui.clearBusy(_$JobCreateModal);
            });
    });

    _$JobCreateModal.on('shown.bs.modal', () => {
        _$JobCreateModal.find('input:not([type=hidden]):first').focus();
    }).on('hidden.bs.modal', () => {
        _$form.clearForm();
    });

    $('.btn-search').on('click', (e) => {
        _$jobsTable.ajax.reload();
    });

    $('.txt-search').on('keypress', (e) => {
        if (e.which == 13) {
            _$jobsTable.ajax.reload();
            return false;
        }
    });

    //Job status handler
    $(document).on('click', '.job-status-selector', function (e) {
        //re-think this selector
        const jobId = $(this).parent().parent().find("div.dropdown-toggle").attr("data-job-id");
        const newJobStatus = $(this).attr("selected-job-status");
        const jobStatusPaneButtonHtml = getStatusButton(+newJobStatus, jobId);
        //set the job status button html
        $('#JobEditModal button.btn-pane-template').html(jobStatusPaneButtonHtml);

        const JobSetStatusInputDto = {
            id: jobId,
            jobStatus: newJobStatus
        };

        e.preventDefault();
        _jobService
            .setJobStatus(JobSetStatusInputDto)
            .done(function () {
                abp.notify.info(l('SavedSuccessfully'));
                abp.event.trigger('job.edited', JobSetStatusInputDto);
            })
            .always(function () {
                abp.ui.clearBusy(_$JobCreateModal);
            });
    });

    //job edit handler
    $(document).on('click', '.edit-job', function (e) {
        const jobId = $(this).attr("data-job-id");
        const jobStatus = $(this).attr("data-job-status");
        const projectId = $('#ProjectId').val();

        e.preventDefault();
        loadJobDetailsModal(jobId, jobStatus);

        //set the URL in the browser's history
        const nextUrl = '/projects/' + projectId + '/jobs/' + jobId
        const nextTitle = 'job details for ' + jobId;
        const nextState = { additionalInformation: 'job details for ' + jobId };
        // This will create a new entry in the browser's history, without reloading
        window.history.pushState(nextState, nextTitle, nextUrl);

    });
    const loadJobDetailsModal = function (jobId, jobStatus) {
        abp.ajax({
            url: abp.appPath + 'Jobs/EditModal?jobId=' + jobId,
            type: 'POST',
            dataType: 'html',
            success: function (content) {

                //set content from ajax call into modal
                $('#JobEditModal div.modal-content').html(content);
                //set the job status button html - check for a job status, if one is not available, we need to get it from the hidden field as we just loaded from server instead of from js.
                if (!jobStatus) { jobStatus = $('#JobEditModal').find('input[name="jobStatus"]').val(); }
                const jobStatusPaneButtonHtml = getStatusButton(+jobStatus, jobId);
                $('#JobEditModal button.btn-pane-template').html(jobStatusPaneButtonHtml);
                //show the modal
                _$JobEditModal.modal('show');
            },
            error: function (e) {
                _$JobEditModal.modal('hide');
            }
        });
    };

    abp.event.on('job.edited', (_data) => {
        _$jobsTable.ajax.reload();
    });

    //job reordering 
    const getOrderByDate = function (updatesArray, reorderedRow) {
        var rowMoved = updatesArray.filter(item => item.oldData === reorderedRow.triggerRow.data()["orderByDate"])[0]

        if (rowMoved != null) {
            var moveDirection = (rowMoved.oldPosition - rowMoved.newPosition) > 0 ? "newer" : "older"
            var movedIntoOrderDate = rowMoved.newData;

            if (moveDirection === "newer") {
                var newerOrderByDate = new Date(movedIntoOrderDate)
                var milisecondsAdded = newerOrderByDate.getMilliseconds() + 1;
                newerOrderByDate.setMilliseconds(milisecondsAdded);
                return newerOrderByDate
            } else {
                var olderOrderByDate = new Date(movedIntoOrderDate)
                var milisecondsReduced = olderOrderByDate.getMilliseconds() - 1;
                olderOrderByDate.setMilliseconds(milisecondsReduced);
                return olderOrderByDate
            }
        }
    };

    //handle re-order event by saving the orderby date
    _$jobsTable.on('row-reorder', function (e, diff, edit) {
        const jobId = edit.triggerRow.data()["id"];
        const orderByDate = getOrderByDate(diff, edit);

        //if no newOrderByDate date is found, it means we moved the row in place and no change is necessary
        if (orderByDate != null) {
            const jobPatchOrderByDateInputDto = { id: jobId, orderByDate: orderByDate };

            e.preventDefault();

            _jobService.patchOrderByDate(jobPatchOrderByDateInputDto)
                .done(function () { })
                .always(function () {
                    abp.ui.clearBusy(_$JobCreateModal);
                });

            //call function to update the parent id to that of the new parent fromt he rowGroup
            //updateParentId(jobId, diff); - no longer calling on re-order, will create left panel to drag items into.
        }
    });

    // Epics Panel
    $('#toggle-epics').on('click', function () {
        $('#epics-panel').toggleClass('d-none');
        var tableDiv = $('.table-responsive');
        var $toggleIcon = $(this).find('i'); // find the icon within the button
        if (tableDiv.hasClass('col-12')) {
            tableDiv.removeClass('col-12').addClass('col-md-9');
            loadEpics(null, 0, $('#ProjectId').val(), 2);
            $toggleIcon.removeClass('fa-chevron-right').addClass('fa-chevron-left'); // change the icon
        } else {
            tableDiv.removeClass('col-md-9').addClass('col-12');
            $('#SelectedEpicId').val('00000000-0000-0000-0000-000000000000');
            _$jobsTable.ajax.reload();
            $toggleIcon.removeClass('fa-chevron-left').addClass('fa-chevron-right'); // change the icon back
        }
    });

    $(document).on('click', '.epic-filter', function (_e) {
        var $parent = $(this).parent(); // Get the parent element
    
        // If the epic is already active, clear the selection
        if ($parent.hasClass('selected')) {
            $parent.removeClass('selected active');
            $('#SelectedEpicId').val('00000000-0000-0000-0000-000000000000');
        } else {
            // If the epic is not active, select it
            // First, remove 'selected' and 'active' classes from all items
            $('.epic-filter').parent().removeClass('selected active');
            // Then, add 'selected' and 'active' classes to the clicked item's parent
            $parent.addClass('selected active');
            $('#SelectedEpicId').val($(this).attr('data-epic-id-filter'));
        }
        _$jobsTable.ajax.reload();
    });

    const pencilIconSvg = '<svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" class="bi bi-pencil" viewBox="0 0 16 16">' +
        '<path d="M12.146.146a.5.5 0 0 1 .708 0l3 3a.5.5 0 0 1 0 .708l-10 10a.5.5 0 0 1-.168.11l-5 2a.5.5 0 0 1-.65-.65l2-5a.5.5 0 0 1 .11-.168l10-10zM11.207 2.5 13.5 4.793 14.793 3.5 12.5 1.207 11.207 2.5zm1.586 3L10.5 3.207 4 9.707V10h.5a.5.5 0 0 1 .5.5v.5h.5a.5.5 0 0 1 .5.5v.5h.293l6.5-6.5zm-9.761 5.175-.106.106-1.528 3.821 3.821-1.528.106-.106A.5.5 0 0 1 5 12.5V12h-.5a.5.5 0 0 1-.5-.5V11h-.5a.5.5 0 0 1-.468-.325z"></path>' +
        '</svg>';

    const createListItem = function (item) {
        return '<div class="epic-filter d-flex align-items-center list-group-item list-group-item-action cursor-pointer" ' +
            'draggable="true" ' +
            'id="list-' + item.id + '-list" ' +
            'data-epic-id-filter="' + item.id + '" ' +
            'data-toggle="pill" ' +
            'role="tab" ' +
            'aria-controls="list-' + item.id + '">' + 
            '<div class="flex-grow-1">' + 
                item.title + 
            '</div>' +
            '<button type="button" class="btn btn-sm bg-default edit-job" data-job-id="' + item.id + '" data-job-status="0" data-toggle="modal" data-target="#JobEditModal">' +
            pencilIconSvg +
            '</button></div>';
    };

    const addEpicRow = function (item) {
        $('#list-tab-epics').append(createListItem(item));
    };

    const loadEpics = function (keyword, jobStatus, projectId, level) {
        abp.services.app.job.getAll({
            keyword: keyword,
            jobStatus: jobStatus, // only showing active epics
            projectId: projectId,
            level: level, // Set level to 1
            sorting: 'OrderByDate DESC',
        }).done(function (data) {
            // Clear the contents of #list-tab-epics
            $('#list-tab-epics').empty();
            // Add the filtered data to the tablist
            if (data.items && Array.isArray(data.items)) {
                data.items.forEach(function (item) { addEpicRow(item) });
            } else {
                //no eipc found
                $('#list-tab-epics').append('<a class="list-group-item list-group-item-action" id="list-no-epics-list" data-toggle="pill" href="#list-no-epics" role="tab" aria-controls="list-no-epics">No Epics Found</a>');
            }
        }).fail(function (error) {
            console.error('Error:', error);
        });
    };
    const _$createEpicForm = $('#epics-list #AddByTitle');

    function createEpic() {
        const title = _$createEpicForm.find('#add-by-title-input').val();
        const dueDate = _$createEpicForm.find('#due-date-button').val();
        const projectId = $('#ProjectId').val();
        const level = $('#epics-list #level').data('level');

        if (!title) {
            abp.notify.warn(l('TitleIsRequired'));
            return;
        }

        abp.ui.setBusy(_$createEpicForm);

        _jobService.create({
            title: title,
            dueDate: dueDate,
            projectId: projectId,
            level: level //level is passed down via the model initializer to the partial view and set as the level attribute
        }).done(function (data) {
            abp.notify.info(l('SavedSuccessfully'));
            _$createEpicForm.find('#add-by-title-input').val('');
            _$createEpicForm.find('#due-date-button').val('');
            addEpicRow(data);
        }).always(function () {
            abp.ui.clearBusy(_$createEpicForm);
        });
    }

        // Function to get order by date for epics
        const getOrderByDateForEpics = function (updatesArray, reorderedRow) {
            var rowMoved = updatesArray.filter(item => item.oldData === reorderedRow.triggerRow.data()["orderByDate"])[0];
    
            if (rowMoved != null) {
                var moveDirection = (rowMoved.oldPosition - rowMoved.newPosition) > 0 ? "newer" : "older";
                var movedIntoOrderDate = rowMoved.newData;
    
                if (moveDirection === "newer") {
                    var newerOrderByDate = new Date(movedIntoOrderDate);
                    var millisecondsAdded = newerOrderByDate.getMilliseconds() + 1;
                    newerOrderByDate.setMilliseconds(millisecondsAdded);
                    return newerOrderByDate;
                } else {
                    var olderOrderByDate = new Date(movedIntoOrderDate);
                    var millisecondsReduced = olderOrderByDate.getMilliseconds() - 1;
                    olderOrderByDate.setMilliseconds(millisecondsReduced);
                    return olderOrderByDate;
                }
            }
        };
    
        // Handle re-order event by saving the order by date for epics
        $('#list-tab-epics').on('row-reorder', function (e, diff, edit) {
            const epicId = edit.triggerRow.data()["id"];
            const orderByDate = getOrderByDateForEpics(diff, edit);
    
            // If no new order by date is found, it means we moved the row in place and no change is necessary
            if (orderByDate != null) {
                const epicPatchOrderByDateInputDto = { id: epicId, orderByDate: orderByDate };
    
                e.preventDefault();
    
                _epicService.patchOrderByDate(epicPatchOrderByDateInputDto)
                    .done(function () { })
                    .always(function () {
                        abp.ui.clearBusy(_$EpicCreateModal);
                    });
            }
        });
    
        // Initialize sortable for epics table
        $('#list-tab-epics tbody').sortable({
            update: function (event, ui) {
                var epicId = ui.item.data('id');
                var newIndex = ui.item.index();
                reorderEpics(epicId, newIndex);
            }
        });
    

    //create a subtask when the button is clicked
    _$createEpicForm.find('.create-by-title-button').on('click', function () {
        createEpic();
    });
    //create a subtask when the enter key is pressed
    _$createEpicForm.find('#add-by-title-input').on('keydown', function (event) {
        if (event.keyCode === 13) { // Enter key
            event.preventDefault();
            createEpic();
        }
    });
    //Job Deletion handlers
    //triggered when the delete modal - sets the job id to be deleted
    _$deleteModal.on('show.bs.modal', function (e) {

        //get data-id attribute of the clicked element
        let jobId = $(e.relatedTarget).attr('data-job-id');

        //populate the hidden field so it can be used later on post
        $(e.currentTarget).find('input[name="JobId"]').val(jobId);

        //hide the delete modal
        _$JobEditModal.modal("hide");
    });
    //since we hide the modal to show the delete modal, let's bring it back if the user cancels out of the deletion
    _$deleteModal.on('click', '.close-button', function (e) { _$JobEditModal.modal("show"); });
    _$deleteModal.on('click', '.delete-button', function (e) {
        let jobId = $(this).closest('div.modal-content').find('input[name="JobId"]').val();

        e.preventDefault();

        abp.ui.setBusy(_$deleteModal);

        deleteJob(jobId)
            .done(function () {
                abp.notify.info(l('Deleted Successfully'));
                _$jobsTable.ajax.reload();
                loadEpics(null, 0, $('#ProjectId').val(), 2);
            })
            .always(function () {
                abp.ui.clearBusy(_$deleteModal);
            });
    });
    //since our service returns IactionResult (httpresponse), it doesn't get ajax wrapping from apb framework, causing a js error if we use the default _jobService.Delete function since it expects a wrapper object
    //https://aspnetboilerplate.com/Pages/Documents/Javascript-API/AJAX?searchKey=MvcAjaxResponse
    const deleteJob = function (id) {
        return $.ajax({
            url: abp.appPath + 'api/services/app/Job/Delete' + abp.utils.buildQueryString([{ name: 'id', value: id }]) + '',
            type: 'DELETE'
        });
    };

})(jQuery);




