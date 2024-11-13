(function ($) {

    const Editor = toastui.Editor;
    const _$getLinkButton = $('.btn-pane-get-link');
    var _debounceTimer = null;
    const _$subtaskForm = $('#subtasks #AddByTitle');

    document.getElementById("dueDate").setAttribute("min", new Date().toJSON().split('T')[0]);

    const editor = new Editor({
        el: document.querySelector('#Description'),
        initialValue: $('#descriptionFromServer').val(),
        height: '400px',
        initialEditType: 'wysiwyg',
        previewStyle: 'vertical'
    });

    var _jobService = abp.services.app.job,
        l = abp.localization.getSource('todo'),
        _$modal = $('#JobEditModal'),
        _$form = _$modal.find('form');


    //JOB
    function save() {
        if (!_$form.valid()) {
            return;
        }

        //serialization works for input types, the description is a div so that we can have markdown functionality.  
        //We need to get the div contents and manually add them to the job object
        var job = _$form.serializeFormToObject();
        job.description = editor.getMarkdown();
        job.dueDate = moment(job.dueDate).endOf('day').utc();

        abp.ui.setBusy(_$form);

        _jobService.updateAllFields(job).done(() => {
            _$modal.modal('hide');
            abp.notify.info(l('SavedSuccessfully'));
            abp.event.trigger('job.edited', job);
        }).always(() => abp.ui.clearBusy(_$form));
    }

    _$form.closest('div.modal-content').find(".save-button").click(function (e) {
        e.preventDefault();
        save();
    });

    _$form.find('input').on('keypress', function (e) {
        if (e.which === 13) {
            e.preventDefault();
            save();
        }
    });
    //this is a debounce function that will wait 1 second after the user stops typing to update the description
    $('#Description').on('input', function () {
        if (_debounceTimer) {
            clearTimeout(_debounceTimer);
        }
        _debounceTimer = setTimeout(function () {
            var job = _$form.serializeFormToObject();
            job.description = editor.getMarkdown();
            _jobService.setDescription(job);
        }, 700);
    });

    _$modal.on('shown.bs.modal', function () {
        _$form.find('input[type=text]:first').focus();
    });

    _$modal.on('hidden.bs.modal', function () {
        //when the modal is hidden, change the url history to the listing page
        const projectId = $('#ProjectId').val();
        const nextUrl = '/projects/' + projectId + '/jobs/';
        const nextTitle = 'jobs';
        const nextState = { additionalInformation: 'jobs' };
        // This will create a new entry in the browser's history, without reloading
        window.history.pushState(nextState, nextTitle, nextUrl);
    });

    _$getLinkButton.on('click', function () {
        navigator.clipboard.writeText(window.location.href);
        abp.notify.info('link is in your clipboard');
    });

    //SUBTASKS
    function Subtask(title, dueDate, parentId, id, level) {
        this.title = title;
        this.dueDate = dueDate;
        this.parentId = parentId;
        this.id = id;
        this.level = level;
    }

    function createSubtask() {
        const title = _$subtaskForm.find('#add-by-title-input').val();
        const dueDate = _$subtaskForm.find('#due-date-button').val();
        const parentId = _$form.find('input[name="id"]').val();
        const projectId = $('#ProjectId').val();
        const level = $('#subtasks #level').data('level');

        if (!title) {
            abp.notify.warn(l('TitleIsRequired'));
            return;
        }

        abp.ui.setBusy(_$subtaskForm);

        _jobService.create({
            title: title,
            dueDate: dueDate,
            parentId: parentId,
            projectId: projectId,
            level: level
        }).done(function (data) {
            abp.notify.info(l('SavedSuccessfully'));
            _$subtaskForm.find('#add-by-title-input').val('');
            _$subtaskForm.find('#due-date-button').val('');
            addSubtaskToTable(new Subtask(title, dueDate, parentId, data.id, data.level));
        }).always(function () {
            abp.ui.clearBusy(_$subtaskForm);
        });
    }
    //create a subtask when the button is clicked
    _$subtaskForm.find('.create-by-title-button').on('click', function () {
        createSubtask();
    });
    //create a subtask when the enter key is pressed
    _$subtaskForm.find('#add-by-title-input').on('keydown', function (event) {
        if (event.keyCode === 13) { // Enter key
            event.preventDefault();
            createSubtask();
        }
    });
    //show hide subtask input field
    $('table').on('click', '.subtask-text', function () {
        var $text = $(this);
        var $input = $text.next('.subtask-input');
        $text.addClass('d-none');
        $input.removeClass('d-none');
        $input.focus();
    });

    $('table').on('blur', '.subtask-input', function () {
        var $input = $(this);
        var $text = $input.prev('.subtask-text');
        $input.addClass('d-none');
        $text.removeClass('d-none');
        $text.text($input.val());
    });
    //add subtask to UI when subtask is created
    function addSubtaskToTable(subtask) {
        var $row = $(`
        <tr>
            <td>
                <input type="checkbox" class="subtask-checkbox" data-subtask-id="${subtask.id}" ${subtask.jobStatusId === 2 ? 'checked' : ''} />
            </td> 
            <td class="align-middle w-100">
                <span class="subtask-text form-control border-0 add-todo-input bg-transparent rounded" data-subtask-id="${subtask.id}">${subtask.title}</span>
                <input type="text" class="form-control border-1 add-todo-input bg-transparent rounded subtask-input d-none" value="${subtask.title}" data-subtask-id="${subtask.id}" />
            </td>
            <td>
                <button type="button" class="btn btn-danger btn-sm subtask-delete" data-subtask-id="${subtask.id}">Delete</button>
            </td>
        </tr>
        `);

        $row.appendTo('#subtask-table tbody');
    }
    //set the subtask title when the user stops typing
    $('#subtask-table').on('input', '.subtask-input', function () {
        let $input = $(this);
        if (_debounceTimer) {
            clearTimeout(_debounceTimer);
        }
        _debounceTimer = setTimeout(function () {
            let jobSetSubTaskTitleInputDto = {
                id: $input.data('subtask-id'),
                title: $input.val()
            };
            _jobService.setTitle(jobSetSubTaskTitleInputDto).done(function () { abp.notify.info(l('SavedSuccessfully')); });
        }, 700);
    });

    $('#subtask-table').on('keydown', '.subtask-input', function (event) {
        if (event.keyCode === 13) {
            event.preventDefault();
            let $input = $(this);
            let jobSetSubTaskTitleInputDto = {
                id: $input.data('subtask-id'),
                title: $input.val()
            };
            _jobService.setTitle(jobSetSubTaskTitleInputDto).done(function () { abp.notify.info(l('SavedSuccessfully')); });
        }
    });

    $('#subtask-table').on('change', '.subtask-checkbox', function () {
        let $checkbox = $(this);
        let jobSetSubTaskStatusInputDto = {
            id: $checkbox.data('subtask-id'),
            jobstatus: $checkbox.is(':checked') ? 2 : 0 // Assuming 2 is the ID for "completed" status and 0 is the ID for "open" status
        }

        _jobService.setJobStatus(jobSetSubTaskStatusInputDto).done(function () { abp.notify.info(l('SavedSuccessfully')); });
    });

    $('#subtask-table').on('click', '.subtask-delete', function () {
        let $button = $(this);
        let jobId = $button.data('subtask-id');

        deleteJob(jobId)
            .done(function () {
                abp.notify.info(l('Deleted Successfully'));
                $button.closest('tr').remove();
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
