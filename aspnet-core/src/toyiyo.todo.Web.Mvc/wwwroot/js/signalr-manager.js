let app = app || {};
(function () {
    let _isConnected = false;

    app.signalr = {
        connect: function () {
            if (_isConnected) {
                console.log('SignalR already connected');
                return;
            }

            try {
                abp.signalr.connect();
                _isConnected = true;
                console.log('SignalR connection initialized');
            } catch (err) {
                console.error('Error connecting to SignalR:', err);
            }
        }
    };
})();
