namespace Battlegrounds.Functional;

/// <summary>
/// Represents the outcome of an operation that can either succeed or fail.
/// </summary>
/// <typeparam name="T">The type of the result value.</typeparam>
public abstract class Result<T> {

    /// <summary>
    /// Gets the value of the result.
    /// </summary>
    public abstract T? Value { get; }

    /// <summary>
    /// Gets a value indicating whether the result is empty.
    /// </summary>
    public abstract bool IsEmpty { get; }

    /// <summary>
    /// Chains an action to be executed if the result is successful.
    /// </summary>
    /// <param name="action">The action to be executed.</param>
    /// <returns>The result instance.</returns>
    public abstract Result<T?> Then(Action<T?> action);

    //public abstract Result<T2> Then<T2>(Func<T, T2> func);

    /// <summary>
    /// Chains an action to be executed if the result is a failure.
    /// </summary>
    /// <param name="action">The action to be executed.</param>
    /// <returns>The result instance.</returns>
    public abstract Result<T?> Else(Action action);

}

/// <summary>
/// Represents a successful result.
/// </summary>
/// <typeparam name="T">The type of the result value.</typeparam>
public sealed class Success<T> : Result<T?> {

    /// <summary>
    /// Gets the value of the successful result.
    /// </summary>
    public override T? Value { get; }

    /// <summary>
    /// Gets a value indicating whether the result is empty.
    /// </summary>
    public override bool IsEmpty => false;

    /// <summary>
    /// Initializes a new instance of the <see cref="Success{T}"/> class with the provided value.
    /// </summary>
    /// <param name="value">The value of the successful result.</param>
    public Success(T? value) {
        Value = value;
    }

    /// <summary>
    /// Chains an action to be executed if the result is successful.
    /// </summary>
    /// <param name="action">The action to be executed.</param>
    /// <returns>The result instance.</returns>
    public override Result<T?> Then(Action<T?> action) {
        action(Value); return this;
    }

    /// <summary>
    /// Chains an action to be executed if the result is a failure.
    /// </summary>
    /// <param name="action">The action to be executed.</param>
    /// <returns>The result instance.</returns>
    public override Result<T?> Else(Action action) => this;

}

/// <summary>
/// Represents a failed result.
/// </summary>
/// <typeparam name="T">The type of the result value.</typeparam>
public sealed class Failure<T> : Result<T?> {

    /// <summary>
    /// Throws an exception since a failure result doesn't have a value.
    /// </summary>
    public override T? Value => throw new InvalidOperationException();

    /// <summary>
    /// Gets a value indicating whether the result is empty.
    /// </summary>
    public override bool IsEmpty => true;

    /// <summary>
    /// Chains an action to be executed if the result is a failure.
    /// </summary>
    /// <param name="action">The action to be executed.</param>
    /// <returns>The result instance.</returns>
    public override Result<T?> Else(Action action) {
        action(); return this;
    }

    /// <summary>
    /// Chains an action to be executed if the result is successful.
    /// </summary>
    /// <param name="action">The action to be executed.</param>
    /// <returns>The result instance.</returns>
    public override Result<T?> Then(Action<T?> action) {
        return this;
    }
}

/// <summary>
/// Represents an asynchronous result that allows deferring the value resolution until it becomes available.
/// </summary>
/// <typeparam name="T">The type of the result value.</typeparam>
public sealed class AsyncResult<T> : Result<T?> {

    /// <summary>
    /// Represents a provider for the asynchronous result.
    /// </summary>
    public sealed class AsyncResultProvider {

        private Action? notifier;
        private T? value;
        private bool hasValue;
        private bool hasProvided;

        /// <summary>
        /// Gets the value of the result.
        /// </summary>
        public T? Value => hasValue ? value : throw new Exception();

        /// <summary>
        /// Gets a value indicating whether the value is available.
        /// </summary>
        public bool HasValue => hasValue;

        /// <summary>
        /// Hooks an action to be invoked when the value becomes available or a failure occurs.
        /// </summary>
        /// <param name="provided">The action to be invoked.</param>
        internal void Hook(Action provided) {
            this.notifier = provided;
        }

        /// <summary>
        /// Sets the value of the result and notifies subscribers.
        /// </summary>
        /// <param name="value">The value of the result.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the provider has already provided a result.
        /// </exception>
        public void Success(T? value) {
            if (hasProvided) {
                throw new InvalidOperationException("Deferred result provider has already provided a result");
            }
            this.value = value;
            hasValue = true;
            hasProvided = true;
            notifier?.Invoke();
        }

        /// <summary>
        /// Notifies subscribers of a failure.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the provider has already provided a result.
        /// </exception>
        public void Failure() {
            if (hasProvided) {
                throw new InvalidOperationException("Deferred result provider has already provided a result");
            }
            hasProvided = true;
            notifier?.Invoke();
        }

        /// <summary>
        /// Returns a <see cref="AsyncResult{T}"/> representing an asynchronous call to <paramref name="action"/>.
        /// If <paramref name="action"/> does not provide a value to the <see cref="AsyncResultProvider"/>,
        /// the call is considered to have failed and will result in a failure notification.
        /// </summary>
        /// <param name="action">The action to invoke to obtain a result.</param>
        /// <returns>The deferred result representing the asynchronous execution of the action.</returns>
        public AsyncResult<T> Defer(Action<AsyncResultProvider> action) {
            var result = new AsyncResult<T>(this);
            Task.Run(() => {
                try {
                    action(this);
                } finally {
                    if (!this.hasProvided) {
                        this.Failure();
                    }
                }
            });
            return result;
        }

    }

    /// <summary>
    /// Represents an asynchronous call to be executed.
    /// </summary>
    /// <param name="ElseAction">The action to be executed if the result is a failure.</param>
    /// <param name="ThenAction">The action to be executed if the result is successful.</param>
    private readonly record struct DeferCall(Action? ElseAction, Action<T?>? ThenAction);

    private readonly Queue<DeferCall> calls;
    private readonly AsyncResultProvider provider;

    /// <summary>
    /// Gets the value of the result.
    /// </summary>
    public override T? Value => provider.HasValue ? provider.Value : throw new Exception();

    /// <summary>
    /// Gets a value indicating whether the result is empty.
    /// </summary>
    public override bool IsEmpty => provider.HasValue;

    /// <summary>
    /// Initializes a new instance of the <see cref="AsyncResult{T}"/> class with the provided result provider.
    /// </summary>
    /// <param name="provider">The provider for the asynchronous result.</param>
    public AsyncResult(AsyncResultProvider provider) {
        this.calls = new();
        this.provider = provider;
        this.provider.Hook(() => {
            while(calls.Count > 0) {
                var next = calls.Dequeue();
                if (this.provider.HasValue) {
                    next.ThenAction?.Invoke(this.provider.Value);
                } else {
                    next.ElseAction?.Invoke();
                }
            }
        });
    }

    /// <summary>
    /// Chains an action to be executed if the result is a failure.
    /// </summary>
    /// <param name="action">The action to be executed.</param>
    /// <returns>The result instance.</returns>
    public override Result<T?> Else(Action action) {
        calls.Enqueue(new(action, null)); return this;
    }

    /// <summary>
    /// Chains an action to be executed if the result is successful.
    /// </summary>
    /// <param name="action">The action to be executed.</param>
    /// <returns>The result instance.</returns>
    public override Result<T?> Then(Action<T?> action) {
        calls.Enqueue(new(null,action)); return this;
    }

}

/// <summary>
/// Provides a helper method for creating and deferring actions with a <see cref="AsyncResult{T}"/>.
/// </summary>
public static class AsyncResult {

    /// <summary>
    /// Creates and defers an action using a <see cref="AsyncResult{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of the deferred result.</typeparam>
    /// <param name="action">The action to invoke to obtain a result.</param>
    /// <returns>The deferred result representing the asynchronous execution of the action.</returns>
    public static AsyncResult<T?> Defer<T>(Action<AsyncResult<T?>.AsyncResultProvider> action) {
        return new AsyncResult<T?>.AsyncResultProvider().Defer(action);
    }

}
