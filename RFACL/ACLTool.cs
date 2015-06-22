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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text.RegularExpressions;

namespace RFACL
{
    class ACLTool
    {
        public static void ApplyACLs(RFACLConfig configSpec, string path, bool verbose = false)
        {
            foreach (var dir in DirSearch(path, configSpec.MaxScanDepth))
            {
                var searchPath = dir.Replace(path, "");
                // find the first FolderSpec that matches
                var watch = Stopwatch.StartNew();
                foreach (var folderSpec in configSpec.Folders)
                {
                    if (folderSpec.Path.Contains("*") || folderSpec.Path.Contains("?"))
                    {
                        var r =
                            new Regex("^" + Regex.Escape(folderSpec.Path).Replace(@"\*", ".*").Replace(@"\?", ".") + "$",
                                RegexOptions.IgnoreCase);
                        if (!r.IsMatch(searchPath)) continue;
                        if (folderSpec.StarDepth > 0)
                        {
                            var searchPathCount = searchPath.Count(f => f == '\\');
                            var folderSpecMax = folderSpec.Path.Count(f => f == '\\') + folderSpec.StarDepth - 1;
                            if (searchPathCount > folderSpecMax) continue;
                        }

                        var curPermission = configSpec.Permissions.FirstOrDefault(permission => folderSpec.Permission == permission.Name);

                        if (verbose)
                            Console.WriteLine("Found match for " + dir + ": " + folderSpec.Path + ".  Applying ACL...");
                        try
                        {
                            ApplyACL(folderSpec, curPermission, dir);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Error Applying ACL: " + dir + " could not be modified. " +
                                              e.Message);
                        }
                        if (verbose)
                            Console.WriteLine("Applying ACL took: " + watch.ElapsedMilliseconds + "ms");
                        break;
                    }

                    if (!folderSpec.Path.Contains(searchPath)) continue;

                    var curPermissions = configSpec.Permissions.FirstOrDefault(permission => folderSpec.Permission == permission.Name);

                    if (verbose)
                        Console.WriteLine("Found match for " + dir + ": " + folderSpec.Path + ". Applying ACL...");
                    try
                    {
                        ApplyACL(folderSpec, curPermissions, dir);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Error Applying ACL: " + dir + " could not be modified, because " + e.Message);
                    }
                    if (verbose)
                        Console.WriteLine("Applying ACL took: " + watch.ElapsedMilliseconds + "ms");
                    break;
                }
            }
        }

        private static void ApplyACL(Folder folderSpec, Permission permissionSpec, string path)
        {
            var dInfo = new DirectoryInfo(path);
            var dSecurity = dInfo.GetAccessControl();

            if (permissionSpec.CleanExplicit)
            {
                var rules = dSecurity.GetAccessRules(true, !permissionSpec.PreserveInherited, typeof(System.Security.Principal.NTAccount));
                foreach (FileSystemAccessRule rule in rules)
                    dSecurity.RemoveAccessRule(rule);
            }

            foreach (var userGroup in permissionSpec.UserGroups)
            {
                dSecurity.AddAccessRule(new FileSystemAccessRule(userGroup.Name, userGroup.FileSystemRight, userGroup.InheritanceFlag, userGroup.PropagationFlag, userGroup.AccessControlType));
            }

            dSecurity.SetAccessRuleProtection(permissionSpec.ProtectFromInheritance, permissionSpec.PreserveInherited);

            dInfo.SetAccessControl(dSecurity);
        }


        private static IEnumerable<string> DirSearch(string sDir, int maxScanDepth)
        {
            var dirsFound = new List<string> {sDir + @"\"};
            dirsFound.AddRange(DirSearchWorker(sDir, 0, maxScanDepth));
            return dirsFound.ToArray();
        }

        private static IEnumerable<string> DirSearchWorker(string sDir, int currDepth, int maxScanDepth)
        {
            var dirsFound = new List<string>();
            try
            {
                foreach (var d in Directory.GetDirectories(sDir))
                {
                    dirsFound.Add(d);
                    if (maxScanDepth == -1 || currDepth < maxScanDepth)
                        dirsFound.AddRange(DirSearchWorker(d, currDepth++, maxScanDepth));
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
