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
using System.IO.Compression;
using System.Security.Permissions;
using System.Text.RegularExpressions;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Net;
using System.Diagnostics;

namespace SPBU12._1MANAGER {
    public partial class Form1 : Form {
        private string rootLeft, rootRight;
        Dictionary<string, string> dirs;
        ListElements listLeft, listRight;
        FileSystemWatcher watcherLeft, watcherRight;
        bool isChanged1, isChanged2;

        ListView lwOnline;

        UserData data;
        private string login, password, rootUser;

        private void Initialization() {
            Start start = new Start();
            DialogResult r = start.ShowDialog();

            if (r == DialogResult.Yes) {
                if (start.Login != "" && start.Password != "") {
                    data = new UserData();
                    data.login = start.Login;
                    data.password = start.Password;
                    rootUser = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                    rootUser += Path.DirectorySeparatorChar + data.login + ".dat";
                } else {
                    MessageBox.Show("НЕВЕРНЫЙ ВВОД");
                    Initialization();
                }
            } else if (r == DialogResult.OK) {
                login = start.Login;
                password = start.Password;
                rootUser = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                rootUser += Path.DirectorySeparatorChar + login + ".dat";

                if (!File.Exists(rootUser)) {
                    MessageBox.Show("НЕВЕРНЫЙ ЛОГИН");
                    Initialization();
                } else {
                    BinaryFormatter binFormat = new BinaryFormatter();
                    Stream fStream = File.Open(rootUser, FileMode.Open);
                    data = (UserData)binFormat.Deserialize(fStream);
                    fStream.Close();

                    if (data.password != password) {
                        MessageBox.Show("НЕВЕРНЫЙ ПАРОЛЬ");
                        Initialization();
                    }
                }
            } else if (r == DialogResult.Cancel) {
                Environment.Exit(0);
            }

            UpdateForm();
        }

        private void UpdateForm() {
            this.password = data.password;
            this.Font = data.mainFont;
            listView1.BackColor = data.color1;
            listView2.BackColor = data.color1;
            listView1.Font = data.fileFont;
            listView2.Font = data.fileFont;
        }

        private void WatchersInitialize() {
            timer1.Interval = 10;
            timer1.Tick += timer1_Tick;
            timer1.Enabled = true;
            
            watcherLeft = new FileSystemWatcher();
            watcherRight = new FileSystemWatcher();

            watcherLeft.Changed += UpdateLeft;
            watcherLeft.Created += UpdateLeft;
            watcherLeft.Deleted += UpdateLeft;
            watcherLeft.Renamed += UpdateLeft;

            watcherRight.Changed += UpdateRight;
            watcherRight.Created += UpdateRight;
            watcherRight.Deleted += UpdateRight;
            watcherRight.Renamed += UpdateRight;

            isChanged1 = false;
            isChanged2 = false;
        }

        public Form1() {
            InitializeComponent();
            Initialization();

            WatchersInitialize();

            dirs = new Dictionary<string, string>();

            listLeft = new ListElements();
            listRight = new ListElements();

            DriveInfo[] drives = DriveInfo.GetDrives();
            foreach (DriveInfo info in drives) {
                try
                {
                    comboBox1.Items.Add(info.Name + "        " + info.VolumeLabel);
                    comboBox2.Items.Add(info.Name + "        " + info.VolumeLabel);
                }
                catch
                {
                }
            }

            comboBox1.Text = comboBox1.Items[0].ToString();
            comboBox2.Text = comboBox2.Items[0].ToString();

            lwOnline = listView1;
            label1.Text = Root(lwOnline);
            label1.Location = new Point(570 - label1.Text.Length * 6, 646);
        }

        private void UpdateLeft(object sendler, FileSystemEventArgs e) {
            isChanged1 = true;
        }

        private void UpdateRight(object sendler, FileSystemEventArgs e) {
            isChanged2 = true;
        }
        
        private string Root(ListView lw) {
            if (lw == listView1)
                return rootLeft;
            return rootRight;
        }

        private ListElements ListE(ListView lw) {
            if (lw == listView1)
                return listLeft;
            return listRight;
        }

        private void UpdateListView(ListView lw) {
            if (lw.SelectedItems.Count > 0 && lw == WhichListView() && Path.GetFileName(lw.SelectedItems[0].Text) != lw.SelectedItems[0].Text) {
                if (lw == listView1)
                    rootLeft = lw.SelectedItems[0].Text;
                else
                    rootRight = lw.SelectedItems[0].Text;
            }

            ListE(lw).Update(Root(lw));
            lw.Items.Clear();

            foreach (ListViewItem item in ListE(lw).list) {
                lw.Items.Add(item);
                if ((item.Index % 2) == 0)
                    item.BackColor = data.color1;
                else
                    item.BackColor = data.color2;
                item.ForeColor = data.fontColor;
            }

            TB tb = new TB(() => {
                if (lw == listView1)
                    return textBox4;
                return textBox5;
            });

            tb.Invoke().Text = Root(lw);

            if (lw == listView1) {
                watcherLeft.Path = rootLeft;
                watcherLeft.EnableRaisingEvents = true;
            } else {
                watcherRight.Path = rootRight;
                watcherRight.EnableRaisingEvents = true;
            }
        }

        delegate TextBox TB();

        delegate DriveInfo Disk();

        private string SizeOfDisk(string name, bool left) {
            name = name.Substring(0, name.IndexOf(" "));
            if (left)
                rootLeft = name;
            else
                rootRight = name;
            Disk info = new Disk(() => {
                DriveInfo[] drives = DriveInfo.GetDrives();
                foreach (DriveInfo inf in drives) {
                    if (inf.Name == name) {
                        return inf;
                    }
                }
                throw new Exception();
            });
            return ("[" + info.Invoke().VolumeLabel.ToLower() + "]  " + info.Invoke().TotalFreeSpace / 1000 + " k of " + info.Invoke().TotalSize / 1000 + " k free");
        }

        private void comboBox1_SelectedValueChanged(object sender, EventArgs e) {
            textBox1.Text = SizeOfDisk(comboBox1.Text, true);
            UpdateListView(listView1);
        }

        private void comboBox2_SelectedValueChanged(object sender, EventArgs e) {
            textBox2.Text = SizeOfDisk(comboBox2.Text, false);
            UpdateListView(listView2);
        }

        private void DoubleClickLV(ListView lw) {
            string path = ListE(lw).DoubleClick(lw, Root(lw));

            if (lw == listView1)
                rootLeft = path;
            else
                rootRight = path;

            UpdateListView(lw);
        }

        private void listView1_DoubleClick(object sender, EventArgs e) {
            DoubleClickLV(listView1);
        }
        private void listView2_DoubleClick(object sender, EventArgs e) {
            DoubleClickLV(listView2);
        }

        //return left
        private void button4_Click(object sender, EventArgs e) {
            if (Path.GetDirectoryName(rootLeft) != null)
                rootLeft = Path.GetDirectoryName(rootLeft);
            UpdateListView(listView1);
        }
        private void button3_Click(object sender, EventArgs e) {
            if (Path.GetDirectoryName(rootLeft) != null)
                rootLeft = Path.GetPathRoot(rootLeft);
            UpdateListView(listView1);
        }

        //return right
        private void button5_Click(object sender, EventArgs e) {
            if (Path.GetDirectoryName(rootRight) != null)
                rootRight = Path.GetDirectoryName(rootRight);
            UpdateListView(listView2);
        }
        private void button6_Click(object sender, EventArgs e) {
            if (Path.GetDirectoryName(rootRight) != null)
                rootRight = Path.GetPathRoot(rootRight);
            UpdateListView(listView2);
        }

        private bool ShowOK(Window wind) {
            return (wind.ShowDialog() == DialogResult.OK);
        }

        private void AddToDirs(string name, string path) {
            if (name == "")
                throw new Exception();

            foreach (var el in dirs) {
                if (el.Value == path)
                    dirs.Remove(el.Key);
            }

            dirs.Add(name, path);
        }

        private void TryAddToDirs(string path) {
            try {
                Window window = new Window("New title for menu entry:");
                window.Font = data.dialogFont;

                if (ShowOK(window))
                    AddToDirs(window.NewName, path);
            } catch {
                MessageBox.Show("This name has already been used");
            }
        }

        //favorite
        private void button8_Click(object sender, EventArgs e) {
            TryAddToDirs(rootLeft);
        }
        private void button10_Click(object sender, EventArgs e) {
            TryAddToDirs(rootRight);
        }

        private void Help() {
            HelpBox hb = new HelpBox();
            hb.Font = data.dialogFont;
            hb.ShowDialog();
        }

        private void View() {

        }

        private void Edit() {

        }

        private string EndDir(string startDir) {
            if (startDir == rootLeft)
                return rootRight;
            return rootLeft;
        }

        private void CopyDir(string nameDir, string endDir) {
            Directory.CreateDirectory(endDir + Path.DirectorySeparatorChar + Path.GetFileName(nameDir));

            DirectoryInfo di = new DirectoryInfo(nameDir);
            DirectoryInfo[] directories = di.GetDirectories();
            FileInfo[] files = di.GetFiles();

            foreach (DirectoryInfo info in directories) {
                CopyDir(nameDir + Path.DirectorySeparatorChar + info.Name, endDir);
            }

            foreach (FileInfo info in files) {
                CopyFile(info.Name, nameDir, endDir);
            }
        }

        private void CopyFile(string nameFile, string startDir, string endDir) {
            using (FileStream SourceStream = File.Open(startDir + Path.DirectorySeparatorChar + nameFile, FileMode.Open)) {
                using (FileStream DestinationStream = File.Create(endDir + Path.DirectorySeparatorChar + nameFile)) {
                    SourceStream.CopyTo(DestinationStream);
                }
            }
        }

        private bool IsDir(string path) {
            return File.Exists(path) ? false : true;
        }

        private bool ShowWind(string root, string label) {
            CopyBox cb = new CopyBox(root, label);
            cb.Font = data.dialogFont;
            if (cb.ShowDialog() == DialogResult.OK)
                return true;
            return false;
        }

        private void CopyElement(ListView lw, string item) {
            string startDirectory = Root(lw);
            string endDirectory = EndDir(startDirectory);
            
            if (IsDir(startDirectory + Path.DirectorySeparatorChar + item))
                CopyDir(startDirectory + Path.DirectorySeparatorChar + item.Substring(1).Remove(item.Length - 2), endDirectory);
            else
                CopyFile(item, startDirectory, endDirectory);
        }

        private void IsCopy(ListView lw) {
            if (ShowWind(EndDir(Root(lw)), "Copy " + lw.SelectedItems.Count + " file(s) to:")) {
                foreach (ListViewItem file in lw.SelectedItems) {
                    CopyElement(lw, file.Text + file.SubItems[1].Text);
                }
            }
        }

        private void MoveElement(ListView lw, string el) {
            CopyElement(lw, el);
            DeleteElement(Root(lw), el);
        }

        private void MoveF(ListView lw) {
            if (ShowWind(EndDir(Root(lw)), "Rename/move " + lw.SelectedItems.Count + " file(s) to:")) {
                foreach (ListViewItem item in lw.SelectedItems) {
                    MoveElement(lw, item.Text + item.SubItems[1].Text);
                }
            }
        }

        private void NewFolder() {
            string path;
            if (IsSelected())
                path = Root(WhichListView());
            else
                path = rootLeft;

            Window window = new Window("New folder (in " + path + "):");
            window.Font = data.dialogFont;

            if (ShowOK(window))
                Directory.CreateDirectory(path + Path.DirectorySeparatorChar + window.NewName);
        }

        private void DeleteElement(string root, string name) {
            if (!IsDir(root + Path.DirectorySeparatorChar + name))
                File.Delete(root + Path.DirectorySeparatorChar + name);
            else {
                string pathDir = root + Path.DirectorySeparatorChar + name.Substring(1, name.Length - 2);
                Directory.Delete(pathDir, true);
            }
        }

        private bool MBShowOK(int k) {
            if (MessageBox.Show("Do you really want to delete " + k + " file(s)", "FF Manager", MessageBoxButtons.OKCancel, MessageBoxIcon.Stop) == DialogResult.OK)
                return true;
            return false;
        }

        private void Delete(ListView lw) {
            if (MBShowOK(lw.SelectedItems.Count)) {
                foreach (ListViewItem item in lw.SelectedItems) {
                    string name = item.Text + item.SubItems[1].Text;
                    DeleteElement(Root(lw), name);
                }
            }
        }

        private void RenameElement(string newPath, string path) {
            if (IsDir(path))
                Directory.Move(path, newPath);
            else
                File.Move(path, newPath);
        }

        private void Rename(ListView lw) {
            Window window = new Window("New name for:" + lw.SelectedItems[0].Text);
            window.Font = data.dialogFont;

            if (ShowOK(window)) {
                string newPath = Root(lw) + Path.DirectorySeparatorChar + window.NewName + Path.GetExtension(lw.SelectedItems[0].Text);
                RenameElement(newPath, Root(lw));
            }
        }

        private void Rename() {
            if (listView1.SelectedItems.Count == 1) {
                Rename(listView1);
            } else if (listView2.SelectedItems.Count == 1) {
                Rename(listView2);
            }
        }

        private void Pack(ListView lw) {
            ListViewItem file = lw.SelectedItems[0];
            string path = Root(lw) + Path.DirectorySeparatorChar + file.Text + file.SubItems[1].Text;

            if (File.Exists(path)) {
                Directory.CreateDirectory(path + "_ZIP");

                File.Copy(path, path + "_ZIP" + Path.DirectorySeparatorChar + Path.GetFileName(path));

                ZipFile.CreateFromDirectory(path + "_ZIP", path + ".zip");
                Directory.Delete(path + "_ZIP", true);
            } else {
                path = Root(lw) + Path.DirectorySeparatorChar + file.Text.Substring(1).Remove(file.Text.Length - 2);
                if (Directory.Exists(path)) 
                    ZipFile.CreateFromDirectory(path, path + ".zip");
            }
        }

        private void Unpack(ListView lw) {
            ListViewItem file = lw.SelectedItems[0];
            if (file.SubItems[1].Text != ".zip") {
                MessageBox.Show("Not *.zip!!!");
                return;
            }
            string path = Root(lw) + Path.DirectorySeparatorChar + file.Text + file.SubItems[1].Text;
            ZipFile.ExtractToDirectory(path, path.Substring(0, path.Length - 4));       
        }

        private void Search(string path, StreamWriter file) {
            try {
                DirectoryInfo di = new DirectoryInfo(path);
                DirectoryInfo[] directories = di.GetDirectories();
                FileInfo[] files = di.GetFiles();

                foreach (DirectoryInfo info in directories) {
                    Search(path + Path.DirectorySeparatorChar + info.Name, file);
                }

                foreach (FileInfo info in files) {
                    SearchFile(path + Path.DirectorySeparatorChar + info.Name, file);
                }
            } catch { }
        }

        private void SearchFile(string path, StreamWriter file) {
            using (FileStream fileS = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                byte[] b = new byte[1024];
                UTF8Encoding temp = new UTF8Encoding(true);
                Regex[] r = new Regex[2];
                r[0] = new Regex(@"[-a-f0-9_.]+@{1}[-0-9a-z]+\.[a-z]{2,5}");
                r[1] = new Regex(@"(\+7|8)-\([0-9]{3}\)-[0-9]{3}-[0-9]{2}-[0-9]{2}");
                string str;

                while (fileS.Read(b, 0, b.Length) > 0) {
                    for (int i = 0; i < 2; i++)
                        foreach (Match m in r[i].Matches(temp.GetString(b))) {
                            str = m.ToString();
                            file.WriteLine(str);
                        }
                }
            }
        }

        private void SearchData(ListView lw) {
            ListViewItem file = lw.SelectedItems[0];

            if (file.SubItems[1].Text != "") {
                MessageBox.Show("Only from directories! (NOT FILES)");
                return;
            }

            string path = Root(lw) + Path.DirectorySeparatorChar + file.Text.Substring(1).Remove(file.Text.Length - 2);

            string newPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) +
                Path.DirectorySeparatorChar + Path.GetFileName(path) + "_LICHNYE_DANNIE" + ".txt";
            using (StreamWriter newFile = new StreamWriter(newPath, false, Encoding.UTF8)) {
                Search(path, newFile);
            }
        }

        private void Exit() {
            Application.Exit();
        }

        private bool IsSelected() {
            return (listView1.SelectedItems.Count > 0 || listView2.SelectedItems.Count > 0);
        }

        private bool IsSelectedOne() {
            return (listView1.SelectedItems.Count == 1 || listView2.SelectedItems.Count == 1);
        }

        private ListView WhichListView() {
            if (listView2.Focused) {
                return listView2;
            }
            return listView1;
        }

        public void Download(DownloadFiles f) {
            string root = f.ROOT;
            string[] EBook = f.EBOOK;
            
            if (EBook != null) {
                string path = root;
                if (root != "" && root[root.Length - 1] != Path.DirectorySeparatorChar)
                    path += Path.DirectorySeparatorChar;
                string nameFile;

                foreach (string eb in EBook) {
                    if (eb != null) {
                        nameFile = Path.GetRandomFileName();

                        using (StreamWriter sw = new StreamWriter(path + nameFile + ".txt")) {
                            sw.WriteLine(eb);
                        }
                    }
                }
            }
        }

        private void Form1_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e) {
            try {
                if (e.KeyCode == Keys.Enter) {
                    if (IsSelected())
                        DoubleClickLV(WhichListView());
                }
                else if (e.Alt && e.KeyCode == Keys.F1) {
                    comboBox1.Focus();
                    comboBox1.DroppedDown = true;
                } else if (e.Alt && e.KeyCode == Keys.F2) {
                    comboBox2.Focus();
                    comboBox2.DroppedDown = true;
                } else if (e.Alt && e.KeyCode == Keys.F5) {
                    if (IsSelectedOne())
                        Pack(WhichListView());
                } else if (e.Alt && e.KeyCode == Keys.F6) {
                    if (IsSelectedOne())
                        Unpack(WhichListView());
                } else if (e.Alt && e.KeyCode == Keys.F7) {
                    textBox3.Focus();
                } else if (e.Alt && e.KeyCode == Keys.F8) {
                    if(IsSelectedOne())
                        Statistics();
                } else if (e.KeyCode == Keys.F1) {
                    Help();
                } else if (e.KeyCode == Keys.F2) {
                    if (IsSelected())
                        UpdateListView(WhichListView());
                } else if (e.KeyCode == Keys.F3) {
                    View();
                } else if (e.KeyCode == Keys.F4) {
                    Edit();
                } else if (e.KeyCode == Keys.F5) {
                    if (IsSelected())
                        IsCopy(WhichListView());
                } else if (e.KeyCode == Keys.F6) {
                    if (IsSelected())
                        MoveF(WhichListView());
                } else if (e.KeyCode == Keys.F7) {
                    NewFolder();
                } else if (e.KeyCode == Keys.F8 || e.KeyCode == Keys.Delete) {
                    if (IsSelected())
                        Delete(WhichListView());
                } else if (e.KeyCode == Keys.F9) {
                    if (IsSelected())
                        Rename();
                }
            } catch (Exception ex) {
                MessageBox.Show(ex.Message);
            }
        }
        
        private void helpToolStripMenuItem_Click(object sender, EventArgs e) {
            Help();
        }

        private void timer1_Tick(object sender, EventArgs e) {
            if (isChanged1) {
                UpdateListView(listView1);
                isChanged1 = false;
            }
            if (isChanged2) {
                UpdateListView(listView2);
                isChanged2 = false;
            }

            if (IsSelected() && lwOnline != WhichListView()) {
                lwOnline = WhichListView();
            }
            label1.Text = Root(lwOnline);
            label1.Location = new Point(570 - label1.Text.Length * 6, 646);
        }

        private void packToolStripMenuItem_Click(object sender, EventArgs e) {
            if (IsSelectedOne())
                Pack(WhichListView());
        }

        private void unpackToolStripMenuItem_Click(object sender, EventArgs e) {
            if (IsSelectedOne())
                Unpack(WhichListView());
        }

        private void findToolStripMenuItem_Click(object sender, EventArgs e) {
            if (IsSelected()) {
                ListViewItem selectFile = WhichListView().SelectedItems[0];
                string fName = Root(WhichListView()) + Path.DirectorySeparatorChar + selectFile.Text;
                string dName = Root(WhichListView()) + Path.DirectorySeparatorChar + selectFile.Text.Substring(1).Remove(selectFile.Text.Length - 2);
                if (Directory.Exists(dName)) {
                    string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) +
                        Path.DirectorySeparatorChar + Path.GetFileName(dName) + "_LICHNYE_DANNIE" + @".txt";
                    using (StreamWriter file = new StreamWriter(path, false, Encoding.UTF8)) {
                        WhichSearch(dName, file);
                    }
                } else if (File.Exists(fName))
                    MessageBox.Show("КРАДЕМ ЛИЧНЫЕ ДАННЫЕ ТОЛЬКО ИЗ ЦЕЛЫХ ПАПОК ИЛИ ДИСКОВ!");

            }
        }

        private void AllFiles(HashSet<string> queue, string path, StreamWriter file) {
            try {
                DirectoryInfo di = new DirectoryInfo(path);
                DirectoryInfo[] directories = di.GetDirectories();
                FileInfo[] files = di.GetFiles();

                foreach (DirectoryInfo info in directories) {
                    AllFiles(queue, path + Path.DirectorySeparatorChar + info.Name, file);
                }

                foreach (FileInfo info in files) {
                    queue.Add(path + Path.DirectorySeparatorChar + info.Name);
                }
            } catch { }
        }

        private void AllFiles(Queue<string>[] queue, string path, StreamWriter file) {
            try {
                DirectoryInfo di = new DirectoryInfo(path);
                DirectoryInfo[] directories = di.GetDirectories();
                FileInfo[] files = di.GetFiles();

                foreach (DirectoryInfo info in directories) {
                    AllFiles(queue, path + Path.DirectorySeparatorChar + info.Name, file);
                }

                int k = 0;
                foreach (FileInfo info in files) {
                    queue[k % Environment.ProcessorCount].Enqueue(path + Path.DirectorySeparatorChar + info.Name);
                    k++;
                }
            } catch { }
        }

        private void SearhThread(string path, StreamWriter file) {
            Stopwatch time = new Stopwatch();

            time.Start();
            Queue<string>[] queue = new Queue<string>[Environment.ProcessorCount];
            for (int i = 0; i < Environment.ProcessorCount; i++)
                queue[i] = new Queue<string>();

            AllFiles(queue, path, file);

            QueueProcessor[] proc = new QueueProcessor[Environment.ProcessorCount];
            for (int i = 0; i < Environment.ProcessorCount; i++) {
                try {
                    proc[i] = new QueueProcessor(queue[i], file);
                    proc[i].BeginProcessData();
                } catch { }
            }

            for (int i = 0; i < Environment.ProcessorCount; i++) {
                proc[i].EndProcessData();
            }
            time.Stop();

            MessageBox.Show("Готово!\nВремя поиска: " + (time.ElapsedMilliseconds / 1000.0) + " секунд");
        }

        private void SearcParallelForEach(string path, StreamWriter file) {
            HashSet<string> queue = new HashSet<string>();

            AllFiles(queue, path, file);
            
            Stopwatch time = new Stopwatch();

            time.Start();
            Parallel.ForEach<string>(queue, (string path2) => {
                try {
                    using (FileStream fileS = File.Open(path2, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                        byte[] b = new byte[1024];
                        UTF8Encoding temp = new UTF8Encoding(true);
                        Regex[] r = new Regex[2];
                        r[0] = new Regex(@"[-a-f0-9_.]+@{1}[-0-9a-z]+\.[a-z]{2,5}");
                        r[1] = new Regex(@"(\+7|8)-\([0-9]{3}\)-[0-9]{3}-[0-9]{2}-[0-9]{2}");
                        string str;

                        while (fileS.Read(b, 0, b.Length) > 0) {
                            for (int i = 0; i < 2; i++)
                                foreach (Match m in r[i].Matches(temp.GetString(b))) {
                                    str = m.ToString();
                                    file.WriteLine(str);
                                }
                        }
                    }
                } catch { }
            });
            time.Stop();

            MessageBox.Show("Готово!\nВремя поиска: " + (time.ElapsedMilliseconds / 1000.0) + " секунд");
        }

        private void SearchTasks(string path, StreamWriter file) {
            List<string> names = new List<string>();
            string[] fileNames = Directory.GetFiles(path, "*", SearchOption.AllDirectories);
            string[] directoriesNames = Directory.GetDirectories(path, "*", SearchOption.AllDirectories);

            foreach (var n in fileNames) {
                names.Add(n);
            }
            foreach (var n in directoriesNames) {
                names.Add(n);
            }

            Stopwatch time = new Stopwatch();

            time.Start();
            List<Task> tasks = new List<Task>();
            foreach (string name in names) {
                tasks.Add(Task.Run(() => {
                    try {
                        using (FileStream fileS = File.Open(name, FileMode.OpenOrCreate)) {
                            byte[] b = new byte[1024];
                            UTF8Encoding temp = new UTF8Encoding(true);
                            Regex[] r = new Regex[2];
                            r[0] = new Regex(@"[-a-f0-9_.]+@{1}[-0-9a-z]+\.[a-z]{2,5}");
                            r[1] = new Regex(@"(\+7|8)-\([0-9]{3}\)-[0-9]{3}-[0-9]{2}-[0-9]{2}");
                            string str;

                            while (fileS.Read(b, 0, b.Length) > 0) {
                                for (int a = 0; a < 2; a++)
                                    foreach (Match m in r[a].Matches(temp.GetString(b))) {
                                        str = m.ToString();
                                        file.WriteLine(str);
                                    }
                            }
                        }
                    } catch { }
                }));
            }

            foreach (Task t in tasks) {
                t.Wait();
            }
            time.Stop();

            MessageBox.Show("Готово!\nВремя поиска: " + (time.ElapsedMilliseconds / 1000.0) + " секунд");
        }

        public void SearchAA(FindAsync f, string dName) {
            f.Close();

            string data = f.DATA;
            double time = f.TIME;
            
            if (data != null) {
                string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) +
                              Path.DirectorySeparatorChar + Path.GetFileName(dName) + "_LICHNYE_DANNIE" + @".txt";
                using (StreamWriter file = new StreamWriter(path, false, Encoding.UTF8)) {
                    file.WriteLine(data);
                }
                MessageBox.Show("Готово!\nВремя поиска: " + time + " секунд");
            } 
        }

        private void WhichSearch(string path, StreamWriter file) {
            WhichFindPD window = new WhichFindPD();
            string which;

            if (window.ShowDialog() == DialogResult.OK) {
                which = window.ITEM;
                switch (which) {
                    case "Thread": {
                            SearhThread(path, file);
                            break;
                        }
                    case "Parallel.ForEach": {
                            SearcParallelForEach(path, file);
                            break;
                        }
                    case "Tasks": {
                            SearchTasks(path, file);
                            break;
                        }
                    case "Async/await": {
                            FindAsync wind = new FindAsync(this, path);
                            wind.Show();
                            wind.Start(path, file);
                            break;
                        }
                    default:
                        break;
                }
            }
        }

        private static HttpWebResponse[] myFileWebResponse;

        private bool Request() {
            bool requestOk = false;
            try {
                Window address = new Window("Ссылки для скачивания:");

                if (address.ShowDialog() == DialogResult.OK) {
                    string add = "";
                    add = address.NewName;
                    string[] uris = add.Split(new Char[] { ' ', ',' });

                    Uri[] uri = new Uri[uris.Length];
                    myFileWebResponse = new HttpWebResponse[uris.Length];
                    int i = 0;

                    foreach (string ur in uris) {
                        uri[i] = new Uri(ur);
                        HttpWebRequest myFileWebRequest = (HttpWebRequest)WebRequest.Create(uri[i]);
                        myFileWebResponse[i++] = (HttpWebResponse)myFileWebRequest.GetResponse();
                    }
                    requestOk = true;
                }
            } catch (WebException e) {
                Console.WriteLine("WebException: " + e.Message);
            } catch (UriFormatException e) {
                Console.WriteLine("UriFormatWebException: " + e.Message);
            }
            return requestOk;
        }

        private void quitToolStripMenuItem_Click(object sender, EventArgs e) {
            Exit();
        }

        private void optionsToolStripMenuItem_Click(object sender, EventArgs e) {
            Configuration conf = new Configuration(data.fontColor, data.color1, data.color2, data.fileFont, data.mainFont, data.dialogFont);
            conf.Font = data.dialogFont;
            DialogResult res = conf.ShowDialog();

            if (res == DialogResult.OK) {
                data.fontColor = conf.FontColor();
                data.color1 = conf.Color1();
                data.color2 = conf.Color2();
                data.fileFont = conf.FileFont();
                data.mainFont = conf.MainFont();
                data.dialogFont = conf.DialogFont();

                this.Font = data.mainFont;
                listView1.BackColor = data.color1;
                listView2.BackColor = data.color1;
                listView1.Font = data.fileFont;
                listView2.Font = data.fileFont;
                UpdateListView(listView1);
                UpdateListView(listView2);
            }
        }

        private void Save() {
            BinaryFormatter binFormat = new BinaryFormatter();
            Stream fStream = new FileStream(rootUser, FileMode.Create, FileAccess.Write, FileShare.None);
            binFormat.Serialize(fStream, data);
            fStream.Close();
        }

        private void saveSettingsToolStripMenuItem_Click(object sender, EventArgs e) {
            Save();
        }

        private string[] GetFiles(ListView lw) {
            try {
                string[] names = Directory.GetFiles(Root(lw), "*", SearchOption.AllDirectories);
                return names;
            } catch {
                return new string[0];
            }
        }
        private string[] GetDirectories(ListView lw) {
            try {
                string[] names = Directory.GetDirectories(Root(lw), "*", SearchOption.AllDirectories);
                return names;
            } catch (Exception ex) {
                MessageBox.Show(ex.Message);
                return new string[0];
            }
        }

        private void Find(ListView lw) {
            if (textBox3.Text == "")
                UpdateListView(lw);
            else {
                string[] fileNames = GetFiles(lw);
                string[] directoryNames = GetDirectories(lw);
                List<string> allNames = new List<string>();
                lw.Items.Clear();

                foreach (string name in directoryNames) {
                    allNames.Add(name);
                }

                foreach (string name in fileNames) {
                    allNames.Add(name);
                }

                List<string> names = allNames.FindAll(name => {
                    return PatternMatch(Path.GetFileName(name), textBox3.Text + "*");
                });

                foreach (string name in names) {
                    if (Directory.Exists(name)) {
                        lw.Items.Add(name);
                    }
                }

                foreach (string name in names) {
                    if (File.Exists(name)) {
                        lw.Items.Add(name);
                    }
                }
            }
        }

        private bool PatternMatch(string strSource, string strMask) {
            int SourceIndex = 0;
            int MaskIndex = 0;

            for (; MaskIndex < strMask.Length && SourceIndex < strSource.Length && strMask[MaskIndex] != '*'; MaskIndex++, SourceIndex++)
                if (strMask[MaskIndex] != strSource[SourceIndex] && strMask[MaskIndex] != '?')
                    return false;

            if (MaskIndex == strMask.Length)
                return true;

            int pSourceIndex = 0;
            int pMaskIndex = 0;

            for (; ; ) {
                if (SourceIndex >= strSource.Length) {
                    while (strMask[MaskIndex] == '*' && MaskIndex < strMask.Length - 1)
                        MaskIndex++;
                    return MaskIndex == strMask.Length ? true : false;
                }
                if (strMask[MaskIndex] == '*') {
                    if (++MaskIndex >= strMask.Length)
                        return true;
                    pMaskIndex = MaskIndex;
                    pSourceIndex = SourceIndex;
                    continue;
                }
                if (strMask[MaskIndex] == strSource[SourceIndex] || strMask[MaskIndex] == '?') {
                    MaskIndex++;
                    SourceIndex++;
                    continue;
                }
                MaskIndex = pMaskIndex; 
                SourceIndex = pSourceIndex++;
            }
        }

        private void textBox3_TextChanged(object sender, EventArgs e) {
            Find(lwOnline);
        }

        private void favoriteToolStripMenuItem_Click(object sender, EventArgs e) {
            Favorite window = new Favorite(dirs);
            DialogResult res = window.ShowDialog();

            if (res == DialogResult.OK) {
                if (lwOnline == listView1)
                    rootLeft = dirs[window.Choice];
                else
                    rootRight = dirs[window.Choice];
                UpdateListView(lwOnline);
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e) {
            Save();
        }

        private void downloadToolStripMenuItem_Click(object sender, EventArgs e) {
            var cancelSource = new CancellationTokenSource();

            if (Request()) {
                DownloadFiles window = new DownloadFiles(this, myFileWebResponse.Length, Root(lwOnline));

                string path = Root(WhichListView());
                if (path != "" && path[path.Length - 1] != Path.DirectorySeparatorChar)
                    path += Path.DirectorySeparatorChar;

                window.Show();
                window.Download(myFileWebResponse, path);
            }
        }

        private void statisticsToolStripMenuItem_Click(object sender, EventArgs e) {
            if (IsSelectedOne()) {
                Statistics();
            }
        }

        private void Statistics() {
            ListViewItem selectFile = WhichListView().SelectedItems[0];
            string path = Root(WhichListView()) + Path.DirectorySeparatorChar + selectFile.Text + selectFile.SubItems[1].Text;
            if (File.Exists(path)) {
                using (StreamReader file = new StreamReader(path,Encoding.UTF8, false)){

                    int words, strings;
                    string textStr = file.ReadToEnd();
                    string[] text = textStr.Split(' ');
                    string[] topTen = new string[10];

                    //words = (from word in textStr.Split(' ', '\n', '\r') where word.Contains(letters) select word).Count();
                    strings = (from word in textStr where word == '\n' || word == '\r' select word).Count() + 1;
                    topTen = (from word in text where (word != " " && word != "\n") select word).ToArray();

                    //MessageBox.Show("Количество слов: " + words + "\n" +
                    //                "Количество строк: " + strings + "\n\n");// +
                    //"Топ-10 слов:\n" + ToStr(topTen));
                }
            }
        }

        private string ToStr(string[] words) {
            string res = "";
            foreach (string word in words)
                res += word + " ";
            return res;
        }
    }


    public class QueueProcessor {
        private Queue<string> queue;
        private Thread thread;
        private StreamWriter file;

        public QueueProcessor(Queue<string> queue, StreamWriter file) {
            this.queue = queue;
            this.file = file;
            thread = new Thread(new ThreadStart(this.ThreadFunc));
        }

        public Thread TheThread {
            get {
                return thread;
            }
        }

        public void BeginProcessData() {
            thread.Start();
        }

        public void EndProcessData() {
            thread.Join();
        }

        private void ThreadFunc() {
            foreach (string path in queue)
                SearchFile(path, file);
        }

        private void SearchFile(string path, StreamWriter file) {
            try {
                using (FileStream fileS = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                    byte[] b = new byte[1024];
                    UTF8Encoding temp = new UTF8Encoding(true);
                    Regex[] r = new Regex[2];
                    r[0] = new Regex(@"[-a-f0-9_.]+@{1}[-0-9a-z]+\.[a-z]{2,5}");
                    r[1] = new Regex(@"(\+7|8)-\([0-9]{3}\)-[0-9]{3}-[0-9]{2}-[0-9]{2}");
                    string str;

                    while (fileS.Read(b, 0, b.Length) > 0) {
                        for (int i = 0; i < 2; i++)
                            foreach (Match m in r[i].Matches(temp.GetString(b))) {
                                str = m.ToString();
                                file.WriteLine(str);
                            }
                    }
                }
            } catch { }
        }
    }
}
