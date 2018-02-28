using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using System.IO;
using System.IO.Compression;
using System.Security.Permissions;
using System.Text.RegularExpressions;


namespace SPBU12._1MANAGER
{
    public partial class CopyBox : Form
    {
        string rootTo;

        public CopyBox(string root, string text)
        {
            InitializeComponent();

            label1.Text = text;
            rootTo = root;

            comboBox1.Items.Add(rootTo);
            comboBox1.Text = comboBox1.Items[0].ToString();
            DirectoryInfo di = new DirectoryInfo(rootTo);
            DirectoryInfo[] directories = di.GetDirectories();
            foreach (DirectoryInfo info in directories)
            {
                comboBox1.Items.Add(rootTo + Path.DirectorySeparatorChar + info.Name);
            }
        }

        public string Root {
            get {
                return comboBox1.Text;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close();
        }
    }
}
