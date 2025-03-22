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
                    const total = progress.totalJobCount;
                    const completed = progress.completedTasks;
                    const actions = `
                        <div class="dropdown">
                            <button class="btn btn-sm btn-light dropdown-toggle ml-2" type="button" data-toggle="dropdown" aria-expanded="false">
                                <i class="fas fa-ellipsis-v"></i>
                            </button>
                            <div class="dropdown-menu dropdown-menu-right">
                                <button type="button" class="dropdown-item edit-project" data-project-id="${row.id}" data-toggle="modal" data-target="#ProjectEditModal">
                                    <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" class="bi bi-pencil mr-2" viewBox="0 0 16 16">
                                        <path d="M12.146.146a.5.5 0 0 1 .708 0l3 3a.5.5 0 0 1 0 .708l-10 10a.5.5 0 0 1-.168.11l-5 2a.5.5 0 0 1-.65-.65l2-5a.5.5 0 0 1 .11-.168l10-10zM11.207 2.5 13.5 4.793 14.793 3.5 12.5 1.207 11.207 2.5zm1.586 3L10.5 3.207 4 9.707V10h.5a.5.5 0 0 1 .5.5v.5h.5a.5.5 0 0 1 .5.5v.5h.293l6.5-6.5zm-9.761 5.175-.106.106-1.528 3.821 3.821-1.528.106-.106A.5.5 0 0 1 5 12.5V12h-.5a.5.5 0 0 1-.5-.5V11h-.5a.5.5 0 0 1-.468-.325z"/>
                                    </svg>
                                    ${l('Edit')}
                                </button>
                                <button type="button" class="dropdown-item delete-project" data-project-id="${row.id}" data-toggle="modal" data-target="#ProjectDeleteModal">
                                    <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" class="bi bi-trash mr-2" viewBox="0 0 16 16">
                                        <path d="M5.5 5.5A.5.5 0 0 1 6 6v6a.5.5 0 0 1-1 0V6a.5.5 0 0 1 .5-.5zm2.5 0a.5.5 0 0 1 .5.5v6a.5.5 0 0 1-1 0V6a.5.5 0 0 1 .5-.5zm3 .5a.5.5 0 0 0-1 0v6a.5.5 0 0 0 1 0V6z"/>
                                        <path fill-rule="evenodd" d="M14.5 3a1 1 0 0 1-1 1H13v9a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2V4h-.5a1 1 0 0 1-1-1V2a1 1 0 0 1 1-1H6a1 1 0 0 1 1 1h3.5a1 1 0 0 1 1 1v1zM4.118 4 4 4.059V13a1 1 0 0 0 1 1h6a1 1 0 0 0 1-1V4.059L11.882 4H4.118zM2.5 3V2h11v1h-11z"/>
                                    </svg>
                                    ${l('Delete')}
                                </button>
                            </div>
                        </div>
                    `;

                    // Generate card content regardless of task count
                    return `
                        <div class="card">
                            <div class="card-body">
                                <div class="d-flex justify-content-between align-items-center">
                                    <h5 class="card-title mb-0">
                                        <a class="project-title" href="/Projects/${row.id}/jobs">${row.title}</a>
                                    </h5>
                                    <div>
                                        <span class="badge badge-pill ${progress.statusClass}">${progress.status}</span>
                                        <span class="ml-2">${actions}</span>
                                    </div>
                                </div>
                                <div class="text-muted mt-3">
                                    <div class="project-stats">
                                        <span class="stat-item">
                                            <i class="fas fa-layer-group mr-2"></i>
                                            ${progress.epicCount} Epics (${progress.completedEpics} done)
                                        </span>
                                        <span class="stat-item">
                                            <i class="fas fa-tasks mr-2"></i>
                                            ${progress.taskCount} Tasks 
                                        </span>
                                        <span class="stat-item">
                                            <i class="fas fa-bug mr-2"></i>
                                            ${progress.bugCount} Bugs
                                        </span>
                                        ${progress.dueDate && !moment(progress.dueDate).isSame('0001-01-01T00:00:00Z') ? `
                                            <span class="stat-item">
                                                <i class="fas fa-calendar mr-2"></i>
                                                Due: ${moment(progress.dueDate).format('M/D/YYYY')}
                                            </span>
                                        ` : `
                                            <span class="stat-item text-muted">
                                                <i class="fas fa-calendar mr-2"></i>
                                                No due date set
                                            </span>
                                        `}
                                    </div>
                                </div>
                            </div>
                            <div class="card-footer p-2">
                                <div class="progress position-relative" style="height: 20px;">
                                    <div class="progress-bar bg-success" 
                                         style="width: ${progress.totalTasksPercentage}%"
                                         role="progressbar" 
                                         aria-valuenow="${progress.totalTasksPercentage}" 
                                         aria-valuemin="0" 
                                         aria-valuemax="100"
                                         data-toggle="tooltip"
                                         title="${completed} completed (${progress.totalTasksPercentage}%)">
                                    </div>
                                    <div class="position-absolute w-100 text-center" style="line-height: 20px;">
                                        ${total === 0 ? 'No tasks' : `${completed} of ${total} tasks completed (${progress.totalTasksPercentage}%)`}
                                    </div>
                                </div>
                            </div>
                        </div>
                    `;
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

    abp.event.on('project.edited', () => {
        _$projectsTable.ajax.reload();
    });

    _$modal.on('shown.bs.modal', () => {
        _$modal.find('input:not([type=hidden]):first').focus();
    }).on('hidden.bs.modal', () => {
        _$form.clearForm();
    });

    $('.btn-search').on('click', () => {
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
