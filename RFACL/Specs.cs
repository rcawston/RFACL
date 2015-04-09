/*
RFACL
Copyright (c) Ross Cawston, All rights reserved.

This library is free software; you can redistribute it and/or
modify it under the terms of the GNU Lesser General Public
License as published by the Free Software Foundation; either
version 3.0 of the License, or (at your option) any later version.

This library is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public
License along with this library.
*/

using System.Linq;
using System.Security.AccessControl;

namespace RFACL
{
    public class Specs
    {
        public class UserGroupSpec
        {
            public string Name;
            public FileSystemRights FileSystemRight;
            public InheritanceFlags InheritanceFlag;
            public PropagationFlags PropagationFlag;
            public AccessControlType AccessControlType;
            public override string ToString()
            {
                return "[UserGroup] " + Name + ": FileSystemRights: " + FileSystemRight + "; InheritanceFlags: " + InheritanceFlag + "; PropagationFlag: " + PropagationFlag + "; AccessControlType: " + AccessControlType;
            }
        }

        public class PermissionSpec
        {
            public string Name;
            public bool ProtectFromInheritance;
            public bool PreserveInherited;
            public bool CleanExplicit;
            public UserGroupSpec[] UserGroups;
            public override string ToString()
            {
                string ret = UserGroups.Aggregate("", (current, userGroup) => current + ("\n" + userGroup));
                return "[Permission] " + Name + ": ProtectFromInheritance: " + ProtectFromInheritance +
                       "; PreserveInherited: " + PreserveInherited + "; CleanExplicit: " + CleanExplicit + ret;
            }
        }

        public class FolderSpec
        {
            public string Path;
            public int StarDepth = 0;
            public PermissionSpec Permission;
            public override string ToString()
            {
                if (Path.Contains("*"))
                {
                    return "[Folder] " + Path + ": (starDepth = " + StarDepth + ")" + "\n" + Permission;
                }
                return "[Folder] " + Path + "\n" + Permission;
            }
        }

        public class RecursionSpec
        {
            public int MaxScanDepth = -1;
        }

        public class ConfigSpec
        {
            public FolderSpec[] FolderSpecs;
            public RecursionSpec RecursionSpec;
        }
    }
}
