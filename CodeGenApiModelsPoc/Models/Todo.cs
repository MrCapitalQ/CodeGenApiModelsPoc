using PocAttributes;
using System.ComponentModel.DataAnnotations;

namespace CodeGenApiModelsPoc.Models
{
    [ModelGeneration]
    public record Todo
    {
        /// <summary>
        /// The ID of the to-do item.
        /// </summary>
        [PostIgnore]
        [PutIgnore]
        public int Id { get; set; }

        /// <summary>
        /// Whether the to-do item has been marked completed or not.
        /// </summary>
        /// <put>
        /// Whether the to-do item is completed or not.
        /// </put>
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
        /// <put>
        /// Notes related to the to-do item. Supports basic markup.
        /// </put>
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
        [PutIgnore]
        public DateTimeOffset Created { get; set; }

        /// <summary>
        /// When the to-do item was last updated.
        /// </summary>
        [PostIgnore]
        [PutIgnore]
        public DateTimeOffset? Updated { get; set; }
    }
}
