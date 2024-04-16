using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BankIngManagementsystem
{
    public partial class frmCustomerInformation : Form
    {
        public frmCustomerInformation()
        {
            InitializeComponent();
        }

        private void frmCustomerInformation_Load(object sender, EventArgs e)
        {
            frmCustomerInformation frm=new frmCustomerInformation();
            //frm.Show();
            //frm.MdiParent = this;
        }
    }
}
