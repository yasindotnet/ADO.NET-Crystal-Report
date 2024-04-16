using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace BankIngManagementsystem
{
    public partial class Form1 : Form
    {
        int inCustomerId = 0;
        bool isDefaultImage = true;
        string strConnectionString = @"Data Source=DESKTOP-1M44CQV;Initial Catalog=BankManagementSystem;Integrated Security=True;",strPreviowsImage="";
        OpenFileDialog ofd=new OpenFileDialog();
        public Form1()
        {
            InitializeComponent();
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            clear();
        }
        void clear()
        {
            textName.Text = "";
            dateTimePicker1.Value = DateTime.Now;
            textContact.Text = "";
            comBranch.SelectedIndex = 0;
            inCustomerId = 0;
            btnSave.Text = "Save";
            btnDelete.Enabled = false;
            pictureBox1.Image = Image.FromFile(Application.StartupPath + "\\images\\defaultImage.png");
            isDefaultImage = true;

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            ComBranchFill();
            FillCustomerDataGridView();
            clear();
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            ofd.Filter = "Image(.jpg,.png)|*.png;*.jpg";
            if(ofd.ShowDialog() == DialogResult.OK)
            {
                pictureBox1.Image=new Bitmap(ofd.FileName);
                isDefaultImage = false;
                strPreviowsImage = "";
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            pictureBox1.Image = new Bitmap(Application.StartupPath + "\\Images\\defaultImage.png");
            isDefaultImage = true;
            strPreviowsImage = "";
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (validateBankingManagementSystem())
            {
                int _incustomerId = 0;
                using (SqlConnection sqlcon = new SqlConnection(strConnectionString))
                {
                    sqlcon.Open();
                    SqlCommand cmd = new SqlCommand("CustomerInfoAddorEdit", sqlcon);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@CustomerId", inCustomerId);
                    cmd.Parameters.AddWithValue("@CustomerName", textName.Text.Trim());
                    cmd.Parameters.AddWithValue("@DateOfBirth", dateTimePicker1.Value);
                    cmd.Parameters.AddWithValue("@Address", textadd.Text.Trim());
                    cmd.Parameters.AddWithValue("@Contact", textContact.Text.Trim());
                    cmd.Parameters.AddWithValue("@branchId", Convert.ToInt32(comBranch.SelectedValue));
                    if (isDefaultImage)
                        cmd.Parameters.AddWithValue("@ImagePath", DBNull.Value);
                    else if (inCustomerId > 0 && strPreviowsImage != "")
                        cmd.Parameters.AddWithValue("@ImagePath", strPreviowsImage);
                    else
                        cmd.Parameters.AddWithValue("@ImagePath", SaveImage(ofd.FileName));
                    _incustomerId = (Convert.ToInt32(cmd.ExecuteScalar()));
                    sqlcon.Close();
                }
                //details
                using (SqlConnection sqlcon = new SqlConnection(strConnectionString))
                {
                    sqlcon.Open();
                    foreach(DataGridViewRow dgvRow in dataGridView1.Rows)
                    {
                        if (dgvRow.IsNewRow) break;
                        else
                        {
                            SqlCommand cmd = new SqlCommand("TransationAddorEdit", sqlcon);
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@TranstionId", Convert.ToInt32(dgvRow.Cells["dgvTransationId"].Value == DBNull.Value ? "0" : dgvRow.Cells["dgvTransationId"].Value));
                            cmd.Parameters.AddWithValue("@branchId", Convert.ToInt32(dgvRow.Cells["dgvBranch"].Value == DBNull.Value ? "0" : dgvRow.Cells["dgvBranch"].Value));
                            cmd.Parameters.AddWithValue("@Credit", Convert.ToInt32(dgvRow.Cells["dgvCredit"].Value == DBNull.Value ? "0" : dgvRow.Cells["dgvCredit"].Value));
                            cmd.Parameters.AddWithValue("@Withdraw", Convert.ToInt32(dgvRow.Cells["dgvWithdraw"].Value == DBNull.Value ? "0" : dgvRow.Cells["dgvWithdraw"].Value));
                            cmd.Parameters.AddWithValue("@TotalBalance", Convert.ToInt32(dgvRow.Cells["dgvTotal"].Value == DBNull.Value ? "0" : dgvRow.Cells["dgvTotal"].Value));
                            cmd.Parameters.AddWithValue("@customerId", _incustomerId);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                FillCustomerDataGridView();
                clear();
                MessageBox.Show("Submitted Successfully");
            }
        }
        bool validateBankingManagementSystem()
        {
            bool _isvalid = true;
            if (textName.Text.Trim() == "")
            {
                MessageBox.Show("Customer Name is required");
            }
            return _isvalid;
        }
        string SaveImage(string _imagepath)
        {
            string _filename = Path.GetFileNameWithoutExtension(_imagepath);
            string _extension=Path.GetExtension(_imagepath);

            //shorten Image Name
            _filename=_filename.Length <=15? _filename: _filename.Substring(0,15);
            _filename = _filename + DateTime.Now.ToString("yymmssfff") + _extension;
            pictureBox1.Image.Save(Application.StartupPath + "\\Images\\" + _filename);
            return _filename;
        }

        void FillCustomerDataGridView()
        {
            using (SqlConnection sqlcon = new SqlConnection(strConnectionString))
            {
                sqlcon.Open();
                SqlDataAdapter sqlda = new SqlDataAdapter("CustomerViewAll", sqlcon);
                sqlda.SelectCommand.CommandType = CommandType.StoredProcedure;
                DataTable dtbl=new DataTable();
                sqlda.Fill(dtbl);
                dataGridView2.DataSource = dtbl;
                dataGridView2.Columns[2].AutoSizeMode= DataGridViewAutoSizeColumnMode.Fill;
                dataGridView2.Columns[3].AutoSizeMode= DataGridViewAutoSizeColumnMode.AllCells;
                dataGridView2.Columns[0].Visible = true;
            }
        }

        private void dataGridView2_DoubleClick(object sender, EventArgs e)
        {
            if(dataGridView2.CurrentRow.Index != -1)
            {
                DataGridViewRow _dgvCurrentRow= dataGridView2.CurrentRow;
                inCustomerId = Convert.ToInt32(_dgvCurrentRow.Cells[0].Value);
                using (SqlConnection sqlcon = new SqlConnection(strConnectionString))
                {
                    sqlcon.Open();
                    SqlDataAdapter sqlda = new SqlDataAdapter("CustomerViewAllById", sqlcon);
                    sqlda.SelectCommand.CommandType=CommandType.StoredProcedure;
                    sqlda.SelectCommand.Parameters.AddWithValue("@CustomerId", inCustomerId);
                    DataSet ds = new DataSet();
                    sqlda.Fill(ds);

                    //Master--fill
                    DataRow dr = ds.Tables[0].Rows[0];
                    textName.Text = dr["CustomerName"].ToString();
                    dateTimePicker1.Value = Convert.ToDateTime(dr["DateofBirth"].ToString());
                    textadd.Text = dr["Address"].ToString();
                    textContact.Text = dr["Contact"].ToString();
                    if (dr["ImagePath"] == DBNull.Value)
                    {
                        pictureBox1.Image = new Bitmap(Application.StartupPath + "\\Images\\defaultImage.png");
                        isDefaultImage = true;
                    }
                    else
                    {
                        pictureBox1.Image=new Bitmap(Application.StartupPath + "\\Images\\" + dr["Imagepath"].ToString());
                        strPreviowsImage = dr["Imagepath"].ToString();
                        isDefaultImage=false;
                    }
                    dataGridView1.AutoGenerateColumns=false;
                    dataGridView1.DataSource=ds.Tables[1];
                    btnDelete.Enabled=true;
                    btnSave.Text = "Update";
                    comBranch.SelectedValue = Convert.ToInt32(dr["BranchId"].ToString());
                    
                }
            }
        }

        private void dataGridView1_UserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
        {
            DataGridViewRow dgvrow = dataGridView1.CurrentRow;
            if (dgvrow.Cells["dgvTransationId"].Value !=DBNull.Value)
            {
                if(MessageBox.Show("Are You want to Delete this Record?","Form1",MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    using (SqlConnection sqlcon = new SqlConnection(strConnectionString))
                    {
                        sqlcon.Open();
                        SqlCommand cmd = new SqlCommand("TransationDelete", sqlcon);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@transtionId", Convert.ToInt32(dgvrow.Cells["dgvTransationId"].Value));
                        cmd.ExecuteNonQuery();
                    }
                }
                else
                    e.Cancel = true;
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are You want to Delete this Record?", "Form1", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                using (SqlConnection sqlcon = new SqlConnection(strConnectionString))
                {
                    sqlcon.Open();
                    SqlCommand cmd = new SqlCommand("CustomerInfoDelete", sqlcon);
                    cmd.CommandType= CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@CustomerId", inCustomerId);
                    cmd.ExecuteNonQuery();
                    clear();
                    FillCustomerDataGridView();
                    MessageBox.Show("Deleted Successfully");
                }
            }
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            frmCustomerInformation frm=new frmCustomerInformation();
            frm.Show();
            //frm.MdiParent = this;
        }

        void ComBranchFill()
        {
            using (SqlConnection sqlcon = new SqlConnection(strConnectionString))
            {
                sqlcon.Open();
                SqlDataAdapter sqlda = new SqlDataAdapter("select * from Branch", sqlcon);
                DataTable dtbl = new DataTable();
                sqlda.Fill(dtbl);
                DataRow topitem = dtbl.NewRow();
                topitem[0] = 0;
                topitem[1] = "select";
                dtbl.Rows.InsertAt(topitem, 0);
                comBranch.ValueMember = dgvBranch.ValueMember = "BranchId";
                comBranch.DisplayMember = dgvBranch.DisplayMember = "BranchaName";
                comBranch.DataSource = dtbl;
                dgvBranch.DataSource = dtbl.Copy();
                
            }
        }
    }
}
