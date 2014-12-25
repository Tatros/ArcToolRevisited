using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections;

namespace ArcToolRevisited
{
    public class TexConverter
    {
        #region Convert DDS to TEX
        public void convertDDSTex(String inDDS, String outFolder)
        {
            FileInfo fi = new FileInfo(inDDS);
            String outTex = "";

            if (outFolder != "")
            {
                outTex = outFolder + "\\" + Helper.nameWithoutExtension(fi.Name) + ".tex";
            }
            else
            {
                outTex = Helper.nameWithoutExtension(inDDS) + ".tex";
            }

            Console.WriteLine("Output Path: " + outTex);
            String headerFile = Helper.nameWithoutExtension(inDDS) + ".header";

            FileStream output = null;
            BinaryWriter texWriter = null;
            FileStream input = null;
            BinaryReader hReader = null;
            // Write Header
            try
            {
                output = new FileStream(outTex, FileMode.Create, FileAccess.ReadWrite);
                texWriter = new BinaryWriter(output);

                input = new FileStream(headerFile, FileMode.Open, FileAccess.Read);
                hReader = new BinaryReader(input);

                int inLen = (int)input.Length;
                byte[] hBuffer = new byte[inLen];

                hReader.Read(hBuffer, 0, inLen);
                texWriter.Write(hBuffer, 0, inLen);

                texWriter.Flush();
                input.Flush();
                input.Close();

                // Write Contents
                input = new FileStream(inDDS, FileMode.Open, FileAccess.Read);
                hReader = new BinaryReader(input);

                inLen = (int)input.Length - 128;
                hBuffer = new byte[inLen];
                hReader.BaseStream.Position = 128;

                hReader.Read(hBuffer, 0, inLen);
                texWriter.Write(hBuffer, 0, inLen);

                output.Flush();
                texWriter.Flush();

                short width = (short)getWidth(hReader);
                short height = (short)getHeight(hReader);

                texWriter.BaseStream.Position = 12;
                texWriter.Write(width);
                texWriter.BaseStream.Position = 14;
                texWriter.Write(height);

                output.Flush();
                texWriter.Flush();
                texWriter.Close();
                output.Close();

                input.Flush();
                hReader.Close();
                input.Close();
            }
            catch (FileNotFoundException)
            {
                throw new FileNotFoundException("Could not find: " + headerFile + " \n\nDo not delete or rename any .header or .dds files - they are needed for DDS to TEX conversion!\nTry unpacking the original archive again and either copy over the .header files from there or copy your current DDS files to the newly unpacked archive.\nThen remember to leave the *.header files alone.\n");
            }
            finally
            {
                if (texWriter != null)
                    texWriter.Close();
                if (output != null)
                {
                    output.Close();
                    output.Dispose();
                }
                if (hReader != null)
                    hReader.Close();
                if (input != null)
                {
                    input.Close();
                    input.Dispose();
                }
            }
        }
        #endregion

        #region Utility Functions
        private int getWidth(BinaryReader ddsReader)
        {
            long oldPos = ddsReader.BaseStream.Position;
            ddsReader.BaseStream.Position = 0x10;
            int width = ddsReader.ReadInt32();
            ddsReader.BaseStream.Position = oldPos;
            return width;
        }

        private int getHeight(BinaryReader ddsReader)
        {
            long oldPos = ddsReader.BaseStream.Position;
            ddsReader.BaseStream.Position = 0xC;
            int height = ddsReader.ReadInt32();
            ddsReader.BaseStream.Position = oldPos;
            return height;
        }

        private void log(String msg, bool lineBreak)
        {
            if (lineBreak)
                Console.WriteLine(msg);
            else
                Console.Write(msg);
        }
        #endregion
    }
}