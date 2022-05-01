(function ($) {
    var _jobService = abp.services.app.job,
        l = abp.localization.getSource('todo'),
        _$modal = $('#JobCreateModal'),
        _$form = $('#JobCreateForm'),
        _$table = $('#JobsTable');

    var _$jobsTable = _$table.DataTable({
        paging: false,
        serverSide: true,
        listAction: {
            ajaxFunction: abp.services.app.job.getAll,
            inputFilter: function () {
                return {
                    keyword: $('#JobsSearchForm input[type=search]').val(),
                    projectId: $('#ProjectId').val()
                }
            },
            dataFilter : function(data){
                var json = jQuery.parseJSON( data );
                json.recordsTotal = json.TotalCount;
                json.recordsFiltered = json.TotalCount;
                json.data = json.list;
                return JSON.stringify( json );
            }
        },
        buttons: [
            {
                name: 'refresh',
                text: '<i class="fas fa-redo-alt"></i>',
                action: () => _$jobsTable.draw(false)
            }
        ],
        responsive: {
            details: {
                type: 'column'
            }
        },
        columnDefs: [
            {
                targets: 0,
                data: 'title',
                className: 'title',
                defaultContent: '',
                sortable: false,
            },
            {
                targets: 1,
                data: null,
                sortable: false,
                autoWidth: false,
                defaultContent: '',
                render: (data, type, row, meta) => {
                    return [
                        `   <button type="button" class="btn btn-sm bg-secondary edit-job" data-job-id="${row.id}" data-toggle="modal" data-target="#JobEditModal">`,
                        `       <i class="fas fa-pencil-alt"></i> ${l('Edit')}`,
                        '   </button>',
                    ].join('');
                }
            }
        ]
    });


    _$form.submit( (e) => {
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
