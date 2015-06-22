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
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Security.AccessControl;
using System.Xml.Serialization;

namespace RFACL
{
    public static class ConfigLoader
    {
        public static RFACLConfig LoadConfig(String configPath)
        {
            var serializer = new XmlSerializer(typeof(RFACLConfig));
            var reader = new FileStream(configPath, FileMode.Open);
            return (RFACLConfig) serializer.Deserialize(reader);
        }
    }

    [Serializable()]
    public class RFACLConfig
    {
        public RFACLConfig()
        {
            Permissions = new List<Permission>();
            Folders = new List<Folder>();
        }

        [XmlElement("Folder")]
        public List<Folder> Folders;
        [XmlElement("Permission")]
        public List<Permission> Permissions;
        [DefaultValue(typeof(int), "-1")]
        public int MaxScanDepth;
    }

    public class Permission
    {
        public string Name;
        public bool ProtectFromInheritance;
        public bool PreserveInherited;
        public bool CleanExplicit;
        [XmlElement("UserGroup")]
        public List<UserGroup> UserGroups;
    }

    public class UserGroup
    {
        public string Name;
        public FileSystemRights FileSystemRight;
        public InheritanceFlags InheritanceFlag;
        public PropagationFlags PropagationFlag;
        public AccessControlType AccessControlType;
    }

    public class Folder
    {
        public string Path;
        public string Permission;
        [DefaultValue(typeof(int), "-1")]
        public int StarDepth;
    }

}

