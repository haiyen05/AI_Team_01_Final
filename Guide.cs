using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AI
{
    public partial class frmGuide : Form
    {
        public frmGuide()
        {
            InitializeComponent();
            btnDaHieu.Click += BtnDaHieu_Click;
        }
        private void BtnDaHieu_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void frmGuide_FormClosing(object sender, FormClosingEventArgs e)
        {

        }
    }
}
