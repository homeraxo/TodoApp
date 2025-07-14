using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using TodoApp.Data;
using TodoApp.Models;
using TodoApp.Repositories;

namespace TodoApp.Hubs
{
    // Puedes aplicar el atributo HubName si quieres un nombre diferente para el cliente
    [HubName("taskHub")]
    public class TaskHub : Hub
    {
        // Puedes inyectar dependencias aquí usando un Resolver de dependencias personalizado de SignalR
        // Por ahora, crearemos una instancia del repositorio directamente.
        // En una aplicación real, usarías un contenedor IoC (ej. Autofac, Unity)
        // para manejar las dependencias y la vida útil del DbContext.

        public IEnumerable<Task> GetAllTasks()
        {
            using (var context = new ApplicationDbContext())
            using (var repo = new TaskRepository(context))
            {
                return repo.GetAll();
            }
        }

        public void AddTask(Task task)
        {
            using (var context = new ApplicationDbContext())
            using (var repo = new TaskRepository(context))
            {
                task.Id = Guid.NewGuid(); // Asegúrate de que el ID sea generado aquí si no lo fue antes
                task.CreatedAt = DateTime.Now;
                task.UpdatedAt = DateTime.Now;
                repo.Add(task);
                repo.Save();

                // Notifica a todos los clientes que se añadió una nueva tarea
                Clients.All.SendAsync("taskAdded", task);
            }
        }

        public void UpdateTask(Task task)
        {
            using (var context = new ApplicationDbContext())
            using (var repo = new TaskRepository(context))
            {
                // Implementa la lógica de bloqueo aquí
                var existingTask = repo.GetById(task.Id);
                if (existingTask != null)
                {
                    // Lógica de bloqueo: Solo si la tarea no está bloqueada o está bloqueada por el cliente actual
                    if (!existingTask.IsLocked || existingTask.LockedByClient == Context.ConnectionId)
                    {
                        existingTask.Title = task.Title;
                        existingTask.Description = task.Description;
                        existingTask.IsCompleted = task.IsCompleted;
                        existingTask.UpdatedAt = DateTime.Now; // Actualiza la fecha de modificación

                        // Lógica para bloquear/desbloquear
                        existingTask.IsLocked = task.IsLocked;
                        existingTask.LockedByClient = task.IsLocked ? Context.ConnectionId : null;

                        repo.Update(existingTask);
                        repo.Save();

                        // Notifica a todos los clientes del cambio
                        Clients.All.SendAsync("taskUpdated", existingTask);
                    }
                    else
                    {
                        // Si la tarea está bloqueada por otro cliente, notifica al cliente que intentó la acción
                        Clients.Caller.SendAsync("taskUpdateFailed", existingTask.Id, "Task is currently locked by another user.");
                        // Y envía el estado actual (bloqueado) a todos los clientes para mantener la coherencia
                        Clients.All.SendAsync("taskUpdated", existingTask);
                    }
                }
            }
        }

        public void DeleteTask(Guid id)
        {
            using (var context = new ApplicationDbContext())
            using (var repo = new TaskRepository(context))
            {
                // Lógica de bloqueo para eliminación: No se puede eliminar si está bloqueada por otro
                var existingTask = repo.GetById(id);
                if (existingTask != null)
                {
                    if (!existingTask.IsLocked || existingTask.LockedByClient == Context.ConnectionId)
                    {
                        repo.Delete(id);
                        repo.Save();

                        // Notifica a todos los clientes
                        Clients.All.SendAsync("taskDeleted", id);
                    }
                    else
                    {
                        Clients.Caller.SendAsync("taskDeleteFailed", id, "Task is currently locked by another user.");
                    }
                }
            }
        }

        // Métodos específicos para bloqueo/desbloqueo (llamados por el cliente cuando empieza/termina de editar)
        public void LockTask(Guid taskId)
        {
            using (var context = new ApplicationDbContext())
            using (var repo = new TaskRepository(context))
            {
                var task = repo.GetById(taskId);
                if (task != null && !task.IsLocked)
                {
                    task.IsLocked = true;
                    task.LockedByClient = Context.ConnectionId; // Usa el ConnectionId de SignalR
                    repo.Update(task);
                    repo.Save();
                    Clients.All.SendAsync("taskLocked", task.Id, task.LockedByClient);
                }
                else if (task != null && task.IsLocked && task.LockedByClient != Context.ConnectionId)
                {
                    // Si ya está bloqueada por otro, avisa al cliente que intentó bloquearla
                    Clients.Caller.SendAsync("taskLockFailed", taskId, "Task is already locked by another user.");
                }
            }
        }

        public void UnlockTask(Guid taskId)
        {
            using (var context = new ApplicationDbContext())
            using (var repo = new TaskRepository(context))
            {
                var task = repo.GetById(taskId);
                // Solo permite desbloquear si está bloqueada y por el mismo cliente
                if (task != null && task.IsLocked && task.LockedByClient == Context.ConnectionId)
                {
                    task.IsLocked = false;
                    task.LockedByClient = null;
                    repo.Update(task);
                    repo.Save();
                    Clients.All.SendAsync("taskUnlocked", task.Id);
                }
            }
        }

        // Manejo de la desconexión del cliente para liberar bloqueos
        public override System.Threading.Tasks.Task OnDisconnected(bool stopCalled)
        {
            // Cuando un cliente se desconecta, desbloquea cualquier tarea que tuviera bloqueada
            using (var context = new ApplicationDbContext())
            using (var repo = new TaskRepository(context))
            {
                var lockedTasks = context.Tasks.Where(t => t.IsLocked && t.LockedByClient == Context.ConnectionId).ToList();
                foreach (var task in lockedTasks)
                {
                    task.IsLocked = false;
                    task.LockedByClient = null;
                    repo.Update(task);
                }
                repo.Save();
                // Notificar a los clientes que estas tareas se desbloquearon
                foreach (var task in lockedTasks)
                {
                    Clients.All.SendAsync("taskUnlocked", task.Id);
                }
            }
            return base.OnDisconnected(stopCalled);
        }
    }
}