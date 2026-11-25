using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace EChannelizer.ViewModels
{
    /// <summary>
    /// Base ViewModel with common functionality
    /// </summary>
    public abstract partial class ViewModelBase : ObservableObject, IDisposable
    {
        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private string _statusMessage = string.Empty;

        [ObservableProperty]
        private double _progressValue;

        [ObservableProperty]
        private bool _isProgressIndeterminate;

        [ObservableProperty]
        private string _title = string.Empty;

        protected ViewModelBase()
        {
        }

        public virtual void Initialize()
        {
        }

        public virtual Task InitializeAsync()
        {
            return Task.CompletedTask;
        }

        public virtual void Cleanup()
        {
        }

        protected void SetBusy(bool busy, string message = "")
        {
            IsBusy = busy;
            StatusMessage = message;
        }

        protected void SetProgress(double value, bool indeterminate = false)
        {
            ProgressValue = value;
            IsProgressIndeterminate = indeterminate;
        }

        public virtual void Dispose()
        {
            Cleanup();
            GC.SuppressFinalize(this);
        }
    }

    /// <summary>
    /// Base ViewModel for screens/dialogs
    /// </summary>
    public abstract partial class ScreenViewModel : ViewModelBase
    {
        [ObservableProperty]
        private bool _canClose = true;

        public event EventHandler<ScreenCloseEventArgs>? RequestClose;

        protected ScreenViewModel() : base()
        {
        }

        public virtual bool CanClose()
        {
            return CanClose;
        }

        public virtual void Close(bool? result = null)
        {
            RequestClose?.Invoke(this, new ScreenCloseEventArgs(result ?? true));
        }

        protected virtual void OnRequestClose(bool result)
        {
            RequestClose?.Invoke(this, new ScreenCloseEventArgs(result));
        }
    }

    /// <summary>
    /// Event args for screen close
    /// </summary>
    public class ScreenCloseEventArgs : EventArgs
    {
        public bool Result { get; }

        public ScreenCloseEventArgs(bool result)
        {
            Result = result;
        }
    }

    /// <summary>
    /// Base ViewModel for list items
    /// </summary>
    public partial class ListItemViewModel : ObservableObject
    {
        [ObservableProperty]
        private bool _isSelected;

        [ObservableProperty]
        private bool _isExpanded;

        [ObservableProperty]
        private bool _isVisible = true;

        [ObservableProperty]
        private int _index;

        public ListItemViewModel()
        {
        }
    }

    /// <summary>
    /// Base ViewModel for editable items
    /// </summary>
    public abstract partial class EditableViewModel<T> : ViewModelBase where T : class
    {
        [ObservableProperty]
        private T _item = null!;

        [ObservableProperty]
        private bool _isEditing;

        [ObservableProperty]
        private bool _isNew;

        [ObservableProperty]
        private string _validationErrors = string.Empty;

        protected EditableViewModel() : base()
        {
        }

        public virtual void BeginEdit(T item, bool isNew = false)
        {
            Item = item;
            IsNew = isNew;
            IsEditing = true;
            ValidationErrors = string.Empty;
            OnBeginEdit();
        }

        public virtual void CancelEdit()
        {
            IsEditing = false;
            OnCancelEdit();
        }

        public virtual bool SaveEdit()
        {
            if (Validate())
            {
                IsEditing = false;
                OnSaveEdit();
                return true;
            }
            return false;
        }

        protected virtual void OnBeginEdit()
        {
        }

        protected virtual void OnCancelEdit()
        {
        }

        protected virtual void OnSaveEdit()
        {
        }

        protected abstract bool Validate();
    }
}