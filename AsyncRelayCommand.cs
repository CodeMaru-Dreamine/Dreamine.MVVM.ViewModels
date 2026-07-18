using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Dreamine.MVVM.ViewModels
{
    /// <summary>
    /// \if KO
    /// <para>Async Relay Command 기능과 관련 상태를 캡슐화합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Provides an asynchronous <see cref="ICommand"/> implementation that prevents re-entrant execution and exposes failure information.</para>
    /// \endif
    /// </summary>
    public sealed class AsyncRelayCommand : ICommand
    {
        /// <summary>
        /// \if KO
        /// <para>execute 값을 보관합니다.</para>
        /// \endif
        /// \if EN
        /// <para>Stores the execute value.</para>
        /// \endif
        /// </summary>
        private readonly Func<Task> _execute;
        /// <summary>
        /// \if KO
        /// <para>can Execute 값을 보관합니다.</para>
        /// \endif
        /// \if EN
        /// <para>Stores the can execute value.</para>
        /// \endif
        /// </summary>
        private readonly Func<bool>? _canExecute;
        // Interlocked: 0 = idle, 1 = executing. Prevents concurrent entry without lock.
        /// <summary>
        /// \if KO
        /// <para>is Executing 값을 보관합니다.</para>
        /// \endif
        /// \if EN
        /// <para>Stores the is executing value.</para>
        /// \endif
        /// </summary>
        private int _isExecuting;
        /// <summary>
        /// \if KO
        /// <para>last Exception 값을 보관합니다.</para>
        /// \endif
        /// \if EN
        /// <para>Stores the last exception value.</para>
        /// \endif
        /// </summary>
        private volatile Exception? _lastException;

        /// <summary>
        /// \if KO
        /// <para>지정한 설정으로 <see cref="AsyncRelayCommand"/> 클래스의 새 인스턴스를 초기화합니다.</para>
        /// \endif
        /// \if EN
        /// <para>Initializes a new instance of the <see cref="AsyncRelayCommand"/> class.</para>
        /// \endif
        /// </summary>
        /// <param name="execute">
        /// \if KO
        /// <para>execute에 사용할 <c>Func&lt;Task&gt;</c> 값입니다.</para>
        /// \endif
        /// \if EN
        /// <para>The asynchronous execute delegate.</para>
        /// \endif
        /// </param>
        /// <param name="canExecute">
        /// \if KO
        /// <para>can Execute에 사용할 <c>Func&lt;bool&gt;</c> 값입니다.</para>
        /// \endif
        /// \if EN
        /// <para>The optional can-execute delegate.</para>
        /// \endif
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// \if KO
        /// <para>필수 입력 인자 중 하나가 <see langword="null"/>인 경우 발생합니다.</para>
        /// \endif
        /// \if EN
        /// <para>Thrown when a required input argument is <see langword="null"/>.</para>
        /// \endif
        /// </exception>
        public AsyncRelayCommand(Func<Task> execute, Func<bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        /// <summary>
        /// \if KO
        /// <para>Can Execute Changed 상황이 발생할 때 알립니다.</para>
        /// \endif
        /// \if EN
        /// <para>Occurs when can execute changed takes place.</para>
        /// \endif
        /// </summary>
        public event EventHandler? CanExecuteChanged;

        /// <summary>
        /// \if KO
        /// <para>Execution Failed 상황이 발생할 때 알립니다.</para>
        /// \endif
        /// \if EN
        /// <para>Occurs when the asynchronous delegate throws an exception.</para>
        /// \endif
        /// </summary>
        public event EventHandler<Exception>? ExecutionFailed;

        /// <summary>
        /// \if KO
        /// <para>Last Exception 값을 가져옵니다.</para>
        /// \endif
        /// \if EN
        /// <para>Gets the last exception thrown by the asynchronous delegate, or <c>null</c>.</para>
        /// \endif
        /// </summary>
        public Exception? LastException => _lastException;

        /// <summary>
        /// \if KO
        /// <para>Can Execute 조건을 확인합니다.</para>
        /// \endif
        /// \if EN
        /// <para>Determines whether can execute.</para>
        /// \endif
        /// </summary>
        /// <param name="parameter">
        /// \if KO
        /// <para>parameter에 사용할 <see cref="object"/> 값입니다.</para>
        /// \endif
        /// \if EN
        /// <para>The <see cref="object"/> value used for parameter.</para>
        /// \endif
        /// </param>
        /// <returns>
        /// \if KO
        /// <para>Can Execute 조건이 충족되면 <see langword="true"/>이고, 그렇지 않으면 <see langword="false"/>입니다.</para>
        /// \endif
        /// \if EN
        /// <para><see langword="true"/> when the can execute condition is satisfied; otherwise, <see langword="false"/>.</para>
        /// \endif
        /// </returns>
        public bool CanExecute(object? parameter)
        {
            return Volatile.Read(ref _isExecuting) == 0 && (_canExecute?.Invoke() ?? true);
        }

        /// <summary>
        /// \if KO
        /// <para>Execute 작업을 수행합니다.</para>
        /// \endif
        /// \if EN
        /// <para>Performs the execute operation.</para>
        /// \endif
        /// </summary>
        /// <param name="parameter">
        /// \if KO
        /// <para>parameter에 사용할 <see cref="object"/> 값입니다.</para>
        /// \endif
        /// \if EN
        /// <para>The <see cref="object"/> value used for parameter.</para>
        /// \endif
        /// </param>
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
        /// \if KO
        /// <para>Raise Can Execute Changed 작업을 수행합니다.</para>
        /// \endif
        /// \if EN
        /// <para>Raises the <see cref="CanExecuteChanged"/> event.</para>
        /// \endif
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
