using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PHPClient
{
    public partial class Weekly : Form
    {
        public Weekly()
        {
            InitializeComponent();
        }


        //stock
        public List<string> stockIds;
        public List<string> names;
        public List<string> prices;
        public List<string> quantities;
        public List<string> categories;

        //sale
        public List<string> saleIds;
        public List<string> saleQuantities;
        public List<string> purchaseIds;
        public List<string> totals;
        public List<string> dates;

        string server;
        string uid;
        string pass;
        string db;
        public MySqlConnection connection;

        //analysis
        string currentDate;
        string currentWeek;
        string currentMonth;
        string currentYear;

        int soldQuantity;
        int saleQuantity;

        int outOfStockNo;
        List<string> outOfStockItems;
        List<string> mostCommonItems;

        int mostCommonValue;
        List<string> leastCommonItems;
        int leastCommonValue;
        string mostExpensiveBought;
        double mostExpensiveValue;
        string leastExpensiveBought;
        double leastExpensiveValue;
        double totalRev;

        List<int> categoryIndex;
        string bestCategory;

        public int getWeek()
        {
            DateTime now = DateTime.Now;
            DateTime month = new DateTime(now.Year, now.Month, 1);
            DateTime firstMonthMonday = month.AddDays((DayOfWeek.Monday + 7 - month.DayOfWeek) % 7);
            if (firstMonthMonday > now)
            {
                month = month.AddMonths(-1);
                firstMonthMonday = month.AddDays((DayOfWeek.Monday + 7 - month.DayOfWeek) % 7);
            }
            return (now - firstMonthMonday).Days / 7 + 1;
        }

        public List<DateTime> getCurrentWeek()
        {
            List<DateTime> weekIndex = new List<DateTime>();
            DayOfWeek day = DateTime.Now.DayOfWeek;
            int days = day - DayOfWeek.Monday;
            DateTime start = DateTime.Now.AddDays(-days);
            DateTime end = start.AddDays(6);
            weekIndex.Add(start);
            weekIndex.Add(end);
            return weekIndex;
        }

        public string getNameFromStockId(string id)
        {
            string result = null;
            int i = 0;
            foreach (string s in stockIds)
            {
                if (s == id)
                {
                    result = names[i];
                    break;
                }
                i++;
            }
            return result;
        }

        public string getCategoryFromStockId(string id)
        {
            string result = null;
            int i = 0;
            foreach (string s in stockIds)
            {
                if (s == id)
                {
                    result = categories[i];
                    break;
                }
                i++;
            }
            return result;
        }

        public List<String> GetCategories()
        {
            List<string> result = new List<string>();
            foreach (string s in categories)
            {
                if (!result.Contains(s))
                {
                    result.Add(s);
                }
            }
            return result;
        }

        public void BestCategorySet(int id)
        {

            string categoryFound = getCategoryFromStockId(purchaseIds[id]);
            //int i = 0;
            int j = 0;
            foreach (string t in GetCategories())
            {
                if (categoryFound == t)
                {
                    categoryIndex[j] += Convert.ToInt32(saleQuantities[id]);
                    break;
                }
                j++;
            }
            //i++;

        }

        List<string> itemIndex = new List<string>();
        List<int> itemValueIndex = new List<int>();
        public void getLeastAndMostCommons(int id)//all
        {
            if (itemIndex.Contains(purchaseIds[id]))
            {
                int index = 0;
                foreach (string item in itemIndex)
                {
                    if (item == purchaseIds[id])
                    {
                        break;
                    }
                    index++;
                }
                itemValueIndex[index] += Convert.ToInt32(saleQuantities[id]);
            }
            else
            {
                itemIndex.Add(purchaseIds[id]);
                itemValueIndex.Add(Convert.ToInt32(saleQuantities[id]));
                itemTotals.Add(Convert.ToDouble(totals[id])); //totals
            }

            int finder = 0;
            foreach (int val in itemValueIndex)
            {
                if (mostCommonValue < val)
                {
                    mostCommonItems.Clear();
                    mostCommonItems.Add(getNameFromStockId(itemIndex[finder]));
                    mostCommonValue = val;
                }
                else if (mostCommonValue == val)
                {
                    if (!mostCommonItems.Contains(getNameFromStockId(itemIndex[finder])))
                    {
                        mostCommonItems.Add(getNameFromStockId(itemIndex[finder]));
                    }
                }

                if (leastCommonValue > val)
                {
                    leastCommonItems.Clear();
                    leastCommonItems.Add(getNameFromStockId(itemIndex[finder]));
                    leastCommonValue = val;
                }
                else if (leastCommonValue == val)
                {
                    if (!leastCommonItems.Contains(getNameFromStockId(itemIndex[finder])))
                    {
                        leastCommonItems.Add(getNameFromStockId(itemIndex[finder]));
                    }
                }
                finder++;
            }

        }

        List<double> itemTotals = new List<double>();
        public void getLeastAndMostTotal(int id)
        {
            if (itemIndex.Contains(purchaseIds[id]))
            {
                int index = 0;
                foreach (string item in itemIndex)
                {
                    if (item == purchaseIds[id])
                    {
                        break;
                    }
                    index++;
                }
                itemTotals[index] += Convert.ToDouble(totals[id]);
            }

            int finder = 0;
            foreach (int val in itemTotals)
            {
                if (mostExpensiveValue < val)
                {
                    mostExpensiveBought = getNameFromStockId(itemIndex[finder]);
                    mostExpensiveValue = val;
                }
                else if (mostExpensiveValue == val)
                {
                    if (mostExpensiveBought != getNameFromStockId(itemIndex[finder]))
                    {
                        mostExpensiveBought += " + " + getNameFromStockId(itemIndex[finder]);
                    }
                }

                if (leastExpensiveValue > val)
                {
                    leastExpensiveBought = getNameFromStockId(itemIndex[finder]);
                    leastExpensiveValue = val;
                }
                else if (leastExpensiveValue == val)
                {
                    if (leastExpensiveBought != getNameFromStockId(itemIndex[finder]))
                    {
                        leastExpensiveBought += " + " + getNameFromStockId(itemIndex[finder]);
                    }
                }
                finder++;
            }

        }

        public void getData(int id)
        {
            soldQuantity = soldQuantity + Convert.ToInt32(saleQuantities[id]);
            saleQuantity++;
            getLeastAndMostTotal(id);
            getLeastAndMostCommons(id);
            BestCategorySet(id);


        }

        public void StockAnalyse()
        {
            int i = 0;
            foreach (string s in quantities)
            {
                if (Convert.ToInt32(s) == 0)
                {
                    outOfStockNo++;
                    outOfStockItems.Add(names[i]);
                }
                i++;
            }
        }

        public void Analyse() //oops month
        {
            DateTime today = DateTime.Now;
            

            currentDate = today.ToString();
            currentWeek = "Week " + getWeek().ToString();
            currentYear = today.Year.ToString();
            currentMonth = DateTime.Now.Month.ToString();

            soldQuantity = 0;
            saleQuantity = 0;

            outOfStockNo = 0;
            outOfStockItems = new List<string>();

            mostCommonItems = new List<string>();
            mostCommonValue = 0;
            leastCommonItems = new List<string>();
            leastCommonValue = 999;

            mostExpensiveValue = 0;
            leastExpensiveValue = 9999;

            InitialiseCategoryIndex();

            int id = 0;
            foreach (string s in dates)
            {
                DateTime date = DateTime.Parse(s);
                List<DateTime> dateIndex = getCurrentWeek();
                if (date >= dateIndex[0] && date <= today)
                {
                    getData(id);
                }
                id++;
            }
            StockAnalyse();


            //DateTime.Now.Month.ToString();
        }

        int max;
        private void Output()
        {
            string text = "";
            text += ("Report as of " + CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(DateTime.Now.Month) + " " + currentYear) + Environment.NewLine;
            text += (currentWeek + Environment.NewLine);
            text += ("Created on " + DateTime.Today.ToString("dd/MM/yyyy") + Environment.NewLine);
            text += ("Total Sales: " + saleQuantity + Environment.NewLine);
            text += ("Total Items Sold: " + soldQuantity + Environment.NewLine);
            text += ("Items Out of Stock: " + outOfStockNo + Environment.NewLine);
            foreach (string outofstock in outOfStockItems)
            {
                text += ("  " + outofstock + Environment.NewLine);
            }
            text += ("Most Commonly Bought Items: " + Environment.NewLine);
            foreach (string commonbuy in mostCommonItems)
            {
                text += ("  " + commonbuy + Environment.NewLine);
            }
            text += ("Least Bought Items: " + Environment.NewLine);
            foreach (string uncommonbuy in leastCommonItems)
            {
                text += ("  " + uncommonbuy + Environment.NewLine);
            }
            text += ("Most Revenue: " + mostExpensiveBought + Environment.NewLine);
            text += ("Least Revenue: " + leastExpensiveBought + " (excluding no sales) " + Environment.NewLine);

            bestCategory = "";
            max = 0;
            int i = 0;
            foreach (int a in categoryIndex)
            {
                if (a > max)
                {
                    max = a;
                    bestCategory = GetCategories()[i];
                }
                i++;
            }

            text += ("Most Popular Category: " + bestCategory + Environment.NewLine);
            text += ("Popular Category Quantity: " + max.ToString() + Environment.NewLine);

            totalRev = 0;
            foreach (double d in itemTotals)
            {
                totalRev += d;
            }
            text += ("Total Revenue: $" + totalRev.ToString() + Environment.NewLine);
            rtbText.Text = text;
        }

        private void Weekly_Load(object sender, EventArgs e)
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
            names = new List<string>();
            purchaseIds = new List<string>();
            stockIds = new List<string>();
            quantities = new List<string>();
            saleQuantities = new List<string>();
            prices = new List<string>();
            categories = new List<string>();
            totals = new List<string>();
            dates = new List<string>();
            categoryIndex = new List<int>();


            try
            {
                ConnectToSaleDb();
                ConnectToStockDb();
                Analyse();
                Output();

            }
            catch
            {
                MessageBox.Show("An error has occured. Try again.");
            }
        }

        public void InitialiseCategoryIndex()
        {
            foreach (string x in GetCategories())
            {
                categoryIndex.Add(0);
            }
        }

        public void ConnectToSaleDb()
        {
            connection.Open();
            string query = "Select * from Sale";
            MySqlCommand cmd = new MySqlCommand(query, connection);
            MySqlDataReader dataReader = cmd.ExecuteReader();
            while (dataReader.Read())
            {
                saleIds.Add(dataReader["saleId"] + "");
                purchaseIds.Add(dataReader["stockId"] + "");
                saleQuantities.Add(dataReader["quantity"] + "");
                totals.Add(dataReader["total"] + "");
                var datetime = dataReader["date"] + "";
                dates.Add(datetime.ToString());

            }
            dataReader.Close();
            connection.Close();

        }

        public void ConnectToStockDb()
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

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        public string writeToCSV()
        {
            string content = "";
            content += ("Report Type,Date Created,Total Sales,Total Items Sold,Out of Stock,Out of Stock Items,Most Common Item Sold,Least Common Item Sold,Biggest Source Of Revenue,Smallest Source Of Revenue,Most Common Category,Quantity from Category,Total Revenue");
            content += Environment.NewLine;

            content += ("Weekly,");
            content += (DateTime.Today.ToString("dd/MM/yyyy") + ",");
            content += (saleQuantity + ",");
            content += (soldQuantity + ",");
            content += (outOfStockNo + ",");

            int o = 0;
            content += '"';
            foreach (string outofstock in outOfStockItems)
            {
                if (o != 0)
                {
                    content += ",";
                }
                content += (outofstock);
                o++;
            }
            content += '"';
            content += (",");

            int m = 0;
            content += '"';
            foreach (string mostcommon in mostCommonItems)
            {
                if (m != 0)
                {
                    content += ",";
                }
                content += (mostcommon);
                m++;
            }
            content += '"';
            content += (",");

            content += '"';
            int l = 0;
            foreach (string leastcommon in leastCommonItems)
            {
                if (l != 0)
                {
                    content += ",";
                }
                content += (leastcommon);
                l++;
            }
            content += '"';
            content += (",");

            content += (mostExpensiveBought + ",");
            content += (leastExpensiveBought + ",");
            content += (bestCategory + ",");
            content += (max.ToString() + ",");
            content += (totalRev.ToString());
            return content;
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Comma Seperated Values|*.csv";
            sfd.Title = "Export to CSV";
            sfd.ShowDialog();
            if (sfd.FileName != "")
            {
                try
                {
                    StreamWriter sw = new StreamWriter(sfd.FileName);
                    sw.Write(writeToCSV());
                    sw.Close();
                }
                catch
                {
                    MessageBox.Show("An error occured.");
                }
            }

        }
    }
}
