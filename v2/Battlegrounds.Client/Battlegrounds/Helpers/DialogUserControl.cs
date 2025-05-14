using System.Windows.Controls;

namespace Battlegrounds.Helpers;

public abstract class DialogUserControl : UserControl {

    private readonly TaskCompletionSource<object> _tcs = new TaskCompletionSource<object>();

    public DialogUserControl(INotifyModalDone notifier) {
        ArgumentNullException.ThrowIfNull(notifier);
        this.DataContext = notifier;
        notifier.ModalDone += (sender, args) => {
            if (sender is INotifyModalDone) {
                Complete(args);
            }
        };
    }

    public async Task<T> Await<T>() {
        await _tcs.Task;
        if (_tcs.Task.IsCompletedSuccessfully) {
            return (T)_tcs.Task.Result;
        }
        if (_tcs.Task.IsFaulted) {
            throw _tcs.Task.Exception!;
        }
        if (_tcs.Task.IsCanceled) {
            throw new TaskCanceledException(_tcs.Task);
        }
        throw new InvalidOperationException("Task is not completed.");
    }

    private void Complete(object result) {
        if (_tcs.Task.IsCompleted) {
            throw new InvalidOperationException("Task is already completed.");
        }
        _tcs.SetResult(result);
    }

}
