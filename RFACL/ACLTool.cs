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
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text.RegularExpressions;

namespace RFACL
{
    class ACLTool
    {
        public static void ApplyACLs(Specs.ConfigSpec configSpec, string path, bool verbose = false)
        {
            foreach (String dir in DirSearch(path))
            {
                string searchPath = dir.Replace(path, "");
                // find the first FolderSpec that matches
                foreach (Specs.FolderSpec folderSpec in configSpec.FolderSpecs)
                {
                    if (folderSpec.Path.Contains("*") || folderSpec.Path.Contains("?"))
                    {
                        Regex r =
                            new Regex("^" + Regex.Escape(folderSpec.Path).Replace(@"\*", ".*").Replace(@"\?", ".") + "$",
                                RegexOptions.IgnoreCase);
                        if (!r.IsMatch(searchPath)) continue;
                        if (folderSpec.StarDepth > 0)
                        {
                            int searchPathCount = searchPath.Count(f => f == '\\');
                            int folderSpecMax = folderSpec.Path.Count(f => f == '\\') + folderSpec.StarDepth - 1;
                            if (searchPathCount > folderSpecMax) continue;
                        }
                        if (verbose)
                            Console.WriteLine("Found match for " + dir + ": " + folderSpec.Path + ". Applying ACL...");
                        try
                        {
                            ApplyACL(folderSpec, dir);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Error Applying ACL: " + dir + " could not be modified. " +
                                              e.Message);
                        }
                        break;
                    }
                    if (!folderSpec.Path.Contains(searchPath)) continue;
                    if (verbose)
                        Console.WriteLine("Found match for " + dir + ": " + folderSpec.Path + ". Applying ACL...");
                    try
                    {
                        ApplyACL(folderSpec, dir);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Error Applying ACL: " + dir + " could not be modified, because " + e.Message);
                    }
                    break;
                }
            }
        }

        private static void ApplyACL(Specs.FolderSpec folderSpec, string path)
        {
            
            DirectoryInfo dInfo = new DirectoryInfo(path);
            DirectorySecurity dSecurity = dInfo.GetAccessControl();

            if (folderSpec.Permission.CleanExplicit)
            {
                AuthorizationRuleCollection rules = null;
                if (folderSpec.Permission.PreserveInherited)
                    rules = dSecurity.GetAccessRules(true, false, typeof(System.Security.Principal.NTAccount));
                else
                    rules = dSecurity.GetAccessRules(true, true, typeof(System.Security.Principal.NTAccount));
                foreach (FileSystemAccessRule rule in rules)
                    dSecurity.RemoveAccessRule(rule);
            }

            foreach (Specs.UserGroupSpec userGroup in folderSpec.Permission.UserGroups)
            {
                dSecurity.AddAccessRule(new FileSystemAccessRule(userGroup.Name, userGroup.FileSystemRight, userGroup.InheritanceFlag, userGroup.PropagationFlag, userGroup.AccessControlType));
            }

            dSecurity.SetAccessRuleProtection(folderSpec.Permission.ProtectFromInheritance, folderSpec.Permission.PreserveInherited);

            dInfo.SetAccessControl(dSecurity);
        }


        private static string[] DirSearch(string sDir)
        {
            List<String> dirsFound = new List<string>();
            dirsFound.Add(sDir + @"\");
            dirsFound.AddRange(DirSearchWorker(sDir));
            return dirsFound.ToArray();
        }

        private static string[] DirSearchWorker(string sDir)
        {
            List<String> dirsFound = new List<string>();
            try
            {
                foreach (string d in Directory.GetDirectories(sDir))
                {
                    dirsFound.Add(d);
                    dirsFound.AddRange(DirSearch(d));
                }
            }
            catch
            {
                // just continue
            }
            return dirsFound.ToArray();
        }
    }
}
