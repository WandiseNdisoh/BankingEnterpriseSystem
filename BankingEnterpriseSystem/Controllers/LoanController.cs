using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BankingEnterpriseSystem.Models;

namespace BankingEnterpriseSystem.Controllers
{
    [Authorize]
    public class LoanController : Controller
    {
        // GET: Loan

        private BankingDbContext db = new BankingDbContext();

        public ActionResult Index()
        {
            var Loans = db.Loans.ToList();
            return View(Loans);
        }

        [HttpGet]
        public ActionResult Apply(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Index", "Customer");
            }

            // Find our matching client target record
            var customer = db.Customers.Find(id);
            if (customer == null)
            {
                return HttpNotFound();
            }

            // Crucial: Send this customer object directly into our View parameter!
            return View(customer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]

        public ActionResult Apply(int customerId, decimal loanAmount, decimal monthlySalary)
        {
            if (loanAmount <= 0 || monthlySalary <=0 )
            {
                ModelState.AddModelError("", "Amount must be greater then zero");
                return View();
            }

            var customer = db.Customers.Find(customerId);
            if (customer == null) return HttpNotFound();

            string finalStatus;

            if ((monthlySalary * 12) < (loanAmount / 3))
            {
                finalStatus = "Rejected";
            }
            else
            {
                finalStatus = "Approved";
            }

            var loanApplication = new Loan
            {
                CustomerId = customerId,
                LoanAmount = loanAmount,
                MonthlySalary = monthlySalary,
                status = finalStatus
            };

            using (var dbTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    db.Loans.Add(loanApplication);

                    if(finalStatus == "Approved")
                    {
                        customer.Balance += loanAmount;

                        var loanPayoutLog = new Transaction
                        {
                            CustomerId = customerId,
                            Amount = loanAmount,
                            TransactionType = "Loan Payout (Approved)",
                            TransactionDate = DateTime.Now
                        };
                        db.Transactions.Add(loanPayoutLog);
                    }
                    db.SaveChanges();
                    dbTransaction.Commit();
                    return RedirectToAction("Index");
                }
                catch (Exception)
                {
                    dbTransaction.Rollback();
                    ModelState.AddModelError("", "Critical failure while evaluating application");
                    ViewBag.Customer = customer;
                    return View();
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}