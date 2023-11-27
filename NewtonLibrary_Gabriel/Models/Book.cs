using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewtonLibrary_Gabriel.Models
{
    internal class Book
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime ReleaseDate { get; set; }
        public int Rating { get; set; }
        public string Isbn { get; set; }

        public ICollection<Author> Authors { get; set; }
    }
}
