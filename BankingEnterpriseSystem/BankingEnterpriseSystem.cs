using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;

namespace BankingEnterpriseSystem.Models
{
    [DbConfigurationType(typeof(MySql.Data.EntityFramework.MySqlEFConfiguration))]
    public class BankingDbContext : DbContext
    {
        public BankingDbContext() : base("name=BankingDbContext")
        {
            Database.SetInitializer<BankingDbContext>(null);
        }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Loan> Loans { get; set; }

    }
}