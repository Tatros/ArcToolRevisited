using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
//using RE5ArcLib;

namespace ArcToolRevisited
{
    public partial class Form2 : Form
    {
        #region Delegates
        public delegate void ActivateForm1(bool value);
        public delegate void ConverterDelegate(DDSConverter value);
        public ConverterDelegate ConverterDelCallback;
        public ActivateForm1 ActivateForm1Callback;
        #endregion

        #region Constructor
        public Form2()
        {
            InitializeComponent();
            this.MinimizeBox = false;
            this.MaximizeBox = false;
            this.ControlBox = false;
            progressBar1.Step = 1;
        }
        #endregion

        #region Extraction
        // extract re5 complete
        public void WorkerExtractAll(String reExecutable, String sPath, bool ownFolder, bool texOnly, DDSConverter conv)
        {
            this.progressBar1.Show();
            this.label1.Hide();
            // get RE5 path
            int pathIndex = reExecutable.LastIndexOf("\\");
            String rePath = reExecutable.Substring(0, pathIndex);

            // get all *.arc files
            String[] fileNames = getAllArcs(rePath);
            progressBar1.Maximum = fileNames.Length;

            // No arcs found?
            if (fileNames.Length == 0)
                MessageBox.Show("No ARC files found in " + rePath);
            else
            {
                // Start BackgroundWorker Thread
                object[] args = { fileNames, sPath, 1, ownFolder, texOnly, conv };

                this.Show();
                ActivateForm1Callback(false);
                backgroundWorker1.WorkerReportsProgress = true;
                backgroundWorker1.RunWorkerAsync(args);
            }
        }

        // extract arc(s)
        public void WorkerExtractArcs(String[] fileNames, String sPath, bool ownFolder, bool texOnly, DDSConverter conv)
        {
            this.progressBar1.Show();
            this.label1.Hide();
            // set Max for progressbar
            progressBar1.Maximum = fileNames.Length;

            // Start BackgroundWorker Thread
            object[] args = { fileNames, sPath, 0, ownFolder, texOnly, conv };
            this.Show();
            ActivateForm1Callback(false);
            backgroundWorker1.WorkerReportsProgress = true;
            backgroundWorker1.RunWorkerAsync(args);
        }
        #endregion

        #region Repacking
        // Repack arc
        public void WorkerRepackArcs(String repackFolder, String sFile)
        {
            // MessageBox.Show("Folder to pack: " + repackFolder);
            // MessageBox.Show("File to Save as: " + sFile);
            this.progressBar1.Hide();
            this.label1.Text = "Please wait: Generating ARC \n" + sFile;
            this.label1.Show();

            // Start BackgroundWorker Thread
            object[] args = { repackFolder, sFile, 2 };
            this.Show();
            ActivateForm1Callback(false);
            backgroundWorker1.WorkerReportsProgress = true;
            backgroundWorker1.RunWorkerAsync(args);
        }
        #endregion

        #region Conversion
        // get a DDSConverter
        public void getConverter()
        {
            this.progressBar1.Hide();
            this.label1.Text = "Please wait: Loading... \n";
            this.label1.Show();
            this.Show();

            object[] args = { null, null, 5 };
            ActivateForm1Callback(false);
            backgroundWorker1.WorkerReportsProgress = true;
            backgroundWorker1.RunWorkerAsync(args);
        }

        // convert DDS to TEX
        public void WorkerConvertDDSTex(String[] ddsFiles, String outFolder)
        {
            this.progressBar1.Show();
            this.label1.Hide();
            // set Max for progressbar
            progressBar1.Maximum = ddsFiles.Length;

            // Start BackgroundWorker Thread
            object[] args = { ddsFiles, outFolder, 3, false };
            this.Show();
            ActivateForm1Callback(false);
            backgroundWorker1.WorkerReportsProgress = true;
            backgroundWorker1.RunWorkerAsync(args);
        }

        // Convert complete Folder DDS to TEX
        public void WorkerConvertFolderDDSTex(String[] ddsFiles, bool delEntries)
        {
            this.progressBar1.Show();
            this.label1.Hide();
            // set Max for progressbar
            progressBar1.Maximum = ddsFiles.Length;

            // Start BackgroundWorker Thread
            object[] args = { ddsFiles, "", 4, delEntries };
            this.Show();
            ActivateForm1Callback(false);
            backgroundWorker1.WorkerReportsProgress = true;
            backgroundWorker1.RunWorkerAsync(args);
        }

        // Tex to DDS
        public void WorkerConvertTexDDS(string[] texFiles, string outFolder, DDSConverter conv)
        {
            this.progressBar1.Show();
            this.label1.Hide();
            // set Max for progressbar
            progressBar1.Maximum = texFiles.Length;

            // Start BackgroundWorker Thread
            object[] args = { texFiles, outFolder, 6, conv };
            this.Show();
            ActivateForm1Callback(false);
            backgroundWorker1.WorkerReportsProgress = true;
            backgroundWorker1.RunWorkerAsync(args);
        }
        #endregion

        #region Async WorkerThread
        /// <summary>
        /// This is the async. WorkerThread:
        /// Options: args[2]
        /// values: 0 = extract selected, 1 = extract all, 2 = repack, 3 = DDS to TEX single,
        /// 4 = DDS to TEX all + subdirs, 5 = get a Converter, 6 = TEX to DDS
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            object[] args = (object[])e.Argument;

            // Selection: extract selected arcs only
            if ((int)args[2] == 0)
            {
                // get Args
                String[] fileNames = (String[])args[0];
                String sPath = (String)args[1];
                bool ownFolder = (bool)args[3];
                bool texOnly = (bool)args[4];
                DDSConverter conv = (DDSConverter)args[5];

                // Log File
                TextWriter twr = Helper.createLogFile("RE5ArcExtractor.log");

                // Extract Arcs
                for (int i = 0; i < fileNames.Length; i++)
                {
                    if (this.backgroundWorker1.CancellationPending)
                    {
                        Console.WriteLine("User aborted convserion");
                        twr.WriteLine("User aborted extraction.");
                        twr.Close();
                        break;
                    }
                    backgroundWorker1.ReportProgress(i, fileNames[i]);

                    // write log
                    Console.WriteLine("Extracting: " + fileNames[i]);
                    twr.Write("Extracting " + fileNames[i] + "... ");

                    // extract current arc file
                    ArcExtractor arcEx = new ArcExtractor(sPath, fileNames[i], ownFolder, texOnly, conv);
                    try
                    {
                        if (!arcEx.extractArc())
                        {
                            twr.WriteLine("Warning: " + arcEx.ErrorMsg);
                        }
                        else
                        {
                            // end log
                            twr.WriteLine("done.");
                        }
                    }
                    catch (Exception error)
                    {
                        MessageBox.Show(error.Message);
                        twr.WriteLine("Abort: " + error.Message);
                        twr.WriteLine("Stack Trace: ");
                        twr.WriteLine(error.StackTrace);
                    }
                }
                twr.Close();
            }

            // Selection: extract Re5
            else if ((int)args[2] == 1)
            {
                // get Args
                String[] fileNames = (String[])args[0];
                String sPath = (String)args[1];
                bool ownFolder = (bool)args[3];
                bool texOnly = (bool)args[4];
                DDSConverter conv = (DDSConverter)args[5];

                // Create log writer
                TextWriter twr = Helper.createLogFile("RE5ArcExtractor.log");

                // Extract each arc
                for (int i = 0; i < fileNames.Length; i++)
                {
                    if (this.backgroundWorker1.CancellationPending)
                    {
                        Console.WriteLine("User aborted convserion");
                        twr.WriteLine("User aborted extraction.");
                        twr.Close();
                        break;
                    }

                    backgroundWorker1.ReportProgress(i, fileNames[i]);

                    // write log
                    Console.WriteLine("Extracting: " + fileNames[i]);
                    twr.Write("Extracting " + fileNames[i] + "... ");

                    // extract current arc file
                    ArcExtractor arcEx = new ArcExtractor(sPath, fileNames[i], ownFolder, texOnly, conv);
                    try
                    {
                        if (!arcEx.extractArc())
                        {
                            twr.WriteLine("Warning: " + arcEx.ErrorMsg);
                        }

                        else
                        {
                            // end log
                            twr.WriteLine("done.");
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("error: " + ex.Message + "\n" + ex.StackTrace);
                        twr.WriteLine("Error: " + ex.Message);
                        twr.WriteLine(ex.StackTrace);
                    }
                }
                twr.Close();
            }

            // Selection: repack Arcs
            else if ((int)args[2] == 2)
            {
                // get Args
                String inFolder = (String)args[0];
                String outFile = (String)args[1];

                try
                {
                    ArcRepacker repacker = new ArcRepacker(inFolder, outFile);
                    repacker.repackArc();
                }
                catch (Exception error)
                {
                    MessageBox.Show(error.Message);
                }
            }

            // convert dds to tex
            else if ((int)args[2] == 3)
            {
                String[] ddsFiles = (String[])args[0];
                String outFolder = (String)args[1];
                FileInfo fi;

                TexConverter converter = new TexConverter();

                // Create log writer
                TextWriter twr = Helper.createLogFile("RE5Converter.log");

                // convert each DDS
                for (int i = 0; i < ddsFiles.Length; i++)
                {
                    if (this.backgroundWorker1.CancellationPending)
                    {
                        Console.WriteLine("User aborted convserion");
                        twr.WriteLine("User aborted conversion.");
                        twr.Close();
                        break;
                    }

                    backgroundWorker1.ReportProgress(i, ddsFiles[i]);

                    // write log
                    Console.WriteLine("Converting: " + ddsFiles[i]);
                    twr.Write("Converting " + ddsFiles[i] + "... ");

                    // convert current DDS
                    try
                    {
                        fi = new FileInfo(ddsFiles[i]);
                        if (!fi.Exists)
                            throw new Exception("The File " + fi.Name + " does not exist.");

                        String texName = fi.Name;
                        texName = texName.Substring(0, texName.LastIndexOf(".")) + ".tex";

                        converter.convertDDSTex(ddsFiles[i], outFolder);
                        twr.WriteLine("done.");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("error: " + ex.Message + "\n");
                        twr.WriteLine("Error: " + ex.Message);
                        twr.WriteLine(ex.StackTrace);
                    }
                }
                twr.Close();
            }

            // convert dds to tex ALL in Folder /w subdirs
            else if ((int)args[2] == 4)
            {
                String[] ddsFiles = (String[])args[0];
                bool delEntries = (bool)args[3];
                FileInfo fi;

                TexConverter converter = new TexConverter();

                // Create log writer
                TextWriter twr = Helper.createLogFile("RE5Converter.log");

                // convert each DDS
                for (int i = 0; i < ddsFiles.Length; i++)
                {
                    if (this.backgroundWorker1.CancellationPending)
                    {
                        Console.WriteLine("User aborted convserion");
                        twr.WriteLine("User aborted conversion.");
                        twr.Close();
                        break;
                    }

                    backgroundWorker1.ReportProgress(i, ddsFiles[i]);

                    // write log
                    Console.WriteLine("Converting: " + ddsFiles[i]);
                    twr.Write("Converting " + ddsFiles[i] + "... ");

                    // convert current DDS
                    try
                    {
                        fi = new FileInfo(ddsFiles[i]);
                        if (!fi.Exists)
                            throw new Exception("The File " + fi.Name + " does not exist.");

                        String texName = fi.FullName;
                        texName = texName.Substring(0, texName.LastIndexOf(".")) + ".tex";

                        converter.convertDDSTex(ddsFiles[i], "");
                        twr.WriteLine("done.");
                        if (delEntries)
                        {
                            String hFile = fi.FullName.Substring(0, fi.FullName.Length - 3);
                            hFile += "header";
                            FileInfo hFi = new FileInfo(hFile);
                            hFi.Delete();
                            fi.Delete();
                            twr.WriteLine("deleted: " + fi.FullName);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error: " + ex.Message + "\n");
                        twr.WriteLine("Error: " + ex.Message);
                        twr.WriteLine(ex.StackTrace);
                    }
                }
                twr.Close();
            }

            // Return Converter
            else if ((int)args[2] == 5)
            {
                DDSConverter conv = new DDSConverter();
                backgroundWorker1.RunWorkerCompleted += new RunWorkerCompletedEventHandler(doneLoading);
                e.Result = conv;
            }

            // TEX to DDS
            else if ((int)args[2] == 6)
            {
                String[] texFiles = (String[])args[0];
                String outDir = (String)args[1];
                DDSConverter conv = (DDSConverter)args[3];

                // Create log writer
                TextWriter twr = Helper.createLogFile("RE5Converter.log");

                // convert each TEX to DDS
                for (int i = 0; i < texFiles.Length; i++)
                {
                    if (this.backgroundWorker1.CancellationPending)
                    {
                        Console.WriteLine("User aborted convserion");
                        twr.WriteLine("User aborted conversion.");
                        twr.Close();
                        break;
                    }

                    backgroundWorker1.ReportProgress(i, texFiles[i]);

                    // write log
                    Console.WriteLine("Converting: " + texFiles[i]);
                    twr.Write("Converting " + texFiles[i] + "... ");

                    try
                    {
                        conv.convertTexDDS(texFiles[i], outDir);
                        twr.WriteLine("done.");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error: " + ex.Message + "\n");
                        twr.WriteLine("Error: " + ex.Message);
                        twr.WriteLine(ex.StackTrace);
                    }
                }
                twr.Close();
            }
        }
        #endregion

        #region WorkerThread Handlers
        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            String filen = (String)e.UserState;
            int fileStartPos = filen.LastIndexOf("\\");
            this.Text = "Processing: " + filen.Substring(fileStartPos);
            progressBar1.PerformStep();
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            ActivateForm1Callback(true);
            this.Dispose();
        }

        private void doneLoading(object sender, RunWorkerCompletedEventArgs e)
        {
            ConverterDelCallback((DDSConverter)e.Result);
        }
        #endregion

        #region Misc
        public string[] getAllArcs(String rootPath)
        {
            return Directory.GetFiles(rootPath, "*.arc", SearchOption.AllDirectories);
        }
        #endregion

        private void button1_Click(object sender, EventArgs e)
        {
            this.backgroundWorker1.CancelAsync();
        }
    }
}