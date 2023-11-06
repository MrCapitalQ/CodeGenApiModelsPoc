using PocAttributes;

namespace CodeGenApiModelsPoc.Models
{
    [ModelGeneration]
    public class Person
    {
        [PostIgnore]
        public int Id { get; set; }

        /// <summary>
        /// The given name of the person.
        /// 
        /// More complex stuff goes here.
        /// 
        ///     Indented stuff here.
        /// </summary>
        /// <post>
        /// This is required
        /// </post>
        public string FirstName { get; set; } = string.Empty;
        public string MiddleName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;

    }
}
