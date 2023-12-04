using CodeGenApiModelsPoc.Models;
using Microsoft.AspNetCore.Mvc;

namespace CodeGenApiModelsPoc.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TodosController : ControllerBase
    {
        private readonly ILogger<TodosController> _logger;

        public TodosController(ILogger<TodosController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Gets all to-do items.
        /// </summary>
        [HttpGet(Name = nameof(GetTodos))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<Todo>> GetTodos()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets a to-do item by ID.
        /// </summary>
        [HttpGet("{id}", Name = nameof(GetTodo))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<Todo> GetTodo(int id)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates a new to-do item.
        /// </summary>
        /// <param name="post">The new to-do item.</param>
        [HttpPost(Name = nameof(CreateTodo))]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<Todo> CreateTodo(TodoPost post)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Updates a to-do item.
        /// </summary>
        /// <param name="id">The ID of the to-do item to update.</param>
        /// <param name="put">The updated to-do item.</param>
        [HttpPut("{id}", Name = nameof(UpdateTodo))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult UpdateTodo(int id, TodoPut put)
        {
            throw new NotImplementedException();
        }
    }
}