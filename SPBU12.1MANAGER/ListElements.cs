using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;

namespace SPBU12._1MANAGER
{
    class ListElements
    {
        public List<ListViewItem> list;
        string path;

        public ListElements()
        {
            list = new List<ListViewItem>();
        }

        private void UpdateDirectories(DirectoryInfo di)
        {
            if (path != Path.GetPathRoot(path))
            {
                list.Add(new ListViewItem("[..]"));
            }

            DirectoryInfo[] directories = di.GetDirectories();

            foreach (DirectoryInfo info in directories)
            {
                ListViewItem el = new ListViewItem("[" + info.Name + "]");
                el.SubItems.Add("");
                el.SubItems.Add("<DIR>");
                el.SubItems.Add(info.CreationTime.ToString());
                list.Add(el);
            }
        }

        private void UpdateFiles(DirectoryInfo di)
        {
            FileInfo[] files = di.GetFiles();

            foreach (FileInfo info in files)
            {
                ListViewItem el = new ListViewItem(Path.GetFileNameWithoutExtension(info.Name));
                el.SubItems.Add(Path.GetExtension(info.Name));
                el.SubItems.Add((info.Length / 1000).ToString());
                el.SubItems.Add(info.CreationTime.ToString());
                list.Add(el);
            }
        }

        public void Update(string path)
        {
            try
            {
                this.path = path;
                DirectoryInfo di = new DirectoryInfo(path);
                
                list.Clear();

                UpdateDirectories(di);
                UpdateFiles(di);
            }
            catch
            {
                path = Path.GetDirectoryName(path);
                //throw new Exception();
            }
        }

        private bool IsDir(string name)
        {
            return (name.Remove(1) == "[");
        }

        private bool IsBack(string name)
        {
            return (name == "[..]");
        }

        public string DoubleClick(ListView lw, string path)
        {
            string name = lw.SelectedItems[0].Text;

            if (IsDir(name))
            {
                if (IsBack(name))
                    path = Path.GetDirectoryName(path);
                else
                {
                    if (path[path.Length - 1] != Path.DirectorySeparatorChar)
                        path += Path.DirectorySeparatorChar;
                    path += name.Remove(0, 1).Remove(name.Length - 2);
                }
            }

            return path;
        }
    }
}
