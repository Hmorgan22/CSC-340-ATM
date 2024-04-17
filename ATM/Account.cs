using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace ATM
{
    public class Account
    {
        public int accountNumber { get; set; }
        public int balance { get; set; }
        public int transactionTotal {get; set;}
        public string lastTransactionDate { get; set; }

       public Account(int accountNumber, int balance, string lastTransactionDate, int transactionTotal)
        {
            this.accountNumber = accountNumber;
            this.balance = balance;
            this.lastTransactionDate = lastTransactionDate;
            this.transactionTotal = transactionTotal;

        }


        

    }
}
