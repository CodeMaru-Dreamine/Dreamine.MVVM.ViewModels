using System;
using System.Threading;
using System.Windows.Input;

namespace Dreamine.MVVM.ViewModels
{
	/// <summary>
	/// 매개변수가 없는 기본 RelayCommand 구현입니다.
	/// ViewModel에서 ICommand 바인딩용으로 사용됩니다.
	/// </summary>
	public sealed class RelayCommand : ICommand
	{
		private readonly Action _execute;
		private readonly Func<bool>? _canExecute;

		private readonly SynchronizationContext? _syncContext;

		/// <summary>
		/// RelayCommand 생성자
		/// </summary>
		/// <param name="execute">실행 메서드</param>
		/// <param name="canExecute">실행 가능 여부 판단 메서드 (선택)</param>
		public RelayCommand(Action execute, Func<bool>? canExecute = null)
		{
			/// <brief>Argument 검증</brief>
			_execute = execute ?? throw new ArgumentNullException(nameof(execute));
			_canExecute = canExecute;

			/// <brief>생성 시점의 SynchronizationContext를 캡처하여 UI 스레드 마샬링에 사용</brief>
			_syncContext = SynchronizationContext.Current;
		}

		/// <summary>
		/// 현재 명령이 실행 가능한지를 나타냅니다.
		/// </summary>
		/// <param name="parameter">명령 실행에 사용될 파라미터 (사용하지 않음)</param>
		/// <returns>명령이 실행 가능한 경우 true, 그렇지 않으면 false</returns>
		public bool CanExecute(object? parameter)
		{
			/// <brief>canExecute 미지정 시 항상 실행 가능</brief>
			return _canExecute?.Invoke() ?? true;
		}

		/// <summary>
		/// 명령을 실행합니다.
		/// </summary>
		/// <param name="parameter">명령 실행에 사용될 파라미터 (사용하지 않음)</param>
		public void Execute(object? parameter)
		{
			_execute();
		}

		/// <summary>
		/// 명령의 실행 가능 상태가 변경될 때 발생하는 이벤트입니다.
		/// </summary>
		public event EventHandler? CanExecuteChanged;

		/// <summary>
		/// CanExecute 상태를 수동으로 갱신합니다.
		/// UI 스레드가 아닌 스레드에서 호출되더라도 UI 스레드로 안전하게 마샬링합니다.
		/// </summary>
		public void RaiseCanExecuteChanged()
		{
			/// <brief>이벤트 핸들러가 없으면 빠른 종료</brief>
			var handler = CanExecuteChanged;
			if (handler is null)
				return;

			/// <brief>캡처한 컨텍스트가 있고 현재 컨텍스트가 다르면 Post로 UI 스레드 호출</brief>
			if (_syncContext != null && !ReferenceEquals(SynchronizationContext.Current, _syncContext))
			{
				_syncContext.Post(static state =>
				{
					/// <brief>state는 (RelayCommand, EventHandler) 튜플</brief>
					var tuple = ((RelayCommand cmd, EventHandler evt))state!;
					tuple.evt.Invoke(tuple.cmd, EventArgs.Empty);
				}, (this, handler));

				return;
			}

			/// <brief>동일 컨텍스트(보통 UI)에서 즉시 호출</brief>
			handler.Invoke(this, EventArgs.Empty);
		}
	}

	/// <summary>
	/// 제네릭 RelayCommand 구현입니다. 매개변수를 사용하는 경우 사용됩니다.
	/// </summary>
	/// <typeparam name="T">매개변수 타입</typeparam>
	public sealed class RelayCommand<T> : ICommand
	{
		private readonly Action<T> _execute;
		private readonly Func<T, bool>? _canExecute;

		private readonly SynchronizationContext? _syncContext;

		/// <summary>
		/// RelayCommand 생성자
		/// </summary>
		/// <param name="execute">실행 메서드</param>
		/// <param name="canExecute">실행 가능 여부 판단 메서드 (선택)</param>
		public RelayCommand(Action<T> execute, Func<T, bool>? canExecute = null)
		{
			/// <brief>Argument 검증</brief>
			_execute = execute ?? throw new ArgumentNullException(nameof(execute));
			_canExecute = canExecute;

			/// <brief>생성 시점 SynchronizationContext 캡처</brief>
			_syncContext = SynchronizationContext.Current;
		}

		/// <summary>
		/// 명령이 현재 실행 가능한지를 결정합니다.
		/// </summary>
		/// <param name="parameter">명령 실행에 전달된 파라미터. <typeparamref name="T"/>로 변환됩니다.</param>
		/// <returns>명령이 실행 가능하면 true, 그렇지 않으면 false</returns>
		public bool CanExecute(object? parameter)
		{
			/// <brief>canExecute 미지정 시 항상 실행 가능</brief>
			if (_canExecute is null)
				return true;

			/// <brief>파라미터 변환 실패 시 실행 불가 처리(예외 대신 false)</brief>
			if (!TryGetParameter(parameter, out var value))
				return false;

			return _canExecute.Invoke(value);
		}

		/// <summary>
		/// 명령을 실행합니다.
		/// </summary>
		/// <param name="parameter">명령 실행에 전달된 파라미터. <typeparamref name="T"/>로 변환됩니다.</param>
		public void Execute(object? parameter)
		{
			/// <brief>Execute는 예외를 숨기기보다, 안전 변환이 안되면 명확히 예외 처리</brief>
			if (!TryGetParameter(parameter, out var value))
				throw new ArgumentException($"Command parameter is not assignable to {typeof(T).FullName}.", nameof(parameter));

			_execute(value);
		}

		/// <summary>
		/// 명령의 실행 가능 상태가 변경되었음을 알리는 이벤트입니다.
		/// UI 바인딩 요소는 이 이벤트를 통해 CanExecute 상태를 다시 평가합니다.
		/// </summary>
		public event EventHandler? CanExecuteChanged;

		/// <summary>
		/// CanExecute 상태를 수동으로 갱신합니다.
		/// UI 스레드가 아닌 스레드에서 호출되더라도 UI 스레드로 안전하게 마샬링합니다.
		/// </summary>
		public void RaiseCanExecuteChanged()
		{
			/// <brief>핸들러 스냅샷</brief>
			var handler = CanExecuteChanged;
			if (handler is null)
				return;

			/// <brief>다른 컨텍스트라면 Post로 이벤트 발생</brief>
			if (_syncContext != null && !ReferenceEquals(SynchronizationContext.Current, _syncContext))
			{
				_syncContext.Post(static state =>
				{
					/// <brief>state는 (RelayCommand&lt;T&gt;, EventHandler) 튜플</brief>
					var tuple = ((RelayCommand<T> cmd, EventHandler evt))state!;
					tuple.evt.Invoke(tuple.cmd, EventArgs.Empty);
				}, (this, handler));

				return;
			}

			handler.Invoke(this, EventArgs.Empty);
		}

		/// <summary>
		/// ICommand parameter를 <typeparamref name="T"/>로 안전하게 변환합니다.
		/// </summary>
		/// <param name="parameter">원본 파라미터</param>
		/// <param name="value">변환 결과</param>
		/// <returns>변환 성공 시 true, 실패 시 false</returns>
		private static bool TryGetParameter(object? parameter, out T value)
		{
			/// <brief>null 처리</brief>
			if (parameter is null)
			{
				/// <brief>참조 타입 또는 Nullable이면 default 허용</brief>
				if (default(T) is null)
				{
					value = default!;
					return true;
				}

				/// <brief>non-nullable value type은 null 불가</brief>
				value = default!;
				return false;
			}

			/// <brief>이미 T면 그대로 사용</brief>
			if (parameter is T t)
			{
				value = t;
				return true;
			}

			/// <brief>타입 불일치</brief>
			value = default!;
			return false;
		}
	}
}
