using Microsoft.AspNet.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TodoApp.Client
{
    public class SignalRClientService
    {
        private HubConnection _connection;
        private IHubProxy _taskHubProxy;
        private readonly string _serverUrl;
                
        public event EventHandler<Task> TaskAdded;
        public event EventHandler<Task> TaskUpdated;
        public event EventHandler<Guid> TaskDeleted;
        public event EventHandler<(Guid taskId, string lockedByClient)> TaskLocked;
        public event EventHandler<Guid> TaskUnlocked;
        public event EventHandler<(Guid taskId, string message)> TaskUpdateFailed;
        public event EventHandler<(Guid taskId, string message)> TaskLockFailed;
        public event EventHandler<(Guid taskId, string message)> TaskDeleteFailed;


        public SignalRClientService(string serverUrl)
        {
            _serverUrl = serverUrl;
        }

        public async Task ConnectAsync()
        {
            _connection = new HubConnection(_serverUrl);
            _taskHubProxy = _connection.CreateHubProxy("taskHub"); // "taskHub" debe coincidir con [HubName("taskHub")] en el servidor
                        
            _taskHubProxy.On<Task>("taskAdded", (task) => TaskAdded?.Invoke(this, task));
            _taskHubProxy.On<Task>("taskUpdated", (task) => TaskUpdated?.Invoke(this, task));
            _taskHubProxy.On<Guid>("taskDeleted", (id) => TaskDeleted?.Invoke(this, id));
            _taskHubProxy.On<Guid, string>("taskLocked", (id, client) => TaskLocked?.Invoke(this, (id, client)));
            _taskHubProxy.On<Guid>("taskUnlocked", (id) => TaskUnlocked?.Invoke(this, id));
            _taskHubProxy.On<Guid, string>("taskUpdateFailed", (id, msg) => TaskUpdateFailed?.Invoke(this, (id, msg)));
            _taskHubProxy.On<Guid, string>("taskLockFailed", (id, msg) => TaskLockFailed?.Invoke(this, (id, msg)));
            _taskHubProxy.On<Guid, string>("taskDeleteFailed", (id, msg) => TaskDeleteFailed?.Invoke(this, (id, msg)));


            try
            {
                await _connection.Start();
                Console.WriteLine("Conectado a SignalR.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al conectar a SignalR: {ex.Message}");
                // Manejar el error de conexión (ej. reintentar, mostrar mensaje al usuario)
            }
        }

        public async Task<IEnumerable<Task>> GetAllTasksAsync()
        {
            if (_connection.State == ConnectionState.Connected)
            {
                return await _taskHubProxy.Invoke<IEnumerable<Task>>("GetAllTasks");
            }
            return new List<Task>();
        }

        public async Task AddTaskAsync(Task task)
        {
            if (_connection.State == ConnectionState.Connected)
            {
                await _taskHubProxy.Invoke("AddTask", task);
            }
        }

        public async Task UpdateTaskAsync(Task task)
        {
            if (_connection.State == ConnectionState.Connected)
            {
                await _taskHubProxy.Invoke("UpdateTask", task);
            }
        }

        public async Task DeleteTaskAsync(Guid taskId)
        {
            if (_connection.State == ConnectionState.Connected)
            {
                await _taskHubProxy.Invoke("DeleteTask", taskId);
            }
        }

        public async Task LockTaskAsync(Guid taskId)
        {
            if (_connection.State == ConnectionState.Connected)
            {
                await _taskHubProxy.Invoke("LockTask", taskId);
            }
        }

        public async Task UnlockTaskAsync(Guid taskId)
        {
            if (_connection.State == ConnectionState.Connected)
            {
                await _taskHubProxy.Invoke("UnlockTask", taskId);
            }
        }

        public void Disconnect()
        {
            if (_connection != null && _connection.State != ConnectionState.Disconnected)
            {
                _connection.Stop();
            }
        }


    }
}
