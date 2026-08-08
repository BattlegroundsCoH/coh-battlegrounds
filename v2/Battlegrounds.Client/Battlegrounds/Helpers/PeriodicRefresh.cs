using System.Windows.Threading;

using Microsoft.Extensions.Logging;

namespace Battlegrounds.Helpers;

public sealed class PeriodicRefresh(TimeSpan interval, Func<CancellationToken, Task> onTick, ILogger logger) {

    private readonly TimeSpan _interval = interval;
    private readonly Func<CancellationToken, Task> _onTick = onTick;
    private readonly ILogger _logger = logger;

    private DispatcherTimer? _timer;
    private CancellationTokenSource? _cancellation;
    private bool _isTicking;

    public bool IsRunning => _timer?.IsEnabled ?? false;

    public void Start() {

        if (IsRunning) {
            return;
        }

        if (_timer is null) {
            _timer = new DispatcherTimer(DispatcherPriority.Background, Dispatcher.CurrentDispatcher) { Interval = _interval };
            _timer.Tick += OnTick;
        }

        _cancellation = new CancellationTokenSource();
        _timer.Start();

    }

    public void Stop() {

        _timer?.Stop();

        _cancellation?.Cancel();
        _cancellation = null;

    }

    private async void OnTick(object? sender, EventArgs e) {

        if (_isTicking || _cancellation is not { Token: var token }) {
            return;
        }

        _isTicking = true;
        try {
            await _onTick(token);
        } catch (OperationCanceledException) {
            // Stopped mid-request. Expected, and not a failure.
        } catch (Exception ex) {
            _logger.LogError(ex, "A periodic refresh failed. The schedule continues.");
        } finally {
            _isTicking = false;
        }

    }

}
