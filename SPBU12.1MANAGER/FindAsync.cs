using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace SPBU12._1MANAGER {
    public partial class FindAsync : Form {
        private CancellationTokenSource cancelToken;
        private CancellationToken cancellationToken;
        string personData;
        Form1 parent;
        int count;
        string dName;
        double time;
        StreamWriter file;

        public FindAsync(Form1 parent, string path) {
            cancelToken = new CancellationTokenSource();
            cancellationToken = cancelToken.Token;
            this.parent = parent;
            this.dName = path;
            count = 0;
            personData = "";
            
            InitializeComponent();
        }

        public string DATA {
            get {
                return personData;
            }
        }

        public double TIME {
            get {
                return time;
            }
        }

        public StreamWriter FILE {
            get {
                return file;
            }
        }

        private void button1_Click(object sender, EventArgs e) {
            cancelToken.Cancel();
            personData = null;
            parent.SearchAA(this, dName);
        }

        public async void Start(string path, StreamWriter fileS) {
            file = fileS;

            var progress = new Progress<int>(v => {
                progressBar1.Value = v;
                progressBar1.Update();
            });

            Stopwatch time = new Stopwatch();
            List<Task<String>> tasks = new List<Task<string>>();
            
            time.Start();
            Search(path, progress, tasks);
            count = tasks.Count();
            int k = 0; 

            foreach (var task in tasks) {
                personData += await task;
                ((IProgress<int>)progress).Report(k++ * 100/ count);
            }
            time.Stop();

            this.time = time.ElapsedMilliseconds / 1000;
            parent.SearchAA(this, dName);
        }

        public void Search(string path, Progress<int> progress, List<Task<string>> tasks) {
            try {
                DirectoryInfo di = new DirectoryInfo(path);
                DirectoryInfo[] directories = di.GetDirectories();
                FileInfo[] files = di.GetFiles();

                foreach (DirectoryInfo info in directories) {
                    Search(path + Path.DirectorySeparatorChar + info.Name, progress, tasks);
                }

                foreach (FileInfo info in files) {
                    tasks.Add(SearchFileAsync(path + Path.DirectorySeparatorChar + info.Name, progress));
                }
            } 
            catch {
            }
        } 
        
        private Task<string> SearchFileAsync(string path, IProgress<int> progress) {
            return Task.Run(() => {
                string res = "---";
                try {
                    using (FileStream fileS = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                        byte[] b = new byte[1024];
                        UTF8Encoding temp = new UTF8Encoding(true);
                        Regex[] r = new Regex[2];
                        r[0] = new Regex(@"[-a-f0-9_.]+@{1}[-0-9a-z]+\.[a-z]{2,5}");
                        r[1] = new Regex(@"(\+7|8)-\([0-9]{3}\)-[0-9]{3}-[0-9]{2}-[0-9]{2}");

                        while (fileS.Read(b, 0, b.Length) > 0) {
                            for (int i = 0; i < 2; i++)
                                foreach (Match m in r[i].Matches(temp.GetString(b))) {
                                    res += m.ToString();
                                    cancellationToken.ThrowIfCancellationRequested();
                                }
                        }
                    }
                } catch { }

                return res;
            });
        }
    }
}
