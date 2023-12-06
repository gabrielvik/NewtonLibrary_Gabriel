using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewtonLibrary_Gabriel.Models
{
    internal class Loan
    {
        public int Id { get; set; }
        
        // Foreign keys
        public int BookId { get; set; }
        public int LibraryCardId { get; set; }

        // Loan columns
        public DateTime? LoanDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public bool IsBorrowed { get; set; }

        // Establish relations between the classes (Many to one)
        public Book Book { get; set; }
        public LibraryCard LibraryCard { get; set; }
    }
}
