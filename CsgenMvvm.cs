
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

#region setup-generation

        public bool SetFullModel(List<ParameterSetting> ps)
        {
            // the mvvm fullmodel is all information passed-in
            var output = new mvvm();
            List<commafield> commafields = this.GetCommaFields();

            // if there is a CSV file specified, grab these first
            List<mvvmfield> csvfields = new List<mvvmfield>();
            List<mvvmfield> parafields = new List<mvvmfield>();
            var checkcsv = ps.FirstOrDefault(x => x.isactive.Equals(true) && (x.setting.Equals("--source") || x.synonym.Equals("-s")));
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
            foreach(var checkfield in commafields.Where(x => x.ismvvmfield.Equals(true) && x.isfilter.Equals(false)))
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
            else if(output.category=="view")
            {
                if(csvfields.Exists(x => x.vname.Equals(output.vname)))
                {
                    output.genfields.AddRange(csvfields.Where(x => x.vname.Equals(output.vname) && x.vftype != "" && x.vfname != ""));
                }
                else
                {
                    output.genfields.AddRange(csvfields.Where(x => x.vftype != "" && x.vfname != ""));
                }
            }
            else
            {
                // we need ALL the loaded fields for other categories
                output.genfields.AddRange(csvfields);
            }
            
            if(output.category=="controller" || output.category=="facade")
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

            foreach(var genfield in parafields)
            {
                if(genfield.mname == "" & genfield.mfname != "")
                    genfield.mname = output.mname;
                if(genfield.vname == "" & genfield.vfname != "")
                    genfield.vname = output.vname;
            }
            if(output.fillempty && parafields.Count>1)
            {
                // Where the first item in a child has text in fields,
                // use that text is ALL subsequent children where that field is blank
                // however, this ONLY APPLIES TO THE SELECTED MODEL/VM/VIEW
                Type maintype = output.GetType();

                Type mvvmfieldtype = parafields[0].GetType();
                foreach(var mvvmfieldprop in mvvmfieldtype.GetProperties())
                {
                    if(mvvmfieldprop.PropertyType.FullName != "System.String")
                        continue;
                    string fieldname = mvvmfieldprop.Name.ToLower().Trim();
                    var checkfield = commafields.First(x => x.singlename.Equals(fieldname));
                    if(checkfield==null)
                        continue;
                    if(checkfield.isfilter || checkfield.isunique)
                        continue;

                    // we need to find the first 
                    string firstvalue = (string)(mvvmfieldprop.GetValue(parafields[0]) ?? "");
                    string firstletter = mvvmfieldprop.Name.ToLower().Substring(0,1);

                    if(fieldname.Equals("wfclass"))
                        firstvalue += "";

                    if(firstvalue != "")
                    {
                        foreach(var genfield in parafields)
                        {
                            // if this is blank, set it to the first value
                            string currentvalue = (string)(mvvmfieldprop.GetValue(genfield) ?? "");
                            if(currentvalue == "")
                                mvvmfieldprop.SetValue(genfield, firstvalue);
                        }
                    }
                }
            }


            if(output.fillempty && output.controlleractions.Count>1)
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

            // add both the CSV and parameter items to the output

            // now filter any of the CSV records that are not required
            if(output.category=="vm")
            {
                // if at least one record has a name then filter, otherwise take them all
                if(parafields.Exists(x => x.vname.Equals(output.vname)))
                {
                    output.genfields.AddRange(parafields.Where(x => x.vname.Equals(output.vname) && x.vfname != "" && x.vftype != ""));
                }
                else
                {
                    output.genfields.AddRange(parafields.Where(x => x.vfname != "" && x.vftype != ""));
                }
            }
            else if(output.category=="model")
            {
                if(parafields.Exists(x => x.mname.Equals(output.mname)))
                {
                    output.genfields.AddRange(parafields.Where(x => x.mname.Equals(output.mname) && x.mfname != "" && x.mftype != ""));
                }
                else
                {
                    output.genfields.AddRange(parafields.Where(x => x.mfname != "" && x.mftype != ""));
                }
            }
            else if(output.category=="view")
            {
                if(parafields.Exists(x => x.vname.Equals(output.vname)))
                {
                    output.genfields.AddRange(parafields.Where(x => x.vname.Equals(output.vname) && x.vftype != "" && x.vfname != ""));
                }
                else
                {
                    output.genfields.AddRange(parafields.Where(x => x.vftype != "" && x.vfname != ""));
                }
            }
            else
            {
                // we need ALL the loaded fields for other categories
                output.genfields.AddRange(parafields);
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
            if(source.vname=="" && (source.category=="vm" || source.category=="view" || source.category=="facade" )) // || source.category=="controller" ))
            {
                this.lastmessage = $"Creating a {source.categoryname} requires a ViewModel name.";
                return false;
            }
            if(source.mname=="" && ( source.category=="model" ))
            {
                this.lastmessage = $"Creating a {source.categoryname} requires a Model name.";
                return false;
            }
            if(source.cname=="" && (source.category=="controller" ))
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
                text = this.GetClassOuterText(source, this.GetModelVmInnerText(source));
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
                text = this.GetClassOuterText(source, this.GetFacadeInnerText(source));
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

#endregion setup-generation

#region generate-view

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
            foreach(var genfield in source.genfields.Where(x => x.vname.Equals(source.vname) && x.vfname != ""))
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
            foreach(var genfield in source.genfields.Where(x => x.vname.Equals(source.vname)))
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
            foreach(var genfield in source.genfields.Where(x => x.vname.Equals(source.vname)))
                sb.Append(this.GetViewSingleFieldText(genfield));
            return sb.ToString();
        }
        public string GetViewFormText(mvvm source)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            foreach(var genfield in source.genfields.Where(x => x.vname.Equals(source.vname)))
                sb.Append(this.GetViewSingleFieldText(genfield));
            return sb.ToString();
        }
        public string GetViewFormText(mvvm source, string innertext)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            int enddiv = 0;
            string formaction = "/";
            string formmethod = "POST";
            if(source.wfrmaction != "")
                formaction = source.wfrmaction;
            if(source.wfrmmethod.ToUpper().Contains("G"))
                formmethod = "GET";
            string formstart = $@"<form action=""{formaction}"" method=""{formmethod}"">";
            string formend = "</form>";
            string btntext = this.GetViewFormButtonText(source);
            if(source.wsubaction.ToUpper().Contains("G"))
                formmethod = "GET";

            string[] formclasses = source.wfrmclass.Split(':',StringSplitOptions.None);
            for (int i = 0; i < formclasses.Length; i++)
            {
                if(formclasses[i].Trim() != "")
                {
                    enddiv++;
                    sb.AppendLine($"<div class=\"{formclasses[i]}\">");
                }
            }
            sb.AppendLine(formstart);

            if(source.wfrmsub.Trim() != "")
                sb.AppendLine($"<div class=\"{source.wfrmsub}\">");

            sb.AppendLine(innertext);

            if(source.wfrmsub.Trim() != "")
                sb.AppendLine("</div>");

            if(source.wfrmbtncss.Trim() != "")
                sb.AppendLine($"<div class=\"{source.wfrmbtncss.Trim()}\">");
            sb.AppendLine(btntext);
            if(source.wfrmbtncss.Trim() != "")
                sb.AppendLine($"</div>");

            sb.AppendLine(formend);

            for (int i = 0; i < enddiv; i++)
                sb.AppendLine($"</div>");





            // include the form object here
            // <form action="/Election/Create" method="POST">

            return sb.ToString();
        }

        public string GetViewFormButtonText(mvvm source)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            foreach(var item in this.GetViewFormButtons(source))
            {
/*
<div class="form-button">
<button type="submit" class="button"> <i class="lni lni-telegram-original"></i> Submit</button>
</div>
*/
                if(item.btndclass != "")
                    sb.AppendLine($"<div class=\"{item.btndclass}\">");
                
                string typetext = " type=\"button\"";
                if(item.btntype == "submit" || item.btntype == "reset")
                    typetext = $" type=\"{item.btntype}\"";

                string classtext = item.btnclass;
                if(classtext != "")
                    classtext = $" class=\"{classtext}\"";

                string onclicktext = "";
                if(item.btnonclick != "")
                    onclicktext = $" onclick=\"{item.btnonclick}\"";

                string itext = "";
                if(item.btniclass != "")
                    itext = $" <i class=\"{item.btniclass}\"></i> ";

                sb.AppendLine($"<button{typetext}{classtext}{onclicktext}>{itext}{item.btntext}</button>");

                if(item.btndclass != "")
                    sb.AppendLine("</div>");
            }

            return sb.ToString();
        }

        public List<frmbutton> GetViewFormButtons(mvvm source)
        {
            var output = new List<frmbutton>();
            string[] btnnames = source.wbtnname.Split(':',StringSplitOptions.None);
            string[] btntypes = source.wbtntype.Split(':',StringSplitOptions.None);
            string[] btntexts = source.wbtntext.Split(':',StringSplitOptions.None);
            string[] btnclasses = source.wbtnclass.Split(':',StringSplitOptions.None);
            string[] btndclasses = source.wbtndclass.Split(':',StringSplitOptions.None);
            string[] btniclasses = source.wbtniclass.Split(':',StringSplitOptions.None);
            string[] btnonclicks = source.wbtnonclick.Split(':',StringSplitOptions.None);
            for (int i = 0; i < btnnames.Length; i++)
            {
                var item = new frmbutton();

                // names must be unique
                item.btnname = btnnames[i].Trim();
                if(item.btnname == "" || ( output.Exists(x => x.btnname.Equals(item.btnname))))
                    item.btnname = "btn"+System.Guid.NewGuid().ToString().Replace(" ","").ToLower();
                
                item.btntype = "default";
                if(btntypes.Length>i)
                {
                    if(btntypes[i].ToLower().Trim() == "submit" && !output.Exists(x => x.btntype.Equals("submit")))
                    {
                        item.btntype = "submit";
                    }
                    else if(btntypes[i].ToLower().Trim() == "reset")
                    {
                        item.btntype = "reset";
                    }
                }

                item.btntext = "";
                if(btntexts.Length == 1 && btntexts[0] == "")
                {
                    // default the text to the name
                    if(!(item.btnname.StartsWith("btn")))
                    {
                        item.btntext = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(item.btnname.Replace("."," ").Replace("_"," ").Replace("-"," "));
                    }
                    else
                    {
                        if(item.btnname.Length > 3 || item.btnname.Length < 35)
                            item.btntext = item.btnname.Substring(2,item.btnname.Length-3);
                    }
                }
                else
                {
                    if(btntexts.Length>i)
                        item.btntext = btntexts[i];  // no trimming
                }
                
                item.btnclass = "";
                if(btnclasses.Length>i)
                    item.btnclass = btnclasses[i].Trim();

                item.btndclass = "";
                if(btndclasses.Length>i)
                    item.btndclass = btndclasses[i].Trim();

                item.btniclass = "";
                if(btniclasses.Length>i)
                    item.btniclass = btniclasses[i].Trim();

                item.btnonclick = "";
                if(btnonclicks.Length>i)
                    item.btnonclick = btnonclicks[i].Trim();
               
                output.Add(item);
            }

            return output;
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
            if(source.wlayout != "")
                sb.AppendLine("@{Layout = \"" + source.wlayout +"\";}");
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



            if(source.wpageclass != "")
                sb.AppendLine("</div>");


            // 2022-10-26 SNJW add scripts here that are called
            // note that if no class name for section is included, add them manually
            if(source.wscrsection != "")
                sb.AppendLine("@section " + source.wscrsection+ " {");
            if(source.wscrlist != "")
            {
                // add these manually
                foreach(var script in source.wscrlist.Split(':',StringSplitOptions.None))
                    sb.AppendLine(@$"<script src=""{script}""></script>");
            }
            if(source.wscrsection != "")
                sb.AppendLine("}");

                    // helptext.Add("  -wjl|--wscrlist    Colon-delimited list of .js files linked on this view.");
                    // helptext.Add("  -wjn|--wscrsection Name of the @section for scripts called in this view.");


            // 2022-09-26 SNJW this is manifestly wrong - remove for now
            /*

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

            */

            return sb.ToString();
        }

#endregion generate-view

#region generate-class

        public string GetModelVmInnerText(mvvm source)
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
                if(source.category=="model" && field.mname == source.mname && source.mpkey!="" && field.mfname==source.mpkey)
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
                else if(source.category=="vm" && field.vname == source.vname && source.vpkey!="" && field.vfname==source.vpkey)
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
                if(source.category=="model" && field.mname == source.mname && field.mfname!=source.mpkey)
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
                else if(source.category=="vm" && field.vname == source.vname && field.vfname!=source.vpkey)
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

            if(source.category == "model" && source.mparent != "")
            {
                string parent = source.mparent.Split('.',StringSplitOptions.None)[0];
                sb.Append(this.GetClassSingleFieldText(
                    parent,
                    parent,
                    "",
                    "",
                    "",
                    "",
                    false,
                    false,
                    classindent));
                sb.AppendLine();
            }

            return sb.ToString();
        }
        public string GetClassOuterText(mvvm source, string innertext)
        {
            int indent = source.indent;
            int nsindent = 0;
            List<string> usings = new List<string>();
            List<string> constructor = new List<string>();

            bool usecomponentmodel = true;
            bool usedataannotations = true;
            bool disablenullmessages = true;

            string ns = "";
            string classname = "newclass";
            string parentclass = "";
            string classmodifier = " ";
            string dbpropname = "db";
            if(source.cdpropname.Trim() != "")
                dbpropname = source.cdpropname.Trim();

            if(source.category == "model")
            {
                if(source.mnamespace != "")
                {
                    nsindent += indent;
                    ns = source.mnamespace;
                }
                if(source.mname != "")
                    classname = source.mname;
            }
            else if(source.category == "vm")
            {
                if(source.vnamespace != "")
                {
                    nsindent += indent;
                    ns = source.vnamespace;
                }
                if(source.vname != "")
                    classname = source.vname;
            }
            else if(source.category == "facade")
            {
                if(source.fnamespace != "")
                {
                    nsindent += indent;
                    ns = source.fnamespace;
                }
                if(source.fname != "")
                    classname = source.fname;
                usecomponentmodel = false;
                usedataannotations = false;
                disablenullmessages = false;
                if(source.vnamespace != "")
                    usings.Add($"using {source.vnamespace};");
                if(source.mnamespace != "")
                    usings.Add($"using {source.mnamespace};");
                if(source.cdcontext != "")
                {
                    constructor.Add(new string(' ', nsindent + indent) + $"private readonly {source.cdcontext} {dbpropname};");
                    constructor.Add(new string(' ', nsindent + indent) + $"public {classname}({source.cdcontext} context)");
                    constructor.Add(new string(' ', nsindent + indent) + "{");
                    constructor.Add(new string(' ', nsindent + indent + indent) + $"this.{dbpropname} = context;");
                    constructor.Add(new string(' ', nsindent + indent) + "}");
                }
            }

            if(usecomponentmodel)
                usings.Add("using System.ComponentModel;");
            if(usedataannotations)
                usings.Add("using System.ComponentModel.DataAnnotations;");
            if(disablenullmessages)
                usings.Add("#pragma warning disable CS8618");

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            foreach(var line in usings)
                sb.AppendLine(line);

            if(ns != "") 
            {
                sb.AppendLine($"namespace {ns}");
                sb.AppendLine("{");
            }
            string topline = $"public{classmodifier}class {classname}";
            if(parentclass != "")
                topline += $" : {parentclass}";

            sb.AppendLine(new string(' ', nsindent) + topline);
            sb.AppendLine(new string(' ', nsindent) + "{");
            foreach(var line in constructor)
                sb.AppendLine(line);
            sb.Append(innertext);
            sb.AppendLine(new string(' ', nsindent) + "}");
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

#endregion generate-class

#region generate-controller
        public string GetControllerOuterText(mvvm source, string innertext)
        {
            int indent = source.indent;
            int nsindent = 0;

            bool disablenullmessages = true;
            string ns = "";
            string classname = "newcontroller";
            string parentclassname = "Controller";
            string dbpropname = "db";
            string areaname = source.careaname.Trim();
//                                [Area("Election")]

            if(source.cnamespace != "")
                ns = source.cnamespace;
            if(source.cname != "")
                classname = source.cname;
            if(source.cparent != "")
                parentclassname = source.cparent;
            if(source.cdpropname.Trim() != "")
                dbpropname = source.cdpropname.Trim();


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
            if(areaname != "")
                sb.AppendLine(new string(' ', nsindent) + $"[Area(\"{areaname}\")]");
            sb.AppendLine(new string(' ', nsindent) + $"public class {classname} : {parentclassname}");
            sb.AppendLine(new string(' ', nsindent) + "{");

            if(source.cdcontext != "")
            {
                sb.AppendLine(new string(' ', nsindent + indent) + $"private readonly {source.cdcontext} {dbpropname};");
                sb.AppendLine(new string(' ', nsindent + indent) + $"public {classname}({source.cdcontext} context)");
                sb.AppendLine(new string(' ', nsindent + indent) + "{");
                sb.AppendLine(new string(' ', nsindent + indent + indent) + $"this.{dbpropname} = context;");
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
            // here we are building the actions inside the controller
            // note that there are usualy two (GET+POST) for each named action
            //
            // If there is NO facade, and the Model is the same as the ViewModel
            //   (that is, identical in capitalisation also)
            //   then we replicate this code:
            // dotnet aspnet-codegenerator controller --controllerName SubjectController -dc xo.Data.ApplicationDbContext --useSqlite --model xo.Models.data.Subject
            //
            // If the Model is NOT the same as the ViewModel
            //   then we do some mapping of fields
            // If there is no Facade, this mapping code will be in the Controller
            // If there is a Facade, this mapping code will be there and the Controller will have NO reference to the underlying Model
            //
            // For each of the View types, the actual code is slightly different




            //               return View(await _context.Subject.ToListAsync());
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            int indent = source.indent;
            int nsindent = 0;
            if(source.cnamespace != "")
                nsindent = indent;

            bool noidentity = source.cnouser;
            bool nofacade = source.cnofacade;
            bool nodbsave = source.cnodbsave;
            bool noasync = source.cnoasync;
            string controllername = "";
            string dbpropname = source.cdpropname.Trim();
            if(dbpropname=="")
                dbpropname = "db";
            string idpropname = source.cactionparameter;
            string routename = "";
            string actionname = "Index";
            string actiontype= action.cacttype;
            string actionhttp = "GET";
            string vmname = action.cvname;
            string modelname = action.cmname;
            string identitytable = source.identitytable;
            string identitytableid = source.identitytableid;
            string useranonname = source.useranonname;
            string useranonval = source.useranonval;
            string objectmodel = source.objectmodel;
            string objectvm = source.objectvm;
            string objectvmuserid = source.objectvm + '.' + action.cvukey;
            string objectvmpkey = source.objectvm + '.' + action.cvpkey;
            string objectvmfkey = source.objectvm + '.' + action.cvfkey;
            string parentmodel = action.cmparent;
            string parentpkey = action.cmparkey;
            string foreignkey = action.cmfkey;


                // // note that this is a parameter but it's a bit wild at this stage
                // string ukeyparameter = "";
                // string ukeyfieldname = "";
                // if(!noidentity)
                // {
                //     ukeyparameter = "id";
                //     ukeyfieldname = "Id";
                // }

            if(source.croute.Trim() != "" && source.cname.Trim() == "")
                controllername = source.croute.Trim() + "Controller";
            if(source.cdpropname.Trim() != "")
                dbpropname = source.cdpropname.Trim();
            if(source.croute.Trim() != "")
                routename = source.croute.Trim();
            if(routename == "")
                routename = source.cname.Replace("Controller","").Replace("controller","").Trim();
            if(action.cactname.Trim() != "")
                actionname = action.cactname.Trim();
            if(action.cacthttp.Trim().ToUpper().Contains('G') && action.cacthttp.Trim().ToUpper().Contains('P'))
            {
                actionhttp = "GET/POST";
            }
            else if(!action.cacthttp.Trim().ToUpper().Contains('G') && action.cacthttp.Trim().ToUpper().Contains('P'))
            {
                actionhttp = "POST";
            }
            else if(action.cacthttp.Trim().ToUpper().Contains('G') && !action.cacthttp.Trim().ToUpper().Contains('P'))
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
                sb.AppendLine();
                sb.AppendLine(new string(' ', nsindent+indent) + $"// {thisactionhttp}: {routename}/{actionname}");
                sb.AppendLine(new string(' ', nsindent+indent) + $"[Http{thisactionhttp[0]}{thisactionhttp.Substring(1,thisactionhttp.Length-1).ToLower()}]");
                if(!noidentity)
                    sb.AppendLine(new string(' ', nsindent+indent) + $"[ValidateAntiForgeryToken]");

                // note that Index call has no 'POST'

                // do the top line first
                string entryline = "";
                if(!noasync)
                {
                    entryline = new string(' ', nsindent+indent) + $"public async Task<IActionResult> {actionname}(";
                }
                else
                {
                    entryline = new string(' ', nsindent+indent) + $"public IActionResult {actionname}(";
                }


                // some action-httptype combinations need the parameter to be passed-in
                // other times it will just be 'vm'
                string thisobjectvm = objectvm;
                string thisobjectvmuserid = objectvmuserid;
                string thisobjectvmpkey = objectvmpkey;
                string thisobjectvmfkey = objectvmfkey;
                if(this.ActionHasVmParameter(actiontype, thisactionhttp, vmname))
                {
                    thisobjectvm = vmname;
                    thisobjectvmuserid = vmname + '.' + action.cvukey;
                    thisobjectvmpkey = vmname + '.' + action.cvpkey;
                    thisobjectvmfkey = vmname + '.' + action.cvfkey;
                }

                entryline += this.GetControllerActionParameterText(source, action, thisactionhttp);

                sb.AppendLine(entryline + ")");

                sb.AppendLine(new string(' ', nsindent+indent) + "{");

                // if this is a GET then we need to create the vm and set the userid (if using Identity) and perent tableid (if it has one)
                // TO DO: if this scaffolding is to be 'true' to MVVM principles then the datacontext and indeed all of the model 
                // 'stuff' would be send to the ViewModel.  I don't think this is necessary - it's more a philosophical debate
                // I personally like 'light' ViewModels and 'thin' controllers with the Facade doing the heavy thinking
                if(actiontype=="Create" && thisactionhttp == "POST")
                    actiontype = actiontype + "";

                if(this.ActionNeedsVmCreation(actiontype, thisactionhttp, vmname))
                    sb.AppendLine(new string(' ', nsindent+indent+indent) + $"var {thisobjectvm} = new {vmname}();");

                if(!noidentity && action.cvukey.Trim() != "")
                    sb.AppendLine(this.IndentText(this.GetSetVmUserIdControllerText(source,action,thisobjectvm),nsindent+indent+indent));

                // there are 2^6 combinations (identity,facade,binding,vm/model,a/synch,raw/dbcontext)
                // this gets out of hand quickly so dismiss the easy ones first
                if(thisactionhttp == "POST" && ( actiontype == "Create" || actiontype == "Edit" ) && action.cvname != "")
                {
                    sb.AppendLine(new string(' ', nsindent+indent+indent) + $"if(!ModelState.IsValid)");
                    sb.AppendLine(new string(' ', nsindent+indent+indent+indent) + $"return View({thisobjectvm});");
                }



                if(nofacade)
                {

                    // if identity is used, we need to check whether this user 'owns' the relevant object
                    //   (based on all parents leading back to AspNetUsers...)
                    // if identity is not used, we only need to check whether it exists...

                    // we will start with the GET
                    if(thisactionhttp == "POST" && ( actiontype == "Create" || actiontype == "Edit" ) && action.cvname != "")
                    {
                        // here we need to know the parent model access
                        // TO DO - many to many relationships... not today


                    }



                    // if there is no facade, this is handled here
                    if(thisactionhttp == "POST" && ( actiontype == "Create" || actiontype == "Edit" ) && action.cvname != "")
                    {
                        // here we need to know the parent model access
                        // TO DO - many to many relationships... not today

                    }
                }

                                
                if(thisactionhttp == "GET")
                {
                    // if there is a facade, this will handle the defaults
                    if(nofacade || action.cfname=="")
                    {
                        // if this is a Create, we need to check whether there is a parent
                        // if this is Edit/Delete/Details, we need to retrieve the current record AND verify the parent
                        // in either case, if there is Identity, we also need to check whether this user owns the relevant item
                        // (checking parent tables to AspNetUsers)

                        if(actiontype == "Create")
                        {
                            // Create:GET
                            // in the corner case that this is a GET + Create + its parent is the Identity table
                            // then it will not have an id passed-in
                            if(parentmodel == identitytable )
                                sb.Append(this.IndentText($"string {idpropname} = {thisobjectvm}.{action.cvukey.Trim()};",nsindent+indent+indent));

                             sb.AppendLine( 
                                this.IndentText(
                                    this.GetSelectOneQueryText(
                                        action.cmname,
                                        dbpropname,
                                        action.cmparent,
                                        action.cmparkey,
                                        "",
                                        "",
                                        action.cmfkey,
                                        idpropname,
                                        identitytableid, 
                                        thisobjectvmuserid,
                                        indent,
                                        objectmodel,
                                        noasync)
                                    ,nsindent+indent+indent));

                            sb.Append(this.IndentText($"if({objectmodel}==null)",nsindent+indent+indent));
                            sb.Append(this.IndentText( "{",nsindent+indent+indent));
                            // if(action.cvfkey.Trim() != "")
                            //     sb.Append(this.IndentText($"{objectvm}.{action.cvfkey.Trim()} = {idpropname};",nsindent+indent+indent+indent));
                            sb.Append(this.IndentText(this.GetModelVmDefaultValueText(source, vmname, "vm", objectmodel, objectvm, action.cvpkey.Trim() + ',' + action.cvfkey.Trim()+',' + action.cvukey.Trim()),nsindent+indent+indent+indent));
                            sb.Append(this.IndentText( "}",nsindent+indent+indent));
                            sb.Append(this.IndentText( "else",nsindent+indent+indent));
                            sb.Append(this.IndentText( "{",nsindent+indent+indent));
                            sb.Append(this.IndentText( "// handle an item that already exists",nsindent+indent+indent+indent));
                            sb.Append(this.IndentText( "}",nsindent+indent+indent));

                        }
                        else if(actiontype == "Index")
                        {
                            // Index:GET
                            // sb.AppendLine( 
                            //     this.IndentText(
                            //         this.GetSelectOneQueryText(
                            //             action.cmname,
                            //             dbpropname,
                            //             action.cmparent,
                            //             "",
                            //             "",
                            //             action.cmfkey,
                            //             "id",
                            //             ukeyparameter, 
                            //             ukeyname, 
                            //             indent)
                            //         ,nsindent+indent+indent));
                        }
                        else
                        {
                            // Details/Edit/Delete:GET
                            sb.AppendLine( 
                                this.IndentText(
                                    this.GetSelectOneQueryText(
                                        action.cmname,
                                        dbpropname,
                                        action.cmparent,
                                        action.cmparkey,
                                        action.cmpkey,
                                        idpropname,
                                        "",
                                        "",
                                        identitytableid, 
                                        thisobjectvmuserid,
                                        indent,
                                        objectmodel,
                                        noasync)
                                    ,nsindent+indent+indent));

                            sb.Append(this.IndentText($"if({objectmodel}!=null)",nsindent+indent+indent));
                            sb.Append(this.IndentText( "{",nsindent+indent+indent));
                            sb.Append(
                                this.IndentText(
                                    this.GetModelToVmMappingText(
                                        source,
                                        action.cmname,
                                        action.cvname,
                                        "vm",
                                        objectmodel,
                                        thisobjectvm,
                                        action.cvpkey.Trim() + ',' + action.cvfkey.Trim() + ',' + action.cvukey.Trim())
                                    ,nsindent+indent+indent+indent)
                                );
                            sb.Append(this.IndentText( "}",nsindent+indent+indent));
                            sb.Append(this.IndentText( "else",nsindent+indent+indent));
                            sb.Append(this.IndentText( "{",nsindent+indent+indent));
                            sb.Append(this.IndentText( "// handle an item that cannot be found",nsindent+indent+indent+indent));
                            sb.Append(this.IndentText( "}",nsindent+indent+indent));

                        }
                    }
                    else
                    {
                        // there is a facade so we can rely on that to do the GET things
                        if(source.cdcontext == "")
                        {
                            sb.AppendLine(new string(' ', nsindent+indent+indent) + $"var facade = new {action.cfname}();");
                        }
                        else
                        {
//                            sb.AppendLine(new string(' ', nsindent+indent+indent) + $"var facade = new {action.cfname}({action.cvname},{dbpropname}));");
                            sb.AppendLine(new string(' ', nsindent+indent+indent) + $"var facade = new {action.cfname}({dbpropname});");
                        }

                        // if this is a GET, the userid and/or fkey and/or id fields will already be populated
                        // also note that if there is a message field then this can have the relevant error
                        sb.AppendLine(new string(' ', nsindent+indent+indent) + $"if(!facade.Pull({thisobjectvm}))");
                        sb.AppendLine(new string(' ', nsindent+indent+indent+indent) + $"return View({thisobjectvm});");
                        //
                    }
                }
                else
                {
                    // this is an http POST action
                    // by this point, the vm is passed-in, had the userid and/or fkey and/or id fields set, and is validated by the Controller
                    // now we need to check:
                    // existence of the parent object
                    // ownership of the parent object
                    // validation in the business model
                    //
                    // if there is no Identity, the ownership part can be skipped
                    if(nofacade || action.cfname=="")
                    {
                        // get either the existing model
                        // we need to know the parent-->grandparent-->greatgrandparent-->etc
                        // 
                        // however we only specify the parent on 
                        if(actiontype == "Create")
                        {
                            // Create:POST
                            if(parentmodel == identitytable )
                                sb.Append(this.IndentText($"string {idpropname} = {thisobjectvm}.{action.cvukey.Trim()};",nsindent+indent+indent));

                            sb.AppendLine( 
                                this.IndentText(
                                    this.GetSelectOneQueryText(
                                        action.cmname,
                                        dbpropname,
                                        action.cmparent,
                                        action.cmparkey,
                                        "",
                                        "",
                                        action.cmfkey,
                                        idpropname,
                                        identitytableid, 
                                        thisobjectvmuserid,
                                        indent,
                                        objectmodel,
                                        noasync
                                        )
                                    ,nsindent+indent+indent));

                            sb.Append(this.IndentText($"if({objectmodel}==null)",nsindent+indent+indent));
                            sb.Append(this.IndentText( "{",nsindent+indent+indent));


                            sb.Append( 
                                this.IndentText(
                                    this.GetSelectOneQueryText(
                                        action.cmname,
                                        "",
                                        action.cmparent,
                                        action.cmparkey,
                                        action.cmpkey,
                                        "System.Guid.NewGuid()",
                                        action.cmfkey,
                                        idpropname,
                                        identitytableid, 
                                        thisobjectvmuserid,
                                        indent,
                                        objectmodel,
                                        noasync, 
                                        true
                                        )
                                    ,nsindent+indent+indent+indent));
                            sb.Append(this.IndentText( "// see above",nsindent+indent+indent));
                            sb.Append(this.IndentText(this.GetModelToVmMappingText(source,action.cmname,action.cvname,"model",objectmodel,thisobjectvm,action.cvpkey.Trim() + ',' + action.cvfkey.Trim() + ',' + action.cvukey.Trim()),nsindent+indent+indent+indent));
                            sb.Append(this.IndentText( "}",nsindent+indent+indent));
                            sb.Append(this.IndentText( "else",nsindent+indent+indent));
                            sb.Append(this.IndentText( "{",nsindent+indent+indent));
                            sb.Append(this.IndentText( "// handle an item that already exists",nsindent+indent+indent+indent));
                            sb.Append(this.IndentText( "}",nsindent+indent+indent));


                        }
                        else if(actiontype == "Index")
                        {
                            // Index:POST
                            // sb.AppendLine( 
                            //     this.IndentText(
                            //         this.GetSelectOneQueryText(
                            //             action.cmname,
                            //             dbpropname,
                            //             action.cmparent,
                            //             "",
                            //             "",
                            //             action.cmfkey,
                            //             "id",
                            //             ukeyparameter, 
                            //             ukeyname, 
                            //             indent)
                            //         ,nsindent+indent+indent));
                        }
                        else
                        {
                            // Details/Edit/Delete:POST
                            sb.AppendLine( 
                                this.IndentText(
                                    this.GetSelectOneQueryText(
                                        action.cmname,
                                        dbpropname,
                                        action.cmparent,
                                        action.cmparkey,
                                        action.cmpkey,
                                        idpropname,
                                        "",
                                        "",
                                        identitytableid, 
                                        thisobjectvmuserid,
                                        indent,
                                        objectmodel,
                                        noasync)
                                    ,nsindent+indent+indent));

                            sb.Append(this.IndentText($"if({objectmodel}!=null)",nsindent+indent+indent));
                            sb.Append(this.IndentText( "{",nsindent+indent+indent));

                            sb.Append(this.IndentText(this.GetModelToVmMappingText(source,action.cmname,action.cvname,"model",objectmodel,thisobjectvm,""),nsindent+indent+indent+indent));
                            sb.Append(this.IndentText( "}",nsindent+indent+indent));
                            sb.Append(this.IndentText( "else",nsindent+indent+indent));
                            sb.Append(this.IndentText( "{",nsindent+indent+indent));
                            sb.Append(this.IndentText( "// handle an item that cannot be found",nsindent+indent+indent+indent));
                            sb.Append(this.IndentText( "}",nsindent+indent+indent));


                        }
                    }
                    else
                    {
                        if(source.cdcontext == "")
                        {
                            sb.AppendLine(new string(' ', nsindent+indent+indent) + $"var facade = new {action.cfname}();");
                        }
                        else
                        {
                            sb.AppendLine(new string(' ', nsindent+indent+indent) + $"var facade = new {action.cfname}({dbpropname});");
                        }

                        // if this is a GET, the userid and/or fkey and/or id fields will already be populated
                        // also note that if there is a message field then this can have the relevant error
                        sb.AppendLine(new string(' ', nsindent+indent+indent) + $"if(!facade.Push({thisobjectvm}))");
                        sb.AppendLine(new string(' ', nsindent+indent+indent+indent) + $"return View({thisobjectvm});");
                    }

                    if(!nodbsave)
                    {
                        // we need to do the save in the Controller
                        if(!noasync)
                        {
                            sb.AppendLine(new string(' ', nsindent+indent+indent) + $"await {dbpropname}.SaveChangesAsync();");
                        }
                        else
                        {
                            sb.AppendLine(new string(' ', nsindent+indent+indent) + $"{dbpropname}.SaveChanges();");
                        }
                    }
                }



                // This is complex and really requires integration testing
                // TO DO: fill this in!
                sb.AppendLine(new string(' ', nsindent+indent+indent) + $"return View({thisobjectvm});");

                // // if the model and viewmodel are the same
                // if(source.cnouser && source.cnofacade && false)
                // //  && source.cnoasync && action.cactview.Trim() == "")
                // {
                //     sb.AppendLine(new string(' ', nsindent+indent+indent) + $"var model = db.Subject.Include(s => s.SubjectModel);");
                //     sb.AppendLine(new string(' ', nsindent+indent+indent) + "return View(await model.ToListAsync());");
                // }
                // if(!source.cnouser && source.cnofacade && false)
                // {
                //     sb.AppendLine(new string(' ', nsindent+indent+indent) + "var lmdbContext = db.Subject.Include(s => s.SubjectModel);");
                //     sb.AppendLine(new string(' ', nsindent+indent+indent) + "return View(await lmdbContext.ToListAsync());");
                // }


                sb.AppendLine(new string(' ', nsindent+indent) + "}");

            }

            return sb.ToString();
        }

        public string GetSelectOneQueryText(
            string table,
            string context = "", 
            string parents = "", 
            string parentkeyfieldname = "", 
            string pkeyfieldname = "", 
            string pkeyparameter = "", 
            string fkeyfieldname = "", 
            string fkeyparameter = "", 
            string ukeyfieldname = "", 
            string ukeyparameter = "", 
            int indent = 4,
            string objectmodel = "", 
            bool noasync = true,
            bool novar = false
        )
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            char separator = '.';
            string[] parentitems = parents.Trim().Split(separator,StringSplitOptions.None);
            string[] parentidfields = parentkeyfieldname.Trim().Split(separator,StringSplitOptions.None);
            string firstmethod = "First";
            string awaittext = " ";
            string vartext = "var ";
            if(!noasync)
            {
                firstmethod = "FirstAsync";
                awaittext = " await ";
            }
            if(novar)
                vartext = "";

            if(objectmodel == "")
                objectmodel = table;

            if(objectmodel == "")
            {
                return "";
            }
            else if(context == "")
            {
                // there is no parent table, just grab the data
                sb.AppendLine($"{vartext}{objectmodel} = new {table}();");
                if(pkeyparameter != "" && pkeyfieldname != "")
                    sb.AppendLine($"{objectmodel}.{pkeyfieldname} = {pkeyparameter};");
                if(fkeyparameter != "" && fkeyfieldname != "")
                    sb.AppendLine($"{objectmodel}.{fkeyfieldname} = {fkeyparameter};");
            }
            else if(parentitems[0] == "" && pkeyfieldname != "" && pkeyparameter != "")
            {
                // there is no parent table, just grab the data
                sb.AppendLine($"{vartext}{objectmodel} ={awaittext}{context}.{table}.{firstmethod}(x => x.{pkeyfieldname}.Equals({pkeyparameter});");
            }
            else if(parentitems.Length == 1 && pkeyfieldname != "" && pkeyparameter != "" && fkeyfieldname != "" && fkeyparameter != "" && ukeyparameter == "" && ukeyfieldname == "")
            {
                // there is exactly one parent table and no Identity.
                // we need to check that this both exists and is owned by the correct foreign key
                sb.AppendLine($"{vartext}{objectmodel} ={awaittext}{context}.{table}.Include(x => x.{parentitems[0]}).{firstmethod}(y => y.{pkeyfieldname}.Equals({pkeyparameter}) && y.{parentitems[0]}.{parentidfields[0]}.Equals({fkeyparameter}));");
            }
            else if(parentitems.Length == 1 && pkeyfieldname != "" && pkeyparameter != "" && fkeyfieldname != "" && fkeyparameter == ukeyparameter && ukeyfieldname == parentidfields[0])
            {
                // there is exactly one parent table and this uses Identity.
                // The table must be the AspNetUsers table.
                // we need to check that this both exists and is owned by the correct foreign key
                sb.AppendLine($"{vartext}{objectmodel} ={awaittext}{context}.{table}.Include(x => x.{parentitems[0]}).{firstmethod}(y => y.{pkeyfieldname}.Equals({pkeyparameter}) && y.{parentitems[0]}.{parentidfields[0]}.Equals({fkeyparameter}));");
            }
            else if(pkeyfieldname != "" && pkeyparameter != "" && ukeyparameter != "" && ukeyfieldname != "")
            {
                // parentitems.Length > 1 && 

                // this is the main Edit/Details/Delete query where there are grandparents and this uses Identity.
                // The very top table must be the AspNetUsers table.
                // we need to check that this both exists and is owned by the correct foreign key
                sb.AppendLine($"{vartext}{objectmodel} ={awaittext}{context}.{table}");
                string modelchain = "";
                for(int i = 0; i < parentitems.Length;i++)
                {
                    modelchain += parentitems[i] + '.';
                    sb.AppendLine(new string(' ', indent) + $".Include(x => x.{modelchain.TrimEnd('.')})");
                }
                sb.AppendLine(new string(' ', indent) + $".{firstmethod}(y => y.{pkeyfieldname}.Equals({pkeyparameter})");
                if(fkeyfieldname != "" && fkeyparameter != "")
                    sb.AppendLine(new string(' ', indent + indent) + $"&& y.{parentitems[0]}.{parentidfields[0]}.Equals({fkeyparameter})");
                sb.AppendLine(new string(' ', indent + indent) + $"&& y.{modelchain}{ukeyfieldname}.Equals({ukeyparameter}));");
            }
            else if(( pkeyfieldname == "" || pkeyparameter == "" )  &&  fkeyfieldname != "" && fkeyparameter != "")
            {
                // this is the main Create query where we need to check ownership by a parent and possibly ownership in the Identity table.
                // The very top table must be the AspNetUsers table.
                // we need to check that this both exists and is owned by the correct foreign key
                sb.AppendLine($"{vartext}{objectmodel} ={awaittext}{context}.{table}");
                if(ukeyparameter == "" && ukeyfieldname == "")
                {
                    sb.AppendLine(new string(' ', indent) + $".{firstmethod}(y => y.{fkeyfieldname}.Equals({fkeyparameter})");
                }
                else
                {
                    string modelchain = "";
                    for(int i = 0; i < parentitems.Length;i++)
                    {
                        modelchain += parentitems[i] + '.';
                        sb.AppendLine(new string(' ', indent) + $".Include(x => x.{modelchain.TrimEnd('.')})");
                    }
                    sb.AppendLine(new string(' ', indent) + $".{firstmethod}(y => y.{fkeyfieldname}.Equals({fkeyparameter})");
                    sb.AppendLine(new string(' ', indent + indent) + $"&& y.{modelchain}{ukeyfieldname}.Equals({ukeyparameter}));");
                }
            }

            return sb.ToString();
        }


        public string GetSetVmUserIdControllerText(mvvm source, controlleraction action, string objectvm = "")
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            string useranonname = source.useranonname;
            string useranonval = source.useranonval;
            string thisobjectvm = source.objectvm;
            if(objectvm != "")
                thisobjectvm = objectvm;


// this is very hacky-y in the most recent version of EF and Identity
//            var username = HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Subject!.Name;
//            vmelection.userid = (_context.Users.First(x => x.UserName.Equals(username ?? ""))!.Id ?? "").ToLower().Replace("-","");
            sb.AppendLine($"{thisobjectvm}.{action.cvukey} = \"\";");
            sb.AppendLine($"System.Security.Claims.Claim claim = this.User.Claims.Where(x => x.Type.Contains(\"nameidentifier\")).FirstOrDefault(new System.Security.Claims.Claim(\"{useranonname}\",\"{useranonval}\"));");
            sb.AppendLine($"if(claim.Type!=\"{useranonname}\")");
            sb.AppendLine("{");

            if(action.cvukey != "")
            {
                // if the vm has a userid field, check its size - if string(32) then this is a truncated Guid,  if string(36) then this is a regular Guid
                var genfield = source.genfields.First(x => x.vname.Equals(action.cvname) && x.vfname.Equals(action.cvukey));
                if(genfield == null)
                    return "";
                if(genfield.vftype.ToLower().Contains("string") && genfield.vfsize.Equals(32))
                {
                    sb.AppendLine(new string(' ', source.indent) + $"{thisobjectvm}.{genfield.vfname} = claim.Value.ToLower().Replace(\"-\",\"\");");
                }
                else if(genfield.vftype.ToLower().Contains("guid"))
                {
                    sb.AppendLine(new string(' ', source.indent) + $"{thisobjectvm}.{genfield.vfname} = new System.Guid(thisclaim.Value);");
                }
                else
                {
                // if(genfield.vftype.ToLower().Contains("string") && ( genfield.vfsize.Equals(36) || genfield.vfsize.Equals(0)) )
                    sb.AppendLine(new string(' ', source.indent) + $"{thisobjectvm}.{genfield.vfname} = claim.Value;");
                }
            }
        
            sb.AppendLine("}");
            sb.AppendLine("else");
            sb.AppendLine("{");
            sb.AppendLine(new string(' ', source.indent) + $"// handle anonymous entry here");
            sb.AppendLine("}");

            // sb.AppendLine($"var username = HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Subject!.Name;");
            // sb.AppendLine($"vmelection.userid = (_context.Users.First(x => x.UserName.Equals(username ?? \"\"))!.Id ?? \"\").ToLower().Replace(\"-\",\"\");");



            return sb.ToString();
        }


        public bool ActionHasIdParameter(string actiontype, string http, string modelfkey, string modelparent, string identitytable)
        {

            // check for whether this action has an id field
            if((actiontype == "edit" || actiontype == "details" || actiontype == "delete") && modelfkey != "" && modelparent != "" )
            {
                return true;
            }
            else if(actiontype == "create")
            {
                // if the create action has no parent OR the parent is the user table
                //    then this has no id
                if(modelfkey=="" || modelparent =="" || modelparent == identitytable )
                {

                }
                else
                {
                    return true;
                }

            }
            else if(actiontype == "index")
            {
                // if the index action has no parent OR the parent is the user table
                //    then this has no id
                if(modelfkey == "" || modelparent == "" || modelparent == identitytable )
                {

                }
                else
                {
                    return true;
                }
            }            
            return false;
        }
        public bool ActionHasVmParameter(string actiontype, string http, string vmname)
        {
            actiontype = actiontype.Trim().ToLower();
            if((actiontype == "edit" || actiontype == "create" || actiontype == "delete" || actiontype == "details") && vmname !="" && http == "POST")
                return true;            
            return false;
        }
        public bool ActionNeedsVmCreation(string actiontype, string http, string vmname)
        {
            actiontype = actiontype.Trim().ToLower();
            if(vmname =="")
                return false;
            if((actiontype == "edit" || actiontype == "create" || actiontype == "delete" || actiontype == "details") && http == "POST")
                return false;
            return true;
        }

        public string GetControllerActionParameterText(mvvm source, controlleraction action, string http)
        {
            // some of the actions will have id fields
            // this will be either the pkey (edit, details, delete) or fkey (create, index)
            // 
            bool hasidparameter = false;
            bool hasvmparameter = false;

            string idpropname = source.cactionparameter;
            string actiontype = action.cacttype.Trim().ToLower();
            string vmname = action.cvname;
            string identitytable = source.identitytable;
            string identitytableid = source.identitytableid;
            string useranonname = source.useranonname;
            string useranonval = source.useranonval;
            string objectmodel = source.objectmodel;
            string objectvm = source.objectvm;
            string modelparent = action.cmparent;
            string modelparentpkey = action.cmparkey;
            string modelfkey = action.cmfkey;


            if(actiontype == "")
                actiontype = "none";

            hasidparameter = this.ActionHasIdParameter(actiontype, http, modelfkey, modelparent, identitytable);
            hasvmparameter = this.ActionHasVmParameter(actiontype, http, vmname);


            // check whether there is a vm
            // note that this is only for POST methods


            // if there is an id, this is first
            string idtext = "";
            string bindtext = "";
            if(hasidparameter)
                idtext = "string " + idpropname;

            if(hasvmparameter)
            {
                string bindfields = string.Join(",", (from vfield in source.genfields where vfield.vname == vmname select vfield.vfname).ToList());
                if(bindfields != "")
                {
                    bindtext = $"[Bind(\"{bindfields}\")] {vmname} {vmname}";
                }
            }
            if(bindtext != "" && idtext != "")
                return idtext + ',' + bindtext;
            return idtext + bindtext;
        }

#endregion generate-controller

#region generate-facade
 
        public string GetFacadeInnerText(mvvm source)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            int indent = source.indent;
            int nsindent = 0;
            if(source.fnamespace != "")
                nsindent = indent;

            bool noidentity = source.cnouser;
            bool nofacade = source.cnofacade;
            bool dbsave = source.fdbsave;
            bool noasync = source.cnoasync;
            
            string dbpropname = "db";
            if(source.cdpropname.Trim() != "")
                dbpropname = source.cdpropname.Trim();
            string vmname = source.vname;
            string modelname = source.mname;
            string modelpkey = source.mpkey;
            string modelfkey = source.mfkey;
            string modelparents = source.mparent;
            string modelparent = source.mparent.Trim().Split('.',StringSplitOptions.None)[0];
            string modelparkey = source.mparkey;
            string facadename = source.fname;
            string facadetype = source.ftype;

            // string identityname = "AspNetUsers";
            string identitytable = source.identitytable;
            string identitytableid = source.identitytableid;
            string useranonname = source.useranonname;
            string useranonval = source.useranonval;
            string objectmodel = source.objectmodel;
            string objectvm = source.objectvm;

            string vmpkey = source.vpkey;
            string vmfkey = source.vfkey;
            string vmukey = source.vuserkey;
            string vmmsg = source.vmessage;

            int asyncindent = 0;
            if(!noasync)
                asyncindent += indent;

/*
        // there are three methods in the Facade: Pull,Push,Default
*/
            List<string> methods = new List<string>(){"Pull","Push","Default"};
            foreach(var method in methods)
            {
                string linetext = "async Task<bool>";
                string entrytext = "var output = Task.Run(async () => {";
                string returntext = "await output;";
                string parameters = $"{vmname} {objectvm}";
                if(noasync)
                {
                    linetext = "bool";
                    returntext = "output;";
                    entrytext = "bool output = true;";
                }
                if(method=="Default")
                {
                    linetext = linetext.Replace("bool",vmname);
                    parameters = "";
                }

                // don't create this method
                if(method =="Push" && facadetype == "select")
                    continue;

                sb.AppendLine(new string(' ', nsindent+indent) + $"public {linetext} {method}({parameters})");
                sb.AppendLine(new string(' ', nsindent+indent) + "{");
                sb.AppendLine(new string(' ', nsindent+indent+indent) + entrytext);

                // if the Default then set values and exit

                // therefore when building the Facade we must see if this ViewModel has a pkey/fkey field
                if(method =="Default")
                {
                    sb.AppendLine(new string(' ', nsindent+indent+indent+asyncindent) + $"// set default values for '{vmname}' here");
                    sb.AppendLine(new string(' ', nsindent+indent+indent+asyncindent) + $"var {objectvm} = new {vmname}();");
                    sb.Append(this.IndentText(this.GetModelVmDefaultValueText(source, vmname, "vm", objectmodel, objectvm ),nsindent+indent+indent+asyncindent));
                    sb.AppendLine(new string(' ', nsindent+indent+indent+asyncindent) + $"return {objectvm};");
                    if(!noasync)
                        sb.AppendLine(new string(' ', nsindent+indent+indent) + "});");
                    sb.AppendLine(new string(' ', nsindent+indent+indent) + "return " + returntext);
                    sb.AppendLine(new string(' ', nsindent+indent) + "}");
                    continue;
                }




                // if there is no pkey and no fkey then we simply return false
                // note that where the vm simply does not need the database then this should return true
                // the idea is that the Controller is 'dumb' and a non-db call is a user decision

                // therefore when building the Facade we must see if this ViewModel has a pkey/fkey field
                if(vmpkey == "" )
                {
                    // no pkey field exists - this is a simple vm
            // var modelasync = Task.Run(() => {
            //     return true;
            //     });
            // // var model = await modelasync;
            // return await modelasync;

                    sb.AppendLine(new string(' ', nsindent+indent+indent+asyncindent) + $"// no pkey is specified on '{vmname}'");
                    sb.AppendLine(new string(' ', nsindent+indent+indent+asyncindent) + $"// add additional logic here");
                    sb.AppendLine(new string(' ', nsindent+indent+indent+asyncindent) + $"return true;");
                    if(!noasync)
                        sb.AppendLine(new string(' ', nsindent+indent+indent) + "});");
                    sb.AppendLine(new string(' ', nsindent+indent+indent) + "return " + returntext);
                    sb.AppendLine(new string(' ', nsindent+indent) + "}");
                    continue;
                }

                // we need to select/insert/delete according to the parameters
                // note that the Pull() method is different for each type but the Pull() is the same

                // create the Pull() first
                if(modelname == "" || modelpkey == "")
                {
                    sb.AppendLine(new string(' ', nsindent+indent+indent+asyncindent) + $"// no Model has been specified");
                    sb.AppendLine(new string(' ', nsindent+indent+indent+asyncindent) + $"// add additional logic here to populate '{vmname}'");
                    sb.AppendLine(new string(' ', nsindent+indent+indent+asyncindent) + $"return true;");
                    if(!noasync)
                        sb.AppendLine(new string(' ', nsindent+indent+indent) + "});");
                    sb.AppendLine(new string(' ', nsindent+indent+indent) + "return " + returntext);
                    sb.AppendLine(new string(' ', nsindent+indent) + "}");
                    continue;
                }
                if(method =="Pull" && ( vmpkey != "" && vmfkey != "" && vmukey != "" ) )
                {
                    // this is a 'simple' SELECT
                    sb.AppendLine(new string(' ', nsindent+indent+indent+asyncindent) + $"// query {modelname} for a record checking parent+identity");
                    sb.Append( 
                        this.IndentText(
                            this.GetSelectOneQueryText(
                                modelname,
                                dbpropname,
                                modelparents,
                                modelparkey,
                                modelpkey,
                                objectvm + "." + vmpkey,
                                modelfkey,
                                objectvm + "." + vmfkey,
                                identitytableid,
                                objectvm + "." + vmukey,
                                indent,
                                objectmodel,
                                noasync)
                            ,nsindent+indent+indent+asyncindent));

                    sb.Append(this.IndentText($"if({modelname} == null)",nsindent+indent+indent+asyncindent));
                    if(vmmsg == "")
                    {
                        sb.AppendLine(this.IndentText("return false;",nsindent+indent+indent+indent+asyncindent));
                    }
                    else
                    {
                        sb.Append(this.IndentText( "{",nsindent+indent+indent+asyncindent));
                        sb.Append(this.IndentText( objectvm + "."+vmmsg+" = $\"Cannot find record associated with id:{" + objectvm + "."+vmpkey + "}." + '"' + ';' ,nsindent+indent+indent+indent+asyncindent));
                        sb.Append(this.IndentText( "return false;",nsindent+indent+indent+indent+asyncindent));
                        sb.Append(this.IndentText( "}",nsindent+indent+indent+asyncindent));
                    }
                    sb.Append(this.IndentText(this.GetModelToVmMappingText(source, modelname, vmname, "vm","",objectvm,vmpkey + ',' + vmfkey + ',' + vmukey),nsindent+indent+indent+asyncindent));

            // string table,
            // string context = "", 
            // string parents = "", 
            // string parentkeyfieldname = "", 
            // string pkeyfieldname = "", 
            // string pkeyparameter = "", 
            // string fkeyfieldname = "", 
            // string fkeyparameter = "", 
            // string ukeyfieldname = "", 
            // string ukeyparameter = "", 

                    sb.AppendLine(new string(' ', nsindent+indent+indent+asyncindent) + $"return true;");
                    if(!noasync)
                        sb.AppendLine(new string(' ', nsindent+indent+indent) + "});");
                    sb.AppendLine(new string(' ', nsindent+indent+indent) + "return " + returntext);
                    sb.AppendLine(new string(' ', nsindent+indent) + "}");
                    continue;
                }


                if(method =="Push" && ( vmpkey != "" && vmfkey != "" && vmukey != "" ) )
                {
                    // for every Push() we check the existence of a record matching this id
                    //
                    // the logic is simple for SQL DELETE/UPDATE: check the existence of the record
                    // if found, remove it or update it and return true
                    // if not found, leave a message and return false
                    //
                    // for SQL INSERT: check the existence of the record
                    // if found, leave a message and return false
                    // if not found, create it and return true
                    //
                    

                    //   for a push with pkey set, grab the model (or default), set the model fields (if found), then update
                    //   for a push with no pkey (or a new one), grab the parent, get the default model, set the model fields (if found), then add
                    if(facadetype == "delete" || facadetype == "update" )
                    {
                        sb.AppendLine(new string(' ', nsindent+indent+indent+asyncindent) + $"// check whether this {modelname} exists (including parent+identity ownership)");
                        sb.Append( 
                            this.IndentText(
                                this.GetSelectOneQueryText(
                                    modelname,
                                    dbpropname,
                                    modelparents,
                                    modelparkey,
                                    modelpkey,
                                    objectvm + "." + vmpkey,
                                    modelfkey,
                                    objectvm + "." + vmfkey,
                                    identitytableid,
                                    objectvm + "." + vmukey,
                                    indent,
                                    objectmodel,
                                    noasync)
                                ,nsindent+indent+indent+asyncindent));
                        sb.Append(this.IndentText($"if({modelname} == null)",nsindent+indent+indent+asyncindent));


                        if(vmmsg == "")
                        {
                            sb.AppendLine(this.IndentText("return false;",nsindent+indent+indent+indent+asyncindent));
                        }
                        else
                        {
                            sb.Append(this.IndentText( "{",nsindent+indent+indent+asyncindent));
                            sb.Append(this.IndentText( objectvm + "."+vmmsg+" = $\"Cannot find record associated with id:{" + objectvm + "."+vmpkey + "}." + '"' + ';',nsindent+indent+indent+indent+asyncindent));
                            sb.Append(this.IndentText( "return false;",nsindent+indent+indent+indent+asyncindent));
                            sb.Append(this.IndentText( "}",nsindent+indent+indent+asyncindent));
                        }
                        if(facadetype == "update" )
                        {
                            // map the update fields here
                            sb.Append(this.IndentText(this.GetModelToVmMappingText(source, modelname, vmname, "model","",objectvm,modelpkey+','+modelfkey),nsindent+indent+indent+asyncindent));
                            sb.Append(this.IndentText(dbpropname + ".Update(" + modelname + ");",nsindent+indent+indent+asyncindent));
                        }
                        else
                        {
                            sb.Append(this.IndentText(dbpropname + ".Remove(" + modelname + ");",nsindent+indent+indent+asyncindent));
                        }
                        if(dbsave)
                        {
                            // the double-negative means that this is NOT saved in the Controller so do it here
                            sb.Append(this.IndentText("bool success = true;",nsindent+indent+indent+asyncindent));
                            sb.Append(this.IndentText("try",nsindent+indent+indent+asyncindent));
                            sb.Append(this.IndentText("{",nsindent+indent+indent+asyncindent));
                            if(noasync)
                            {
                                sb.Append(this.IndentText($"{dbpropname}.SaveChanges();",nsindent+indent+indent+indent+asyncindent));
                            }
                            else
                            {
                                sb.Append(this.IndentText($"await {dbpropname}.SaveChangesAsync();",nsindent+indent+indent+indent+asyncindent));
                            }
                            sb.Append(this.IndentText("}",nsindent+indent+indent+asyncindent));
                            sb.Append(this.IndentText("catch",nsindent+indent+indent+asyncindent));
                            sb.Append(this.IndentText("{",nsindent+indent+indent+asyncindent));
                            sb.Append(this.IndentText("success = false;",nsindent+indent+indent+indent+asyncindent));
                            if(vmmsg == "")
                                sb.Append(this.IndentText( objectvm + "."+vmmsg+" = $\"Cannot save changes to id:{"+objectvm + "." + vmpkey + "}." + '"' + ';',nsindent+indent+indent+indent+asyncindent));
                            sb.Append(this.IndentText("}",nsindent+indent+indent+asyncindent));
                            sb.Append(this.IndentText($"if(success == false)",nsindent+indent+indent+asyncindent));
                            sb.Append(this.IndentText( objectvm + "."+vmmsg+" = $\"Cannot "+facadetype+" record associated with id:{" + objectvm + "." + vmpkey + "}." + '"' + ';' ,nsindent+indent+indent+indent+asyncindent));
                            sb.AppendLine(new string(' ', nsindent+indent+indent+asyncindent) + $"return success;");
                            if(!noasync)
                                sb.AppendLine(new string(' ', nsindent+indent+indent) + "});");
                            sb.AppendLine(new string(' ', nsindent+indent+indent) + "return " + returntext);
                            sb.AppendLine(new string(' ', nsindent+indent) + "}");
                            continue;
                        }
                    }
                    else
                    {
                        // doing a SQL INSERT
                        // 
                        // this code will operate differently if there is:
                        // pkey value
                        // fkey + parent table
                        // ukey + identity table

                        // if there is no parent + fkey and no identity, but a pkey value passed-in 
                        //    then we need to see if this exists
                        //    if it does, don't insert a new record
                        //    if it does not, insert a new record with this as the pkey
                        // else if there is no parent + fkey and no identity and no pkey value passed-in 
                        //    then generate the pkey and insert a new record

                        if(modelfkey == "" && noidentity)
                        {
                            sb.AppendLine(new string(' ', nsindent+indent+indent+asyncindent) + $"// check whether this {modelname} exists");
                            sb.Append( 
                                this.IndentText(
                                    this.GetSelectOneQueryText(
                                        modelname,
                                        dbpropname,
                                        modelparents,
                                        modelparkey,
                                        modelpkey,
                                        objectvm + "."+ vmpkey,
                                        "",
                                        "",
                                        "",
                                        "",
                                        indent,
                                        objectmodel,
                                        noasync)
                                    ,nsindent+indent+indent+asyncindent));
                            sb.Append(this.IndentText($"if({modelname} != null)",nsindent+indent+indent+asyncindent));
                            if(vmmsg == "")
                            {
                                sb.AppendLine(this.IndentText("return false;",nsindent+indent+indent+indent+asyncindent));
                            }
                            else
                            {
                                sb.Append(this.IndentText( "{",nsindent+indent+indent+asyncindent));
                                sb.Append(this.IndentText( objectvm + "."+vmmsg+" = $\"Cannot create a new record as id:{" + objectvm + "." + vmpkey + "} exists." + '"' + ';',nsindent+indent+indent+indent+asyncindent));
                                sb.Append(this.IndentText( "return false;",nsindent+indent+indent+indent+asyncindent));
                                sb.Append(this.IndentText( "}",nsindent+indent+indent+asyncindent));
                            }
                        }
                        else if(modelfkey != "" && noidentity)
                        {
                            // there is a parent - here we check whether the parent exists
                            sb.AppendLine(new string(' ', nsindent+indent+indent+asyncindent) + $"// check whether the parent {modelparent} exists");
                            sb.Append( 
                                this.IndentText(
                                    this.GetSelectOneQueryText(
                                        modelname,
                                        dbpropname,
                                        modelparents,
                                        modelparkey,
                                        modelpkey,
                                        objectvm + "."+ vmpkey,
                                        modelfkey,
                                        objectvm + "." + vmfkey,
                                        "",
                                        "",
                                        indent,
                                        objectmodel,
                                        noasync)
                                    ,nsindent+indent+indent+asyncindent));
                            sb.Append(this.IndentText($"if({modelname} == null)",nsindent+indent+indent+asyncindent));
                            if(vmmsg == "")
                            {
                                sb.AppendLine(this.IndentText("return false;",nsindent+indent+indent+indent+asyncindent));
                            }
                            else
                            {
                                sb.Append(this.IndentText( "{",nsindent+indent+indent+asyncindent));
                                sb.Append(this.IndentText( objectvm + "."+vmmsg+" = $\"Cannot create a new record as "+modelparent+"."+modelparkey+" = {"+objectvm + "." + vmfkey + "} does not exist." + '"' + ';',nsindent+indent+indent+indent+asyncindent));
                                sb.Append(this.IndentText( "return false;",nsindent+indent+indent+indent+asyncindent));
                                sb.Append(this.IndentText( "}",nsindent+indent+indent+asyncindent));
                            }
                        }
                        else if(modelfkey != "" && vmukey != "" && !noidentity)
                        {
                            // there is a parent - here we check whether the parent exists
                            sb.AppendLine(new string(' ', nsindent+indent+indent+asyncindent) + $"// check whether the parent {modelparent} exists and is owned by this user");
                            sb.Append( 
                                this.IndentText(
                                    this.GetSelectOneQueryText(
                                        modelname,
                                        dbpropname,
                                        modelparents,
                                        "",
                                        "",
                                        objectvm + "."+ vmpkey,
                                        modelfkey,
                                        objectvm + "."+ vmfkey,
                                        identitytableid,
                                        objectvm + "."+ vmukey,
                                        indent,
                                        objectmodel,
                                        noasync)
                                    ,nsindent+indent+indent+asyncindent));
                            sb.Append(this.IndentText($"if({modelname} == null)",nsindent+indent+indent+asyncindent));
                            if(vmmsg == "")
                            {
                                sb.AppendLine(this.IndentText("return false;",nsindent+indent+indent+indent+asyncindent));
                            }
                            else
                            {
                                sb.Append(this.IndentText( "{",nsindent+indent+indent+asyncindent));
                                sb.Append(this.IndentText( objectvm + "."+vmmsg+" = $\"Cannot create a new record as "+modelparent+"."+modelparkey+" = {" + objectvm + "." + vmfkey + "} does not exist." + '"' + ';',nsindent+indent+indent+indent+asyncindent));
                                sb.Append(this.IndentText( "return false;",nsindent+indent+indent+indent+asyncindent));
                                sb.Append(this.IndentText( "}",nsindent+indent+indent+asyncindent));
                            }
                        }

                        // map the create fields here
                        sb.Append(this.IndentText(this.GetModelToVmMappingText(source, modelname, vmname, "model","",objectvm,modelpkey+','+modelfkey),nsindent+indent+indent+asyncindent));


                        // try to save the newly-created record
                        sb.Append(this.IndentText(dbpropname + ".Add(" + modelname + ");",nsindent+indent+indent+asyncindent));
                        if(dbsave)
                        {
                            // the double-negative means that this is NOT saved in the Controller so do it here
                            sb.Append(this.IndentText("bool success = true;",nsindent+indent+indent+asyncindent));
                            sb.Append(this.IndentText("try",nsindent+indent+indent+asyncindent));
                            sb.Append(this.IndentText("{",nsindent+indent+indent+asyncindent));
                            if(noasync)
                            {
                                sb.Append(this.IndentText($"{dbpropname}.SaveChanges();",nsindent+indent+indent+indent+asyncindent));
                            }
                            else
                            {
                                sb.Append(this.IndentText($"await {dbpropname}.SaveChangesAsync();",nsindent+indent+indent+indent+asyncindent));
                            }
                            sb.Append(this.IndentText("}",nsindent+indent+indent+asyncindent));
                            sb.Append(this.IndentText("catch",nsindent+indent+indent+asyncindent));
                            sb.Append(this.IndentText("{",nsindent+indent+indent+asyncindent));
                            sb.Append(this.IndentText("success = false;",nsindent+indent+indent+indent+asyncindent));
                            if(vmmsg == "")
                                sb.Append(this.IndentText( objectvm + "."+vmmsg+" = $\"Cannot save changes to id:{"+objectvm + "." + vmpkey + "}." + '"' + ';',nsindent+indent+indent+indent+asyncindent));
                            sb.Append(this.IndentText("}",nsindent+indent+indent+asyncindent));
                            sb.Append(this.IndentText($"if(success == false)",nsindent+indent+indent+asyncindent));
                            sb.Append(this.IndentText( objectvm + "."+vmmsg+" = $\"Cannot "+facadetype+" record associated with id:{" + objectvm + "."+ vmpkey + "}." + '"' + ';' ,nsindent+indent+indent+indent+asyncindent));
                            sb.AppendLine(new string(' ', nsindent+indent+indent+asyncindent) + $"return success;");
                            if(!noasync)
                                sb.AppendLine(new string(' ', nsindent+indent+indent) + "});");
                            sb.AppendLine(new string(' ', nsindent+indent+indent) + "return " + returntext);
                            sb.AppendLine(new string(' ', nsindent+indent) + "}");
                            continue;
                        }
                    }


            // string table,
            // string context = "", 
            // string parents = "", 
            // string parentkeyfieldname = "", 
            // string pkeyfieldname = "", 
            // string pkeyparameter = "", 
            // string fkeyfieldname = "", 
            // string fkeyparameter = "", 
            // string ukeyfieldname = "", 
            // string ukeyparameter = "", 

                    if(!noasync)
                        sb.AppendLine(new string(' ', nsindent+indent+indent) + "});");
                    sb.AppendLine(new string(' ', nsindent+indent+indent) + "return " + returntext);
                    sb.AppendLine(new string(' ', nsindent+indent) + "}");
                    continue;
                }


                   // sb.AppendLine(new string(' ', nsindent+indent+indent+asyncindent) + $"// query {modelname} for a record without additional checks");

                // sb.AppendLine();
                // if(!noasync)
                // {
                //     sb.AppendLine(new string(' ', nsindent+indent) + $"public async Task<bool> Push({vmname} vm)");
                // }
                // else
                // {
                //     sb.AppendLine(new string(' ', nsindent+indent) + $"public bool Push({vmname} vm)");
                // }
                // sb.AppendLine(new string(' ', nsindent+indent) + "{");
                // sb.Append(this.IndentText(this.GetModelToVmMappingText(source, modelname, vmname, "model") ,nsindent+indent+indent));
                // sb.AppendLine(new string(' ', nsindent+indent) + "}");
                // sb.AppendLine();

                // if(!noasync)
                // {
                //     sb.AppendLine(new string(' ', nsindent+indent) + $"public async Task<{vmname}> Default()");
                // }
                // else
                // {
                //     sb.AppendLine(new string(' ', nsindent+indent) + $"public {vmname} Default()");
                // }

                // sb.AppendLine(new string(' ', nsindent+indent) + "{");
                // sb.Append(this.IndentText(this.GetModelVmDef aultValueText(source, vmname, "vm"),nsindent+indent+indent));
                // sb.AppendLine(new string(' ', nsindent+indent) + "}");


            }


            return sb.ToString();
        }

#endregion generate-facade

#region general-helper-methods

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

        public string GetModelToVmMappingText(mvvm source, string modelname, string vmname, string lhs = "model", string modelobject = "", string vmobject = "", string skipfields = "")
        {
            // ignore indents (these can be fixed later)
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            if(lhs == "")
                lhs = "vm";
            skipfields = skipfields.TrimStart(',').TrimEnd(',');
            if(skipfields != "")
                skipfields = ','+skipfields+',';

            // fields that have a one-to-one mapping are listed here
            foreach(var genfield in source.genfields.Where(x => x.mname.Equals(modelname) && x.vname.Equals(vmname) && x.mfname != "" && x.vfname!= ""))
            {
                // bool skippkey = false, bool skipfkey = false, bool skipukey = false
                if( (lhs == "model" && skipfields.Contains(','+genfield.mfname.Trim()+',') || 
                    (lhs == "vm" && skipfields.Contains(','+genfield.vfname.Trim()+','))) )
                    continue;

                string mtype = genfield.mftype.ToLower().Trim();
                string vtype = genfield.vftype.ToLower().Trim();
                if(mtype=="")
                    mtype = "string";
                if(vtype=="")
                    vtype = "string";

                string lhstype = mtype;
                string rhstype = vtype;
                string lhsname = modelname;
                if(modelobject != "")
                    lhsname = modelobject;
                string rhsname = vmname;
                if(vmobject != "")
                    rhsname = vmobject;
                string lhsfname = genfield.mfname;
                string rhsfname = genfield.vfname;

                if(lhs == "vm")
                {
                    lhstype = vtype;
                    rhstype = mtype;
                    lhsname = vmname;
                    if(vmobject != "")
                        lhsname = vmobject;
                    rhsname = modelname;
                    if(modelobject != "")
                        rhsname = modelobject;
                    lhsfname = genfield.vfname;
                    rhsfname = genfield.mfname;
                }

                //WrapProperty

                if(lhstype.Contains("string") && rhstype.Contains("string"))
                {
                    sb.AppendLine($"{lhsname}.{lhsfname} = {rhsname}.{rhsfname} ?? \"\";");
                }
                else if(lhstype.Contains("int32") && rhstype.Contains("string"))
                {
                    sb.AppendLine($"{lhsname}.{lhsfname} = System.Int32.Parse('0' + ({rhsname}.{rhsfname} ?? \"0\").ToCharArray().Where(Char.IsDigit)).PadLeft(9,'0'));");
                }
                else if(lhstype.Contains("bool") && rhstype.Contains("string"))
                {
                    sb.AppendLine($"{lhsname}.{lhsfname} = ( ({rhsname}.{rhsfname} ?? \"\").ToLower().StartsWith('t') || ({rhsname}.{rhsfname} ?? \"\").ToLower().StartsWith('1') || ({rhsname}.{rhsfname} ?? \"\").ToLower().StartsWith('y')  );");
                }
                else if(lhstype.Contains("date") && rhstype.Contains("string"))
                {
                    sb.AppendLine($"{lhsname}.{lhsfname} = System.DateTime.Parse({rhsname}.{rhsfname} ?? \"\");");
                }
                else if(lhstype.Contains("int") && rhstype.Contains("int"))
                {
                    sb.AppendLine($"{lhsname}.{lhsfname} = {rhsname}.{rhsfname} ?? 0;");
                }
                else if(lhstype.Contains("bool") && rhstype.Contains("bool"))
                {
                    sb.AppendLine($"{lhsname}.{lhsfname} = {rhsname}.{rhsfname} ?? false;");
                }
                else if(lhstype.Contains("double") && rhstype.Contains("double"))
                {
                    sb.AppendLine($"{lhsname}.{lhsfname} = {rhsname}.{rhsfname} ?? 0.00;");
                }
                else if(lhstype.Contains("float") && rhstype.Contains("float"))
                {
                    sb.AppendLine($"{lhsname}.{lhsfname} = {rhsname}.{rhsfname} ?? 0.00f;");
                }
                else if(lhstype.Contains("decimal") && rhstype.Contains("decimal"))
                {
                    sb.AppendLine($"{lhsname}.{lhsfname} = {rhsname}.{rhsfname} ?? 0.00m;");
                }
                else if(lhstype.Contains("long") && rhstype.Contains("long"))
                {
                    sb.AppendLine($"{lhsname}.{lhsfname} = {rhsname}.{rhsfname} ?? 0L;");
                }
                else if(lhstype.Contains("guid") && rhstype.Contains("guid"))
                {
                    sb.AppendLine($"{lhsname}.{lhsfname} = {rhsname}.{rhsfname} ?? System.Guid.NewGuid();");
                }
                else if(lhstype.Contains("date") && ( rhstype.Contains("date?") || rhstype.Contains("datetime?") ) )
                {
                    sb.AppendLine($"{lhsname}.{lhsfname} = {rhsname}.{rhsfname} ?? System.DateTime.Now;");
                }
                else if(lhstype.Contains("date") && rhstype.Contains("date"))
                {
                    sb.AppendLine($"{lhsname}.{lhsfname} = {rhsname}.{rhsfname};");
                }

            }
/*

dotnet aspnet-codegenerator controller --controllerName SubjectController -dc xo.Data.ApplicationDbContext --useSqlite --model Subject

c:\SNJW\code\shared\csgen.exe controller --cname ModelController --source "C:\SNJW\code\scriptloader\scriptloader-small.csv" --output c:\SNJW\code\xo\Controllers\ModelController.cs --cparent Controller --croute Model/Create --ccontext xo.Data.ApplicationDbContext --vpkey id --vfkey userid --vftable AspNetUsers --vuserkey userid --vmessage message  --mpkey id --mfkey userid --mftable AspNetUsers --chttps GET:GET/POST:GET/POST:GET/POST:GET --cactnames Index:Create:Edit:Delete:Details --cacttypes Index:Create:Edit:Delete:Details --cvnames model --cmnames Model --cwnames Index:Create:Edit:Delete:Details --cvpkeys id --cvfkeys userid --cmpkeys ModelId --cmfkeys ModelUserId --cmparents AspNetUsers --cvukeys userid --cvmsgs message --fillempty

*/

            return sb.ToString();
        }

        public string GetModelVmDefaultValueText(mvvm source, string name, string lhs = "model", string modelobject = "", string vmobject = "", string skipfields = "")
        {
            // ignore indents (these can be fixed later)
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            if(lhs == "")
                lhs = "vm";
            skipfields = skipfields.TrimStart(',').TrimEnd(',');
            if(skipfields != "")
                skipfields = ','+skipfields+',';

            // fields that have a one-to-one mapping are listed here
            foreach(var genfield in source.genfields.Where(x => (lhs == "model" && x.mname.Equals(name) && x.mfname != "") || (lhs == "vm" && x.vname.Equals(name) && x.vfname != "") ))
            {
                if( (lhs == "model" && skipfields.Contains(','+genfield.mfname.Trim()+',') || 
                    (lhs == "vm" && skipfields.Contains(','+genfield.vfname.Trim()+','))) )
                    continue;

                string lhstype = genfield.mftype.ToLower().Trim();
                if(lhs == "model")
                    lhstype = genfield.vftype.ToLower().Trim();
                if(lhstype=="")
                    lhstype = "string";

                string lhsname = genfield.mname;
                if(modelobject != "")
                    lhsname = modelobject;
                string lhsfname = genfield.mfname;
                if(lhs == "vm")
                {
                    lhsname = genfield.vname.Trim();
                    if(vmobject != "")
                        lhsname = vmobject;
                    lhsfname = genfield.vfname.Trim();
                }
                if(lhsname == "" || lhsfname == "")
                    continue;

                if(lhstype.Contains("string") )
                {
                    sb.AppendLine($"{lhsname}.{lhsfname} = \"\";");
                }
                else if(lhstype.Contains("int32") )
                {
                    sb.AppendLine($"{lhsname}.{lhsfname} = 0;");
                }
                else if(lhstype.Contains("bool"))
                {
                    sb.AppendLine($"{lhsname}.{lhsfname} = false;");
                }
                else if(lhstype.Contains("int"))
                {
                    sb.AppendLine($"{lhsname}.{lhsfname} = 0;");
                }
                else if(lhstype.Contains("bool"))
                {
                    sb.AppendLine($"{lhsname}.{lhsfname} = false;");
                }
                else if(lhstype.Contains("double"))
                {
                    sb.AppendLine($"{lhsname}.{lhsfname} = 0.00;");
                }
                else if(lhstype.Contains("float"))
                {
                    sb.AppendLine($"{lhsname}.{lhsfname} = 0.00f;");
                }
                else if(lhstype.Contains("decimal"))
                {
                    sb.AppendLine($"{lhsname}.{lhsfname} = 0.00m;");
                }
                else if(lhstype.Contains("long"))
                {
                    sb.AppendLine($"{lhsname}.{lhsfname} = 0L;");
                }
                else if(lhstype.Contains("guid"))
                {
                    sb.AppendLine($"{lhsname}.{lhsfname} = System.Guid.NewGuid();");
                }
                else if(lhstype.Contains("date"))
                {
                    sb.AppendLine($"{lhsname}.{lhsfname} = System.DateTime.Now;");
                }
            }
            return sb.ToString();
        }

        public string WrapProperty(string property, string rhstype, string lhstype = "string", int rhssize = 0, int lhssize = 0)
        {
            rhstype = rhstype.ToLower().Trim();
            lhstype = lhstype.ToLower().Trim();
            if(rhstype == "")
                rhstype = "string";
            if(lhstype == "")
                lhstype = "string";
            
            if(lhstype == "string" && lhssize == 32 && rhstype == "guid")
                return property+".ToString().ToLower().Replace(\"-\",\"\")";
            if(lhstype == "guid" && ( rhssize == 32 || rhssize == 36 ) && rhstype == "string")
                return "Guid.Parse("+property+")";
            if(lhstype == "string" && ( lhssize == 32 && ( rhssize == 32 || rhssize == 36 ) ) && rhstype == "string")
                return "Guid.Parse("+property+").ToString().ToLower().Replace(\"-\",\"\")";
            if(lhstype == "string" && rhstype.Contains("date"))
                return property+".ToString(\"yyyy-MM-dd\")";
            if(lhstype == "string" && rhstype != "string")
                return property+".ToString()";

            return property;
        }


        public string IndentText(string text, int indent)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            string[] lines = text.Split(Environment.NewLine,StringSplitOptions.None);
            for(int i = 0; i < lines.Length; i++)
            {
                if(i == (lines.Length - 1) &&  lines[lines.Length - 1] == "")
                    continue;
                sb.AppendLine(new string(' ',indent) + lines[i]);
            }
            return sb.ToString();
        }

#endregion general-helper-methods

#region parameter-helper-methods

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
                    singlename="fname",
                    pluralname="fnames",
                    synonym="fn",
                    isfilter = true,
                    ismvvmfield = true
                },
                new commafield(){
                    singlename="vfname",
                    pluralname="vfnames",
                    synonym="vfn",
                    ismvvmfield = true,
                    isunique = true
                },
                new commafield(){
                    singlename="vftype",
                    pluralname="vftypes",
                    synonym="vft",
                    ismvvmfield = true
                },
                new commafield(){
                    singlename="vfsize",
                    pluralname="vfsizes",
                    synonym="vfz",
                    ismvvmfield = true
                },
                new commafield(){
                    singlename="vfdesc",
                    pluralname="vfdescs",
                    synonym="vfc",
                    ismvvmfield = true
                },
                new commafield(){
                    singlename="vfreq",
                    pluralname="vfreqs",
                    synonym="vfq",
                    ismvvmfield = true
                },
                new commafield(){
                    singlename="vfcap",
                    pluralname="vfcaps",
                    synonym="vfa",
                    ismvvmfield = true
                },
                new commafield(){
                    singlename="mfname",
                    pluralname="mfnames",
                    synonym="mfn",
                    ismvvmfield = true,
                    isunique = true
                },
                new commafield(){
                    singlename="mftype",
                    pluralname="mftypes",
                    synonym="mft",
                    ismvvmfield = true
                },
                new commafield(){
                    singlename="mfsize",
                    pluralname="mfsizes",
                    synonym="mfs",
                    ismvvmfield = true
                },
                new commafield(){
                    singlename="wfclass",
                    pluralname="wfclasses",
                    synonym="wfe",
                    ismvvmfield = true
                },
                new commafield(){
                    singlename="wftype",
                    pluralname="wftypes",
                    synonym="wfy",
                    ismvvmfield = true
                },
                new commafield(){
                    singlename="wfdclass",
                    pluralname="wfdclasses",
                    synonym="wfd",
                    ismvvmfield = true
                },
                new commafield(){
                    singlename="wficlass",
                    pluralname="wficlasses",
                    synonym="wfi",
                    ismvvmfield = true
                },
                new commafield(){
                    singlename="cacthttp",
                    pluralname="cacthttps",
                    synonym="cap",
                    isctrlaction = true
                },
                new commafield(){
                    singlename="cactname",
                    pluralname="cactnames",
                    synonym="can",
                    isctrlaction = true,
                    isunique = true
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
                    singlename="cmparkey",
                    pluralname="cmparkeys",
                    synonym="cmy",
                    isctrlaction = true
                },
                new commafield(){
                    singlename="cvukey",
                    pluralname="cvukeys",
                    synonym="cvu",
                    isctrlaction = true
                },
                new commafield(){
                    singlename="cvmkey",
                    pluralname="cvmkeys",
                    synonym="cvm",
                    isctrlaction = true
                },
                new commafield(){
                    singlename="cvmsg",
                    pluralname="cvmsgs",
                    synonym="cvm",
                    isctrlaction = true
                },
                new commafield(){
                    singlename="cfname",
                    pluralname="cfnames",
                    synonym="cfn",
                    isctrlaction = true
                }


            };
        }
    }

#endregion parameter-helper-methods

#region parameter-helper-classes


    public class frmbutton
    {
        public string btnname = "";
        public string btntext = "";
        public string btntype = "";
        public string btnclass = "";
        public string btndclass = "";
        public string btniclass = "";
        public string btnonclick = "";
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
        public bool isunique = false;
    }

    public class mvvmfield
    {
        public string vname {get;set;} = "";  // ViewModel name
        public string mname {get;set;} = "";  // Model name
        public string wname {get;set;} = "";  // View name
        public string fname {get;set;} = "";  // Facade name

        // public bool ispkey {get;set;} = false;
        // public bool ismodel {get;set;} = false;
        // public bool isvm {get;set;} = false;

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
        // public string ccontext {get;set;} = "";  // Class name of the ApplicationContext
        // public bool cfacade {get;set;}  = true;  // Does Controller use a façade?
        // public bool cidentity {get;set;} = true;  // Does Controller use Identity?
        // public bool casync {get;set;} = true;  // Does Controller use asynchronous methods?
        // public bool cdbsave {get;set;} = true;  // Does Controller issue a dbsave?
        public bool cbinding {get;set;} = true;  // Does Controller bind individual fields?
        public string cacthttp {get;set;} = "";  // GET/SET action properties
        public string cactname {get;set;} = "";  // action name
        public string cactsyn {get;set;} = "";  // action synonyms
        public string cacttype {get;set;} = "";  // type (Create/Delete/Edit/Index/Details)


        public string cvname {get;set;} = "";  // ViewModel names
        // public string cactvmfld {get;set;} = "";  // ViewModel field names
        public string cmname {get;set;} = "";  // Model name
        public string cwname {get;set;} = "";  // View name
        public string cfname {get;set;} = "";  // Facade name
        public string cvpkey {get;set;} = "";  // Colon-delimited ViewModel primary key fields
        public string cvfkey {get;set;} = "";  // Colon-delimited ViewModel foreign key fields
        public string cmpkey {get;set;} = "";  // Colon-delimited Model primary key fields
        public string cmfkey {get;set;} = "";  // Colon-delimited Model foreign key fields
        public string cmparent {get;set;} = "";  // Colon-delimited Model parent(+grandparent) table names
        public string cmparkey {get;set;} = "";  // Colon-delimited Model parent(+grandparent) key names
        public string cvukey {get;set;} = "";  // Colon-delimited action ViewModel user key fields
        public string cvmsg {get;set;} = "";  // Colon-delimited action ViewModel message fields

    }


/// This is setup to create ONE file but it needs HEAPS of tangential infomration to do this
    public class mvvm
    {
        // These properties are set in code:
        public string category {get;set;} = "";
        public string categoryname {get;set;} = "";
        public int indent {get;set;} = 4;

        // these are set using System.Reflection:
        public string output {get;set;} = "";  //  Full path to output .cs file.
        public string source {get;set;} = "";  // Loads field properties from a CSV file.

        public bool fillempty {get;set;} = false;  // Where only one item is specified as a parameter, use it for ALL children where empty

        // public string fieldprefix = "";
        public string vname {get;set;} = "";
        public string vnamespace {get;set;} = "";

        public string mname {get;set;} = "";  // Model class name.
        public string mnamespace {get;set;} = "";  // Model namespace.
        public string fname {get;set;} = "";  // Facade class name
        public string fnamespace {get;set;} = "";  // Facade namespace
        public string ftype {get;set;} = "";  // Facade type (select/insert/delete/update)
        public bool fdbsave {get;set;} = false;  // Facade will issue dbsave commands
        public string wname {get;set;} = "";  // View name
        public string waction {get;set;} = "";  // View action (Index/Create/Edit/Delete/Details).

        public string mpkey {get;set;} = "";  // Specifies the primary key field in the Model.
        public string mfkey {get;set;} = "";  // Specifies the foreign key field in the Model.
        public string mparent {get;set;} = "";  // Specifies the parent (and grandparent(s)) of the Model.
        public string mparkey {get;set;} = "";  // Specifies the primary key in the parent of the Model.

        public string vpkey {get;set;} = "";  // Specifies the primary key field in the ViewModel.
        public string vfkey {get;set;} = "";  // Specifies the foreign key field in the ViewModel.
        public string vftable {get;set;} = "";  // Specifies the parent of the ViewModel.
        public string vuserkey {get;set;} = "";  // Specifies the userid field in the ViewModel.
        public string vmessage {get;set;} = "";  // Specifies a field in the ViewModel to relay messages.


        public string wbtnname {get;set;} = "";  // Colon-separated list of button names on the form.
        public string wbtntype {get;set;} = "";  // Colon-separated list of button types on the form.
        public string wbtntext {get;set;} = "";  // Colon-separated list of form button text.
        public string wbtnclass {get;set;} = "";  // Colon-separated list of form button CSS classes.
        public string wbtndclass {get;set;} = "";  // Colon-separated list of div classes to wrap the form button.
        public string wbtniclass {get;set;} = "";  // Colon-separated list of form button embedded <i> tags.
        public string wbtnonclick {get;set;} = "";  // Colon-separated list of JS code for form buttons.


        public string wsubmit {get;set;} = "";  // Type of Submit object on the form.
        public string wsubaction {get;set;} = "";  // Specifies the Submit action.
        public string wsubdclass {get;set;} = "";  // Colon-delimited CSS classes for the Submit object.
        public string wsubiclass {get;set;} = "";  // CSS class for an embedded <i> tag for the Submit object.
        public string wreturn {get;set;} = "";  // Type of Return object on the form.
        public string wretaction {get;set;} = "";  // Specifies the Return action.
        public string wretdclass {get;set;} = "";  // Colon-delimited CSS classes for the Return object.
        public string wreticlass {get;set;} = "";  // CSS class for an embedded <i> tag for the Return object.

        public string wscrlist {get;set;} = "";  // Colon-delimited list of .js files linked on this view
        public string wscrsection {get;set;} = "";  // name of the @section for scripts called in this view



        public string wfrmaction {get;set;} = "";  // Specifies the Form action (target URI).
        public string wfrmmethod {get;set;} = "";  // Specifies the Form method (GET/POST).
        public string wfrmclass {get;set;} = "";  // Colon-delimited CSS classes wrapping the Form object.
        public string wfrmsub {get;set;} = "";  // Colon-delimited CSS classes wrapping all objects inside the Form object.
        public string wfrmbtncss {get;set;} = "";  // Specifies CSS class to wrap all Form buttons.

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
        public string cdcontext {get;set;} = "";  // Class name of the ApplicationDbContext.
        public string cdpropname {get;set;} = "";  // Property name in each Contorller of the ApplicationDbContext.
        public string careaname {get;set;} = "";  // name of the Area this Controller is associated with

        public bool cnofacade {get;set;} = false;  //  Whether Controller uses a facade.
        public bool cnouser {get;set;} = false;  // Whether Controller uses Identity.
        public bool cnoasync {get;set;} = false;  // Whether Controller uses asynchronous methods.
        public bool cnodbsave {get;set;} = false;  // Whether Controller issues a dbsave command.
        public bool cnobinding {get;set;} = false;  // Whether Controller binds individual fields.
        public string cacthttps {get;set;} = "";  // Colon-delimited GET/SET action properties.
        public string cactnames {get;set;} = "";  // Colon-delimited action names.
        public string cactsyns {get;set;} = "";  // Colon-delimited action synonyms.
        public string cacttypes {get;set;} = "";  // Colon-delimited action types (Create/Delete/Edit/Index/Details).

        public string cvnames {get;set;} = "";  // ViewModel names
        public string cmnames {get;set;} = "";  // Model names
        public string cwnames {get;set;} = "";  // View names
        public string cvpkeys {get;set;} = "";  // Colon-delimited ViewModel primary key fields
        public string cvfkeys {get;set;} = "";  // Colon-delimited ViewModel foreign key fields
        public string cmpkeys {get;set;} = "";  // Colon-delimited Model primary key fields
        public string cmfkeys {get;set;} = "";  // Colon-delimited Model foreign key fields
        public string cmparents {get;set;} = "";  // Colon-delimited Model parent(+grandparent) table names
        public string cmparkeys {get;set;} = "";  // Colon-delimited Model parent(+grandparent) table key fields
        public string cvukeys {get;set;} = "";  // Colon-delimited action ViewModel user key fields
        public string cvmsgs {get;set;} = "";  // Colon-delimited action ViewModel message fields
        public string cfnames {get;set;} = "";  // Colon-delimited action Facade names

        public string identitytable {get;set;} = "User";  // The Identity table's Model name
        public string identitytableid {get;set;} = "Id";  // The AspNetUsers pkey
        public string useranonname {get;set;} = "Anonymous";  // Name when not logged-in
        public string useranonval {get;set;} = "00000000-0000-0000-0000-000000000000";  // ID when not logged-in
        public string objectmodel {get;set;} = "Item";  // variable name for a Model retrieved in the Facade/Controller
        public string objectvm {get;set;} = "vm";  // variable name for vm in Facade/Controller.  NOTE: the ACTUAL PARAMETER NAME in a Controller MUST MAKE THE NAME IN THE VIEW IN MVC!
        public string cactionparameter {get;set;} = "id";  // variable name for vm in Facade/Controller.  NOTE: the ACTUAL PARAMETER NAME in a Controller MUST MAKE THE NAME IN THE VIEW IN MVC!



        public List<mvvmfield> genfields = new List<mvvmfield>();
        public List<controlleraction> controlleractions = new List<controlleraction>();
        // public List<string> parentmodels = new List<string>();
        // public List<string> childmodels = new List<string>();
    }

#endregion parameter-helper-classes

}