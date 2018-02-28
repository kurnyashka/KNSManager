using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
//using Microsoft.DirectX.AudioVideoPlayback;

namespace SPBU12._1MANAGER {
    public partial class Configuration : Form {
        Color color1, color2, fontColor;
        Font fileFont, mainFont, dialogFont;

        public Configuration(Color fontColor, Color color1, Color color2, Font fileFont, Font mainFont, Font dialogFont) {
            InitializeComponent();

            this.fontColor = fontColor;
            this.color1 = color1;
            this.color2 = color2;
            this.fileFont = fileFont;
            this.mainFont = mainFont;
            this.dialogFont = dialogFont;

            listBox1.Items.Add("Colors");
            listBox1.Items.Add("Font");
            listBox1.Items.Add("Music");
            
            MyColors();
        }

        public Color FontColor() {
            return fontColor;
        }
        public Color Color1() {
            return color1;
        }
        public Color Color2() {
            return color2;
        }
        public Font FileFont() {
            return fileFont;
        }
        public Font MainFont() {
            return mainFont;
        }
        public Font DialogFont() {
            return dialogFont;
        }

        private void MyColors() {
            tableLayoutPanel1.Controls.Clear();
            textBox1.Text = "Colors";

            Label label1 = new Label();
            label1.Text = "Font color:";
            tableLayoutPanel1.Controls.Add(label1, 0, 3);
            PictureBox pb1 = new PictureBox();
            pb1.BorderStyle = BorderStyle.FixedSingle;
            pb1.BackColor = fontColor;
            pb1.Click += new System.EventHandler(this.pb1_Click);
            tableLayoutPanel1.Controls.Add(pb1, 1, 3);

            Label label2 = new Label();
            label2.Text = "Background1:";
            tableLayoutPanel1.Controls.Add(label2, 0, 1);
            PictureBox pb2 = new PictureBox();
            pb2.BorderStyle = BorderStyle.FixedSingle;
            pb2.BackColor = color1;
            pb2.Click += new System.EventHandler(this.pb2_Click);
            tableLayoutPanel1.Controls.Add(pb2, 1, 1);

            Label label3 = new Label();
            label3.Text = "Background2:";
            tableLayoutPanel1.Controls.Add(label3, 0, 2);
            PictureBox pb3 = new PictureBox();
            pb3.BorderStyle = BorderStyle.FixedSingle;
            pb3.BackColor = color2;
            pb3.Click += new System.EventHandler(this.pb3_Click);
            tableLayoutPanel1.Controls.Add(pb3, 1, 2);
        }

        private void colorClick(ref Color color) {
            ColorDialog MyDialog = new ColorDialog();

            MyDialog.AllowFullOpen = true;
            MyDialog.ShowHelp = false;

            if (MyDialog.ShowDialog() == DialogResult.OK) {
                color = MyDialog.Color;
                MyColors();
            }
        }

        private void pb1_Click(object sendler, EventArgs e) {
            colorClick(ref fontColor);
        }
        private void pb2_Click(object sendler, EventArgs e) {
            colorClick(ref color1);
        }
        private void pb3_Click(object sendler, EventArgs e) {
            colorClick(ref color2);
        }

        private void MyFont() {
            tableLayoutPanel1.Controls.Clear();
            textBox1.Text = "Font";

            Label label1 = new Label();
            label1.Text = "File list font:";
            tableLayoutPanel1.Controls.Add(label1, 0, 1);
            Button b1 = new Button();
            b1.Text = "Change font";
            b1.Click += new System.EventHandler(this.b1_Click);
            tableLayoutPanel1.Controls.Add(b1, 1, 1);
            Label label11 = new Label();
            label11.Font = fileFont;
            label11.Text = "ABCDIFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            tableLayoutPanel1.Controls.Add(label11, 0, 2);

            Label label2 = new Label();
            label2.Text = "Main window font:";
            tableLayoutPanel1.Controls.Add(label2, 0, 3);
            Button b2 = new Button();
            b2.Text = "Change font";
            b2.Click += new System.EventHandler(this.b2_Click);
            tableLayoutPanel1.Controls.Add(b2, 1, 3);
            Label label12 = new Label();
            label12.Font = mainFont;
            label12.Text = "ABCDIFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            tableLayoutPanel1.Controls.Add(label12, 0, 4);

            Label label3 = new Label();
            label3.Text = "Dialog box font:";
            tableLayoutPanel1.Controls.Add(label3, 0, 5);
            Button b3 = new Button();
            b3.Text = "Change font";
            b3.Click += new System.EventHandler(this.b3_Click);
            tableLayoutPanel1.Controls.Add(b3, 1, 5);
            Label label13 = new Label();
            label13.Font = dialogFont;
            label13.Text = "ABCDIFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            tableLayoutPanel1.Controls.Add(label13, 0, 6);
        }

        private void fontClick(ref Font font) {
            FontDialog MyDialog = new FontDialog();

            if (MyDialog.ShowDialog() == DialogResult.OK) {
                font = MyDialog.Font;
                MyFont();
            }
        }

        private void b1_Click(object sendler, EventArgs e) {
            fontClick(ref fileFont);
        }
        private void b2_Click(object sendler, EventArgs e) {
            fontClick(ref mainFont);
        }
        private void b3_Click(object sendler, EventArgs e) {
            fontClick(ref dialogFont);
        }
        
        private void Music() {
            tableLayoutPanel1.Controls.Clear();
            textBox1.Text = "Music";

            string path = "C:\\Users\\Natasha\\Desktop\\Projects\\FManager\\SPBU12.1MANAGER\\Music";
            DirectoryInfo di = new DirectoryInfo(path);
            FileInfo[] files = di.GetFiles();

            for (int i = 0; i < files.Length; i++) {
                Label lb = new Label();
                lb.Text = files[i].Name;
                tableLayoutPanel1.Controls.Add(lb, 0, i + 1);
            }

            //audio = new Audio(path + Path.DirectorySeparatorChar + files[0].Name, true);
        }

        /*private void User() {
            tableLayoutPanel1.Controls.Clear();
            textBox1.Text = "User";

            Label label1 = new Label();
            label1.Text = "Login:";
            tableLayoutPanel1.Controls.Add(label1, 0, 1);
            TextBox tb1 = new TextBox();
            tableLayoutPanel1.Controls.Add(tb1, 1, 1);

            Label label2 = new Label();
            label2.Text = "Password:";
            tableLayoutPanel1.Controls.Add(label2, 0, 2);
            TextBox tb2 = new TextBox();
            tableLayoutPanel1.Controls.Add(tb2, 1, 2);
        }*/

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e) {
            switch (listBox1.SelectedItem.ToString()) {
                case "Colors":
                    MyColors();
                    break;
                case "Font":
                    MyFont();
                    break;
                case "Music":
                    Music();
                    break;
                default:
                    break;
            }
        }

        private void button2_Click(object sender, EventArgs e) {
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e) {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close();
        }
    }
}
