using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace ATM
{
    public class Customer
    {
        public int customerID { get; set; }
       
        public int accountNumber { get; set; }
        public string pin { get; set; }

        public Customer(int customerID)
        {
            this.customerID = customerID;
        }
    }
}
