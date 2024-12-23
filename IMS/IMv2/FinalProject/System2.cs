using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FinalProject
{
    public partial class System2 : Form
    {
        // Reusable Variables
        SqlConnection con = DBManager.Connection();
        SqlCommand cm = new SqlCommand();
        SqlDataReader reader;
        public System1 system1;
        LogIn form = new LogIn();

        private int currentRow = 0;
        private const int maxRowsPerPage = 20;

        public int EmployeeID { get; set; }

        public System2(LogIn form, System1 system1Instance, int employeeID)
        {
            InitializeComponent();
            system1 = system1Instance;
            EmployeeID = employeeID;
        }

        private void System2_Load(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Maximized;
            Bounds = Screen.PrimaryScreen.Bounds;
        }
        // UI RELATED CODE HERE
        // EXIT BUTTON
        private void exitSys1Btn_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.Application.Exit();
        }
        // LOG OUT BUTTON
        private void logOutBtn_Click(object sender, EventArgs e)
        {
            this.form.Clear();
            this.form.Show();
            this.Hide();
        }
        // PANEL CLOSE
        public void PClose()
        {
            SalesOrder.Visible = false;
            Invoice.Visible = false;
        }
        // PANEL SWITCHING
        private void SalesOrderBtn_Click(object sender, EventArgs e)
        {
            if (SalesOrder.Visible == false)
            {
                PClose();
                SalesOrder.Visible = true;
                FillOrder();
            }
            else
            {
                PClose();
            }
        }
        private void InvoiceBtn_Click(object sender, EventArgs e)
        {
            if (Invoice.Visible == false)
            {
                PClose();
                PopulateEmployeeInvoicesGrid(EmployeeID);
                Invoice.Visible = true;
            }
            else
            {
                PClose();
            }
        }
        private void CloseInvoiceBtn_Click(object sender, EventArgs e)
        {
            Invoice.Visible = false;
        }
        private void SOrderExitBtn1_Click(object sender, EventArgs e)
        {
            SalesOrder.Visible = false;
        }
        private void SOrderUpdateBtn1_Click(object sender, EventArgs e)
        {
            if (SOrderUpdate.Visible == true)
            {
                SOrderUpdate.Visible = false;
            }
            else
            {
                SOrderUpdate.Visible = true;
                OrderNoCBX();
            }
        }
        private void SOrderNewBtn1_Click(object sender, EventArgs e)
        {
            if (NewSOrder.Visible == true)
            {
                NewSOrder.Visible = false;
            }
            else
            {
                NewSOrder.Visible = true;
                OrderList.View = View.Details;
                OrderList.Columns.Add("Quantity", 100, HorizontalAlignment.Left);
                OrderList.Columns.Add("Product", 180, HorizontalAlignment.Left);
                CusCBX();
                ItemCBX();
            }
        }
        private void NewSOrderClose_Click(object sender, EventArgs e)
        {
            NewSOrder.Visible = false;
        }
        private void SOrderUpExitBtn_Click(object sender, EventArgs e)
        {
            SOrderUpdate.Visible = false;
        }
        public void Clear()
        {
            // Sales Order Clear
            SCustomerCBX.SelectedIndex = -1;
            SProductCBX.SelectedIndex = -1;
            TxtQuantity.Clear();
            OrderList.Items.Clear();
            dueDate1.Value = DateTime.Today;
            TxtOrderSearch.Clear();
            InvoiceCbx1.SelectedIndex = -1;
            SOrderStatusCbx1.SelectedIndex = -1;

            // Invoice Clear
            TxtInvoiceSearch.Clear();
        }
        // SALES ORDER CODE STARTS HERE
        // --------------------------------------------------------
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
        private void InvoiceCbx1_SelectedIndexChanged(object sender, EventArgs e)
        {
            OrderCBX();
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
                    // SOrderStatusCbx1.Items.Add("Delivery Complete");
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
            con.Open();
            cm = new SqlCommand("SELECT s.orderID, s.employeeID, s.invoiceNo, s.iStatus, s.invoiceDate, c.fName +' '+ c.lName, s.dueDate FROM tblSales s INNER JOIN tblCustomer c on s.customerID = c.customerID WHERE employeeID = @employeeID order by s.dueDate \r\n", con);
            cm.Parameters.AddWithValue("@employeeID", EmployeeID);
            OSalesTbl.Rows.Clear();
            reader = cm.ExecuteReader();
            while (reader.Read())
            {
                OSalesTbl.Rows.Add(String.Format("{0:MM/dd/yyyy}", reader.GetValue(4)), reader[0].ToString(), reader[2].ToString(), reader[5].ToString(), reader[3].ToString(), String.Format("{0:MM/dd/yyyy}", reader.GetValue(6)), reader.GetValue(1));
            }
            OrderCount.Text = $"Total records: {OSalesTbl.Rows.Count}";
            reader.Close();
            con.Close();
        }

        public void salesUpdate()
        {
            int flagg = 0;
            String status = SOrderStatusCbx1.Text;
            int invoice = Convert.ToInt32(InvoiceCbx1.Text);

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
                            flagg = 1;
                            reader.Close();
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
                    system1.AddAuditLog("updated an order status to transit");
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

                    system1.AddAuditLog("completed an order");
                    SOrderUpdate.Visible = false;
                    flagg = 1;
                }
                else if (SOrderStatusCbx1.Text.Equals("Cancelled"))
                {
                    system1.AddAuditLog("cancelled an order");
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
                Clear();
            }

            if (flagg == 1)
            {
                try
                {
                    cm = new SqlCommand("UPDATE tblSales SET iStatus = @stats WHERE invoiceNo = @invoice", con);
                    cm.Parameters.AddWithValue("@stats", status);
                    cm.Parameters.AddWithValue("@invoice", invoice);
                    con.Open();
                    cm.ExecuteNonQuery();
                    con.Close();
                    FillOrder();
                    Clear();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        private void SOrderUpStatusBtn_Click(object sender, EventArgs e)
        {
            salesUpdate();
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
                cm.Parameters.AddWithValue("@empId", this.EmployeeID);
                cm.Parameters.AddWithValue("@iStatus", "Order Placed");
                cm.Parameters.AddWithValue("@iDate", DateTime.Now);
                cm.Parameters.AddWithValue("@customerId", cus.cId);
                cm.Parameters.AddWithValue("@DDate", this.dueDate1.Text);
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

                system1.AddAuditLog("inserted a new order");
            }
            catch (Exception c)
            {
                MessageBox.Show("Error: " + c.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                FillOrder();
                Clear();
            }
        }

        private void NewSOrderClose_Click_1(object sender, EventArgs e)
        {
            NewSOrder.Visible = false;
        }

        // INVOICE CODE STARTS HERE
        // --------------------------------------------------------
        public void PopulateEmployeeInvoicesGrid(int employeeId)
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
        WHERE s.iStatus = 'Delivery Complete' AND s.employeeID = 1
        GROUP BY s.invoiceDate, co.invoiceNo, c.fName, c.lName, s.dueDate
        ORDER BY s.invoiceDate DESC";

            try
            {
                using (SqlConnection connect = new SqlConnection(@"Data Source=.\SQLEXPRESS;Initial Catalog=IMS;Integrated Security=True"))
                {
                    connect.Open();
                    using (SqlCommand cmd = new SqlCommand(query, connect))
                    {
                        cmd.Parameters.AddWithValue("@EmployeeId", employeeId);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (!reader.HasRows)
                            {
                                MessageBox.Show("No completed deliveries found.");
                                return;
                            }

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
        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
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
        private void btnClose_Click(object sender, EventArgs e)
        {
            panel5.Visible = false;
        }
        private void btnPrint_Click(object sender, EventArgs e)
        {
            PrintPreviewDialog previewDialog = new PrintPreviewDialog();
            previewDialog.Document = printDocument1;
            previewDialog.ShowDialog();
        }
        private void printDocument1_PrintPage_1(object sender, System.Drawing.Printing.PrintPageEventArgs e)
        {
            Graphics g = e.Graphics;

            Font titleFont = new Font("Arial", 24, FontStyle.Bold);
            Font headerFont = new Font("Arial", 14, FontStyle.Bold);
            Font contentFont = new Font("Arial", 12, FontStyle.Regular);

            int pageWidth = e.PageBounds.Width;
            int receiptWidth = 700;
            int startX = (pageWidth - receiptWidth) / 2;
            int startY = 40;
            int offsetY = 40;

            SizeF titleSize = g.MeasureString("LUMINAIRE", titleFont);
            g.DrawString("LUMINAIRE", titleFont, Brushes.Black, startX + (receiptWidth - titleSize.Width) / 2, startY);
            offsetY += 70;

            g.DrawString($"Invoice No: {lblOrder.Text}", contentFont, Brushes.Black, startX, startY + offsetY);
            offsetY += 30;
            g.DrawString($"Date: {lblDate.Text}", contentFont, Brushes.Black, startX, startY + offsetY);
            offsetY += 30;
            g.DrawString($"Customer Name: {lblName.Text}", contentFont, Brushes.Black, startX, startY + offsetY);
            offsetY += 50;

            g.DrawString("Product Name", headerFont, Brushes.Black, startX, startY + offsetY);
            g.DrawString("Qty", headerFont, Brushes.Black, startX + 280, startY + offsetY);
            g.DrawString("Unit Price", headerFont, Brushes.Black, startX + 420, startY + offsetY);
            g.DrawString("Total", headerFont, Brushes.Black, startX + 580, startY + offsetY);
            offsetY += 30;

            g.DrawLine(Pens.Black, startX, startY + offsetY, startX + receiptWidth, startY + offsetY);
            offsetY += 10;

            int rowCounter = 0;
            for (int i = currentRow; i < dataGridView2.Rows.Count; i++)
            {
                DataGridViewRow row = dataGridView2.Rows[i];
                if (row.IsNewRow) continue;

                g.DrawString(row.Cells[0].Value.ToString(), contentFont, Brushes.Black, startX, startY + offsetY);
                g.DrawString(row.Cells[1].Value.ToString(), contentFont, Brushes.Black, startX + 280, startY + offsetY);
                g.DrawString(row.Cells[2].Value.ToString(), contentFont, Brushes.Black, startX + 420, startY + offsetY);
                g.DrawString(row.Cells[3].Value.ToString(), contentFont, Brushes.Black, startX + 580, startY + offsetY);

                offsetY += 30;
                rowCounter++;

                if (rowCounter >= maxRowsPerPage)
                {
                    currentRow = i + 1;
                    e.HasMorePages = true;
                    return;
                }
            }
            offsetY += 40;
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
    }
}
