
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
    /// Manages and read csgen-specific parameters
    /// </summary>
    public class CsgenMvvm
    {
        public string lastmessage="";
        public mvvm mv = new mvvm();


        public bool SetFullModel(List<ParameterSetting> ps)
        {
            // the mvvm fullmodel is all information passed-in
            var output = new mvvm();
            var commafields = this.GetCommaFields();

            // if there is a CSV file specified, grab these first
            List<mvvmfield> csvfields = new List<mvvmfield>();
            List<mvvmfield> parafields = new List<mvvmfield>();
            var checkcsv = ps.FirstOrDefault(x => x.isactive.Equals(true) && (x.setting.Equals("--sourcefile") || x.synonym.Equals("-s")));
            if(checkcsv != null)
            {
                // load from a CSV first
                CsvToDataSet csv = new CsvToDataSet();
                System.Data.DataSet ds = new System.Data.DataSet();
                bool success = true;
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
                    this.lastmessage = $"Could not load MVVM field information from source file '{checkcsv.nextinput}'.";
                    return false;
                }

                foreach(var commafield in commafields)
                    if(ds!.Tables[0].Columns.Contains(commafield.singlename))
                        commafield.csvcolnum = ds!.Tables[0].Columns[commafield.singlename]!.Ordinal;

                int rowcount = 0;
                foreach(System.Data.DataRow dr in ds!.Tables[0].Rows)
                {
                    mvvmfield newfield = new mvvmfield();
                    Type type = newfield.GetType();
                    foreach(var commafield in commafields.Where( x=>x.csvcolnum>=0))
                    {
                        System.Reflection.PropertyInfo? prop = type.GetProperty(commafield.singlename);
                        if(prop != null)
                            prop.SetValue(newfield, dr.ItemArray[commafield.csvcolnum]);
                    }
                    csvfields.Add(newfield);
                    rowcount++;
                }
            }

            // now load the equivalent of the CSV data from parameters - these are appended
            int maxrows = 0;
            foreach(var checkfield in commafields) // .Where(x => x.isfilter.Equals(false))
            {
                // did the user specify some field information?
                // if so, we need to find the fieldtype with the most comma-separated field values
                // this is the number of rows in the output
                var checkvalue = ps.FirstOrDefault(x => x.isactive.Equals(true) && (x.setting.Equals("--"+checkfield.pluralname) || x.synonym.Equals("-"+checkfield.synonym)));
                if(checkvalue != null)
                {
                    int fieldcount = checkvalue.nextinput.Split(checkvalue.nextparaseparator,StringSplitOptions.None).Length;
                    if(fieldcount > maxrows)
                        maxrows = fieldcount;
                }
            }
            if(maxrows>0)
            {
                // find the passed-in valus for filtering purposes
                // var checkvalue = ps.FirstOrDefault(x => x.isactive.Equals(true) && (x.setting.Equals("--"+checkfield.pluralname) || x.synonym.Equals("-"+checkfield.synonym)));

                // append some data to the CSV-loaded rows
                for(int i = 0; i<maxrows;i++)
                {
                    mvvmfield newfield = new mvvmfield();
                    Type type = newfield.GetType();
                    // check each potential match - this is inefficient but thorough
                    foreach(var checkfield in commafields) // .Where(x => x.isfilter.Equals(false))
                    {
                        foreach(var parasetting in ps.Where(x => x.isactive.Equals(true) && (x.setting.Equals("--"+checkfield.pluralname) || x.synonym.Equals("-"+checkfield.synonym))))
                        {
                            string[] checkvalues = parasetting.nextinput.Split(parasetting.nextparaseparator,StringSplitOptions.None);
                            if(i < checkvalues.Length)
                            {
                                System.Reflection.PropertyInfo? prop = type.GetProperty(checkfield.singlename);
                                if(prop != null)
                                    prop.SetValue(newfield, checkvalues[i].Trim());
                            }
                        }
                    }
                    parafields.Add(newfield);
                }
            }

            // note that only the first parafield can have any of the filters (because these are not comma-separated)
            if(parafields.Count>1)
            {
                Type type = parafields[0].GetType();
                foreach(var checkfield in commafields.Where(x => x.isfilter.Equals(true)))
                {
                    foreach(var parafield in parafields)
                    {
                        System.Reflection.PropertyInfo? prop = type.GetProperty(checkfield.singlename);
                        if(prop != null)
                            prop.SetValue(parafield, prop.GetValue(parafields[0]));
                    }

                }
            }


            // now add all other properties onto the output
            foreach(var prop in output.GetType().GetProperties())
            {
                var checkvalue = ps.FirstOrDefault(x => x.isactive.Equals(true) && (x.setting.Equals("--"+prop.Name)));
                if(checkvalue != null)
                {
                    prop.SetValue(output, checkvalue.nextinput);
                    if(output.category=="")
                        output.category = checkvalue.category.Trim().ToLower();
                }
            }

            // now filter any of the CSV records that are not
            if(output.category=="vm")
            {
                // if at least one record has a name then filter, otherwise take them all
                if(csvfields.Exists(x => x.vname.Equals(output.vname)))
                {
                    output.genfields.AddRange(csvfields.Where(x => x.vname.Equals(output.vname) && x.vfname != "" && x.vftype != ""));
                }
                else
                {
                    output.genfields.AddRange(csvfields.Where(x => x.vfname != "" && x.vftype != ""));
                }
            }
            else if(output.category=="model")
            {
                if(csvfields.Exists(x => x.mname.Equals(output.mname)))
                {
                    output.genfields.AddRange(csvfields.Where(x => x.mname.Equals(output.mname) && x.mfname != "" && x.mftype != ""));
                }
                else
                {
                    output.genfields.AddRange(csvfields.Where(x => x.mfname != "" && x.mftype != ""));
                }
            }
            
            // add both the CSV and parameter items to the output
            output.genfields.AddRange(parafields);


            this.mv = output;

            // now decorate with category-specific things




            //var distinct = parafields.GroupBy(x => x.mname).Select(y => y.FirstOrDefault()).ToList();
            // if(distinct.Count != 0)
            // {
            //     output.genfields.AddRange(parafields);
            // }


            // we now have ALL fieldmodel - we need to filter on the ones we need

            /*

            var distinct = allfields.GroupBy(x => x.vmfieldname).Select(y => y.FirstOrDefault()).ToList();
            if(distinct.Count != setup.vmfields.Count)
            {
                setup.vmfields.Clear();
                setup.vmfields.AddRange(distinct);
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

            */



            return true;
        }

        public bool CreateFile()
        {
            return this.CreateFile(this.mv);
        }
        public bool CreateFile(mvvm source)
        {
            if(source.category=="")
            {
                this.lastmessage = "No category of output has been entered.";
                return false;
            }
            string categoryname = "ViewModel";
            if(source.category!="vm")
                categoryname = source.category[0].ToString().ToUpper()+(source.category+"  ").Substring(1,source.category.Length).Trim().ToLower();

            if(source.genfields.Count==0)
            {
                this.lastmessage = $"Creating a {categoryname} requires at least one field to be entered.";
                return false;
            }
            if(source.vname=="" && (source.category=="vm" || source.category=="view"  || source.category=="facade"  || source.category=="controller" ))
            {
                this.lastmessage = $"Creating a {categoryname} requires a ViewModel name.";
                return false;
            }
            if(source.mname=="" && (source.category=="facade" || source.category=="model" ))
            {
                this.lastmessage = $"Creating a {categoryname} requires a Model name.";
                return false;
            }

            int indent = 4;
            int currentindent = 0;

            // add fields here - placing the pkey first
            // note that this is verified when the model is created
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            int classindent = indent;
            if(source.category == "model" && source.mnamespace != "")
                classindent += indent;
            if(source.category == "vm" && source.vnamespace != "")
                classindent += indent;

            // put the primary key at the top
            foreach(var field in source.genfields)
            {
                if(source.mpkey!="" && source.category=="model" && field.mfname==source.mpkey)
                {
                    sb.Append(this.GetClassSingleFieldText(
                        field.mfname,
                        field.mftype,
                        "",
                        "",
                        "",
                        "",
                        true,
                        false,
                        classindent));
                    sb.AppendLine();
                }
                else if(source.vpkey!="" && source.category=="vm" && field.vfname==source.vpkey)
                {
                    sb.Append(this.GetClassSingleFieldText(
                        field.vfname,
                        field.vftype,
                        field.vfdesc,
                        field.vfcap,
                        field.vfsize,
                        field.vfreq,
                        true,
                        false,
                        classindent));
                    sb.AppendLine();
                }
            }

            // next put in the other fields
            foreach(var field in source.genfields)
            {
                if(source.category=="model" && field.mfname != source.mpkey)
                {
                    sb.Append(this.GetClassSingleFieldText(
                        field.mfname,
                        field.mftype,
                        "",
                        "",
                        "",
                        "",
                        false,
                        false,
                        classindent));
                    sb.AppendLine();
                }
                else if(source.category=="vm" && field.vfname != source.vpkey)
                {
                    sb.Append(this.GetClassSingleFieldText(
                        field.vfname,
                        field.vftype,
                        field.vfdesc,
                        field.vfcap,
                        field.vfsize,
                        field.vfreq,
                        false,
                        false,
                        classindent));
                    sb.AppendLine();
                }
            }

            string text = "";
            if(source.category=="model")
            {
                text = this.GetClassText(source.mname,source.mnamespace,sb.ToString(),currentindent);
            }
            else if(source.category=="vm")
            {
                text = this.GetClassText(source.vname,source.vnamespace,sb.ToString(),currentindent);
            }
            else if(source.category=="view")
            {
                text = this.GetViewText(source,indent);
            }
            else if(source.category=="controller")
            {
                text = this.GetControllerText(source,indent);
            }
            else if(source.category=="facade")
            {
                text = this.GetFacadeText(source,indent);
            }



            // now create the file
            bool success = true;
            string destfile = source.output;
            string extension = "cs";
            if(source.category=="view")
                extension = "cshtml";
            if(destfile == "")
                destfile = "new"+categoryname.ToLower() + "." + extension;
            if(!destfile.ToLower().EndsWith("cshtml") && !destfile.ToLower().EndsWith("cs"))
                destfile = destfile + "." + extension;

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
            int indent = 4,
            bool usecomponentmodel = true,
            bool usedataannotations = true,
            bool disablenullmessages = true
            )
        {

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            // sb.AppendLine("using System;");
            if(usecomponentmodel)
                sb.AppendLine("using System.ComponentModel;");
            if(usedataannotations)
                sb.AppendLine("using System.ComponentModel.DataAnnotations;");
            if(disablenullmessages)
                sb.AppendLine("#pragma warning disable CS8618");

            int namespaceindent = 4;
            if(ns != "") 
            {
                sb.AppendLine($"namespace {ns}");
                sb.AppendLine("{");
                indent += namespaceindent;
            }
            sb.AppendLine(new string(' ', indent) + $"public class {classname}");
            sb.AppendLine(new string(' ', indent) + "{");
            sb.Append(fieldtext);
            sb.AppendLine(new string(' ', indent) + "}");
            if(ns != "") 
            {
                sb.AppendLine("}");
            }
            return sb.ToString();
        }

        public string GetClassSingleFieldText(
            string name, 
            string type = "System.String",
            string desc = "",
            string caption = "",
            string maxsize = "0",
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
            if(maxsize != "0" && maxsize != "" )
                sb.AppendLine(new string(' ',indent)+@$"[MaxLength({maxsize})]");
            sb.AppendLine(new string(' ',indent)+$"public {this.GetShortType(type)} {name} "+"{get;set;}");
            return sb.ToString();
        }

        public string GetViewText(mvvm source, int indent)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            return sb.ToString();
        }
        public string GetControllerText(mvvm source, int indent)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            return sb.ToString();
        }
        public string GetFacadeText(mvvm source, int indent)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            return sb.ToString();
        }

        public List<commafield> GetCommaFields()
        {
            return new List<commafield>(){
                new commafield(){
                    singlename="vname",
                    pluralname="vnames",
                    synonym="vn",
                    isfilter = true
                },
                new commafield(){
                    singlename="mname",
                    pluralname="mnames",
                    synonym="mn",
                    isfilter = true
                },
                new commafield(){
                    singlename="wname",
                    pluralname="wnames",
                    synonym="wn",
                    isfilter = true
                },
                new commafield(){
                    singlename="vfname",
                    pluralname="vfnames",
                    synonym="vf"
                },
                new commafield(){
                    singlename="vftype",
                    pluralname="vftypes",
                    synonym="vt"
                },
                new commafield(){
                    singlename="vfsize",
                    pluralname="vfsizes",
                    synonym="vz"
                },
                new commafield(){
                    singlename="vfdesc",
                    pluralname="vfdescs",
                    synonym="vc"
                },
                new commafield(){
                    singlename="vfreq",
                    pluralname="vfreqs",
                    synonym="vq"
                },
                new commafield(){
                    singlename="vfcap",
                    pluralname="vfcaps",
                    synonym="va"
                },
                new commafield(){
                    singlename="mfname",
                    pluralname="mfnames",
                    synonym="mf"
                },
                new commafield(){
                    singlename="mftype",
                    pluralname="mftypes",
                    synonym="mt"
                },
                new commafield(){
                    singlename="wfclass",
                    pluralname="wfclasses",
                    synonym="we"
                },
                new commafield(){
                    singlename="wftype",
                    pluralname="wftypes",
                    synonym="wy"
                },
                new commafield(){
                    singlename="wfdclass",
                    pluralname="wfdclasses",
                    synonym="wd"
                },
                new commafield(){
                    singlename="wficlass",
                    pluralname="wficlasses",
                    synonym="wi"
                }
            };
        }

    }

    public class commafield
    {
        public string singlename = "";
        public string pluralname = "";
        public string synonym = "";
        public int csvcolnum = -1;
        public bool isfilter = false;

    }

    public class mvvmfield
    {
        public string vname {get;set;} = "";  // ViewModel name.
        public string mname {get;set;} = "";  // Model name.
        public string wname {get;set;} = "";  // View name.

        public bool ispkey {get;set;} = false;
        public bool ismodel {get;set;} = false;
        public bool isvm {get;set;} = false;

        public string mfname {get;set;} = "";  // Comma-separated list of Model field names in order.");
        public string mftype {get;set;} = "";  //Comma-separated list of Model field types in order.");

        public string wfclass {get;set;} = "";  // Comma-separated list of View form field CSS classes in order.");
        public string wftype {get;set;} = "";   // Comma-separated list of View form field HTML types in order.");
        public string wfdclass {get;set;} = "";  // Comma-separated list of colon-delimited CSS classes in order to wrap a form field in <div> tags.");
        public string wficlass {get;set;} = "";  // Comma-separated list of a CSS class for an <i> tag that follows a form field.");

        public string vfname {get;set;} = "";  // Comma-separated list of ViewModel field names in order.
        public string vftype {get;set;} = "";  // Comma-separated list of ViewModel field types in order.
        public string vfsize {get;set;} = "";  // Comma-separated list of ViewModel field sizes in order.
        public string vfdesc {get;set;} = "";  // Comma-separated list of ViewModel field descriptions in order.
        public string vfreq {get;set;} = "";  // Comma-separated list of ViewModel field required text in order.
        public string vfcap {get;set;} = "";  // Comma-separated list of ViewModel field captions in order.

    }


    public class controlleraction
    {

    }

/// This is setup to create ONE file but it needs HEAPS of tangential infomration to do this
    public class mvvm
    {
        public string category {get;set;} = "";
        public string output {get;set;} = "";  //  Full path to output .cs file.
        public string sourcefile {get;set;} = "";  // Loads field properties from a CSV file.
        // public string fieldprefix = "";
        public string vname {get;set;} = "";
        public string vnamespace {get;set;} = "";

        public string mname {get;set;} = "";  // Model name.
        public string mnamespace {get;set;} = "";  // Model namespace.
        public string wname {get;set;} = "";  // View name.

        public string mpkey {get;set;} = "";  // Specifies the primary key field in the Model.
        public string mfkey {get;set;} = "";  // Specifies the foreign key field in the Model.
        public string mftable {get;set;} = "";  // Specifies the parent of the Model.
        public string vpkey {get;set;} = "";  // Specifies the primary key field in the ViewModel.
        public string vfkey {get;set;} = "";  // Specifies the foreign key field in the ViewModel.
        public string vftable {get;set;} = "";  // Specifies the parent of the ViewModel.
        public string vuserkey {get;set;} = "";  // Specifies the userid field in the ViewModel.
        public string vmessage {get;set;} = "";  // Specifies a field in the ViewModel to relay messages.

        public List<mvvmfield> genfields = new List<mvvmfield>();
        public List<controlleraction> controlleractions = new List<controlleraction>();
        public List<string> parentmodels = new List<string>();
        public List<string> childmodels = new List<string>();
    }



}