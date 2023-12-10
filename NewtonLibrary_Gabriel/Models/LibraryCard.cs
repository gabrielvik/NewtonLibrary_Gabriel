using EntityFrameworkCore.EncryptColumn.Attribute;

namespace NewtonLibrary_Gabriel.Models
{
    internal class LibraryCard
    {
        public int Id { get; set; }
        public string CardNumber { get; set; }
        [EncryptColumn]
        public string Pin { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
