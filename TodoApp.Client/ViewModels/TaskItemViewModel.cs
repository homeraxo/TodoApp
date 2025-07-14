using GalaSoft.MvvmLight.Command;
using System;
using System.Windows.Input;
using TodoApp.Client.Models;

namespace TodoApp.Client.ViewModels
{
    public class TaskItemViewModel : BaseViewModel
    {
        private Task _task;
        private bool _isEditing;
        private string _originalTitle; 
        private string _originalDescription;

        public Task Task
        {
            get => _task;
            set => SetProperty(ref _task, value);
        }

        public Guid Id => _task.Id;
        public string Title
        {
            get => _task.Title;
            set => SetProperty(ref _task.Title, value);
        }
        public string Description
        {
            get => _task.Description;
            set => SetProperty(ref _task.Description, value);
        }
        public bool IsCompleted
        {
            get => _task.IsCompleted;
            set
            {
                if (SetProperty(ref _task.IsCompleted, value))
                {
                    // Notifivr al servidor que la tarea ha sido completada o descompletada
                }
            }
        }

        public bool IsLocked
        {
            get => _task.IsLocked;
            set
            {
                if (SetProperty(ref _task.IsLocked, value))
                {
                    OnPropertyChanged(nameof(CanEdit)); 
                }
            }
        }

        public string LockedByClient
        {
            get => _task.LockedByClient;
            set => SetProperty(ref _task.LockedByClient, value);
        }
                
        public bool IsEditing
        {
            get => _isEditing;
            set
            {
                if (SetProperty(ref _isEditing, value))
                {
                    if (value) 
                    {
                        _originalTitle = Title;
                        _originalDescription = Description;
                    }
                    OnPropertyChanged(nameof(CanEdit)); 
                }
            }
        }
                
        public bool CanEdit => !IsEditing && !IsLocked;
                
        public ICommand ToggleCompleteCommand { get; }
        public ICommand StartEditCommand { get; }
        public ICommand SaveEditCommand { get; }
        public ICommand CancelEditCommand { get; }
        public ICommand DeleteCommand { get; }

        public TaskItemViewModel(Task task)
        {
            _task = task;
            ToggleCompleteCommand = new RelayCommand(OnToggleComplete);
            StartEditCommand = new RelayCommand(OnStartEdit, () => CanEdit);
            SaveEditCommand = new RelayCommand(OnSaveEdit);
            CancelEditCommand = new RelayCommand(OnCancelEdit);
            DeleteCommand = new RelayCommand(OnDelete);
        }

        private void OnToggleComplete()
        {
            IsCompleted = !IsCompleted;            
        }

        private void OnStartEdit()
        {
            IsEditing = true;            
        }

        private void OnSaveEdit()
        {
            IsEditing = false;
        }

        private void OnCancelEdit()
        {
            IsEditing = false;
            Title = _originalTitle;
            Description = _originalDescription;
        }

        private void OnDelete()
        {
            // La lógica para eliminar se manejará en TaskListViewModel o el servicio SignalR
        }
    }

}
