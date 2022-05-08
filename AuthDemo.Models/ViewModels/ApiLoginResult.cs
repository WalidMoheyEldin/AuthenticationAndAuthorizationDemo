using System;
using System.Collections.Generic;
using System.Text;

namespace AuthDemo.Models.ViewModels
{
    public class ApiLoginResult
    {
        public string Token { get; set; }
        public DateTime ExpiredOn { get; set; }
    }
}
