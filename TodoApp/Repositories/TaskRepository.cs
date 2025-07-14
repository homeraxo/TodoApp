using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using TodoApp.Data;
using TodoApp.Models;

namespace TodoApp.Repositories
{
    public class TaskRepository : ITaskRepository
    {
        private readonly ApplicationDbContext _context;

        public TaskRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public Task GetById(Guid id)
        {
            return _context.Tasks.Find(id);
        }

        public IEnumerable<Task> GetAll()
        {
            return _context.Tasks.ToList();
        }

        public void Add(Task task)
        {
            _context.Tasks.Add(task);
        }

        public void Update(Task task)
        {
            _context.Entry(task).State = EntityState.Modified;
        }

        public void Delete(Guid id)
        {
            Task task = _context.Tasks.Find(id);
            if (task != null)
            {
                _context.Tasks.Remove(task);
            }
        }

        public void Save()
        {
            _context.SaveChanges();
        }

        // Implementación de IDisposable para liberar recursos del DbContext
        private bool disposed = false;
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
            }
            this.disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}