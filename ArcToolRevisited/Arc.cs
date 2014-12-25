using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ArcToolRevisited
{
    /// <summary>
    /// This class represents the abstraction of an .arc file.
    /// </summary>
    public class Arc
    {
        #region Members
        private String _name;
        private String _magic;
        private short _version;
        private short _filesCount;
        #endregion

        #region Constructor
        public Arc(String name)
        {
            Name = name;
            _magic = "";
            _version = -1;
            _filesCount = -1;
        }
        #endregion

        #region Accessors
        public String Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public short Version
        {
            get { return _version; }
            set { _version = value; }
        }

        public short FilesCount
        {
            get { return _filesCount; }
            set { _filesCount = value; }
        }

        public String Magic
        {
            get { return _magic; }
            set { _magic = value; }
        }
        #endregion
    }
}
