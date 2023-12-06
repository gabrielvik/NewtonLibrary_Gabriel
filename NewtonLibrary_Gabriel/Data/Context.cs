using EntityFrameworkCore.EncryptColumn.Interfaces;
using EntityFrameworkCore.EncryptColumn.Util;
using Microsoft.EntityFrameworkCore;
using NewtonLibrary_Gabriel.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using EntityFrameworkCore.EncryptColumn;
using EntityFrameworkCore.EncryptColumn.Attribute;
using Microsoft.Extensions.Options;
using EntityFrameworkCore.EncryptColumn.Extension;

namespace NewtonLibrary_Gabriel.Data
{
    internal class Context : DbContext
    {
        public DbSet<Book> Books { get; set; }
        public DbSet<Author> Authors { get; set; }
        public DbSet<LibraryCard> LibraryCards { get; set; }
        public DbSet<Loan> Loans { get; set; }

        private IEncryptionProvider _provider;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            /* Local SQL DB
             * optionsBuilder.UseSqlServer(@"
                    Server=localhost;
                    Database=NewtonLibrary_Gabriel; 
                    Trusted_Connection=True; 
                    Trust Server Certificate=Yes; 
                    User Id=NewtonLibrary_Gabriel; 
                    password=NewtonLibrary");
            */

            // Azure SQL DB
            optionsBuilder.UseSqlServer(@"
                    Server=tcp:newtonlibrary-gabriel.database.windows.net,1433;Initial Catalog=NewtonLibrary_Gabriel;Persist Security Info=False;User ID=NewtonLibrary_Gabriel;Password=NewtonLib123321;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.UseEncryption(this._provider);
        }

        public Context()
        {
            this._provider = new GenerateEncryptionProvider("kljsdkkdlo4454GG");
        }
    }
}
