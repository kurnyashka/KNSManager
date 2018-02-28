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
    public partial class Start : Form {
        private string login, password;

        public Start() {
            InitializeComponent();
        }

        public string Login {
            get {
                return login;
            }
        }

        public string Password {
            get {
                return password;
            }
        }

        private void button2_Click(object sender, EventArgs e) {
            this.DialogResult = System.Windows.Forms.DialogResult.Yes;
            login = textBox1.Text;
            password = textBox2.Text;
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e) {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close();
        }

        private void button3_Click(object sender, EventArgs e) {
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            login = textBox1.Text;
            password = textBox2.Text;
            this.Close();
        }
    }
}
