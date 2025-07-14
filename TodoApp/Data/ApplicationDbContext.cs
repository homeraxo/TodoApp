// Data/ApplicationDbContext.cs
using System.Data.Entity;
using TodoApp.Models; // Importante: usar System.Data.Entity para EF6
// Asegúrate de que la clase Task esté en un namespace accesible.

namespace TodoApp.Data
{
    public class ApplicationDbContext : DbContext
    {
        // El constructor por defecto buscará una cadena de conexión con el mismo nombre de la clase
        // en Web.config. O puedes pasar el nombre de la cadena de conexión:
        public ApplicationDbContext() : base("DefaultConnection")
        {
        }

        // DbSet representa la colección de todas las entidades en la base de datos
        public DbSet<Task> Tasks { get; set; }

        // Puedes anular OnModelCreating para configuraciones adicionales (Fluent API)
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // Esto es opcional si tus convenciones de nombres son estándar
            // y tus atributos (ej. [StringLength]) son suficientes.
            // Ejemplo de configuración Fluent API:
            // modelBuilder.Entity<Task>()
            //     .Property(t => t.Title)
            //     .IsRequired()
            //     .HasMaxLength(255);

            base.OnModelCreating(modelBuilder);
        }
    }
}