using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FinalProject
{
    public partial class LogIn : Form
    {
        // Reusable Variables
        SqlConnection con = DBManager.Connection();
        SqlCommand cm = new SqlCommand();
        SqlDataReader reader;
        public int EmployeeID { get; set; }

        public LogIn()
        {
            InitializeComponent();
            txtUsername.Focus();
        }

        // UI RELATED CODE HERE
        public void Clear()
        {
            txtUsername.Clear();
            txtPassword.Clear();
            txtFName.Clear();
            txtLName.Clear();
            txtNum.Clear();
            txtUsername2.Clear();
            txtPassword2.Clear();
        }

        private void btnSignIn_Click(object sender, EventArgs e)
        {
            Clear();
            pnlSign.Visible = true;
            pnlLogin.Visible = false;
        }

        private void exitLogBtn_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.Application.Exit();
        }

        private void ExitSignInBtn_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.Application.Exit();
        }

        public int EmpId()
        {
            cm = new SqlCommand("Select COALESCE(MAX(employeeID), '0') From tblLogin", con);
            con.Open();
            cm.ExecuteNonQuery();
            int a = Convert.ToInt32(cm.ExecuteScalar());
            con.Close();
            int b = a + 1;
            return b;
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            String username, password;
            String n = txtUsername.Text;
            String p = txtPassword.Text;

            try
            {
                string query = "SELECT * FROM tblLogin WHERE username = '" + n + "' AND password = '" + p + "'";

                SqlDataAdapter sda = new SqlDataAdapter(query, con);

                DataTable dataTable = new DataTable();
                sda.Fill(dataTable);

                if (dataTable.Rows.Count > 0)
                {
                    for (int i = 0; i < dataTable.Rows.Count; i++)
                    {
                        EmployeeID = Convert.ToInt32(dataTable.Rows[i]["employeeID"]);
                        if (dataTable.Rows[i]["usertype"].ToString() == "admin")
                        {
                            username = n;
                            password = p;

                            System1 form1 = new System1(this, EmployeeID);
                            form1.EmployeeID = EmployeeID;
                            form1.Show();
                            this.Hide();
                        }
                        else if (dataTable.Rows[i]["usertype"].ToString() == "employee")
                        {
                            username = n;
                            password = p;

                            System1 form1 = new System1(this, EmployeeID);
                            System2 form2 = new System2(this, form1, EmployeeID);
                            form2.EmployeeID = EmployeeID;
                            form2.Show();
                            this.Hide();
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Error: Invalid login details. Please try again", "Warning",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                    txtUsername.Focus();
                }
            }
            catch
            {
                MessageBox.Show("Error: Invalid login details. Please try again", "Warning",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtUsername.Focus();
            }
            finally
            {
                Clear();
            }

        }
        private void btnSignIn2_Click(object sender, EventArgs e)
        {
            String fName = txtFName.Text;
            String lName = txtLName.Text;
            String pNumber = txtNum.Text;
            String username = txtUsername2.Text;
            String password = txtPassword2.Text;
            String phoneRegex = @"^\d{11}$";
            long id = EmpId();

            try
            {
                if (Regex.IsMatch(pNumber, phoneRegex) && !String.IsNullOrEmpty(fName) && !String.IsNullOrEmpty(lName) && !String.IsNullOrEmpty(username) && !String.IsNullOrEmpty(password))
                {
                    cm = new SqlCommand("SELECT * FROM tblLogin WHERE username = '" + username + "'", con);
                    con.Open();
                    reader = cm.ExecuteReader();
                    if (reader.Read())
                    {
                        reader.Close();
                        MessageBox.Show("Username already exist, please try another", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        cm = new SqlCommand("INSERT INTO tblEmployee (fName, lName, phoneNumber) VALUES (@fName, @lName, @pNumber)", con);
                        cm.Parameters.AddWithValue("@fName", fName);
                        cm.Parameters.AddWithValue("@lName", lName);
                        cm.Parameters.AddWithValue("@pNumber", pNumber);
                        cm.ExecuteNonQuery();

                        cm = new SqlCommand("INSERT INTO tblLogin (employeeID,username, password, usertype) VALUES (@id, @username, @password, 'employee')", con);
                        cm.Parameters.AddWithValue("@id", id);
                        cm.Parameters.AddWithValue("@username", username);
                        cm.Parameters.AddWithValue("@password", password);
                        cm.ExecuteNonQuery();
                        con.Close();
                        MessageBox.Show("Your Account is now created. Please login now.", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        pnlLogin.Visible = true;
                        pnlSign.Visible = false;
                    }
                }
                else if (String.IsNullOrEmpty(fName) || String.IsNullOrEmpty(lName) || String.IsNullOrEmpty(username) || !String.IsNullOrEmpty(password) || String.IsNullOrEmpty(pNumber))
                {
                    throw new ArgumentNullException();
                }
                else if (!Regex.IsMatch(pNumber, phoneRegex))
                {
                    throw new NumberFormatException("Error: Incorrect Phone Number Format. Try again");
                }
                else
                {
                    throw new Exception();
                }
            }
            catch (NumberFormatException a)
            {
                MessageBox.Show(a.Message);
            }
            catch (ArgumentNullException)
            {
                MessageBox.Show("Error: Value can not be Null. Try again");
            }
            catch (Exception c)
            {
                MessageBox.Show("Error: " + c.Message);
            }
            finally
            {
                Clear();
            }
        }
        private void btnLogin2_Click(object sender, EventArgs e)
        {
            Clear();
            pnlLogin.Visible = true;
            pnlSign.Visible = false;
        }

        class NumberFormatException : Exception
        {
            public NumberFormatException(string phone) : base(phone) { }
        }

    }
}
