using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ArcToolRevisited
{
    /// <summary>
    /// ArcEntry represents the abstraction of an entry in an arc file.
    /// </summary>
    public class ArcEntry
    {
        #region Members
        String _fileName;
        String _directory;
        String _fullPath;
        String _extension;
        String _nameInArc;
        int _cSize;
        int _size;
        int _ext;
        #endregion

        #region Constructor
        public ArcEntry(String fileName, String directory, String extension, int size, String fullPath)
        {
            FileName = fileName;
            Extension = extension;
            Size = size;
            FullPath = fullPath;
            Directory = directory;
        }
        #endregion

        #region Extension Handling
        public String Extension
        {
            get { return _extension; }
            set
            {
                _extension = value.Substring(1).ToLower();

                switch (_extension)
                {
                    case "nck":
                        _ext = (int)Helper.Ext.nck; break;

                    case "oba":
                        _ext = (int)Helper.Ext.oba; break;

                    case "ccl":
                        _ext = (int)Helper.Ext.ccl; break;

                    case "cdg":
                        _ext = (int)Helper.Ext.cdg; break;

                    case "bfx":
                        _ext = (int)Helper.Ext.bfx; break;

                    case "tex":
                        _ext = (int)Helper.Ext.tex; break;

                    case "lmt":
                        _ext = (int)Helper.Ext.lmt; break;

                    case "mod":
                        _ext = (int)Helper.Ext.mod; break;

                    case "vib":
                        _ext = (int)Helper.Ext.vib; break;

                    case "msg":
                        _ext = (int)Helper.Ext.msg; break;

                    case "lsp":
                        _ext = (int)Helper.Ext.lsp; break;

                    case "sdl":
                        _ext = (int)Helper.Ext.sdl; break;

                    case "stp":
                        _ext = (int)Helper.Ext.stp; break;

                    case "idm":
                        _ext = (int)Helper.Ext.idm; break;

                    case "mtg":
                        _ext = (int)Helper.Ext.mtg; break;

                    case "lcm":
                        _ext = (int)Helper.Ext.lcm; break;

                    case "jex":
                        _ext = (int)Helper.Ext.jex; break;

                    case "chn":
                        _ext = (int)Helper.Ext.chn; break;

                    case "ahc":
                        _ext = (int)Helper.Ext.ahc; break;

                    case "hit":
                        _ext = (int)Helper.Ext.hit; break;

                    case "rtex":
                        _ext = (int)Helper.Ext.rtex; break;

                    case "xfs":
                        _ext = (int)Helper.Ext.xfs; break;

                    case "ean":
                        _ext = (int)Helper.Ext.ean; break;

                    case "efl":
                        _ext = (int)Helper.Ext.efl; break;

                    case "efs":
                        _ext = (int)Helper.Ext.efs; break;

                    case "rtt":
                        _ext = (int)Helper.Ext.rtt; break;

                    case "aef":
                        _ext = (int)Helper.Ext.aef; break;

                    case "adh":
                        _ext = (int)Helper.Ext.adh; break;

                    case "cef":
                        _ext = (int)Helper.Ext.cef; break;

                    case "spc":
                        _ext = (int)Helper.Ext.spc; break;

                    case "srd":
                        _ext = (int)Helper.Ext.srd; break;

                    case "srq":
                        _ext = (int)Helper.Ext.srq; break;

                    case "stq":
                        _ext = (int)Helper.Ext.stq; break;

                    case "rev_win":
                        _ext = (int)Helper.Ext.rev_win; break;

                    case "equ":
                        _ext = (int)Helper.Ext.equ; break;

                    case "scs":
                        _ext = (int)Helper.Ext.scs; break;

                    case "sds":
                        _ext = (int)Helper.Ext.sds; break;

                    case "mse":
                        _ext = (int)Helper.Ext.mse; break;

                    case "ase":
                        _ext = (int)Helper.Ext.ase; break;

                    case "ogg":
                        _ext = (int)Helper.Ext.ogg; break;

                    case "sngw":
                        _ext = (int)Helper.Ext.ogg; break;

                    default:
                        _ext = Convert.ToInt32(_extension, 16); break;
                }
            }
        }
        #endregion

        #region Accessors
        public int CompressedSize
        {
            get { return _cSize; }
            set { _cSize = value; }
        }

        public String FileName
        {
            get { return _fileName; }
            set
            {
                _fileName = value;
                int index = _fileName.IndexOf(".");
                _fileName = _fileName.Substring(0, index);
            }
        }

        public String NameInArc
        {
            get { return _nameInArc; }
            set { _nameInArc = value; }
        }

        public String FullPath
        {
            get { return _fullPath; }
            set { _fullPath = value; }
        }

        public int Size
        {
            get { return _size; }
            set { _size = value; }
        }

        public String Directory
        {
            get { return _directory; }
            set { _directory = value; }
        }

        public int ExtensionAsInt
        {
            get { return _ext; }
        }

        public String ExtensionAsHex
        {
            get { return Helper.IntToHex(_ext); }
        }
        #endregion
    }
}