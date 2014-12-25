using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ArcToolRevisited
{
    /// <summary>
    /// Provides several static utility functions
    /// </summary>
    public class Helper
    {
        #region Members
        // Directory of the Executable, set on startup.
        public static String homeDir = "";
        // Deprecated: used for Hashing based Conversion
        public static int hashSelect = 10;
        #endregion

        #region Logging
        public static TextWriter createLogFile(String name)
        {
            FileStream ls = new FileStream(homeDir + @"\log\" + name, FileMode.Create, FileAccess.ReadWrite);
            TextWriter tw = new StreamWriter(ls);
            return tw;
        }
        #endregion

        #region Encoding / Type Conversion
        public static String ASCIIStringToHex(String asciiString)
        {
            return BitConverter.ToString(Encoding.ASCII.GetBytes(asciiString));
        }

        public static String IntToHex(int x)
        {
            return Convert.ToString(x, 16);
        }

        public static String reverseHex(String hexString)
        {
            String revStr = "";
            while (hexString.Length >= 2)
            {
                revStr += hexString.Substring(hexString.Length - 2);
                hexString = hexString.Substring(0, hexString.Length - 2);
            }
            if (hexString != "")
                revStr += "0" + hexString;

            return revStr;
        }
        #endregion

        #region Array related
        public static int sumOverByteArray(byte[] barray)
        {
            int result = 0;
            for (int i = 0; i < barray.Length; i++)
            {
                result += barray[i];
            }
            return result * barray[3];
        }

        public static bool compareByteArray(byte[] x, byte[] y)
        {
            int xlength = x.Length;

            if (xlength != y.Length)
                return false;

            for (int i = 0; i < xlength; i++)
            {
                if (x[i] != y[i])
                    return false;
            }

            return true;
        }
        #endregion

        #region Texture related
        // Get compressed Image size of DDS file by calculating the size of each MipMap and building the sum over it.
        public static int getCImageSize(int width, int height, int blocksize, int nbMipMaps)
        {
            int size = 0;

            for (int i = 0; i < nbMipMaps; ++i)
            {
                int bufSize = ((width + 3) / 4) * ((height + 3) / 4) * blocksize;


                size += bufSize;
                if ((width /= 2) == 0) width = 1;
                if ((height /= 2) == 0) height = 1;
            }
            return size;
        }
        #endregion

        #region Deprecated: Hashing based Conversion
        public static int getKey(String ddsFile)
        {
            // HASH: ddsSize * DXTByte(LastfourCC) + width*height + sumOverByteArray(ddsName)
            FileInfo fi = new FileInfo(ddsFile);
            int key;
            // get width
            FileStream indds = new FileStream(fi.FullName, FileMode.Open, FileAccess.Read);
            BinaryReader br = new BinaryReader(indds);
            br.ReadInt32();
            br.ReadInt32();
            br.ReadInt32();
            int width = br.ReadInt32();
            int height = br.ReadInt32();
            br.BaseStream.Position = 87;
            int dxtVer = br.ReadByte();

            String name = fi.Name.ToLower();
            int x;
            if ((x = name.LastIndexOf("(")) != -1)
            {
                x--;
                name = name.Substring(0, x);
                name += ".dds";
                Console.WriteLine("XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX");
                Console.WriteLine("NAME " + fi.Name + " ENDED WITH ) : NEW NAME = " + name);
                Console.WriteLine("XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX");
            }
            key = name.GetHashCode();
            // byte[] asciiName = Encoding.ASCII.GetBytes(name);
            // key += Helper.sumOverByteArray(asciiName).GetHashCode();
            Console.WriteLine("HASH: " + key);

            indds.Flush();
            br.Close();
            indds.Close();

            return key;
        }
        #endregion

        #region String Functions (Extensions)
        public static String getCorrectName(String name)
        {
            int x;
            if ((x = name.LastIndexOf("(")) != -1)
            {
                String oldName = name;
                x--;
                name = name.Substring(0, x);
                name = name.Replace("-", "_");
                if (name.StartsWith("9"))
                    name = "a" + name;
                return name;
            }
            name = nameWithoutExtension(name);

            name = name.Replace("-", "");
            if (name.StartsWith("9"))
                name = "a" + name;
            return name;
        }

        public static String nameWithoutExtension(String fileName)
        {
            int index = fileName.IndexOf(".");

            if (index != -1)
                return fileName.Substring(0, index); ;

            return fileName;
        }
        #endregion

        #region Known Extensions
        public enum Ext
        {
            cdg = 0x2DC54131,
            ccl = 0x0026E7FF,
            oba = 0x0DADAB62,
            nck = 0x1BA81D3C,
            bfx = 0x50f9db3e,
            tex = 0x241f5deb,
            lmt = 0x76820D81,
            mod = 0x58A15856,
            vib = 0x358012E8,
            msg = 0x10C460E6,
            lsp = 0x60DD1B16,
            sdl = 0x4C0DB839,
            stp = 0x671F21DA,
            idm = 0x2447D742,
            mtg = 0x4E2FEF36,
            lcm = 0x39C52040,
            jex = 0x2282360D,
            chn = 0x3E363245,
            ahc = 0x5802B3FF,
            hit = 0x0253f147,
            rtex = 0x7808EA10,
            xfs = 0x4D894D5D,
            // effects
            ean = 0x4E397417,
            efl = 0x6D5AE854,
            efs = 0x02833703,
            rtt = 0x276DE8B7,
            aef = 0x557ECC08,
            adh = 0x1EFB1B67,
            cef = 0x758B2EB7,
            // sounds
            spc = 0x7E33A16C,
            srd = 0x2D12E086,
            srq = 0x1BCC4966,
            stq = 0x167DBBFF,
            rev_win = 0x232E228C,
            equ = 0x2B40AE8F,
            scs = 0x0ECD7DF4,
            sds = 0x0315E81F,
            mse = 0x4CA26828,
            ase = 0x07437CCE,
            ogg = 0x7D1530C2
        }
        #endregion
    }
}
