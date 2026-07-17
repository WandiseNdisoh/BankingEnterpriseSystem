using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BankingEnterpriseSystem.Models
{
    public class DashboardViewModel
    {
        public int TotalCustomers { get; set; }
        public decimal TotalDeposits { get; set; }
        public decimal TotalActiveLoans { get; set; }
        public int TotalTransactionsCount { get; set; }

        public List<Transaction> RecentTransactions { get; set; }
    }
}