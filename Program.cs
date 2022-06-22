
namespace csgenns
{
    class Program
    {
        static void Main(string[] args)
        {
            csgenmain cg = new csgenmain();
            // test
            bool testme = false;
            if(testme)
            {
                cg.parameters.Add("replacen");
                cg.parameters.Add(@"c:\temp\test-replacen-source.txt");
                cg.parameters.Add("searchfor");
                cg.parameters.Add("replaceme");
                cg.parameters.Add("2");
                cg.parameters.Add("2");
                cg.parameters.Add(@"c:\temp\test-replacen-output.txt");
                cg.Run();
                return;
            }

            if(args.Length==0)
            {
                cg.NoParameters();
            }
            else if(args.Length==1)
            {
                cg.OneParameter(args[0].ToString());
            }
            else
            {
                cg.SetParameters(args);
                cg.Run();
                // Console.WriteLine("");
                // Console.WriteLine("csgen.exe completed");
            }
        }
    }

    public class csgenmain
    {
        public List<string> parameters = new List<string>();


        public void Run()
        {
            if(parameters[0].ToLower()=="replace")
            {
                if(parameters.Count == 2 && ( parameters[1].ToLower() == "-h" || parameters[1].ToLower() == "--help"))
                {
                    this.OneParameter("replace");
                    return;
                }
                else if(parameters.Count < 4 || parameters.Count > 5 )
                {
                    this.Message("Incorrect number of parameters passed to csgen replace");
                    this.Message("");
                    this.OneParameter("replace");
                    return;
                }
                else if(parameters[2].ToLower() == "")
                {
                    this.Message("Search text cannot be empty.");
                    return;
                }
                string destfile = "";
                if(parameters.Count==5)
                    destfile = parameters[4];
                this.Replace(parameters[1], parameters[2], parameters[3], destfile,0,0);
            }
            else if(parameters[0].ToLower()=="replacedq")
            {
                if(parameters.Count == 2 && ( parameters[1].ToLower() == "-h" || parameters[1].ToLower() == "--help"))
                {
                    this.OneParameter("replacedq");
                    return;
                }
                else if(parameters.Count < 3 || parameters.Count > 4 )
                {
                    this.Message("Incorrect number of parameters passed to csgen replacedq");
                    this.Message("");
                    this.OneParameter("replace");
                    return;
                }
                else if(parameters[2].ToLower() == "")
                {
                    this.Message("Search text cannot be empty.");
                    return;
                }
                string destfile = "";
                if(parameters.Count==4)
                    destfile = parameters[3];
                this.Replace(parameters[1], '"'.ToString(), parameters[2], destfile,0,0);
            }
            else if(parameters[0].ToLower()=="replacewithdq")
            {
                if(parameters.Count == 2 && ( parameters[1].ToLower() == "-h" || parameters[1].ToLower() == "--help"))
                {
                    this.OneParameter("replacewithdq");
                    return;
                }                
                else if(parameters.Count < 3 || parameters.Count > 4 )
                {
                    this.Message("Incorrect number of parameters passed to csgen replacewithdq");
                    this.Message("");
                    this.OneParameter("replace");
                    return;
                }
                else if(parameters[2].ToLower() == "")
                {
                    this.Message("Search text cannot be empty.");
                    return;
                }
                string destfile = "";
                if(parameters.Count==4)
                    destfile = parameters[3];
                this.Replace(parameters[1], parameters[2], '"'.ToString(), destfile,0,0);
            }
            else if(parameters[0].ToLower()=="replacechar" 
                || parameters[0].ToLower()=="replacechr" 
                || parameters[0].ToLower()=="replaceasc" 
                || parameters[0].ToLower()=="replaceascii" )
            {
                if(parameters.Count == 2 && ( parameters[1].ToLower() == "-h" || parameters[1].ToLower() == "--help"))
                {
                    this.OneParameter("replacechar");
                    return;
                }
                else if(parameters.Count < 4 || parameters.Count > 5 )
                {
                    this.Message("Incorrect number of parameters passed to csgen replacechar");
                    this.Message("");
                    this.OneParameter("replace");
                    return;
                }
                else if(parameters[2].ToLower() == "")
                {
                    this.Message("Search text cannot be empty.");
                    return;
                }
                string searchchartext = new String(parameters[2].Where(Char.IsDigit).ToArray());
                string replacechartext = new String(parameters[3].Where(Char.IsDigit).ToArray());
                if(searchchartext == "")
                {
                    this.Message("Search CHR() must be a number from 1 to 254.");
                    return;
                }
                if(replacechartext == "")
                {
                    this.Message("Replace CHR() must be a number from 1 to 254.");
                    return;
                }
                int searchcharnum = Int32.Parse(searchchartext);
                int replacecharnum = Int32.Parse(replacechartext);
                if(searchcharnum < 1 || searchcharnum > 254)
                {
                    this.Message("Search CHR() must be a number from 1 to 254.");
                    return;
                }
                if(replacecharnum < 1 || replacecharnum > 254)
                {
                    this.Message("Replace CHR() must be a number from 1 to 254.");
                    return;
                }
                string destfile = "";
                if(parameters.Count==5)
                    destfile = parameters[4];
                this.Replace(parameters[1], ((char)searchcharnum).ToString(), ((char)replacecharnum).ToString(), destfile,0,0);
            }
            else if(parameters[0].ToLower()=="replacen"
                || parameters[0].ToLower()=="replacenth")
            {
                if(parameters.Count == 2 && ( parameters[1].ToLower() == "-h" || parameters[1].ToLower() == "--help"))
                {
                    this.OneParameter("replacen");
                    return;
                }
                else if(parameters.Count < 6 || parameters.Count > 7 )
                {
                    this.Message("Incorrect number of parameters passed to csgen replacen");
                    this.Message("");
                    this.OneParameter("replacen");
                    return;
                }
                else if(parameters[2].ToLower() == "")
                {
                    this.Message("Search text cannot be empty.");
                    return;
                }
                string nthitemtext = new String(parameters[4].Where(Char.IsDigit).ToArray());
                string numitemstext = new String(parameters[5].Where(Char.IsDigit).ToArray());
                if(nthitemtext == "")
                {
                    this.Message("The nth number must be between 1 and 65535.");
                    return;
                }
                if(numitemstext == "")
                {
                    this.Message("The number of items to replace must be a number from 0 (all) to 65535.");
                    return;
                }
                int nthitem = Int32.Parse(nthitemtext);
                int numitems = Int32.Parse(numitemstext);
                if(nthitem < 1 || nthitem > 65535)
                {
                    this.Message("The nth number must be between 1 and 65535.");
                    return;
                }
                if(numitems < 0 || numitems > 65535)
                {
                    this.Message("The number of items to replace must be a number from 0 (all) to 65535.");
                    return;
                }
                string destfile = "";
                if(parameters.Count==7)
                    destfile = parameters[6];

// Usage: csgen replacen sourcefile searchtext replacetext [nthitem] [numitems] [destfile]

                this.Replace(parameters[1], parameters[2], parameters[3], destfile, nthitem, numitems);



            }

        }


        public void Replace(string sourcefile, string searchtext, string replacetext, string destfile, int nthitem, int numitems)
        {
            if(!System.IO.File.Exists(sourcefile))
            {
                this.Message($"File '{sourcefile}' cannot be found.");
                return;
            }
            // if(destfile != "" && !System.IO.File.Exists(destfile))
            // {
            //     this.Message($"File '{destfile}' cannot be found.");
            //     return;
            // }
            string text = "";
            bool success = true;
            try
            {
                text = System.IO.File.ReadAllText(sourcefile);
            }
            catch
            {
                success = false;
            }
            if(!success)
            {
                this.Message($"Could not open file '{sourcefile}'.");
                return;
            }
            int occurcount = ( text.Length - text.Replace(searchtext,"").Length ) / searchtext.Length;
            int itemsreplaced = 0;
            if(nthitem == 0 && numitems == 0)
            {
                text = text.Replace(searchtext,replacetext);
                itemsreplaced = occurcount;
            }
            else if(nthitem > occurcount)
            {
                 this.Message($"There are fewer than {nthitem} instances of '{searchtext}' in '{sourcefile}'.");
                 return;
            }
            else
            {
                // we will do a split then recombine
                string[] parts = text.Split(searchtext,StringSplitOptions.None);
                var sb = new System.Text.StringBuilder();
                for(int i=0;i<parts.Length;i++)
                {
                    // if this is NOT yet at the nthitem then we put the original string back in
                    sb.Append(parts[i]);
                    if(i==(parts.Length-1))
                    {

                    }
                    else if(i == (nthitem-1) || ( i > (nthitem-1) && ((itemsreplaced < numitems) || numitems == 0)))
                    {
                        sb.Append(replacetext);
                        itemsreplaced++;
                    }
                    else
                    {
                        sb.Append(searchtext);
                    }
                }
                text = sb.ToString();
            }
            

            if(destfile == "")
                destfile = sourcefile;
            try
            {
                System.IO.File.WriteAllText(destfile,text);
            }
            catch
            {
                success = false;
            }
            if(!success)
            {
                this.Message($"Could not write to file '{destfile}'.");
                return;
            }
            if(occurcount == 0)
            {
                this.Message($"File '{destfile}' does not contain text '{searchtext}'.");
            }
            else if(itemsreplaced == 0)
            {
                this.Message($"Wrote to file '{destfile}' with no changes.");
            }
            else if(itemsreplaced == 1)
            {
                this.Message($"Wrote to file '{destfile}' with one change.");
            }
            else
            {
                this.Message($"Wrote to file '{destfile}' with {itemsreplaced} changes.");
            }

        }

        public void NoParameters()
        {
            this.Message("Usage: csgen [options]");
            this.Message("");
            this.Message("Options:");
            this.Message("  -h|--help         Display help.");
            this.Message("  --info            Display csgen installation information.");
            this.Message("  -c|--commands     Display a list of csgen commands.");
        }

        public void OneParameter(string singleparameter)
        {
            if(singleparameter.Trim().ToLower()=="-h" || singleparameter.Trim().ToLower()=="--help")
            {
                this.Message("Usage: csgen [command] [options]");
                this.Message("");
                this.Message("Command:");
                this.Message("  replace           Performs a search-replace in text file.");
                this.Message("  replacechar       Performs a search-replace with CHR() numbers in text file.");
                this.Message("  replacen          Search and replace the nth items found in a text file.");
                this.Message("  replacedq         Performs a search in text file for double-quotes.");
                this.Message("  replacewithdq     Performs a replace in text file with double-quotes.");
                this.Message("");
                this.Message("Options:");
                this.Message("  -h|--help         Display help.");
            }
            else if(singleparameter.Trim().ToLower()=="replace")
            {
                this.Message("Usage: csgen replace sourcefile searchtext replacetext [destfile]");
                this.Message("");
                this.Message("sourcefile");
                this.Message("  Full path to the source file to be searched.");
                this.Message("");
                this.Message("searchtext");
                this.Message("  Text to be searched for.");
                this.Message("");
                this.Message("replacetext");
                this.Message("  Text that will replace 'searchtext' in the new file.");
                this.Message("");
                this.Message("destfile");
                this.Message("  Optional: Full path to output text file.");
            }
            else if(singleparameter.Trim().ToLower()=="replacechar")
            {
                this.Message("Usage: csgen replacechar sourcefile searchcharnum replacecharnum [destfile]");
                this.Message("");
                this.Message("sourcefile");
                this.Message("  Full path to the source file to be searched.");
                this.Message("");
                this.Message("searchcharnum");
                this.Message("  CHR() value of single-character to be searched for.");
                this.Message("");
                this.Message("replacecharnum");
                this.Message("  CHR() value of single-character to replace with.");
                this.Message("");
                this.Message("destfile");
                this.Message("  Optional: Full path to output text file.");
            }
            else if(singleparameter.Trim().ToLower()=="replacedq")
            {
                this.Message("Usage: csgen replacedq sourcefile replacetext [destfile]");
                this.Message("");
                this.Message("sourcefile");
                this.Message("  Full path to the source file to be searched.");
                this.Message("");
                this.Message("destfile");
                this.Message("  Optional: Full path to output text file.");
            }
            else if(singleparameter.Trim().ToLower()=="replacewithdq")
            {
                this.Message("Usage: csgen replacewithdq sourcefile searchtext [destfile]");
                this.Message("");
                this.Message("sourcefile");
                this.Message("  Full path to the source file to be searched.");
                this.Message("");
                this.Message("destfile");
                this.Message("  Optional: Full path to output text file.");
            }
            else if(singleparameter.Trim().ToLower()=="replacen")
            {
                this.Message("Usage: csgen replacen sourcefile searchtext replacetext nthitem [numitems] [destfile]");
                this.Message("");
                this.Message("sourcefile");
                this.Message("  Full path to the source file to be searched.");
                this.Message("");
                this.Message("searchtext");
                this.Message("  Text to be searched for.");
                this.Message("");
                this.Message("replacetext");
                this.Message("  Text that will replace 'searchtext' in the new file.");
                this.Message("");
                this.Message("nthitem");
                this.Message("  Find the nth-item and replace it with replacetext.");
                this.Message("");
                this.Message("numitems");
                this.Message("  Optional: Find the nth item then replace this numitems times afterwards.");
                this.Message("");
                this.Message("destfile");
                this.Message("  Optional: Full path to output text file.");
            }


        }


/*
.NET SDK (6.0.201)
Usage: dotnet [runtime-options] [path-to-application] [arguments]

Execute a .NET application.

runtime-options:
  --additionalprobingpath <path>   Path containing probing policy and assemblies to probe for.
  --additional-deps <path>         Path to additional deps.json file.
  --depsfile                       Path to <application>.deps.json file.
  --fx-version <version>           Version of the installed Shared Framework to use to run the application.
  --roll-forward <setting>         Roll forward to framework version  (LatestPatch, Minor, LatestMinor, Major, LatestMajor, Disable).
  --runtimeconfig                  Path to <application>.runtimeconfig.json file.

path-to-application:
  The path to an application .dll file to execute.

Usage: dotnet [sdk-options] [command] [command-options] [arguments]

Execute a .NET SDK command.

sdk-options:
  -d|--diagnostics  Enable diagnostic output.
  -h|--help         Show command line help.
  --info            Display .NET information.
  --list-runtimes   Display the installed runtimes.
  --list-sdks       Display the installed SDKs.
  --version         Display .NET SDK version in use.

SDK commands:
  add               Add a package or reference to a .NET project.
  build             Build a .NET project.
  build-server      Interact with servers started by a build.
  clean             Clean build outputs of a .NET project.
  format            Apply style preferences to a project or solution.
  help              Show command line help.
  list              List project references of a .NET project.
  msbuild           Run Microsoft Build Engine (MSBuild) commands.
  new               Create a new .NET project or file.
  nuget             Provides additional NuGet commands.
  pack              Create a NuGet package.
  publish           Publish a .NET project for deployment.
  remove            Remove a package or reference from a .NET project.
  restore           Restore dependencies specified in a .NET project.
  run               Build and run a .NET project output.
  sdk               Manage .NET SDK installation.
  sln               Modify Visual Studio solution files.
  store             Store the specified assemblies in the runtime package store.
  test              Run unit tests using the test runner specified in a .NET project.
  tool              Install or manage tools that extend the .NET experience.
  vstest            Run Microsoft Test Engine (VSTest) commands.
  workload          Manage optional workloads.

Additional commands from bundled tools:
  dev-certs         Create and manage development certificates.
  fsi               Start F# Interactive / execute F# scripts.
  sql-cache         SQL Server cache command-line tools.
  user-secrets      Manage development user secrets.
  watch             Start a file watcher that runs a command when files change.

Run 'dotnet [command] --help' for more information on a command.

c:\SNJW\code\shared\csgen>
*/



/*


Usage: dotnet [options]
Usage: dotnet [path-to-application]

Options:
  -h|--help         Display help.
  --info            Display .NET information.
  --list-sdks       Display the installed SDKs.
  --list-runtimes   Display the installed runtimes.

path-to-application:
  The path to an application .dll file to execute.

c:\SNJW\code\shared\csgen>

*/            

        public void SetParameters(string[] args)
        {
            parameters.Clear();
            parameters.AddRange(args.ToList());
        }

        public void Message(string message) {Console.WriteLine(message);}
    }


}