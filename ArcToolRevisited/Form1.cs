using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Reflection;
//using RE5ArcLib;

namespace ArcToolRevisited
{
    public partial class Form1 : Form
    {
        #region Members
        private System.Windows.Forms.OpenFileDialog oFileDialog;
        private System.Windows.Forms.FolderBrowserDialog sFileDialog;
        private DDSConverter DDSConvert;
        #endregion

        #region Preload
        // init and load the form
        public Form1(String execPath)
        {
            Helper.homeDir = execPath;
            int pos = Helper.homeDir.LastIndexOf(@"\");
            Helper.homeDir = Helper.homeDir.Substring(0, pos);
            Console.WriteLine(Helper.homeDir);
            if (!System.IO.Directory.Exists(Helper.homeDir + @"\log"))
            {
                System.IO.Directory.CreateDirectory(Helper.homeDir + @"\log");
                System.Console.WriteLine("created " + Helper.homeDir + @"\log");
            }
            InitializeComponent();
            this.MaximizeBox = false;
            //this.checkBoxOwnFolder.Text = "Extract each ARC\n to it's own Folder";
            this.checkBoxOwnFolder.CheckAlign = ContentAlignment.TopLeft;

            this.checkBoxOwnFolder.TextAlign = ContentAlignment.TopLeft;
            this.Show();
            Form2 load = new Form2();
            load.StartPosition = FormStartPosition.Manual;
            load.Location = new Point(this.Left + (this.Width / 2 - load.Width / 2), this.Top + (this.Height / 2 - load.Height / 2));
            load.ActivateForm1Callback = new Form2.ActivateForm1(activateForm);
            load.ConverterDelCallback = new Form2.ConverterDelegate(recConverter);
            load.getConverter();
        }
        #endregion

        #region onLoad
        private void Form1_Load(object sender, EventArgs e)
        {
        }
        #endregion

        #region Extraction
        // extract selected arcs
        private void button1_Click(object sender, EventArgs e)
        {
            oFileDialog = new OpenFileDialog();
            oFileDialog.Multiselect = true;
            oFileDialog.Title = "Select the RE5 ARC file that you want to extract";
            oFileDialog.Filter = "RE5 arc|*.arc";

            if (oFileDialog.ShowDialog() == DialogResult.OK)
            {
                sFileDialog = new System.Windows.Forms.FolderBrowserDialog();
                sFileDialog.Description = "Select a destination Directory";

                if (sFileDialog.ShowDialog() == DialogResult.OK)
                {
                    bool ownFolder = checkBoxOwnFolder.Checked;
                    bool texOnly = checkBoxTexOnly.Checked;
                    Form2 exForm = new Form2();
                    exForm.StartPosition = FormStartPosition.Manual;
                    exForm.Location = new Point(this.Left + (this.Width / 2 - exForm.Width / 2), this.Top + (this.Height / 2 - exForm.Height / 2));
                    exForm.ActivateForm1Callback = new Form2.ActivateForm1(activateForm);
                    exForm.WorkerExtractArcs(oFileDialog.FileNames, sFileDialog.SelectedPath, ownFolder, texOnly, DDSConvert);
                }
            }
        }

        // extract re5 complete
        private void button2_Click(object sender, EventArgs e)
        {
            oFileDialog = new OpenFileDialog();
            oFileDialog.Multiselect = false;
            oFileDialog.Title = "Please select RE5DX9.EXE or RE5DX10.exe in your main RE5 directory.";
            oFileDialog.Filter = "RE5 DX9 executable|RE5DX9.EXE|RE5 DX10 executable|RE5DX10.exe";

            if (oFileDialog.ShowDialog() == DialogResult.OK)
            {
                sFileDialog = new System.Windows.Forms.FolderBrowserDialog();
                sFileDialog.Description = "Select a destination Directory";

                if (sFileDialog.ShowDialog() == DialogResult.OK)
                {
                    bool ownFolder = checkBoxOwnFolder.Checked;
                    bool texOnly = checkBoxTexOnly.Checked;
                    Form2 exForm = new Form2();
                    exForm.StartPosition = FormStartPosition.Manual;
                    exForm.Location = new Point(this.Left + (this.Width / 2 - exForm.Width / 2), this.Top + (this.Height / 2 - exForm.Height / 2));
                    exForm.ActivateForm1Callback = new Form2.ActivateForm1(activateForm);
                    exForm.WorkerExtractAll(oFileDialog.FileName, sFileDialog.SelectedPath, ownFolder, texOnly, DDSConvert);
                }
            }
        }
        #endregion

        #region Repacking
        // repack arc
        private void button3_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog oFolderDialog = new FolderBrowserDialog();
            oFolderDialog.Description = "Select the folder containing the folders and files to repack";

            if (oFolderDialog.ShowDialog() == DialogResult.OK)
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.DefaultExt = "arc";
                saveFileDialog.Filter = "RE5 arc|*.arc";
                saveFileDialog.InitialDirectory = oFolderDialog.SelectedPath;
                saveFileDialog.AddExtension = false;
                saveFileDialog.Title = "Name of the ARC file to generate.";

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    Form2 repackForm = new Form2();
                    repackForm.StartPosition = FormStartPosition.Manual;
                    repackForm.Location = new Point(this.Left + (this.Width / 2 - repackForm.Width / 2), this.Top + (this.Height / 2 - repackForm.Height / 2));
                    repackForm.ActivateForm1Callback = new Form2.ActivateForm1(activateForm);
                    repackForm.WorkerRepackArcs(oFolderDialog.SelectedPath, saveFileDialog.FileName);
                }
            }
        }
        #endregion

        #region Conversion
        // DDS to TEX
        private void btnDDSTex_Click(object sender, EventArgs e)
        {
            ConvertChoice choice = new ConvertChoice();
            choice.StartPosition = FormStartPosition.Manual;
            //choice.Location = new Point(this.Left + (this.Width / 2 - choice.Width / 2), this.Top + (this.Height / 2 - choice.Height / 2));
            choice.Location = new Point(this.Left + (this.Width / 2 - choice.Width / 2), this.Top + (this.Height / 2 - choice.Height / 2));
            choice.Size = new Size(270, 90);
            choice.ActivateForm1ConvertCallback = new ConvertChoice.ActivateForm1_convert(activateForm);
            choice.ShowForm();
        }

        // TEX to DDS
        private void btnTexDDS_Click(object sender, EventArgs e)
        {
            oFileDialog = new OpenFileDialog();
            oFileDialog.Multiselect = true;
            oFileDialog.Title = "Select the TEX File(s) to convert to DDS. Some files (e.g. CubeMaps) not supported. You can select multiple files.";
            oFileDialog.Filter = "RE5 TEX|*.tex";

            if (oFileDialog.ShowDialog() == DialogResult.OK)
            {
                FolderBrowserDialog oFolderDialog = new FolderBrowserDialog();
                oFolderDialog.Description = "Select output directory";

                if (oFolderDialog.ShowDialog() == DialogResult.OK)
                {
                    Form2 exForm = new Form2();
                    exForm.StartPosition = FormStartPosition.Manual;
                    exForm.Location = new Point(this.Left + (this.Width / 2 - exForm.Width / 2), this.Top + (this.Height / 2 - exForm.Height / 2));
                    exForm.ActivateForm1Callback = new Form2.ActivateForm1(activateForm);
                    exForm.WorkerConvertTexDDS(oFileDialog.FileNames, oFolderDialog.SelectedPath, this.DDSConvert);
                }
            }
        }
        #endregion

        #region Callback Functions
        // Callback Functions
        private void activateForm(bool value)
        {
            this.Enabled = value;
        }

        private void recConverter(DDSConverter conv)
        {
            this.DDSConvert = conv;
        }
        #endregion

        #region lol ui stuff
        // UI Stuff
        private void label1_Click(object sender, EventArgs e)
        {
            MessageBox.Show("RE5 ArcTool Revisited\n\nFound a bug?\nWant to request a feature?\nEmail me at tattioff42@gmail.com");
        }

        private void label1_MouseMove(object sender, MouseEventArgs e)
        {
            label1.ForeColor = Color.Black;
        }

        private void label1_MouseLeave(object sender, EventArgs e)
        {
            label1.ForeColor = System.Drawing.SystemColors.MenuHighlight;
        }

        private void label2_MouseMove(object sender, MouseEventArgs e)
        {
            label2.ForeColor = Color.Black;
        }

        private void label2_MouseLeave(object sender, EventArgs e)
        {
            label2.ForeColor = System.Drawing.SystemColors.MenuHighlight;
        }
        #endregion

        private void label2_Click(object sender, EventArgs e)
        {
            var help = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "help.txt");
            System.IO.File.WriteAllText(help, ArcToolRevisited.Properties.Resources.help);
            System.Diagnostics.Process.Start(help);
        }
    }
}