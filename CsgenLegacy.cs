
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Seamlex.Utilities;
#pragma warning disable CS8602, CS8600
namespace Seamlex.Utilities
{
    /// <summary>
    /// Manages parameters
    /// </summary>
    public class CsgenLegacy
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
                else if(parameters.Count < 5 || parameters.Count > 7 )
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
                string numitemstext = "";
                if(parameters.Count>5)
                    numitemstext = new String(parameters[5].Where(Char.IsDigit).ToArray());
                string destfile = "";

// Usage: csgen replacen sourcefile searchtext replacetext nthitem [numitems] [destfile]

                // if this is instead an output file, it will not be an integer
                if(numitemstext != "" && numitemstext != parameters[5])
                {
                    // this can occur if there is a number in the file-name
                    destfile = parameters[5];
                    numitemstext = "1";
                }
                else if(parameters.Count>6)
                {
                    destfile = parameters[6];
                }
                else
                {
                    destfile = parameters[1];
                }
                if(nthitemtext == "")
                    nthitemtext = "1";
                if(numitemstext == "")
                    numitemstext = "1";

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

// Usage: csgen replacen sourcefile searchtext replacetext nthitem [numitems] [destfile]

                this.Replace(parameters[1], parameters[2], parameters[3], destfile, nthitem, numitems);
            }
            else if(parameters[0].ToLower()=="model")
            {
//                  Usage: csgen model new [filename] [options]
                if(parameters.Count == 2 && ( parameters[1].ToLower() == "-h" || parameters[1].ToLower() == "--help"))
                {
                    this.OneParameter("model");
                    return;
                }
                else if(parameters.Count < 4 || parameters.Count > 4 )
                {
                    this.Message("Incorrect number of parameters passed to csgen model");
                    this.Message("");
                    this.OneParameter("model");
                    return;
                }
                else if(parameters[1].ToLower() != "new" && parameters[1].ToLower() != "edit")
                {
                    this.Message("The 'csgen model' command must be followed by 'new' or 'edit'.");
                    return;
                }

                this.Model(parameters[1].ToLower(),parameters[2].Trim(),parameters[3].Trim());
            }
            else if(parameters[0].ToLower()=="insert"
                || parameters[0].ToLower()=="insertline")
            {
                if(parameters.Count == 2 && ( parameters[1].ToLower() == "-h" || parameters[1].ToLower() == "--help"))
                {
                    this.OneParameter("insert");
                    return;
                }
                else if(parameters.Count < 3 || parameters.Count > 5 )
                {
                    this.Message("Incorrect number of parameters passed to csgen insert");
                    this.Message("");
                    this.OneParameter("insert");
                    return;
                }
                // else if(parameters[2].ToLower() == "")
                // {
                //     this.Message("Search text cannot be empty.");
                //     return;
                // }
                string insertitemtext = "";
                string linenumtext = "";
                if(parameters.Count>2)
                    insertitemtext = new String(parameters[2].Where(Char.IsDigit).ToArray());
                if(parameters.Count>3)
                    linenumtext = new String(parameters[3].Where(Char.IsDigit).ToArray());
                string destfile = "";

                // if this is instead an output file, it will not be an integer
                if(linenumtext != "" && linenumtext != parameters[3])
                {
                    // this can occur if there is a number in the file-name
                    destfile = parameters[3];
                    linenumtext = "";
                }
                else if(parameters.Count>4)
                {
                    destfile = parameters[4];
                }
                else
                {
                    destfile = parameters[1];
                }
                int linenumber = 0;
                if(linenumtext != "")
                    linenumber = Int32.Parse(linenumtext);

// Usage: csgen insert sourcefile inserttext [linenumber] [destfile]

                this.Insert(parameters[1], parameters[2], linenumber, destfile);
            }

        }

        public void Model(string command, string destfile, string options)
        {
            modeloptions setup = this.GetModel(command,destfile,options);
            string text = this.GetModelText(setup);
            bool success = true;

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


            this.Message($"Created file '{destfile}' with {setup.modelfields.Count}/{setup.parentmodels.Count}/{setup.childmodels.Count} fields/parents/children.");

        }

        public string GetModelText(modeloptions setup)
        {

            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"namespace {setup.modelnamespace}");
            sb.AppendLine("{");
            sb.AppendLine($"    public class {setup.modelname}");
            sb.AppendLine("    {");
            foreach(var item in setup.modelfields)
            {
                sb.AppendLine($"        public {item.datatype} {setup.fieldprefix}{item.fieldname} "+"{ get; set; }");
            }
            foreach(var item in setup.childmodels)
            {
                sb.AppendLine($"        public List<{item} {item}s "+"{ get; set; }");
            }
            foreach(var item in setup.parentmodels)
            {
                sb.AppendLine($"        public {item} {item} "+"{ get; set; }");
            }

            sb.AppendLine("    }");
            sb.AppendLine("}");


            return sb.ToString();
        }

/*

public class Blog
{
    public int BlogId { get; set; }
    public string Url { get; set; }

    [DataType(DataType.Date)]
    public DateTime ReleaseDate { get; set; }

    public List<Post> Posts { get; set; }
}

public class Post
{
    public int PostId { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }

    public string BlogUrl { get; set; }
    public Blog Blog { get; set; }
}

*/

        public modeloptions GetModel(string command, string destfile, string options)
        {


            // load the options-object
            modeloptions setup = new modeloptions();
            setup.command = command;
            setup.destfile = destfile;
            setup.modelname = "";
            setup.modelnamespace = "";
            setup.fieldprefix = "";
            
            string[] optionparts = options.Split(';',StringSplitOptions.None);
            for(int i = 0;i<optionparts.Length;i++)
            {
                string[] fieldparts = optionparts[i].Split('=',StringSplitOptions.None);
                string fieldname = fieldparts[0].Trim().ToLower();
                string fieldvalue = "";
                if(fieldparts.Length>1)
                    fieldvalue = fieldparts[1].Trim();
                
                if(fieldname=="name")
                {
                    setup.modelname = fieldvalue;
                }
                else if(fieldname=="namespace")
                {
                    setup.modelnamespace = fieldvalue;
                }
                else if(fieldname=="fieldprefix")
                {
                    setup.fieldprefix = fieldvalue;
                }
                else if(fieldname=="fields")
                {
                    var fields = fieldvalue.Split(',',StringSplitOptions.None);
                    for(int j = 0;j<fields.Length;j++)
                    {
                        var fieldvalues = fields[j].Trim().Split(':',StringSplitOptions.None);
                        string dbname = fieldvalues[0].Trim();
                        string dbtype = "";
                        if(fieldvalues.Length>1)
                            dbtype = fieldvalues[1].Trim();

                        if(dbname!="")
                        {
                            var dbfield = new modelfield();
                            dbfield.fieldname = dbname;
                            dbfield.datatype = dbtype;
                            dbfield.ispkey = false;
                            setup.modelfields.Add(dbfield);
                        }
                    }
                }
                else if(fieldname=="parentmodels")
                {
                    var parentmodels = fieldvalue.Split(',',StringSplitOptions.None);
                    for(int j = 0;j<parentmodels.Length;j++)
                    {
                        setup.parentmodels.Add(parentmodels[j].Trim());
                    }
                }
                else if(fieldname=="childmodels")
                {
                    var childmodels = fieldvalue.Split(',',StringSplitOptions.None);
                    for(int j = 0;j<childmodels.Length;j++)
                    {
                        setup.childmodels.Add(childmodels[j].Trim());
                    }
                }


                // this.Message("Examples:");
                // this.Message(@"csgen newmodel Project.cs ""name=Project;namespace=zc.Models;fields=Id:Guid,Code:String,Name:String,Desc:String;fieldprefix=Project;pkey=Id;parentmodels=Agent;childmodels=Job,Staff""");
                // this.Message(@"csgen newmodel Job.cs ""name=Job;namespace=abc.Models;fields=Id:Int32,Name:String,Desc:String;pkey=Id;parentmodels=Project;""");
            }
            return setup;

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

        public void Insert(string sourcefile, string inserttext, int linenumber, string destfile)
        {
// Usage: csgen insert sourcefile inserttext [linenumber] [destfile]
            if(!System.IO.File.Exists(sourcefile))
            {
                this.Message($"File '{sourcefile}' cannot be found.");
                return;
            }
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

            // 2022-07-11 SNJW I'm not sure how I should account for DOS/UNIX "\r\n" vs '\n' differences
            string newline = this.GuessDosOrUnix(text);
            string[] lines = text.Split(newline,StringSplitOptions.None);

            if(linenumber>lines.Length)
                linenumber = 0;

            // if the value of linenumber is zero then it just goes at the end
            var sb = new System.Text.StringBuilder();
            if(linenumber == 0)
            {
                sb.Append(text);
                sb.Append(newline);
                sb.Append(inserttext);
// text
            }
            else if(linenumber == 1)
            {
                sb.Append(inserttext);
                sb.Append(newline);
                sb.Append(text);
// text
            }
            else
            {
                // put these back together - note that linenumber is 1-based
                for(int i = 0; i < lines.Length;i++)
                {
                    if(linenumber == (i + 1))
                    {
                        sb.Append(inserttext);
                        sb.Append(newline);
                    }
                    sb.Append(lines[i]);
                    if(i != (lines.Length - 1))
                    {
                        sb.Append(newline);
                    }
                }

            }

            text = sb.ToString();

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
                this.Message($"Could not insert text to file '{destfile}'.");
                return;
            }
            this.Message($"Inserted text to file '{destfile}' successfully.");

        }

        public string GuessDosOrUnix(string text)
        {
            string dos = "\r\n";
            string unix = "\n";
            int doscount = text.Split(dos).Length - 1;
            int unixcount = text.Split(unix).Length - 1;
            if(doscount == 0 && unixcount == 0)
                return dos;
            if(doscount == 0)
                return unix;
// determine DOS/UNIX "\r\n" vs '\n'
            return dos;
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
                this.Message("  model             Creates and edits C# model classes.");
                this.Message("  insert            Inserts text at a specified line in a text file.");
                this.Message("");
                this.Message("Options:");
                this.Message("  -h|--help         Display help.");

// csgen newmodel C:\SNJW\code\zc\Data\Entities\Project.cs "name=Project;namespace=zc.Models;fields=Id:Guid,Code:String,Name:String,Desc:String"

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
            else if(singleparameter.Trim().ToLower()=="model")
            {
                this.Message("Usage: csgen model [command] [filename] [options]");
                this.Message("");
                this.Message("command");
                this.Message("  The action to take. Possible values are:");
                this.Message("    new         [filename] [options]: Creates a new model.");
                this.Message("    edit        [filename] [options]: Updates an existing model.");
                this.Message("");
                this.Message("filename");
                this.Message("  File to be created or edited.");
                this.Message("");
                this.Message("options");
                this.Message("  A single line of text with mode information.");
                this.Message("  Options are separated by semi-colons.");
                this.Message("  Values for options are set with the equals sign.");
                this.Message("  Possible options are:");
                this.Message("    name         Model name");
                this.Message("    namespace    C# namespace that the model is set to.");
                this.Message("    fields       List of field names and the relevant dotnet type.");
                this.Message("                 Syntax is fieldname1:type2[,fieldname2:type2].");
                this.Message("    fieldprefix  Inserts text (often the table name) in front of all fields.");
                this.Message("                 Syntax is fieldname1:type2[,fieldname2:type2].");
                this.Message("    pkey         Specifies the field name of the primary key.");
                this.Message("    parentmodels   Specifies the model name of each parent.");
                this.Message("                 Syntax is parentmodel1[,parentmodel2].");
                this.Message("    childmodels  Specifies the model name of each child.");
                this.Message("                 Syntax is childmodel1[,childmodel2].");
                this.Message("");
                this.Message("Examples:");
                this.Message(@"csgen newmodel Project.cs ""name=Project;namespace=zc.Models;fields=Id:Guid,Code:String,Name:String,Desc:String;fieldprefix=Project;pkey=Id;parentmodels=Agent;childmodels=Job,Staff""");
                this.Message(@"csgen newmodel Job.cs ""name=Job;namespace=abc.Models;fields=Id:Int32,Name:String,Desc:String;pkey=Id;parentmodels=Project;""");

//csgen newmodel C:\SNJW\code\zc\Data\Entities\Project.cs "name=Project;namespace=zc.Models;fields=Id:Guid,Code:String,Name:String,Desc:String;fieldprefix"

            }
            else if(singleparameter.Trim().ToLower()=="insert")
            {
                this.Message("Usage: csgen insert sourcefile inserttext [linenumber] [destfile]");
                this.Message("");
                this.Message("sourcefile");
                this.Message("  Full path to the source file to have text inserted.");
                this.Message("");
                this.Message("inserttext");
                this.Message("  Text to be inserted into the file.");
                this.Message("");
                this.Message("linenumber");
                this.Message("  Optional: Line at which the text is inserted.  If omitted, inserted as last line.");
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



    public class modelfield
    {
        public string fieldname = "";
        public string datatype = "";
        public bool ispkey = false;
    }

    public class modeloptions
    {
        public string command = "";
        public string destfile = "";
        public string modelname = "";
        public string modelnamespace = "";
        public string fieldprefix = "";

        public List<modelfield> modelfields = new List<modelfield>();
        public List<string> parentmodels = new List<string>();
        public List<string> childmodels = new List<string>();
    }

    
    public abstract class generichandler
    {
        public List<ParameterSetting> ps = new List<ParameterSetting>();
        public string lastmessage = "";
        public string appname = "csgen";

        
        public virtual bool Run()
        {
            return true;
        }

    }

    public abstract class classhandler : generichandler
    {


        bool usecomponentmodel = true;
        bool usedataannotations = true;
        bool disablenullmessages = true;
        // bool usenamespace = true;
        // bool usegetsetdefault = true;



        public string GetShortType(string longtype)
        {
            if(longtype == "System.String")
                return "string";
            if(longtype == "System.Int32")
                return "int";
            if(longtype == "System.Boolean")
                return "bool";
            if(longtype == "System.Byte")
                return "byte[]";
            return longtype;
        } 

        public string GetClassText(
            string classname, 
            string ns,
            string fieldtext,
            int indent = 4)
        {


            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            // sb.AppendLine("using System;");
            if(usecomponentmodel)
                sb.AppendLine("using System.ComponentModel;");
            if(usedataannotations)
                sb.AppendLine("using System.ComponentModel.DataAnnotations;");

            if(disablenullmessages)
                sb.AppendLine("#pragma warning disable CS8618");

            int namespaceindent = 0;
            if(ns != "") 
            {
                sb.AppendLine($"namespace {ns}");
                sb.AppendLine("{");
                namespaceindent = indent;
            }

            sb.AppendLine(new string(' ', namespaceindent) + $"public class {classname}");
            sb.AppendLine(new string(' ', namespaceindent) + "{");

            sb.Append(fieldtext);

            sb.AppendLine(new string(' ', namespaceindent) + "}");
            if(ns != "") 
            {
                sb.AppendLine("}");
            }
            return sb.ToString();
        }

        public string GetClassFieldText(
            string name, 
            string type = "System.String",
            string desc = "",
            string caption = "",
            int maxsize = 0,
            string required = "",
            bool ispkey = false,
            bool setdefaultgetset = false,
            int indent = 4)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            if(ispkey)
                sb.AppendLine(new string(' ',indent)+"[Key]");
            if(desc != "")
                sb.AppendLine(new string(' ',indent)+@$"[Description(""{desc}"")]");
            if(required != "")
                sb.AppendLine(new string(' ',indent)+@$"[Required(ErrorMessage = ""{required}"")]");
            if(caption != "")
                sb.AppendLine(new string(' ',indent)+@$"[Display(Name = ""{caption}"")]");
            if(maxsize > 0 )
                sb.AppendLine(new string(' ',indent)+@$"[MaxLength({maxsize})]");
            sb.AppendLine(new string(' ',indent)+$"public {this.GetShortType(type)} {name} "+"{get;set;}");
            return sb.ToString();
        }



    }
    
    public class actionhandler : generichandler
    {
        
        public override bool Run()
        {
            bool output = true;

            string action = "";
            var checkcategory = this.ps.FirstOrDefault(x => x.isactive.Equals(true));
            if(checkcategory == null)
            {
                this.lastmessage = "Cannot determine action to run.  For more information, enter: csgen --help";
                return false;
            }
            action = checkcategory!.category;
            // if(action == "vm")
            // {
            //     vmhandler handler = new vmhandler();
            //     handler.ps = this.ps;
            //     return handler.Run();
            // }


            return output;
        }


       

    }

/*    
#pragma warning disable CS8620
    public class vmhandler : classhandler
    {
        public vmoptions vmmodel = new vmoptions();
        
        public int indent = 4;
        public override bool Run()
        {
            if(!this.CreateModel())
                return false;
            if(!this.BuildOutput())
                return false;
            return true;
        }

        public bool BuildOutput()
        {
            if(this.vmmodel.vmfields.Count==0)
                return false;

            // add fields here - placing the pkey first
            // note that this is verified when the model is created
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            int classindent = this.indent;
            if(this.vmmodel.vmnamespace != "")
                classindent += this.indent;

            var pkey = this.vmmodel.vmfields.FirstOrDefault(x => x.ispkey.Equals(true));
            if(pkey != null)
            {
                sb.Append(this.GetClassFieldText(
                    pkey.vmfieldname,
                    pkey.vmfieldtype,
                    pkey.vmfielddesc,
                    pkey.vmfieldcap,
                    pkey.vmfieldsize,
                    pkey.vmfieldreq,
                    true,
                    false,
                    classindent));
                sb.AppendLine();
            }
            foreach(var field in this.vmmodel.vmfields)
            {
                if(!field.ispkey)
                {
                    sb.Append(this.GetClassFieldText(
                        field.vmfieldname,
                        field.vmfieldtype,
                        field.vmfielddesc,
                        field.vmfieldcap,
                        field.vmfieldsize,
                        field.vmfieldreq,
                        false,
                        false,
                        classindent));
                    sb.AppendLine();
                }
            }

            string text = this.GetClassText(this.vmmodel.vmname,this.vmmodel.vmnamespace,sb.ToString(),this.indent);

            bool success = true;
            string destfile = this.vmmodel.destfile;
            if(destfile == "")
                destfile = this.vmmodel.vmname + ".cs";

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
                this.lastmessage = ($"Could not write to file '{destfile}'.");
                return false;
            }
            
            return true;
        }


        public bool CreateModelOld()
        {

            bool success = true;
            vmoptions setup = new vmoptions();
            setup.vmname = "newviewmodel"; 
            var ph = new parameterhander();
            var checkname = this.ps.FirstOrDefault(x => x.isactive.Equals(true) && (x.setting.Equals("--name") || x.synonym.Equals("-n")));
            if(checkname != null)
                setup.vmname = checkname.nextinput;
            var checknamespace = this.ps.FirstOrDefault(x => x.isactive.Equals(true) && (x.setting.Equals("--namespace") || x.synonym.Equals("-p")));
            if(checknamespace != null)
                setup.vmnamespace = checknamespace.nextinput;

            var checkcsv = this.ps.FirstOrDefault(x => x.isactive.Equals(true) && (x.setting.Equals("--sourcefile") || x.synonym.Equals("-s")));
            if(checkcsv != null)
            {
                // load from a CSV first
                csvhandler csv = new csvhandler();
                System.Data.DataSet ds = new System.Data.DataSet();
                try
                {
                    ds = csv.GetDataSetFromCsv(checkcsv.nextinput);
                }
                catch
                {
                    success = false;
                }
                if(!success)
                {
                    this.lastmessage = $"Could not load ViewModel field information from source file '{checkcsv.nextinput}'.";
                    return false;
                }
                string validatetext = "";
                int vmnamecol = -1;
                int vmfieldnamecol = -1;
                int vmfieldtypecol = -1;
                int vmfieldsizecol = -1;
                int vmfielddesccol = -1;
                int vmfieldreqcol = -1;
                int vmfieldcapcol = -1;
                string vmfieldname = "";
                string vmfieldtype = "";
                string vmfieldsize = "";
                string vmfielddesc = "";
                string vmfieldreq = "";
                string vmfieldcap = "";
                int rowcount = 0;

                if(ds!.Tables[0].Columns.Contains("vmname"))
                    vmnamecol = ds!.Tables[0].Columns["vmname"]!.Ordinal;
                if(ds!.Tables[0].Columns.Contains("vmfieldname"))
                    vmfieldnamecol = ds!.Tables[0].Columns["vmfieldname"]!.Ordinal;
                if(ds!.Tables[0].Columns.Contains("vmfieldtype"))
                    vmfieldtypecol = ds!.Tables[0].Columns["vmfieldtype"]!.Ordinal;
                if(ds!.Tables[0].Columns.Contains("vmfieldsize"))
                    vmfieldsizecol = ds!.Tables[0].Columns["vmfieldsize"]!.Ordinal;
                if(ds!.Tables[0].Columns.Contains("vmfielddesc"))
                    vmfielddesccol = ds!.Tables[0].Columns["vmfielddesc"]!.Ordinal;
                if(ds!.Tables[0].Columns.Contains("vmfieldreq"))
                    vmfieldreqcol = ds!.Tables[0].Columns["vmfieldreq"]!.Ordinal;
                if(ds!.Tables[0].Columns.Contains("vmfieldcap"))
                    vmfieldcapcol = ds!.Tables[0].Columns["vmfieldcap"]!.Ordinal;

                foreach(System.Data.DataRow dr in ds!.Tables[0].Rows)
                {
                    if(vmnamecol >= 0)
                    {
                        if(dr[vmnamecol].ToString() != setup.vmname)
                            continue;
                    }
                    rowcount++;
                    vmfieldname = "";
                    vmfieldtype = "";
                    vmfieldsize = "";
                    vmfielddesc = "";
                    vmfieldreq = "";
                    vmfieldcap = "";
                    if(vmfieldnamecol >= 0)
                        vmfieldname = dr[vmfieldnamecol].ToString()!;
                    if(vmfieldtypecol >= 0)
                        vmfieldtype = dr[vmfieldtypecol].ToString()!;
                    if(vmfieldsizecol >= 0)
                        vmfieldsize = dr[vmfieldsizecol].ToString()!;
                    if(vmfielddesccol >= 0)
                        vmfielddesc = dr[vmfielddesccol].ToString()!;
                    if(vmfieldreqcol >= 0)
                        vmfieldreq = dr[vmfieldreqcol].ToString()!;
                    if(vmfieldcapcol >= 0)
                        vmfieldcap = dr[vmfieldcapcol].ToString()!;
                    
                    validatetext = ph.ValidateParameter(vmfieldname,"ViewModel Field Name",ParameterType.CsFieldName);
                    if(validatetext != "")
                    {
                        this.lastmessage = $"File '{checkcsv.nextinput}' row {rowcount}.  "+validatetext;
                        return false;
                    }
                    vmfieldtype = ph.FixFieldTypeCs(vmfieldtype);
                    vmfieldsize = new String(('0' + vmfieldsize).Where(Char.IsDigit).ToArray());
                    var newitem = new vmfield(){vmfieldname=vmfieldname,vmfieldtype=vmfieldtype,vmfieldsize=Int32.Parse(vmfieldsize),vmfielddesc=vmfielddesc,vmfieldreq=vmfieldreq,vmfieldcap=vmfieldcap};
                    setup.vmfields.Add(newitem);
                }
            }

            // now load fields from parameters
            var checkfieldnames = this.ps.FirstOrDefault(x => x.isactive.Equals(true) && (x.setting.Equals("--fieldnames") || x.synonym.Equals("-vf")));
            var checkfieldtypes = this.ps.FirstOrDefault(x => x.isactive.Equals(true) && (x.setting.Equals("--fieldtypes") || x.synonym.Equals("-vt")));
            var checkfieldsizes = this.ps.FirstOrDefault(x => x.isactive.Equals(true) && (x.setting.Equals("--fieldsizes") || x.synonym.Equals("-vz")));
            var checkfielddescs = this.ps.FirstOrDefault(x => x.isactive.Equals(true) && (x.setting.Equals("--fielddescs") || x.synonym.Equals("-vc")));
            var checkfieldreqs = this.ps.FirstOrDefault(x => x.isactive.Equals(true) && (x.setting.Equals("--fieldreqs") || x.synonym.Equals("-vq")));
            var checkfieldcaps = this.ps.FirstOrDefault(x => x.isactive.Equals(true) && (x.setting.Equals("--fieldcaps") || x.synonym.Equals("-va")));
            if(checkfieldnames != null)
            {
                // we can try to load these.  Note that these have already been validated
                string[] vmfieldnames = checkfieldnames.nextinput.Split(checkfieldnames.nextparaseparator,StringSplitOptions.None);
                for(int i = 0; i<vmfieldnames.Length;i++)
                {
                    var newitem = new vmfield(){vmfieldname=vmfieldnames[i]};
                    if(checkfieldtypes != null)
                    {
                        string[] checkarray = checkfieldtypes.nextinput.Split(checkfieldtypes.nextparaseparator,StringSplitOptions.None);
                        if(checkarray.Length>=i)
                            newitem.vmfieldtype = ph.FixFieldTypeCs(checkarray[i]);
                    }
                    if(checkfieldsizes != null)
                    {
                        string[] checkarray = checkfieldsizes.nextinput.Split(checkfieldsizes.nextparaseparator,StringSplitOptions.None);
                        if(checkarray.Length>=i)
                            newitem.vmfieldsize = Int32.Parse(new String(('0' + checkarray[i]).Where(Char.IsDigit).ToArray()));
                    }
                    if(checkfielddescs != null)
                    {
                        string[] checkarray = checkfielddescs.nextinput.Split(checkfielddescs.nextparaseparator,StringSplitOptions.None);
                        if(checkarray.Length>=i)
                            newitem.vmfielddesc = checkarray[i];
                    }
                    if(checkfieldreqs != null)
                    {
                        string[] checkarray = checkfieldreqs.nextinput.Split(checkfieldreqs.nextparaseparator,StringSplitOptions.None);
                        if(checkarray.Length>=i)
                            newitem.vmfieldreq = checkarray[i];
                    }
                    if(checkfieldcaps != null)
                    {
                        string[] checkarray = checkfieldcaps.nextinput.Split(checkfieldcaps.nextparaseparator,StringSplitOptions.None);
                        if(checkarray.Length>=i)
                            newitem.vmfieldcap = checkarray[i];
                    }
                    setup.vmfields.Add(newitem);
                }
            }

            // now remove duplicates
            var distinct = setup.vmfields.GroupBy(x => x.vmfieldname).Select(y => y.FirstOrDefault()).ToList();
            if(distinct.Count != setup.vmfields.Count)
            {
                setup.vmfields.Clear();
                setup.vmfields.AddRange(distinct);
            }

            // now find the primary key
            var checkpkey = this.ps.FirstOrDefault(x => x.isactive.Equals(true) && (x.setting.Equals("--key") || x.synonym.Equals("-k")));
            if(checkpkey != null)
            {
                var checkvmkeyfield = setup.vmfields.FirstOrDefault(x => x.vmfieldname.ToLower().Equals(checkpkey.nextinput.ToLower()));
                if(checkvmkeyfield != null)
                {
                    checkvmkeyfield.ispkey = true;
                    setup.vmpkey = checkvmkeyfield!.vmfieldname;
                }
            }

            // set the destination file
            var checkdestfile = this.ps.FirstOrDefault(x => x.isactive.Equals(true) && (x.setting.Equals("--output") || x.synonym.Equals("-o")));
            if(checkdestfile != null)
                setup.destfile = checkdestfile!.nextinput;

            this.vmmodel = setup;
            return true;
        }
    }
    */
}

