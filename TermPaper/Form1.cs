using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TermPaper
{
    public partial class MainForm : Form
    {
        public Movement Movement { get; set; }
        public MainForm()
        {
            InitializeComponent();
            Movement = new Movement(pictureBox1, 1, 3, 7, 9, 11);
        }

        private void createNewBallsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MenuForm menuForm = new MenuForm(this);
            menuForm.ShowDialog();
        }
    }
}
