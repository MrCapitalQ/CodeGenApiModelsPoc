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
        /// This is required for post.
        /// </post>
        public string FirstName { get; set; } = string.Empty;

        /// <summary>
        /// Some complex indented stuff below
        /// 
        ///     blah blah blah
        ///     
        /// 
        /// omg
        /// </summary>
        public string MiddleName { get; set; } = string.Empty;

        /// <summary>
        /// Regular summary with no post specific verb summary.
        /// </summary>
        public string LastName { get; set; } = string.Empty;

    }
}
