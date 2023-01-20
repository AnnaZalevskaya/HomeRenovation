using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using WebApplication2_2;

namespace WebApplication2._2
{
    public partial class _Default : Page
    {
        DB db = new DB();
        protected void AddList()
        {
            DropDownList1.Items.Clear();
            DropDownList2.Items.Clear();
            DropDownList4.Items.Clear();
            db.OpenConnection();
            MySqlCommand commandType = new MySqlCommand("select type from type_of_repair", db.GetConnection());
            MySqlDataReader dataReader1 = commandType.ExecuteReader();
            try
            {
                for (int i = 0; i < dataReader1.FieldCount; i++)
                {
                    while (dataReader1.Read())
                    {

                        DropDownList1.Items.Add(dataReader1.GetValue(i).ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                Label10.Text = ex.ToString() + "Сработало исключение!";
            }
            db.CloseConnection();
            db.OpenConnection();
            MySqlCommand commandMaster = new MySqlCommand("select name_master from full_name_master", db.GetConnection());
            MySqlDataReader dataReader2 = commandMaster.ExecuteReader();
            try
            {
                for (int i = 0; i < dataReader2.FieldCount; i++)
                {
                    while (dataReader2.Read())
                    {
                        DropDownList2.Items.Add(dataReader2.GetValue(i).ToString());
                        DropDownList4.Items.Add(dataReader2.GetValue(i).ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                Label10.Text = "Сработало исключение!" + ex.ToString();
            }
            db.CloseConnection();
        }
        protected void ButtonAdd_Click(object sender, EventArgs e)
        {
            if (TextBox1.Text != string.Empty && TextBox2.Text != string.Empty && TextBox3.Text != string.Empty && TextBox4.Text != string.Empty && TextBox5.Text != string.Empty) {
                MySqlCommand command = new MySqlCommand("insert into repair(customer, address, type_of_repair_id, application_date, repair_start_date, `repair_period(days)`, repair_end_date, name_master_id)  " +
                "values(@customer, @address, @type, @appDate, @startDate, @period, DATE_ADD(repair_start_date, INTERVAL `repair_period(days)` DAY), @master)", db.GetConnection());
                MySqlCommand comType = new MySqlCommand("select id from type_of_repair where type = '" + DropDownList1.SelectedValue + "'", db.GetConnection());
                db.OpenConnection();
                MySqlDataReader dataReader1 = comType.ExecuteReader();

                command.Parameters.Add("@customer", MySqlDbType.VarChar).Value = TextBox1.Text;
                command.Parameters.Add("@address", MySqlDbType.VarChar).Value = TextBox2.Text;

                if (dataReader1.Read())
                    command.Parameters.Add("@type", MySqlDbType.VarChar).Value = dataReader1.GetValue(0).ToString();
                db.CloseConnection();
                command.Parameters.Add("@appDate", MySqlDbType.Date).Value = TextBox3.Text;
                command.Parameters.Add("@startDate", MySqlDbType.Date).Value = TextBox4.Text;
                command.Parameters.Add("@period", MySqlDbType.Int32).Value = TextBox5.Text;
                MySqlCommand comMaster = new MySqlCommand("select id from full_name_master where name_master = '" + DropDownList2.SelectedValue + "'", db.GetConnection());
                db.OpenConnection();
                MySqlDataReader dataReader2 = comMaster.ExecuteReader();
                if (dataReader2.Read())
                    command.Parameters.Add("@master", MySqlDbType.VarChar).Value = dataReader2.GetValue(0).ToString();
                db.CloseConnection();

                db.OpenConnection();
                if (command.ExecuteNonQuery() == 1)
                    Label10.Text = "Заявка добавлена!";
                db.CloseConnection();

                TextBox1.Text = string.Empty;
                TextBox2.Text = string.Empty;
                TextBox3.Text = string.Empty;
                TextBox4.Text = string.Empty;
                TextBox5.Text = string.Empty;
            }
            else Label10.Text = "Поля пусты! Заполните их, пожалуйста.";
        }

        protected void Page_PreRender(object sender, EventArgs e)
        {
            AddList();
            string query = "select r.id as Номер_заявки, customer as Заказчик, address as Адресс, tor.type as Тип_ремонта, DATE_FORMAT(application_date, '%d.%m.%Y') as Дата_заказа, DATE_FORMAT(repair_start_date, '%d.%m.%Y') as Начало_ремонта, `repair_period(days)` as Период_ремонта, DATE_FORMAT(repair_end_date, '%d.%m.%Y') as Окончание_ремонта, nm.name_master as ФИО_мастера from repair r " +
            "inner join full_name_master nm on r.name_master_id = nm.id " +
            "inner join type_of_repair tor on r.type_of_repair_id = tor.id";
            using (db.GetConnection())
            {
                using (MySqlCommand cmd = new MySqlCommand(query, db.GetConnection()))
                {
                    db.OpenConnection();
                    MySqlDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                    DataTable dt = new DataTable();
                    dt.Load(dr);
                    GridView1.DataSource = dt;
                    GridView1.DataBind();
                }
            }
        }
        private void WriteToFile(string path, DataTable dataTable)
        {
            using (var sw = new System.IO.StreamWriter(new System.IO.FileStream(path, System.IO.FileMode.Create)))
            {
                if (dataTable != null)
                {
                    foreach (var row in dataTable.Select()) 
                    {
                        object[] array = row.ItemArray;
                        for (int j = 0; j < array.Length; j++)
                        {
                            sw.Write(row.ItemArray.GetValue(j) + " | ");
                        }
                        sw.WriteLine();
                    }
                }
            }
        }
        protected void ButtonExecute_Click(object sender, EventArgs e)
        {
            string query = null;
            string baseQuery = "select r.id as Номер_заявки, customer as Заказчик, address as Адресс, tor.type as Тип_ремонта, DATE_FORMAT(application_date, '%d.%m.%Y') as Дата_заказа, DATE_FORMAT(repair_start_date, '%d.%m.%Y') as Начало_ремонта, `repair_period(days)` as Период_ремонта, DATE_FORMAT(repair_end_date, '%d.%m.%Y') as Окончание_ремонта, nm.name_master as ФИО_мастера from repair r " +
            "inner join full_name_master nm on r.name_master_id = nm.id " +
            "inner join type_of_repair tor on r.type_of_repair_id = tor.id";
            string file = "";
            if(DropDownList3.SelectedIndex == 0)
            {
                query = baseQuery + " where DATE_FORMAT(repair_start_date, '%Y') = YEAR(CURRENT_DATE) && DATE_FORMAT(repair_end_date, '%Y') = YEAR(CURRENT_DATE)";
                file = "E:\\универ\\3 курс\\ПдИ\\report1.txt";
            }
            else if (DropDownList3.SelectedIndex == 1)
            {
                query = baseQuery + " where DAYNAME(application_date) = 'Wednesday'";
            }
            else if (DropDownList3.SelectedIndex == 2)
            {
                query = baseQuery + " where MONTH(application_date) = 5 && type_of_repair_id = 2";
                file = "E:\\универ\\3 курс\\ПдИ\\report2.txt";
            }
            else if (DropDownList3.SelectedIndex == 3)
            {
                query = "select customer as Заказчик, DATE_FORMAT(repair_end_date, '%d.%m.%Y') as Окончание_ремонта, DAYNAME(repair_end_date) as День_недели from repair";
            }
            else if (DropDownList3.SelectedIndex == 4)
            {
                query = baseQuery + " where name_master_id = " + (DropDownList4.SelectedIndex + 1);
            }
            using (db.GetConnection())
            {
                using (MySqlCommand cmd = new MySqlCommand(query, db.GetConnection()))
                {
                    db.OpenConnection();
                    MySqlDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                    DataTable dt = new DataTable();
                    dt.Load(dr);
                    WriteToFile(file, dt);
                    GridView2.DataSource = dt;
                    GridView2.DataBind();    
                }
            }
        }
    }
}
