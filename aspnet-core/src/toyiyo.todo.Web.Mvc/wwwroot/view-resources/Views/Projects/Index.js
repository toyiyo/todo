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
                data: 'title',
                className: 'title',
                defaultContent: '',
                fnCreatedCell: function (nTd, sData, oData, iRow, iCol) {
                    $(nTd).html('<a href="' + 'Projects/' + oData.id + '/jobs' + '">' + sData + '</a>');
                }
            },
            {
                targets: 1,
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
