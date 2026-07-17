using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace BankingEnterpriseSystem.Models
{
    public class Loan
    {
        [Key]
        public int LoanId { get; set; }

        [Required]
        public int CustomerId { get; set; }

        [Required]
        [Display(Name ="Requested Loan Amount")]
        public decimal LoanAmount { get; set; }

        [Required]
        [Display(Name ="Monthly Salary")]
        public decimal MonthlySalary { get; set; }

        public string status { get; set; } // Approved, Rejected, Pending
    }
}