(function ($) {
    var _jobService = abp.services.app.job,
        l = abp.localization.getSource('todo'),
        _$modal = $('#JobCreateModal'),
        _$form = $('#JobCreateForm'),
        _$table = $('#JobsTable');

    var _$jobsTable = _$table.DataTable({
        paging: true,
        serverSide: true,
        lengthMenu: [ [25, 50, 2147483647], [25, 50, "All"] ],
        listAction: {
            ajaxFunction: abp.services.app.job.getAll,
            inputFilter: function () {
                return {
                    keyword: $('#JobsSearchForm input[type=search]').val(),
                    jobStatus: $('#SelectedJobStatus').val(),
                    projectId: $('#ProjectId').val()                  
                }
            },
            dataFilter: function (data) {
                var json = jQuery.parseJSON(data);
                json.recordsTotal = json.TotalCount;
                json.recordsFiltered = json.TotalCount;
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
            {
                targets: 0,
                data: null,
                defaultContent: '',
                sortable: false,
                width: '1em',
                render: (data, type, row, meta) => {
                    if (row.jobStatus != 0) {
                        return `<i class="fas fa-check-circle fa-2x job-status" data-job-status="${row.jobStatus}" data-job-id="${row.id}" data-toggle="tooltip" data-placement="bottom" title="Send to Backlog"></i>`;
                    } else {
                        return `<i class="far fa-circle fa-2x job-status" data-job-status="${row.jobStatus}" data-job-id="${row.id}" data-toggle="tooltip" data-placement="bottom" title="Mark as complete"></i>`;
                    }
                }
            },
            {
                targets: 1,
                data: 'title',
                className: 'title',
                defaultContent: '',
                sortable: false
            },
            {
                targets: 2,
                data: null,
                sortable: false,
                autoWidth: false,
                defaultContent: '',
                width: '1em',
                render: (data, type, row, meta) => {
                    return [
                        `   <button type="button" class="btn btn-sm bg-secondary edit-job" data-job-id="${row.id}" data-toggle="modal" data-target="#JobEditModal">`,
                        `       <i class="fas fa-pencil-alt"></i>`,
                        '   </button>',
                    ].join('');
                }
            }
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
    $(document).on('click', '.job-status', function (e) {
        var jobId = $(this).attr("data-job-id");
        var currentJobStatus = $(this).attr("data-job-status");
        var newJobStatus = currentJobStatus == 0 ? 2 : 0;
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


    //on hover for job-status, update the icon to a checkmark
    $(document).on('mouseenter', '.job-status', function (e) {
        var jobStatus = $(this).attr("data-job-status");
        if (jobStatus == 0) {
            $(this).removeClass("far fa-circle");
            $(this).addClass("fas fa-check-circle");
        }
    });

    //on hover for job-status, update the icon to a circle
    $(document).on('mouseleave', '.job-status', function (e) {
        var jobStatus = $(this).attr("data-job-status");
        if (jobStatus == 0) {
            $(this).removeClass("fas fa-check-circle");
            $(this).addClass("far fa-circle");
        }
    });

    //handle filtering by job status
    $(document).on('click', '.job-status-filter', function (e) {
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

    abp.event.on('job.edited', (data) => {
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
})(jQuery);
