using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using BankingEnterpriseSystem.Models;

namespace BankingEnterpriseSystem.Controllers
{
    [Authorize]
    public class CustomerController : Controller
    {
        private BankingDbContext db = new BankingDbContext();

        // 1. LIST ALL CUSTOMERS
        public ActionResult Index()
        {
            var customers = db.Customers.ToList();
            return View(customers);
        }

        // 2. CREATE CUSTOMER (GET)
        public ActionResult Create()
        {
            return View();
        }

        // 3. CREATE CUSTOMER (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Customer customer)
        {
            if (ModelState.IsValid)
            {
                customer.Balance = 0.00m; // Enforce R0 base balance
                db.Customers.Add(customer);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(customer);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}