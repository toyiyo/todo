(function ($) {

    const Editor = toastui.Editor;
    const _$getLinkButton = $('.btn-pane-get-link');

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

    function save() {
        if (!_$form.valid()) {
            return;
        }

        //serialization works for input types, the description is a div so that we can have markdown functionality.  
        //We need to get the div contents and manually add them to the job object
        var job = _$form.serializeFormToObject();
        job.description = editor.getMarkdown();
        job.dueDate = moment($("dueDate").val()).endOf('day').utc();

        abp.ui.setBusy(_$form);
        //calling multiple services in parallel will fetch data from the DB in parallel.
        //once the first update passes through, the second update overwrites the info from the first update.
        //This is due to the initial load.
        //to fix it, we must update one field at a time, or create a call to update all fields in bulk

        _jobService.setDueDate(job).done(() =>
            _jobService.setTitle(job).done(() =>
                _jobService.setDescription(job).done(
                    () =>
                        _$modal.modal('hide'),
                    abp.notify.info(l('SavedSuccessfully')),
                    abp.event.trigger('job.edited', job)
                )).always(
                    () => abp.ui.clearBusy(_$form)));
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

})(jQuery);
