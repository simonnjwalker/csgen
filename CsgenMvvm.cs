
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
            List<commafield> commafields = this.GetCommaFields();

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

                foreach(var commafield in commafields.Where(x => x.ismvvmfield.Equals(true)))
                    if(ds!.Tables[0].Columns.Contains(commafield.singlename))
                        commafield.csvcolnum = ds!.Tables[0].Columns[commafield.singlename]!.Ordinal;

                int rowcount = 0;
                foreach(System.Data.DataRow dr in ds!.Tables[0].Rows)
                {
                    mvvmfield newfield = new mvvmfield();
                    Type type = newfield.GetType();
                    foreach(var commafield in commafields.Where( x=>x.csvcolnum>=0 && x.ismvvmfield.Equals(true)))
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
            foreach(var checkfield in commafields.Where(x => x.ismvvmfield.Equals(true))) // .Where(x => x.isfilter.Equals(false))
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
                    foreach(var checkfield in commafields.Where(x => x.ismvvmfield.Equals(true))) // .Where(x => x.isfilter.Equals(false))
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
                    if(prop.PropertyType.FullName.Equals("System.Boolean"))
                    {
                        if(checkvalue.nextinput.Trim().ToLower().StartsWith('t') ||
                            checkvalue.nextinput.Trim().ToLower().StartsWith('1') ||
                            checkvalue.nextinput.Trim().ToLower().StartsWith('y') ||
                            checkvalue.paratype.Equals(ParameterType.Switch))
                        {
                            prop.SetValue(output, true);
                        }
                        else
                        {
                            prop.SetValue(output, false);
                        }
                    }
                    else
                    {
                        prop.SetValue(output, checkvalue.nextinput);
                    }
                    if(output.category=="")
                        output.category = checkvalue.category.Trim().ToLower();
                }
            }
            output.categoryname = "ViewModel";
            if(output.category!="vm")
                output.categoryname = output.category[0].ToString().ToUpper()+(output.category+"  ").Substring(1,output.category.Length).Trim().ToLower();

            // now filter any of the CSV records that are not required
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
            else if(output.category=="controller" || output.category=="facade")
            {
                // 
                // create the controller-specific fields
                // the reason that these exist are to make creation of the Controller.cs file easier
                List<controlleraction> ctrlfields = new List<controlleraction>();
                var actions = output.cactnames.Split(':',StringSplitOptions.None);
                for(int i=0; i<actions.Length; i++)
                    ctrlfields.Add(new controlleraction(){cname=actions[i]});

                if(ctrlfields[0].cname.Trim() != "")
                {
                    Type maintype = output.GetType();
                    Type ctrltype = ctrlfields[0].GetType();
                    // now go through each controller property on the output object
                    // and assign it to the relevant row in the output list<> 
                    //
                    // this is a little complex, but basically for the csgen controller 
                    // command there are heaps of colon-delimited fields (one per action)
                    foreach(var checkfield in commafields.Where(x => x.isfilter.Equals(false) && x.isctrlaction.Equals(true)))
                    {
                        foreach(var property in maintype.GetProperties())
                        {
                            System.Reflection.PropertyInfo? prop = maintype.GetProperty(checkfield.pluralname);
                            if(prop != null)
                            {
                                // see if this contains any text
                                string checkprop = (string)(prop.GetValue(output) ?? "");
                                var actionprops = checkprop.Split(':',StringSplitOptions.None);
                                for(int i=0; i<actions.Length; i++)
                                {
                                    if(actionprops.Length>i)
                                    {
                                        // get the value
                                        string actionpropvalue = actionprops[i];

                                        // set this on the relevant list<> item
                                        var ctrlitem = ctrlfields[i];
                                        System.Reflection.PropertyInfo? ctrlprop = ctrltype.GetProperty(checkfield.singlename);
                                        if(ctrlprop != null)
                                            ctrlprop.SetValue(ctrlitem, actionpropvalue);
                                    }
                                }
                            }
                        }
                    }
                }

                // we add ALL fields for controllers and facade
                output.controlleractions.AddRange(ctrlfields);
            }

            // add both the CSV and parameter items to the output
            output.genfields.AddRange(parafields);

            foreach(var genfield in output.genfields)
            {
                if(genfield.mname == "" & genfield.mfname != "")
                    genfield.mname = output.mname;
                if(genfield.vname == "" & genfield.vfname != "")
                    genfield.vname = output.vname;
            }
            if(output.fillblanks)
            {
                // Where the first item in a child has text in fields,
                // use that text is ALL subsequent children where that field is blank
                Type maintype = output.GetType();
                if(output.genfields.Count>1)
                {
                    Type mvvmfieldtype = output.genfields[0].GetType();
                    foreach(var mvvmfieldprop in mvvmfieldtype.GetProperties())
                    {
                        if(mvvmfieldprop.PropertyType.FullName != "System.String")
                            continue;
                        string firstvalue = (string)(mvvmfieldprop.GetValue(output.genfields[0]) ?? "");
                        if(firstvalue != "")
                        {
                            foreach(var genfield in output.genfields)
                            {
                                // if this is blank, set it to the first value
                                string currentvalue = (string)(mvvmfieldprop.GetValue(genfield) ?? "");
                                if(currentvalue == "")
                                        mvvmfieldprop.SetValue(genfield, firstvalue);
                            }
                        }
                    }
                }
                if(output.controlleractions.Count>1)
                {
                    Type ctrlfieldtype = output.controlleractions[0].GetType();
                    foreach(var ctrlfieldprop in ctrlfieldtype.GetProperties())
                    {
                        if(ctrlfieldprop.PropertyType.FullName != "System.String")
                            continue;
                        string firstvalue = (string)(ctrlfieldprop.GetValue(output.controlleractions[0]) ?? "");
                        if(firstvalue != "")
                        {
                            foreach(var ctrlfield in output.controlleractions)
                            {
                                // if this is blank, set it to the first value
                                string currentvalue = (string)(ctrlfieldprop.GetValue(ctrlfield) ?? "");
                                if(currentvalue == "")
                                        ctrlfieldprop.SetValue(ctrlfield, firstvalue);
                            }
                        }
                    }
                }
            }



            this.mv = output;

            return true;
        }
        public bool CreateFile()
        {
            return this.CreateFile(this.mv);
        }
        public bool ValidateSource(mvvm source)
        {
            if(source.category=="")
            {
                this.lastmessage = "No category of output has been entered.";
                return false;
            }
            if(source.genfields.Count==0)
            {
                this.lastmessage = $"Creating a {source.categoryname} requires at least one field to be entered.";
                return false;
            }
            if(source.vname=="" && (source.category=="vm" || source.category=="view"  || source.category=="facade" ))
            {
                this.lastmessage = $"Creating a {source.categoryname} requires a ViewModel name.";
                return false;
            }
            if(source.mname=="" && (source.category=="facade" || source.category=="model" ))
            {
                this.lastmessage = $"Creating a {source.categoryname} requires a Model name.";
                return false;
            }
            if(source.cname=="" && (source.category=="controller"  ))
            {
                this.lastmessage = $"Creating a {source.categoryname} requires a Controller action name.";
                return false;
            }

            return true;
        }

        public bool CreateFile(mvvm source)
        {
            if(!this.ValidateSource(source))
                return false;

            string text = "";
            if(source.category=="model" || source.category=="vm")
            {
                text = this.GetClassOuterText(source, this.GetClassInnerText(source));
            }
            else if(source.category=="view")
            {
                text = this.GetViewOuterText(source, this.GetViewInnerText(source));
            }
            else if(source.category=="controller")
            {
                text = this.GetControllerOuterText(source, this.GetControllerInnerText(source));
            }
            else if(source.category=="facade")
            {
                text = this.GetFacadeText(source);
            }



            // now create the file
            bool success = true;
            string destfile = source.output;
            string extension = "cs";
            if(source.category=="view")
                extension = "cshtml";
            if(destfile == "")
                destfile = "new"+source.categoryname.ToLower() + "." + extension;
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

        public string GetViewInnerText(mvvm source)
        {
            // the outer is the same for all action types but the inside is structured differently
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            if(source.waction.ToLower().Trim() == "create" || source.waction.ToLower().Trim() == "edit" )
            {
                string formfieldstext = this.GetViewInnerFormFieldsText(source);
                sb.Append(this.GetViewFormText(source,formfieldstext));
            }
            if(source.waction.ToLower().Trim() == "index" )
            {
                sb.Append(this.GetViewInnerIndexTableText(source));
            }
            if(source.waction.ToLower().Trim() == "details" || source.waction.ToLower().Trim() == "delete")
            {
                string formfieldstext = this.GetViewInnerFormFieldsText(source);
                formfieldstext = formfieldstext.Replace(" asp-for", " disabled asp-for");
                sb.Append(this.GetViewFormText(source,formfieldstext));
            }

            return sb.ToString();
        }

        public string GetViewInnerIndexText(mvvm source)
        {
            // this will be a table - see lamod subject designer
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            foreach(var genfield in source.genfields)
                sb.Append(this.GetViewSingleFieldText(genfield));
            return sb.ToString();
        }

        public string GetViewInnerIndexTableText(mvvm source)
        {
            // this will be a table - see lamod subject designer
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            // the default viewmodel is a List<> so throw something together here
            // TO DO: fix this up by adding table-classes

            sb.AppendLine($"<table class=\"table\">");
            sb.AppendLine($"<thead>");
            sb.AppendLine($"<tr>");

            int fieldcount = 0;            
            foreach(var genfield in source.genfields)
            {
                if(genfield.wftype.Trim().ToLower() != "hidden")
                {
                    fieldcount++;
                    sb.AppendLine("<th>");
                    sb.AppendLine($"@Html.DisplayNameFor(model => model.{genfield.vfname})");
                    sb.AppendLine("</th>");
                }
            }
            sb.AppendLine($"<th>");
            sb.AppendLine($"</th>");
            sb.AppendLine($"</tr>");
            sb.AppendLine($"</thead>");
            sb.AppendLine($"<tbody>");
            sb.AppendLine("@foreach (var item in Model) {");
            sb.AppendLine($"<tr>");
            foreach(var genfield in source.genfields)
            {
                if(genfield.wftype.Trim().ToLower() != "hidden")
                {
                    sb.AppendLine("<td>");
                    sb.AppendLine($"@Html.DisplayFor(model => model.{genfield.vfname})");
                    sb.AppendLine("</td>");
                }
                sb.AppendLine($"<td>");
                sb.AppendLine($"<a asp-action=\"Edit\" asp-route-id=\"@item.{source.vpkey}\">Edit</a> |");
                sb.AppendLine($"<a asp-action=\"Details\" asp-route-id=\"@item.{source.vpkey}\">Details</a> |");
                sb.AppendLine($"<a asp-action=\"Delete\" asp-route-id=\"@item.{source.vpkey}\">Delete</a>" );
                sb.AppendLine($"</td>");
            }
            sb.AppendLine($"</tr>");
            sb.AppendLine($"</tbody>");
            sb.AppendLine($"</table>");


/*


        public string wfrmaction {get;set;} = "";  // Specifies the Form action.
        public string wfrmclass {get;set;} = "";  // Colon-delimited CSS classes wrapping the Form object.
        public string wfrmsub {get;set;} = "";  // Colon-delimited CSS classes wrapping all objects inside the Form object.


<table class="table">
    <thead>
        <tr>
            <th>
                @Html.DisplayNameFor(model => model.userid)
            </th>
            <th>
                @Html.DisplayNameFor(model => model.code)
            </th>
            <th>
                @Html.DisplayNameFor(model => model.name)
            </th>
            <th></th>
        </tr>
    </thead>
    <tbody>
@foreach (var item in Model) {
        <tr>
            <td>
                @Html.DisplayFor(modelItem => item.userid)
            </td>
            <td>
                @Html.DisplayFor(modelItem => item.code)
            </td>
            <td>
                @Html.DisplayFor(modelItem => item.name)
            </td>
            <td>
                <a asp-action="Edit" asp-route-id="@item.id">Edit</a> |
                <a asp-action="Details" asp-route-id="@item.id">Details</a> |
                <a asp-action="Delete" asp-route-id="@item.id">Delete</a>
            </td>
        </tr>
}
    </tbody>
</table>

<div id="assessment-list" class="collapse in">
    @if(Model.assessments.Count> 0)
    {
        @foreach(var assessment in Model.assessments)
        {
            {               
                @Html.Partial("_DesignAssessmentItem",assessment)
            }
        }
    }

    <div id="no-assessment-container">
        @if(Model.assessments.Count== 0)
        {
            <div id="no-assessment-text">There are no assessments.  Click the plus button above to add a new one.</div>
        }
    </div>
</div>

*/
            return sb.ToString();
        }        

        public string GetViewInnerFormFieldsText(mvvm source)
        {
            // the outer is the same for all action types but the inside is structured differently
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            foreach(var genfield in source.genfields)
                sb.Append(this.GetViewSingleFieldText(genfield));
            return sb.ToString();
        }
        public string GetViewFormText(mvvm source)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            foreach(var genfield in source.genfields)
                sb.Append(this.GetViewSingleFieldText(genfield));
            return sb.ToString();
        }
        public string GetViewFormText(mvvm source, string innertext)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            int enddiv = 0;
            string[] formclasses = source.wfrmclass.Split(':',StringSplitOptions.None);
            for (int i = 0; i < formclasses.Length; i++)
            {
                if(formclasses[i].Trim() != "")
                {
                    enddiv++;
                    sb.AppendLine($"<div class=\"{formclasses[i]}\">");
                }
            }
            sb.AppendLine(innertext);
            for (int i = 0; i < enddiv; i++)
                sb.AppendLine($"</div>");
            return sb.ToString();
        }

        public string GetViewSingleFieldText(mvvmfield genfield)
        {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                int enddiv = 0;
                string[] outerclasses = genfield.wfdclass.Split(':',StringSplitOptions.None);
                bool useouterclasses = (outerclasses[0].Trim() != "");

                if(useouterclasses)
                {
                    for (int i = 0; i < outerclasses.Length; i++)
                    {
                        if(outerclasses[i].Trim() != "")
                        {
                            enddiv++;
                            sb.AppendLine($"<div class=\"{outerclasses[i]}\">");
                        }
                    }
                }

                string inputtype = "text";
                if(genfield.wftype.Trim().ToLower() != "")
                    inputtype = genfield.wftype.Trim().ToLower();
                if(genfield.wftype.Trim().ToLower() != "textarea")
                {
                    sb.AppendLine($"<input asp-for=\"{genfield.vfname}\" name=\"{genfield.vname}.{genfield.vfname}\" class=\"{genfield.wfclass}\" type=\"{inputtype}\" placeholder=\"{genfield.vfcap}\">");
                }
                else
                {
                    int rows = this.GuessRowSize(genfield.vfsize); 
                    sb.AppendLine($"<textarea asp-for=\"{genfield.vfname}\" name=\"{genfield.vname}.{genfield.vfname}\" class=\"{genfield.wfclass}\" type=\"{inputtype}\" placeholder=\"{genfield.vfcap}\" rows=\"{rows}\"></textarea>");
                }

                if(genfield.wficlass.Trim() != "")
                    sb.AppendLine($"<i class=\"{genfield.wficlass}\"></i>");

                if(useouterclasses)
                {
                    for (int i = 0; i < enddiv; i++)
                        sb.AppendLine($"</div>");
                }
/*
                this is the target output:
                        <div class="col-md-6">
                            <div class="single-input">
                                <input asp-for="id" name="vmelection.id" class="form-input" type="hidden" value="1235678901234567890123456789012">
                                <i class="lni lni-user"></i>
                            </div>
                        </div>

                        <div class="col-md-6 single-input">
                            <input asp-for="name" name="vmelection.name" class="form-input" placeholder="Name">
                            <i class="lni lni-text-format"></i>
                        </div>
<input type="button">
<input type="checkbox">
<input type="color">
<input type="date">
<input type="datetime-local">
<input type="email">
<input type="file">
<input type="hidden">
<input type="image">
<input type="month">
<input type="number">
<input type="password">
<input type="radio">
<input type="range">
<input type="reset">
<input type="search">
<input type="submit">
<input type="tel">
<input type="text">
<input type="time">
<input type="url">
<input type="week">
*/


                    // helptext.Add("  -vf|--vfnames     Comma-separated list of ViewModel field names in order.");
                    // helptext.Add("                    Syntax is vfname1[,vfname2][,...].");
                    // helptext.Add("  -vt|--vftypes     Comma-separated list of ViewModel field types in order.");
                    // helptext.Add("                    Syntax is vftype1[,vftype2][,...].");
                    // helptext.Add("  -vz|--vfsizes     Comma-separated list of ViewModel field sizes in order.");
                    // helptext.Add("                    Syntax is vfsize1[,vfsize2][,...].");
                    // helptext.Add("  -vc|--vfdescs     Comma-separated list of ViewModel field descriptions in order.");
                    // helptext.Add("                    Syntax is vfdesc1[,vfdesc2][,...].");
                    // helptext.Add("  -vq|--vfreqs      Comma-separated list of ViewModel field required text in order.");
                    // helptext.Add("                    Syntax is vfreq1[,vfreqc2][,...].");
                    // helptext.Add("  -va|--vfcaps      Comma-separated list of ViewModel field captions in order.");
                    // helptext.Add("                    Syntax is vfcap1[,vfcap2][,...].");            

                    // helptext.Add("  -we|--wfclasses   Comma-separated list of View form field CSS classes in order.");
                    // helptext.Add("                    Syntax is wfclass1[,wfclass2][,...].");
                    // helptext.Add("  -wy|--wftypes     Comma-separated list of View form field HTML types in order.");
                    // helptext.Add("                    Syntax is wftype1[,wftype2][,...].");
                    // helptext.Add("  -wd|--wfdclasses  Comma-separated list of colon-delimited CSS classes in order to wrap a form field in <div> tags.");
                    // helptext.Add("                    Syntax is wfdclass1a:wfdclass1b[:wfdclass1c][,wfdclass2a][,...].");
                    // helptext.Add("  -wi|--wficlasses  Comma-separated list of a CSS class for an <i> tag that follows a form field.");
                    // helptext.Add("                    Syntax is wficlass1[,wficlass2][,...].");


        
            return sb.ToString();
        }

        public int GuessRowSize(string size)
        {
            int rows = 2;
            size = String.Join("", size.ToCharArray().Where(Char.IsDigit)).PadLeft(4,'0');
            int checksize = Int32.Parse(size);
            if(checksize >=50 && checksize <100)
                rows = 3;
            if(checksize >=100 && checksize <150)
                rows = 4;
            if(checksize >=150 && checksize <200)
                rows = 5;
            if(checksize >=200)
                rows = 6;
            return rows;
        }
        public string GetViewOuterText(mvvm source, string formtext)
        {
            // the outer text is the same for all views
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            if(source.vname != "")
            {
                if(source.waction.ToLower().Trim() == "index")
                {
                    sb.AppendLine($"@model List<{source.vname}>");
                }
                else
                {
                    sb.AppendLine($"@model {source.vname}");
                }
            }
            if(source.layout != "")
                sb.AppendLine("@{Layout = \"" + source.layout +"\";}");
            if(source.wpageclass != "")
                sb.AppendLine($"<div class=\"{source.wpageclass}\">");


            int enddiv = 0;
            string[] infoclasses = source.winfoclass.Split(':',StringSplitOptions.None);
            for (int i = 0; i < infoclasses.Length; i++)
            {
                if(infoclasses[i].Trim() != "")
                {
                    enddiv++;
                    sb.AppendLine($"<div class=\"{infoclasses[i]}\">");
                }
            }
            if(source.winfohead != "")
                sb.AppendLine($"<h3 class=\"{source.winfohclass}\">{source.winfohead}</h3>");
            if(source.winfotext != "")
                sb.AppendLine($"<p>{source.winfotext}</p>");
            for (int i = 0; i < enddiv; i++)
                sb.AppendLine($"</div>");

            sb.AppendLine(formtext);


            // for (int i = 0; i < enddiv; i++)
            //     sb.AppendLine($"</div>");


            //     cg.parameters.Add("--wfrmaction");
            //     cg.parameters.Add("Create");
            //     cg.parameters.Add("--wfrmclass");
            //     cg.parameters.Add("row:col-lg-8:contact-form-wrapper");
            //     cg.parameters.Add("--wfrmsub");
            //     cg.parameters.Add("row");

            //     string inputtype = "text";
            //     if(genfield.wftype.Trim().ToLower() != "")
            //         inputtype = genfield.wftype.Trim().ToLower();
            //     if(genfield.wftype.Trim().ToLower() != "textarea")
            //     {
            //         sb.AppendLine($"<input asp-for=\"{genfield.vfname}\" name=\"{genfield.vname}.{genfield.vfname}\" class=\"{genfield.wfclass}\" type=\"{inputtype}\" placeholder=\"{genfield.vfcap}\">");
            //     }
            //     else
            //     {
            //         int rows = this.GuessRowSize(genfield.vfsize); 
            //         sb.AppendLine($"<textarea asp-for=\"{genfield.vfname}\" name=\"{genfield.vname}.{genfield.vfname}\" class=\"{genfield.wfclass}\" type=\"{inputtype}\" placeholder=\"{genfield.vfcap}\" rows=\"{rows}\">");
            //     }

            //     if(genfield.wficlass.Trim() != "")
            //         sb.AppendLine($"<i class=\"{genfield.wficlass}\"></i>");




            if(source.wpageclass != "")
                sb.AppendLine("</div>");

            string[] sections = source.wlaynames.Split(':',StringSplitOptions.None);
            string[] sectionfiles = source.wlayfiles.Split(':',StringSplitOptions.None);
            for (int i = 0; i < sections.Length; i++)
            {
                if(sections[i].Trim() != "")
                {
                    if(sectionfiles.Length > i)
                    {
                        if(sectionfiles[i].Trim() != "")
                        {
                            sb.AppendLine("@section " + sections[i]+ " {<partial name=\"" + sectionfiles[i]+ "\" />}");
                        }
                    }
                }
            }

            return sb.ToString();


/*

@model vmelection
@{Layout = "_Main";}
<h4>vmelection</h4>
<hr />
@* <section id="contact" class="contact-section contact-style-3"> *@
<div class="container">
    <div class="row justify-content-center">
        <div class="col-xxl-5 col-xl-5 col-lg-7 col-md-10">
            <div class="section-title text-center mb-50">
                <h3 class="mb-15">Get in touch</h3>
                <p>Find out more about how to combine legal writing and software development.</p>
            </div>
        </div>
    </div>
    <div class="row">
        <div class="col-lg-8">
            <div class="contact-form-wrapper">
                <form asp-action="Create">
                    <div class="row">
<!-- innertext goes here --!>

                        <div class="col-md-12">
                            <div class="form-button">
                                <button type="submit" class="button"> <i class="lni lni-telegram-original"></i> Submit</button>
                            </div>
                        </div>

                        <div>
                            <a asp-action="Index">Back to List</a>
                        </div>

                    </div>
                </form>
            </div>
        </div>
    </div>
</div>


@section Head {<partial name="_MainHeadPartial.cshtml" />}
@section Styles {<partial name="_MainStylesPartial.cshtml" />}
@section Preload {<partial name="_MainPreloadPartial.cshtml" />}
@section Header {<partial name="_MainHeaderPartial.cshtml" />}
@section Client {<partial name="_MainClientPartial.cshtml" />}
@section Footer {<partial name="_MainFooterPartial.cshtml" />}
@section Scripts {<partial name="_MainScriptsPartial.cshtml" />}



*/

                    // helptext.Add("  -wst|--submit     Type of Submit object on the form.");
                    // helptext.Add("  -wsa|--subaction  Specifies the Submit action.");
                    // helptext.Add("  -wsd|--subdclass  Colon-delimited CSS classes for the Submit object.");
                    // helptext.Add("  -wsi|--subiclass  CSS class for an embedded <i> tag for the Submit object.");
                    // helptext.Add("  -wrt|--return     Type of Return object on the form.");
                    // helptext.Add("  -wra|--retaction  Specifies the Return action.");
                    // helptext.Add("  -wrd|--retdclass  Colon-delimited CSS classes for the Return object.");
                    // helptext.Add("  -wri|--reticlass  CSS class for an embedded <i> tag for the Return object.");
                    // helptext.Add("  -wfa|--formaction Specifies the Form action.");
                    // helptext.Add("  -wfc|--formclass  Colon-delimited CSS classes wrapping the Form object.");
                    // helptext.Add("  -wfs|--formsub    Colon-delimited CSS classes wrapping all objects inside the Form object.");
                    // helptext.Add("  -wpc|--pageclass  Specifies the CSS class wrapping the Info and Form sections.");
                    // helptext.Add("  -wic|--infoclass  Colon-delimited CSS classes wrapping the Info section above form fields.");
                    // helptext.Add("  -wih|--infohclass CSS class of the heading in the Info section.");
                    // helptext.Add("  -wit|--infotext   Text for the information section.");
                    // helptext.Add("  -wlf|--layfiles   Colon-separated list of Layout cshtml files associated with --laynames.");
                    // helptext.Add("  -wln|--laynames   Colon-separated list of @section names.");

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

        public string GetClassInnerText(mvvm source)
        {
            int indent = source.indent;

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

            return sb.ToString();
        }
        public string GetClassOuterText(mvvm source, string innertext)
        {
            int indent = source.indent;
            int namespaceindent = source.indent;

            bool usecomponentmodel = true;
            bool usedataannotations = true;
            bool disablenullmessages = true;
            string ns = "";
            string classname = "newclass";
            if(source.category == "model")
            {
                if(source.mnamespace != "")
                    ns = source.mnamespace;
                if(source.mname != "")
                    classname = source.mname;
            }
            if(source.category == "vm")
            {
                if(source.vnamespace != "")
                    ns = source.vnamespace;
                if(source.vname != "")
                    classname = source.vname;
            }


            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            // sb.AppendLine("using System;");
            if(usecomponentmodel)
                sb.AppendLine("using System.ComponentModel;");
            if(usedataannotations)
                sb.AppendLine("using System.ComponentModel.DataAnnotations;");
            if(disablenullmessages)
                sb.AppendLine("#pragma warning disable CS8618");

            if(ns != "") 
            {
                sb.AppendLine($"namespace {ns}");
                sb.AppendLine("{");
                indent += namespaceindent;
            }
            sb.AppendLine(new string(' ', indent) + $"public class {classname}");
            sb.AppendLine(new string(' ', indent) + "{");
            sb.Append(innertext);
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

        public string GetControllerOuterText(mvvm source, string innertext)
        {
            int indent = source.indent;
            int nsindent = 0;

            bool disablenullmessages = true;
            string ns = "";
            string classname = "newcontroller";
            string parentclassname = "Controller";
            if(source.cnamespace != "")
                ns = source.cnamespace;
            if(source.cname != "")
                classname = source.cname;
            if(source.cparent != "")
                parentclassname = source.cparent;


            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using System.Linq;");
            sb.AppendLine("using System.Threading.Tasks;");
            sb.AppendLine("using Microsoft.AspNetCore.Mvc;");
            sb.AppendLine("using Microsoft.AspNetCore.Mvc.Rendering;");
            sb.AppendLine("using Microsoft.EntityFrameworkCore;");
            if(disablenullmessages)
                sb.AppendLine("#pragma warning disable CS8618");

            if(ns != "") 
            {
                sb.AppendLine($"namespace {ns}");
                sb.AppendLine("{");
                nsindent = indent;
            }
            sb.AppendLine(new string(' ', nsindent) + $"public class {classname} : {parentclassname}");
            sb.AppendLine(new string(' ', nsindent) + "{");

            if(source.ccontext != "")
            {
                sb.AppendLine(new string(' ', nsindent + indent) + "private readonly " + source.ccontext + " db;");
                sb.AppendLine(new string(' ', nsindent + indent) + $"public {classname}({source.ccontext} context)");
                sb.AppendLine(new string(' ', nsindent + indent) + "{");
                sb.AppendLine(new string(' ', nsindent + indent + indent) + "this.db = context;");
                sb.AppendLine(new string(' ', nsindent + indent) + "}");
            }
            sb.Append(innertext);
            sb.AppendLine(new string(' ', nsindent) + "}");
            if(ns != "") 
            {
                sb.AppendLine("}");
            }
            return sb.ToString();
        }
         public string GetControllerInnerText(mvvm source)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            foreach(var controlleraction in source.controlleractions)
                sb.Append(this.GetControllerInnerActionText(source,controlleraction));
            return sb.ToString();
        }
        public string GetControllerInnerActionText(mvvm source, controlleraction action)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            int indent = source.indent;
            int nsindent = 0;
            if(source.cnamespace != "")
                nsindent = indent;
            
            string controllername = "";
            string routename = "";
            string actionname = "Index";
            string actiontype= action.cacttype;
            string actionhttp = "GET";
            if(source.croute.Trim() != "" && source.cname.Trim() == "")
                controllername = source.croute.Trim() + "Controller";
            if(source.croute.Trim() != "")
                routename = source.croute.Trim();
            if(routename == "")
                routename = source.cname.Replace("Controller","").Replace("controller","").Trim();
            if(action.cactname.Trim() != "")
                actionname = action.cactname.Trim();
            if(action.chttp.Trim().ToUpper().Contains('G') && action.chttp.Trim().ToUpper().Contains('P'))
            {
                actionhttp = "GET/POST";
            }
            else if(!action.chttp.Trim().ToUpper().Contains('G') && action.chttp.Trim().ToUpper().Contains('P'))
            {
                actionhttp = "POST";
            }
            else if(action.chttp.Trim().ToUpper().Contains('G') && !action.chttp.Trim().ToUpper().Contains('P'))
            {
                actionhttp = "GET";
            }
            else if(actiontype ==  "Index" || actiontype ==  "Details")
            {
                actionhttp = "GET";
            }
            else
            {
                actionhttp = "GET/POST";
            }

/*
        // POST: Journey/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
*/
            var actionhttps = actionhttp.Split('/',StringSplitOptions.None);
            for(int i = 0; i < actionhttps.Length; i++)
            {
                string thisactionhttp = actionhttps[i];
                sb.AppendLine(new string(' ', nsindent+indent) + $"// {thisactionhttp}: {routename}/{actionname}");
                sb.AppendLine(new string(' ', nsindent+indent) + $"[Http{thisactionhttp[0]}{thisactionhttp.Substring(1,thisactionhttp.Length-1).ToLower()}]");
                if(!source.cnouser)
                    sb.AppendLine(new string(' ', nsindent+indent) + $"[ValidateAntiForgeryToken]");

                // note that Index call has no 'POST'
                if(!source.cnoasync)
                {
                    sb.AppendLine(new string(' ', nsindent+indent) + $"public async Task<IActionResult> {actionname}()");
                }
                else if(source.cnoasync)
                {
                    sb.AppendLine(new string(' ', nsindent+indent) + $"public IActionResult {actionname}()");
                }
                sb.AppendLine(new string(' ', nsindent+indent) + "{");

                // there are 2^6 combinations (identity,facade,binding,vm/model,a/synch,raw/dbcontext)
                // this gets out of hand quickly so dismiss the easy ones first

                // This is complex and really requires integration testing
                // TO DO: fill this in!
                sb.AppendLine(new string(' ', nsindent+indent+indent) + $"return View({action.cvname});");

                // if the model and viewmodel are the same
                if(source.cnouser && source.cnofacade && false)
                //  && source.cnoasync && action.cactview.Trim() == "")
                {
                    sb.AppendLine(new string(' ', nsindent+indent+indent) + $"var model = db.Subject.Include(s => s.SubjectModel);");
                    sb.AppendLine(new string(' ', nsindent+indent+indent) + "return View(await model.ToListAsync());");
                }
                if(!source.cnouser && source.cnofacade && false)
                {
                    sb.AppendLine(new string(' ', nsindent+indent+indent) + "var lmdbContext = db.Subject.Include(s => s.SubjectModel);");
                    sb.AppendLine(new string(' ', nsindent+indent+indent) + "return View(await lmdbContext.ToListAsync());");
                }


                sb.AppendLine(new string(' ', nsindent+indent) + "}");
                sb.AppendLine();

            }

/*


            
            // if this exists, update that one
            var tlo = db.Tlo
                .Include(s => s.TloJourney)
                .Include(s => s.TloJourney.JourneySubject)
                .Include(s => s.TloJourney.JourneySubject.SubjectModel)
                .FirstOrDefault(x => x.TloId == tloId && x.TloJourney.JourneySubject.SubjectModel.ModelAgentId == userId);

            // we need to check the credentials
            // noting that the userid is passed-in via the controller via Identity
            if (tlo == null)
            {
                vm.error = "TLO '" + tloId + "' does not exist or user '" + userId + "' does not own it.";
                return false;
            }

        // GET: Project/Project
        public async Task<IActionResult> Index()
        {
              return View(await _context.Project.ToListAsync());
        }

        // GET: Subject
        public async Task<IActionResult> Index()
        {
            var lmdbContext = db.Subject.Include(s => s.SubjectModel);
            return View(await lmdbContext.ToListAsync());
        }

        // GET: Model
        public async Task<IActionResult> Index()
        {
            // only show the current user please!
            string userid = "";
            if (User.Identity.IsAuthenticated == true)
                userid = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value.ToLower().Replace("-","");
            var models = await db.Model.Where(x => x.ModelAgentId.ToLower().Replace("-","") == userid).ToListAsync();
            return View(models);
        }

    public IActionResult Index()
    {
        return Redirect("index.html");
    }

*/
            return sb.ToString();
        }



        public string GetVmToModelMappingText(mvvm source, string modelname, string vmname)
        {
            // it's important to 
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            //

            return sb.ToString();
        }


 
        public string GetFacadeText(mvvm source)
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
                    isfilter = true,
                    ismvvmfield = true
                },
                new commafield(){
                    singlename="mname",
                    pluralname="mnames",
                    synonym="mn",
                    isfilter = true,
                    ismvvmfield = true
                },
                new commafield(){
                    singlename="wname",
                    pluralname="wnames",
                    synonym="wn",
                    isfilter = true,
                    ismvvmfield = true
                },
                new commafield(){
                    singlename="vfname",
                    pluralname="vfnames",
                    synonym="vf",
                    ismvvmfield = true
                },
                new commafield(){
                    singlename="vftype",
                    pluralname="vftypes",
                    synonym="vt",
                    ismvvmfield = true
                },
                new commafield(){
                    singlename="vfsize",
                    pluralname="vfsizes",
                    synonym="vz",
                    ismvvmfield = true
                },
                new commafield(){
                    singlename="vfdesc",
                    pluralname="vfdescs",
                    synonym="vc",
                    ismvvmfield = true
                },
                new commafield(){
                    singlename="vfreq",
                    pluralname="vfreqs",
                    synonym="vq",
                    ismvvmfield = true
                },
                new commafield(){
                    singlename="vfcap",
                    pluralname="vfcaps",
                    synonym="va",
                    ismvvmfield = true
                },
                new commafield(){
                    singlename="mfname",
                    pluralname="mfnames",
                    synonym="mf",
                    ismvvmfield = true
                },
                new commafield(){
                    singlename="mftype",
                    pluralname="mftypes",
                    synonym="mt",
                    ismvvmfield = true
                },
                new commafield(){
                    singlename="mfsize",
                    pluralname="mfsizes",
                    synonym="ms",
                    ismvvmfield = true
                },
                new commafield(){
                    singlename="wfclass",
                    pluralname="wfclasses",
                    synonym="we",
                    ismvvmfield = true
                },
                new commafield(){
                    singlename="wftype",
                    pluralname="wftypes",
                    synonym="wy",
                    ismvvmfield = true
                },
                new commafield(){
                    singlename="wfdclass",
                    pluralname="wfdclasses",
                    synonym="wd",
                    ismvvmfield = true
                },
                new commafield(){
                    singlename="wficlass",
                    pluralname="wficlasses",
                    synonym="wi",
                    ismvvmfield = true
                },
                new commafield(){
                    singlename="chttp",
                    pluralname="chttps",
                    synonym="cap",
                    isctrlaction = true
                },
                new commafield(){
                    singlename="cactname",
                    pluralname="cactnames",
                    synonym="can",
                    isctrlaction = true
                },
                new commafield(){
                    singlename="cactsyn",
                    pluralname="cactsyns",
                    synonym="cas",
                    isctrlaction = true
                },
                new commafield(){
                    singlename="cacttype",
                    pluralname="cacttypes",
                    synonym="cat",
                    isctrlaction = true
                },
                new commafield(){
                    singlename="cactvm",
                    pluralname="cactvms",
                    synonym="cav",
                    isctrlaction = true
                },
                // new commafield(){
                //     singlename="cactvmfld",
                //     pluralname="cactvmflds",
                //     synonym="cal"
                // },
                new commafield(){
                    singlename="cvname",
                    pluralname="cvnames",
                    synonym="cvn",
                    isctrlaction = true
                },
                new commafield(){
                    singlename="cmname",
                    pluralname="cmnames",
                    synonym="cmn",
                    isctrlaction = true
                },                new commafield(){
                    singlename="cwname",
                    pluralname="cwnames",
                    synonym="cwn",
                    isctrlaction = true
                },
                new commafield(){
                    singlename="cvpkey",
                    pluralname="cvpkeys",
                    synonym="cvk",
                    isctrlaction = true
                },
                new commafield(){
                    singlename="cvfkey",
                    pluralname="cvfkeys",
                    synonym="cvf",
                    isctrlaction = true
                },
                new commafield(){
                    singlename="cmpkey",
                    pluralname="cmpkeys",
                    synonym="cmk",
                    isctrlaction = true
                },
                new commafield(){
                    singlename="cmfkey",
                    pluralname="cmfkeys",
                    synonym="cmf",
                    isctrlaction = true
                },
                new commafield(){
                    singlename="cmparent",
                    pluralname="cmparents",
                    synonym="cmp",
                    isctrlaction = true
                },
                new commafield(){
                    singlename="cvukey",
                    pluralname="cvukeys",
                    synonym="cvu",
                    isctrlaction = true
                },
                new commafield(){
                    singlename="cvmsg",
                    pluralname="cvmsgs",
                    synonym="cvm",
                    isctrlaction = true
                }

                // new commafield(){
                //     singlename="cactpkey",
                //     pluralname="cactpkeys",
                //     synonym="cak"
                // },
                // new commafield(){
                //     singlename="cactfkey",
                //     pluralname="cactfkeys",
                //     synonym="caf"
                // },
                // new commafield(){
                //     singlename="cactukey",
                //     pluralname="cactukeys",
                //     synonym="cau"
                // },
                // new commafield(){
                //     singlename="cactmsg",
                //     pluralname="cactmsgs",
                //     synonym="cag"
                // }

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
        public bool isctrlaction = false;
        public bool ismvvmfield = false;

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
        public string mfsize {get;set;} = "";  //Comma-separated list of Model field sizes in order.");

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
        public string cname {get;set;} = "";  // Name of the Controller (class name)
        public string cnamespace {get;set;} = "";  // Namespace of the Controller
        public string croute {get;set;} = "";  // Route name of the Controller (base URL route)
        public string cparent {get;set;} = "";  // Name of the parentClass
        public string ccontext {get;set;} = "";  // Class name of the ApplicationContext
        public bool cfacade {get;set;}  = true;  // Does Controller use a façade?
        public bool cidentity {get;set;} = true;  // Does Controller use Identity?
        public bool casync {get;set;} = true;  // Does Controller use asynchronous methods?
        public bool cdbsave {get;set;} = true;  // Does Controller issue a dbsave?
        public bool cbinding {get;set;} = true;  // Does Controller bind individual fields?
        public string chttp {get;set;} = "";  // GET/SET action properties
        public string cactname {get;set;} = "";  // action name
        public string cactsyn {get;set;} = "";  // action synonyms
        public string cacttype {get;set;} = "";  // type (Create/Delete/Edit/Index/Details)


        public string cvname {get;set;} = "";  // ViewModel names
        // public string cactvmfld {get;set;} = "";  // ViewModel field names
        public string cmname {get;set;} = "";  // Model name
        public string cwname {get;set;} = "";  // View name
        public string cvpkey {get;set;} = "";  // Colon-delimited ViewModel primary key fields.");
        public string cvfkey {get;set;} = "";  // Colon-delimited ViewModel foreign key fields.");
        public string cmpkey {get;set;} = "";  // Colon-delimited Model primary key fields.");
        public string cmfkey {get;set;} = "";  // Colon-delimited Model foreign key fields.");
        public string cmparent {get;set;} = "";  // Colon-delimited Model parent table names.");
        public string cvukey {get;set;} = "";  // Colon-delimited action ViewModel user key fields.");
        public string cvmsg {get;set;} = "";  // Colon-delimited action ViewModel message fields.");

    }

/*


        // POST: Subject/Create
        // 2019-05-30 SNJW note that the 'id' field is the foreign key (modelid in this case) so we do not bind it
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string id, [Bind("code,name,desc,component,creditpoints,modelid,type,subjectid")] SubjectViewModel vm)
        {
            if (ModelState.IsValid)
            {
                lm.Models.shared.facade facade = new lm.Models.shared.facade(db);
                // as this can be anonymous, we need to set the user id to then set the project Id in the vm here if the user is logged-in
                vm.userid = "";
                if (User.Identity.IsAuthenticated == true)
                    vm.userid = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value.Replace("-","");
                facade.Push(vm);
                try
                {
                    await db.SaveChangesAsync();
                }
                catch (Exception e)
                {
                    vm.error = e.InnerException.ToString();
                }
            }
            return RedirectToAction("Edit","Model", new { id = id } );
            //return View(vm);
        }

*/


/// This is setup to create ONE file but it needs HEAPS of tangential infomration to do this
    public class mvvm
    {
        // These properties are set in code:
        public string category {get;set;} = "";
        public string categoryname {get;set;} = "";
        public int indent {get;set;} = 4;

        // these are set using System.Reflection:
        public string output {get;set;} = "";  //  Full path to output .cs file.
        public string sourcefile {get;set;} = "";  // Loads field properties from a CSV file.

        public bool fillblanks {get;set;} = true;  // Where only one item is specified as a parameter, use it for ALL children where empty

        // public string fieldprefix = "";
        public string vname {get;set;} = "";
        public string vnamespace {get;set;} = "";

        public string mname {get;set;} = "";  // Model name.
        public string mnamespace {get;set;} = "";  // Model namespace.
        public string wname {get;set;} = "";  // View name.
        public string waction {get;set;} = "";  // View action (Index/Create/Edit/Delete/Details).

        public string mpkey {get;set;} = "";  // Specifies the primary key field in the Model.
        public string mfkey {get;set;} = "";  // Specifies the foreign key field in the Model.
        public string mftable {get;set;} = "";  // Specifies the parent of the Model.
        // public string vuserkey {get;set;} = "";  // Specifies the userid field in the ViewModel.
        // public string vmessage {get;set;} = "";  // Specifies a field in the ViewModel to relay messages.
        public string vpkey {get;set;} = "";  // Specifies the primary key field in the ViewModel.
        public string vfkey {get;set;} = "";  // Specifies the foreign key field in the ViewModel.
        public string vftable {get;set;} = "";  // Specifies the parent of the ViewModel.
        public string vuserkey {get;set;} = "";  // Specifies the userid field in the ViewModel.
        public string vmessage {get;set;} = "";  // Specifies a field in the ViewModel to relay messages.
        public string layout {get;set;} = "";  // Specifies a field in the ViewModel to relay messages.

        public string wsubmit {get;set;} = "";  // Type of Submit object on the form.
        public string wsubaction {get;set;} = "";  // Specifies the Submit action.
        public string wsubdclass {get;set;} = "";  // Colon-delimited CSS classes for the Submit object.
        public string wsubiclass {get;set;} = "";  // CSS class for an embedded <i> tag for the Submit object.
        public string wreturn {get;set;} = "";  // Type of Return object on the form.
        public string wretaction {get;set;} = "";  // Specifies the Return action.
        public string wretdclass {get;set;} = "";  // Colon-delimited CSS classes for the Return object.
        public string wreticlass {get;set;} = "";  // CSS class for an embedded <i> tag for the Return object.
        public string wfrmaction {get;set;} = "";  // Specifies the Form action.
        public string wfrmclass {get;set;} = "";  // Colon-delimited CSS classes wrapping the Form object.
        public string wfrmsub {get;set;} = "";  // Colon-delimited CSS classes wrapping all objects inside the Form object.
        public string wpageclass {get;set;} = "";  // Specifies the CSS class wrapping the Info and Form sections.
        public string winfoclass {get;set;} = "";  // Colon-delimited CSS classes wrapping the Info section above form fields.
        public string winfohclass {get;set;} = "";  // CSS class of the heading in the Info section.
        public string winfohead {get;set;} = "";  // Heading text for the information section.
        public string winfotext {get;set;} = "";  // Text for the information section.
        public string wlayfiles {get;set;} = "";  // Colon-separated list of Layout cshtml files associated with --laynames.
        public string wlaynames {get;set;} = "";  // Colon-separated list of @section names.
        public string wlayout {get;set;} = "";  // Name of the primary Layout.cshtml file.


        // public string ukey {get;set;} = "";  // name of userid field.  Not used if 'useidentity' is false
        // public string message {get;set;} = "";  // name of messaging field (usually for errors)



        public string cname {get;set;} = "";  // Name of the Controller class.
        public string cnamespace {get;set;} = "";  // Namespace of the Controller.
        public string croute {get;set;} = "";  // Name of the Action route.
        public string cparent {get;set;} = "";  // Name of the parent class.
        public string ccontext {get;set;} = "";  // Class name of the ApplicationContext.
        public bool cnofacade {get;set;} = false;  //  Whether Controller uses a facade.
        public bool cnouser {get;set;} = false;  // Whether Controller uses Identity.
        public bool cnoasync {get;set;} = false;  // Whether Controller uses asynchronous methods.
        public bool cnodbsave {get;set;} = false;  // Whether Controller issues a dbsave command.
        public bool cnobinding {get;set;} = false;  // Whether Controller binds individual fields.
        public string chttps {get;set;} = "";  // Colon-delimited GET/SET action properties.
        public string cactnames {get;set;} = "";  // Colon-delimited action names.
        public string cactsyns {get;set;} = "";  // Colon-delimited action synonyms.
        public string cacttypes {get;set;} = "";  // Colon-delimited action types (Create/Delete/Edit/Index/Details).

        public string cvnames {get;set;} = "";  // ViewModel names
        public string cmnames {get;set;} = "";  // Model name
        public string cwnames {get;set;} = "";  // View name
        public string cvpkeys {get;set;} = "";  // Colon-delimited ViewModel primary key fields
        public string cvfkeys {get;set;} = "";  // Colon-delimited ViewModel foreign key fields
        public string cmpkeys {get;set;} = "";  // Colon-delimited Model primary key fields
        public string cmfkeys {get;set;} = "";  // Colon-delimited Model foreign key fields
        public string cmparents {get;set;} = "";  // Colon-delimited Model parent table names
        public string cvukeys {get;set;} = "";  // Colon-delimited action ViewModel user key fields
        public string cvmsgs {get;set;} = "";  // Colon-delimited action ViewModel message fields



        public List<mvvmfield> genfields = new List<mvvmfield>();
        public List<controlleraction> controlleractions = new List<controlleraction>();
        public List<string> parentmodels = new List<string>();
        public List<string> childmodels = new List<string>();
    }



}