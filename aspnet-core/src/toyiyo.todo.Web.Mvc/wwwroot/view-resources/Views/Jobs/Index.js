(function ($) {
    var _jobService = abp.services.app.job,
        l = abp.localization.getSource('todo'),
        _$modal = $('#JobCreateModal'),
        _$deleteModal = $('#JobDeleteModal'),
        _$form = $('#JobCreateForm'),
        _$table = $('#JobsTable');

    const backlogFavicon = 'far fa-circle';
    const inProgressFavicon = 'fa fa-spinner';
    const doneFavicon = 'far fa-check-circle';

    const getStatusDropdown = function (jobStatus, id, favicon) {
        return `<div class="dropdown show">
        <a class="btn btn-secondary dropdown-toggle" href="#" role="button" id="dropdownMenuLink" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">
            <i class="${favicon} fa-2x job-status" data-job-status="${jobStatus}" data-job-id="${id}"></i>
        </a>
      
        <div class="dropdown-menu" aria-labelledby="dropdownMenuLink">
            <a class="dropdown-item job-status-selector ${backlogFavicon}" selected-job-status=0 href="#"> Backlog</a>
            <a class="dropdown-item job-status-selector ${inProgressFavicon}" selected-job-status=1 href="#"> In progress</a>
            <a class="dropdown-item job-status-selector ${doneFavicon}" selected-job-status=2 href="#"> Done</a>
        </div>
      </div>`
    }

    var _$jobsTable = _$table.DataTable({
        rowReorder: {
            dataSrc: 'orderByDate',
            update: false
        },
        responsive: true,
        paging: true,
        serverSide: true,
        select: true,
        listAction: {
            ajaxFunction: abp.services.app.job.getAll,
            inputFilter: function () {
                return {
                    keyword: $('#JobsSearchForm input[type=search]').val(),
                    jobStatus: $('#SelectedJobStatus').val(),
                    projectId: $('#ProjectId').val(),
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
        responsive: {
            details: {
                type: 'column'
            }
        },
        columnDefs: [
            { orderable: true, className: 'reorder', targets: 0 },
            { orderable: false, targets: '_all' },
            {
                targets: 0,
                data: 'lastModificationTime',
                width: '1em',
                className: 'reorder',
                render: (data, type, row, meta) => {
                    return [
                        `<div data-order-datetime"${row.lastModificationTime}">`,
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
                    if (row.jobStatus === 2) {
                        return getStatusDropdown(row.jobStatus, row.id, doneFavicon);
                    } else if (row.jobStatus === 1) {
                        return getStatusDropdown(row.jobStatus, row.id, inProgressFavicon);
                    } else if (row.jobStatus === 0) {
                        return getStatusDropdown(row.jobStatus, row.id, backlogFavicon);
                    }
                }
            },
            {
                targets: 2,
                data: 'title',
                className: 'title',
                defaultContent: ''
            },
            {
                targets: 3,
                data: null,
                autoWidth: false,
                defaultContent: '',
                width: '5em',
                render: (data, type, row, meta) => {
                    return [
                        `   <button type="button" class="btn btn-sm bg-secondary edit-job" data-job-id="${row.id}" data-toggle="modal" data-target="#JobEditModal">`,
                        `       <i class="fas fa-pencil-alt"></i>`,
                        '   </button>',
                        `   <button type="button" class="btn btn-sm bg-danger delete-job" data-job-id="${row.id}" data-toggle="modal" data-target="#JobDeleteModal">`,
                        `       <i class="fas fa-trash-alt" title=${l('Delete')}></i>`,
                        '   </button>',
                    ].join('');
                }
            },
        ]
    });

    //create job
    _$form.submit((e) => {
        e.preventDefault();

        if (!_$form.valid()) {
            return;
        }

        var job = _$form.serializeFormToObject();
        job.projectId = $('#ProjectId').val();

        abp.ui.setBusy(_$modal);
        _jobService
            .create(job)
            .done(function () {
                _$modal.modal('hide');
                _$form[0].reset();
                abp.notify.info(l('SavedSuccessfully'));
                _$jobsTable.ajax.reload();
            })
            .always(function () {
                abp.ui.clearBusy(_$modal);
            });
    });

    //update job status
    $(document).on('click', '.job-status-selector', function (e) {
        //re-think this selector
        var jobId = $(this).parent().parent().find("i.job-status").attr("data-job-id");
        var newJobStatus = $(this).attr("selected-job-status");

        var JobSetStatusInputDto = {
            id: jobId,
            jobStatus: newJobStatus
        }

        e.preventDefault();
        _jobService
            .setJobStatus(JobSetStatusInputDto)
            .done(function () {
                abp.notify.info(l('SavedSuccessfully'));
                abp.event.trigger('job.edited', JobSetStatusInputDto);
            })
            .always(function () {
                abp.ui.clearBusy(_$modal);
            });
    });

    //handle filtering by job status
    $(document).on('click', '.job-status-filter', function (_e) {
        $('#SelectedJobStatus').val($(this).attr('data-job-status-filter'));
        _$jobsTable.ajax.reload();
    });

    //edit job
    $(document).on('click', '.edit-job', function (e) {
        var jobId = $(this).attr("data-job-id");

        e.preventDefault();
        abp.ajax({
            url: abp.appPath + 'Jobs/EditModal?jobId=' + jobId,
            type: 'POST',
            dataType: 'html',
            success: function (content) {
                $('#JobEditModal div.modal-content').html(content);
            }
        })
    });

    const getOrderByDate = function (diff, edit) {
        const rowMoved = diff.filter(item => item.oldData === edit.triggerRow.data()["orderByDate"])[0]

        if (rowMoved != null) {
            const moveDirection = (rowMoved.oldPosition - rowMoved.newPosition) > 0 ? "newer" : "older"
            const movedIntoOrderDate = rowMoved.newData;

            if (moveDirection === "newer") {
                const newerOrderByDate = new Date(movedIntoOrderDate)
                const miliseconds = newerOrderByDate.getMilliseconds() + 1;
                newerOrderByDate.setMilliseconds(miliseconds);
                return newerOrderByDate
            } else {
                const olderOrderByDate = new Date(movedIntoOrderDate)
                const miliseconds = olderOrderByDate.getMilliseconds() - 1;
                olderOrderByDate.setMilliseconds(miliseconds);
                return olderOrderByDate
            }
        }
    }

    //handle re-order event by saving the orderby date
    _$jobsTable.on('row-reorder', function (e, diff, edit) {
        const jobId = edit.triggerRow.data()["id"];
        const orderByDate = getOrderByDate(diff, edit);

        //if no newOrderByDate date is found, it means we moved the row in place and no change is necessary
        if (orderByDate != null) {
            let jobPatchOrderByDateInputDto = { id: jobId, orderByDate: orderByDate };

            e.preventDefault();

            _jobService.patchOrderByDate(jobPatchOrderByDateInputDto)
                .done(function () { })
                .always(function () {
                    abp.ui.clearBusy(_$modal);
                });
        }
    });

    abp.event.on('job.edited', (_data) => {
        //since we have HTML rather than object data (how do we get the object data?), we need to query the server again and refresh the row
        // _jobService.get(data.id).done(function (result) {
        //     _$table.dataTable().fnUpdate(result, $(`i[data-job-id=${data.id}]`).parents('tr')[0], undefined, false);
        // })
        _$jobsTable.ajax.reload();
    });

    _$modal.on('shown.bs.modal', () => {
        _$modal.find('input:not([type=hidden]):first').focus();
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
    //triggered when the delete modal - sets the job id to be deleted
    _$deleteModal.on('show.bs.modal', function (e) {

        //get data-id attribute of the clicked element
        let jobId = $(e.relatedTarget).attr('data-job-id');

        //populate the textbox
        $(e.currentTarget).find('input[name="JobId"]').val(jobId);
    });

    _$deleteModal.on('click', '.delete-button', function (e) {
        let jobId = $(this).closest('div.modal-content').find('input[name="JobId"]').val();

        e.preventDefault();

        abp.ui.setBusy(_$deleteModal);

        deleteJob(jobId)
            .done(function () {
                abp.notify.info(l('Deleted Successfully'));
                _$jobsTable.ajax.reload();
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
