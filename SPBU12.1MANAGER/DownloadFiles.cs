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
//using System.Security.Cryptography.X509Certificates;

namespace SPBU12._1MANAGER {
    public partial class DownloadFiles : Form {
        private CancellationTokenSource[] cancelTokens;
        private CancellationToken[] cancellationTokens;
        string[] theEBook;
        Task<string>[] theEBookTask;
        Form1 parent;
        private ProgressBar[] progressBars;
        private Button[] buttons;
        string root;

        SynchronizationContext syncContext = SynchronizationContext.Current;

        public DownloadFiles(Form1 parent, int count, string root) {
            cancelTokens = new CancellationTokenSource[count];
            cancellationTokens = new CancellationToken[count];
            theEBook = new string[count];
            theEBookTask = new Task<string>[count];
            progressBars = new ProgressBar[count];
            buttons = new Button[count];

            this.parent = parent;
            this.root = root;
            InitializeComponent();
            this.Size = new Size(288, 125 + 25 * (count - 1));

            for (int i = 0; i < count; i++) {
                progressBars[i] = new ProgressBar();
                progressBars[i].Size = new Size(100, 23);
                progressBars[i].Location = new Point(14, 57 + 24 * i);
                progressBars[i].Visible = true;

                cancelTokens[i] = new CancellationTokenSource();
                cancellationTokens[i] = cancelTokens[i].Token;

                buttons[i] = new Button();
                buttons[i].Location = new Point(120, 57 + 24 * i);
                buttons[i].Text = "ОТМЕНА";
                buttons[i].Visible = true;

                this.Controls.Add(progressBars[i]);
                this.Controls.Add(buttons[i]);
            }

            int k = 0;

            foreach (var cancelToken in cancelTokens) {
                buttons[k++].Click += new EventHandler((object sender, EventArgs e) => cancelToken.Cancel());
            }
            this.Shown += DownloadFiles_Shown;
        }

        private void DownloadFiles_Shown(object sender, EventArgs e) {
            this.Update();
        }

        public string[] EBOOK { get { return this.theEBook; } }

        public string ROOT { get { return this.root; } }

        private Task<string> readFile(HttpWebResponse myFWResp, IProgress<int> progress, CancellationToken cancellationToken) {
            return Task.Run(() => {
                string res = "";

                int delta = 256;
                char[] readBuffer = new Char[delta];
                int max = (int)myFWResp.ContentLength;
                int k = 0;

                using (Stream receiveStream = myFWResp.GetResponseStream()) {
                    using (StreamReader readStream = new StreamReader(receiveStream, Encoding.Default)) {
                        try {
                            int count = readStream.Read(readBuffer, 0, delta);
                            while (count > 0) {
                                String str = new String(readBuffer, 0, count);
                                res += str;
                                count = readStream.Read(readBuffer, 0, delta);

                                syncContext.Send(_ => {
                                    progress.Report(k++ * delta * 100 / max);
                                }, null);

                                if (cancellationToken.IsCancellationRequested) {
                                    syncContext.Send(_ => {
                                        progress.Report(0);
                                    }, null);
                                    return null;
                                }
                            }
                            this.Invoke((Action)delegate { progress.Report(100); });
                            return res;
                        } finally {
                            myFWResp.Close();
                        }
                    }
                }
            });
        }

        public async void Download(HttpWebResponse[] myFileWebResponse, string path) {
            Progress<int>[] progress = new Progress<int>[myFileWebResponse.Length];
            Task<String>[] tasks = new Task<string>[myFileWebResponse.Length];

            Parallel.For(0, progressBars.Length, (i) => {
                progress[i] = new Progress<int>(v => {
                    syncContext.Send(_ => {
                        progressBars[i].Value = v;
                    }, null);
                });
                tasks[i] = readFile(myFileWebResponse[i], progress[i], cancellationTokens[i]);
                tasks[i].GetAwaiter()
                .OnCompleted(() => EBOOK[i] = tasks[i].Result);
            });

            await Task.WhenAll(tasks);
            parent.Download(this);
            MessageBox.Show("Готово!");
            this.Close();
        }
    }
}
