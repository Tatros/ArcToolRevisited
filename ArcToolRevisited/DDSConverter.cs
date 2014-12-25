using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.IO;
using System.Windows.Forms;

namespace ArcToolRevisited
{
    /// <summary>
    /// Converts TEX to DDS
    /// </summary>
    public class DDSConverter
    {
        #region Members
        private TextWriter logWriter = null;
        private FileStream ddsStream = null;
        private FileStream texStream = null;
        private BinaryReader texReader = null;
        private BinaryWriter ddsWriter = null;
        #endregion

        #region Constructor
        public DDSConverter()
        {
        }
        #endregion

        #region Conversion Types
        public void convertTexDDS(String inTex, String outFolder)
        {
            FileInfo fi = new FileInfo(inTex);
            convert(inTex, outFolder + @"\" + Helper.nameWithoutExtension(fi.Name) + ".dds");
        }

        public void convertTexDDS(String inTex)
        {
            convert(inTex, getDDSName(inTex));
        }
        #endregion

        #region Main Conversion Method
        private void convert(String inTex, String outDDS)
        {
            try
            {
                FileInfo texInfo = new FileInfo(inTex);
                logWriter = Helper.createLogFile("TexDDS.log");

                // get DDS Name
                String ddsName = outDDS;
                // init streams
                ddsStream = new FileStream(ddsName, FileMode.Create, FileAccess.ReadWrite);
                ddsWriter = new BinaryWriter(ddsStream);

                texStream = new FileStream(inTex, FileMode.Open, FileAccess.Read);
                texReader = new BinaryReader(texStream);

                // get header
                log("converting to DDS: " + texInfo.Name, true);
                writeDDSHeader();
                log("-- wrote DDS Header.", true);
                int texHeaderLength = writeContent();
                log("-- wrote DDS Contents.", true);
                writeHeaderFile(texHeaderLength, texInfo.FullName, outDDS);
                log("-- wrote .header file.", true);
                log("", true);

                texReader.Close();
                ddsWriter.Close();
                logWriter.Flush();
                logWriter.Close();
                texStream.Close();
                ddsStream.Close();
            }

            catch (Exception)
            {
                throw;
            }

            finally
            {
                if (texReader != null)
                    texReader.Close();
                if (ddsWriter != null)
                    ddsWriter.Close();
                if (logWriter != null)
                    logWriter.Close();
                if (texStream != null)
                    texStream.Dispose();
                if (ddsStream != null)
                    ddsStream.Dispose();
            }
        }
        #endregion

        #region Write Content
        private int writeContent()
        {
            int fileSize = (int)texStream.Length;
            log("--- FileSize: " + fileSize + " = 0x" + fileSize.ToString("X"), true);
            int width = getWidth(texReader);
            log("--- width: " + width + " = 0x" + width.ToString("X"), true);
            int height = getHeight(texReader);
            log("--- height: " + height + " = 0x" + height.ToString("X"), true);
            int nbMipMaps = getMipMapCount(texReader);
            log("--- nbMipMaps: " + nbMipMaps, false);
            log(" 0x" + nbMipMaps.ToString("X"), true);
            int dxtVer = getDxtVer(texReader);
            if (dxtVer == 827611204)
                log("--- dxtVer: DXT1 = " + dxtVer + " = 0x" + dxtVer.ToString("X"), true);
            else if (dxtVer == 894720068)
                log("--- dxtVer: DXT5 = " + dxtVer + " = 0x" + dxtVer.ToString("X"), true);
            else
                log("--- dxtVer: unknown = " + dxtVer + " = 0x" + dxtVer.ToString("X"), true);
            int blockSize;

            if (dxtVer == 827611204) // DXT1 827611204
                blockSize = 8;
            else // DXT3/5: 894720068
                blockSize = 16;
            log("--- blockSize: " + blockSize, true);

            int cImageSize = Helper.getCImageSize(width, height, blockSize, nbMipMaps);
            log("--- cImageSize: " + cImageSize + " = 0x" + cImageSize.ToString("X"), true);
            int texHeaderLength = fileSize - cImageSize;
            log("--- texHeaderLength: " + texHeaderLength + " = 0x" + texHeaderLength.ToString("X"), true);

            ddsWriter.BaseStream.Position = 128;
            texReader.BaseStream.Position = texHeaderLength;

            byte[] texture = new byte[cImageSize];
            texReader.Read(texture, 0, cImageSize);
            texReader.BaseStream.Flush();

            ddsWriter.Write(texture, 0, cImageSize);
            ddsStream.Flush();
            ddsWriter.Flush();

            return texHeaderLength;
        }
        #endregion

        #region Write Header File (contains TEX Header)
        private void writeHeaderFile(int headerLength, String texFile, String outDDS)
        {
            String hFileName = Helper.nameWithoutExtension(outDDS) + ".header";
            FileStream fs = new FileStream(hFileName, FileMode.Create, FileAccess.ReadWrite);
            BinaryWriter bw = new BinaryWriter(fs);

            byte[] header = new byte[headerLength];
            texReader.BaseStream.Position = 0;
            texReader.Read(header, 0, headerLength);
            bw.Write(header, 0, headerLength);
            bw.Flush();
            bw.Close();
            fs.Close();
        }

        /* * HASHING BASED. Buggy (as hell).
        private int getTexHeaderLength(String correctTexName)
        {
            int hash = correctTexName.GetHashCode();
            List<String> cHeaderList = texData.getValue(hash);

            for (int i = 0; i < cHeaderList.Count; i++)
            {
                byte[] cHeader = Convert.FromBase64String(cHeaderList[i]);
                int cHeaderLength = (int)cHeader.Length;
                byte[] buffer = new byte[cHeaderLength];
                texReader.BaseStream.Position = 0;
                texReader.Read(buffer, 0, cHeaderLength);
                if (Helper.compareByteArray(buffer, cHeader))
                    return cHeaderLength;
            }

            log("Did not find header Length. There were " + cHeaderList.Count + " Headers.", true);
            log("- No Header Match found returning generic size 0x40 (64 bytes)", true);
            for (int i = 0; i < cHeaderList.Count; i++)
                log("-- Header " + i + ": " + cHeaderList[i], true);

            return 0x40;
        }
        */
        #endregion

        #region Write DDS Header
        private void writeDDSHeader()
        {
            // DXT1: 827611204
            // DXT5: 894720068
            int dxtVer = getDxtVer(texReader);
            int width = getWidth(texReader);
            int height = getHeight(texReader);
            int mipMapCount = getMipMapCount(texReader);
            int linearSize = 0;

            byte[] dxtHeader = new byte[128];
            if (dxtVer == 827611204)
            {
                // DXT1
                dxtHeader = Convert.FromBase64String("RERTIHwAAAAHEAAAAAIAAAACAAAAAAAAAAAAAAoAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAACAAAAAEAAAARFhUMQAAAAAAAAAAAAAAAAAAAAAAAAAAABAAAAAAAAAAAAAAAAAAAAAAAAA=");
            }
            else if (dxtVer == 894720068)
            {
                // DXT5
                dxtHeader = Convert.FromBase64String("RERTIHwAAAAHEAAAgAAAAAABAAAAAAAAAAAAAAkAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAACAAAAAEAAAARFhUNQAAAAAAAAAAAAAAAAAAAAAAAAAAABAAAAAAAAAAAAAAAAAAAAAAAAA=");
            }
            else
                log("DXT Version " + dxtVer + " unrecognized", true);

            // write generic header
            ddsWriter.Write(dxtHeader, 0, 128);
            // write height and width
            ddsWriter.BaseStream.Position = 12;
            ddsWriter.Write(height);
            ddsWriter.Write(width);
            ddsWriter.Write(linearSize);
            ddsWriter.Write(0);
            ddsWriter.Write(mipMapCount);
            // write dxt version
            ddsWriter.BaseStream.Position = 84;
            ddsWriter.Write(dxtVer);

            // Flush
            texStream.Flush();
            ddsStream.Flush();
            ddsWriter.Flush();
        }
        #endregion

        #region Utility Functions
        private int getNBMipMaps(BinaryReader texReader)
        {
            long oldPos = texReader.BaseStream.Position;
            texReader.BaseStream.Position = 8;
            int nbMipMaps = texReader.ReadByte();
            texReader.BaseStream.Position = oldPos;
            return nbMipMaps;
        }

        private int getDxtVer(BinaryReader texReader)
        {
            long oldPos = texReader.BaseStream.Position;
            texReader.BaseStream.Position = 20;
            int dxtVer = texReader.ReadInt32();
            texReader.BaseStream.Position = oldPos;
            return dxtVer;
        }

        public int getWidth(BinaryReader texReader)
        {
            long oldPos = texReader.BaseStream.Position;
            texReader.BaseStream.Position = 12;
            int width = texReader.ReadInt16();
            texReader.BaseStream.Position = oldPos;
            return width;
        }

        public int getHeight(BinaryReader texReader)
        {
            long oldPos = texReader.BaseStream.Position;
            texReader.BaseStream.Position = 14;
            int height = texReader.ReadInt16();
            texReader.BaseStream.Position = oldPos;
            return height;
        }

        public int getMipMapCount(BinaryReader texReader)
        {
            long oldPos = texReader.BaseStream.Position;
            texReader.BaseStream.Position = 8;
            int mipMapCount = (int)texReader.ReadByte();
            texReader.BaseStream.Position = oldPos;
            return mipMapCount;
        }

        private String getDDSName(String texName)
        {
            return Helper.nameWithoutExtension(texName) + ".dds";
        }

        private void log(String message, bool lineBreak)
        {
            if (lineBreak)
            {
                Console.WriteLine(message);
                logWriter.WriteLine(message);
            }
            else
            {
                Console.Write(message);
                logWriter.Write(message);
            }
        }
        #endregion
    }
}
