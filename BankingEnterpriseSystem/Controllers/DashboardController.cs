using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BankingEnterpriseSystem.Models;

namespace BankingEnterpriseSystem.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private BankingDbContext db = new BankingDbContext();
        // GET: Dasboard
        public ActionResult Index()
        {
            int CustomerCount = db.Customers.Count();

            decimal totalDeposits = db.Customers.Sum(c => (decimal?)c.Balance) ?? 0;
            decimal totalLoans = db.Loans
                                    .Where(l => l.status == "Approved")
                                    .Sum(l => (decimal?)l.LoanAmount) ?? 0;

            int transactionCount = db.Transactions.Count();

            var recentActivities = db.Transactions
                                    .OrderByDescending(t => t.TransactionDate)
                                    .Take(5)
                                    .ToList();

            var dashboardData = new DashboardViewModel
            {
                TotalCustomers = CustomerCount,
                TotalDeposits = totalDeposits,
                TotalActiveLoans = totalLoans,
                TotalTransactionsCount = transactionCount,
                RecentTransactions = recentActivities
            };
            return View(dashboardData);
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}