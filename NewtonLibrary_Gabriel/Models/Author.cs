namespace NewtonLibrary_Gabriel.Models
{
    internal class Author
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        // Associate the books with the author (Many-To-Many relation)
        public ICollection<Book> Books { get; set; }
    }
}
