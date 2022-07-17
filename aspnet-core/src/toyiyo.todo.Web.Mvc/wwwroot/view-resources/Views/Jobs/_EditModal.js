(function ($) {
    
    const Editor = toastui.Editor;
    
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
        //editor.getMarkdown();

        abp.ui.setBusy(_$form);
        //calling multiple services in parallel will fetch data from the DB in parallel.
        //once the first update passes through, the second update overwrites the info from the first update.
        //This is due to the initial load.
        //to fix it, we must update one field at a time, or create a call to update all fields in bulk


        _jobService.setTitle(job).done(
            () => _jobService.setDescription(job).done(
                () =>
                    _$modal.modal('hide'),
                abp.notify.info(l('SavedSuccessfully')),
                abp.event.trigger('job.edited', job)
            )).always(
                () => abp.ui.clearBusy(_$form));
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
})(jQuery);
