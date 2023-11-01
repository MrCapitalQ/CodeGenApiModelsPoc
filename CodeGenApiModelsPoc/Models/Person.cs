using PocAttributes;

namespace CodeGenApiModelsPoc.Models
{
    [ModelGeneration]
    public class Person
    {
        [PostIgnore]
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string MiddleName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;

    }
}
