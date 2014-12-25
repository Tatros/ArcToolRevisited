using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace ArcToolRevisited
{
    /// <summary>
    /// A form that allows choosing a conversion method.
    /// Currently: {ALL in Folder}, {Selection}
    /// </summary>
    public partial class ConvertChoice : Form
    {
        #region Delegates
        public delegate void ActivateForm1_convert(bool value);
        public ActivateForm1_convert ActivateForm1ConvertCallback;
        #endregion

        #region Constructor
        public ConvertChoice()
        {
            InitializeComponent();
        }
        #endregion

        #region Misc UI Funct
        public void ShowForm()
        {
            this.Show();
            ActivateForm1ConvertCallback(false);
        }

        private bool showDeleteEntries()
        {
            string message = "Delete DDS Files after conversion?";
            string caption = "Please choose";

            MessageBoxButtons buttons = MessageBoxButtons.YesNo;
            DialogResult result;

            // Displays the MessageBox.
            result = MessageBox.Show(message, caption, buttons);

            if (result == System.Windows.Forms.DialogResult.Yes)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion

        #region Button Handling
        // convert all dds in directory
        private void button2_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog oFolderDialog = new System.Windows.Forms.FolderBrowserDialog();
            oFolderDialog.Description = "Select the folder containing the .dds files to convert.";
            if (oFolderDialog.ShowDialog() == DialogResult.OK)
            {
                bool delEntries = showDeleteEntries();
                Form2 exForm = new Form2();
                exForm.ActivateForm1Callback = new Form2.ActivateForm1(activateForm1);
                string[] filenames = getAllFiles(oFolderDialog.SelectedPath, "*.DDS");
                exForm.WorkerConvertFolderDDSTex(filenames, delEntries);
                this.Dispose();
            }
        }

        // manually select files to convert
        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog oFileDialog = new OpenFileDialog();
            oFileDialog.Multiselect = true;
            oFileDialog.Title = "Select the DDS Texture file(s) that you want to convert into .TEX. You can select multiple files.";
            oFileDialog.Filter = "DXT1/5|*.dds";
            if (oFileDialog.ShowDialog() == DialogResult.OK)
            {
                FolderBrowserDialog sFileDialog = new System.Windows.Forms.FolderBrowserDialog();
                sFileDialog.Description = "Select a destination Directory";
                if (sFileDialog.ShowDialog() == DialogResult.OK)
                {
                    Form2 exForm = new Form2();
                    exForm.ActivateForm1Callback = new Form2.ActivateForm1(activateForm1);
                    exForm.WorkerConvertDDSTex(oFileDialog.FileNames, sFileDialog.SelectedPath);
                    this.Dispose();
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            ActivateForm1ConvertCallback(true);
            this.Dispose();
        }
        #endregion

        #region Callback Functions
        private void activateForm1(bool value)
        {
            ActivateForm1ConvertCallback(value);
        }
        #endregion

        #region Utility Functions
        private string[] getAllFiles(String rootPath, String typeFilter)
        {
            return Directory.GetFiles(rootPath, typeFilter, SearchOption.AllDirectories);
        }
        #endregion
    }
}