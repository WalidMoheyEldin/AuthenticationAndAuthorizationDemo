using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace AuthDemo.Models
{
    public class Employee
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Firs tName")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }
    }
}
