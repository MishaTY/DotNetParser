﻿using LibDotNetParser;
using LibDotNetParser.DotNet.Tabels.Defs;
using LibDotNetParser.PE;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibDotNetParser.CILApi
{
    public class DotNetType
    {
        private PEFile file;
        private TypeDef type;
        private TypeFlags flags;
        private int NextTypeIndex;

        public string Name { get; private set; }
        public string NameSpace { get; private set; }
        public string FullName
        {
            get
            {
                if (string.IsNullOrEmpty(NameSpace))
                    return Name;
                else
                    return NameSpace + "." + Name;
            }
        }
        public bool IsPublic
        {
            get
            {
                return flags.HasFlag(TypeFlags.tdPublic);
            }
        }
        public bool IsInterface
        {
            get
            {
                return flags.HasFlag(TypeFlags.tdInterface);
            }
        }

        private List<DotNetMethod> methods = new List<DotNetMethod>();
        public List<DotNetMethod> Methods
        {
            get
            {
                return methods;
            }
        }

        private List<DotNetField> fields = new List<DotNetField>();
        public List<DotNetField> Fields
        {
            get
            {
                return fields;
            }
        }

        public DotNetFile File { get; internal set; }

        /// <summary>
        /// Should be used internaly
        /// </summary>
        /// <param name="file"></param>
        /// <param name="item"></param>
        /// <param name="NextTypeIndex"></param>
        public DotNetType(DotNetFile file, TypeDef item, int NextTypeIndex)
        {
            this.file = file.Backend;
            this.type = item;
            this.File = file;
            this.NextTypeIndex = NextTypeIndex;
            this.flags = (TypeFlags)item.Flags;

            Name = this.file.ClrStringsStream.GetByOffset(item.Name);
            NameSpace = this.file.ClrStringsStream.GetByOffset(item.Namespace);
            InitMethods();
            InitFields();
        }

        private void InitFields()
        {
            fields.Clear();
            uint startIndex = type.FieldList;

            int max;

            if (file.Tabels.FieldTabel.Count <= NextTypeIndex)
            {
                //Happens when this is the last type
                max = file.Tabels.FieldTabel.Count;
            }
            else
            {
                //Get the max value from the next type.
                max = (int)file.Tabels.TypeDefTabel[file.Tabels.FieldTabel.Count - 1].FieldList;
            }


            for (uint i = startIndex - 1; i < max; i++)
            {
                if ((startIndex - 1) == max)
                {
                    //No methods for this type, contiune
                    break;
                }
                if (file.Tabels.FieldTabel.Count != 0)
                {
                    var item = file.Tabels.FieldTabel[(int)i];
                    fields.Add(new DotNetField(file, item, this, (int)i+1));
                }
            }
        }

        private void InitMethods()
        {
            methods.Clear();
            uint startIndex = type.MethodList;

            int max;

            if (file.Tabels.TypeDefTabel.Count <= NextTypeIndex)
            {
                //Happens when this is the last type
                max = file.Tabels.MethodTabel.Count;
            }
            else
            {
                //Get the max value from the next type.
                max = (int)file.Tabels.TypeDefTabel[file.Tabels.TypeDefTabel.Count - 1].MethodList;
            }


            for (uint i = startIndex - 1; i < max; i++)
            {
                if ((startIndex - 1) == max)
                {
                    //No methods for this type, contiune
                    break;
                }
                if (file.Tabels.MethodTabel.Count != 1)
                {
                    var item = file.Tabels.MethodTabel[(int)i];
                    methods.Add(new DotNetMethod(file, item, this));
                }
            }
        }

        public override string ToString()
        {
            return FullName;
        }
    }
}
