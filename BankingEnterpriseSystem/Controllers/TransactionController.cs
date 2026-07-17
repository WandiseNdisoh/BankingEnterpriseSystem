using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BankingEnterpriseSystem.Models;

namespace BankingEnterpriseSystem.Controllers
{
    [Authorize]
    public class TransactionController : Controller
    {
        private BankingDbContext db = new BankingDbContext();

        // ==========================================
        // 1. TRANSACTION LEDGER INDEX
        // ==========================================
        // GET: Transaction/Index/5
        public ActionResult Index(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Index", "Customer");
            }

            var customer = db.Customers.Find(id);
            if (customer == null) return HttpNotFound();

            ViewBag.Customer = customer;

            var history = db.Transactions
                            .Where(t => t.CustomerId == id)
                            .OrderByDescending(t => t.TransactionDate)
                            .ToList();
            return View(history);
        }

        // ==========================================
        // 2. DEPOSIT OPERATIONS (MATCHES DEPOSIT VIEW)
        // ==========================================
        // GET: Transaction/Deposit/5
        [HttpGet]
        public ActionResult Deposit(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Index", "Customer");
            }

            var customer = db.Customers.Find(id);
            if (customer == null)
            {
                return HttpNotFound();
            }

            // CRITICAL FIX: Explicitly passing the 'customer' object to the View
            // so @Model inside Deposit.cshtml is populated with data.
            return View(customer);
        }

        // POST: Transaction/Deposit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Deposit(int customerId, decimal amount)
        {
            var customer = db.Customers.Find(customerId);
            if (customer == null)
            {
                return HttpNotFound();
            }

            if (amount <= 0)
            {
                ModelState.AddModelError("", "Deposit amount must be greater than zero.");
                return View(customer);
            }

            using (var dbTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    // Balance operation
                    customer.Balance += amount;

                    // Audit ledger entry
                    var transaction = new Transaction
                    {
                        CustomerId = customerId,
                        Amount = amount,
                        TransactionType = "Deposit",
                        TransactionDate = DateTime.Now
                    };

                    db.Transactions.Add(transaction);
                    db.Entry(customer).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();

                    dbTransaction.Commit();

                    return RedirectToAction("Index", "Customer");
                }
                catch (Exception)
                {
                    dbTransaction.Rollback();
                    ModelState.AddModelError("", "Internal database error. Transaction rolled back.");
                    return View(customer);
                }
            }
        }

        // ==========================================
        // 3. WITHDRAWAL OPERATIONS
        // ==========================================
        // GET: Transaction/Withdraw/5
        [HttpGet]
        public ActionResult Withdraw(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Index", "Customer");
            }

            var customer = db.Customers.Find(id);
            if (customer == null) return HttpNotFound();

            return View(customer);
        }

        // POST: Transaction/Withdraw
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Withdraw(int customerId, decimal amount)
        {
            var customer = db.Customers.Find(customerId);
            if (customer == null) return HttpNotFound();

            if (amount <= 0)
            {
                ModelState.AddModelError("", "Withdrawal must be greater than zero.");
                return View(customer);
            }

            using (var dbTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    if (customer.Balance < amount)
                    {
                        ModelState.AddModelError("", "Insufficient funds available.");
                        return View(customer);
                    }

                    customer.Balance -= amount;

                    var transaction = new Transaction
                    {
                        CustomerId = customerId,
                        Amount = -amount,
                        TransactionType = "Withdrawal",
                        TransactionDate = DateTime.Now
                    };

                    db.Transactions.Add(transaction);
                    db.Entry(customer).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();

                    dbTransaction.Commit();

                    return RedirectToAction("Index", "Customer");
                }
                catch (Exception)
                {
                    dbTransaction.Rollback();
                    ModelState.AddModelError("", "Transaction rejected due to system error.");
                    return View(customer);
                }
            }
        }

        // ==========================================
        // 4. TRANSFER OPERATIONS
        // ==========================================
        // GET: Transaction/Transfer/5
        [HttpGet]
        public ActionResult Transfer(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Index", "Customer");
            }

            var sender = db.Customers.Find(id);
            if (sender == null) return HttpNotFound();

            ViewBag.Sender = sender;
            ViewBag.Recipients = db.Customers.Where(c => c.customerId != id).ToList();
            return View(sender);
        }

        // POST: Transaction/Transfer
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Transfer(int senderId, int receiverId, decimal amount)
        {
            var sender = db.Customers.Find(senderId);
            var receiver = db.Customers.Find(receiverId);

            if (amount <= 0)
            {
                ModelState.AddModelError("", "Transfer amount must be greater than zero.");
                ViewBag.Sender = sender;
                ViewBag.Recipients = db.Customers.Where(c => c.customerId != senderId).ToList();
                return View(sender);
            }

            using (var dbTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    if (sender == null || receiver == null)
                    {
                        ModelState.AddModelError("", "Entities could not be resolved.");
                        return View(sender);
                    }

                    if (sender.Balance < amount)
                    {
                        ModelState.AddModelError("", "Transaction Declined: Insufficient balance.");
                        ViewBag.Sender = sender;
                        ViewBag.Recipients = db.Customers.Where(c => c.customerId != senderId).ToList();
                        return View(sender);
                    }

                    sender.Balance -= amount;
                    receiver.Balance += amount;

                    var debitRecord = new Transaction
                    {
                        CustomerId = senderId,
                        Amount = -amount,
                        TransactionType = $"Transfer Out to {receiver.Name}",
                        TransactionDate = DateTime.Now
                    };

                    var creditRecord = new Transaction
                    {
                        CustomerId = receiverId,
                        Amount = amount,
                        TransactionType = $"Transfer In from {sender.Name}",
                        TransactionDate = DateTime.Now
                    };

                    db.Transactions.Add(debitRecord);
                    db.Transactions.Add(creditRecord);

                    db.Entry(sender).State = System.Data.Entity.EntityState.Modified;
                    db.Entry(receiver).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();

                    dbTransaction.Commit();

                    return RedirectToAction("Index", "Customer");
                }
                catch (Exception)
                {
                    dbTransaction.Rollback();
                    ModelState.AddModelError("", "Processing failure. Transfer aborted.");
                    return View(sender);
                }
            }
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