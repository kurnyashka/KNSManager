using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SPBU12._1MANAGER
{
    public partial class HelpBox : Form
    {
        public HelpBox()
        {
            InitializeComponent();

            listView1.Items.Add(new ListViewItem(new string[] { "F1", "Help" }));
            listView1.Items.Add(new ListViewItem(new string[] { "F2", "Reread source window" }));
            listView1.Items.Add(new ListViewItem(new string[] { "F3", "List Files" }));
            listView1.Items.Add(new ListViewItem(new string[] { "F4", "Edit files" }));
            listView1.Items.Add(new ListViewItem(new string[] { "F5", "Copy files" }));
            listView1.Items.Add(new ListViewItem(new string[] { "F6", "Rename or move files" }));
            listView1.Items.Add(new ListViewItem(new string[] { "F7", "Create directory" }));
            listView1.Items.Add(new ListViewItem(new string[] { "F8 or DEL", "Delete files" }));
        }
    }
}
