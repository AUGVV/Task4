using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Task4.Models;
using Microsoft.Extensions.Configuration;
using System.IO;


namespace Task4.Data
{
    public class UsersContext : DbContext
    {
        public DbSet<UserDataModel> Users { get; set; }
        public UsersContext(DbContextOptions<UsersContext> options) : base(options)
        {
            Database.EnsureCreated();
        }
    }
}
