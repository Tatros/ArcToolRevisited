using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ArcToolRevisited
{
    /// <summary>
    /// ArcExtractor reads a RE5 ARC file and writes it's contents to a specified directory
    /// </summary>
    public class ArcExtractor
    {
        #region Members
        private String _errorMsg;
        private String _outDir;
        private Arc _inArc;
        private Byte[] buffer;
        private BinaryReader _br;
        private int _pos;
        private long _length;
        private bool _ownFolder;
        private bool _texOnly;

        private int _dirEntrySize = 80;
        private int _dirEnd = 32768;

        private List<FileEntry> arcEntries;
        TextWriter tw;
        private DDSConverter ddsConv;
        #endregion

        #region Constructor
        public ArcExtractor(String outDir, String inArc, bool ownFolder, bool texOnly, DDSConverter conv)
        {
            tw = Helper.createLogFile("LastExtract.log");

            _inArc = new Arc(inArc);
            FileInfo ifInArc = new FileInfo(inArc);
            if (ownFolder)
                _outDir = outDir + @"\" + Helper.nameWithoutExtension(ifInArc.Name);
            else
                _outDir = outDir;

            _pos = 0;
            buffer = new Byte[80];
            _ownFolder = ownFolder;
            _texOnly = texOnly;

            arcEntries = new List<FileEntry>();
            _errorMsg = "Sorry, unhandled Error while extracting ARC. :'(";
            ddsConv = conv;
        }
        #endregion

        #region Arc related
        public bool extractArc()
        {
            try
            {
                if (openArc())
                {
                    writeFilesToLog();
                    closeLog();
                    return true;
                }

                log("An Error occured: Did not finish properly", true);
                closeLog();
                return false;
            }
            catch (Exception ex)
            {
                log("An Error occured: Did not finish properly", true);
                log(ex.Message, true);
                log(ex.StackTrace, true);
                closeLog();
                throw;
            }
        }

        private bool openArc()
        {
            try
            {
                using (_br = new BinaryReader(File.Open(_inArc.Name, FileMode.Open)))
                {
                    // Length of Stream
                    _length = _br.BaseStream.Length;

                    // Read first 8 bytes for ARC Info
                    _br.Read(buffer, 0, 8);

                    // set Magic, Version, FilesCount
                    _inArc.Magic = new String(Encoding.ASCII.GetChars(buffer, 0, 3));
                    _inArc.Version = BitConverter.ToInt16(buffer, 4);
                    _inArc.FilesCount = BitConverter.ToInt16(buffer, 6);
                    _pos = 8;

                    if ((_inArc.FilesCount * 80) >= 32768)
                        _dirEnd = 65536;

                    processEntries();
                    processFiles();
                    return true;
                }
            }

            catch (Exception e)
            {
                throw new Exception(_errorMsg + ": " + e.Message);
            }
        }
        #endregion

        #region Entry related
        private void processEntries()
        {
            // Does another entry fit into the directory?
            while (_pos + _dirEntrySize < _dirEnd)
            {
                // buffer next 80 bytes
                _br.Read(buffer, 0, 80);
                _pos += 80;

                // if Filename = 0, return
                if (BitConverter.ToInt64(buffer, 0) == 0)
                    break;

                // Add new File to list, skip file if Textures only and no texture
                arcEntries.Add(
                    new FileEntry(
                        new String(Encoding.ASCII.GetChars(buffer, 0, 64)),
                        BitConverter.ToInt32(buffer, 64),
                        BitConverter.ToInt32(buffer, 68),
                        BitConverter.ToInt32(new byte[4] { buffer[72], buffer[73], buffer[74], 0 }, 0),
                        BitConverter.ToInt32(buffer, 76)
                        ));
            }
        }
        #endregion

        #region File related
        private void processFiles()
        {
            bool skip;
            // current FileEntry
            FileEntry currentEntry;
            long currentOffset;

            // go to the files offset
            _br.BaseStream.Seek(32768, 0);
            currentOffset = 32768;
            currentEntry = getFileAtOffset(currentOffset);
            if (currentEntry == null)
            {
                _br.BaseStream.Seek(65536, 0);
                currentOffset = 65536;
                currentEntry = getFileAtOffset(currentOffset);
                if (currentEntry == null)
                    throw new NullReferenceException("No START was found @Offset: 32768 or " + currentOffset);
            }

            // log
            // TextWriter tws = new StreamWriter("decompress.log");

            while ((currentOffset = _br.BaseStream.Position) < _length)
            {
                // get the FileEntry that starts at the offset
                currentEntry = getFileAtOffset(currentOffset);
                if (currentEntry == null)
                {
                    currentOffset = getSmallestUnfinishedOffset();
                    if (currentOffset < _br.BaseStream.Position || currentOffset == int.MaxValue)
                        break;
                }
                /*
                if (currentEntry == null)
                    throw new NullReferenceException("No File was found @Offset: 32768 or " + currentOffset);
                 * */

                skip = false;
                if (_texOnly)
                    if (currentEntry.getExt() != 0x241F5DEB)
                        skip = true;

                if (!skip)
                {
                    createPath(currentEntry.Path);
                    writeFile(currentEntry.FileName, currentEntry.Extension, currentEntry.SizeCompressed, currentEntry.Size);
                    currentEntry.Finished = true;
                    if (currentEntry.getExt() == 0x241F5DEB)
                        ddsConv.convertTexDDS(_outDir + @"\" + currentEntry.FileName + ".tex");
                }
                else
                {
                    _br.BaseStream.Position += currentEntry.SizeCompressed;
                    currentEntry.Finished = true;
                }
            }
        }

        private void writeFile(String fileName, String extension, int length, int size)
        {
            long endOffset = _br.BaseStream.Position + length;
            _br.BaseStream.ReadByte();
            _br.BaseStream.ReadByte();
            byte[] sBuffer = new byte[size];
            using (Stream dest = File.Create(_outDir + "\\" + fileName + "." + extension))
            {
                System.IO.Compression.DeflateStream ds = new System.IO.Compression.DeflateStream(_br.BaseStream, System.IO.Compression.CompressionMode.Decompress);
                ds.Read(sBuffer, 0, size);
                dest.Write(sBuffer, 0, size);
                dest.Close();
                _br.BaseStream.Position = endOffset;
            }
        }
        #endregion

        #region Utility Functions
        private int getSmallestUnfinishedOffset()
        {
            FileEntry current;
            int minOffset = int.MaxValue;

            for (int i = 0; i < arcEntries.Count; i++)
            {
                if (!(current = arcEntries[i]).Finished)
                {
                    if (current.OffsetBegin < minOffset)
                        minOffset = current.OffsetBegin;
                }
            }

            return minOffset;
        }

        private void createPath(String path)
        {
            String dir = Path.Combine(_outDir, path);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
                // System.Console.WriteLine("Created path: " + dir);
            }
            // else { System.Console.WriteLine("Path Existed! " + dir); }
        }

        private FileEntry getFileAtOffset(long offset)
        {
            for (int i = 0; i < arcEntries.Count; i++)
            {
                if (arcEntries[i].OffsetBegin == offset)
                    return arcEntries[i];
            }

            return null;
        }

        private void writeFilesToLog()
        {
            int count = 1;
            foreach (FileEntry e in arcEntries)
            {
                tw.WriteLine("# of File: " + count.ToString());
                tw.WriteLine("Name: " + e.FileName);
                tw.WriteLine("Name as HEX: " + e.FileNameAsHex);
                tw.WriteLine();
                tw.WriteLine("Extension: " + e.Extension);
                tw.WriteLine("Extension as HEX: " + e.ExtensionAsHex);
                tw.WriteLine();
                tw.WriteLine("CompressedSize: " + e.SizeCompressed);
                tw.WriteLine("CompressedSize as HEX: " + e.SizeCompressedAsHex);
                tw.WriteLine();
                tw.WriteLine("Size: " + e.Size);
                tw.WriteLine("Size as HEX: " + e.SizeAsHex);
                tw.WriteLine();
                tw.WriteLine("OffsetBegin: " + e.OffsetBegin);
                tw.WriteLine("OffsetBegin as HEX: " + e.OffsetBeginAsHex);
                tw.WriteLine("---------------------------------------------------------");
                tw.WriteLine();
                count++;
            }
        }

        public void log(String message, bool lineBreak)
        {
            if (lineBreak)
                tw.WriteLine(message);
            else
                tw.Write(message);
        }

        private void closeLog()
        {
            tw.Close();
        }

        public Arc Arc
        {
            get { return _inArc; }
        }

        public String ErrorMsg
        {
            get { return _errorMsg; }
        }
        #endregion
    }
}