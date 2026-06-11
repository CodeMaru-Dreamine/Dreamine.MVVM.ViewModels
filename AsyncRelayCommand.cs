using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Dreamine.MVVM.ViewModels
{
    /// <summary>
    /// Provides an asynchronous <see cref="ICommand"/> implementation that prevents
    /// re-entrant execution and exposes failure information.
    /// </summary>
    public sealed class AsyncRelayCommand : ICommand
    {
        private readonly Func<Task> _execute;
        private readonly Func<bool>? _canExecute;
        // Interlocked: 0 = idle, 1 = executing. Prevents concurrent entry without lock.
        private int _isExecuting;
        private volatile Exception? _lastException;

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncRelayCommand"/> class.
        /// </summary>
        /// <param name="execute">The asynchronous execute delegate.</param>
        /// <param name="canExecute">The optional can-execute delegate.</param>
        public AsyncRelayCommand(Func<Task> execute, Func<bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        /// <inheritdoc />
        public event EventHandler? CanExecuteChanged;

        /// <summary>
        /// Occurs when the asynchronous delegate throws an exception.
        /// </summary>
        public event EventHandler<Exception>? ExecutionFailed;

        /// <summary>
        /// Gets the last exception thrown by the asynchronous delegate, or <c>null</c>.
        /// </summary>
        public Exception? LastException => _lastException;

        /// <inheritdoc />
        public bool CanExecute(object? parameter)
        {
            return Volatile.Read(ref _isExecuting) == 0 && (_canExecute?.Invoke() ?? true);
        }

        /// <inheritdoc />
        public async void Execute(object? parameter)
        {
            // Atomically claim execution slot; bail if already executing.
            if (Interlocked.CompareExchange(ref _isExecuting, 1, 0) != 0)
            {
                return;
            }

            RaiseCanExecuteChanged();
            try
            {
                await _execute().ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                _lastException = ex;

                var handler = ExecutionFailed;
                if (handler is not null)
                {
                    handler.Invoke(this, ex);
                }
                else
                {
                    // No subscriber — emit a diagnostic trace so the exception is visible
                    // in the Output window rather than silently disappearing.
                    Debug.WriteLine(
                        $"[AsyncRelayCommand] Unhandled exception (subscribe to ExecutionFailed to suppress this): {ex}");
                }
            }
            finally
            {
                Interlocked.Exchange(ref _isExecuting, 0);
                RaiseCanExecuteChanged();
            }
        }

        /// <summary>
        /// Raises the <see cref="CanExecuteChanged"/> event.
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
