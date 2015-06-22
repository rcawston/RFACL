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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text.RegularExpressions;

namespace RFACL
{
    /// <summary>
    /// Tool for applying file/folder ACLs
    /// </summary>
    class ACLTool
    {
        /// <summary>
        /// Applies a list of folder ACLs
        /// </summary>
        /// <param name="configSpec">ACL and folders rules</param>
        /// <param name="path">path to apply rules in</param>
        /// <param name="verbose">output to console</param>
        public static void ApplyACLs(RFACLConfig configSpec, string path, bool verbose = false)
        {
            // for each sub-folder in the search path:
            // - find the first Folder spec that matches
            // - fetch the permission rules for the matching Folder spec
            // - apply the permission rules (ACLs)
            foreach (var dir in DirSearch(path, configSpec.MaxScanDepth))
            {
                // start a stopwatch to track how long it takes to apply this ACL
                var watch = Stopwatch.StartNew();
                // determine searchPath by removing the root folder (for later comparison to folder rules)
                var searchPath = dir.Replace(path, "");

                // find the first Folder spec that matches
                foreach (var folderSpec in configSpec.Folders)
                {
                    // does this spec contain wildcards (*/?)?
                    if (folderSpec.Path.Contains("*") || folderSpec.Path.Contains("?"))
                    {
                        // contains wildcards, so see if the spec matches our current folder
                        if (!(new Regex(
                            "^" + Regex.Escape(folderSpec.Path).Replace(@"\*", ".*").Replace(@"\?", ".") + "$",
                            RegexOptions.IgnoreCase)).IsMatch(searchPath))
                        {
                            // it doesn't match, so try the next folder spec
                            continue;
                        }
                        // we found a matching spec...
                        // is star depth limited?
                        if (folderSpec.StarDepth > 0)
                        {
                            // yes, start depth is limited.
                            // if the search path is deeper then maximum allowed star depth, try the next folder spec
                            if (searchPath.Count(f => f == '\\') >
                                (folderSpec.Path.Count(f => f == '\\') + folderSpec.StarDepth - 1))
                                continue;
                        }

                        if (verbose)
                            Console.WriteLine("Found match for " + dir + ": " + folderSpec.Path + ".  Applying ACL...");

                        try
                        {
                            // Fetch the permission rule, and attempt to apply the ACL
                            ApplyACL(configSpec.GetMatchingPermission(folderSpec.Permission), dir);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Error Applying ACL: " + dir + " could not be modified. " +
                                              e.Message);
                        }
                        if (verbose)
                            Console.WriteLine("Applying ACL took: " + watch.ElapsedMilliseconds + "ms");

                        // we're done with this folder; so, exit the for loop
                        break;
                    }

                    // this is a non-wildcard spec
                    // is the current folder doesn't match the spec, continue to the next spec
                    if (!folderSpec.Path.Contains(searchPath)) continue;

                    if (verbose)
                        Console.WriteLine("Found match for " + dir + ": " + folderSpec.Path + ". Applying ACL...");
                    try
                    {
                        // Fetch the permission rule, and attempt to apply the ACL
                        ApplyACL(configSpec.GetMatchingPermission(folderSpec.Permission), dir);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Error Applying ACL: " + dir + " could not be modified, because " + e.Message);
                    }
                    if (verbose)
                        Console.WriteLine("Applying ACL took: " + watch.ElapsedMilliseconds + "ms");

                    // we're done with this folder; so, exit the for loop
                    break;
                }
            }
        }

        /// <summary>
        /// Applies a permission spec (folder ACLs) to the given path
        /// </summary>
        /// <param name="permissionSpec">folder ACL rules</param>
        /// <param name="path">the path to which to apply the ACL rules</param>
        private static void ApplyACL(Permission permissionSpec, string path)
        {
            // get directory info and security information
            var dInfo = new DirectoryInfo(path);
            var dSecurity = dInfo.GetAccessControl();

            // if the spec says to clean explicit rules, do so
            if (permissionSpec.CleanExplicit)
            {
                var rules = dSecurity.GetAccessRules(true, !permissionSpec.PreserveInherited, typeof(System.Security.Principal.NTAccount));
                foreach (FileSystemAccessRule rule in rules)
                    dSecurity.RemoveAccessRule(rule);
            }

            // add user groups from the spec
            foreach (var userGroup in permissionSpec.UserGroups)
            {
                // add the access rule
                dSecurity.AddAccessRule(new FileSystemAccessRule(userGroup.Name,
                    userGroup.FileSystemRight, userGroup.GetInheritanceFlags(),
                    userGroup.GetPropagationFlags(), userGroup.AccessControlType)
                );
            }

            // set the access rule protections from the spec
            dSecurity.SetAccessRuleProtection(permissionSpec.ProtectFromInheritance, permissionSpec.PreserveInherited);

            // apply the ACL
            dInfo.SetAccessControl(dSecurity);
        }

        /// <summary>
        /// DirSearch - search a path for all contained folders 
        /// </summary>
        /// <param name="sDir">path to search</param>
        /// <param name="maxScanDepth">maximum path depth</param>
        /// <returns>list of contained folders</returns>
        private static IEnumerable<string> DirSearch(string sDir, int maxScanDepth)
        {
            var dirsFound = new List<string> {sDir + @"\"};
            dirsFound.AddRange(DirSearchWorker(sDir, 0, maxScanDepth));
            return dirsFound.ToArray();
        }

        /// <summary>
        /// Worker function for DirSearch
        /// </summary>
        /// <param name="sDir">path to search</param>
        /// <param name="currDepth">the current depth</param>
        /// <param name="maxScanDepth">maximum path depth</param>
        /// <returns></returns>
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
