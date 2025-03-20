(function ($) {
    var _projectService = abp.services.app.project,
        l = abp.localization.getSource('todo'),
        _$modal = $('#ProjectCreateModal'),
        _$deleteModal = $('#ProjectDeleteModal'),
        _$form = _$modal.find('form'),
        _$table = $('#ProjectsTable');

    var _$projectsTable = _$table.DataTable({
        paging: true,
        serverSide: true,
        listAction: {
            ajaxFunction: abp.services.app.project.getAll,
            inputFilter: function () {
                return {
                    keyword: $('#ProjectsSearchForm input[type=search]').val()
                }
            },
            dataFilter: function (data) {
                var json = jQuery.parseJSON(data);
                json.recordsTotal = json.totalCount;
                json.recordsFiltered = json.items.length;
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
                className: 'project-card',
                render: function(data, type, row) {
                    const progress = row.progress;
                    const actions = [
                        `<button type="button" class="btn btn-sm bg-secondary edit-project" data-project-id="${row.id}" data-toggle="modal" data-target="#ProjectEditModal">`,
                        `   <i class="fas fa-pencil-alt" title=${l('Edit')}></i>`,
                        `</button>`,
                        `<button type="button" class="btn btn-sm bg-danger delete-project" data-project-id="${row.id}" data-toggle="modal" data-target="#ProjectDeleteModal">`,
                        `   <i class="fas fa-trash-alt" title=${l('Delete')}></i>`,
                        `</button>`
                    ].join('');

                    return `
                        <div class="card">
                            <div class="card-body">
                                <div class="d-flex justify-content-between align-items-center">
                                    <div class="h4 mb-0">
                                        <a href="/Projects/${row.id}/jobs">${row.title}</a>
                                    </div>
                                    <div>
                                        <span class="badge ${progress.statusClass}">${progress.status}</span>
                                        <span class="ml-2">${actions}</span>
                                    </div>
                                </div>
                                <div class="text-muted small mt-2">
                                    <div class="d-flex gap-4">
                                        <span class="d-inline-flex align-items-center">
                                            <i class="fas fa-layer-group mr-1"></i>
                                            ${progress.epicCount} Epics (${progress.completedEpics} done)
                                        </span>
                                        <span class="d-inline-flex align-items-center">
                                            <i class="fas fa-tasks mr-1"></i>
                                            ${progress.taskCount} Tasks 
                                        </span>
                                        <span class="d-inline-flex align-items-center">
                                            <i class="fas fa-bug mr-1"></i>
                                            ${progress.bugCount} Bugs
                                        </span>
                                        ${progress.dueDate ? `
                                            <span class="d-inline-flex align-items-center">
                                                <i class="fas fa-calendar mr-1"></i>
                                                Due: ${moment(progress.dueDate).format('M/D/YYYY')}
                                            </span>
                                        ` : ''}
                                    </div>
                                </div>
                            </div>
                            <div class="card-footer p-2">
                                <div class="d-flex align-items-center gap-2">
                                    <div class="progress flex-grow-1" style="height: 8px;">
                                        <div class="progress-bar bg-primary" 
                                             style="width: ${progress.totalTasksPercentage}%"
                                             role="progressbar" 
                                             aria-valuenow="${progress.totalTasksPercentage}" 
                                             aria-valuemin="0" 
                                             aria-valuemax="100">
                                        </div>
                                    </div>
                                    <span class="small font-weight-bold">${Math.round(progress.totalTasksPercentage)}%</span>
                                </div>
                            </div>
                        </div>
                    `;
                }
            },
            {
                targets: 1,
                data: 'progress',
                width: '300px',
                render: function(data) {
                    const total = data.totalTasks;
                    if (total === 0) return '<small class="text-muted">No tasks</small>';
                    
                    const completed = data.completedTasks;
                    const inProgress = data.inProgressTasks;
                    const completedPct = (completed / total * 100).toFixed(0);
                    const inProgressPct = (inProgress / total * 100).toFixed(0);
                    
                    return `
                        <div class="progress" style="height: 20px;">
                            <div class="progress-bar bg-success" 
                                 role="progressbar" 
                                 style="width: ${completedPct}%"
                                 data-toggle="tooltip" 
                                 title="${completed} completed">
                                ${completed}
                            </div>
                            <div class="progress-bar bg-info" 
                                 role="progressbar" 
                                 style="width: ${inProgressPct}%"
                                 data-toggle="tooltip" 
                                 title="${inProgress} in progress">
                                ${inProgress}
                            </div>
                        </div>
                        <small class="text-muted">${completed} of ${total} tasks completed</small>
                    `;
                }
            },
            {
                targets: 2,
                data: null,
                sortable: false,
                autoWidth: false,
                width: '5em',
                defaultContent: '',
                render: (data, type, row, meta) => {
                    return [
                        `   <button type="button" class="btn btn-sm bg-secondary edit-project" data-project-id="${row.id}" data-toggle="modal" data-target="#ProjectEditModal">`,
                        `       <i class="fas fa-pencil-alt" title=${l('Edit')}></i>`,
                        '   </button>',
                        `   <button type="button" class="btn btn-sm bg-danger delete-project" data-project-id="${row.id}" data-toggle="modal" data-target="#ProjectDeleteModal">`,
                        `       <i class="fas fa-trash-alt" title=${l('Delete')}></i>`,
                        '   </button>',
                    ].join('');
                }
            }
        ]
    });

    _$projectsTable.on('draw.dt', function() {
        $('[data-toggle="tooltip"]').tooltip();
    });

    _$form.find('.save-button').on('click', (e) => {
        e.preventDefault();

        if (!_$form.valid()) {
            return;
        }

        var project = _$form.serializeFormToObject();

        abp.ui.setBusy(_$modal);
        _projectService
            .create(project)
            .done(function () {
                _$modal.modal('hide');
                _$form[0].reset();
                abp.notify.info(l('Saved Successfully'));
                _$projectsTable.ajax.reload();
            })
            .always(function () {
                abp.ui.clearBusy(_$modal);
            });
    });


    $(document).on('click', '.edit-project', function (e) {
        var projectId = $(this).attr("data-project-id");

        e.preventDefault();
        abp.ajax({
            url: abp.appPath + 'Projects/EditModal?projectId=' + projectId,
            type: 'POST',
            dataType: 'html',
            success: function (content) {
                $('#ProjectEditModal div.modal-content').html(content);
            }
        })
    });

    abp.event.on('project.edited', (data) => {
        _$projectsTable.ajax.reload();
    });

    _$modal.on('shown.bs.modal', () => {
        _$modal.find('input:not([type=hidden]):first').focus();
    }).on('hidden.bs.modal', () => {
        _$form.clearForm();
    });

    $('.btn-search').on('click', (e) => {
        _$projectsTable.ajax.reload();
    });

    $('.txt-search').on('keypress', (e) => {
        if (e.which == 13) {
            _$projectsTable.ajax.reload();
            return false;
        }
    });

    //triggered when the delete modal - sets the project id to be deleted
    _$deleteModal.on('show.bs.modal', function (e) {

        //get data-id attribute of the clicked element
        let projectId = $(e.relatedTarget).attr('data-project-id');

        //populate the textbox
        $(e.currentTarget).find('input[name="ProjectId"]').val(projectId);
    });

    _$deleteModal.on('click', '.delete-button', function (e) {
        let projectId = $(this).closest('div.modal-content').find('input[name="ProjectId"]').val();

        e.preventDefault();

        abp.ui.setBusy(_$deleteModal);

        _projectService
            .delete(projectId)
            .done(function () {
                abp.notify.info(l('Deleted Successfully'));
                _$projectsTable.ajax.reload();
            })
            .always(function () {
                abp.ui.clearBusy(_$deleteModal);
            });
    });

})(jQuery);
