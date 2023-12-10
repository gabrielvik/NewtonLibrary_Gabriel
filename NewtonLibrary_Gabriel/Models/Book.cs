namespace NewtonLibrary_Gabriel.Models
{
    internal class Book
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime ReleaseDate { get; set; }
        public int Rating { get; set; }
        public string Isbn { get; set; }

        // Associate the authors with the book (Many-To-Many relation)
        public ICollection<Author> Authors { get; set; }
    }
}
