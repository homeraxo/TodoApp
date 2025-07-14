// Repositories/ITaskRepository.cs
using System;
using System.Collections.Generic;
using TodoApp.Models;
// Asegúrate de que la clase Task esté en un namespace accesible.

namespace TodoApp.Repositories
{
    public interface ITaskRepository : IDisposable
    {
        Task GetById(Guid id);
        IEnumerable<Task> GetAll();
        void Add(Task task);
        void Update(Task task);
        void Delete(Guid id);
        void Save(); // Para confirmar los cambios en la DB
    }
}
