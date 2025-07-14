using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // Necesario para [DatabaseGenerated]

namespace TodoApp.Models
{
    public class Task
    {
        // [Key] no es estrictamente necesario para Id si se sigue la convención de EF
        // Pero lo añadimos para claridad.
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // Indica que el valor es generado por la DB
        public Guid Id { get; set; } // Coincide con UNIQUEIDENTIFIER en SQL Server

        [Required] // Hace que la propiedad sea obligatoria
        [StringLength(255)] // Limita la longitud de la cadena
        public string Title { get; set; }

        public string Description { get; set; } // NVARCHAR(MAX) en SQL Server

        public bool IsCompleted { get; set; } = false; // BIT en SQL Server

        public DateTime CreatedAt { get; set; } // Por defecto es DateTime.MinValue, se inicializará en el código
        public DateTime UpdatedAt { get; set; } // Por defecto es DateTime.MinValue, se inicializará en el código

        public bool IsLocked { get; set; } = false;

        [StringLength(255)]
        public string LockedByClient { get; set; } // String porque puede ser nulo en la DB
    }
}
