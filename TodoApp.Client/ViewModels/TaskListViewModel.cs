using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using TodoApp.Client.Models;

namespace TodoApp.Client.ViewModels
{
    public class TaskListViewModel : BaseViewModel
    {
        private readonly SignalRClientService _signalRService;
        private ObservableCollection<TaskItemViewModel> _tasks;
        private string _newTaskTitle;
        private string _newTaskDescription;

        public ObservableCollection<TaskItemViewModel> Tasks
        {
            get => _tasks;
            set => SetProperty(ref _tasks, value);
        }

        public string NewTaskTitle
        {
            get => _newTaskTitle;
            set => SetProperty(ref _newTaskTitle, value);
        }

        public string NewTaskDescription
        {
            get => _newTaskDescription;
            set => SetProperty(ref _newTaskDescription, value);
        }

        public ICommand AddTaskCommand { get; }

        public TaskListViewModel()
        {
            _tasks = new ObservableCollection<TaskItemViewModel>();
            _signalRService = new SignalRClientService("http://localhost:8080/signalr"); 

            AddTaskCommand = new RelayCommand(async () => await OnAddTask());
                        
            _signalRService.TaskAdded += OnTaskAdded;
            _signalRService.TaskUpdated += OnTaskUpdated;
            _signalRService.TaskDeleted += OnTaskDeleted;
            _signalRService.TaskLocked += OnTaskLocked;
            _signalRService.TaskUnlocked += OnTaskUnlocked;
            _signalRService.TaskUpdateFailed += OnTaskUpdateFailed;
            _signalRService.TaskLockFailed += OnTaskLockFailed;
            _signalRService.TaskDeleteFailed += OnTaskDeleteFailed;
                                    
            _ = InitializeAsync(); // Llama a un método asíncrono sin esperar
        }

        private async System.Threading.Tasks.Task InitializeAsync()
        {
            await _signalRService.ConnectAsync();
            var initialTasks = await _signalRService.GetAllTasksAsync();
            foreach (var task in initialTasks)
            {
                Tasks.Add(new TaskItemViewModel(task));
            }
        }

        private async System.Threading.Tasks.Task OnAddTask()
        {
            if (string.IsNullOrWhiteSpace(NewTaskTitle)) return;

            var newTask = new Task
            {
                Title = NewTaskTitle,
                Description = NewTaskDescription,
                IsCompleted = false,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
            await _signalRService.AddTaskAsync(newTask);

            NewTaskTitle = string.Empty;
            NewTaskDescription = string.Empty;
        }

        private void OnTaskAdded(object sender, Task task)
        {
            // Ejecutar en el hilo de la UI si es necesario (Dispatcher)
            App.Current.Dispatcher.Invoke(() =>
            {
                Tasks.Add(new TaskItemViewModel(task));
            });
        }

        private void OnTaskUpdated(object sender, Task updatedTask)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                var existingTaskVm = Tasks.FirstOrDefault(t => t.Id == updatedTask.Id);
                if (existingTaskVm != null)
                {                    
                    existingTaskVm.Task = updatedTask; // Asigna el objeto Task actualizado
                    existingTaskVm.OnPropertyChanged(nameof(TaskItemViewModel.Title)); // Notifica cambios
                    existingTaskVm.OnPropertyChanged(nameof(TaskItemViewModel.Description));
                    existingTaskVm.OnPropertyChanged(nameof(TaskItemViewModel.IsCompleted));
                    existingTaskVm.OnPropertyChanged(nameof(TaskItemViewModel.IsLocked));
                    existingTaskVm.OnPropertyChanged(nameof(TaskItemViewModel.LockedByClient));
                    existingTaskVm.OnPropertyChanged(nameof(TaskItemViewModel.CanEdit)); // Recalcular
                }
            });
        }

        private void OnTaskDeleted(object sender, Guid taskId)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                var taskToRemove = Tasks.FirstOrDefault(t => t.Id == taskId);
                if (taskToRemove != null)
                {
                    Tasks.Remove(taskToRemove);
                }
            });
        }

        private void OnTaskLocked(object sender, (Guid taskId, string lockedByClient) args)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                var taskVm = Tasks.FirstOrDefault(t => t.Id == args.taskId);
                if (taskVm != null)
                {
                    taskVm.IsLocked = true;
                    taskVm.LockedByClient = args.lockedByClient;
                }
            });
        }

        private void OnTaskUnlocked(object sender, Guid taskId)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                var taskVm = Tasks.FirstOrDefault(t => t.Id == taskId);
                if (taskVm != null)
                {
                    taskVm.IsLocked = false;
                    taskVm.LockedByClient = null;
                }
            });
        }

        private void OnTaskUpdateFailed(object sender, (Guid taskId, string message) args)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                System.Windows.MessageBox.Show($"Failed to update task {args.taskId}: {args.message}", "Update Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            });
        }

        private void OnTaskLockFailed(object sender, (Guid taskId, string message) args)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                System.Windows.MessageBox.Show($"Failed to lock task {args.taskId}: {args.message}", "Lock Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            });
        }

        private void OnTaskDeleteFailed(object sender, (Guid taskId, string message) args)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                System.Windows.MessageBox.Show($"Failed to delete task {args.taskId}: {args.message}", "Delete Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            });
        }
    }
}
