using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SPBU12._1MANAGER {
    public partial class Favorite : Form {
        string path;

        public Favorite(Dictionary<string, string> list) {
            InitializeComponent();
            path = null;
            foreach (string str in list.Keys)
                listBox1.Items.Add(str);
        }

        private void button1_Click(object sender, EventArgs e) {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close();
        }

        private void listBox1_DoubleClick(object sender, EventArgs e) {
            path = listBox1.SelectedItem.ToString();
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
        }

        public string Choice {
            get {
                return path;
            }
        }
    }
}
