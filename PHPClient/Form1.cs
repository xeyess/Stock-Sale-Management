using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PHPClient
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void LblN_Click(object sender, EventArgs e)
        {

        }

        private void Panel1_Paint(object sender, PaintEventArgs e)
        {

        }
        public MySqlConnection connection;
        public string server;
        public string uid;
        public string pass;
        public string db;
        public List<string> stockIds;
        public List<string> names;
        public List<string> prices;
        public List<string> quantities;
        public List<string> categories;
        private void Form1_Load(object sender, EventArgs e)
        {
            string connectionString;
            server = "dbAddress"; //feenixmariadb
            uid = "uID"; //513462101s
            pass = "password"; //794061
            db= "dbName"; //bd_513462101s

            connectionString = "SERVER=" + server + ";" + "DATABASE=" +
                 db + ";" + "UID=" + uid + ";" + "PASSWORD=" + pass + ";";

            connection = new MySqlConnection(connectionString);

            stockIds = new List<string>();
            names = new List<string>();
            prices = new List<string>();
            quantities = new List<string>();
            categories = new List<string>();

            try
            {
                ConnectToDb();
            }
            catch
            {
                MessageBox.Show("An error has occured. Try again.");
            }
            PopulateForm();
        }

        public void ConnectToDb()
        {
            connection.Open();
            string query = "Select * from Stock";
            MySqlCommand cmd = new MySqlCommand(query, connection);
            MySqlDataReader dataReader = cmd.ExecuteReader();
            while (dataReader.Read())
            {
                stockIds.Add(dataReader["stockId"] + "");
                names.Add(dataReader["name"] + "");
                prices.Add(dataReader["price"] + "");
                quantities.Add(dataReader["quantity"] + "");
                categories.Add(dataReader["category"] + "");
            }
            dataReader.Close();
            connection.Close();
        }

        public void PopulateForm()
        {
            foreach (string s in stockIds)
            {
                lbStockIds.Items.Add(s);
            }

            cbCategory.Items.Add("All");
            foreach (string a in categories)
            {
                if(!cbCategory.Items.Contains(a))
                {
                    cbCategory.Items.Add(a);
                }
            }

        }

        public void RefreshForm()
        {
            lbStockIds.Items.Clear();
            cbCategory.Items.Clear();
            cbCategory.Text = "All";

            stockIds.Clear();
            names.Clear();
            prices.Clear();
            quantities.Clear();
            categories.Clear();

            ConnectToDb();
            this.Refresh();
            this.Update();
            System.Threading.Thread.Sleep(100);
            this.Refresh();
            this.Update();
            PopulateForm();
            //Application.Restart();

        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            RefreshForm();
        }

        public int GetIndexFromLb(string id)
        {
            int i = 0;
            foreach(string s in lbStockIds.Items)
            {
                if(s == id)
                {
                    return i;
                }
                i++;
            }
            return -1;
        }

        public int GetIndexFromList(string id)
        {
            int i = 0;
            foreach(string s in stockIds)
            {
                if(s == id)
                {
                    return i;
                }
                i++;
            }
            return -1;
        }


        private void LbStockIds_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lbStockIds.SelectedIndex != -1)
            {
                string id = lbStockIds.Items[lbStockIds.SelectedIndex].ToString();
                lblName.Text = names[GetIndexFromList(id)];
                lblPrice.Text = prices[GetIndexFromList(id)];
                lblQuantity.Text = quantities[GetIndexFromList(id)];
                lblCategory.Text = categories[GetIndexFromList(id)];
                txtStockID.Text = id;
                txtQuantity.ReadOnly = false;

                if (txtQuantity.Text != String.Empty)
                {
                    try
                    {
                        GetTotal();
                    }
                    catch
                    {

                    }
                }
            }
        }

        public bool ValidSaleCheck()
        {
            if(stockIds.Contains(txtStockID.Text))
            {
                try
                {
                    int qtty = Convert.ToInt32(txtQuantity.Text);
                    return true;
                }
                catch
                {
                    MessageBox.Show("Quantity was not an integer, please enter an integer.");
                    txtQuantity.Text = String.Empty;
                    return false;
                }
            }
            return false;
        }

        public double CalculateTotal()
        {
            double result = 0;
            int i;
            for(i = 0; i < stockIds.Count; i++)
            {
                if(stockIds[i] == txtStockID.Text)
                {
                    break;
                }
            }
            result = Convert.ToDouble(prices[i]) * Convert.ToDouble(txtQuantity.Text);
            return result;
        }

        private void TxtStockID_TextChanged(object sender, EventArgs e)
        { 
        }

        public void GetTotal()
        {
            if (ValidSaleCheck())
            {
                lblTotal.Text = "$" + CalculateTotal().ToString();
            }
        }

        private void TxtQuantity_TextChanged(object sender, EventArgs e)
        {
            GetTotal();
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            if(ValidSaleCheck())
            {
                int main = Convert.ToInt32(lblQuantity.Text);
                int sub = Convert.ToInt32(txtQuantity.Text);
                if((Convert.ToInt32(lblQuantity.Text) > 0 && main >= sub))
                {
                    MakeSale();
                    RemoveStock();
                    MessageBox.Show("Checkout successful!");
                    RefreshForm();
                  
                }
                else
                {
                    MessageBox.Show("Not enough of stock!");
                }
            }
        }

        public void RemoveStock()
        {
            connection.Open();
            int newQuantity = Convert.ToInt32(lblQuantity.Text) - Convert.ToInt32(txtQuantity.Text);
            string query = "UPDATE Stock SET quantity='" + newQuantity.ToString() +"' WHERE stockId='"+ txtStockID.Text +"'";

            MySqlCommand cmd = new MySqlCommand(query, connection);

            cmd.ExecuteNonQuery();
            connection.Close();
        }

        public void MakeSale()
        {
            try
            {
                connection.Open();
                DateTime today = DateTime.Today;
                string date = today.ToString("yyyy-MM-dd");

                string query = "INSERT INTO Sale (stockId, quantity, total, date) VALUES('" + txtStockID.Text + "', '" + txtQuantity.Text + "', '" + CalculateTotal().ToString() + "', '" + date + "')";

                MySqlCommand cmd = new MySqlCommand(query, connection);

                cmd.ExecuteNonQuery();
                connection.Close();
            }
            catch
            {
                MessageBox.Show("A error has occured. Please try again.");
            }
        }

        private void BtnSalesView_Click(object sender, EventArgs e)
        {
            Form2 fm2 = new Form2();
            fm2.Show();
        }

        private void label9_Click(object sender, EventArgs e)
        {

        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            string searchString = txtSearch.Text;
            int i = 0;
            foreach (string s in names)
            {
                if(s.Contains(txtSearch.Text))
                {
                    int j = 0;
                    foreach(string a in lbStockIds.Items)
                    {
                        if(a == stockIds[i])
                        {
                            lbStockIds.SelectedIndex = j;
                            break;
                        }
                        j++;
                    }
                    //lbStockIds.SelectedIndex = i;
                    break;
                }
                i++;
            }
        }

        private void btnFilter_Click(object sender, EventArgs e)
        {
            string categ = cbCategory.Text;
            if(categ == "All")
            {
                btnRefresh.PerformClick();
            }
            else if(categ == "")
            {

            }
            else if(cbCategory.Items.Contains(categ))
            {
                lbStockIds.Items.Clear();
                int i = 0;
                foreach(string s in stockIds)
                {
                    if(categories[i] == categ)
                    {
                        lbStockIds.Items.Add(s);
                    }
                    i++;
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Weekly frm3 = new Weekly();
            
            frm3.Show();
        }

        private void btnMonthly_Click(object sender, EventArgs e)
        {
            Monthly frm4 = new Monthly();

            frm4.Show();
        }
    }
}
