using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AuthDemo.Web.ViewModels
{
    public class CreateRoleViewModel
    {
        [Required]
        public string Name { get; set; }

        public List<string> Policies { get; set; }
    }

    public class EditRoleViewModel
    {
        public string Id { get; set; }

        [Required]
        public string Name { get; set; }

        public List<string> Policies { get; set; }
    }
}
