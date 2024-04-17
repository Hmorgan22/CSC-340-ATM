using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;


namespace ATM
{
    public partial class Form1 : Form
    {
        //declare variables
        public List<Account> accountList = new List<Account>();
        Customer customer = new Customer(0);
        string transactionAmount = "";
        string customerPin = "";


        MySqlConnection conn = new MySqlConnection("server=csitmariadb.eku.edu;user=student;database=csc340_db;port=3306;password=Maroon@21?;");

        public Form1()
        {
            //set all panels to hidden
            InitializeComponent();
            loginPanel.Visible = false;
            homePanel.Visible = false;
            checkBalancePanel.Visible = false;
            selectAccountBalancePanel.Visible = false;
            depositPanel.Visible = false;
            depositAccountListPanel.Visible = false;
            withdrawAccountLlistPanel.Visible = false;
            withdrawPanel.Visible = false;
            transferFromPanel.Visible = false;
            transferToPanel.Visible = false;
            transferPanel.Visible = false;

        }

        private void logoutHomeBtn_Click(object sender, EventArgs e)
        {
            //returns to login screen
            homePanel.Visible = false;
            loginPanel.Visible = true;
            customerPin = "";
            pinText.Text = "";
        }

        private void enterBtn_Click(object sender, EventArgs e)
        {
            //opens connections and reads all customers from teh DB
            conn.Open();
            string sql = "SELECT * FROM morgan_h_customer";
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            MySqlDataReader myReader = cmd.ExecuteReader();
            while (myReader.Read())
            {
                if (myReader["pin"].ToString().Equals(customerPin))
                {
                    loginPanel.Visible = false;
                    homePanel.Visible = true;

                    customer.customerID = Int32.Parse(myReader["customerId"].ToString());
                }

            }
            myReader.Close();
            //checks for a customer with the pin that was entered
            if (customer.customerID == 0)
            {
                string message = "Invalid pin, please try again.";
                MessageBox.Show(message);
                customerPin = "";
                startBtn_Click(sender, e);
            }
            conn.Close();
        }

        private void checkBalanceBtn_Click(object sender, EventArgs e)
        {
            homePanel.Visible = false;
            selectAccountBalancePanel.Visible = true;
            accountList.Clear();

            //gets the info using customer id
            conn.Open();
            string sql = "SELECT accountNumber, balance, lastTransDate, transTotal FROM morgan_h_account WHERE customerId=@id";
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            //creates list of accounts
            cmd.Parameters.AddWithValue("@id", customer.customerID);
            MySqlDataReader myReader = cmd.ExecuteReader();
            while (myReader.Read())
            {
                accountList.Add(new Account(Int32.Parse(myReader["accountNumber"].ToString()), Int32.Parse(myReader["balance"].ToString()), myReader["lastTransDate"].ToString(), Int32.Parse(myReader["transTotal"].ToString())));

            }
            myReader.Close();

            //adds accoutns to display box
            selectAccountList.Items.Clear();
            foreach (Account acc in accountList)
            {
                if (!selectAccountList.Items.Contains(acc))
                {
                    selectAccountList.Items.Add(acc.accountNumber);
                }
            }
            conn.Close();
        }

        private void checkBalanceReturnBtn_Click(object sender, EventArgs e)
        {
            checkBalancePanel.Visible = false;
            homePanel.Visible = true;
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            //displays text based on the selected account
            checkBalanceText.Text = $"${accountList.ElementAt(selectAccountList.SelectedIndex).balance}";
            accountTextBox.Text = $"{accountList.ElementAt(selectAccountList.SelectedIndex).accountNumber}";
        }

        private void selectAccountReturnBtn_Click(object sender, EventArgs e)
        {
            selectAccountBalancePanel.Visible = false;
            homePanel.Visible = true;
        }

        private void selectAccountBtn_Click(object sender, EventArgs e)
        {
            selectAccountBalancePanel.Visible = false;
            checkBalancePanel.Visible = true;
        }

        private void selectAccountBalancePanel_Paint(object sender, PaintEventArgs e)
        {

        }

        private void depositBtn_Click(object sender, EventArgs e)
        {
            homePanel.Visible = false;
            depositAccountListPanel.Visible = true;
            transactionAmount = "";
            accountList.Clear();

            //uses customer ID to get the account list
            conn.Open();
            string sql = "SELECT accountNumber, balance, lastTransDate, transTotal FROM morgan_h_account WHERE customerId=@id";
            MySqlCommand cmd = new MySqlCommand(sql, conn);

            cmd.Parameters.AddWithValue("@id", customer.customerID);
            MySqlDataReader myReader = cmd.ExecuteReader();
            while (myReader.Read())
            {
                accountList.Add(new Account(Int32.Parse(myReader["accountNumber"].ToString()), Int32.Parse(myReader["balance"].ToString()), myReader["lastTransDate"].ToString(), Int32.Parse(myReader["transTotal"].ToString())));

            }
            myReader.Close();

            depositAccountList.Items.Clear();
            foreach (Account acc in accountList)
            {
                if (!depositAccountList.Items.Contains(acc))
                {
                    depositAccountList.Items.Add(acc.accountNumber);
                }
            }
            conn.Close();
        }

        private void depositAccountList_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void depositAccountListReturn_Click(object sender, EventArgs e)
        {
            depositAccountListPanel.Visible = false;
            homePanel.Visible = true;

        }

        private void depositAccountListSelect_Click(object sender, EventArgs e)
        {
            depositAccountListPanel.Visible = false;
            depositPanel.Visible = true;
            depositText.Text = transactionAmount;
        }

        private void depositEnter_Click(object sender, EventArgs e)
        {

            //gets current time and last trans time
            DateTime date1 = Convert.ToDateTime(accountList.ElementAt(depositAccountList.SelectedIndex).lastTransactionDate);
            DateTime date2 = DateTime.Now;

            //checks to see if its the same day or not
            if (DateTime.Compare(date1.Date, date2.Date) != 0)
            {
                accountList.ElementAt(depositAccountList.SelectedIndex).transactionTotal = 0;
            }
            //checks to see if the amount text is empty
            if (string.IsNullOrEmpty(depositText.Text))
                transactionAmount = "0";

            //checks to see if the the transaction total is less than 3000
            if (accountList.ElementAt(depositAccountList.SelectedIndex).transactionTotal + Int32.Parse(transactionAmount) <= 3000)
            {
                conn.Open();
                int newBalance = accountList.ElementAt(depositAccountList.SelectedIndex).balance += Int32.Parse(transactionAmount);
                int accountNumber = accountList.ElementAt(depositAccountList.SelectedIndex).accountNumber;
                int transTotal = accountList.ElementAt(depositAccountList.SelectedIndex).transactionTotal += Int32.Parse(transactionAmount);

                //updates the DB 
                string sql = "UPDATE morgan_h_account SET balance=@balance, transTotal=@transTotal, lastTransDate=@transDate WHERE accountNumber=@accNumber";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@balance", newBalance);
                cmd.Parameters.AddWithValue("@transDate", DateTime.Now.ToString());
                cmd.Parameters.AddWithValue("@transTotal", transTotal);
                cmd.Parameters.AddWithValue("@accNumber", accountNumber);
                cmd.ExecuteNonQuery();


                sql = "INSERT INTO morgan_h_transaction (accountNumber, transDate, transType, customerId, amount) VALUES " +
                    "(@accNumber, @transDate, @transType, @customerId, @amount )";
                cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@accNumber", accountNumber);
                cmd.Parameters.AddWithValue("@transDate", DateTime.Now.ToString());
                cmd.Parameters.AddWithValue("@transType", "Deposit");
                cmd.Parameters.AddWithValue("@customerId", customer.customerID);
                cmd.Parameters.AddWithValue("@amount", transactionAmount);
                cmd.ExecuteNonQuery();
                conn.Close();

                depositPanel.Visible = false;
                checkBalancePanel.Visible = true;
                accountTextBox.Text = accountList.ElementAt(depositAccountList.SelectedIndex).accountNumber.ToString();
                checkBalanceText.Text = $"${ accountList.ElementAt(depositAccountList.SelectedIndex).balance}";
            }
            else
            {
                string message = "Reached transaction maximum, please try again tomorrow";
                MessageBox.Show(message);
            }

        }

        private void withdrawBtn_Click(object sender, EventArgs e)
        {
            homePanel.Visible = false;
            withdrawAccountLlistPanel.Visible = true;
            transactionAmount = "";
            accountList.Clear();

            conn.Open();
            string sql = "SELECT accountNumber, balance, lastTransDate, transTotal FROM morgan_h_account WHERE customerId=@id";
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            //creates account list
            cmd.Parameters.AddWithValue("@id", customer.customerID);
            MySqlDataReader myReader = cmd.ExecuteReader();
            while (myReader.Read())
            {
                accountList.Add(new Account(Int32.Parse(myReader["accountNumber"].ToString()), Int32.Parse(myReader["balance"].ToString()), myReader["lastTransDate"].ToString(), Int32.Parse(myReader["transTotal"].ToString())));

            }
            myReader.Close();
            //adds list to the screen
            withdrawAccountList.Items.Clear();
            foreach (Account acc in accountList)
            {
                if (!withdrawAccountList.Items.Contains(acc))
                {
                    withdrawAccountList.Items.Add(acc.accountNumber);
                }
            }
            conn.Close();
        }

        private void withdrawAccountListReturnBtn_Click(object sender, EventArgs e)
        {
            homePanel.Visible = true;
            withdrawAccountLlistPanel.Visible = false;
        }

        private void withdrawAccountListSelectBtn_Click(object sender, EventArgs e)
        {
            withdrawAccountLlistPanel.Visible = false;
            withdrawPanel.Visible = true;
            withdrawText.Text = transactionAmount;
        }

        private void withdrawEnter_Click(object sender, EventArgs e)
        {
            //get current time
            DateTime date1 = Convert.ToDateTime(accountList.ElementAt(withdrawAccountList.SelectedIndex).lastTransactionDate);
            DateTime date2 = DateTime.Now;
            //checks if its the same day
            if (DateTime.Compare(date1.Date, date2.Date) != 0)
            {
                accountList.ElementAt(withdrawAccountList.SelectedIndex).transactionTotal = 0;
            }
            if (string.IsNullOrEmpty(withdrawText.Text))
                transactionAmount = "0";
            //check to see if the transaction is less that 3000
            if (accountList.ElementAt(withdrawAccountList.SelectedIndex).transactionTotal + Int32.Parse(transactionAmount) <= 3000)
            {
                //check the balance to see if the account has the money
                if (accountList.ElementAt(withdrawAccountList.SelectedIndex).balance >= Int32.Parse(transactionAmount))
                {
                    conn.Open();
                    int newBalance = accountList.ElementAt(withdrawAccountList.SelectedIndex).balance -= Int32.Parse(transactionAmount);
                    int accountNumber = accountList.ElementAt(withdrawAccountList.SelectedIndex).accountNumber;
                    int transTotal = accountList.ElementAt(withdrawAccountList.SelectedIndex).transactionTotal += Int32.Parse(transactionAmount);

                    //updating the DB
                    string sql = "UPDATE morgan_h_account SET balance=@balance, transTotal=@transTotal, lastTransDate=@transDate WHERE accountNumber=@accNumber";
                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@balance", newBalance);
                    cmd.Parameters.AddWithValue("@transDate", DateTime.Now.ToString());
                    cmd.Parameters.AddWithValue("@transTotal", transTotal);
                    cmd.Parameters.AddWithValue("@accNumber", accountNumber);
                    cmd.ExecuteNonQuery();

                    sql = "INSERT INTO morgan_h_transaction (accountNumber, transDate, transType, customerId, amount) VALUES " +
                        "(@accNumber, @transDate, @transType, @customerId, @amount )";
                    cmd = new MySqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@accNumber", accountNumber);
                    cmd.Parameters.AddWithValue("@transDate", DateTime.Now.ToString());
                    cmd.Parameters.AddWithValue("@transType", "Withdraw");
                    cmd.Parameters.AddWithValue("@customerId", customer.customerID);
                    cmd.Parameters.AddWithValue("@amount", "-" + transactionAmount);
                    cmd.ExecuteNonQuery();

                    withdrawPanel.Visible = false;
                    checkBalancePanel.Visible = true;
                    accountTextBox.Text = accountList.ElementAt(withdrawAccountList.SelectedIndex).accountNumber.ToString();
                    checkBalanceText.Text = $"${ accountList.ElementAt(withdrawAccountList.SelectedIndex).balance}";
                    conn.Close();
                }
                else
                {
                    string message = "Insufficient funds, please enter a lower amount.";
                    MessageBox.Show(message);
                }
            }
            else
            {
                string message = "Reached transaction maximum, please try again tomorrow";
                MessageBox.Show(message);
            }
        }

        private void depositReturn_Click(object sender, EventArgs e)
        {
            depositPanel.Visible = false;
            depositAccountListPanel.Visible = true;
        }

        private void withdrawReturn_Click(object sender, EventArgs e)
        {
            withdrawPanel.Visible = false;
            withdrawAccountLlistPanel.Visible = true;
        }

        private void transferMoneyBtn_Click(object sender, EventArgs e)
        {
            homePanel.Visible = false;
            transferFromPanel.Visible = true;
            transactionAmount = "";
            accountList.Clear();

            conn.Open();
            string sql = "SELECT accountNumber, balance, lastTransDate, transTotal FROM morgan_h_account WHERE customerId=@id";
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            //list of accounts to transfer from
            cmd.Parameters.AddWithValue("@id", customer.customerID);
            MySqlDataReader myReader = cmd.ExecuteReader();
            while (myReader.Read())
            {
                accountList.Add(new Account(Int32.Parse(myReader["accountNumber"].ToString()), Int32.Parse(myReader["balance"].ToString()), myReader["lastTransDate"].ToString(), Int32.Parse(myReader["transTotal"].ToString())));

            }
            myReader.Close();

            transferFromList.Items.Clear();
            foreach (Account acc in accountList)
            {
                if (!transferFromList.Items.Contains(acc))
                {
                    transferFromList.Items.Add(acc.accountNumber);
                }
            }
            conn.Close();
        }

        private void transferFromSelect_Click(object sender, EventArgs e)
        {
            transferFromPanel.Visible = false;
            transferPanel.Visible = true;
            transferAmountText.Text = transactionAmount;
        }

        private void transferReturn_Click(object sender, EventArgs e)
        {
            transferFromPanel.Visible = true;
            transferPanel.Visible = false;
        }

        private void transferFromReturn_Click(object sender, EventArgs e)
        {
            transferFromPanel.Visible = false;
            homePanel.Visible = true;
        }

        private void depositAccountListPanel_Paint(object sender, PaintEventArgs e)
        {

        }

        private void transferEnter_Click(object sender, EventArgs e)
        {
            
            DateTime date1 = Convert.ToDateTime(accountList.ElementAt(transferFromList.SelectedIndex).lastTransactionDate);
            DateTime date2 = DateTime.Now;

            if (DateTime.Compare(date1.Date, date2.Date) != 0)
            {
                accountList.ElementAt(transferFromList.SelectedIndex).transactionTotal = 0;
            }
            if (string.IsNullOrEmpty(transferAmountText.Text))
                transactionAmount = "0";
            //checks to see if the account selected had the amount and if the total is under 3000
            if (accountList.ElementAt(transferFromList.SelectedIndex).transactionTotal + Int32.Parse(transactionAmount) <= 3000)
            {
                if (accountList.ElementAt(transferFromList.SelectedIndex).balance >= Int32.Parse(transactionAmount))
                {
                    conn.Open();
                    int newBalance = accountList.ElementAt(transferFromList.SelectedIndex).balance -= Int32.Parse(transactionAmount);
                    int accountNumber = accountList.ElementAt(transferFromList.SelectedIndex).accountNumber;
                    int transTotal = accountList.ElementAt(transferFromList.SelectedIndex).transactionTotal += Int32.Parse(transactionAmount);
                    //updating db
                    string sql = "UPDATE morgan_h_account SET balance=@balance, transTotal=@transTotal, lastTransDate=@transDate WHERE accountNumber=@accNumber";
                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@balance", newBalance);
                    cmd.Parameters.AddWithValue("@transDate", DateTime.Now.ToString());
                    cmd.Parameters.AddWithValue("@transTotal", transTotal);
                    cmd.Parameters.AddWithValue("@accNumber", accountNumber);
                    cmd.ExecuteNonQuery();


                    sql = "INSERT INTO morgan_h_transaction (accountNumber, transDate, transType, customerId, amount) VALUES " +
                    "(@accNumber, @transDate, @transType, @customerId, @amount )";
                    cmd = new MySqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@accNumber", accountNumber);
                    cmd.Parameters.AddWithValue("@transDate", DateTime.Now.ToString());
                    cmd.Parameters.AddWithValue("@transType", "Transfer");
                    cmd.Parameters.AddWithValue("@customerId", customer.customerID);
                    cmd.Parameters.AddWithValue("@amount", "-" + transactionAmount);
                    cmd.ExecuteNonQuery();

                    transferToPanel.Visible = true;
                    transferPanel.Visible = false;
                    accountList.RemoveAt(transferFromList.SelectedIndex);

                    transferToList.Items.Clear();
                    foreach (Account acc in accountList)
                    {
                        if (!transferToList.Items.Contains(acc))
                        {
                            transferToList.Items.Add(acc.accountNumber);
                        }
                    }

                    conn.Close();
                }
                else
                {
                    string message = "Insufficient funds, please enter a lower amount.";
                    MessageBox.Show(message);
                }
            }
            else
            {
                string message = "Reached transaction maximum, please try again tomorrow";
                MessageBox.Show(message);
            }

        }

        private void transferToSelect_Click(object sender, EventArgs e)
        {
            DateTime date1 = Convert.ToDateTime(accountList.ElementAt(transferToList.SelectedIndex).lastTransactionDate);
            DateTime date2 = DateTime.Now;

            if (DateTime.Compare(date1.Date, date2.Date) != 0)
            {
                accountList.ElementAt(transferToList.SelectedIndex).transactionTotal = 0;
            }
            if (string.IsNullOrEmpty(transferAmountText.Text))
                transactionAmount = "0";
            //check to make sure the transfer to account total is under 3000
            if (accountList.ElementAt(transferToList.SelectedIndex).transactionTotal + Int32.Parse(transactionAmount) <= 3000)
            {
                conn.Open();
                int newBalance = accountList.ElementAt(transferToList.SelectedIndex).balance += Int32.Parse(transactionAmount);
                int accountNumber = accountList.ElementAt(transferToList.SelectedIndex).accountNumber;
                int transTotal = accountList.ElementAt(transferToList.SelectedIndex).transactionTotal += Int32.Parse(transactionAmount);
                //updating db
                string sql = "UPDATE morgan_h_account SET balance=@balance, transTotal=@transTotal, lastTransDate=@transDate WHERE accountNumber=@accNumber";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@balance", newBalance);
                cmd.Parameters.AddWithValue("@transDate", DateTime.Now.ToString());
                cmd.Parameters.AddWithValue("@transTotal", transTotal);
                cmd.Parameters.AddWithValue("@accNumber", accountNumber);
                cmd.ExecuteNonQuery();

                sql = "INSERT INTO morgan_h_transaction (accountNumber, transDate, transType, customerId, amount) VALUES " +
                    "(@accNumber, @transDate, @transType, @customerId, @amount )";
                cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@accNumber", accountNumber);
                cmd.Parameters.AddWithValue("@transDate", DateTime.Now.ToString());
                cmd.Parameters.AddWithValue("@transType", "Transfer");
                cmd.Parameters.AddWithValue("@customerId", customer.customerID);
                cmd.Parameters.AddWithValue("@amount", transactionAmount);
                cmd.ExecuteNonQuery();
                conn.Close();

                transferToPanel.Visible = false;
                checkBalancePanel.Visible = true;
                accountTextBox.Text = accountList.ElementAt(transferFromList.SelectedIndex).accountNumber.ToString();
                checkBalanceText.Text = $"${ accountList.ElementAt(transferFromList.SelectedIndex).balance}";
            }
            else
            {
                string message = "Reached transaction maximum, please try again tomorrow";
                MessageBox.Show(message);
            }

        }
        //***-------------------------------------------------Everything below this line is keypad button commands-------------------------***
        private void transfer1_Click(object sender, EventArgs e)
        {
            transactionAmount += "1";
            transferFromSelect_Click(sender, e);
        }

        private void transfer2_Click(object sender, EventArgs e)
        {
            transactionAmount += "2";
            transferFromSelect_Click(sender, e);
        }

        private void transfer3_Click(object sender, EventArgs e)
        {
            transactionAmount += "3";
            transferFromSelect_Click(sender, e);
        }

        private void transfer4_Click(object sender, EventArgs e)
        {
            transactionAmount += "4";
            transferFromSelect_Click(sender, e);
        }

        private void transfer5_Click(object sender, EventArgs e)
        {
            transactionAmount += "5";
            transferFromSelect_Click(sender, e);
        }

        private void transfer6_Click(object sender, EventArgs e)
        {
            transactionAmount += "6";
            transferFromSelect_Click(sender, e);
        }

        private void transfer7_Click(object sender, EventArgs e)
        {
            transactionAmount += "7";
            transferFromSelect_Click(sender, e);
        }

        private void transfer8_Click(object sender, EventArgs e)
        {
            transactionAmount += "8";
            transferFromSelect_Click(sender, e);
        }

        private void transfer9_Click(object sender, EventArgs e)
        {
            transactionAmount += "9";
            transferFromSelect_Click(sender, e);
        }

        private void transfer0_Click(object sender, EventArgs e)
        {
            transactionAmount += "0";
            transferFromSelect_Click(sender, e);
        }

        private void transferClr_Click(object sender, EventArgs e)
        {
            transactionAmount = "";
            transferFromSelect_Click(sender, e);
        }

        private void transferDel_Click(object sender, EventArgs e)
        {
            transactionAmount = transactionAmount.Substring(0, transactionAmount.Length - 1);
            transferFromSelect_Click(sender, e);
        }
        private void withdraw1_Click(object sender, EventArgs e)
        {
            transactionAmount += "1";
            withdrawAccountListSelectBtn_Click(sender, e);
        }

        private void withdraw2_Click(object sender, EventArgs e)
        {
            transactionAmount += "2";
            withdrawAccountListSelectBtn_Click(sender, e);
        }

        private void withdraw3_Click(object sender, EventArgs e)
        {
            transactionAmount += "3";
            withdrawAccountListSelectBtn_Click(sender, e);
        }

        private void withdraw4_Click(object sender, EventArgs e)
        {
            transactionAmount += "4";
            withdrawAccountListSelectBtn_Click(sender, e);
        }

        private void withdraw5_Click(object sender, EventArgs e)
        {
            transactionAmount += "5";
            withdrawAccountListSelectBtn_Click(sender, e);
        }

        private void withdraw6_Click(object sender, EventArgs e)
        {
            transactionAmount += "6";
            withdrawAccountListSelectBtn_Click(sender, e);
        }

        private void withdraw7_Click(object sender, EventArgs e)
        {
            transactionAmount += "7";
            withdrawAccountListSelectBtn_Click(sender, e);
        }

        private void withdraw8_Click(object sender, EventArgs e)
        {
            transactionAmount += "8";
            withdrawAccountListSelectBtn_Click(sender, e);
        }

        private void withdraw9_Click(object sender, EventArgs e)
        {
            transactionAmount += "9";
            withdrawAccountListSelectBtn_Click(sender, e);
        }

        private void withdraw0_Click(object sender, EventArgs e)
        {
            transactionAmount += "0";
            withdrawAccountListSelectBtn_Click(sender, e);
        }

        private void withdrawClr_Click(object sender, EventArgs e)
        {
            transactionAmount = "";
            withdrawAccountListSelectBtn_Click(sender, e);
        }

        private void withdrawDel_Click(object sender, EventArgs e)
        {
            transactionAmount = transactionAmount.Substring(0, transactionAmount.Length - 1);
            withdrawAccountListSelectBtn_Click(sender, e);
        }

        private void depositPanel_Paint(object sender, PaintEventArgs e)
        {

        }
        private void deposit1_Click(object sender, EventArgs e)
        {
            transactionAmount += "1";
            depositAccountListSelect_Click(sender, e);
        }

        private void deposit2_Click(object sender, EventArgs e)
        {
            transactionAmount += "2";
            depositAccountListSelect_Click(sender, e);
        }

        private void deposit3_Click(object sender, EventArgs e)
        {
            transactionAmount += "3";
            depositAccountListSelect_Click(sender, e);
        }

        private void deposit4_Click(object sender, EventArgs e)
        {
            transactionAmount += "4";
            depositAccountListSelect_Click(sender, e);
        }

        private void deposit5_Click(object sender, EventArgs e)
        {
            transactionAmount += "5";
            depositAccountListSelect_Click(sender, e);
        }

        private void deposit6_Click(object sender, EventArgs e)
        {
            transactionAmount += "6";
            depositAccountListSelect_Click(sender, e);
        }

        private void deposit7_Click(object sender, EventArgs e)
        {
            transactionAmount += "7";
            depositAccountListSelect_Click(sender, e);
        }

        private void deposit8_Click(object sender, EventArgs e)
        {
            transactionAmount += "8";
            depositAccountListSelect_Click(sender, e);
        }

        private void deposit9_Click(object sender, EventArgs e)
        {
            transactionAmount += "9";
            depositAccountListSelect_Click(sender, e);
        }

        private void deposit0_Click(object sender, EventArgs e)
        {
            transactionAmount += "0";
            depositAccountListSelect_Click(sender, e);
        }

        private void depositClr_Click(object sender, EventArgs e)
        {
            transactionAmount = "";
            depositAccountListSelect_Click(sender, e);
        }

        private void depositDel_Click(object sender, EventArgs e)
        {
            transactionAmount = transactionAmount.Substring(0, transactionAmount.Length - 1);
            depositAccountListSelect_Click(sender, e);
        }

        private void loginPanel_Paint(object sender, PaintEventArgs e)
        {

        }

        private void button1_Click(object sender, PaintEventArgs e)
        {
            customerPin += "1";
            startBtn_Click(sender, e);
        }

        private void startBtn_Click(object sender, EventArgs e)
        {
            startPanel.Visible = false;
            loginPanel.Visible = true;
            pinText.Text = customerPin;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            customerPin += "2";
            startBtn_Click(sender, e);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            customerPin += "3";
            startBtn_Click(sender, e);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            customerPin += "4";
            startBtn_Click(sender, e);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            customerPin += "5";
            startBtn_Click(sender, e);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            customerPin += "6";
            startBtn_Click(sender, e);
        }

        private void button7_Click(object sender, EventArgs e)
        {
            customerPin += "7";
            startBtn_Click(sender, e);
        }

        private void button8_Click(object sender, EventArgs e)
        {
            customerPin += "8";
            startBtn_Click(sender, e);
        }

        private void button9_Click(object sender, EventArgs e)
        {
            customerPin += "9";
            startBtn_Click(sender, e);
        }

        private void button10_Click(object sender, EventArgs e)
        {
            customerPin += "0";
            startBtn_Click(sender, e);
        }

        private void buttonClr_Click(object sender, EventArgs e)
        {
            customerPin = "";
            startBtn_Click(sender, e);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            customerPin += "1";
            startBtn_Click(sender, e);
        }

        private void transferToReturn_Click(object sender, EventArgs e)
        {
            transferToPanel.Visible = false;
            transferPanel.Visible = true;
        }
    }

}
