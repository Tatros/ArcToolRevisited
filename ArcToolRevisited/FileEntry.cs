using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ArcToolRevisited
{
    /// <summary>
    /// FileEntry represents a file on disk that should be packed as an ArcEntry into an RE5 .arc 
    /// </summary>
    class FileEntry
    {
        #region Members
        private String _fileName;
        private String _extension;
        private String _path;
        private int _ext;
        private int _cSize;
        private int _size;
        private int _offsetBegin;
        private bool _finished;
        #endregion

        #region Constructor
        public FileEntry(String fileName, int extension, int sizeCompressed, int size, int offsetBegin)
        {
            FileName = fileName;
            this.setExtension(extension);
            SizeCompressed = sizeCompressed;
            Size = size;
            OffsetBegin = offsetBegin;
            Finished = false;
        }
        #endregion

        #region Extension Handling
        public void setExtension(int x)
        {
            _ext = x;
            switch (_ext)
            {
                case (int)Helper.Ext.nck:
                    _extension = "nck"; break;

                case (int)Helper.Ext.oba:
                    _extension = "oba"; break;

                case (int)Helper.Ext.ccl:
                    _extension = "ccl"; break;

                case (int)Helper.Ext.cdg:
                    _extension = "cdg"; break;

                case (int)Helper.Ext.bfx:
                    _extension = "bfx"; break;

                case (int)Helper.Ext.tex:
                    _extension = "tex"; break;

                case (int)Helper.Ext.lmt:
                    _extension = "lmt"; break;

                case (int)Helper.Ext.mod:
                    _extension = "mod"; break;

                case (int)Helper.Ext.vib:
                    _extension = "vib"; break;

                case (int)Helper.Ext.msg:
                    _extension = "msg"; break;

                case (int)Helper.Ext.lsp:
                    _extension = "lsp"; break;

                case (int)Helper.Ext.sdl:
                    _extension = "sdl"; break;

                case (int)Helper.Ext.stp:
                    _extension = "stp"; break;

                case (int)Helper.Ext.idm:
                    _extension = "idm"; break;

                case (int)Helper.Ext.mtg:
                    _extension = "mtg"; break;

                case (int)Helper.Ext.lcm:
                    _extension = "lcm"; break;

                case (int)Helper.Ext.jex:
                    _extension = "jex"; break;

                case (int)Helper.Ext.chn:
                    _extension = "chn"; break;

                case (int)Helper.Ext.ahc:
                    _extension = "ahc"; break;

                case (int)Helper.Ext.hit:
                    _extension = "hit"; break;

                case (int)Helper.Ext.rtex:
                    _extension = "rtex"; break;

                case (int)Helper.Ext.xfs:
                    _extension = "xfs"; break;

                case (int)Helper.Ext.ean:
                    _extension = "ean"; break;

                case (int)Helper.Ext.efl:
                    _extension = "efl"; break;

                case (int)Helper.Ext.efs:
                    _extension = "efs"; break;

                case (int)Helper.Ext.rtt:
                    _extension = "rtt"; break;

                case (int)Helper.Ext.aef:
                    _extension = "aef"; break;

                case (int)Helper.Ext.adh:
                    _extension = "adh"; break;

                case (int)Helper.Ext.cef:
                    _extension = "cef"; break;

                case (int)Helper.Ext.spc:
                    _extension = "spc"; break;

                case (int)Helper.Ext.srd:
                    _extension = "srd"; break;

                case (int)Helper.Ext.srq:
                    _extension = "srq"; break;

                case (int)Helper.Ext.stq:
                    _extension = "stq"; break;

                case (int)Helper.Ext.rev_win:
                    _extension = "rev_win"; break;

                case (int)Helper.Ext.equ:
                    _extension = "equ"; break;

                case (int)Helper.Ext.scs:
                    _extension = "scs"; break;

                case (int)Helper.Ext.sds:
                    _extension = "sds"; break;

                case (int)Helper.Ext.mse:
                    _extension = "mse"; break;

                case (int)Helper.Ext.ase:
                    _extension = "ase"; break;

                case (int)Helper.Ext.ogg:
                    _extension = "ogg"; break;

                default:
                    _extension = ExtensionAsHex; break;
            }
        }
        #endregion

        #region Accessors
        public bool Finished
        {
            get { return _finished; }
            set { _finished = value; }
        }

        public String FileName
        {
            get { return _fileName; }
            set
            {
                _fileName = value.Trim(Convert.ToChar((byte)0));
                int endPath = _fileName.LastIndexOf("\\");
                _path = _fileName.Substring(0, endPath);
            }
        }

        public String Path
        {
            get { return _path; }
        }

        public String FileNameAsHex
        {
            get { return Helper.ASCIIStringToHex(FileName); }
        }


        public int getExt()
        {
            return _ext;
        }

        public String Extension
        {
            get { return _extension; }
        }

        public String ExtensionAsHex
        {
            get { return Helper.IntToHex(_ext); }
        }

        public int SizeCompressed
        {
            get { return _cSize; }
            set { _cSize = value; }
        }

        public String SizeCompressedAsHex
        {
            get { return Helper.IntToHex(_cSize); }
        }

        public int Size
        {
            get { return _size; }
            set { _size = value; }
        }

        public String SizeAsHex
        {
            get { return Helper.IntToHex(_size); }
        }

        public int OffsetBegin
        {
            get { return _offsetBegin; }
            set { _offsetBegin = value; }
        }

        public String OffsetBeginAsHex
        {
            get { return Helper.IntToHex(_offsetBegin); }
        }
        #endregion
    }
}
