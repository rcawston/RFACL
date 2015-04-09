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
using System.Linq;
using System.Security.AccessControl;
using System.Xml;

namespace RFACL
{
    class ConfigLoader
    {
        public static Specs.ConfigSpec LoadConfig(String configPath)
        {
            List<Specs.PermissionSpec> permissionSpecs = new List<Specs.PermissionSpec>();
            List<Specs.UserGroupSpec> userGroupSpecs = new List<Specs.UserGroupSpec>();
            List<Specs.FolderSpec> folderSpecs = new List<Specs.FolderSpec>();

            XmlTextReader reader = new XmlTextReader(configPath);

            /* elements with sub-elements */
            bool inRoot = false;
            bool inPermissionsSpecs = false;
            bool inPermissionsSpec = false;
            bool inUserGroup = false;
            bool inFolderSpecs = false;
            bool inFolder = false;
            /* text elements */
            bool inMaxScanDepth = false;
            bool inName = false;
            /* text elements in PermissionSpec */
            bool inProtectFromInheritance = false;
            bool inPreserveInherited = false;
            bool inCleanExplicit = false;
            /* text elements in PermissionSpec\UserGroup */
            bool inFileSystemRight = false;
            bool inInheritanceFlag = false;
            bool inPropagationFlag = false;
            bool inAccessControlType = false;
            /* text elements in FolderSpecs\Folder */
            bool inPath = false;
            bool inPermission = false;
            bool inStarDepth = false;

            Specs.ConfigSpec configSpec = new Specs.ConfigSpec();
            Specs.FolderSpec currFolderSpec = null;
            Specs.PermissionSpec currPermissionSpec = null;
            Specs.UserGroupSpec currUserGroupSpec = null;

            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element: // The node is an element.
                        switch (reader.Name)
                        {
                            case "RFACL_spec":
                                inRoot = true;
                                break;
                            case "PermissionsSpecs":
                                if (!inRoot)
                                    throw new Exception("XML Parse Error: PermissionsSpecs element detected outside of RFACL_spec.");
                                inPermissionsSpecs = true;
                                break;
                            case "PermissionSpec":
                                if (!inRoot || !inPermissionsSpecs)
                                    throw new Exception(@"XML Parse Error: PermissionsSpec element detected outside of RFACL_spec\PermissionsSpecs.");
                                inPermissionsSpec = true;
                                currPermissionSpec = new Specs.PermissionSpec();
                                break;
                            case "UserGroup":
                                if (!inRoot || !inPermissionsSpecs || !inPermissionsSpec)
                                    throw new Exception(@"XML Parse Error: UserGroup element detected outside of RFACL_spec\PermissionsSpecs\PermissionSpec.");
                                inUserGroup = true;
                                currUserGroupSpec = new Specs.UserGroupSpec();
                                break;
                            case "MaxScanDepth":
                                if (!inRoot)
                                    throw new Exception(@"XML Parse Error: MaxScanDepth element detected outside of RFACL_spec.");
                                inMaxScanDepth = true;
                                break;
                            case "Name":
                                if (!inRoot || !inPermissionsSpecs || !inPermissionsSpec)
                                    throw new Exception(@"XML Parse Error: Name element detected outside of RFACL_spec\PermissionSpec.");
                                inName = true;
                                break;
                            case "ProtectFromInheritance":
                                if (!inRoot || !inPermissionsSpecs || !inPermissionsSpec)
                                    throw new Exception(@"XML Parse Error: ProtectFromInheritance element detected outside of RFACL_spec\PermissionSpecs\PermissionSpec.");
                                inProtectFromInheritance = true;
                                break;
                            case "PreserveInherited":
                                if (!inRoot || !inPermissionsSpecs || !inPermissionsSpec)
                                    throw new Exception(@"XML Parse Error: PreserveInherited element detected outside of RFACL_spec\PermissionSpecs\PermissionSpec.");
                                inPreserveInherited = true;
                                break;
                            case "CleanExplicit":
                                if (!inRoot || !inPermissionsSpecs || !inPermissionsSpec)
                                    throw new Exception(@"XML Parse Error: CleanExplicit element detected outside of RFACL_spec\PermissionSpecs\PermissionSpec.");
                                inCleanExplicit = true;
                                break;
                            case "FileSystemRight":
                                if (!inRoot || !inPermissionsSpecs || !inPermissionsSpec || !inUserGroup)
                                    throw new Exception(@"XML Parse Error: CleanExplicit element detected outside of RFACL_spec\PermissionSpecs\PermissionSpec\UserGroup.");
                                inFileSystemRight = true;
                                break;
                            case "InheritanceFlag":
                                if (!inRoot || !inPermissionsSpecs || !inPermissionsSpec || !inUserGroup)
                                    throw new Exception(@"XML Parse Error: InheritanceFlag element detected outside of RFACL_spec\PermissionSpecs\PermissionSpec\UserGroup.");
                                inInheritanceFlag = true;
                                break;
                            case "PropagationFlag":
                                if (!inRoot || !inPermissionsSpecs || !inPermissionsSpec || !inUserGroup)
                                    throw new Exception(@"XML Parse Error: PropagationFlag element detected outside of RFACL_spec\PermissionSpecs\PermissionSpec\UserGroup.");
                                inPropagationFlag = true;
                                break;
                            case "AccessControlType":
                                if (!inRoot || !inPermissionsSpecs || !inPermissionsSpec || !inUserGroup)
                                    throw new Exception(@"XML Parse Error: AccessControlType element detected outside of RFACL_spec\PermissionSpecs\PermissionSpec\UserGroup.");
                                inAccessControlType = true;
                                break;
                            case "FolderSpecs":
                                if (!inRoot)
                                    throw new Exception("XML Parse Error: FolderSpecs element detected outside of RFACL_spec.");
                                inFolderSpecs = true;
                                break;
                            case "Folder":
                                if (!inRoot || !inFolderSpecs)
                                    throw new Exception(@"XML Parse Error: Folder element detected outside of RFACL_spec\FolderSpecs.");
                                inFolder = true;
                                currFolderSpec = new Specs.FolderSpec();
                                break;
                            case "Path":
                                if (!inRoot || !inFolderSpecs || !inFolder)
                                    throw new Exception(@"XML Parse Error: Path element detected outside of RFACL_spec\FolderSpecs\Folder.");
                                inPath = true;
                                break;
                            case "Permission":
                                if (!inRoot || !inFolderSpecs || !inFolder)
                                    throw new Exception(@"XML Parse Error: Permission element detected outside of RFACL_spec\FolderSpecs\Folder.");
                                inPermission = true;
                                break;
                            case "StarDepth":
                                if (!inRoot || !inFolderSpecs || !inFolder)
                                    throw new Exception(@"XML Parse Error: StarDepth element detected outside of RFACL_spec\FolderSpecs\Folder.");
                                inStarDepth = true;
                                break;
                            default:
                                throw new Exception("XML Parse Error: unrecognized element detected. " + reader.Name + " is not a known element type.");
                        }
                        break;
                    case XmlNodeType.Text: //Display the text in each element.
                        if (inName)
                        {
                            if (inUserGroup)
                                currUserGroupSpec.Name = reader.Value;
                            else
                                currPermissionSpec.Name = reader.Value;
                        }
                        else if (inMaxScanDepth)
                        {   
                            if (reader.Value != "-1" && !reader.Value.All(Char.IsDigit))
                            {
                                throw new Exception("XML Parse Error: MaxScanDepth must be -1 or a positive integer ('" + reader.Value + "' specified)");
                            }
                            configSpec.RecursionSpec.MaxScanDepth = Int32.Parse(reader.Value);
                        }
                        else if (inProtectFromInheritance)
                        {
                            switch (reader.Value)
                            {
                                case "true":
                                case "1":
                                    currPermissionSpec.ProtectFromInheritance = true;
                                    break;
                                case "false":
                                case "0":
                                    currPermissionSpec.ProtectFromInheritance = false;
                                    break;
                                default:
                                    throw new Exception("XML Parse Error: ProtectFromInheritance must be either true, 1, false, or 0");
                            }
                        }
                        else if (inPreserveInherited)
                        {
                            switch (reader.Value)
                            {
                                case "true":
                                case "1":
                                    currPermissionSpec.PreserveInherited = true;
                                    break;
                                case "false":
                                case "0":
                                    currPermissionSpec.PreserveInherited = false;
                                    break;
                                default:
                                    throw new Exception("XML Parse Error: PreserveInherited must be either true, 1, false, or 0");
                            }
                        }
                        else if (inCleanExplicit)
                        {
                            switch (reader.Value)
                            {
                                case "true":
                                case "1":
                                    currPermissionSpec.CleanExplicit = true;
                                    break;
                                case "false":
                                case "0":
                                    currPermissionSpec.CleanExplicit = false;
                                    break;
                                default:
                                    throw new Exception("XML Parse Error: PreserveInherited must be either true, 1, false, or 0");
                            }
                        }
                        else if (inFileSystemRight)
                        {
                            switch (reader.Value)
                            {
                                case "AppendData":
                                    currUserGroupSpec.FileSystemRight |= FileSystemRights.AppendData;
                                    break;
                                case "ChangePermissions":
                                    currUserGroupSpec.FileSystemRight |= FileSystemRights.ChangePermissions;
                                    break;
                                case "CreateDirectories":
                                    currUserGroupSpec.FileSystemRight |= FileSystemRights.CreateDirectories;
                                    break;
                                case "CreateFiles":
                                    currUserGroupSpec.FileSystemRight |= FileSystemRights.CreateFiles;
                                    break;
                                case "Delete":
                                    currUserGroupSpec.FileSystemRight |= FileSystemRights.Delete;
                                    break;
                                case "DeleteSubdirectoriesAndFiles":
                                    currUserGroupSpec.FileSystemRight |= FileSystemRights.DeleteSubdirectoriesAndFiles;
                                    break;
                                case "ExecuteFile":
                                    currUserGroupSpec.FileSystemRight |= FileSystemRights.ExecuteFile;
                                    break;
                                case "FullControl":
                                    currUserGroupSpec.FileSystemRight |= FileSystemRights.FullControl;
                                    break;
                                case "ListDirectory":
                                    currUserGroupSpec.FileSystemRight |= FileSystemRights.ListDirectory;
                                    break;
                                case "Modify":
                                    currUserGroupSpec.FileSystemRight |= FileSystemRights.Modify;
                                    break;
                                case "Read":
                                    currUserGroupSpec.FileSystemRight |= FileSystemRights.Read;
                                    break;
                                case "ReadAndExecute":
                                    currUserGroupSpec.FileSystemRight |= FileSystemRights.ReadAndExecute;
                                    break;
                                case "ReadAttributes":
                                    currUserGroupSpec.FileSystemRight |= FileSystemRights.ReadAttributes;
                                    break;
                                case "ReadData":
                                    currUserGroupSpec.FileSystemRight |= FileSystemRights.ReadData;
                                    break;
                                case "ReadExtendedAttributes":
                                    currUserGroupSpec.FileSystemRight |= FileSystemRights.ReadExtendedAttributes;
                                    break;
                                case "ReadPermissions":
                                    currUserGroupSpec.FileSystemRight |= FileSystemRights.ReadPermissions;
                                    break;
                                case "Synchronize":
                                    currUserGroupSpec.FileSystemRight |= FileSystemRights.Synchronize;
                                    break;
                                case "TakeOwnership":
                                    currUserGroupSpec.FileSystemRight |= FileSystemRights.TakeOwnership;
                                    break;
                                case "Traverse":
                                    currUserGroupSpec.FileSystemRight |= FileSystemRights.Traverse;
                                    break;
                                case "Write":
                                    currUserGroupSpec.FileSystemRight |= FileSystemRights.Write;
                                    break;
                                case "WriteAttributes":
                                    currUserGroupSpec.FileSystemRight |= FileSystemRights.WriteAttributes;
                                    break;
                                case "WriteData":
                                    currUserGroupSpec.FileSystemRight |= FileSystemRights.WriteData;
                                    break;
                                case "WriteExtendedAttributes":
                                    currUserGroupSpec.FileSystemRight |= FileSystemRights.WriteExtendedAttributes;
                                    break;
                                default:
                                    throw new Exception("XML Parse Error: FileSystemRight is not a valid FileSystemRight type.");
                            }
                        }
                        else if (inInheritanceFlag)
                        {
                            switch (reader.Value)
                            {
                                case "None":
                                    currUserGroupSpec.InheritanceFlag |= InheritanceFlags.None;
                                    break;
                                case "ContainerInherit":
                                    currUserGroupSpec.InheritanceFlag |= InheritanceFlags.ContainerInherit;
                                    break;
                                case "ObjectInherit":
                                    currUserGroupSpec.InheritanceFlag |= InheritanceFlags.ObjectInherit;
                                    break;
                            }
                        }
                        else if (inPropagationFlag)
                        {
                            switch (reader.Value)
                            {
                                case "None":
                                    currUserGroupSpec.PropagationFlag |= PropagationFlags.None;
                                    break;
                                case "InheritOnly":
                                    currUserGroupSpec.PropagationFlag |= PropagationFlags.InheritOnly;
                                    break;
                                case "NoPropagateInherit":
                                    currUserGroupSpec.PropagationFlag |= PropagationFlags.NoPropagateInherit;
                                    break;
                            }
                        }
                        else if (inAccessControlType)
                        {
                            switch (reader.Value)
                            {
                                case "Allow":
                                    currUserGroupSpec.AccessControlType = AccessControlType.Allow;
                                    break;
                                case "Deny":
                                    currUserGroupSpec.AccessControlType = AccessControlType.Deny;
                                    break;
                            }
                        }
                        else if (inPath)
                        {
                            currFolderSpec.Path = reader.Value;
                        }
                        else if (inPermission)
                        {
                            bool found = false;
                            foreach (Specs.PermissionSpec spec in permissionSpecs.Where(spec => spec.Name == reader.Value))
                            {
                                currFolderSpec.Permission = spec;
                                found = true;
                                break;
                            }
                            if (!found)
                                throw new Exception("XML Parse Error: PermissionSpec named '" + reader.Value + "' was not found!");
                        }
                        else if (inStarDepth)
                        {
                            if (reader.Value != "-1" && !reader.Value.All(Char.IsDigit))
                            {
                                throw new Exception("XML Parse Error: StartDepth must be -1 or a positive integer ('" + reader.Value + "' specified)");
                            }
                            currFolderSpec.StarDepth = Int32.Parse(reader.Value);
                        }
                        else
                        {
                            throw new Exception("XML Parse Error: text detected in non-text element.");
                        }
                        break;
                    case XmlNodeType.EndElement: //Display the end of the element.
                        switch (reader.Name)
                        {
                            case "RFACL_spec":
                                inRoot = false;
                                break;
                            case "PermissionsSpecs":
                                if (!inPermissionsSpecs)
                                    throw new Exception("XML Parse Error: PermissionsSpecs end element detected but was not expected.");
                                inPermissionsSpecs = false;
                                break;
                            case "PermissionSpec":
                                if (!inPermissionsSpec)
                                    throw new Exception(@"XML Parse Error: PermissionsSpec end element detected but was not expected.");
                                inPermissionsSpec = false;
                                currPermissionSpec.UserGroups = userGroupSpecs.ToArray();
                                permissionSpecs.Add(currPermissionSpec);
                                userGroupSpecs = new List<Specs.UserGroupSpec>();
                                break;
                            case "UserGroup":
                                if (!inUserGroup)
                                    throw new Exception(@"XML Parse Error: UserGroup end element detected but was not expected.");
                                inUserGroup = false;
                                userGroupSpecs.Add(currUserGroupSpec);
                                break;
                            case "Name":
                                if (!inName)
                                    throw new Exception(@"XML Parse Error: Name end element detected but was not expected.");
                                inName = false;
                                break;
                            case "MaxScanDepth":
                                if (!inMaxScanDepth)
                                    throw new Exception(@"XML Parse Error: MaxScanDepth end element detected but was not expected.");
                                inMaxScanDepth = false;
                                break;
                            case "ProtectFromInheritance":
                                if (!inProtectFromInheritance)
                                    throw new Exception(@"XML Parse Error: ProtectFromInheritance end element detected but was not expected.");
                                inProtectFromInheritance = false;
                                break;
                            case "PreserveInherited":
                                if (!inPreserveInherited)
                                    throw new Exception(@"XML Parse Error: PreserveInherited end element detected but was not expected.");
                                inPreserveInherited = false;
                                break;
                            case "CleanExplicit":
                                if (!inCleanExplicit)
                                    throw new Exception(@"XML Parse Error: CleanExplicit end element detected but was not expected.");
                                inCleanExplicit = false;
                                break;
                            case "FileSystemRight":
                                if (!inFileSystemRight)
                                    throw new Exception(@"XML Parse Error: CleanExplicit end element detected but was not expected.");
                                inFileSystemRight = false;
                                break;
                            case "InheritanceFlag":
                                if (!inInheritanceFlag)
                                    throw new Exception(@"XML Parse Error: InheritanceFlag end element detected but was not expected.");
                                inInheritanceFlag = false;
                                break;
                            case "PropagationFlag":
                                if (!inPropagationFlag)
                                    throw new Exception(@"XML Parse Error: PropagationFlag end element detected but was not expected.");
                                inPropagationFlag = false;
                                break;
                            case "AccessControlType":
                                if (!inAccessControlType)
                                    throw new Exception(@"XML Parse Error: AccessControlType end element detected but was not expected.");
                                inAccessControlType = false;
                                break;
                            case "FolderSpecs":
                                if (!inFolderSpecs)
                                    throw new Exception("XML Parse Error: FolderSpecs end element detected but was not expected.");
                                inFolderSpecs = false;
                                break;
                            case "Folder":
                                if (!inFolder)
                                    throw new Exception(@"XML Parse Error: Folder end element detected but was not expected.");
                                inFolder = false;
                                folderSpecs.Add(currFolderSpec);
                                break;
                            case "Path":
                                if (!inPath)
                                    throw new Exception(@"XML Parse Error: Path end element detected but was not expected.");
                                inPath = false;
                                break;
                            case "Permission":
                                if (!inPermission)
                                    throw new Exception(@"XML Parse Error: Permission end element detected but was not expected.");
                                inPermission = false;
                                break;
                            case "StarDepth":
                                if (!inStarDepth)
                                    throw new Exception(@"XML Parse Error: StarDepth end element detected but was not expected.");
                                inStarDepth = false;
                                break;
                            default:
                                throw new Exception("XML Parse Error: unrecognized element detected.");
                        }
                        break;
                }
            }
            configSpec.FolderSpecs = folderSpecs.ToArray();
            return configSpec;
        }
    }
}
