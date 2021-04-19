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
    public partial class MenuForm : Form
    {
        MainForm MainForm;

        public MenuForm(MainForm mainForm)
        {
            InitializeComponent();
            MainForm = mainForm;
        }


        private void buttonCreate_Click(object sender, EventArgs e)
        {
            int verySmall, small, normal, big, veryBig;
            try
            {
                verySmall = Convert.ToInt32(numericUpDownVerySmall.Text);
                small = Convert.ToInt32(numericUpDownSmall.Text);
                normal = Convert.ToInt32(numericUpDownNormal.Text);
                big = Convert.ToInt32(numericUpDownBig.Text);
                veryBig = Convert.ToInt32(numericUpDownVeryBig.Text);
                //Movement.Stop();
                MainForm.Movement.Destroy();
                MainForm.Movement = new Movement(MainForm.Movement.TargetPictureBox, veryBig, big, normal, small, verySmall);
                Close();
            }
            catch(FormatException)
            {
                MessageBox.Show("Помилка при уведенні чисел");
            }
            catch (ArgumentOutOfRangeException ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

    }
}
