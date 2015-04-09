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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

/**
 * Ross' Fast ACL Utility
 */
namespace RFACL
{
    class RFACL
    {
        static int Main(string[] args)
        {
            // Start a stopwatch to track runtime
            var watch = Stopwatch.StartNew();
            
            // Show usage information and exit if we have less then 2 arguments
            if (args.Length < 2)
            {
                ShowUsage();
                Console.WriteLine("Press any key to exit.");
                Console.ReadKey();
                return -1;
            }

            // Flags for CLI arguments
            bool superquiet = false; /* suppress all console output - also implies quiet */
            bool quiet = false; /* suppress Press any key to exit prompts */
            bool verbose = false; /* be very verbose */

            // Parse arguments for flags
            foreach (string arg in args)
            {
                if (arg.ToLower() == "/q")
                    quiet = true;
                if (arg.ToLower() == "/qq")
                    superquiet = true;
                if (arg.ToLower() == "/v")
                    verbose = true;
            }

            // Parse arguments for XML config and Path
            string config = args[args.Count() - 2];
            string path = args[args.Count() - 1];

            // Fail out if the argument specified path does not exist
            if (!Directory.Exists(path))
            {
                if (!superquiet)
                {
                    Console.WriteLine("Path Error: " + path + " does not exist or is not a directory.");
                    ShowUsage();
                }
                if (!quiet)
                {
                    Console.WriteLine("Press any key to exit.");
                    Console.ReadLine();
                }
                return -1;
            }

            // Load and Parse the XML Config
            Specs.ConfigSpec configSpec;
            try
            {
                configSpec = ConfigLoader.LoadConfig(config);
            }
            catch (Exception e)
            {
                if (!superquiet)
                {
                    Console.WriteLine("XML Config Error: " + e.Message);
                    ShowUsage();
                }
                if (quiet) return -1;
                Console.WriteLine("Press any key to exit.");
                Console.ReadLine();
                return -1;
            }

            // In verbose mode, display all rules from the XML configuration
            if (verbose)
            {
                Console.WriteLine("Done loading XML configuration.");
                foreach (Specs.FolderSpec folderSpec in configSpec.FolderSpecs)
                {
                    Console.WriteLine(folderSpec.ToString());
                    Console.WriteLine("");
                }
                Console.WriteLine("Time to Load/Parse XML: " + watch.ElapsedMilliseconds + "ms");
            }

            // Actually apply the ACLs
            ACLTool.ApplyACLs(configSpec, path, verbose);

            if (!superquiet)
            {
                Console.WriteLine("Total time elapsed: " + watch.ElapsedMilliseconds + "ms");
            }
            if (quiet) return 0;
            Console.WriteLine("Press any key to exit.");
            Console.ReadLine();

            return 0;
        }

        public static void ShowUsage()
        {
            Console.WriteLine("RFACL v" + GetProductVersion());
            Console.WriteLine("");
            Console.WriteLine("Usage: rfacl.exe [/q] [/qq] [/v] <acl spec file> <path>");
            Console.WriteLine("       /q   =  quiet mode (suppresses 'Press any key to exit' prompts)");
            Console.WriteLine("       /qq  =  super quiet mode (no console output)");
            Console.WriteLine("       /v   =  verbose mode");
            Console.WriteLine("");
            Console.WriteLine(@"e.g.: rfacl.exe acl.xml c:\path");
            Console.WriteLine(@"    : rfacl.exe c:\path\to\config\acl.xml c:\path");
            Console.WriteLine(@"    : rfacl.exe /qq c:\path\to\config\acl.xml c:\path");
            Console.WriteLine(@"    : rfacl.exe /v c:\path\to\config\acl.xml c:\path");
            Console.WriteLine("");
        }

        public static string GetProductVersion()
        {
            return Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }
    }
}