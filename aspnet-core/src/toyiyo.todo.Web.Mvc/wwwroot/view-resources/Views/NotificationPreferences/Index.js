(function ($) {
    const _notificationPreferenceService = abp.services.app.notificationPreference;

    function initialize() {
        $('.notification-toggle').on('change', function (e) {
            const $checkbox = $(this);
            const type = $checkbox.data('type');
            const channel = $checkbox.data('channel');
            const isEnabled = $checkbox.prop('checked');

            updatePreference(type, channel, isEnabled);
        });
    }

    function updatePreference(type, channel, isEnabled) {
        _notificationPreferenceService.updatePreference({
            notificationType: type,
            channel: channel,
            isEnabled: isEnabled
        }).done(function () {
            abp.notify.success(l('NotificationPreferencesSaved'));
        }).fail(function (error) {
            console.error(error);
            abp.notify.error(l('ErrorSavingPreferences'));
        });
    }

    $(document).ready(function () {
        initialize();
    });
})(jQuery);
