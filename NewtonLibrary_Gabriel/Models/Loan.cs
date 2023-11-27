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
        public int BookId { get; set; }
        public int LibraryCardId { get; set; }
        public DateTime? LoanDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public bool IsBorrowed { get; set; }

        public Book Book { get; set; }
        public LibraryCard LibraryCard { get; set; }
    }
}
