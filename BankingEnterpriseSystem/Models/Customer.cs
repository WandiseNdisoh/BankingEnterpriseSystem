using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace BankingEnterpriseSystem.Models
{
    public class Customer
    {
        [Key]
        public int customerId { get; set; }

        [Required]
        [Display( Name ="Full Name")]
        public string Name { get; set; }

        public string Phone { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Currency)]
        public decimal Balance { get; set; }
    }
}