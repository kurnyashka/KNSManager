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
    public partial class WhichFindPD : Form {
        string selectedItem;

        public WhichFindPD() {
            InitializeComponent();
            string[] list = { "Thread", "Parallel.ForEach", "Tasks", "Async/await" };
            foreach (string str in list)
                listBox1.Items.Add(str);
        }

        public string ITEM {
            get {
                return selectedItem;
            }
        }

        private void button1_Click(object sender, EventArgs e) {
            if (listBox1.SelectedItem != null) {
                this.DialogResult = System.Windows.Forms.DialogResult.OK;
                selectedItem = listBox1.SelectedItem.ToString();
                this.Close();
            } else
                MessageBox.Show("Выберите один вариант");
        }

        private void button2_Click(object sender, EventArgs e) {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close();
        }
    }
}
