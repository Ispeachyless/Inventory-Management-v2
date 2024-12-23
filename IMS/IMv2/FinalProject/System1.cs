using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using SReturnAutoinvoiceNoUpdate = System.Windows.Forms.TextBox;
using SReturnAutoStatusComboBox = System.Windows.Forms.ComboBox;

namespace FinalProject
{
    public partial class System1 : Form
    {
        // Reusable Variables
        SqlConnection con = DBManager.Connection();
        SqlCommand cm = new SqlCommand();
        SqlDataReader reader;
        LogIn form = new LogIn();

        bool ReportExpand = false;
        bool SalesExpand = false;
        bool InventoryExpand = false;

        private int currentRow = 0;
        public int EmployeeID { get; set; }
        private DataTable paymentDataTable;
        private const int maxRowsPerPage = 20;
        private ISalesReturnStrat currentStrategy;
        public System1(LogIn form, int employeeID)
        {
            InitializeComponent();
            LoadAuditLogHistory();
            overView();
            EmployeeID = employeeID;
            this.form = form;
        }
        private void System1_Load(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Maximized;
            Bounds = Screen.PrimaryScreen.Bounds;
            SetStrategy(new PopulateInvoiceComboBox());
            ExecuteStrategy();
        }

        // UI RELATED CODE STARTS HERE
        // --------------------------------------------------------
        // EXIT BUTTON
        private void button5_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.Application.Exit();
        }
        // LOGOUT BUTTON
        private void logOutBtn_Click(object sender, EventArgs e)
        {
            this.form.Clear();
            this.form.Show();
            this.Hide();
        }
        // BUTTON TRANSITION
        private void SalesTransition_Tick(object sender, EventArgs e)
        {
            if (SalesExpand == false)
            {
                MSales.Height += 10;
                if (MSales.Height >= 215)
                {
                    SalesTransition.Stop();
                    SalesExpand = true;

                    if (ReportExpand == true)
                    {
                        ReportTransition.Start();
                    }
                    else if (InventoryExpand == true)
                    {
                        InventoryTransition.Start();
                    }
                }
            }
            else
            {
                MSales.Height -= 10;
                if (MSales.Height <= 55)
                {
                    SalesTransition.Stop();
                    SalesExpand = false;
                }
            }
        }
        private void SalesBtn_Click(object sender, EventArgs e)
        {
            SalesTransition.Start();
        }
        private void ReportTransition_Tick(object sender, EventArgs e)
        {
            if (ReportExpand == false)
            {
                MReport.Height += 10;
                if (MReport.Height >= 307)
                {
                    ReportTransition.Stop();
                    ReportExpand = true;

                    if (SalesExpand == true)
                    {
                        SalesTransition.Start();
                    }
                    else if (InventoryExpand == true)
                    {
                        InventoryTransition.Start();
                    }
                }
            }
            else
            {
                MReport.Height -= 10;
                if (MReport.Height <= 55)
                {
                    ReportTransition.Stop();
                    ReportExpand = false;
                }
            }
        }
        private void InventoryBtn_Click(object sender, EventArgs e)
        {
            ReportTransition.Start();
        }
        private void InventoryTransition_Tick(object sender, EventArgs e)
        {
            if (InventoryExpand == false)
            {
                MInventory.Height += 10;
                if (MInventory.Height >= 156)
                {
                    InventoryTransition.Stop();
                    InventoryExpand = true;

                    if (SalesExpand == true)
                    {
                        SalesTransition.Start();
                    }
                    else if (ReportExpand == true)
                    {
                        ReportTransition.Start();
                    }
                }
            }
            else
            {
                MInventory.Height -= 10;
                if (MInventory.Height <= 55)
                {
                    InventoryTransition.Stop();
                    InventoryExpand = false;
                }
            }
        }
        private void InventoryBtn_Click_1(object sender, EventArgs e)
        {
            InventoryTransition.Start();
        }
        // PANEL CLOSE
        public void PClose()
        {
            Dashboard.Visible = false;
            Contacts.Visible = false;
            Item.Visible = false;
            ItemProduction.Visible = false;
            SalesOrder.Visible = false;
            Invoice.Visible = false;
            SalesReturn.Visible = false;
            LowStock.Visible = false;
            CompletedSales.Visible = false;
            PaymentRecieved.Visible = false;
            InventorySummary.Visible = false;
            AuditLog.Visible = false;
        }
        //PANEL SWITCHING
        private void DashboardBtn_Click(object sender, EventArgs e)
        {
            PClose();
            Dashboard.Visible = true;
        }
        private void ItemProductionBtn_Click(object sender, EventArgs e)
        {
            PClose();
            ItemProduction.Visible = true;

            if (con.State == ConnectionState.Open)
            {
                con.Close();
            }
            FillsProduction();
        }
        private void RLowStock_Click(object sender, EventArgs e)
        {
            PClose();
            LowStock.Visible = true;

            FillLowStockItems();
        }
        private void RCompletedSales_Click(object sender, EventArgs e)
        {
            PClose();
            CompletedSales.Visible = true;

            CompletedSalesDataGridView();
        }
        private void RPaymentsRecieved_Click(object sender, EventArgs e)
        {
            PClose();
            PaymentRecieved.Visible = true;

            PopulatePaymentReport();
        }
        private void RInventorySummary_Click(object sender, EventArgs e)
        {
            PClose();
            InventorySummary.Visible = true;

            FillSummary();
        }
        private void RAuditLog_Click(object sender, EventArgs e)
        {
            PClose();
            AuditLog.Visible = true;

            LoadAuditLogHistory();
        }
        private void SOrder_Click(object sender, EventArgs e)
        {
            PClose();
            SalesOrder.Visible = true;

            FillOrder();
        }
        private void ItemBtn_Click(object sender, EventArgs e)
        {
            PClose();
            Item.Visible = true;

            LoadItems();
            Item1CBX();

        }
        private void SInvoice_Click(object sender, EventArgs e)
        {
            PClose();
            Invoice.Visible = true;

            PopulateEmployeeInvoicesGrid();
        }
        private void SReturn_Click(object sender, EventArgs e)
        {
            PClose();
            SalesReturn.Visible = true;

            SetStrategy(new PopulateInvoiceComboBox());
            ExecuteStrategy();
            LoadUpdateInvoiceNos();
            PopulateReasonComboBox();
            PopulateUpdateRStatusComboBox();
            LoadSalesReturnDataGridVIew();
        }
        private void ContactsBtn_Click(object sender, EventArgs e)
        {
            PClose();

            Contacts.Visible = true;

            FillEmpContacts();
            FillCusContacts();
        }
        private void CloseInvoiceBtn_Click(object sender, EventArgs e)
        {
            Invoice.Visible = false;
        }
        private void IProductionCloseBtn_Click(object sender, EventArgs e)
        {
            ItemProduction.Visible = false;
        }
        private void CSalesCloseBtn_Click(object sender, EventArgs e)
        {
            CompletedSales.Visible = false;
        }
        private void LStockCloseBtn_Click(object sender, EventArgs e)
        {
            LowStock.Visible = false;
        }
        private void PReceivedCloseBtn_Click(object sender, EventArgs e)
        {
            PaymentRecieved.Visible = false;
        }
        private void ISummaryCloseBtn_Click(object sender, EventArgs e)
        {
            InventorySummary.Visible = false;
        }
        public void Clear()
        {
            // Contacts Clear
            TxtSearch.Clear();
            txtFName.Clear();
            txtLName.Clear();
            txtPNum.Clear();

            // Sales Order Clear
            SCustomerCBX.SelectedIndex = -1;
            SProductCBX.SelectedIndex = -1;
            TxtQuantity.Clear();
            OrderList.Items.Clear();
            DueDate.Value = DateTime.Today;
            TxtOrderSearch.Clear();
            InvoiceCbx1.SelectedIndex = -1;
            SOrderStatusCbx1.SelectedIndex = -1;

            // Item Clear
            TxtItemSearch.Clear();
            txtbx_Nme.Clear();
            txtbx_Prc.Clear();
            txtbx_ROrdrLvl.Clear();
            txtbx_Dscrptn.Clear();
            txtbx_Nme.Clear();
            txtbx_Prc.Clear();
            txtbx_Dscrptn.Clear();
            txtbx_ROrdrLvl.Clear();
            comboBox_ProductID.SelectedIndex = -1;
            TxtbxUntPrc.Clear();

            // Invoice Clear
            TxtInvoiceSearch.Clear();

            // Audit Log Clear
            TxtAuditSearch.Clear();

            // Sales Return Clear
            TxtReturnSearch.Clear();
            SReturnInvoiceNoCmb.SelectedIndex = -1;
            SReturnReasonCmb.SelectedIndex = -1;
            SReturnProductNameCmb.SelectedIndex = -1;
            SReturnQuantitytxt.Clear();
            SReturnUpdateInvoiceCmb.SelectedIndex = -1;
            SReturnProductNameTxt.Clear();
            SReturnUpdateStatusCmb.SelectedIndex = -1;

            // Item Production Clear
            TxtProductionSearch.Clear();
            txtbx_Nme.Clear();
            txtbx_Prc.Clear();
            txtbx_ROrdrLvl.Clear();
            txtbx_Dscrptn.Clear();
            txtbx_QnttyUpdte.Clear();
            Reason.Clear();
            UpdtPrdctCBX.SelectedIndex = -1;
            cmbbx_Product.SelectedIndex = -1;
            productiobCBX.SelectedIndex = -1;
            operationCB.SelectedIndex = -1;
            txtbx_Qntty.Clear();
            LVProductList.Items.Clear();

            // Reports Clear
            TxtLStockSearch.Clear();
            TxtPRecievedSearch.Clear();
            TxtCSalesSearch.Clear();
        }
        public void ClosePanel()
        {
            // Sales Order Panels
            NewSOrder.Visible = false;
            SOrderUpdate.Visible = false;

            // Item Panels
            pnl_AddItem.Visible = false;
            pnl_UpdtPrc.Visible = false;
        }

        // CONTACT CODES STARTS HERE
        // --------------------------------------------------------
        // PANELS SWITCHING
        private void CusCancel_Click(object sender, EventArgs e)
        {
            NewCus.Visible = false;
        }
        private void cusNew_Click(object sender, EventArgs e)
        {
            if (NewCus.Visible == false)
            {
                NewCus.Visible = true;
            }
            else
            {
                NewCus.Visible = false;
            }
        }
        private void ContactsSearchBtn_Click(object sender, EventArgs e)
        {
            if (ContactsSearchPanel.Visible == false)
            {
                ContactsSearchPanel.Visible = true;
            }
            else
            {
                ContactsSearchPanel.Visible = false;
            }
        }
        private void ContactsSearchClose_Click(object sender, EventArgs e)
        {
            if (ContactsSearchPanel.Visible == true)
            {
                ContactsSearchPanel.Visible = false;
            }

            FillEmpContacts();
            FillCusContacts();
        }
        private void ContactsCloseBtn_Click(object sender, EventArgs e)
        {
            Contacts.Visible = false;
        }
        private void FillCusContacts()
        {
            CustomerTbl.Rows.Clear();
            cm = new SqlCommand("SELECT * FROM tblCustomer ORDER BY customerID", con);
            con.Open();
            reader = cm.ExecuteReader();
            while (reader.Read())
            {
                CustomerTbl.Rows.Add(reader[1].ToString() + " " + reader[2].ToString(), reader[3].ToString());
            }
            CustomerCount.Text = $"Total records: {CustomerTbl.Rows.Count}";
            reader.Close();
            con.Close();
        }
        private void FillEmpContacts()
        {
            EmployeeTbl.Rows.Clear();
            cm = new SqlCommand("SELECT * FROM tblEmployee ORDER BY employeeID", con);

            con.Open();
            reader = cm.ExecuteReader();
            while (reader.Read())
            {
                EmployeeTbl.Rows.Add(reader[0].ToString(), reader[1].ToString() + " " + reader[2].ToString(), reader[3].ToString());
            }
            EmployeeCount.Text = $"Total records: {EmployeeTbl.Rows.Count}";
            reader.Close();
            con.Close();
        }
        private void CusSaveBtn_Click(object sender, EventArgs e)
        {
            String fName = txtFName.Text;
            String lName = txtLName.Text;
            String pNumber = txtPNum.Text;
            String phoneRegex = @"^\d{11}$";
            try
            {
                if (Regex.IsMatch(pNumber, phoneRegex) && !String.IsNullOrEmpty(fName) && !String.IsNullOrEmpty(lName) && !String.IsNullOrEmpty(pNumber))
                {
                    cm = new SqlCommand("INSERT INTO tblCustomer (fName, lName, phoneNumber) VALUES (@fName, @lName, @pNumber)", con);
                    cm.Parameters.AddWithValue("@fName", fName);
                    cm.Parameters.AddWithValue("@lName", lName);
                    cm.Parameters.AddWithValue("@pNumber", pNumber);
                    con.Open();
                    cm.ExecuteNonQuery();
                    con.Close();

                    FillCusContacts();
                }
                else if (String.IsNullOrEmpty(fName) || String.IsNullOrEmpty(lName) || String.IsNullOrEmpty(pNumber))
                {
                    throw new ArgumentNullException();
                }
                else if (!Regex.IsMatch(pNumber, phoneRegex))
                {
                    throw new NumberFormatException("Error: Incorrect Phone Number Format.");
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
                MessageBox.Show("Error: Value can not be Null.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (FormatException a)
            {
                MessageBox.Show("Error: " + a.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception c)
            {
                MessageBox.Show("Error: " + c.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Clear();
            }
        }
        private void ContactsSearch_Click(object sender, EventArgs e)
        {
            try
            {
                cm = new SqlCommand("SELECT COALESCE(e.employeeID, '') as EmpID, COALESCE(e.fName, '') as EmpFName,COALESCE(e.lName, '') as EmpLName, COALESCE(e.phoneNumber, '') as EmpNum, COALESCE(c.fName, '') as CusFName,  COALESCE(c.lName, '') as CusLName, COALESCE(c.phoneNumber, '') as CusNum FROM tblEmployee e FULL JOIN tblCustomer c on e.employeeID = c.customerID WHERE e.fName LIKE @search or e.lName LIKE @search or c.fName LIKE @search or c.lName LIKE @search", con);
                cm.Parameters.AddWithValue("@search", TxtSearch.Text);
                con.Open();
                cm.ExecuteNonQuery();
                EmployeeTbl.Rows.Clear();
                CustomerTbl.Rows.Clear();
                reader = cm.ExecuteReader();
                while (reader.Read())
                {
                    if (!reader[0].ToString().Equals("" + 0))
                    {
                        EmployeeTbl.Rows.Add(reader[0].ToString(), reader[1].ToString() + " " + reader[2].ToString(), reader[3].ToString());
                    }
                    if (!reader[4].ToString().Equals(""))
                    {
                        CustomerTbl.Rows.Add(reader[4].ToString() + " " + reader[5].ToString(), reader[6].ToString());
                    }
                }
                EmployeeCount.Text = $"Total records: {EmployeeTbl.Rows.Count}";
                CustomerCount.Text = $"Total records: {CustomerTbl.Rows.Count}";
                reader.Close();
                con.Close();

                if (TxtSearch.Text == "")
                {
                    FillEmpContacts();
                    FillCusContacts();
                }
            }
            catch (Exception c)
            {
                MessageBox.Show("Error: " + c.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Clear();
            }
        }
        class NumberFormatException : Exception
        {
            public NumberFormatException(string phone) : base(phone) { }
        }

        // SALES ORDER CODE STARTS HERE
        // --------------------------------------------------------
        // PANELS SWITCHING
        private void SOrderExitBtn1_Click(object sender, EventArgs e)
        {
            SalesOrder.Visible = false;
        }
        private void SOrderClose_Click(object sender, EventArgs e)
        {
            if (SOrderUpdate.Visible == true)
            {
                SOrderUpdate.Visible = false;
            }
        }
        private void SOrderUpdateBtn1_Click(object sender, EventArgs e)
        {
            if (SOrderUpdate.Visible == false)
            {
                ClosePanel();
                SOrderUpdate.Visible = true;

                // OrderCBX();
                OrderNoCBX();
            }
            else
            {
                SOrderUpdate.Visible = false;
            }
        }
        private void SOrderNewBtn1_Click(object sender, EventArgs e)
        {
            if (NewSOrder.Visible == false)
            {
                ClosePanel();
                NewSOrder.Visible = true;
                OrderList.View = View.Details;
                OrderList.Columns.Add("Quantity", 100, HorizontalAlignment.Left);
                OrderList.Columns.Add("Product", 180, HorizontalAlignment.Left);
                CusCBX();
                ItemCBX();
            }
            else
            {
                NewSOrder.Visible = false;
                Clear();
            }
        }
        private void SOrderUpExitBtn_Click(object sender, EventArgs e)
        {
            SOrderUpdate.Visible = false;
        }
        private void NewOrderClose_Click(object sender, EventArgs e)
        {
            NewSOrder.Visible = false;
        }
        // ID
        public int OrderId()
        {
            con.Open();
            cm = new SqlCommand("SELECT MAX(orderId) FROM tblSales", con);
            int maxOrderId = Convert.ToInt32(cm.ExecuteScalar());
            con.Close();
            return maxOrderId + 1;
        }
        public int InvoiceId()
        {
            con.Open();
            cm = new SqlCommand("SELECT MAX(invoiceNo) FROM tblSales", con);
            int maxOrderId = Convert.ToInt32(cm.ExecuteScalar());
            con.Close();
            return maxOrderId + 1;
        }
        // CBX
        public void OrderNoCBX()
        {
            InvoiceCbx1.Items.Clear();
            cm = new SqlCommand("SELECT invoiceNo FROM tblSales WHERE iStatus NOT IN ('Delivery Complete', 'Cancelled') AND employeeID = @employee;", con);
            cm.Parameters.AddWithValue("@employee", EmployeeID);

            con.Open();
            reader = cm.ExecuteReader();
            while (reader.Read())
            {
                InvoiceCbx1.Items.Add(reader[0].ToString());
            }
            InvoiceCbx1.AutoCompleteSource = AutoCompleteSource.ListItems;
            reader.Close();
            con.Close();
        }

        private string GetCurrentStatusFromTblSales()
        {
            string currentStatus = "";
            con.Open();
            cm = new SqlCommand("SELECT iStatus FROM tblSales WHERE invoiceNo = @invoice", con);
            cm.Parameters.AddWithValue("@invoice", InvoiceCbx1.Text);
            reader = cm.ExecuteReader();
            while (reader.Read())
            {
                currentStatus = reader["iStatus"].ToString();
            }
            con.Close();
            return currentStatus;
        }
        public void OrderCBX()
        {
            string currentStatus = GetCurrentStatusFromTblSales();
            SOrderStatusCbx1.Items.Clear();

            switch (currentStatus)
            {
                case "Order Placed":
                    SOrderStatusCbx1.Items.Add("In Transit");
                    SOrderStatusCbx1.Items.Add("Cancelled");
                    break;
                case "In Transit":
                    SOrderStatusCbx1.Items.Add("Delivery Complete");
                    break;
                default:
                    break;
            }
            SOrderStatusCbx1.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            SOrderStatusCbx1.AutoCompleteSource = AutoCompleteSource.ListItems;
        }
        public void CusCBX()
        {
            Customer cus = new Customer();
            ArrayList customerList = new ArrayList();
            con.Open();
            cm = new SqlCommand("SELECT customerID, fName, lName FROM tblCustomer ORDER BY fName", con);

            SqlDataReader reader = cm.ExecuteReader();
            while (reader.Read())
            {
                cus = new Customer();
                cus.cId = Convert.ToInt32(reader.GetValue(0));
                cus.fullName = reader.GetValue(1) + " " + reader.GetValue(2);
                customerList.Add(cus);
            }
            SCustomerCBX.DataSource = customerList;
            SCustomerCBX.ValueMember = "cId";
            SCustomerCBX.DisplayMember = "fullName";
            SCustomerCBX.SelectedIndex = -1;
            SCustomerCBX.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            SCustomerCBX.AutoCompleteSource = AutoCompleteSource.ListItems;
            con.Close();
        }
        public void ItemCBX()
        {
            Item item = new Item();
            ArrayList itemList = new ArrayList();
            con.Open();
            cm = new SqlCommand("SELECT ProductID, ProductName, Description FROM tblItem", con);

            SqlDataReader reader = cm.ExecuteReader();
            while (reader.Read())
            {
                item = new Item();
                item.iId = Convert.ToInt32(reader.GetValue(0));
                item.iName = reader.GetValue(1) + " (" + reader.GetValue(2) + ")";
                item.itemOnly = reader.GetValue(1) + "";
                itemList.Add(item);
            }
            SProductCBX.DataSource = itemList;
            SProductCBX.ValueMember = "iID";
            SProductCBX.DisplayMember = "iName";
            SProductCBX.SelectedIndex = -1;
            SProductCBX.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            SProductCBX.AutoCompleteSource = AutoCompleteSource.ListItems;
            con.Close();
        }
        private void FillOrder()
        {
            OSalesTbl.Rows.Clear();
            cm = new SqlCommand("SELECT s.orderID, s.employeeID, s.invoiceNo, s.iStatus, s.invoiceDate, c.fName +' '+ c.lName, s.dueDate FROM tblSales s \r\nINNER JOIN tblCustomer c on s.customerID = c.customerID order by s.dueDate\r\n", con);

            con.Open();
            reader = cm.ExecuteReader();
            while (reader.Read())
            {
                OSalesTbl.Rows.Add(String.Format("{0:MM/dd/yyyy}", reader.GetValue(4)), reader[0].ToString(), reader[2].ToString(), reader[5].ToString(), reader[3].ToString(), String.Format("{0:MM/dd/yyyy}", reader.GetValue(6)), reader.GetValue(1));
            }
            OrderCount.Text = $"Total records: {OSalesTbl.Rows.Count}";
            reader.Close();
            con.Close();

            PopulatePaymentReport();
        }
        private void SOrderUpStatusBtn_Click(object sender, EventArgs e)
        {
            int flagg = 0;
            ArrayList pidList = new ArrayList();
            Production production = new Production();
            ArrayList transitOrderList = new ArrayList();

            try
            {
                if (SOrderStatusCbx1.Text.Equals("In Transit"))
                {
                    int productId = 0;
                    int totalProductionQty = 0;
                    int totalSalesQty = 0;

                    cm = new SqlCommand("SELECT s.invoiceNo, s.quantity AS SalesQty, p.ProductID, SUM(p.Quantity) AS TotalProductionQty, s.ProductID, s.PDetailsID\r\nFROM tblSalesDetails s INNER JOIN tblProductionDetails p ON p.ProductID = s.ProductID WHERE s.invoiceNo = @invoice GROUP BY s.invoiceNo, s.quantity, p.ProductID, s.ProductID, s.PDetailsID", con);
                    cm.Parameters.AddWithValue("@invoice", InvoiceCbx1.Text);
                    con.Open();
                    reader = cm.ExecuteReader();

                    while (reader.Read())
                    {
                        TransitOrder transitOrder = new TransitOrder();
                        productId = Convert.ToInt32(reader.GetValue(4));
                        totalSalesQty = Convert.ToInt32(reader.GetValue(1));
                        totalProductionQty = Convert.ToInt32(reader.GetValue(3));

                        transitOrder.ProductId = productId;
                        transitOrder.TotalSalesQty = totalSalesQty;
                        transitOrder.TotalProductionQty = totalProductionQty;
                        transitOrderList.Add(transitOrder);
                    }

                    reader.Close();

                    foreach (TransitOrder transitItem in transitOrderList)
                    {
                        if (totalSalesQty <= totalProductionQty)
                        {
                            cm = new SqlCommand("SELECT ProductionID, Quantity, ProductID FROM tblProductionDetails WHERE ProductID = @productId ORDER BY ProductID", con);
                            cm.Parameters.AddWithValue("@productId", transitItem.ProductId);
                            reader = cm.ExecuteReader();

                            int toCal = transitItem.TotalSalesQty;

                            while (reader.Read())
                            {
                                Production production1 = new Production();
                                int productionID = Convert.ToInt32(reader.GetValue(0));
                                int productionQty = Convert.ToInt32(reader.GetValue(1));
                                int productID = Convert.ToInt32(reader.GetValue(2));

                                production1.ProductionID = productionID;
                                production1.Quantity = productionQty;
                                production1.ProductID = productID;
                                production1.QtyUpdate = Math.Min(toCal, productionQty);
                                pidList.Add(production1);

                                toCal -= production1.QtyUpdate;
                                if (toCal <= 0)
                                {
                                    break;
                                }
                            }
                            reader.Close();
                            flagg = 1;
                        }
                        else
                        {
                            MessageBox.Show("Error: Insufficient stock for an Item");
                        }
                        string a = null;
                        foreach (Production prod in pidList)
                        {
                            bool isFirstProduction = pidList.IndexOf(prod) == 0;
                            bool isFirstProduction1 = prod.ProductID == 0;

                            bool flag = false;
                            if (a == null || a != prod.ProductID + "")
                            {
                                a = prod.ProductID + "";
                                flag = true;
                            }
                            else
                            {
                                flag = false;
                            }

                            if (flag)
                            {
                                cm.Parameters.Clear();
                                cm = new SqlCommand("UPDATE tblSalesDetails SET pDetailsID = @pDetails WHERE invoiceNo = @invoiceNo AND ProductID = @productId", con);
                                cm.Parameters.AddWithValue("@pDetails", prod.ProductionID);
                                cm.Parameters.AddWithValue("@invoiceNo", InvoiceCbx1.Text);
                                cm.Parameters.AddWithValue("@productId", prod.ProductID);
                                cm.ExecuteNonQuery();

                                cm.Parameters.Clear();
                                SqlCommand cm1 = new SqlCommand("UPDATE tblSalesDetails SET quantity = @qty WHERE invoiceNo = @invoice AND ProductID = @productId", con);
                                cm1.Parameters.AddWithValue("@qty", prod.QtyUpdate);
                                cm1.Parameters.AddWithValue("@invoice", InvoiceCbx1.Text);
                                cm1.Parameters.AddWithValue("@productId", prod.ProductID);
                                cm1.ExecuteNonQuery();
                            }
                            else
                            {
                                cm.Parameters.Clear();
                                cm = new SqlCommand("INSERT INTO tblSalesDetails (invoiceNo, productID, quantity, pDetailsID) VALUES (@invoiceId,@productId,@quantity,@productionID)", con);
                                cm.Parameters.AddWithValue("@invoiceId", InvoiceCbx1.Text);
                                cm.Parameters.AddWithValue("@productId", prod.ProductID);
                                cm.Parameters.AddWithValue("@quantity", prod.QtyUpdate);
                                cm.Parameters.AddWithValue("@productionID", prod.ProductionID);
                                cm.ExecuteNonQuery();
                            }
                        }
                    }
                    cm.Parameters.Clear();
                    cm = new SqlCommand("With Updater as (SELECT PDetailsID, SUM(Qty) AS Qty FROM (Select PDetailsID, -(quantity) AS Qty from tblSalesDetails\r\nwhere invoiceNo = @invoiceId UNION SELECT ProductionID, Quantity FROM tblProductionDetails) updatedTbl GROUP BY PDetailsID)\r\nUpdate tblProductionDetails set Quantity = \r\n(SELECT Qty FROM Updater WHERE Updater.PDetailsID = tblProductionDetails.ProductionID)", con);
                    cm.Parameters.AddWithValue("@invoiceId", InvoiceCbx1.Text);
                    cm.ExecuteNonQuery();

                    con.Close();
                    AddAuditLog("updated an order status to transit");
                    SOrderUpdate.Visible = false;
                }
                else if (SOrderStatusCbx1.Text.Equals("Delivery Complete"))
                {
                    ArrayList completedList = new ArrayList();
                    cm = new SqlCommand("SELECT sd.invoiceNo, sd.ProductID, i.UnitPrice FROM tblSalesDetails sd INNER JOIN tblItem i ON sd.ProductID = i.ProductID WHERE invoiceNo = @invoiceId", con);
                    cm.Parameters.AddWithValue("@invoiceId", InvoiceCbx1.Text);
                    con.Open();
                    reader = cm.ExecuteReader();
                    while (reader.Read())
                    {
                        Complete order = new Complete();
                        int invoiceNo = Convert.ToInt32(reader.GetValue(0));
                        int productID = Convert.ToInt32(reader.GetValue(1));
                        double price = Convert.ToDouble(reader.GetValue(2));

                        order.Invoice = invoiceNo;
                        order.ProductID = productID;
                        order.UnitPrice = price;
                        completedList.Add(order);
                    }
                    reader.Close();
                    con.Close();

                    foreach (Complete order in completedList)
                    {
                        SqlCommand cmd = new SqlCommand("INSERT INTO tblCompleteOrders (invoiceNo, productID, UnitPrice) VALUES (@invoiceId,@productId,@UnitPrice)", con);
                        cmd.Parameters.AddWithValue("@invoiceId", order.Invoice);
                        cmd.Parameters.AddWithValue("@productId", order.ProductID);
                        cmd.Parameters.AddWithValue("@UnitPrice", order.UnitPrice);
                        con.Open();
                        cmd.ExecuteNonQuery();
                        con.Close();
                    }

                    AddAuditLog("completed an order");
                    SOrderUpdate.Visible = false;
                    flagg = 1;
                }
                else if (SOrderStatusCbx1.Text.Equals("Cancelled"))
                {
                    AddAuditLog("cancelled an order");
                    SOrderUpdate.Visible = false;
                    flagg = 1;
                }
                else
                {
                    MessageBox.Show("Invalid Input");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (flagg == 1)
                {
                    cm.Parameters.Clear();
                    cm = new SqlCommand("UPDATE tblSales SET iStatus = @stats WHERE invoiceNo = @invoice", con);
                    cm.Parameters.AddWithValue("@stats", SOrderStatusCbx1.Text);
                    cm.Parameters.AddWithValue("@invoice", InvoiceCbx1.Text);
                    con.Open();
                    cm.ExecuteNonQuery();
                    con.Close();
                    FillOrder();
                }
            }
            FillSummary();
            FillsProduction();
            Clear();
            overView();
        }
        private void NewSOrderSave_Click(object sender, EventArgs e)
        {
            int orderId = OrderId();
            int invoiceNo = InvoiceId();
            Customer cus = SCustomerCBX.SelectedItem as Customer;
            Item item = SProductCBX.SelectedItem as Item;
            try
            {
                cm = new SqlCommand("INSERT INTO tblSales(orderID,employeeID,iStatus,invoiceDate,customerID,dueDate) VALUES (@orderId,@empId,@iStatus,@iDate,@customerId,@DDate)", con);
                cm.Parameters.AddWithValue("@orderId", orderId);
                cm.Parameters.AddWithValue("@empId", EmployeeID);
                cm.Parameters.AddWithValue("@iStatus", "Order Placed");
                cm.Parameters.AddWithValue("@iDate", DateTime.Now);
                cm.Parameters.AddWithValue("@customerId", cus.cId);
                cm.Parameters.AddWithValue("@DDate", this.DueDate.Text);
                con.Open();
                cm.ExecuteNonQuery();
                con.Close();

                foreach (ListViewItem lst_item in OrderList.Items)
                {
                    cm = new SqlCommand("INSERT INTO tblSalesDetails (invoiceNo, productID, quantity) VALUES (@invoiceId,@productId,@quantity)", con);
                    int quantity = Convert.ToInt32(lst_item.SubItems[0].Text);
                    int productId = Convert.ToInt32(lst_item.SubItems[2].Text);

                    cm.Parameters.AddWithValue("@invoiceId", invoiceNo);
                    cm.Parameters.AddWithValue("@productId", productId);
                    cm.Parameters.AddWithValue("@quantity", quantity);
                    con.Open();
                    cm.ExecuteNonQuery();
                    con.Close();
                }

                AddAuditLog("inserted a new order");
            }
            catch (Exception c)
            {
                MessageBox.Show("Error: " + c.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                overView();
                FillOrder();
                Clear();
            }
        }
        private void NewSOrderAdd_Click(object sender, EventArgs e)
        {
            ListViewItem newItem;
            Item item = SProductCBX.SelectedItem as Item;
            int qty = Convert.ToInt32(TxtQuantity.Text);
            bool flag = true;
            try
            {
                if (!String.IsNullOrEmpty(TxtQuantity.Text) && !String.IsNullOrEmpty(item.ToString()))
                {
                    foreach (ListViewItem lst_item in OrderList.Items)
                    {
                        if (lst_item.SubItems[1].Text == item.itemOnly)
                        {
                            int quantity = Convert.ToInt32(lst_item.SubItems[0].Text) + Convert.ToInt32(qty.ToString());
                            OrderList.Items.RemoveAt(lst_item.Index);

                            newItem = new ListViewItem(quantity.ToString());
                            newItem.SubItems.Add(item.itemOnly);
                            OrderList.Items.Add(newItem);
                            newItem.SubItems.Add(item.iId + "");
                            flag = false;
                            break;
                        }
                    }
                    if (flag)
                    {
                        newItem = new ListViewItem(qty.ToString());
                        newItem.SubItems.Add(item.itemOnly);
                        OrderList.Items.Add(newItem);
                        newItem.SubItems.Add(item.iId + "");
                    }
                }
                else if (String.IsNullOrEmpty(TxtQuantity.Text) || String.IsNullOrEmpty(item.ToString()))
                {
                    throw new ArgumentNullException();
                }
            }
            catch (Exception c)
            {
                MessageBox.Show("Error: " + c.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void SOrderClear_Click(object sender, EventArgs e)
        {
            OrderList.Items.Clear();
        }
        private void SOrderSearch_Click(object sender, EventArgs e)
        {
            try
            {
                OSalesTbl.Rows.Clear();
                string searchTerm = TxtOrderSearch.Text.Trim();
                string query = @"SELECT 
                s.invoiceDate AS OrderDate,
                s.orderID AS OrderNo,
                s.invoiceNo AS InvoiceNo,
                c.fName + ' ' + c.lName AS Customer,
                s.iStatus AS Status,
                s.dueDate AS DueDate,
                s.employeeId AS EmployeeID
                FROM 
                tblSales s
                INNER JOIN 
                tblCustomer c ON s.customerID = c.customerID
                WHERE 
                (c.fName + ' ' + c.lName LIKE '%' + @SearchTerm + '%' OR 
                c.fName LIKE '%' + @SearchTerm + '%' OR 
                c.lName LIKE '%' + @SearchTerm + '%' OR 
                s.orderID LIKE '%' + @SearchTerm + '%' OR 
                s.invoiceNo LIKE '%' + @SearchTerm + '%' OR 
                s.dueDate LIKE '%' + @SearchTerm + '%')
                ORDER BY 
                s.dueDate;";

                cm = new SqlCommand(query, con);
                cm.Parameters.AddWithValue("@SearchTerm", "%" + searchTerm + "%");
                con.Open();

                using (SqlDataReader reader = cm.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        DateTime orderDate = Convert.ToDateTime(reader["OrderDate"]);
                        DateTime dueDate = Convert.ToDateTime(reader["DueDate"]);

                        OSalesTbl.Rows.Add(orderDate.ToString("MM/dd/yyyy"),
                                          reader["OrderNo"].ToString(),
                                          reader["InvoiceNo"].ToString(),
                                          reader["Customer"].ToString(),
                                          reader["Status"].ToString(),
                                          dueDate.ToString("MM/dd/yyyy"),
                                          reader["EmployeeID"].ToString());
                    }
                }
                OrderCount.Text = $"Total records: {OSalesTbl.Rows.Count}";
                con.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (con.State == ConnectionState.Open) con.Close();
            }
        }
        private void InvoiceCbx1_SelectedIndexChanged(object sender, EventArgs e)
        {
            OrderCBX();
        }
        // AUDIT LOG CODE STARTS HERE
        // --------------------------------------------------------
        public void AddAuditLog(string action)
        {
            try
            {
                con.Open();
                string query = "INSERT INTO tblAuditLog (Action, employeeID) VALUES (@Action, @employeeID)";

                using (SqlCommand cm = new SqlCommand(query, con))
                {
                    cm.Parameters.AddWithValue("@Action", action);
                    cm.Parameters.AddWithValue("@employeeID", EmployeeID);
                    cm.ExecuteNonQuery();
                }
                con.Close();
                LoadAuditLogHistory();
            }
            catch (FormatException a)
            {
                MessageBox.Show("Error: " + a.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void LoadAuditLogHistory()
        {
            try
            {
                {
                    con.Open();
                    string query = @"
                    SELECT 
                        al.LogID, 
                        al.Timestamp, 
                        al.employeeID, 
                        CONCAT(e.fName, ' ', e.lName, ' ', al.Action) AS Action
                    FROM 
                        tblAuditLog al
                    INNER JOIN 
                        tblEmployee e ON al.employeeID = e.employeeID
                    ORDER BY 
                        al.Timestamp ASC";

                    SqlDataAdapter adapter = new SqlDataAdapter(query, con);
                    DataTable dataTable = new DataTable();
                    adapter.Fill(dataTable);

                    // Bind the DataTable to the DataGridView to display data
                    AuditLogTbl.DataSource = dataTable;

                    AuditLogTbl.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

                    AuditLogTbl.Columns["LogID"].FillWeight = 10;
                    AuditLogTbl.Columns["Timestamp"].FillWeight = 25;
                    AuditLogTbl.Columns["Action"].FillWeight = 40;
                    AuditLogTbl.Columns["employeeID"].FillWeight = 10;
                    AuditCount.Text = $"Total records: {AuditLogTbl.Rows.Count}";
                    con.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void AuditSearch_Click(object sender, EventArgs e)
        {
            try
            {
                con.Open();
                string query = @"

            SELECT 

                al.LogID, 

                al.Timestamp, 

                al.employeeID, 

            CONCAT(e.fName, ' ', e.lName, ' ', al.Action) AS Action

            FROM 

                tblAuditLog al

            INNER JOIN 

                tblEmployee e ON al.employeeID = e.employeeID

            WHERE 

                al.Action LIKE @SearchText OR 

                CAST(al.employeeID AS NVARCHAR) LIKE @SearchText

            ORDER BY 

                al.Timestamp ASC";

                using (SqlCommand cm = new SqlCommand(query, con))
                {
                    cm.Parameters.AddWithValue("@SearchText", $"%{TxtAuditSearch.Text.Trim()}%");
                    SqlDataAdapter adapter = new SqlDataAdapter(cm);
                    DataTable dataTable = new DataTable();
                    adapter.Fill(dataTable);
                    AuditLogTbl.DataSource = dataTable;
                    AuditCount.Text = $"Total Records: {dataTable.Rows.Count}";
                }
            }
            catch (FormatException a)
            {
                MessageBox.Show("Error: " + a.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (NullReferenceException d)
            {
                MessageBox.Show("Error: " + d.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (ArgumentNullException)
            {
                MessageBox.Show("Error: Value can not be Null.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                con.Close();
            }
        }
        // ITEM CODE STARTS HERE
        // --------------------------------------------------------
        public void LoadItems()
        {
            try
            {
                ItemTbl.Rows.Clear();
                cm = new SqlCommand("SELECT ProductID, ProductName, UnitPrice, Description, ProductionPoint FROM tblItem", con);
                con.Open();
                reader = cm.ExecuteReader();
                while (reader.Read())
                {
                    int productId = Convert.ToInt32(reader["ProductID"]);
                    string productName = reader["ProductName"].ToString();
                    string description = reader["Description"].ToString();
                    String unitPrice = Convert.ToDecimal(reader["UnitPrice"]).ToString("C2");
                    string category = reader["ProductionPoint"].ToString();
                    int rowIndex = ItemTbl.Rows.Add(productName, description, unitPrice, category);
                    ItemTbl.Rows[rowIndex].Tag = productId;
                }
                reader.Close();
                con.Close();
                ItemCount.Text = "Total records: " + ItemTbl.Rows.Count.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Clear();
                FillsProduction();
            }
        }
        private void bttnUpdte_Click(object sender, EventArgs e)
        {
            Item item = comboBox_ProductID.SelectedItem as Item;
            decimal price = Convert.ToDecimal(TxtbxUntPrc.Text);
            try
            {
                string updateQuery = "UPDATE tblItem SET UnitPrice = @UnitPrice WHERE ProductID = @ProductID";

                SqlCommand cm = new SqlCommand(updateQuery, con);
                cm.Parameters.AddWithValue("@UnitPrice", price);
                cm.Parameters.AddWithValue("@ProductID", item.iId);

                con.Open();
                cm.ExecuteNonQuery();
                con.Close();
            }
            catch (NullReferenceException d)
            {
                MessageBox.Show("Error: " + d.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (ArgumentNullException)
            {
                MessageBox.Show("Error: Value can not be Null.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                LoadItems();
                Clear();
            }
            pnl_UpdtPrc.Visible = false;
        }
        public void Item1CBX()
        {
            Item item = new Item();
            ArrayList itemList = new ArrayList();
            con.Open();
            cm = new SqlCommand("SELECT ProductID, ProductName, Description FROM tblItem", con);

            SqlDataReader reader = cm.ExecuteReader();
            while (reader.Read())
            {
                item = new Item();
                item.iId = Convert.ToInt32(reader.GetValue(0));
                item.iName = reader.GetValue(1) + " (" + reader.GetValue(2) + ")";
                item.itemOnly = reader.GetValue(1) + "";
                itemList.Add(item);
            }
            comboBox_ProductID.DataSource = itemList;
            comboBox_ProductID.ValueMember = "iID";
            comboBox_ProductID.DisplayMember = "iName";
            comboBox_ProductID.SelectedIndex = -1;
            comboBox_ProductID.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            comboBox_ProductID.AutoCompleteSource = AutoCompleteSource.ListItems;
            con.Close();
        }
        private void bttnAddItm_Click(object sender, EventArgs e)
        {
            try
            {
                string productName = txtbx_Nme.Text;
                decimal price = Convert.ToDecimal(txtbx_Prc.Text);
                string description = txtbx_Dscrptn.Text;
                int productionPoint = Convert.ToInt32(txtbx_ROrdrLvl.Text);

                string query = "INSERT INTO tblItem (ProductName, UnitPrice, Description, ProductionPoint) VALUES (@ProductName, @UnitPrice, @Description, @ProductionPoint)";
                // Create the SqlCommand object
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@ProductName", productName);
                    cmd.Parameters.AddWithValue("@UnitPrice", price);
                    cmd.Parameters.AddWithValue("@Description", description);
                    cmd.Parameters.AddWithValue("ProductionPoint", productionPoint);

                    con.Open();
                    cmd.ExecuteNonQuery();
                    con.Close();
                }
            }
            catch (FormatException a)
            {
                MessageBox.Show("Error: " + a.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (NullReferenceException d)
            {
                MessageBox.Show("Error: " + d.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (ArgumentNullException)
            {
                MessageBox.Show("Error: Value can not be Null.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error adding item: " + ex.Message);
            }
            finally
            {
                LoadItems();
                Item1CBX();
                Clear();
            }
        }
        public void LoadProductIDs()
        {
            comboBox_ProductID.Items.Clear();
            string query = "SELECT ProductID, ProductName FROM tblItem";
            SqlCommand cmd = new SqlCommand(query, con);
            con.Open();
            reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                comboBox_ProductID.Items.Add($"{reader["ProductID"]} - {reader["ProductName"]}");
            }
            reader.Close();
            con.Close();
        }
        private void TxtItemSearch_TextChanged(object sender, EventArgs e)
        {
            string searchText = TxtItemSearch.Text.Trim().ToLower();
            foreach (DataGridViewRow row in ItemTbl.Rows)
            {
                if (row.IsNewRow) continue;
                string productName = row.Cells[0].Value.ToString().ToLower();
                if (productName.Contains(searchText))
                {
                    row.Visible = true;
                }
                else
                {
                    row.Visible = false;
                }
            }
        }
        private void NewItemBtn_Click(object sender, EventArgs e)
        {
            if (pnl_AddItem.Visible == false)
            {
                pnl_AddItem.Visible = true;
            }
            else
            {
                pnl_AddItem.Visible = false;
            }
        }
        private void ItemUpdateBtn_Click(object sender, EventArgs e)
        {
            if (pnl_UpdtPrc.Visible == false)
            {
                pnl_UpdtPrc.Visible = true;
            }
            else
            {
                pnl_UpdtPrc.Visible = false;
            }
        }
        private void AddItmCnclbttn_Click(object sender, EventArgs e)
        {
            pnl_AddItem.Visible = false;
        }
        private void UppdtBtncncl_Click(object sender, EventArgs e)
        {
            pnl_UpdtPrc.Visible = false;
        }
        private void ItemCloseBtn_Click(object sender, EventArgs e)
        {
            Item.Visible = false;
        }
        // INVOICE CODE STARTS HERE
        // --------------------------------------------------------
        private void btnClose_Click(object sender, EventArgs e)
        {
            panel5.Visible = false;
        }
        public void PopulateEmployeeInvoicesGrid()
        {
            string query = @"
        SELECT 
            s.invoiceDate AS InvoiceDate,
            co.invoiceNo AS InvoiceNo,
            c.fName + ' ' + c.lName AS CustomerName,
            s.dueDate AS DueDate,
            SUM(sd.Quantity * co.UnitPrice) AS TotalAmount
        FROM tblCompleteOrders co
        INNER JOIN tblSales s ON co.invoiceNo = s.invoiceNo
        INNER JOIN tblCustomer c ON s.customerID = c.customerID
        INNER JOIN tblSalesDetails sd ON co.invoiceNo = sd.invoiceNo AND co.ProductID = sd.ProductID
        INNER JOIN tblItem i ON co.ProductID = i.ProductID
        WHERE s.iStatus = 'Delivery Complete'
        GROUP BY s.invoiceDate, co.invoiceNo, c.fName, c.lName, s.dueDate
        ORDER BY s.invoiceDate DESC";

            try
            {
                using (SqlConnection connect = new SqlConnection(@"Data Source=.\SQLEXPRESS;Initial Catalog=IMS;Integrated Security=True"))
                {
                    connect.Open();
                    using (SqlCommand cmd = new SqlCommand(query, connect))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            InvoiceTbl.Rows.Clear();

                            while (reader.Read())
                            {
                                try
                                {
                                    DateTime invoiceDate = Convert.ToDateTime(reader["InvoiceDate"]);
                                    string invoiceNo = reader["InvoiceNo"].ToString();
                                    string customerName = reader["CustomerName"].ToString();
                                    DateTime dueDate = Convert.ToDateTime(reader["DueDate"]);
                                    decimal totalAmount = Convert.ToDecimal(reader["TotalAmount"]);

                                    InvoiceTbl.Rows.Add(
                                        invoiceDate.ToString("MMMM dd, yyyy"),
                                        invoiceNo,
                                        customerName,
                                        dueDate.ToString("MMMM dd, yyyy"),
                                        totalAmount.ToString("C2")
                                    );
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show($"Error while reading row data: {ex.Message}");
                                }
                            }
                        }
                    }
                    connect.Close();
                }
                InvoiceCount.Text = $"Total records: {InvoiceTbl.Rows.Count}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }
        private void InvoiceTbl_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (InvoiceTbl.CurrentCell.OwningColumn.Name == "print")
            {
                panel5.Visible = true;

                int orderNo = Convert.ToInt32(InvoiceTbl.CurrentRow.Cells["invoiceNo"].Value.ToString());
                DateTime invoiceDate = Convert.ToDateTime(InvoiceTbl.CurrentRow.Cells["invoicedate"].Value.ToString());
                string customerName = InvoiceTbl.CurrentRow.Cells["customername"].Value.ToString();
                lblOrder.Text = InvoiceTbl.CurrentRow.Cells["invoiceNo"].Value.ToString();

                var items = GetOrderItems(orderNo);

                SetInvoiceData(invoiceDate, customerName, items);
            }
        }
        public void SetInvoiceData(DateTime date, string customerName, List<(string productName, int qty, decimal unitPrice)> items)
        {
            lblDate.Text = date.ToString("MMMM dd, yyyy");
            lblName.Text = customerName.ToString();

            dataGridView2.Rows.Clear();
            foreach (var item in items)
            {
                decimal total = item.qty * item.unitPrice;
                dataGridView2.Rows.Add(item.productName, item.qty, item.unitPrice.ToString("C2"), total.ToString("C2"));
            }
            decimal grandTotal = items.Sum(i => i.qty * i.unitPrice);
            lblSubTotal.Text = grandTotal.ToString("C2");
        }
        private List<(string productName, int qty, decimal unitPrice)> GetOrderItems(int orderNo)
        {
            var items = new List<(string productName, int qty, decimal unitPrice)>();

            string query = @"
                SELECT i.ProductName, sd.Quantity, i.UnitPrice
                FROM tblSalesDetails sd
                INNER JOIN tblItem i ON sd.ProductID = i.ProductID
                WHERE sd.invoiceNo = @OrderNo;";

            try
            {
                using (SqlConnection connect = new SqlConnection(@"Data Source=.\SQLEXPRESS;Initial Catalog=IMS;Integrated Security=True"))
                {
                    connect.Open();
                    using (SqlCommand cmd = new SqlCommand(query, connect))
                    {
                        cmd.Parameters.AddWithValue("@OrderNo", orderNo);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string productName = reader["ProductName"].ToString();
                                int quantity = Convert.ToInt32(reader["quantity"]);
                                decimal unitPrice = Convert.ToDecimal(reader["UnitPrice"]);
                                items.Add((productName, quantity, unitPrice));
                            }
                        }
                    }
                    connect.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while fetching order items: {ex.Message}");
            }
            return items;
        }
        private void btnPrint_Click(object sender, EventArgs e)
        {
            PrintPreviewDialog previewDialog = new PrintPreviewDialog();
            previewDialog.Document = printDocument1;
            previewDialog.ShowDialog();
        }
        private void printDocument1_PrintPage(object sender, System.Drawing.Printing.PrintPageEventArgs e)
        {
            Graphics g = e.Graphics;

            Font titleFont = new Font("Arial", 18, FontStyle.Bold);
            Font headerFont = new Font("Arial", 12, FontStyle.Bold);
            Font contentFont = new Font("Arial", 10, FontStyle.Regular);

            int pageWidth = e.PageBounds.Width;
            int receiptWidth = 500;
            int startX = (pageWidth - receiptWidth) / 2;
            int startY = 40;
            int offsetY = 30;

            g.DrawString("LUMINAIRE", titleFont, Brushes.Black, startX + (receiptWidth / 2) - 70, startY);
            offsetY += 50;

            g.DrawString($"Order No: {lblOrder.Text}", contentFont, Brushes.Black, startX, startY + offsetY);
            offsetY += 25;
            g.DrawString($"Date: {lblDate.Text}", contentFont, Brushes.Black, startX, startY + offsetY);
            offsetY += 25;
            g.DrawString($"Customer Name: {lblName.Text}", contentFont, Brushes.Black, startX, startY + offsetY);
            offsetY += 40;

            g.DrawString("Product Name", headerFont, Brushes.Black, startX, startY + offsetY);
            g.DrawString("Qty", headerFont, Brushes.Black, startX + 200, startY + offsetY);
            g.DrawString("Unit Price", headerFont, Brushes.Black, startX + 300, startY + offsetY);
            g.DrawString("Total", headerFont, Brushes.Black, startX + 400, startY + offsetY);
            offsetY += 25;

            g.DrawLine(Pens.Black, startX, startY + offsetY, startX + receiptWidth, startY + offsetY);
            offsetY += 10;

            int rowCounter = 0;
            for (int i = currentRow; i < dataGridView2.Rows.Count; i++)
            {
                DataGridViewRow row = dataGridView2.Rows[i];
                if (row.IsNewRow) continue;

                g.DrawString(row.Cells[0].Value.ToString(), contentFont, Brushes.Black, startX, startY + offsetY);
                g.DrawString(row.Cells[1].Value.ToString(), contentFont, Brushes.Black, startX + 200, startY + offsetY);
                g.DrawString(row.Cells[2].Value.ToString(), contentFont, Brushes.Black, startX + 300, startY + offsetY);
                g.DrawString(row.Cells[3].Value.ToString(), contentFont, Brushes.Black, startX + 400, startY + offsetY);

                offsetY += 25;
                rowCounter++;

                if (rowCounter >= maxRowsPerPage)
                {
                    currentRow = i + 1;
                    e.HasMorePages = true;
                    return;
                }
            }

            offsetY += 30;
            g.DrawString($"Total Amount: {lblSubTotal.Text}", headerFont, Brushes.Black, startX, startY + offsetY);

            currentRow = 0;
            e.HasMorePages = false;
        }
        private void InvoiceSearch_Click(object sender, EventArgs e)
        {
            SearchInvoice();
        }
        public void SearchInvoice()
        {
            string searchTerm = TxtInvoiceSearch.Text.Trim();

            string query = @"
            SELECT s.InvoiceDate, s.orderID, s.InvoiceNo, 
                   c.fName + ' ' + c.lName AS CustomerName, 
                   s.dueDate,
                   SUM(sd.Quantity * i.UnitPrice) AS TotalAmount
            FROM tblSales s
            INNER JOIN tblSalesDetails sd ON s.InvoiceNo = sd.InvoiceNo
            INNER JOIN tblItem i ON sd.ProductID = i.ProductID
            INNER JOIN tblCustomer c ON s.customerID = c.customerID
            WHERE (s.InvoiceNo LIKE @SearchTerm OR 
                   c.fName + ' ' + c.lName LIKE @SearchTerm)
                   AND s.iStatus = 'Delivery Complete'
            GROUP BY s.InvoiceNo, s.InvoiceDate, s.orderID, c.fName, c.lName, s.dueDate
            ORDER BY s.InvoiceDate DESC";

            try
            {
                con.Open();
                cm = new SqlCommand(query, con);
                cm.Parameters.AddWithValue("@SearchTerm", "%" + searchTerm + "%");
                reader = cm.ExecuteReader();

                if (!reader.HasRows)
                {
                    MessageBox.Show("No completed invoices found.");
                    return;
                }

                InvoiceTbl.Rows.Clear();

                while (reader.Read())
                {
                    DateTime invoiceDate = Convert.ToDateTime(reader["InvoiceDate"]);
                    string invoiceNumber = reader["InvoiceNo"].ToString();
                    string customerName = reader["CustomerName"].ToString();
                    DateTime dueDate = Convert.ToDateTime(reader["DueDate"]);
                    decimal totalAmount = Convert.ToDecimal(reader["TotalAmount"]);

                    InvoiceTbl.Rows.Add(
                        invoiceDate.ToString("MMMM dd, yyyy"),
                        invoiceNumber,
                        customerName,
                        dueDate.ToString("MMMM dd, yyyy"),
                        totalAmount.ToString("C2")
                    );
                }

                InvoiceCount.Text = $"Total records: {InvoiceTbl.Rows.Count}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
            finally
            {
                con.Close();
                reader.Close();
            }
        }
        // DASHBOARD CODE STARTS HERE
        // --------------------------------------------------------
        public void overView()
        {
            con.Open();
            string cmd = "SELECT COUNT(*) FROM tblSales WHERE iStatus = ";
            cm = new SqlCommand($"{cmd} 'Order Placed'", con);
            OrderPlacedCount.Text = cm.ExecuteScalar().ToString();
            cm = new SqlCommand($"{cmd} 'In Transit'", con);
            InTransitCount.Text = cm.ExecuteScalar().ToString();
            cm = new SqlCommand($"{cmd} 'Delivery Complete'", con);
            CompletedCount.Text = cm.ExecuteScalar().ToString();
            string cmd1 = "SELECT COUNT(*) FROM tblSales";
            cm = new SqlCommand($"{cmd1}", con);
            TotalSalesCount.Text = cm.ExecuteScalar().ToString();
            string cmd2 = "SELECT COUNT(*) FROM tblCustomer";
            cm = new SqlCommand($"{cmd2}", con);
            TotalCus.Text = cm.ExecuteScalar().ToString();
            string cmd3 = "SELECT COUNT(*) FROM tblItem";
            cm = new SqlCommand($"{cmd3}", con);
            TotalItems.Text = cm.ExecuteScalar().ToString();
            string cmd4 = "SELECT SUM(Quantity) FROM tblProductionDetails";
            cm = new SqlCommand($"{cmd4}", con);
            TotalStock.Text = cm.ExecuteScalar().ToString();
            con.Close();

            string query = "WITH CompletedOrders AS (SELECT s.invoiceNo, co.UnitPrice, ss.quantity FROM tblSales s INNER JOIN tblSalesDetails ss ON s.invoiceNo = ss.invoiceNo\r\n    INNER JOIN tblItem i ON ss.ProductID = i.ProductID \r\n    INNER JOIN tblCompleteOrders co ON s.invoiceNo = co.invoiceNo AND i.ProductID = co.ProductID\r\n    WHERE s.iStatus = 'Delivery Complete'\r\n),\r\nTotalPriceByInvoice AS (\r\n    SELECT invoiceNo, SUM(UnitPrice * quantity) AS totalPrice \r\n    FROM CompletedOrders\r\n    GROUP BY invoiceNo\r\n)\r\nSELECT s.invoiceDate, t.totalPrice \r\nFROM tblSales s\r\nINNER JOIN TotalPriceByInvoice t ON s.invoiceNo = t.invoiceNo\r\nORDER BY s.invoiceDate;";

            SqlDataAdapter da = new SqlDataAdapter(query, con);
            DataSet ds = new DataSet();

            try
            {
                con.Open();
                da.Fill(ds);
                SalesChart.DataSource = ds.Tables[0];
                SalesChart.Series[0].XValueMember = "invoiceDate"; 
                SalesChart.Series[0].YValueMembers = "totalPrice"; 
                SalesChart.DataBind();
            }
            catch (SqlException ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
            finally
            {
                con.Close();
            }
        }
        // INVENTORY SUMMARY CODE STARTS HERE
        // --------------------------------------------------------
        public void FillSummary()
        {
            con.Open();
            InventorySummaryTbl.Rows.Clear();
            string query = @"SELECT i.ProductName AS ItemName, SUM(COALESCE(pd.Quantity, 0)) AS InventoryIn, SUM(COALESCE(sd.Quantity, 0)) AS InventoryOut
            FROM tblItem i LEFT JOIN tblProductionDetails pd ON i.ProductID = pd.ProductID LEFT JOIN ( SELECT sd.ProductID, SUM(COALESCE(sd.Quantity, 0)) AS Quantity
            FROM tblSalesDetails sd INNER JOIN tblSales s ON sd.invoiceNo = s.invoiceNo WHERE s.iStatus = 'Delivery Complete' GROUP BY sd.ProductID) sd ON i.ProductID = sd.ProductID
            GROUP BY i.ProductName";

            cm = new SqlCommand(query, con);
            reader = cm.ExecuteReader();
            while (reader.Read())
            {
                InventorySummaryTbl.Rows.Add(reader[0].ToString(), reader[1].ToString(), reader.GetValue(2));
            }
            SummaryCount.Text = $"Total records: {InventorySummaryTbl.Rows.Count}";
            reader.Close();
            con.Close();
        }
        // ITEM PRODUCTION CODE STARTS HERE
        // --------------------------------------------------------
        private void IProductionNewBtn_Click(object sender, EventArgs e)
        {
            if (pnl_NwPrdctn.Visible == true)
            {
                pnl_NwPrdctn.Visible = false;
            }
            else
            {
                LoadProductCBX();
                pnl_NwPrdctn.Visible = true;
            }
        }
        private void UpdateClose_Click(object sender, EventArgs e)
        {
            UPDTPRDCTN.Visible = false;
        }
        private void bttn_Cancel_Click(object sender, EventArgs e)
        {
            pnl_NwPrdctn.Visible = false;
        }
        private void IProductionUpdateBtn_Click(object sender, EventArgs e)
        {
            if (UPDTPRDCTN.Visible == false)
            {
                LoadPDetailsCBX();
                OperationCBX();
                UPDTPRDCTN.Visible = true;
            }
            else
            {
                UPDTPRDCTN.Visible = false;
            }
        }
        public void OperationCBX()
        {
            operationCB.Items.Clear();
            operationCB.Items.Add("+");
            operationCB.Items.Add("-");
        }
        private void FillsProduction()
        {
            ItemProductionTbl.Rows.Clear();

            if (con.State == ConnectionState.Open)
                con.Close();

            string query = @"
            SELECT p.ArrivalDate, pd.ProductionID AS ProductionNo, pd.ProductID AS ItemID, pr.ProductName AS Product, pd.Quantity
            FROM tblProduction p INNER JOIN tblProductionDetails pd ON p.ProductionID = pd.ProductionID INNER JOIN 
            tblItem pr ON pd.ProductID = pr.ProductID";

            try
            {

                con.Open();
                using (SqlCommand cmd = new SqlCommand(query, con))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        ItemProductionTbl.Rows.Add(
                            Convert.ToDateTime(reader["ArrivalDate"]).ToString("yyyy-MM-dd"),
                            reader["ProductionNo"].ToString(),
                            reader["ItemID"].ToString(),
                            reader["Product"].ToString(),
                            reader["Quantity"].ToString()
                        );
                    }
                }
                ProductionCount.Text = $"Total records: {ItemProductionTbl.Rows.Count}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                con.Close();
                FillLowStockItems();
            }
        }
        private void LoadProductCBX()
        {
            try
            {
                cmbbx_Product.Items.Clear();
                string query = "SELECT ProductID, ProductName FROM tblItem ORDER BY ProductName";
                con.Open();
                cm = new SqlCommand(query, con);
                reader = cm.ExecuteReader();
                while (reader.Read())
                {
                    cmbbx_Product.Items.Add(new KeyValuePair<int, string>(
                        Convert.ToInt32(reader["ProductID"]),
                        reader["ProductName"].ToString()
                    ));
                }
                cmbbx_Product.DisplayMember = "Value";
                cmbbx_Product.ValueMember = "Key";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
            finally
            {
                reader.Close();
                con.Close();
            }
        }
        private void LoadPDetailsCBX()
        {
            try
            {
                productiobCBX.Items.Clear();
                string query = "SELECT DISTINCT ProductionID from tblProductionDetails";
                con.Open();
                cm = new SqlCommand(query, con);
                reader = cm.ExecuteReader();
                while (reader.Read())
                {
                    productiobCBX.Items.Add(Convert.ToInt32(reader["ProductionID"]));
                }
                productiobCBX.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
                productiobCBX.AutoCompleteSource = AutoCompleteSource.ListItems;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
            finally
            {
                reader.Close();
                con.Close();
            }
        }
        private void InsertProductionData()
        {
            if (LVProductList.Items.Count == 0)
            {
                MessageBox.Show("No products to save.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            DateTime arrivalDate = DateTime.Now.AddMonths(1);

            try
            {
                con.Open();
                string insertProductionQuery = @"
                INSERT INTO tblProduction (ArrivalDate) 
                VALUES (@ArrivalDate); 
                SELECT SCOPE_IDENTITY();";

                SqlCommand productionCmd = new SqlCommand(insertProductionQuery, con);
                productionCmd.Parameters.AddWithValue("@ArrivalDate", arrivalDate);

                object result = productionCmd.ExecuteScalar();
                if (result == null || !long.TryParse(result.ToString(), out long productionId))
                {
                    throw new Exception("Failed to retrieve ProductionID.");
                }

                string insertDetailsQuery = @"
                INSERT INTO tblProductionDetails (ProductID, ProductionID, Quantity) 
                VALUES (@ProductID, @ProductionID, @Quantity)";
                foreach (ListViewItem item in LVProductList.Items)
                {
                    if (!long.TryParse(item.SubItems[1].Text, out long productId))
                    {
                        throw new Exception($"Invalid ProductID: {item.SubItems[1].Text}");
                    }

                    if (!long.TryParse(item.SubItems[0].Text, out long quantity))
                    {
                        throw new Exception($"Invalid Quantity: {item.SubItems[0].Text}");
                    }

                    SqlCommand detailsCmd = new SqlCommand(insertDetailsQuery, con);
                    detailsCmd.Parameters.AddWithValue("@ProductID", productId);
                    detailsCmd.Parameters.AddWithValue("@ProductionID", productionId);
                    detailsCmd.Parameters.AddWithValue("@Quantity", quantity);
                    detailsCmd.ExecuteNonQuery();
                }
                LVProductList.Items.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                con.Close();
                Clear();
            }
        }
        private void LoadProductionCbxforUpdate()
        {
            try
            {
                int ProductionId = (int)productiobCBX.SelectedItem;

                cm.Parameters.Clear();
                UpdtPrdctCBX.Items.Clear();
                string query = "SELECT p.ProductionID, i.ProductName, p.ProductID FROM tblItem i INNER JOIN tblProductionDetails p ON p.ProductID = i.ProductID WHERE ProductionID = @pp";
                cm.Parameters.AddWithValue("@pp", ProductionId);

                con.Open();
                cm.CommandText = query;
                reader = cm.ExecuteReader();

                while (reader.Read())
                {
                    UpdtPrdctCBX.Items.Add(new KeyValuePair<int, string>(
                        Convert.ToInt32(reader["ProductID"]),
                        reader["ProductName"].ToString()));
                }

                UpdtPrdctCBX.DisplayMember = "Value";
                UpdtPrdctCBX.ValueMember = "Key";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
            finally
            {
                reader?.Close();
                con.Close();
            }
        }

        private void BTTN_PrdctnUpdt_Click(object sender, EventArgs e)
        {
            UpdateProductionStatus();
            FillsProduction();
        }

        private void bttn_Clear_Click(object sender, EventArgs e)
        {
            LVProductList.Items.Clear();
        }
        private void productiobCBX_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadProductionCbxforUpdate();
        }
        private void bttn_Add_Click(object sender, EventArgs e)
        {
            if (cmbbx_Product.SelectedItem == null)
            {
                MessageBox.Show("Error: Please select a product.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!int.TryParse(txtbx_Qntty.Text, out int qty) || qty <= 0)
            {
                MessageBox.Show("Error: Please enter a valid quantity.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!(cmbbx_Product.SelectedItem is KeyValuePair<int, string> selectedProduct))
            {
                MessageBox.Show("Error: Invalid product selected.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            int productId = selectedProduct.Key;
            string productName = selectedProduct.Value;

            foreach (ListViewItem listViewItem in LVProductList.Items)
            {
                if (listViewItem.SubItems[1].Text == productId.ToString())
                {
                    int existingQty = Convert.ToInt32(listViewItem.SubItems[0].Text);
                    listViewItem.SubItems[0].Text = (existingQty + qty).ToString();

                    txtbx_Qntty.Clear();
                    cmbbx_Product.SelectedIndex = -1;
                    return;
                }
            }

            ListViewItem newItem = new ListViewItem(qty.ToString());
            newItem.SubItems.Add(productId.ToString());

            newItem.SubItems.Add(productName);
            LVProductList.Items.Add(newItem);

            txtbx_Qntty.Clear();
            cmbbx_Product.SelectedIndex = -1;
        }

        private void bttn_Save_Click(object sender, EventArgs e)
        {
            InsertProductionData();
            FillsProduction();
            pnl_NwPrdctn.Visible = false;
        }
        private void UpdateProductionStatus()
        {
            if (UpdtPrdctCBX == null || UpdtPrdctCBX.SelectedItem == null)
            {
                MessageBox.Show("Error: Please select a product.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtbx_QnttyUpdte?.Text))
            {
                MessageBox.Show("Error: Please enter a valid quantity.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (operationCB == null || operationCB.SelectedItem == null)
            {
                MessageBox.Show("Error: Please select an operation type.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                int selectedProductId = ((KeyValuePair<int, string>)UpdtPrdctCBX.SelectedItem).Key;
                int quantityChange;

                if (!int.TryParse(txtbx_QnttyUpdte.Text, out quantityChange))
                {
                    MessageBox.Show("Please enter a valid quantity.");
                    return;
                }

                string operatorType = operationCB.SelectedItem.ToString();
                string reason = Reason.Text;

                string fetchQuery = @"
         SELECT TOP 1 Quantity 
         FROM tblProductionDetails 
         WHERE ProductID = @ProductID";

                con.Open();
                cm = new SqlCommand(fetchQuery, con);
                cm.Parameters.AddWithValue("@ProductID", selectedProductId);

                object result = cm.ExecuteScalar();
                con.Close();

                if (result == null)
                {
                    MessageBox.Show("Product not found in production details.");
                    return;
                }

                int currentQuantity = Convert.ToInt32(result);

                int newQuantity = operatorType == "+"
                    ? currentQuantity + quantityChange
                    : currentQuantity - quantityChange;

                if (newQuantity < 0)
                {
                    MessageBox.Show("Error: Quantity cannot be negative.");
                    return;
                }

                string updateQuery = @"
         UPDATE tblProductionDetails 
         SET Quantity = @NewQuantity 
         WHERE ProductID = @ProductID";

                con.Open();
                cm = new SqlCommand(updateQuery, con);
                cm.Parameters.AddWithValue("@NewQuantity", newQuantity);
                cm.Parameters.AddWithValue("@ProductID", selectedProductId);
                cm.ExecuteNonQuery();
                con.Close();

                MessageBox.Show("Production status updated successfully.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error updating production status: " + ex.Message);
            }
            finally
            {
                if (con.State == ConnectionState.Open)
                    con.Close();
            }

        }

        private void UpdtPrdctCBX_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (UpdtPrdctCBX.SelectedItem != null)
            {
                try
                {
                    int selectedProductId = ((KeyValuePair<int, string>)UpdtPrdctCBX.SelectedItem).Key;
                    

                    string query = @"
                    SELECT TOP 1 Quantity 
                    FROM tblProductionDetails 
                    WHERE PDetailsID = @PDetailsID";

                    con.Open();
                    cm = new SqlCommand(query, con);
                    cm.Parameters.AddWithValue("@PDetailsID", selectedProductId);

                    object result = cm.ExecuteScalar();

                    if (result != null)
                    {
                        txtbx_QnttyUpdte.Text = result.ToString();
                    }
                    else
                    {
                        txtbx_QnttyUpdte.Text = "0";
                        MessageBox.Show("No quantity found for the selected Product ID.");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading quantity: " + ex.Message);
                }
                finally
                {
                    if (con.State == ConnectionState.Open)
                        con.Close();
                }
            }
        }
        // SALES RETURN CODE STARTS HERE
        // --------------------------------------------------------
        private void SReturnUpdateBtn_Click(object sender, EventArgs e)
        {
            if (SReturnUpdateRequestPnl.Visible == true)
            {
                SReturnUpdateRequestPnl.Visible = false;
            }
            else
            {
                SReturnUpdateRequestPnl.Visible = true;
            }
        }
        private void SReturnNewBtn_Click(object sender, EventArgs e)
        {
            if (SReturnNewRequestPnl.Visible == true)
            {
                SReturnNewRequestPnl.Visible = false;
            }
            else
            {
                SReturnNewRequestPnl.Visible = true;
            }
        }
        private void SReturnUpdateStatusCancelBtn_Click(object sender, EventArgs e)
        {
            SReturnUpdateRequestPnl.Visible = false;
        }
        private void SReturnNewRequestCancelBtn_Click(object sender, EventArgs e)
        {
            SReturnNewRequestPnl.Visible = false;
            ClearSalesReturnFields();
        }
        public interface ISalesReturnStrat
        {
            void Execute(System1 context);
        }
        public void SetStrategy(ISalesReturnStrat strategy)
        {
            currentStrategy = strategy;
        }

        public void ExecuteStrategy()
        {
            currentStrategy?.Execute(this);
        }
        public class PopulateInvoiceComboBox : ISalesReturnStrat
        {
            public void Execute(System1 context)
            {

                try
                {
                    context.con.Open();
                    SqlCommand cmd = new SqlCommand("SELECT invoiceNo FROM tblSales WHERE iStatus = 'Delivery Complete'", context.con);
                    SqlDataReader reader = cmd.ExecuteReader();
                    context.SReturnInvoiceNoCmb.Items.Clear();

                    while (reader.Read())
                    {
                        context.SReturnInvoiceNoCmb.Items.Add(reader["invoiceNo"].ToString());
                    }

                    reader.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred while fetching invoice numbers: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    context.con.Close();
                }
            }
        }
        public class PopulateProductIDComboBox : ISalesReturnStrat
        {
            private readonly string selectedInvoiceNo;

            public PopulateProductIDComboBox(string invoiceNo)
            {
                selectedInvoiceNo = invoiceNo;
            }

            public void Execute(System1 context)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(selectedInvoiceNo))
                    {
                        MessageBox.Show("No InvoiceNo selected. Please select an InvoiceNo first.");
                        return;
                    }

                    if (context.SReturnProductNameCmb == null)
                    {
                        MessageBox.Show("Product ID ComboBox is not initialized.");
                        return;
                    }
                    context.con.Open();
                    MessageBox.Show($"Fetching ProductIDs for InvoiceNo: {selectedInvoiceNo}");

                    string query = @"
                    SELECT sd.ProductID, sd.Quantity 
                    FROM tblSalesDetails sd 
                    WHERE sd.InvoiceNo = @InvoiceNo";
                    SqlCommand cmd = new SqlCommand(query, context.con);
                    cmd.Parameters.AddWithValue("@InvoiceNo", selectedInvoiceNo);
                    SqlDataReader reader = cmd.ExecuteReader();

                    context.SReturnProductNameCmb.Items.Clear();
                    context.SReturnQuantitytxt.Text = "";

                    bool hasRecords = false;
                    while (reader.Read())
                    {
                        hasRecords = true;
                        string productID = reader["ProductID"].ToString();
                        context.SReturnProductNameCmb.Items.Add(productID);

                        MessageBox.Show($"Added ProductID: {productID}");
                    }

                    if (!hasRecords)
                    {
                        MessageBox.Show($"No products found for InvoiceNo: {selectedInvoiceNo}");
                    }

                    reader.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error populating ProductID ComboBox: {ex.Message}\n{ex.StackTrace}");
                }
                finally
                {

                    if (context.con.State == ConnectionState.Open)
                        context.con.Close();
                }
            }
        }
        public class SubmitSalesReturn : ISalesReturnStrat
        {
            public void Execute(System1 context)
            {
                try
                {
                    if (context.SReturnInvoiceNoCmb.SelectedItem == null ||
                        context.SReturnProductNameCmb.SelectedItem == null ||
                        string.IsNullOrWhiteSpace(context.SReturnQuantitytxt.Text) ||
                        context.SReturnReasonCmb.SelectedItem == null)
                    {
                        MessageBox.Show("Please fill all fields.");
                        return;
                    }

                    string invoiceNo = context.SReturnInvoiceNoCmb.SelectedItem.ToString();
                    string productName = context.SReturnProductNameCmb.SelectedItem.ToString();
                    string reason = context.SReturnReasonCmb.SelectedItem.ToString();
                    int enteredQuantity;

                    if (!int.TryParse(context.SReturnQuantitytxt.Text, out enteredQuantity) || enteredQuantity <= 0)
                    {
                        MessageBox.Show("Please enter a valid quantity greater than zero.");
                        return;
                    }

                    int employeeID = context.EmployeeID;

                    context.con.Open();

                    string productIDQuery = "SELECT ProductID FROM tblItem WHERE ProductName = @ProductName";
                    SqlCommand productIDCmd = new SqlCommand(productIDQuery, context.con);
                    productIDCmd.Parameters.AddWithValue("@ProductName", productName);
                    object productIDResult = productIDCmd.ExecuteScalar();
                    if (productIDResult == null)
                    {
                        MessageBox.Show("Product not found for the selected ProductName. Please try again.");
                        return;
                    }
                    string productID = productIDResult.ToString();

                    // Retrieve CustomerID
                    string customerQuery = "SELECT CustomerID FROM tblSales WHERE InvoiceNo = @InvoiceNo";
                    SqlCommand customerCmd = new SqlCommand(customerQuery, context.con);
                    customerCmd.Parameters.AddWithValue("@InvoiceNo", invoiceNo);
                    object customerResult = customerCmd.ExecuteScalar();
                    if (customerResult == null)
                    {
                        MessageBox.Show("Customer not found for the selected invoice. Please try again.");
                        return;
                    }
                    string customerID = customerResult.ToString();

                    // Retrieve OrderID
                    string orderIDQuery = "SELECT orderID FROM tblSales WHERE InvoiceNo = @InvoiceNo";
                    SqlCommand orderIDCmd = new SqlCommand(orderIDQuery, context.con);
                    orderIDCmd.Parameters.AddWithValue("@InvoiceNo", invoiceNo);
                    object orderIDResult = orderIDCmd.ExecuteScalar();
                    if (orderIDResult == null)
                    {
                        MessageBox.Show("OrderID not found for the selected invoice. Please try again.");
                        return;
                    }
                    long orderID = (long)orderIDResult;

                    // Check Remaining Quantity
                    string checkQuantityQuery = "SELECT Quantity FROM tblSalesDetails WHERE InvoiceNo = @InvoiceNo AND ProductID = @ProductID";
                    SqlCommand checkQuantityCmd = new SqlCommand(checkQuantityQuery, context.con);
                    checkQuantityCmd.Parameters.AddWithValue("@InvoiceNo", invoiceNo);
                    checkQuantityCmd.Parameters.AddWithValue("@ProductID", productID);
                    object quantityResult = checkQuantityCmd.ExecuteScalar();

                    // Ensure the entered quantity won't exceed the remaining quantity
                    if (quantityResult == null || Convert.ToInt32(quantityResult) < enteredQuantity)
                    {
                        int availableQuantity = (quantityResult == null) ? 0 : Convert.ToInt32(quantityResult);
                        MessageBox.Show($"Entered quantity exceeds the available quantity. Only {availableQuantity} available.",
                                        "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    string insertQuery = @"
                    INSERT INTO tblSalesReturn (InvoiceNo, ProductID, CustomerID, EmployeeID, RStatus, Reason, RequestDate, OrderID, Quantity)
                    VALUES (@InvoiceNo, @ProductID, @CustomerID, @EmployeeID, 'Return Request', @Reason, GETDATE(), @OrderID, @Quantity)";
                    SqlCommand insertCmd = new SqlCommand(insertQuery, context.con);
                    insertCmd.Parameters.AddWithValue("@InvoiceNo", invoiceNo);
                    insertCmd.Parameters.AddWithValue("@ProductID", productID);
                    insertCmd.Parameters.AddWithValue("@CustomerID", customerID);
                    insertCmd.Parameters.AddWithValue("@EmployeeID", employeeID);
                    insertCmd.Parameters.AddWithValue("@Reason", reason);
                    insertCmd.Parameters.AddWithValue("@OrderID", orderID);
                    insertCmd.Parameters.AddWithValue("@Quantity", enteredQuantity);
                    insertCmd.ExecuteNonQuery();

                    // If "Damage Item" on Reason
                    if (reason == "Damage Item")
                    {
                        //// Mark this invoiceNo as discarded and don't allow further updates
                        //string markAsDiscardedQuery = "UPDATE tblSalesReturn SET RStatus = 'Discarded' WHERE InvoiceNo = @InvoiceNo";
                        //SqlCommand discardCmd = new SqlCommand(markAsDiscardedQuery, context.con);
                        //discardCmd.Parameters.AddWithValue("@InvoiceNo", invoiceNo);
                        //discardCmd.ExecuteNonQuery();

                        MessageBox.Show("Item marked as 'Damage Item' it will now be discarded. Recorded for history only and will not affect inventory and productions.",
                                        "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        //context.LoadSalesReturnDataGridVIew();
                        //context.FetchAndPopulateUpdateInvoiceNoComboBox();
                        return;
                    }
                    // Deduct quantity from tblSalesDetails
                    string updateSalesDetailsQuery = "UPDATE tblSalesDetails SET Quantity = Quantity - @Quantity WHERE InvoiceNo = @InvoiceNo AND ProductID = @ProductID";
                    SqlCommand updateSalesDetailsCmd = new SqlCommand(updateSalesDetailsQuery, context.con);
                    updateSalesDetailsCmd.Parameters.AddWithValue("@Quantity", enteredQuantity); // Use enteredQuantity directly
                    updateSalesDetailsCmd.Parameters.AddWithValue("@InvoiceNo", invoiceNo);
                    updateSalesDetailsCmd.Parameters.AddWithValue("@ProductID", productID);
                    updateSalesDetailsCmd.ExecuteNonQuery();

                    context.LoadSalesReturnDataGridVIew();
                    context.FetchAndPopulateUpdateInvoiceNoComboBox();
                    MessageBox.Show("Sales Return Request submitted successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error submitting sales return: {ex.Message}");
                }
                finally
                {
                    if (context.con.State == ConnectionState.Open)
                        context.con.Close();
                }
            }
        }
        private void PopulateReasonComboBox()
        {
            SReturnReasonCmb.Items.Clear();
            SReturnReasonCmb.Items.Add("Damage Item");
            SReturnReasonCmb.Items.Add("Received Wrong Item");
            SReturnReasonCmb.Items.Add("Delivery Delay");
            SReturnReasonCmb.Items.Add("Change of Mind");
        }
        private void PopulateUpdateRStatusComboBox()
        {
            SReturnUpdateStatusCmb.Items.Clear();
            SReturnUpdateStatusCmb.Items.Add("Return in Progress");
            SReturnUpdateStatusCmb.Items.Add("Cancelled");
        }
        private void PopulateQuantityComboBox(string productName)
        {
            try
            {
                if (SReturnInvoiceNoCmb.SelectedItem == null)
                {
                    MessageBox.Show("Please select an invoice number.");
                    return;
                }
                string productID = string.Empty;
                con.Open();
                string productIDQuery = "SELECT ProductID FROM tblItem WHERE ProductName = @ProductName";
                SqlCommand productIDCmd = new SqlCommand(productIDQuery, con);
                productIDCmd.Parameters.AddWithValue("@ProductName", productName);

                object productIDResult = productIDCmd.ExecuteScalar();
                if (productIDResult == null)
                {
                    MessageBox.Show("Product not found for the selected ProductName. Please try again.");
                    return;
                }
                productID = productIDResult.ToString();

                string query = @"
                    SELECT Quantity
                    FROM tblSalesDetails
                    WHERE ProductID = @ProductID AND InvoiceNo = @InvoiceNo";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@ProductID", productID);
                cmd.Parameters.AddWithValue("@InvoiceNo", SReturnInvoiceNoCmb.SelectedItem.ToString());

                SqlDataReader reader = cmd.ExecuteReader();
                SReturnQuantitytxt.Text = "";
                while (reader.Read())
                {
                    int quantity = Convert.ToInt32(reader["Quantity"]);
                    SReturnQuantitytxt.Text = quantity.ToString();
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error populating quantity: {ex.Message}");
            }
            finally
            {
                if (con.State == ConnectionState.Open)
                    con.Close();
            }
        }

        public void LoadSalesReturnDataGridVIew()
        {
            try
            {
                if (con.State == ConnectionState.Open)
                    con.Close();

                con.Open();
                string query = @"
                    SELECT 
                     sr.ReturnID, 
                     sr.EmployeeID,
                     sr.invoiceNo, 
                     p.ProductName,
                    sr.Reason,      
                    sr.RStatus,      
                    sr.RequestDate, 
                    sr.customerID
                FROM tblSalesReturn sr
                INNER JOIN tblItem p ON sr.ProductID = p.ProductID
                ORDER BY sr.RequestDate DESC";

                SqlDataAdapter adapter = new SqlDataAdapter(query, con);
                DataTable dataTable = new DataTable();
                adapter.Fill(dataTable);

                SReturnDataGridView.DataSource = dataTable;
                SReturnDataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                SReturnDataGridView.Columns["ReturnID"].FillWeight = 15;
                SReturnDataGridView.Columns["InvoiceNo"].FillWeight = 15;
                SReturnDataGridView.Columns["ProductName"].FillWeight = 20;
                SReturnDataGridView.Columns["Reason"].FillWeight = 25;
                SReturnDataGridView.Columns["RStatus"].FillWeight = 25;
                SReturnDataGridView.Columns["RequestDate"].FillWeight = 25;
                SReturnDataGridView.Columns["CustomerID"].FillWeight = 15;
                SReturnDataGridView.Columns["EmployeeID"].FillWeight = 15;

                ReturnCount.Text = $"Total records: {SReturnDataGridView.Rows.Count}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading sales return data: {ex.Message}");
            }
            finally
            {
                if (con.State == ConnectionState.Open)
                {
                    con.Close();
                }
            }
        }
        private void SReturnSearch_Click(object sender, EventArgs e)
        {
            try
            {
                con.Open();
                string query = @"
                    SELECT 
                        sr.ReturnID, 
                        sr.EmployeeID,
                        sr.InvoiceNo, 
                        p.ProductName,
                        sr.Reason,      
                        sr.RStatus,      
                        sr.RequestDate, 
                        sr.CustomerID
                    FROM tblSalesReturn sr
                    INNER JOIN tblItem p ON sr.ProductID = p.ProductID
                    WHERE 
                        (CAST(sr.ReturnID AS NVARCHAR) LIKE @SearchText OR 
                        sr.InvoiceNo LIKE @SearchText OR 
                        p.ProductName LIKE @SearchText OR 
                        sr.Reason LIKE @SearchText OR 
                        sr.RStatus LIKE @SearchText OR 
                    CAST(sr.RequestDate AS NVARCHAR) LIKE @SearchText OR 
                    CAST(sr.CustomerID AS NVARCHAR) LIKE @SearchText OR 
                    CAST(sr.EmployeeID AS NVARCHAR) LIKE @SearchText)
                        AND sr.Reason != 'Damage Item' -- Exclude 'Damage Item' records   
                    ORDER BY sr.RequestDate DESC";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {

                    cmd.Parameters.AddWithValue("@SearchText", $"%{TxtReturnSearch.Text.Trim()}%");

                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    DataTable dataTable = new DataTable();
                    adapter.Fill(dataTable);
                    SReturnDataGridView.DataSource = dataTable;

                    SReturnDataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                    SReturnDataGridView.Columns["ReturnID"].FillWeight = 15;
                    SReturnDataGridView.Columns["InvoiceNo"].FillWeight = 15;
                    SReturnDataGridView.Columns["ProductName"].FillWeight = 20;
                    SReturnDataGridView.Columns["Reason"].FillWeight = 25;
                    SReturnDataGridView.Columns["RStatus"].FillWeight = 25;
                    SReturnDataGridView.Columns["RequestDate"].FillWeight = 25;
                    SReturnDataGridView.Columns["CustomerID"].FillWeight = 15;
                    SReturnDataGridView.Columns["EmployeeID"].FillWeight = 15;

                    ReturnCount.Text = $"Total records: {SReturnDataGridView.Rows.Count}";
                    con.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error while searching Sales Return: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void LoadUpdateInvoiceNos()
        {
            string query = @"SELECT DISTINCT invoiceNo FROM tblSalesReturn WHERE RStatus != 'Returned'";

            SqlCommand sqlCommand = new SqlCommand(query, con);

            try
            {
                con.Open();
                SqlDataReader reader = sqlCommand.ExecuteReader();
                SReturnUpdateInvoiceCmb.Items.Clear();

                while (reader.Read())
                {
                    string invoiceNo = reader["invoiceNo"].ToString();
                    SReturnUpdateInvoiceCmb.Items.Add(invoiceNo);
                }

                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading invoice numbers: " + ex.Message);
            }
            finally
            {
                con.Close();
            }
        }
        public void AutoSearchReturn(SReturnAutoinvoiceNoUpdate SReturnProductNameTxt, SReturnAutoStatusComboBox SReturnUpdateStatusCmb, long invoiceNo)
        {
            con.Open();
            string query = "SELECT ReturnID, CustomerID, ProductID, RStatus, RequestDate FROM tblSalesReturn WHERE invoiceNo = @invoiceNo";
            SqlCommand sqlCommand = new SqlCommand(query, con);
            sqlCommand.Parameters.Add("@invoiceNo", SqlDbType.BigInt).Value = invoiceNo;

            try
            {
                SqlDataAdapter adapter = new SqlDataAdapter(sqlCommand);
                DataTable dataTable = new DataTable();
                adapter.Fill(dataTable);

                if (dataTable.Rows.Count > 0)
                {
                    DataRow row = dataTable.Rows[0];
                    long productID = Convert.ToInt64(row["ProductID"]);
                    SReturnProductNameTxt.Text = GetProductName(productID);

                    string currentStatus = row["RStatus"].ToString();
                    SReturnUpdateStatusCmb.Text = currentStatus;
                    UpdateRStatusChoices(currentStatus);
                }
                else
                {
                    MessageBox.Show("No data found for the selected invoice number.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
            finally
            {
                if (con.State == ConnectionState.Open)
                {
                    con.Close();
                }
            }
        }


        public void UpdateRStatus(string newStatus, string invoiceNo, string productName)
        {
            long productID = GetProductID(productName);

            if (productID == -1)
            {
                MessageBox.Show("Product not found!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string query = @"
                UPDATE tblSalesReturn
                SET RStatus = @RStatus
                WHERE InvoiceNo = @InvoiceNo AND ProductID = @ProductID";

            try
            {
                con.Open();
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@RStatus", newStatus);
                cmd.Parameters.AddWithValue("@InvoiceNo", Convert.ToInt64(invoiceNo));
                cmd.Parameters.AddWithValue("@ProductID", productID);

                int rowsAffected = cmd.ExecuteNonQuery();

                if (rowsAffected > 0 && newStatus == "Return in Progress")
                {
                    FetchUpdateProductionQuantity(invoiceNo, productID);
                }
                else if (rowsAffected == 0)
                {
                    MessageBox.Show("No matching record found to update.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
            finally
            {
                con.Close();
            }
        }
        private string GetProductName(long productID)
        {
            string productName = string.Empty;
            string query = "SELECT ProductName FROM tblItem WHERE ProductID = @ProductID";

            using (SqlCommand cmd = new SqlCommand(query, con))
            {
                cmd.Parameters.AddWithValue("@ProductID", productID);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        productName = reader["ProductName"].ToString();
                    }
                }
            }
            return productName;
        }

        private long GetProductID(string productName)
        {
            string query = "SELECT ProductID FROM tblItem WHERE ProductName = @ProductName";
            SqlCommand cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@ProductName", productName);

            try
            {
                con.Open();
                object result = cmd.ExecuteScalar();
                con.Close();

                if (result != null)
                {
                    return Convert.ToInt64(result);
                }
                else
                {
                    return -1;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error retrieving ProductID: " + ex.Message);
                con.Close();
                return -1;
            }
        }

        private void FetchUpdateProductionQuantity(string invoiceNo, long productID)
        {
            string fetchQuantityQuery = @"
                SELECT Quantity
                FROM tblSalesReturn
                WHERE InvoiceNo = @InvoiceNo AND ProductID = @ProductID AND RStatus = 'Return in Progress'";

            try
            {
                int quantityReturned = 0;
                using (SqlConnection fetchCon = new SqlConnection(con.ConnectionString))
                {
                    SqlCommand fetchQuantityCmd = new SqlCommand(fetchQuantityQuery, fetchCon);
                    fetchQuantityCmd.Parameters.AddWithValue("@InvoiceNo", Convert.ToInt64(invoiceNo));
                    fetchQuantityCmd.Parameters.AddWithValue("@ProductID", productID);

                    fetchCon.Open();
                    object result = fetchQuantityCmd.ExecuteScalar();
                    if (result != null)
                    {
                        quantityReturned = Convert.ToInt32(result);
                    }
                }

                if (quantityReturned > 0)
                {
                    string updateProductionQuery = @"
                UPDATE tblProductionDetails
                SET Quantity = Quantity + @QuantityReturned
                WHERE ProductID = @ProductID";

                    using (SqlConnection updateCon = new SqlConnection(con.ConnectionString))
                    {
                        SqlCommand updateProductionCmd = new SqlCommand(updateProductionQuery, updateCon);
                        updateProductionCmd.Parameters.AddWithValue("@QuantityReturned", quantityReturned);
                        updateProductionCmd.Parameters.AddWithValue("@ProductID", productID);

                        updateCon.Open();
                        int rowsAffected = updateProductionCmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            //MessageBox.Show("Production quantity updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show("No matching production record found to update.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error updating production details: " + ex.Message);
            }
        }


        private void FetchAndPopulateUpdateInvoiceNoComboBox()
        {
            try
            {
                con.Open();
                SqlCommand cmd = new SqlCommand("SELECT DISTINCT invoiceNo FROM tblSalesReturn", con);
                SqlDataReader reader = cmd.ExecuteReader();
                SReturnInvoiceNoCmb.Items.Clear();

                while (reader.Read())
                {
                    string invoiceNo = reader["invoiceNo"].ToString();
                    SReturnInvoiceNoCmb.Items.Add(invoiceNo);
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error fetching InvoiceNo: {ex.Message}");
            }
            finally
            {
                con.Close();
            }
        }

        private void SReturnSubmitNewRequestBtn_Click(object sender, EventArgs e)
        {
            try
            {
                SetStrategy(new SubmitSalesReturn());
                ExecuteStrategy();
                string selectedProductID = SReturnProductNameCmb.SelectedItem?.ToString();

                PopulateReasonComboBox();
                if (SReturnInvoiceNoCmb.SelectedItem != null)
                    PopulateQuantityComboBox(selectedProductID);

                ClearSalesReturnFields();
                SReturnProductNameCmb.SelectedIndex = -1;
                SReturnReasonCmb.SelectedIndex = -1;
                SReturnQuantitytxt.Text = "";
                LoadUpdateInvoiceNos();
                AddAuditLog("added a New Sales Return");
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message);
            }
        }
        private void ClearSalesReturnFields()
        {
            SReturnProductNameCmb.SelectedIndex = -1;
            SReturnProductNameCmb.Text = "";
            SReturnReasonCmb.SelectedIndex = -1;
            SReturnReasonCmb.Text = "";
            SReturnInvoiceNoCmb.SelectedIndex = -1;
            SReturnQuantitytxt.Clear();
        }
        private void UpdateRStatusChoices(string currentStatus)
        {
            SReturnUpdateStatusCmb.Items.Clear();

            if (currentStatus == "Return Request")
            {
                SReturnUpdateStatusCmb.Items.Add("Return in Progress");
            }
            else if (currentStatus == "Return in Progress")
            {
                SReturnUpdateStatusCmb.Items.Add("Returned");
            }
            SReturnUpdateStatusCmb.SelectedItem = currentStatus;
        }

        private void SReturnUpdateRequestBtn_Click(object sender, EventArgs e)
        {
            try
            {
                if (SReturnUpdateStatusCmb.SelectedItem == null)
                {
                    MessageBox.Show("Please select a status to update.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string newStatus = SReturnUpdateStatusCmb.SelectedItem.ToString();
                string invoiceNo = SReturnUpdateInvoiceCmb.SelectedItem.ToString();
                string productName = SReturnProductNameTxt.Text;

                if (string.IsNullOrWhiteSpace(invoiceNo) || string.IsNullOrWhiteSpace(productName))
                {
                    MessageBox.Show("Please select a valid Invoice and Product.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                UpdateRStatus(newStatus, invoiceNo, productName);

                // AuditLog for status
                if (newStatus == "Return in Progress")
                {
                    AddAuditLog("updated Return Status to 'Return in Progress'");
                }
                else if (newStatus == "Returned")
                {
                    AddAuditLog("updated Return Status to 'Returned'");
                }

                MessageBox.Show("Status updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadSalesReturnDataGridVIew();
                LoadUpdateInvoiceNos();

                SReturnUpdateInvoiceCmb.SelectedIndex = -1;
                SReturnUpdateStatusCmb.SelectedIndex = -1;
                SReturnProductNameTxt.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error while updating the status: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SReturnUpdateInvoiceCmb_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (SReturnUpdateInvoiceCmb.SelectedItem != null)
            {
                long invoiceNo = Convert.ToInt64(SReturnUpdateInvoiceCmb.SelectedItem.ToString());
                AutoSearchReturn(SReturnProductNameTxt, SReturnUpdateStatusCmb, invoiceNo);
            }
        }

        private void SReturnInvoiceNoCmb_SelectedIndexChanged(object sender, EventArgs e)
        {
             if (SReturnInvoiceNoCmb.SelectedItem == null) return;
            string selectedInvoiceNo = SReturnInvoiceNoCmb.SelectedItem.ToString();
            SReturnProductNameCmb.Items.Clear();

            try
            {
                con.Open();
                string query = @"
                    SELECT i.ProductName 
                    FROM tblSalesDetails sd
                    JOIN tblItem i ON sd.ProductID = i.ProductID
                    WHERE sd.InvoiceNo = @InvoiceNo";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@InvoiceNo", selectedInvoiceNo);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        SReturnProductNameCmb.Items.Add(reader["ProductName"].ToString());
                    }
                }
                if (SReturnProductNameCmb.Items.Count == 0)
                    MessageBox.Show($"No products found for InvoiceNo: {selectedInvoiceNo}");
                con.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        // PAYMENTS RECEIVED CODE HERE
        // --------------------------------------------------------
        private void PopulatePaymentReport()
        {
            string query = @"
            WITH CompletedOrders AS (
                SELECT s.invoiceNo, co.UnitPrice, ss.quantity 
                FROM tblSales s 
                INNER JOIN tblSalesDetails ss ON s.invoiceNo = ss.invoiceNo
                INNER JOIN tblItem i ON ss.ProductID = i.ProductID 
                INNER JOIN tblCompleteOrders co ON s.invoiceNo = co.invoiceNo AND i.ProductID = co.ProductID
                WHERE s.iStatus = 'Delivery Complete'
            ),
            TotalPriceByInvoice AS (
                SELECT invoiceNo, SUM(UnitPrice * quantity) AS totalPrice 
                FROM CompletedOrders
                GROUP BY invoiceNo
            )
            SELECT 
                c.FName + ' ' + c.LName AS FullName, 
                s.invoiceDate, 
                s.invoiceNo, 
                t.totalPrice 
            FROM tblCustomer c 
            INNER JOIN tblSales s ON c.customerID = s.customerID
            INNER JOIN TotalPriceByInvoice t ON s.invoiceNo = t.invoiceNo 
            ORDER BY s.invoiceNo";

            con.Open();
            SqlDataAdapter adapter = new SqlDataAdapter(query, con);
            paymentDataTable = new DataTable();
            adapter.Fill(paymentDataTable);

            PaymentsTbl.AutoGenerateColumns = false;
            PaymentsTbl.DataSource = paymentDataTable;
            PaymentsTbl.Columns["PRecievedDate"].DataPropertyName = "invoiceDate";
            PaymentsTbl.Columns["PRecievedInvoiceNo"].DataPropertyName = "invoiceNo";
            PaymentsTbl.Columns["PRCusName"].DataPropertyName = "FullName";
            PaymentsTbl.Columns["PRTotalPayment"].DataPropertyName = "totalPrice";

            PaymentCount.Text = $"Total records: {PaymentsTbl.Rows.Count}";
            con.Close();
        }
        private void SearchPaymentReport(string searchTerm)
        {
            try
            {
                if (paymentDataTable != null)
                {
                    DataView dv = new DataView(paymentDataTable);
                    dv.RowFilter = $"FullName LIKE '%{searchTerm}%' OR CONVERT(invoiceNo, 'System.String') LIKE '%{searchTerm}%'";
                    PaymentsTbl.DataSource = dv;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error while searching: " + ex.Message, "Search Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Clear();
            }
        }
        private void PRecievedSearch_Click(object sender, EventArgs e)
        {
            SearchPaymentReport(TxtPRecievedSearch.Text);
        }
        // LOW STOCK CODE HERE
        // --------------------------------------------------------
        private void FillLowStockItems()
        {
            LowStocksTbl.Rows.Clear();

            string query = @"
            SELECT i.ProductName, tbl.totalQty, i.ProductionPoint 
            FROM (
                SELECT SUM(p.Quantity) AS totalQty, ProductID 
                FROM tblProductionDetails p 
                GROUP BY p.ProductID
            ) tbl 
            INNER JOIN tblItem i ON i.ProductID = tbl.ProductID 
            ORDER BY tbl.totalQty";

            try
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand(query, con))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        LowStocksTbl.Rows.Add(
                            reader["ProductName"].ToString(),
                            reader["totalQty"].ToString(),
                            reader["ProductionPoint"].ToString()
                        );
                    }
                }
                StockCount.Text = $"Total records: {LowStocksTbl.Rows.Count}"; // ito nabago
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                con.Close();
            }
        }
        private void LStockSearch_Click(object sender, EventArgs e)
        {
            string searchTerm = TxtLStockSearch.Text.Trim().ToLower();

            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                FillLowStockItems();
                return;
            }

            LowStocksTbl.Rows.Clear();

            string query = @"
            SELECT i.ProductName, tbl.totalQty, i.ProductionPoint 
            FROM (
                SELECT SUM(p.Quantity) AS totalQty, ProductID 
                FROM tblProductionDetails p 
                GROUP BY p.ProductID
            ) tbl 
            INNER JOIN tblItem i ON i.ProductID = tbl.ProductID 
            WHERE LOWER(i.ProductName) LIKE @SearchTerm OR LOWER(tbl.totalQty) LIKE @SearchTerm
            ORDER BY tbl.totalQty";

            try
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@SearchTerm", "%" + searchTerm + "%");

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            LowStocksTbl.Rows.Add(
                                reader["ProductName"].ToString(),
                                reader["totalQty"].ToString(),
                                reader["ProductionPoint"].ToString()
                            );
                        }
                    }

                }

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                con.Close();
                Clear();
            }
        }
        // COMPLETED SALES CODE HERE
        // --------------------------------------------------------
        private void CompletedSalesDataGridView()
        {
            try
            {
                string query = @"SELECT s.invoiceDate AS OrderDate, 
                                s.orderID AS OrderNo, 
                                s.invoiceNo AS InvoiceNo, 
                                c.fName + ' ' + c.lName AS Customer, 
                                s.iStatus AS Status, 
                                s.dueDate AS DueDate 
                         FROM tblSales s
                         INNER JOIN tblCustomer c ON s.customerID = c.customerID
                         WHERE s.iStatus = 'Delivery Complete'
                         ORDER BY s.dueDate";

                SqlDataAdapter adapter = new SqlDataAdapter(query, con);
                DataTable dataTable = new DataTable();
                adapter.Fill(dataTable);

                // Bind the DataTable to the DataGridView
                CompletedSalesTbl.DataSource = dataTable;
                CompletedSalesTbl.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                CompletedSalesTbl.Columns["OrderDate"].FillWeight = 25;
                CompletedSalesTbl.Columns["OrderNo"].FillWeight = 15;
                CompletedSalesTbl.Columns["InvoiceNo"].FillWeight = 15;
                CompletedSalesTbl.Columns["Customer"].FillWeight = 20;
                CompletedSalesTbl.Columns["Status"].FillWeight = 20;
                CompletedSalesTbl.Columns["DueDate"].FillWeight = 25;

                CompleteCount.Text = $"Total records: {dataTable.Rows.Count}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading completed sales: {ex.Message}");
            }
            finally
            {
                if (con.State == ConnectionState.Open) con.Close();
            }
        }
        private void CSalesSearch_Click(object sender, EventArgs e)
        {
            try
            {
                string searchTerm = TxtCSalesSearch.Text.Trim();
                string query = @"SELECT s.invoiceDate AS OrderDate, 
                                 s.orderID AS OrderNo, 
                                 s.invoiceNo AS InvoiceNo, 
                                 c.fName + ' ' + c.lName AS Customer, 
                                 s.iStatus AS Status, 
                                 s.dueDate AS DueDate 
                         FROM tblSales s
                         INNER JOIN tblCustomer c ON s.customerID = c.customerID
                         WHERE s.iStatus = 'Delivery Complete' 
                           AND (c.fName + ' ' + c.lName LIKE @SearchTerm 
                             OR s.orderID LIKE @SearchTerm 
                             OR s.invoiceNo LIKE @SearchTerm 
                             OR s.dueDate LIKE @SearchTerm) 
                         ORDER BY s.dueDate";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@SearchTerm", $"%{searchTerm}%");
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    DataTable dataTable = new DataTable();
                    adapter.Fill(dataTable);
                    CompletedSalesTbl.DataSource = dataTable;
                    CompleteCount.Text = $"Total completed sales: {dataTable.Rows.Count}";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error searching completed sales: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (con.State == ConnectionState.Open) con.Close();
                Clear();
            }
        }
        private void TxtProductionSearch_TextChanged(object sender, EventArgs e)
        {
            string searchText = TxtItemSearch.Text.Trim().ToLower();
            foreach (DataGridViewRow row in ItemTbl.Rows)
            {
                if (row.IsNewRow) continue;
                string productName = row.Cells[0].Value.ToString().ToLower();
                if (productName.Contains(searchText))
                {
                    row.Visible = true;
                }
                else
                {
                    row.Visible = false;
                }
            }
        }
        // PUT YOUR OWN CODE HERE!!
        // --------------------------------------------------------

    }
}
