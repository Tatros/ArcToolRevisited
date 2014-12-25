using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;

namespace ArcToolRevisited
{
    /// <summary>
    /// Repacks Files into an RE5 .arc Archive.
    /// </summary>
    public class ArcRepacker
    {
        #region Members
        String inFolder;
        String outFile;
        TextWriter twr;

        List<ArcEntry> arcEntries;
        BinaryWriter bw;
        int fileStartOffset;
        int fileCurrentOffset;
        int dirOffset;
        bool convertDDS;
        #endregion

        #region Constructor
        public ArcRepacker(String inFolder, String outFile)
        {
            this.inFolder = inFolder;
            this.outFile = outFile;
            twr = Helper.createLogFile("LastRepack.log");
            arcEntries = new List<ArcEntry>();

            fileStartOffset = 32768;
            fileCurrentOffset = fileStartOffset;
        }
        #endregion

        #region Dialogues
        private void showConvertDialog()
        {
            if (MessageBox.Show("Convert back all found .dds to .tex before repack?",
                "Conversion Option", MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button1) == DialogResult.Yes)
                convertDDS = true;
            else
                convertDDS = false;

        }
        #endregion

        #region Arc related
        public void repackArc()
        {
            showConvertDialog();
            if (convertDDS)
                convertAllDDS();
            getArcEntries();
            closeLog();
            compressFiles();
            writeDirectory();
        }

        private void getArcEntries()
        {
            List<String> filePaths = new List<String>();
            string[] sfilePaths = Directory.GetFiles(inFolder, "*.*", SearchOption.AllDirectories);
            string[] extNames = Enum.GetNames(typeof(Helper.Ext));
            String fiExt;

            for (int i = 0; i < sfilePaths.Length; i++)
            {
                FileInfo fi = new FileInfo(sfilePaths[i]);
                fiExt = fi.Extension.ToLower().Substring(1);
                if (fiExt != "dds" && fiExt != "header")
                    filePaths.Add(sfilePaths[i]);
            }

            for (int i = 0; i < filePaths.Count; i++)
            {
                FileInfo fi = new FileInfo(filePaths[i]);
                try
                {
                    // get Size
                    String name = fi.Name;
                    String dir = fi.DirectoryName;
                    String ext = fi.Extension;
                    String fullPath = fi.FullName;

                    int size = (int)fi.Length;

                    arcEntries.Add(new ArcEntry(name, dir, ext, size, fullPath));

                    String nameInArc = dir + "\\" + arcEntries[i].FileName;
                    nameInArc = nameInArc.Substring(inFolder.Length + 1);
                    arcEntries[i].NameInArc = nameInArc;

                    // write log entries
                    log("# File: " + i, true);
                    log("Name: " + arcEntries[i].FileName, true);
                    log("Name in Arc: " + arcEntries[i].NameInArc, true);
                    log("Directory: " + arcEntries[i].Directory, true);
                    log("Extension: " + arcEntries[i].Extension, true);
                    log("Extension as HEX " + arcEntries[i].ExtensionAsHex, true);
                    log("Extension as HEX rev " + Helper.reverseHex(arcEntries[i].ExtensionAsHex), true);
                    log("Extension as Int " + arcEntries[i].ExtensionAsInt, true);
                    log("Full Path: " + arcEntries[i].FullPath, true);
                    log("Size: " + arcEntries[i].Size, true);
                    log("--------------------------------------------------------", true);
                }

                catch (FormatException e)
                {
                    String error = (e.Message + ": The extension " + fi.Extension + " is unknown!");
                    log("Warning: Extension " + fi.Extension + " in File " + fi.FullName + " UNKNOWN.", true);
                    log("", true);
                    log("Stack Trace:", true);
                    log(e.StackTrace, true);
                    closeLog();
                    throw new Exception(error);
                }

                catch (Exception e)
                {
                    log("ABORT: An error occured " + e.Message, true);
                    log("", true);
                    log("Stack Trace:", true);
                    log(e.StackTrace, true);
                    closeLog();
                    throw new Exception("An Error occured: " + e.Message);
                }
            }

            if (arcEntries.Count * 80 > 32768)
            {
                fileStartOffset = 65536;
                fileCurrentOffset = fileStartOffset;
            }
        }
        #endregion

        #region Conversion related
        private void convertAllDDS()
        {
            TexConverter tc = new TexConverter();
            string[] ddsFiles = Directory.GetFiles(inFolder, "*.dds", SearchOption.AllDirectories);
            for (int i = 0; i < ddsFiles.Length; i++)
                tc.convertDDSTex(ddsFiles[i], "");
        }
        #endregion

        #region File related
        public static void CopyStream(System.IO.Stream input, System.IO.Stream output)
        {
            byte[] buffer = new byte[2000];
            int len;
            while ((len = input.Read(buffer, 0, 2000)) > 0)
            {
                output.Write(buffer, 0, len);
            }
            output.Flush();
        }

        public void compressFile(ArcEntry e)
        {
            FileStream inputStream = new FileStream(e.FullPath, FileMode.Open);
            FileStream outputStream = new FileStream(e.FullPath + ".compressed", FileMode.Create);
            zlib.ZOutputStream outZStream = new zlib.ZOutputStream(outputStream, zlib.zlibConst.Z_DEFAULT_COMPRESSION);
            try
            {
                CopyStream(inputStream, outZStream);
            }
            finally
            {
                outZStream.Close();
                outputStream.Close();
                inputStream.Close();
            }
        }

        public void compressFiles()
        {
            foreach (ArcEntry e in arcEntries)
            {
                compressFile(e);
            }

            foreach (ArcEntry e in arcEntries)
            {
                getCompressedSize(e);
            }
        }
        #endregion

        #region Entry related
        public void writeDirectoryEntry(int i)
        {
            writeFileName(bw, i);
            writeFileExtension(bw, i);
            writeCompressedSize(bw, i);
            writeSize(bw, i);
            writeOffset(bw, i);
        }

        public void writeDirectory()
        {
            using (bw = new BinaryWriter(new
            FileStream(outFile, FileMode.Create)))
            {
                writeHeader(bw, (short)arcEntries.Count);
                for (int i = 0; i < arcEntries.Count; i++)
                {
                    writeDirectoryEntry(i);
                }

                writePadding();
                writeFiles();
            }
        }

        private void writePadding()
        {
            while (bw.BaseStream.Position < fileStartOffset)
                bw.BaseStream.WriteByte(0x0);
        }
        #endregion

        #region Utility Functions
        private void writeFiles()
        {
            for (int i = 0; i < arcEntries.Count; i++)
            {
                writeFile(bw, i);
            }
        }

        private void writeFile(BinaryWriter bw, int i)
        {
            FileStream input = new FileStream(arcEntries[i].FullPath + ".compressed", FileMode.Open, FileAccess.Read);
            int size = (int)input.Length;
            byte[] sBuffer = new byte[size];
            input.Read(sBuffer, 0, size);
            bw.Write(sBuffer, 0, size);
            input.Close();

            FileInfo fi = new FileInfo(arcEntries[i].FullPath + ".compressed");
            fi.Delete();
        }

        private int getCompressedSize(ArcEntry e)
        {
            FileInfo inf = new FileInfo(e.FullPath + ".compressed");
            e.CompressedSize = (int)inf.Length;
            return e.CompressedSize;
        }

        private void writeOffset(BinaryWriter bw, int i)
        {
            int currentOffset = fileStartOffset + dirOffset;
            dirOffset += arcEntries[i].CompressedSize;

            byte[] b = BitConverter.GetBytes(currentOffset);
            bw.Write(b);
        }

        public void writeCompressedSize(BinaryWriter bw, int i)
        {
            byte[] b = BitConverter.GetBytes(arcEntries[i].CompressedSize);
            bw.Write(b);
        }

        public void writeSize(BinaryWriter bw, int i)
        {
            byte[] b = BitConverter.GetBytes(arcEntries[i].Size);
            b[3] = 0x40;
            bw.Write(b);
        }

        public void writeFileName(BinaryWriter bw, int i)
        {
            byte[] b = ASCIIEncoding.ASCII.GetBytes(arcEntries[i].NameInArc);
            int c = 64 - b.Length;
            bw.Write(b);
            while (c > 0)
            {
                bw.BaseStream.WriteByte(0x0);
                c--;
            }
        }

        public void writeFileExtension(BinaryWriter bw, int i)
        {
            byte[] b = BitConverter.GetBytes(arcEntries[i].ExtensionAsInt);
            bw.Write(b);
        }

        public void writeHeader(BinaryWriter bw, short fileCount)
        {
            byte[] b = { 0x41, 0x52, 0x43, 0x00, 0x07, 0x00 };
            bw.Write(b);
            bw.Write(fileCount);
        }

        public void log(String message, bool lineBreak)
        {
            if (lineBreak)
                twr.WriteLine(message);
            else
                twr.Write(message);
        }

        public void closeLog()
        {
            twr.Close();
        }
        #endregion
    }
}