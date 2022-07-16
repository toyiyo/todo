(function ($) {
    var _jobService = abp.services.app.job,
        l = abp.localization.getSource('todo'),
        _$modal = $('#JobEditModal'),
        _$form = _$modal.find('form');

    function save() {
        if (!_$form.valid()) {
            return;
        }

        var job = _$form.serializeFormToObject();

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
