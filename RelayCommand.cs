using System.Windows.Input;

namespace Dreamine.MVVM.ViewModels
{
	/// <summary>
	/// \if KO
	/// <para>매개변수가 없는 기본 RelayCommand 구현입니다. ViewModel에서 ICommand 바인딩용으로 사용됩니다.</para>
	/// \endif
	/// \if EN
	/// <para>Encapsulates relay command functionality and related state.</para>
	/// \endif
	/// </summary>
	public sealed class RelayCommand : ICommand
	{
		/// <summary>
		/// \if KO
		/// <para>execute 값을 보관합니다.</para>
		/// \endif
		/// \if EN
		/// <para>Stores the execute value.</para>
		/// \endif
		/// </summary>
		private readonly Action _execute;
		/// <summary>
		/// \if KO
		/// <para>can Execute 값을 보관합니다.</para>
		/// \endif
		/// \if EN
		/// <para>Stores the can execute value.</para>
		/// \endif
		/// </summary>
		private readonly Func<bool>? _canExecute;

		/// <summary>
		/// \if KO
		/// <para>sync Context 값을 보관합니다.</para>
		/// \endif
		/// \if EN
		/// <para>Stores the sync context value.</para>
		/// \endif
		/// </summary>
		private readonly SynchronizationContext? _syncContext;

		/// <summary>
		/// \if KO
		/// <para>RelayCommand 생성자</para>
		/// \endif
		/// \if EN
		/// <para>Initializes a new instance of the <see cref="RelayCommand"/> class with the specified settings.</para>
		/// \endif
		/// </summary>
		/// <param name="execute">
		/// \if KO
		/// <para>실행 메서드</para>
		/// \endif
		/// \if EN
		/// <para>The <see cref="Action"/> value used for execute.</para>
		/// \endif
		/// </param>
		/// <param name="canExecute">
		/// \if KO
		/// <para>실행 가능 여부 판단 메서드 (선택)</para>
		/// \endif
		/// \if EN
		/// <para>The <c>Func&lt;bool&gt;</c> value used for can execute.</para>
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
		public RelayCommand(Action execute, Func<bool>? canExecute = null)
		{
			// Argument 검증
			_execute = execute ?? throw new ArgumentNullException(nameof(execute));
			_canExecute = canExecute;

			// 생성 시점의 SynchronizationContext를 캡처하여 UI 스레드 마샬링에 사용
			_syncContext = SynchronizationContext.Current;
		}

		/// <summary>
		/// \if KO
		/// <para>현재 명령이 실행 가능한지를 나타냅니다.</para>
		/// \endif
		/// \if EN
		/// <para>Determines whether can execute.</para>
		/// \endif
		/// </summary>
		/// <param name="parameter">
		/// \if KO
		/// <para>명령 실행에 사용될 파라미터 (사용하지 않음)</para>
		/// \endif
		/// \if EN
		/// <para>The <see cref="object"/> value used for parameter.</para>
		/// \endif
		/// </param>
		/// <returns>
		/// \if KO
		/// <para>명령이 실행 가능한 경우 true, 그렇지 않으면 false</para>
		/// \endif
		/// \if EN
		/// <para><see langword="true"/> when the can execute condition is satisfied; otherwise, <see langword="false"/>.</para>
		/// \endif
		/// </returns>
		public bool CanExecute(object? parameter)
		{
			// canExecute 미지정 시 항상 실행 가능
			return _canExecute?.Invoke() ?? true;
		}

		/// <summary>
		/// \if KO
		/// <para>명령을 실행합니다.</para>
		/// \endif
		/// \if EN
		/// <para>Performs the execute operation.</para>
		/// \endif
		/// </summary>
		/// <param name="parameter">
		/// \if KO
		/// <para>명령 실행에 사용될 파라미터 (사용하지 않음)</para>
		/// \endif
		/// \if EN
		/// <para>The <see cref="object"/> value used for parameter.</para>
		/// \endif
		/// </param>
		public void Execute(object? parameter)
		{
			if (CanExecute(parameter))
				_execute();
		}

		/// <summary>
		/// \if KO
		/// <para>명령의 실행 가능 상태가 변경될 때 발생하는 이벤트입니다.</para>
		/// \endif
		/// \if EN
		/// <para>Occurs when can execute changed takes place.</para>
		/// \endif
		/// </summary>
		public event EventHandler? CanExecuteChanged;

		/// <summary>
		/// \if KO
		/// <para>CanExecute 상태를 수동으로 갱신합니다. UI 스레드가 아닌 스레드에서 호출되더라도 UI 스레드로 안전하게 마샬링합니다.</para>
		/// \endif
		/// \if EN
		/// <para>Performs the raise can execute changed operation.</para>
		/// \endif
		/// </summary>
		public void RaiseCanExecuteChanged()
		{
			// 이벤트 핸들러가 없으면 빠른 종료
			var handler = CanExecuteChanged;
			if (handler is null)
				return;

			// 캡처한 컨텍스트가 있고 현재 컨텍스트가 다르면 Post로 UI 스레드 호출
			if (_syncContext != null && !ReferenceEquals(SynchronizationContext.Current, _syncContext))
			{
				_syncContext.Post(static state =>
				{
					// state는 (RelayCommand, EventHandler) 튜플
					var tuple = ((RelayCommand cmd, EventHandler evt))state!;
					tuple.evt.Invoke(tuple.cmd, EventArgs.Empty);
				}, (this, handler));

				return;
			}

			// 동일 컨텍스트(보통 UI)에서 즉시 호출
			handler.Invoke(this, EventArgs.Empty);
		}
	}

	/// <summary>
	/// \if KO
	/// <para>제네릭 RelayCommand 구현입니다. 매개변수를 사용하는 경우 사용됩니다.</para>
	/// \endif
	/// \if EN
	/// <para>Encapsulates relay command functionality and related state.</para>
	/// \endif
	/// </summary>
	/// <typeparam name="T">
	/// \if KO
	/// <para>매개변수 타입</para>
	/// \endif
	/// \if EN
	/// <para>The T type parameter.</para>
	/// \endif
	/// </typeparam>
	public sealed class RelayCommand<T> : ICommand
	{
		/// <summary>
		/// \if KO
		/// <para>execute 값을 보관합니다.</para>
		/// \endif
		/// \if EN
		/// <para>Stores the execute value.</para>
		/// \endif
		/// </summary>
		private readonly Action<T> _execute;
		/// <summary>
		/// \if KO
		/// <para>can Execute 값을 보관합니다.</para>
		/// \endif
		/// \if EN
		/// <para>Stores the can execute value.</para>
		/// \endif
		/// </summary>
		private readonly Func<T, bool>? _canExecute;

		/// <summary>
		/// \if KO
		/// <para>sync Context 값을 보관합니다.</para>
		/// \endif
		/// \if EN
		/// <para>Stores the sync context value.</para>
		/// \endif
		/// </summary>
		private readonly SynchronizationContext? _syncContext;

		/// <summary>
		/// \if KO
		/// <para>RelayCommand 생성자</para>
		/// \endif
		/// \if EN
		/// <para>Initializes a new instance of the <see cref="RelayCommand"/> class with the specified settings.</para>
		/// \endif
		/// </summary>
		/// <param name="execute">
		/// \if KO
		/// <para>실행 메서드</para>
		/// \endif
		/// \if EN
		/// <para>The <see cref="Action{T}"/> value used for execute.</para>
		/// \endif
		/// </param>
		/// <param name="canExecute">
		/// \if KO
		/// <para>실행 가능 여부 판단 메서드 (선택)</para>
		/// \endif
		/// \if EN
		/// <para>The <c>Func&lt;T, bool&gt;</c> value used for can execute.</para>
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
		public RelayCommand(Action<T> execute, Func<T, bool>? canExecute = null)
		{
			// Argument 검증
			_execute = execute ?? throw new ArgumentNullException(nameof(execute));
			_canExecute = canExecute;

			// 생성 시점 SynchronizationContext 캡처
			_syncContext = SynchronizationContext.Current;
		}

		/// <summary>
		/// \if KO
		/// <para>명령이 현재 실행 가능한지를 결정합니다.</para>
		/// \endif
		/// \if EN
		/// <para>Determines whether can execute.</para>
		/// \endif
		/// </summary>
		/// <param name="parameter">
		/// \if KO
		/// <para>명령 실행에 전달된 파라미터. <typeparamref name="T"/>로 변환됩니다.</para>
		/// \endif
		/// \if EN
		/// <para>The <see cref="object"/> value used for parameter.</para>
		/// \endif
		/// </param>
		/// <returns>
		/// \if KO
		/// <para>명령이 실행 가능하면 true, 그렇지 않으면 false</para>
		/// \endif
		/// \if EN
		/// <para><see langword="true"/> when the can execute condition is satisfied; otherwise, <see langword="false"/>.</para>
		/// \endif
		/// </returns>
		public bool CanExecute(object? parameter)
		{
			// canExecute 미지정 시 항상 실행 가능
			if (_canExecute is null)
				return true;

			// 파라미터 변환 실패 시 실행 불가 처리(예외 대신 false)
			if (!TryGetParameter(parameter, out var value))
				return false;

			return _canExecute.Invoke(value);
		}

		/// <summary>
		/// \if KO
		/// <para>명령을 실행합니다.</para>
		/// \endif
		/// \if EN
		/// <para>Performs the execute operation.</para>
		/// \endif
		/// </summary>
		/// <param name="parameter">
		/// \if KO
		/// <para>명령 실행에 전달된 파라미터. <typeparamref name="T"/>로 변환됩니다.</para>
		/// \endif
		/// \if EN
		/// <para>The <see cref="object"/> value used for parameter.</para>
		/// \endif
		/// </param>
		/// <exception cref="ArgumentException">
		/// \if KO
		/// <para>입력 인자가 유효하지 않은 경우 발생합니다.</para>
		/// \endif
		/// \if EN
		/// <para>Thrown when an input argument is invalid.</para>
		/// \endif
		/// </exception>
		public void Execute(object? parameter)
		{
			if (!CanExecute(parameter))
				return;

			if (!TryGetParameter(parameter, out var value))
				throw new ArgumentException($"Command parameter is not assignable to {typeof(T).FullName}.", nameof(parameter));

			_execute(value);
		}

		/// <summary>
		/// \if KO
		/// <para>명령의 실행 가능 상태가 변경되었음을 알리는 이벤트입니다. UI 바인딩 요소는 이 이벤트를 통해 CanExecute 상태를 다시 평가합니다.</para>
		/// \endif
		/// \if EN
		/// <para>Occurs when can execute changed takes place.</para>
		/// \endif
		/// </summary>
		public event EventHandler? CanExecuteChanged;

		/// <summary>
		/// \if KO
		/// <para>CanExecute 상태를 수동으로 갱신합니다. UI 스레드가 아닌 스레드에서 호출되더라도 UI 스레드로 안전하게 마샬링합니다.</para>
		/// \endif
		/// \if EN
		/// <para>Performs the raise can execute changed operation.</para>
		/// \endif
		/// </summary>
		public void RaiseCanExecuteChanged()
		{
			// 핸들러 스냅샷
			var handler = CanExecuteChanged;
			if (handler is null)
				return;

			// 다른 컨텍스트라면 Post로 이벤트 발생
			if (_syncContext != null && !ReferenceEquals(SynchronizationContext.Current, _syncContext))
			{
				_syncContext.Post(static state =>
				{
					// state는 (RelayCommand<T>, EventHandler) 튜플
					var tuple = ((RelayCommand<T> cmd, EventHandler evt))state!;
					tuple.evt.Invoke(tuple.cmd, EventArgs.Empty);
				}, (this, handler));

				return;
			}

			handler.Invoke(this, EventArgs.Empty);
		}

		/// <summary>
		/// \if KO
		/// <para>ICommand parameter를 <typeparamref name="T"/>로 안전하게 변환합니다.</para>
		/// \endif
		/// \if EN
		/// <para>Attempts to get parameter and returns whether the operation succeeds.</para>
		/// \endif
		/// </summary>
		/// <param name="parameter">
		/// \if KO
		/// <para>원본 파라미터</para>
		/// \endif
		/// \if EN
		/// <para>The <see cref="object"/> value used for parameter.</para>
		/// \endif
		/// </param>
		/// <param name="value">
		/// \if KO
		/// <para>변환 결과</para>
		/// \endif
		/// \if EN
		/// <para>The value to apply.</para>
		/// \endif
		/// </param>
		/// <returns>
		/// \if KO
		/// <para>변환 성공 시 true, 실패 시 false</para>
		/// \endif
		/// \if EN
		/// <para><see langword="true"/> when the try get parameter condition is satisfied; otherwise, <see langword="false"/>.</para>
		/// \endif
		/// </returns>
		private static bool TryGetParameter(object? parameter, out T value)
		{
			// null 처리
			if (parameter is null)
			{
				// 참조 타입 또는 Nullable이면 default 허용
				if (default(T) is null)
				{
					value = default!;
					return true;
				}

				// non-nullable value type은 null 불가
				value = default!;
				return false;
			}

			// 이미 T면 그대로 사용
			if (parameter is T t)
			{
				value = t;
				return true;
			}

			// 타입 불일치
			value = default!;
			return false;
		}
	}
}
