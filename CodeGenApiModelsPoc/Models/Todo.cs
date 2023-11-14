using PocAttributes;
using System.ComponentModel.DataAnnotations;

namespace CodeGenApiModelsPoc.Models
{
    [ModelGeneration]
    public class Todo
    {
        /// <summary>
        /// The ID of the to-do item.
        /// </summary>
        [PostIgnore]
        public int Id { get; set; }

        /// <summary>
        /// Whether the to-do item has been marked completed or not.
        /// </summary>
        [PostIgnore]
        [Required]
        public bool IsCompleted { get; set; }

        /// <summary>
        /// The title of the to-do item.
        /// </summary>
        [Required]
        [StringLength(100, MinimumLength = 1)]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Notes related to the to-do item.
        /// </summary>
        /// <post>
        /// Notes related to the to-do item. Supports basic markup.
        /// </post>
        [StringLength(255, MinimumLength = 1)]
        public string? Notes { get; set; }

        /// <summary>
        /// The due date of the to-do item.
        /// </summary>
        public DateOnly? Due { get; set; }

        /// <summary>
        /// When the to-do item was created.
        /// </summary>
        [PostIgnore]
        public DateTimeOffset Created { get; set; }

        /// <summary>
        /// When the to-do item was last updated.
        /// </summary>
        [PostIgnore]
        public DateTimeOffset? Updated { get; set; }
    }
}
