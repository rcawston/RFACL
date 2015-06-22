/*
RFACL
Copyright © 2015 Ross Cawston; All rights reserved.

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
using System.Linq;
using System.Security.AccessControl;
using System.Xml.Serialization;

namespace RFACL
{
    /// <summary>
    /// Loads configuration from XML file
    /// </summary>
    public static class ConfigLoader
    {
        /// <summary>
        /// Loads a given XML file
        /// </summary>
        /// <param name="configPath">path to XML file</param>
        /// <returns>RFACL configuration specs</returns>
        public static RFACLConfig LoadConfig(String configPath)
        {
            // Deserialize the XML file to a RFACLConfig object
            var serializer = new XmlSerializer(typeof(RFACLConfig));
            var reader = new FileStream(configPath, FileMode.Open);
            return (RFACLConfig) serializer.Deserialize(reader);
        }
    }

    /// <summary>
    /// Represents the XML configuration file schema
    /// </summary>
    [Serializable()]
    public class RFACLConfig
    {
        /// <summary>
        /// List of folder specs
        /// </summary>
        [XmlElement("Folder")]
        public List<Folder> Folders;

        /// <summary>
        /// List of permissions specs
        /// </summary>
        [XmlElement("Permission")]
        public List<Permission> Permissions;

        /// <summary>
        /// Global max scan depth
        /// </summary>
        [DefaultValue(typeof(int), "-1")]
        public int MaxScanDepth;

        /// <summary>
        /// Configuration constructor
        /// </summary>
        public RFACLConfig()
        {
            Permissions = new List<Permission>();
            Folders = new List<Folder>();
        }

        /// <summary>
        /// Returns the first permission spec with a given name
        /// </summary>
        /// <param name="name">permission spec name</param>
        /// <returns>permission spec</returns>
        public Permission GetMatchingPermission(string name)
        {
            return Permissions.FirstOrDefault(permission => name == permission.Name);
        }
    }

    /// <summary>
    /// A Permission node in the XML configuration
    /// </summary>
    public class Permission
    {
        /// <summary>
        /// Name of the permission spec
        /// </summary>
        public string Name;

        /// <summary>
        /// Protect ACL rules from inheritance
        /// </summary>
        public bool ProtectFromInheritance;

        /// <summary>
        /// Preserve inherited ACL rules
        /// </summary>
        public bool PreserveInherited;

        /// <summary>
        /// Clean existing explicit ACL rules
        /// </summary>
        public bool CleanExplicit;

        /// <summary>
        /// List of UserGroup specs
        /// </summary>
        [XmlElement("UserGroup")]
        public List<UserGroup> UserGroups;
    }

    /// <summary>
    /// A UserGroup node in the XML configuration
    /// </summary>
    public class UserGroup
    {
        /// <summary>
        /// User or Group for access control
        /// </summary>
        public string Name;

        /// <summary>
        /// access rights to use when creating the access control entry
        /// </summary>
        //[XmlElement("FileSystemRight")]
        public FileSystemRights FileSystemRight;

        /// <summary>
        /// the semantics of inheritance for this access control entry
        /// </summary>
        [XmlElement("InheritanceFlag")]
        public InheritanceFlags[] InheritanceFlags;

        /// <summary>
        /// Get the bitwise OR merged inheritance flags
        /// </summary>
        /// <returns>inheritance flags</returns>
        public InheritanceFlags GetInheritanceFlags()
        {
            return InheritanceFlags.Aggregate(new InheritanceFlags(), (current, iFlag) => current | iFlag);
        }

        /// <summary>
        /// specifies how the access control entry is propagated to child objects
        /// </summary>
        [XmlElement("PropagationFlag")]
        public PropagationFlags[] PropagationFlags;
        
        /// <summary>
        /// Get the bitwise OR merged propagation flags
        /// </summary>
        /// <returns>inheritance flags</returns>
        public PropagationFlags GetPropagationFlags()
        {
            return PropagationFlags.Aggregate(new PropagationFlags(), (current, pFlag) => current | pFlag);
        }

        /// <summary>
        /// Specifies whether the access control entry is used to allow or deny access
        /// </summary>
        public AccessControlType AccessControlType;
    }

    /// <summary>
    /// A Folder node in the XML configuration
    /// </summary>
    public class Folder
    {
        /// <summary>
        /// Path to match against found folders.  Can use * and ? for wildcards.
        /// </summary>
        public string Path;

        /// <summary>
        /// Permissions spec to apply to matching folders
        /// </summary>
        public string Permission;

        /// <summary>
        /// Maximum folder depth allowed for * wildcards. -1 means unlimited.
        /// </summary>
        [DefaultValue(typeof(int), "-1")]
        public int StarDepth;
    }

}

