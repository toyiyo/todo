(function ($) {
    var _jobService = abp.services.app.job,
        l = abp.localization.getSource('todo'),
        _$modal = $('#JobCreateModal'),
        _$form = $('#JobCreateForm'),
        _$table = $('#JobsTable');
        
        const backlogFavicon = 'far fa-circle';
        const inProgressFavicon = 'fa fa-spinner';
        const doneFavicon = 'far fa-check-circle';

        const getStatusDropdown =  function(jobStatus, id, favicon){
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
        paging: true,
        serverSide: true,
        //lengthMenu: [ [25, 50, 2147483647], [25, 50, "All"] ],
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
            {
                targets: 0,
                data: null,
                defaultContent: '',
                sortable: false,
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
})(jQuery);
