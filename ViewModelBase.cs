using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Dreamine.MVVM.ViewModels
{
    /// <summary>
    /// \if KO
    /// <para>Dreamine MVVM의 모든 ViewModel이 상속하는 기본 클래스입니다. INotifyPropertyChanged 및 INotifyPropertyChanging을 구현하며, 속성 변경 알림 기능을 제공합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Encapsulates view model base functionality and related state.</para>
    /// \endif
    /// </summary>
    public abstract partial class ViewModelBase : INotifyPropertyChanged, INotifyPropertyChanging
    {
        /// <summary>
        /// \if KO
        /// <para>Property Changed 상황이 발생할 때 알립니다.</para>
        /// \endif
        /// \if EN
        /// <para>Occurs when property changed takes place.</para>
        /// \endif
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// \if KO
        /// <para>Property Changing 상황이 발생할 때 알립니다.</para>
        /// \endif
        /// \if EN
        /// <para>Occurs when property changing takes place.</para>
        /// \endif
        /// </summary>
        public event PropertyChangingEventHandler? PropertyChanging;

        /// <summary>
        /// \if KO
        /// <para>지정된 속성 이름으로 PropertyChanged 이벤트를 발생시킵니다.</para>
        /// \endif
        /// \if EN
        /// <para>Handles the property changed event or state change.</para>
        /// \endif
        /// </summary>
        /// <param name="propertyName">
        /// \if KO
        /// <para>속성 이름</para>
        /// \endif
        /// \if EN
        /// <para>The <see cref="string"/> value used for property name.</para>
        /// \endif
        /// </param>
        // null! is intentional: CallerMemberName always supplies the caller's member name at compile time.
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null!)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// \if KO
        /// <para>지정된 속성 이름으로 PropertyChanging 이벤트를 발생시킵니다.</para>
        /// \endif
        /// \if EN
        /// <para>Handles the property changing event or state change.</para>
        /// \endif
        /// </summary>
        /// <param name="propertyName">
        /// \if KO
        /// <para>속성 이름</para>
        /// \endif
        /// \if EN
        /// <para>The <see cref="string"/> value used for property name.</para>
        /// \endif
        /// </param>
        protected void OnPropertyChanging([CallerMemberName] string propertyName = null!)
        {
            PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(propertyName));
        }

        /// <summary>
        /// \if KO
        /// <para>백킹 필드를 설정하고 값이 변경되었을 경우 PropertyChanging/PropertyChanged를 발생시킵니다.</para>
        /// \endif
        /// \if EN
        /// <para>Sets the property value.</para>
        /// \endif
        /// </summary>
        /// <typeparam name="T">
        /// \if KO
        /// <para>속성 타입</para>
        /// \endif
        /// \if EN
        /// <para>The T type parameter.</para>
        /// \endif
        /// </typeparam>
        /// <param name="field">
        /// \if KO
        /// <para>백킹 필드</para>
        /// \endif
        /// \if EN
        /// <para>The <typeparamref name="T"/> value used for field.</para>
        /// \endif
        /// </param>
        /// <param name="value">
        /// \if KO
        /// <para>새로운 값</para>
        /// \endif
        /// \if EN
        /// <para>The value to apply.</para>
        /// \endif
        /// </param>
        /// <param name="propertyName">
        /// \if KO
        /// <para>속성 이름</para>
        /// \endif
        /// \if EN
        /// <para>The <see cref="string"/> value used for property name.</para>
        /// \endif
        /// </param>
        /// <returns>
        /// \if KO
        /// <para>값이 변경되었는지 여부</para>
        /// \endif
        /// \if EN
        /// <para><see langword="true"/> when the set property condition is satisfied; otherwise, <see langword="false"/>.</para>
        /// \endif
        /// </returns>
        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null!)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;

            OnPropertyChanging(propertyName);
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
