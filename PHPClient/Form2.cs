using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PHPClient
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }

        public MySqlConnection connection;
        public string server;
        public string uid;
        public string pass;
        public string db;
        public List<String> saleIds;
        public List<string> quantities;
        public List<string> stockIds;
        public List<string> totals;
        public List<string> dates;
        private void Form2_Load(object sender, EventArgs e)
        {
            string connectionString;
            server = "dbAddress"; //feenixmariadb
            uid = "uID"; //513462101s
            pass = "password"; //794061
            db = "dbName"; //bd_513462101s

            connectionString = "SERVER=" + server + ";" + "DATABASE=" +
                 db + ";" + "UID=" + uid + ";" + "PASSWORD=" + pass + ";";

            connection = new MySqlConnection(connectionString);

            saleIds = new List<string>();
            stockIds = new List<string>();
            quantities = new List<string>();
            totals = new List<string>();
            dates = new List<string>();

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
            string query = "Select * from Sale";
            MySqlCommand cmd = new MySqlCommand(query, connection);
            MySqlDataReader dataReader = cmd.ExecuteReader();
            while (dataReader.Read())
            {
                saleIds.Add(dataReader["saleId"] + "");
                stockIds.Add(dataReader["stockId"] + "");
                quantities.Add(dataReader["quantity"] + "");
                totals.Add(dataReader["total"] + "");
                var datetime = dataReader["date"] + "";
                dates.Add(datetime.ToString());
            }
            dataReader.Close();
            connection.Close();
        }

        public void PopulateForm()
        {
            foreach (string s in saleIds)
            {
                lbSalesID.Items.Add(s);
            }
        }

        private void LbSalesID_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lbSalesID.SelectedIndex != -1)
            {
                lblStockID.Text = stockIds[lbSalesID.SelectedIndex];
                lblQuantity.Text = quantities[lbSalesID.SelectedIndex];
                lblTotal.Text = "$" + totals[lbSalesID.SelectedIndex];
                lblDate.Text = DateTime.Parse(dates[lbSalesID.SelectedIndex]).ToString("yyyy-MM-dd");
                    //;

            }
        }
    }
}
