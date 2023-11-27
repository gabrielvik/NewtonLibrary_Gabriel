using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewtonLibrary_Gabriel.Models
{
    internal class LibraryCard
    {
        public int Id { get; set; }
        public string CardNumber { get; set; }
        public int Pin { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
