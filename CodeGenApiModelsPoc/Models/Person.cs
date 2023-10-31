using PocAttributes;

namespace CodeGenApiModelsPoc.Models
{
    [ModelGeneration]
    public class Person
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string MiddleName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;

    }
}
