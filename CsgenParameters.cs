
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
    public class CsgenParameters
    {
        public List<ParameterSetting> ps = new List<ParameterSetting>();
        public string appname = "csgen";
        public void SetParameterInfo()
        {
            ps.Clear();
            ps.AddRange(this.SetParameterInfoByCategory("--help"));
            ps.AddRange(this.SetParameterInfoByCategory("vm"));
            ps.AddRange(this.SetParameterInfoByCategory("view"));
            ps.AddRange(this.SetParameterInfoByCategory("facade"));
            ps.AddRange(this.SetParameterInfoByCategory("controller"));
            ps.AddRange(this.SetParameterInfoByCategory("model"));
            return;
        }
        private List<ParameterSetting> SetParameterInfoByCategory(string category)
        {
            List<ParameterSetting> output = new List<ParameterSetting>();

            if(category == "--help")
            {
                output.Add(new ParameterSetting(){
                    category = "--help",
                    setting = "--help",
                    synonym = "-h",
                    description = this.appname + " Help Information",
                    isactive = false,
                    input = "",
                    nextisactive = false,
                    nextinput = "",
                    required  = false,
                    helptext = new List<string>(){
                        "Usage: csgen [command] [options]",
                        "",
                        "Creates and edits text files to scaffold C# applications from the command-line.",
                        "",
                        "Command:",
                        "  replace           Performs a search-replace in text file.",
                        "  replacechar       Performs a search-replace with CHR() numbers in text file.",
                        "  replacen          Search and replace the nth items found in a text file.",
                        "  replacedq         Performs a search in text file for double-quotes.",
                        "  replacewithdq     Performs a replace in text file with double-quotes.",
                        "  insert            Inserts text at a specified line in a text file.",
                        "  model             Creates a .cs Model class.",
                        "  vm                Creates a .cs ViewModel class.",
                        "  view              Creates a .cshtml View file.",
                        "  controller        Creates a .cs Controller class.",
                        "  facade            Creates a .cs Facade class.",
                        "",
                        "Options:",
                        "  -h|--help         Display help for each command."
                    },
                    paratype = ParameterType.Switch,
                    paraintmin = 0,
                    paraintmax = 65535,
                    nextparatype = ParameterType.Any,
                    nextparaintmin = 0,
                    nextparaintmax = 65535
                });


            }
            if(category == "vm" || category == "view" || category == "model" || category == "controller" || category == "facade")
            {
                string categoryname = "ViewModel";
                string defaultfile = "newfile.cs";
                string fileextension = "cs";
                if(category != "vm")
                    categoryname = category[0].ToString().ToUpper() + category.Substring(1,category.Length-1);
                if(category == "view")
                    fileextension = "cshtml";
                defaultfile = "new"+categoryname.ToLower()+'.'+fileextension;

                ParameterSetting help = new ParameterSetting(){
                    category = category,
                    setting = "--help",
                    synonym = "-h",
                    description = $"{categoryname} Creation Help",
                    restriction = "",
                    input = "",
                    nextinput = "",
                    required  = false,
                    paratype = ParameterType.Switch,
                    paraintmin = 0,
                    paraintmax = 65535,
                    nextparatype = ParameterType.Any,
                    nextparaintmin = 0,
                    nextparaintmax = 65535
                };

                List<ParameterSetting> load = new List<ParameterSetting>();

                var helptext = new List<string>(){
                        $"Usage: csgen {category} [options]",
                        "",
                        $"Creates a {categoryname} file.",
                        "",
                        "Options:"};
                
                if(category != "model")
                {
                    helptext.Add("  -vn|--vname        ViewModel name.");
                    helptext.Add("  -vm|--vnamespace   ViewModel namespace.");
                    load.Add(new ParameterSetting(){
                            category = category,
                            setting = "--vname",
                            synonym = "-vn",
                            description = "ViewModel Name",
                            helptext = new List<string>(){
                                $"Usage: csgen {category} -vn viewmodelname",
                                "",
                                "Specify the name of the ViewModel.  This must be both valid in both HTML5 and C#.",
                                "If no name is specified, the filename (without '.cs') is used.  If neither are supplied, the ViewModel is named 'newviewmodel'.",
                            },
                            paratype = ParameterType.Input,
                            nextparatype = ParameterType.CsClassName
                        });
                    load.Add(new ParameterSetting(){
                            category = category,
                            setting = "--vnamespace",
                            synonym = "-vm",
                            description = "ViewModel Namespace",
                            helptext = new List<string>(){
                                $"Usage: csgen {category} -vm viewmodelnamespace",
                                "",
                                "Specify the name of the ViewModel namespace.  This must be valid in C#.",
                                "This parameter is option.  If no name is specified, the ViewModel class is created with no namespace.",
                            },
                            paratype = ParameterType.Input,
                            nextparatype = ParameterType.CsNameSpace
                        });

                }
                if(category == "view" || category == "controller" )
                {
                    helptext.Add("  -wn|--wname        View name.");
                    helptext.Add("  -wa|--waction      Structure of the View (Index/Create/Edit/Delete/Details).");
                    load.Add(new ParameterSetting(){
                            category = category,
                            setting = "--wname",
                            synonym = "-wn",
                            description = "View Name",
                            helptext = new List<string>(){
                                $"Usage: csgen {category} -wm viewname",
                                "",
                                "Specify the name of the View.  This is the text appearing in the <h3> tags inside the view.  This can be any text.",
                                "If not supplied, there will be no heading in the View."
                            },
                            paratype = ParameterType.Input,
                            nextparatype = ParameterType.Any
                        });
                    load.Add(new ParameterSetting(){
                            category = category,
                            setting = "--waction",
                            synonym = "-wa",
                            description = "View Action",
                            helptext = new List<string>(){
                                $"Usage: csgen {category} -wa viewaction",
                                "",
                                "Specify the structure of the View (Index/Create/Edit/Delete/Details).",
                                "If not supplied, this will default to Index."
                            },
                            paratype = ParameterType.Input,
                            nextparatype = ParameterType.Any
                        });
                }
                if(category == "model" || category == "controller" || category == "facade" )
                {
                    helptext.Add("  -mn|--mname        Model name.");
                    helptext.Add("  -mm|--mnamespace   Model namespace.");
                    load.Add(new ParameterSetting(){
                            category = category,
                            setting = "--mname",
                            synonym = "-mn",
                            description = "Model Name",
                            helptext = new List<string>(){
                                $"Usage: csgen {category} -mn modelname",
                                "",
                                "Specify the name of the Model.",
                                "If not supplied, the name will be 'newmodel'."
                            },
                            paratype = ParameterType.Input,
                            nextparatype = ParameterType.CsNameSpace
                        });
                    load.Add(new ParameterSetting(){
                            category = category,
                            setting = "--mnamespace",
                            synonym = "-mm",
                            description = "Model Namespace",
                            helptext = new List<string>(){
                                $"Usage: csgen {category} -mm modelnamespace",
                                "",
                                "Specify the name of the Model namespace.",
                                "If not supplied, no namespace will be used."
                            },
                            paratype = ParameterType.Input,
                            nextparatype = ParameterType.CsNameSpace
                        });
                }
                if(category == "controller" || category == "facade" )
                {
                    helptext.Add("  -fn|--fname        Facade name.");
                    helptext.Add("  -fm|--fnamespace   Facade namespace.");
                    load.Add(new ParameterSetting(){
                            category = category,
                            setting = "--fname",
                            synonym = "-fn",
                            description = "Facade Name",
                            helptext = new List<string>(){
                                $"Usage: csgen {category} -fn facadename",
                                "",
                                "Specify the name of the Facade.",
                                "If not supplied, the name will be 'newfacade'."
                            },
                            paratype = ParameterType.Input,
                            nextparatype = ParameterType.CsClassName
                        });
                    load.Add(new ParameterSetting(){
                            category = category,
                            setting = "--fnamespace",
                            synonym = "-fm",
                            description = "Facade Namespace",
                            helptext = new List<string>(){
                                $"Usage: csgen {category} -fm facadenamespace",
                                "",
                                "Specify the name of the Facade namespace.",
                                "If not supplied, no namespace will be used."
                            },
                            paratype = ParameterType.Input,
                            nextparatype = ParameterType.CsNameSpace
                        });
                }
                
                if(category == "facade" )
                {
                    helptext.Add("  -ft|--ftype        Facade type.");
                    helptext.Add("  -fdb|--fdbsave     Facade issues dbsave commands.");
                    load.Add(new ParameterSetting(){
                            category = category,
                            setting = "--ftype",
                            synonym = "-ft",
                            description = "Facade Type",
                            helptext = new List<string>(){
                                $"Usage: csgen {category} -ft facadetype",
                                "",
                                "Specify the type of the Facade.",
                                "",
                                "Each Facade has a Pull() which is SQL SELECT",
                                "",
                                "The type determines the Push() function:",
                                "",
                                "select   Push() is empty",
                                "insert   Push() is SQL INSERT",
                                "update   Push() is SQL UPDATE",
                                "delete   Push() is SQL DELETE",
                                "",
                                "If not supplied, the type will be 'select'."
                            },
                            paratype = ParameterType.Input,
                            nextparatype = ParameterType.CsClassName
                        });
                    load.Add(new ParameterSetting(){
                        category = category,
                        setting = "--fdbsave",
                        synonym = "-fdb",
                        description = "Put dbsave commands in the Facade",
                        helptext = new List<string>(){
                            $"Usage: csgen {category} -fdb",
                            "",
                            "By default the Controller will do the dbsave and not the Facade.",
                            "Set this to issue dbsave commands in the Facade."
                        },
                        paratype = ParameterType.Switch
                    });
                }


                helptext.Add($"  -o|--output        Full path to output .{fileextension} file.");
                load.Add(new ParameterSetting(){
                        category = category,
                        setting = "--output",
                        synonym = "-o",
                        description = "Output File",
                        helptext = new List<string>(){
                            $"Usage: csgen {category} -o outputfilename",
                            "",
                            "Specify the name of the file to be created and optionally the full path.",
                            "",
                            "If no full path is specified then the current directory is used.",
                            "This must be a valid filename and will be overwritten without notification.",
                            $"If not specified, the {category} name is used with '.{fileextension}' appended.",
                            $"If neither are specified, the file '{defaultfile}' is created in the current directory."

                        },
                        paratype = ParameterType.Input,
                        nextparatype = ParameterType.File
                    });
                helptext.Add($"  -s|--source        Loads field properties from a CSV file.");
                load.Add(new ParameterSetting(){
                        category = category,
                        setting = "--source",
                        synonym = "-s",
                        description = "Model/ViewModel Field Information Source File",
                        helptext = new List<string>(){
                            $"Usage: csgen {category} -s sourcefilename",
                            "",
                            "Specify the name of a CSV file containing fields.",
                            "",
                            "If no full path is specified then the current directory is checked.",
                            "",
                            "The order that fields are loaded is the natural order of the rows in the source file.",
                            "If the -f|--fieldnames option is also used, those fields will be added after any obtained through the -s|--source     load.",
                            "",
                            "Each item in the header row aligns with the relavent parameter:",
                            "vfnames,vftypes,vfsizes,vfdescs,vfreqs,vfcaps,mfnames,mftypes,wfclass,wftype, wfdclass, wficlass",
                            "",
                            "The CSV can optionally have vname or mname fields to filter records.",
                            "Only records that have a matching vname/mname to the --vname or --mname parameter will be used."
                        },
                        paratype = ParameterType.Input,
                        nextparatype = ParameterType.File
                    });

                if(category == "model" || category == "controller" || category == "facade" || category == "vm" || category == "view" )
                {
                    helptext.Add($"  -l|--fillempty     Fill in blank {category} records.");
                    load.Add(new ParameterSetting(){
                            category = category,
                            setting = "--fillempty",
                            synonym = "-l",
                            description = $"Fill {category.Substring(0,1).ToUpper()}{category.Substring(1,category.Length-1).ToUpper()} Field Information where blank",
                            helptext = new List<string>(){
                                $"Usage: csgen {category} -l",
                                "",
                                $"If selected, each {category} record in the source fields which has data in the first record will be cascaded to other records.",
                                "",
                                $"For example if the first three lines are:",
                                $"{category.Substring(0,1)}name='my{category}',{category.Substring(0,1)}fname='id',{category.Substring(0,1)}ftype='string' ",
                                $"{category.Substring(0,1)}name='my{category}',{category.Substring(0,1)}fname='code',{category.Substring(0,1)}ftype='' ",
                                $"{category.Substring(0,1)}name='my{category}',{category.Substring(0,1)}fname='name',{category.Substring(0,1)}ftype='' ",
                                "",
                                "Then if --fillempty is set, the second and third rows will also be set to 'string'"
                            },
                            paratype = ParameterType.Switch
                        });
                }
                    // helptext.Add($"                    Field properties are filtered by the 'mname' field matching the --mname property.");

                    



                // if(category == "model" || category == "controller" || category == "facade" )
                //     helptext.Add($"                    Field properties are filtered by the 'mname' field matching the --mname property.");
                // if(category == "vm" || category == "view" )
                //     helptext.Add($"                    Field properties are filtered by the 'vname' field matching the --vname property.");
                
                
                if(category != "model" )
                {
                    helptext.Add("  -vfn|--vfnames     Comma-separated list of ViewModel field names in order.");
                    helptext.Add("                     Syntax is vfname1[,vfname2][,...].");
                    helptext.Add("  -vft|--vftypes     Comma-separated list of ViewModel field types in order.");
                    helptext.Add("                     Syntax is vftype1[,vftype2][,...].");
                    helptext.Add("  -vfz|--vfsizes     Comma-separated list of ViewModel field sizes in order.");
                    helptext.Add("                     Syntax is vfsize1[,vfsize2][,...].");
                    helptext.Add("  -vfc|--vfdescs     Comma-separated list of ViewModel field descriptions in order.");
                    helptext.Add("                     Syntax is vfdesc1[,vfdesc2][,...].");
                    helptext.Add("  -vfq|--vfreqs      Comma-separated list of ViewModel field required text in order.");
                    helptext.Add("                     Syntax is vfreq1[,vfreqc2][,...].");
                    helptext.Add("  -vfa|--vfcaps      Comma-separated list of ViewModel field captions in order.");
                    helptext.Add("                     Syntax is vfcap1[,vfcap2][,...].");

                    load.Add(new ParameterSetting(){
                        category = category,
                        setting = "--vfnames",
                        synonym = "-vfn",
                        description = "ViewModel Field Names",
                        helptext = new List<string>(){
                            $"Usage: csgen {category} -vfn fieldnames",
                            "",
                            "Specify a comma-separated list of fieldnames.",
                            "",
                            "The format of this is:",
                            "vfname1[,vfname2][,...]",
                            "",
                            "If the -s|--source     option is also used, fields will be loaded from that file first.",
                            "",
                            "Each name must be valid as a CSS/HTML name."
                        },
                        paratype = ParameterType.Input,
                        nextparatype = ParameterType.HtmlFieldName,
                        nextparaseparator = ","
                    });

                    load.Add(new ParameterSetting(){
                        category = category,
                        setting = "--vftypes",
                        synonym = "-vft",
                        description = "ViewModel Field Types",
                        helptext = new List<string>(){
                            $"Usage: csgen {category} -vft fieldtypes",
                            "",
                            "Specify a comma-separated list of supported DataTypes associated with fields.",
                            "",
                            "The format of this is:",
                            "vftype1[,vftype2][,...]",
                            "",
                            "If the -s|--source     option is also used, fields will be loaded from that file first.",
                            "",
                            "If not specified,unknown, or unsupported this will default to string."
                        },
                        paratype = ParameterType.Input,
                        nextparatype = ParameterType.Any,
                        nextparaseparator = ","
                    });

                    load.Add(new ParameterSetting(){
                        category = category,
                        setting = "--vfsizes",
                        synonym = "-vfz",
                        description = "ViewModel Field Sizes",
                        helptext = new List<string>(){
                            $"Usage: csgen {category} -vfz fieldsizes",
                            "",
                            "Specify a comma-separated list of field sizes.",
                            "",
                            "The format of this is:",
                            "vfsize1[,vfsiz2][,...]",
                            "",
                            "Where the field size is set to zero then this is ignored."
                        },
                        paratype = ParameterType.Input,
                        nextparatype = ParameterType.Integer,
                        nextparaseparator = ",",
                        nextparaintmin = 0
                    });

                    load.Add(new ParameterSetting(){
                        category = category,
                        setting = "--vfdescs",
                        synonym = "-vfc",
                        description = "ViewModel Field Descriptions",
                        helptext = new List<string>(){
                            $"Usage: csgen {category} -vfc fielddescs",
                            "",
                            "Specify a comma-separated list of supported DataTypes associated with fields.",
                            "",
                            "The format of this is:",
                            "vfdesc1[,vfdesc2][,...]"
                        },
                        paratype = ParameterType.Input,
                        nextparatype = ParameterType.Any,
                        nextparaseparator = ","
                    });

                    load.Add(new ParameterSetting(){
                        category = category,
                        setting = "--vfreqs",
                        synonym = "-vfq",
                        description = "ViewModel Field Required Text",
                        helptext = new List<string>(){
                            $"Usage: csgen {category} -vfq fieldreqtext",
                            "",
                            "Specify a comma-separated list of validation text to appear.",
                            "",
                            "The format of this is:",
                            "vfreq1[,vfreq2][,...]"
                        },
                        paratype = ParameterType.Input,
                        nextparatype = ParameterType.Any,
                        nextparaseparator = ","
                    });

                    load.Add(new ParameterSetting(){
                        category = category,
                        setting = "--vfcaps",
                        synonym = "-vfa",
                        description = "ViewModel Field Captions",
                        helptext = new List<string>(){
                            $"Usage: csgen {category} -vfa fieldcaps",
                            "",
                            "Specify a comma-separated list of label text associated with fields.",
                            "",
                            "The format of this is:",
                            "vfcap1[,vfcap2][,...]"
                        },
                        paratype = ParameterType.Input,
                        nextparatype = ParameterType.Any,
                        nextparaseparator = ","
                    });

                }
                if(category == "model" || category == "controller" || category == "facade" )
                {
                    helptext.Add("  -mfn|--mfnames     Comma-separated list of Model field names in order.");
                    helptext.Add("                     Syntax is mfname1[,mfname2][,...].");
                    helptext.Add("  -mft|--mftypes     Comma-separated list of Model field types in order.");
                    helptext.Add("                     Syntax is mftype1[,mftype2][,...].");
                    helptext.Add("  -mfs|--mfsizes     Comma-separated list of Model field sizes in order.");
                    helptext.Add("                     Syntax is mfsize1[,mfsize2][,...].");

                    load.Add(new ParameterSetting(){
                        category = category,
                        setting = "--mfnames",
                        synonym = "-mfn",
                        description = "Model Field Names",
                        helptext = new List<string>(){
                            $"Usage: csgen {category} -mfn fieldnames",
                            "",
                            "Specify a comma-separated list of Model field names.",
                            "",
                            "The format of this is:",
                            "mfname1[,mfname2][,...]"
                        },
                        paratype = ParameterType.Input,
                        nextparatype = ParameterType.CsFieldName,
                        nextparaseparator = ","
                    });

                    load.Add(new ParameterSetting(){
                        category = category,
                        setting = "--mftypes",
                        synonym = "-mft",
                        description = "Model Field Types",
                        helptext = new List<string>(){
                            $"Usage: csgen {category} -mft fieldtypes",
                            "",
                            "Specify a comma-separated list of Model field types.",
                            "",
                            "The format of this is:",
                            "mftype1[,mftype2][,...]",
                            "",
                            "Field types that are empty, not C# data types, or not language shortcuts default to 'System.String'.",
                            "",
                            "Note that if only one item is entered and --fillblanks is used then this will apply to all items."
                        },
                        paratype = ParameterType.Input,
                        nextparatype = ParameterType.Any,
                        nextparaseparator = ","
                    });

                    load.Add(new ParameterSetting(){
                        category = category,
                        setting = "--mfsizes",
                        synonym = "-mfs",
                        description = "Model Field Sizes",
                        helptext = new List<string>(){
                            $"Usage: csgen {category} -mfs fieldsizes",
                            "",
                            "Specify a comma-separated list of Model field sizes.",
                            "",
                            "The format of this is:",
                            "mfsize1[,mfsize2][,...]",
                            "",
                            "Note that if only one item is entered and --fillblanks is used then this will apply to all items."
                        },
                        paratype = ParameterType.Input,
                        nextparatype = ParameterType.Integer,
                        nextparaseparator = ","
                    });

                }
                if(category == "view"  )
                {
                    helptext.Add("  -wwe|--wfclasses   Comma-separated list of View form field CSS classes in order.");
                    helptext.Add("                     Syntax is wfclass1[,wfclass2][,...].");
                    helptext.Add("  -wwy|--wftypes     Comma-separated list of View form field HTML types in order.");
                    helptext.Add("                     Syntax is wftype1[,wftype2][,...].");
                    helptext.Add("  -wwd|--wfdclasses  Comma-separated list of colon-delimited CSS classes in order to wrap a form field in <div> tags.");
                    helptext.Add("                     Syntax is wfdclass1a:wfdclass1b[:wfdclass1c][,wfdclass2a][,...].");
                    helptext.Add("  -wwi|--wficlasses  Comma-separated list of a CSS class for an <i> tag that follows a form field.");
                    helptext.Add("                     Syntax is wficlass1[,wficlass2][,...].");

                    load.Add(new ParameterSetting(){
                        category = category,
                        setting = "--wfclasses",
                        synonym = "-wwe",
                        description = "View Form Field CSS Classes",
                        helptext = new List<string>(){
                            $"Usage: csgen {category} -wwe cssclasses",
                            "",
                            "Specify a comma-separated list of View form field CSS classes in order.",
                            "",
                            "The format of this is:",
                            "wfclass1[,wfclass2][,...]",
                            "",
                            "This is the class of the field itself and contain spaces.",
                            "",
                            "Note that if only one item is entered and --fillblanks is used then this will apply to all items."
                        },
                        paratype = ParameterType.Input,
                        nextparatype = ParameterType.Any,
                        nextparaseparator = ","
                    });

                    load.Add(new ParameterSetting(){
                        category = category,
                        setting = "--wftypes",
                        synonym = "-wwy",
                        description = "View Form Field HTML Types",
                        helptext = new List<string>(){
                            $"Usage: csgen {category} -wwy fieldtypes",
                            "",
                            "Specify a comma-separated list of View form field HTML types in order.",
                            "",
                            "The format of this is:",
                            "wftype1[,wftype2][,...]",
                            "",
                            "This defaults to 'input' if missing or unrecognised.",
                            "",
                            "Note that if only one item is entered and --fillblanks is used then this will apply to all items."
                        },
                        paratype = ParameterType.Input,
                        nextparatype = ParameterType.Any,
                        nextparaseparator = ","
                    });

                    load.Add(new ParameterSetting(){
                        category = category,
                        setting = "--wfdclasses",
                        synonym = "-wwd",
                        description = "View Form Field <div> wrapper CSS Classes",
                        helptext = new List<string>(){
                            $"Usage: csgen {category} -wwd cssclasses",
                            "",
                            "Specify a comma-separated list of colon-delimited CSS classes in order to wrap a form field in <div> tags.",
                            "",
                            "The format of this is:",
                            "wfdclass1a:wfdclass1b[:wfdclass1c][,wfdclass2a][,...]",
                            "",
                            "If there are colons, the particular form object is wrapped in nested <div> tags.",
                            "Otherwise you can simply have spaces between class names.",
                            "",
                            "Note that if only one item is entered and --fillblanks is used then this will apply to all items."
                        },
                        paratype = ParameterType.Input,
                        nextparatype = ParameterType.Any,
                        nextparaseparator = ","
                    });

                    load.Add(new ParameterSetting(){
                        category = category,
                        setting = "--wficlasses",
                        synonym = "-wwi",
                        description = "View Form Field <i> wrapper CSS Classes",
                        helptext = new List<string>(){
                            $"Usage: csgen {category} -wwi cssclasses",
                            "",
                            "Specify a comma-separated list of CSS classes for an <i> tag that follows a form field.",
                            "",
                            "The format of this is:",
                            "wficlass1[,wficlass2][,...]",
                            "",
                            "Note that if only one item is entered and --fillblanks is used then this will apply to all items."
                        },
                        paratype = ParameterType.Input,
                        nextparatype = ParameterType.Any,
                        nextparaseparator = ","
                    });
                }

                if(category == "view" || category == "vm" || category == "controller" || category == "facade" )
                {
                    helptext.Add("  -vfk|--vpkey       Specifies the primary key field in the ViewModel.");
                    helptext.Add("  -vfy|--vfkey       Specifies the foreign key field in the ViewModel.");
                    helptext.Add("  -vfb|--vftable     Specifies the parent of the ViewModel.");
                    helptext.Add("  -vpc|--vchild      Specifies the children of the ViewModel.");
                    load.Add(new ParameterSetting(){
                        category = category,
                        setting = "--vpkey",
                        synonym = "-vfk",
                        description = "ViewModel Primary Key Field",
                        helptext = new List<string>(){
                            $"Usage: csgen {category} -vfk primarykey",
                            "",
                            "Specify the primary key field in the ViewModel."
                        },
                        paratype = ParameterType.Input,
                        nextparatype = ParameterType.Any,
                        nextparaseparator = ","
                    });
                    load.Add(new ParameterSetting(){
                        category = category,
                        setting = "--vfkey",
                        synonym = "-vfy",
                        description = "ViewModel Parent Foreign Key Field",
                        helptext = new List<string>(){
                            $"Usage: csgen {category} -vfy foreignkey",
                            "",
                            "Specify the foreign key field in the ViewModel."
                        },
                        paratype = ParameterType.Input,
                        nextparatype = ParameterType.Any,
                        nextparaseparator = ","
                    });
                    load.Add(new ParameterSetting(){
                        category = category,
                        setting = "--vftable",
                        synonym = "-vfb",
                        description = "ViewModel Parent Table",
                        helptext = new List<string>(){
                            $"Usage: csgen {category} -vfb parenttable",
                            "",
                            "Specify the parent of this ViewModel."
                        },
                        paratype = ParameterType.Input,
                        nextparatype = ParameterType.Any,
                        nextparaseparator = ","
                    });
                    load.Add(new ParameterSetting(){
                        category = category,
                        setting = "--vchild",
                        synonym = "-vpc",
                        description = "ViewModel Child ViewModel",
                        helptext = new List<string>(){
                            $"Usage: csgen {category} -vpc childvm",
                            "",
                            "Specify the children (and lower) name for this ViewModel.",
                            "",
                            "The format of this is:",
                            "cvchild1[.cvgrandchild1][.cvgreatgrandchild1][...][+cvchild2][.cvgrandchild2][.cvgreatgrandchild2][...]",
                            "",
                            "Subsequent period-delimited Models (grandchildren) are used for cross-checking.",
                            "",
                            "Where there are multiple children, then each 'chain' is separated by a plus ('+')."
                        },
                        paratype = ParameterType.Input,
                        nextparatype = ParameterType.CsFieldName,
                        nextparaseparator = "+"
                    });

                }
                if(category == "vm" || category == "controller" || category == "facade" || category == "view" )
                {
                    helptext.Add("  -vfu|--vuserkey    Specifies the userid field in the ViewModel.");
                    helptext.Add("  -vfm|--vmessage    Specifies a field in the ViewModel to relay messages.");
                    load.Add(new ParameterSetting(){
                        category = category,
                        setting = "--vuserkey",
                        synonym = "-vfu",
                        description = "ViewModel UserId Field",
                        helptext = new List<string>(){
                            $"Usage: csgen {category} -vfu useridfield",
                            "",
                            "Specify a field that the ViewModel uses to check and pass UserId details."
                        },
                        paratype = ParameterType.Input,
                        nextparatype = ParameterType.Any
                    });
                    load.Add(new ParameterSetting(){
                        category = category,
                        setting = "--vmessage",
                        synonym = "-vfm",
                        description = "ViewModel Message Field",
                        helptext = new List<string>(){
                            $"Usage: csgen {category} -vfm messagefield",
                            "",
                            "Specify a field that the ViewModel uses to pass messages."
                        },
                        paratype = ParameterType.Input,
                        nextparatype = ParameterType.Any
                    });
                }

                if(category == "model" || category == "controller" || category == "facade" )
                {
                    helptext.Add("  -mpk|--mpkey       Specifies the primary key field in the Model.");
                    helptext.Add("  -mfk|--mfkey       Specifies the foreign key field in the Model.");
                    helptext.Add("  -mpt|--mparent     Specifies the parent of the Model.");
                    helptext.Add("  -mpc|--mchild      Specifies the children of the Model.");
                    helptext.Add("  -mpp|--mparkey     Specifies the primary key field in parent of the Model.");

                    load.Add(new ParameterSetting(){
                        category = category,
                        setting = "--mpkey",
                        synonym = "-mpk",
                        description = "Model Primary Key Field",
                        helptext = new List<string>(){
                            $"Usage: csgen {category} -mpk primarykey",
                            "",
                            "Specify the primary key field in the Model."
                        },
                        paratype = ParameterType.Input,
                        nextparatype = ParameterType.CsFieldName
                    });
                    load.Add(new ParameterSetting(){
                        category = category,
                        setting = "--mfkey",
                        synonym = "-mfk",
                        description = "Model Parent Foreign Key Field",
                        helptext = new List<string>(){
                            $"Usage: csgen {category} -mfk foreignkey",
                            "",
                            "Specify the foreign key field in the Model."
                        },
                        paratype = ParameterType.Input,
                        nextparatype = ParameterType.CsFieldName
                    });
                    load.Add(new ParameterSetting(){
                        category = category,
                        setting = "--mparent",
                        synonym = "-mpt",
                        description = "Model Parent Table",
                        helptext = new List<string>(){
                            $"Usage: csgen {category} -mpt parenttable",
                            "",
                            "Specify the parent (and higher) name for this Model.",
                            "",
                            "The format of this is:",
                            "cmparent1[.cmgrandparent1][.cmgreatgrandparent1][...][+cmparent2[.cmgrandparent2][.cmgreatgrandparent2][...]",
                            "",
                            "Subsequent period-delimited Models (grandparents) are used for cross-checking ownership with Identity.",
                            "If this is the case, the final item should be '.AspNetUsers' or whatever the Identity table name is.",
                            "",
                            "Where there are multiple parents (such as a many-to-many linking table), then each 'chain' is separated by a plus ('+')."

                        },
                        paratype = ParameterType.Input,
                        nextparatype = ParameterType.CsNameSpace,
                        nextparaseparator = "+"
                    });
                    load.Add(new ParameterSetting(){
                        category = category,
                        setting = "--mchild",
                        synonym = "-mpc",
                        description = "Model Child Table",
                        helptext = new List<string>(){
                            $"Usage: csgen {category} -mpc childtable",
                            "",
                            "Specify the children (and lower) name for this Model.",
                            "",
                            "The format of this is:",
                            "cmchild1[.cmgrandchild1][.cmgreatgrandchild1][...][+cmchild2][.cmgrandchild2][.cmgreatgrandchild2][...]",
                            "",
                            "Subsequent period-delimited Models (grandchildren) are used for cross-checking.",
                            "",
                            "Where there are multiple children, then each 'chain' is separated by a plus ('+')."
                        },
                        paratype = ParameterType.Input,
                        nextparatype = ParameterType.CsNameSpace,
                        nextparaseparator = "+"
                    });
                    load.Add(new ParameterSetting(){
                        category = category,
                        setting = "--mparkey",
                        synonym = "-mpp",
                        description = "Model Parent Table Primary Key",
                        helptext = new List<string>(){
                            $"Usage: csgen {category} -mpp parenttableid",
                            "",
                            "Specify the primary key parent of this Model."
                        },
                        paratype = ParameterType.Input,
                        nextparatype = ParameterType.CsFieldName
                    });
                }
                // if(category == "model" || category == "controller" || category == "facade" )
                // {

                //     helptext.Add("  -fu|--vuserkey    Specifies the userid field in the ViewModel.");
                //     helptext.Add("  -fm|--vmessage    Specifies a field in the ViewModel to relay messages.");
                //     load.Add(new ParameterSetting(){
                //         category = category,
                //         setting = "--vuserkey",
                //         synonym = "-fu",
                //         description = "ViewModel UserId Field",
                //         helptext = new List<string>(){
                //             $"Usage: csgen {category} -fu useridfield",
                //             "",
                //             "Specify a field that the ViewModel uses to check and pass UserId details."
                //         },
                //         paratype = ParameterType.Input,
                //         nextparatype = ParameterType.Any,
                //         nextparaseparator = ","
                //     });
                //     load.Add(new ParameterSetting(){
                //         category = category,
                //         setting = "--vmessage",
                //         synonym = "-fm",
                //         description = "ViewModel Message Field",
                //         helptext = new List<string>(){
                //             $"Usage: csgen {category} -fm messagefield",
                //             "",
                //             "Specify a field that the ViewModel uses to pass messages."
                //         },
                //         paratype = ParameterType.Input,
                //         nextparatype = ParameterType.Any,
                //         nextparaseparator = ","
                //     });
                // }



                if(category == "view" )
                {
                    helptext.Add("  -wbn|--wbtnname    Colon-separated list of button names on the form.");
                    helptext.Add("  -wby|--wbtntype    Colon-separated list of button types on the form.");
                    helptext.Add("  -wbt|--wbtntext    Colon-separated list of form button text.");
                    helptext.Add("  -wbc|--wbtnclass   Colon-separated list of form button CSS classes.");
                    helptext.Add("  -wbd|--wbtndclass  Colon-separated list of CSS classes wrapping form buttons.");
                    helptext.Add("  -wbi|--wbtniclass  Colon-separated list of form button embedded <i> tags.");
                    helptext.Add("  -wbo|--wbtnonclick Colon-separated list of JS code for form buttons.");

                    helptext.Add("  -wst|--wsubmit     Type of Submit object on the form.");
                    helptext.Add("  -wsa|--wsubaction  Specifies the Submit action.");
                    helptext.Add("  -wsd|--wsubdclass  Colon-delimited CSS classes for the Submit object.");
                    helptext.Add("  -wsi|--wsubiclass  CSS class for an embedded <i> tag for the Submit object.");
                    helptext.Add("  -wrt|--wreturn     Type of Return object on the form.");
                    helptext.Add("  -wra|--wretaction  Specifies the Return action.");
                    helptext.Add("  -wrd|--wretdclass  Colon-delimited CSS classes for the Return object.");
                    helptext.Add("  -wri|--wreticlass  CSS class for an embedded <i> tag for the Return object.");

                    helptext.Add("  -wfa|--wfrmaction  Specifies the Form action.");
                    helptext.Add("  -wfm|--wfrmmethod  Specifies the Form method.");
                    helptext.Add("  -wfc|--wfrmclass   Colon-delimited CSS classes wrapping the Form object.");
                    helptext.Add("  -wfs|--wfrmsub     Colon-delimited CSS classes wrapping all objects inside the Form object.");
                    helptext.Add("  -wfb|--wfrmbtncss  Specify the CSS class to wrap all Form buttons.");


                    helptext.Add("  -wjl|--wscrlist    Colon-delimited list of .js files linked on this view.");
                    helptext.Add("  -wjn|--wscrsection Name of the @section for scripts called in this view.");



                    helptext.Add("  -wpc|--wpageclass  Colon-delimited CSS classes wrapping the Info and Form sections.");
                    helptext.Add("  -wic|--winfoclass  Colon-delimited CSS classes wrapping the Info section above form fields.");
                    helptext.Add("  -wih|--winfohclass CSS class of the heading in the Info section.");
                    helptext.Add("  -wid|--winfohead   Heading for the information section.");
                    helptext.Add("  -wit|--winfotext   Text for the information section.");
                    helptext.Add("  -wlf|--wlayfiles   Colon-separated list of Layout cshtml files associated with --laynames.");
                    helptext.Add("  -wln|--wlaynames   Colon-separated list of @section names.");
                    helptext.Add("  -wlm|--wlayout     Name of the primary Layout.cshtml file.");


                    load.Add(new ParameterSetting(){
                        category = category,
                        setting = "--wbtnname",
                        synonym = "-wbn",
                        description = "List of button names on the form",
                        helptext = new List<string>(){
                            $"Usage: csgen {category} -wbn btnnames",
                            "",
                            "Specify a colon-separated list of button names in the form."
                        },
                        paratype = ParameterType.Input,
                        nextparatype = ParameterType.CssName,
                        nextparaseparator = ":"
                    });
                    load.Add(new ParameterSetting(){
                        category = category,
                        setting = "--wbtntype",
                        synonym = "-wby",
                        description = "List of form button types",
                        helptext = new List<string>(){
                            $"Usage: csgen {category} -wby btntypes",
                            "",
                            "Specify a colon-separated list of form button types.",
                            "",
                            "Only the values 'submit' and 'reset' are valid."
                        },
                        paratype = ParameterType.Input,
                        nextparatype = ParameterType.CssName,
                        nextparaseparator = ":"
                    });
                    load.Add(new ParameterSetting(){
                        category = category,
                        setting = "--wbtntext",
                        synonym = "-wbt",
                        description = "List of text for form buttons",
                        helptext = new List<string>(){
                            $"Usage: csgen {category} -wbt btntext",
                            "",
                            "Specify a colon-separated list of text for form buttons."
                        },
                        paratype = ParameterType.Input,
                        nextparatype = ParameterType.Any,
                        nextparaseparator = ":"
                    });
                    load.Add(new ParameterSetting(){
                        category = category,
                        setting = "--wbtnclass",
                        synonym = "-wbc",
                        description = "List of CSS classes for form buttons",
                        helptext = new List<string>(){
                            $"Usage: csgen {category} -wbc btnclassnames",
                            "",
                            "Specify a colon-separated list of form button CSS classes."
                        },
                        paratype = ParameterType.Input,
                        nextparatype = ParameterType.CssName,
                        nextparaseparator = ":"
                    });
                    load.Add(new ParameterSetting(){
                        category = category,
                        setting = "--wbtndclass",
                        synonym = "-wbd",
                        description = "List of CSS classes wrapping form buttons",
                        helptext = new List<string>(){
                            $"Usage: csgen {category} -wbd dclassnames",
                            "",
                            "Specify a colon-separated list of CSS classes for <div> tags to wrap form buttons."
                        },
                        paratype = ParameterType.Input,
                        nextparatype = ParameterType.CssName,
                        nextparaseparator = ":"
                    });

                    load.Add(new ParameterSetting(){
                        category = category,
                        setting = "--wbtniclass",
                        synonym = "-wbi",
                        description = "List of CSS classes for button embedded <i> tags",
                        helptext = new List<string>(){
                            $"Usage: csgen {category} -wbi iclassnames",
                            "",
                            "Specify a colon-separated list of form button embedded <i> tag CSS classes."
                        },
                        paratype = ParameterType.Input,
                        nextparatype = ParameterType.CssName,
                        nextparaseparator = ":"
                    });
                    load.Add(new ParameterSetting(){
                        category = category,
                        setting = "--wbtnonclick",
                        synonym = "-wbo",
                        description = "List of JS code for form button click events",
                        helptext = new List<string>(){
                            $"Usage: csgen {category} -wbo onclickcode",
                            "",
                            "Specify a colon-separated list of JavaScript code to populate the onclick methods."
                        },
                        paratype = ParameterType.Input,
                        nextparatype = ParameterType.Any,
                        nextparaseparator = ":"
                    });

                     load.Add(new ParameterSetting(){
                        category = category,
                        setting = "--wsubmit",
                        synonym = "-wst",
                        description = "Type of Submit object on the form",
                        helptext = new List<string>(){
                            $"Usage: csgen {category} -wst submit",
                            "",
                            "Specify the type of Submit object on the form."
                        },
                        paratype = ParameterType.Input,
                        nextparatype = ParameterType.CssName,
                        nextparaseparator = ":"
                    });
                     load.Add(new ParameterSetting(){
                        category = category,
                        setting = "--wsubaction",
                        synonym = "-wsa",
                        description = "Specifies the Submit action",
                        helptext = new List<string>(){
                            $"Usage: csgen {category} -wsa subaction",
                            "",
                            "Specify the type of Submit action."
                        },
                        paratype = ParameterType.Input,
                        nextparatype = ParameterType.CssName,
                        nextparaseparator = ":"
                    });
                     load.Add(new ParameterSetting(){
                        category = category,
                        setting = "--wsubdclass",
                        synonym = "-wsd",
                        description = "Colon-delimited CSS classes for the Submit object",
                        helptext = new List<string>(){
                            $"Usage: csgen {category} -wsd subdclass",
                            "",
                            "Specify a colon-delimited set of CSS classes for the Submit object."
                        },
                        paratype = ParameterType.Input,
                        nextparatype = ParameterType.CssName,
                        nextparaseparator = ":"
                    });
                     load.Add(new ParameterSetting(){
                        category = category,
                        setting = "--wsubiclass",
                        synonym = "-wsi",
                        description = "CSS class for an embedded <i> tag for the Submit object",
                        helptext = new List<string>(){
                            $"Usage: csgen {category} -wsi subiclass",
                            "",
                            "Specify a CSS class for an embedded <i> tag for the Submit object."
                        },
                        paratype = ParameterType.Input,
                        nextparatype = ParameterType.CssName,
                        nextparaseparator = ":"
                    });
                     load.Add(new ParameterSetting(){
                        category = category,
                        setting = "--wreturn",
                        synonym = "-wrt",
                        description = "Type of Return object on the form",
                        helptext = new List<string>(){
                            $"Usage: csgen {category} -wrt return",
                            "",
                            "Specify a Type of Return object on the form."
                        },
                        paratype = ParameterType.Input,
                        nextparatype = ParameterType.CssName,
                        nextparaseparator = ":"
                    });
                     load.Add(new ParameterSetting(){
                        category = category,
                        setting = "--wretaction",
                        synonym = "-wra",
                        description = "Specifies the Return action",
                        helptext = new List<string>(){
                            $"Usage: csgen {category} -wra retaction",
                            "",
                            "Specify the Return action."
                        },
                        paratype = ParameterType.Input,
                        nextparatype = ParameterType.CssName,
                        nextparaseparator = ":"
                    });
                     load.Add(new ParameterSetting(){
                        category = category,
                        setting = "--wretdclass",
                        synonym = "-wrd",
                        description = "Colon-delimited CSS classes for the Return object",
                        helptext = new List<string>(){
                            $"Usage: csgen {category} -wrd retdclass",
                            "",
                            "Specify a colon-delimited set of CSS classes for the Return object."
                        },
                        paratype = ParameterType.Input,
                        nextparatype = ParameterType.CssName,
                        nextparaseparator = ":"
                    });
                     load.Add(new ParameterSetting(){
                        category = category,
                        setting = "--wreticlass",
                        synonym = "-wri",
                        description = "CSS class for an embedded <i> tag for the Return object",
                        helptext = new List<string>(){
                            $"Usage: csgen {category} -wri reticlass",
                            "",
                            "Specify a CSS class for an embedded <i> tag for the Return object."
                        },
                        paratype = ParameterType.Input,
                        nextparatype = ParameterType.CssName,
                        nextparaseparator = ":"
                    });
                    load.Add(new ParameterSetting(){
                        category = category,
                        setting = "--wfrmaction",
                        synonym = "-wfa",
                        description = "Specifies the Form action",
                        helptext = new List<string>(){
                            $"Usage: csgen {category} -wfa formaction",
                            "",
                            "Specify the Form action.",
                            "",
                            "This is the URI that the page will send data to."
                        },
                        paratype = ParameterType.Input,
                        nextparatype = ParameterType.Any
                    });
                    load.Add(new ParameterSetting(){
                        category = category,
                        setting = "--wfrmmethod",
                        synonym = "-wfm",
                        description = "Specifies the Form method",
                        helptext = new List<string>(){
                            $"Usage: csgen {category} -wfm formmethod",
                            "",
                            "Specify the Form method (GET or POST).",
                            "",
                            "Default is 'POST'."
                        },
                        paratype = ParameterType.Input,
                        nextparatype = ParameterType.Any
                    });
                     load.Add(new ParameterSetting(){
                        category = category,
                        setting = "--wfrmclass",
                        synonym = "-wfc",
                        description = "Colon-delimited CSS classes wrapping the Form object",
                        helptext = new List<string>(){
                            $"Usage: csgen {category} -wfc formclass",
                            "",
                            "Specify a colon-delimited set of CSS classes wrapping the Form object."
                        },
                        paratype = ParameterType.Input,
                        nextparatype = ParameterType.CssName,
                        nextparaseparator = ":"
                    });
                     load.Add(new ParameterSetting(){
                        category = category,
                        setting = "--wfrmsub",
                        synonym = "-wfs",
                        description = "Colon-delimited CSS classes wrapping all objects inside the Form object",
                        helptext = new List<string>(){
                            $"Usage: csgen {category} -wfs formsub",
                            "",
                            "Specify a colon-delimited set of CSS classes wrapping all objects inside the Form object."
                        },
                        paratype = ParameterType.Input,
                        nextparatype = ParameterType.CssName,
                        nextparaseparator = ":"
                    });

                    load.Add(new ParameterSetting(){
                        category = category,
                        setting = "--wfrmbtncss",
                        synonym = "-wfb",
                        description = "CSS class to wrap all Form buttons",
                        helptext = new List<string>(){
                            $"Usage: csgen {category} -wfb btncss",
                            "",
                            "Specify a CSS class to wrap all Form buttons."
                        },
                        paratype = ParameterType.Input,
                        nextparatype = ParameterType.CssName
                    });


                    load.Add(new ParameterSetting(){
                        category = category,
                        setting = "--wscrlist",
                        synonym = "-wjl",
                        description = "Colon-delimited list of .js files linked on this view",
                        helptext = new List<string>(){
                            $"Usage: csgen {category} -wjl jsfiles",
                            "",
                            "Colon-delimited list of .js files linked on this view"
                        },
                        paratype = ParameterType.Input,
                        nextparatype = ParameterType.Any,
                        nextparaseparator = ":"
                    });
                    load.Add(new ParameterSetting(){
                        category = category,
                        setting = "--wscrsection",
                        synonym = "-wjn",
                        description = "Name of the @section for scripts called in this view",
                        helptext = new List<string>(){
                            $"Usage: csgen {category} -wjn sectionname",
                            "",
                            "Name of the @section for scripts called in this view."
                        },
                        paratype = ParameterType.Input,
                        nextparatype = ParameterType.CsClassName
                    });



                    
                     load.Add(new ParameterSetting(){
                        category = category,
                        setting = "--wpageclass",
                        synonym = "-wpc",
                        description = "Colon-delimited CSS classes wrapping the Info and Form sections",
                        helptext = new List<string>(){
                            $"Usage: csgen {category} -wpc pageclasses",
                            "",
                            "Colon-delimited CSS classes wrapping the Info and Form sections."
                        },
                        paratype = ParameterType.Input,
                        nextparatype = ParameterType.CssName,
                        nextparaseparator = ":"
                    });
                     load.Add(new ParameterSetting(){
                        category = category,
                        setting = "--winfoclass",
                        synonym = "-wic",
                        description = "Colon-delimited CSS classes wrapping the Info section above form fields",
                        helptext = new List<string>(){
                            $"Usage: csgen {category} -wic infoclass",
                            "",
                            "Specify a colon-delimited CSS class wrapping the Info section above form fields."
                        },
                        paratype = ParameterType.Input,
                        nextparatype = ParameterType.CssName,
                        nextparaseparator = ":"
                    });
                     load.Add(new ParameterSetting(){
                        category = category,
                        setting = "--winfohclass",
                        synonym = "-wih",
                        description = "CSS class of the heading in the Info section",
                        helptext = new List<string>(){
                            $"Usage: csgen {category} -wih infohclass",
                            "",
                            "Specify a CSS class of the heading in the Info section."
                        },
                        paratype = ParameterType.Input,
                        nextparatype = ParameterType.CssName,
                        nextparaseparator = ":"
                    });

                    load.Add(new ParameterSetting(){
                        category = category,
                        setting = "--winfohead",
                        synonym = "-wid",
                        description = "Heading text for the information section",
                        helptext = new List<string>(){
                            $"Usage: csgen {category} -wid infoheading",
                            "",
                            "Specify the heading text for the information section."
                        },
                        paratype = ParameterType.Input,
                        nextparatype = ParameterType.Any,
                        nextparaseparator = ":"
                    });
                     load.Add(new ParameterSetting(){
                        category = category,
                        setting = "--winfotext",
                        synonym = "-wit",
                        description = "Text for the information section",
                        helptext = new List<string>(){
                            $"Usage: csgen {category} -wit infotext",
                            "",
                            "Specify text for the information section."
                        },
                        paratype = ParameterType.Input,
                        nextparatype = ParameterType.Any,
                        nextparaseparator = ":"
                    });
                     load.Add(new ParameterSetting(){
                        category = category,
                        setting = "--wlayfiles",
                        synonym = "-wlf",
                        description = "Colon-separated list of Layout cshtml files associated with --laynames",
                        helptext = new List<string>(){
                            $"Usage: csgen {category} -wlf layfiles",
                            "",
                            "Specify a colon-separated list of Layout cshtml files associated with --laynames."
                        },
                        paratype = ParameterType.Input,
                        nextparatype = ParameterType.Any,
                        nextparaseparator = ":"
                    });
                    load.Add(new ParameterSetting(){
                        category = category,
                        setting = "--wlaynames",
                        synonym = "-wln",
                        description = "Colon-separated list of @section names",
                        helptext = new List<string>(){
                            $"Usage: csgen {category} -wln laynames",
                            "",
                            "Specify a colon-separated list of @section names."
                        },
                        paratype = ParameterType.Input,
                        nextparatype = ParameterType.Any,
                        nextparaseparator = ":"
                    });
                    load.Add(new ParameterSetting(){
                        category = category,
                        setting = "--wlayout",
                        synonym = "-wlm",
                        description = "Name of the primary layout cshtml",
                        helptext = new List<string>(){
                            $"Usage: csgen {category} -wlm layout",
                            "",
                            "Specify the layout file used by the new View."
                        },
                        paratype = ParameterType.Input,
                        nextparatype = ParameterType.Any,
                        nextparaseparator = ":"
                    });
                }


                if(category == "controller")
                {

                    helptext.Add("  -cn|--cname        Name of the Controller class.");
                    helptext.Add("  -cm|--cnamespace   Namespace of the Controller.");
                    helptext.Add("  -cp|--cparent      Name of the parent class.");
                    load.Add(new ParameterSetting(){
                        category = category,
                        setting = "--cname",
                        synonym = "-cn",
                        description = "Name of the Controller",
                        helptext = new List<string>(){
                            $"Usage: csgen {category} -cn cname",
                            "",
                            "Name of the Controller."
                        },
                        paratype = ParameterType.Input,
                        nextparatype = ParameterType.CsClassName
                    });
                    load.Add(new ParameterSetting(){
                        category = category,
                        setting = "--cnamespace",
                        synonym = "-cm",
                        description = "Namespace of the Controller",
                        helptext = new List<string>(){
                            $"Usage: csgen {category} -cm cnamespace",
                            "",
                            "Namespace of the Controller."
                        },
                        paratype = ParameterType.Input,
                        nextparatype = ParameterType.CsNameSpace
                    });
                    load.Add(new ParameterSetting(){
                        category = category,
                        setting = "--cparent",
                        synonym = "-cp",
                        description = "Name of the parent class",
                        helptext = new List<string>(){
                            $"Usage: csgen {category} -cp cparent",
                            "",
                            "Name of the parent class."
                        },
                        paratype = ParameterType.Input,
                        nextparatype = ParameterType.CsClassName
                    });
                }

                if(category == "controller" || category == "facade")
                {
                    helptext.Add("  -cdc|--cdcontext   Class name of the ApplicationDbContext.");

                    load.Add(new ParameterSetting(){
                        category = category,
                        setting = "--cdcontext",
                        synonym = "-cdc",
                        description = "Class name of the ApplicationDbContext",
                        helptext = new List<string>(){
                            $"Usage: csgen {category} -cdc dbcontextname",
                            "",
                            "Class name of the ApplicationContext.",
                            "",
                            "This should be fully-qualified with its namespace."
                        },
                        paratype = ParameterType.Input,
                        nextparatype = ParameterType.CsNameSpace
                    });
                }

                if(category == "controller" )
                {

                    helptext.Add("  -cca|--careaname   Name of the Controller Area.");
                    helptext.Add("  -cdp|--cdpropname  Property name of the ApplicationDbContext.");
                    helptext.Add("  -cur|--curoute     Name of the Controller URI route.");


// SNJW TO DO add these
                    helptext.Add("  -chs|--chshared    Add shared helper methods to this Controller.");
                    helptext.Add("  -chi|--chidentity  Add Identity helper methods to this Controller.");
                    helptext.Add("  -cnm|--cnoanonmg   Controller does not manage anonymous access.");
                    helptext.Add("  -cnh|--cnohelper   Controller does use helper methods.");

                    helptext.Add("  -cnf|--cnofacade   Controller does not use a facade.");
                    helptext.Add("  -cni|--cnouser     Controller does not use Identity.");
                    helptext.Add("  -cnn|--cnoanon     Controller does not allow anonymous access.");
                    helptext.Add("  -cna|--cnoasync    Controller does not use asynchronous methods.");
                    helptext.Add("  -cnd|--cnodbsave   Controller does not use issue a dbsave command.");
                    helptext.Add("  -cnb|--cnobinding  Controller does not bind individual fields.");

                    helptext.Add("  -cap|--cacthttps   Colon-delimited GET/POST action properties.");
                    helptext.Add("  -can|--cactnames   Colon-delimited action names.");
                    helptext.Add("  -cas|--cactsyns    Colon-delimited action synonyms.");
                    helptext.Add("  -cat|--cacttypes   Colon-delimited action types (Create/Delete/Edit/Index/Details).");
                    helptext.Add("  -cah|--cactpaths   Colon-delimited action paths.");

                    helptext.Add("  -cvn|--cvnames     Colon-delimited ViewModel names.");
                    helptext.Add("  -cmn|--cmnames     Colon-delimited Model names.");
                    helptext.Add("  -cwn|--cwnames     Colon-delimited View names.");
                    helptext.Add("  -cfn|--cfnames     Colon-delimited Facade names.");

                    load.Add(new ParameterSetting(){
                        category = category,
                        setting = "--careaname",
                        synonym = "-cca",
                        description = "Name of the Controller Area",
                        helptext = new List<string>(){
                            $"Usage: csgen {category} -cca areaname",
                            "",
                            "Name of the Controller Area.  If not entered, no Area is used."
                        },
                        paratype = ParameterType.Input,
                        nextparatype = ParameterType.CsFieldName
                    });
                    load.Add(new ParameterSetting(){
                        category = category,
                        setting = "--cdpropname",
                        synonym = "-cdp",
                        description = "Property name of the ApplicationDbContext",
                        helptext = new List<string>(){
                            $"Usage: csgen {category} -cdp dbcontextpropname",
                            "",
                            "Property/variable name of the ApplicationDbContext in each Controller/Facade."
                        },
                        paratype = ParameterType.Input,
                        nextparatype = ParameterType.CsFieldName
                    });
                    load.Add(new ParameterSetting(){
                        category = category,
                        setting = "--curoute",
                        synonym = "-cur",
                        description = "Route name for the controller",
                        helptext = new List<string>(){
                            $"Usage: csgen {category} -cur routename",
                            "",
                            "Base URI route name for this Controller."
                        },
                        paratype = ParameterType.Input,
                        nextparatype = ParameterType.Any
                    });


                    load.Add(new ParameterSetting(){
                        category = category,
                        setting = "--chshared",
                        synonym = "-chs",
                        description = "Add shared helper methods to this Controller",
                        helptext = new List<string>(){
                            $"Usage: csgen {category} -chs",
                            "",
                            "By default the Controller will not have shared helper methods.",
                            "It is assumed that these are in a parent class.",
                            "Set this parameter to have these created in the current Controller."
                        },
                        paratype = ParameterType.Switch
                    });
                    load.Add(new ParameterSetting(){
                        category = category,
                        setting = "--chidentity",
                        synonym = "-chi",
                        description = "Add Identity helper methods to this Controller",
                        helptext = new List<string>(){
                            $"Usage: csgen {category} -chi",
                            "",
                            "By default the Controller will not have helper methods for Identity.",
                            "It is assumed that these are in a parent class.",
                            "Set this parameter to have these created in the current Controller."
                        },
                        paratype = ParameterType.Switch
                    });

                    load.Add(new ParameterSetting(){
                        category = category,
                        setting = "--cnoanonmg",
                        synonym = "-cnm",
                        description = "Do not manage anonymous access in this Controller",
                        helptext = new List<string>(){
                            $"Usage: csgen {category} -cnm",
                            "",
                            "If Identity is set and the --cnoanon option is not set, then anonymous users can access Controller actions.",
                            "In many situations, anonymous users are handled different to other users.",
                            "If this flag is set, there will not be boilerplate code to check for an anonymous user."
                        },
                        paratype = ParameterType.Switch
                    });
                    load.Add(new ParameterSetting(){
                        category = category,
                        setting = "--cnohelper",
                        synonym = "-cnh",
                        description = "Do not use helper methods in this Controller",
                        helptext = new List<string>(){
                            $"Usage: csgen {category} -cnh",
                            "",
                            "By default the Controller will use helper methods for repeated shared and/or Identity actions.",
                            "These include converting GUIDs and checking for Anonymous Users.",
                            "If this is set, these actions will be hard-coded in each line where required."
                        },
                        paratype = ParameterType.Switch
                    });


                    load.Add(new ParameterSetting(){
                        category = category,
                        setting = "--cnofacade",
                        synonym = "-cnf",
                        description = "Do not use a Facade in the Controller",
                        helptext = new List<string>(){
                            $"Usage: csgen {category} -cnf",
                            "",
                            "By default the Controller will use a Facade class.",
                            "Set this to do have all logic be inside the Controller actions "
                        },
                        paratype = ParameterType.Switch
                    });
                    load.Add(new ParameterSetting(){
                        category = category,
                        setting = "--cnouser",
                        synonym = "-cni",
                        description = "Do not use Identity in the Controller",
                        helptext = new List<string>(){
                            $"Usage: csgen {category} -cni",
                            "",
                            "By default the Controller will use the Identity framework.",
                            "Set this to not use Identity in the Controller."
                        },
                        paratype = ParameterType.Switch
                    });
                    load.Add(new ParameterSetting(){
                        category = category,
                        setting = "--cnoanon",
                        synonym = "-cnn",
                        description = "Do not allow anonymous access in each method of the Controller",
                        helptext = new List<string>(){
                            $"Usage: csgen {category} -cnn",
                            "",
                            "By default the Controller will leave each method anonymous for Identity purposes.",
                            "It will instead do validation in each method.",
                            "",
                            "If -cni|--cnouser is set then this setting does nothing."
                        },
                        paratype = ParameterType.Switch
                    });
                    load.Add(new ParameterSetting(){
                        category = category,
                        setting = "--cnoasync",
                        synonym = "-cna",
                        description = "Use synchronous methods",
                        helptext = new List<string>(){
                            $"Usage: csgen {category} -cna",
                            "",
                            "By default the Controller will use asynchronous methods.",
                            "Set this to have synchronous methods instead."
                        },
                        paratype = ParameterType.Switch
                    });
                    load.Add(new ParameterSetting(){
                        category = category,
                        setting = "--cnodbsave",
                        synonym = "-cnd",
                        description = "Do not put dbsave commands in the Controller",
                        helptext = new List<string>(){
                            $"Usage: csgen {category} -cnd",
                            "",
                            "By default the Controller will do the dbsave and not the Facade.",
                            "Set this to not issue dbsave commands in the Controller."
                        },
                        paratype = ParameterType.Switch
                    });
                    load.Add(new ParameterSetting(){
                        category = category,
                        setting = "--cnobinding",
                        synonym = "-cnb",
                        description = "Whether Controller actions bind ViewModel fields",
                        helptext = new List<string>(){
                            $"Usage: csgen {category} -cnb",
                            "",
                            "By default the Controller will bind ViewModel fields in POST actions.",
                            "Set this to not bind fields."
                        },
                        paratype = ParameterType.Switch
                    });
                    load.Add(new ParameterSetting(){
                        category = category,
                        setting = "--cacthttps",
                        synonym = "-cap",
                        description = "Colon-delimited GET/POST action properties",
                        helptext = new List<string>(){
                            $"Usage: csgen {category} -cap getpostlist",
                            "",
                            "Colon-delimited GET/POST action properties.",
                            "",
                            "Where the same named action has both GET and POST controller methods, enter 'GET/POST' or 'GP'.",
                            "If a delimited part contains 'g' or 'G' a GET action will be created for the action corresponding to that part.",
                            "If a delimited part contains 'p' or 'P' a POST action will be created for the action corresponding to that part.",
                            "If empty, the --cactype will be checked.  Index/Details types will have only GET and Create/Delete/Edit will have both."
                        },
                        paratype = ParameterType.Input,
                        nextparatype = ParameterType.Any,
                        nextparaseparator = ":"
                    });
                     load.Add(new ParameterSetting(){
                        category = category,
                        setting = "--cactnames",
                        synonym = "-can",
                        description = "Colon-delimited action names",
                        helptext = new List<string>(){
                            $"Usage: csgen {category} -can cactnames",
                            "",
                            "Colon-delimited action names."
                        },
                        paratype = ParameterType.Input,
                        nextparatype = ParameterType.Any,
                        nextparaseparator = ":"
                    });
                     load.Add(new ParameterSetting(){
                        category = category,
                        setting = "--cactsyns",
                        synonym = "-cas",
                        description = "Colon-delimited action synonyms",
                        helptext = new List<string>(){
                            $"Usage: csgen {category} -cas cactsyns",
                            "",
                            "Colon-delimited action synonyms.",
                            "",
                            "In each part, the alternative route names for each action can be entered.",
                            "These can be separated by spaces or the '+' symbol."
                        },
                        paratype = ParameterType.Input,
                        nextparatype = ParameterType.Any,
                        nextparaseparator = ":"
                    });
                     load.Add(new ParameterSetting(){
                        category = category,
                        setting = "--cacttypes",
                        synonym = "-cat",
                        description = "Colon-delimited action types (Create/Delete/Edit/Index/Details)",
                        helptext = new List<string>(){
                            $"Usage: csgen {category} -cat cacttypes",
                            "",
                            "Colon-delimited action types (Create/Delete/Edit/Index/Details)."
                        },
                        paratype = ParameterType.Input,
                        nextparatype = ParameterType.Any,
                        nextparaseparator = ":"
                    });
                    load.Add(new ParameterSetting(){
                        category = category,
                        setting = "--cactpaths",
                        synonym = "-cah",
                        description = "Colon-delimited action paths (Create/Delete/Edit/Index/Details)",
                        helptext = new List<string>(){
                            $"Usage: csgen {category} -cah cactpaths",
                            "",
                            "Colon-delimited paths to each .cshtml file {path/filename.cshtml}.",
                            "The full file name must be included."
                        },
                        paratype = ParameterType.Input,
                        nextparatype = ParameterType.Any,
                        nextparaseparator = ":"
                    });


                    load.Add(new ParameterSetting(){
                        category = category,
                        setting = "--cvnames",
                        synonym = "-cvn",
                        description = "Colon-delimited action ViewModel names",
                        helptext = new List<string>(){
                            $"Usage: csgen {category} -cvn vmnames",
                            "",
                            "Colon-delimited action list of ViewModel names.",
                            "If only one is specified and --fillblanks is used, this ViewModel is used for all Controller actions."
                        },
                        paratype = ParameterType.Input,
                        nextparatype = ParameterType.CsClassName,
                        nextparaseparator = ":"
                    });
                    load.Add(new ParameterSetting(){
                        category = category,
                        setting = "--cmnames",
                        synonym = "-cmn",
                        description = "Colon-delimited action Model names",
                        helptext = new List<string>(){
                            $"Usage: csgen {category} -cmn modelnames",
                            "",
                            "Colon-delimited action Model names.",
                            "If only one is specified and --fillblanks is used, this Model is used for all Controller actions."
                        },
                        paratype = ParameterType.Input,
                        nextparatype = ParameterType.CsClassName,
                        nextparaseparator = ":"
                    });
                    load.Add(new ParameterSetting(){
                        category = category,
                        setting = "--cwnames",
                        synonym = "-cwn",
                        description = "Colon-delimited action View names",
                        helptext = new List<string>(){
                            $"Usage: csgen {category} -cwn viewnames",
                            "",
                            "Colon-delimited action View names.",
                            "If not specified, the default 'View(vmname)' is called."
                        },
                        paratype = ParameterType.Input,
                        nextparatype = ParameterType.Any,
                        nextparaseparator = ":"
                    });

                    load.Add(new ParameterSetting(){
                        category = category,
                        setting = "--cfnames",
                        synonym = "-cfn",
                        description = "Colon-delimited action Facade names",
                        helptext = new List<string>(){
                            $"Usage: csgen {category} -cfn facadenames",
                            "",
                            "Colon-delimited action list of Facade names.",
                            "If only one is specified and --fillblanks is used, this Facade is used for all Controller actions."
                        },
                        paratype = ParameterType.Input,
                        nextparatype = ParameterType.CsClassName,
                        nextparaseparator = ":"
                    });
                }

                if(category == "controller" || category == "facade")
                {
                    helptext.Add("  -cvk|--cvpkeys     Colon-delimited ViewModel primary key fields.");
                    helptext.Add("  -cvf|--cvfkeys     Colon-delimited ViewModel foreign key fields.");
                    helptext.Add("  -cmk|--cmpkeys     Colon-delimited Model primary key fields.");
                    //helptext.Add("  -cmf|--cmfkeys     Colon-delimited Model foreign key fields.");
                    helptext.Add("  -cmp|--cmparents   Colon-delimited Model parent(+grandparent) names.");
                    helptext.Add("  -cmy|--cmparkeys   Colon-delimited Model parent(+grandparent) key fields.");
                    helptext.Add("  -cmu|--cmukeys     Colon-delimited action Model User key fields.");
                    helptext.Add("  -cvu|--cvukeys     Colon-delimited action ViewModel user key fields.");
                    helptext.Add("  -cvm|--cvmsgs      Colon-delimited action ViewModel message fields.");

                    load.Add(new ParameterSetting(){
                        category = category,
                        setting = "--cvpkeys",
                        synonym = "-cvk",
                        description = "Colon-delimited action ViewModel primary key fields",
                        helptext = new List<string>(){
                            $"Usage: csgen {category} -cvk vmpkeynames",
                            "",
                            "Colon-delimited action ViewModel primary key fields."
                        },
                        paratype = ParameterType.Input,
                        nextparatype = ParameterType.Any,
                        nextparaseparator = ":"
                    });
                    load.Add(new ParameterSetting(){
                        category = category,
                        setting = "--cvfkeys",
                        synonym = "-cvf",
                        description = "Colon-delimited action ViewModel foreign key fields",
                        helptext = new List<string>(){
                            $"Usage: csgen {category} -cvf vmfkeynames",
                            "",
                            "Colon-delimited action ViewModel foreign key fields."
                        },
                        paratype = ParameterType.Input,
                        nextparatype = ParameterType.Any,
                        nextparaseparator = ":"
                    });
                    load.Add(new ParameterSetting(){
                        category = category,
                        setting = "--cmpkeys",
                        synonym = "-cmk",
                        description = "Colon-delimited action Model primary key fields",
                        helptext = new List<string>(){
                            $"Usage: csgen {category} -cmk modelpkeynames",
                            "",
                            "Colon-delimited action Model primary key fields."
                        },
                        paratype = ParameterType.Input,
                        nextparatype = ParameterType.Any,
                        nextparaseparator = ":"
                    });
                    // load.Add(new ParameterSetting(){
                    //     category = category,
                    //     setting = "--cmfkeys",
                    //     synonym = "-cmf",
                    //     description = "Colon-delimited action Model foreign key fields",
                    //     helptext = new List<string>(){
                    //         $"Usage: csgen {category} -cmf modelfkeynames",
                    //         "",
                    //         "Colon-delimited action Model foreign key fields."
                    //     },
                    //     paratype = ParameterType.Input,
                    //     nextparatype = ParameterType.Any,
                    //     nextparaseparator = ":"
                    // });
                    load.Add(new ParameterSetting(){
                        category = category,
                        setting = "--cmparents",
                        synonym = "-cmp",
                        description = "Colon-delimited action Model parent(+grandparent) table fields",
                        helptext = new List<string>(){
                            $"Usage: csgen {category} -cmp modelparentnames",
                            "",
                            "Colon-delimited action Model parent (and higher) table names.",
                            "",
                            "The format of this is:",
                            "cmparent1[.cmgrandparent1][.cmgreatgrandparent1][,cmparent2][.cmgrandparent2][.cmgreatgrandparent2][,...]",
                            "",
                            $"The colon-delimited list is used in the {category.Substring(0,1).ToUpper()}{category.Substring(1,category.Length-1)}.",
                            "",
                            "Subsequent period-delimited Models (grandparents) are used for cross-checking ownership with Identity.",
                            "If this is the case, the final item should be '.User' or '.AspNetUsers' or whatever the Identity table name is.",
                            "Therefore, if this Entity is a top-level table, this field should simply be 'User'.",
                            "",
                            "It is also used to cross-check ownership of parent tables in Controller actions.",
                            "",
                            "If this is not set, no checking of parents will be made.",
                            "",
                            "If the top-level table is not 'User', then no cross-checking of ownership in Identity will be made.",
                            "",
                            "For example, if the Identity table is 'User' and you have Projects and Tasks, when creating the Task Controller:",
                            "-cmp Project.User",
                            ""
                        },
                        paratype = ParameterType.Input,
                        nextparatype = ParameterType.Any,
                        nextparaseparator = ":"
                    });
                    load.Add(new ParameterSetting(){
                        category = category,
                        setting = "--cmparkeys",
                        synonym = "-cmy",
                        description = "Colon-delimited Model parent(+grandparent) key fields",
                        helptext = new List<string>(){
                            $"Usage: csgen {category} -cmy modelparentkeyfieldnames",
                            "",
                            "Colon-delimited action Model parent (and higher) key field names.",
                            "",
                            "The format of this is:",
                            "cmparentkey1[.cmgrandparentkey1][.cmgreatgrandparentkey1][,cmparentkey2][.cmgrandparentkey2][.cmgreatgrandparentkey2][,...]",
                            "",
                            $"The colon-delimited list is used in the {category.Substring(0,1).ToUpper()}{category.Substring(1,category.Length-1)}.",
                            "",
                            "Subsequent period-delimited Models (grandparents) are used for cross-checking ownership with Identity.",
                            "If this is the case, the final item should be 'Id' or whatever the pkey of the Identity table is."
                        },
                        paratype = ParameterType.Input,
                        nextparatype = ParameterType.Any,
                        nextparaseparator = ":"
                    });

                    load.Add(new ParameterSetting(){
                        category = category,
                        setting = "--cvmkeys",
                        synonym = "-cvm",
                        description = "Colon-delimited action Model UserId fields",
                        helptext = new List<string>(){
                            $"Usage: csgen {category} -cvu modeluserkeynames",
                            "",
                            "Colon-delimited action Model user key fields.",
                            "",
                            "If the --fillblanks option is set, the first item is used.",
                            "",
                            "Note that this is used to check the userid in the Model for ownership.",
                            "",
                            "By default, this is 'Id' in the 'AspNetUsers' table."
                        },
                        paratype = ParameterType.Input,
                        nextparatype = ParameterType.Any,
                        nextparaseparator = ":"
                    });

                    load.Add(new ParameterSetting(){
                        category = category,
                        setting = "--cvukeys",
                        synonym = "-cvu",
                        description = "Colon-delimited action ViewModel user key fields",
                        helptext = new List<string>(){
                            $"Usage: csgen {category} -cvu vmukeynames",
                            "",
                            "Colon-delimited action ViewModel user key fields."
                        },
                        paratype = ParameterType.Input,
                        nextparatype = ParameterType.Any,
                        nextparaseparator = ":"
                    });
                    load.Add(new ParameterSetting(){
                        category = category,
                        setting = "--cvmsgs",
                        synonym = "-cvm",
                        description = "Colon-delimited action ViewModel messaging fields",
                        helptext = new List<string>(){
                            $"Usage: csgen {category} -cvm vmmessagenames",
                            "",
                            "Colon-delimited action ViewModel messaging fields."
                        },
                        paratype = ParameterType.Input,
                        nextparatype = ParameterType.Any,
                        nextparaseparator = ":"
                    });
                }


/*

        public string http {get;set;} = "";  // GET or POST
        public string name {get;set;} = "";  // this is the case-sensitive name.  Defaults to Create/Index/Edit/Delete/Details
        public string synonyms {get;set;} = "";  // additional names for this method.  Separated by a plus '+' sign
        public string actiontype {get;set;} = "";  // Create/Index/Edit/Delete/Details
        public string vm {get;set;} = "";  // ViewModel class
        public string model {get;set;} = "";  // Model class.  Not needed if 'usefacade' is true
        public string view {get;set;} = "";  // View name
        public string userid {get;set;} = "";  // name of userid field.  Not used if 'useidentity' is false
        public string message {get;set;} = "";  // name of messaging field (usually for errors)
        public string fkey {get;set;} = "";  // name of the foreign key field
        public bool usefacade {get;set;} = false;  // if true, just call the facade.  If false, bind to model fields here
        public bool useidentity {get;set;} = false;  // Create/Index/Edit/Delete/Details
        public bool usesync {get;set;} = false;  // async vs sync
        public bool usedbsave {get;set;} = false;  // save changes here (and not in facade)
        public bool usebinding {get;set;} = false;  // include a [Bind("x,y,z")] property on the vm

*/


                helptext.Add("  -h|--help          Display help.");
                help.helptext.AddRange(helptext);
                output.Add(help);
                output.AddRange(load);
            }

            return output;
        }

    //     private void LoadParameterInfo()
    //     {


    //     //    ps.Add(new ParameterSetting(){
    //     //         category = "vm",
    //     //         setting = "--help",
    //     //         synonym = "-h",
    //     //         description = "ViewModel Creation Help",
    //     //         restriction = "",
    //     //         input = "",
    //     //         nextinput = "",
    //     //         required  = false,
    //     //         helptext = new List<string>(){
    //     //             "Usage: csgen vm [options]",
    //     //             "",
    //     //             "Creates a ViewModel file.",
    //     //             "",
    //     //             "Options:",
    //     //             "  -n|--name         ViewModel name.",
    //     //             "  -p|--namespace    ViewModel namespace.",
    //     //             "  -o|--output       Full path to output .cs file.",
    //     //             "  -s|--source       Loads field properties from a CSV file.",
    //     //             "                    The header row must contain 'vmfieldname','vmfieldtype','vmfieldsize','vmfielddesc''vmfieldreq','vmfieldcap' to be used.",
    //     //             "  -vf|--fieldnames  Comma-separated list of ViewModel field names in order.",
    //     //             "                    Syntax is vmfieldname1[,vmfieldname2][,...].",
    //     //             "  -vt|--fieldtypes  Comma-separated list of ViewModel field types in order.",
    //     //             "                    Syntax is vmfieldtype1[,vmfieldtype2][,...].",
    //     //             "  -vz|--fieldsizes  Comma-separated list of ViewModel field sizes in order.",
    //     //             "                    Syntax is vmfieldsize1[,vmfieldsize2][,...].",
    //     //             "  -vc|--fielddescs  Comma-separated list of ViewModel field descriptions in order.",
    //     //             "                    Syntax is vmfielddesc1[,vmfielddesc2][,...].",
    //     //             "  -vq|--fieldreqs   Comma-separated list of ViewModel field required text in order.",
    //     //             "                    Syntax is vmfieldreq1[,vmfieldreqc2][,...].",
    //     //             "  -va|--fieldcaps   Comma-separated list of ViewModel field captions in order.",
    //     //             "                    Syntax is vmfieldcap1[,vmfieldcap2][,...].",
    //     //             "  -k|--key          Specifies the primary key field.",
    //     //             "  -h|--help         Display help."
    //     //         },
    //     //         paratype = ParameterType.Switch,
    //     //         paraintmin = 0,
    //     //         paraintmax = 65535,
    //     //         nextparatype = ParameterType.Any,
    //     //         nextparaintmin = 0,
    //     //         nextparaintmax = 65535
    //     //     });

    //     //    ps.Add(new ParameterSetting(){
    //     //         category = "vm",
    //     //         setting = "--name",
    //     //         synonym = "-n",
    //     //         description = "ViewModel Model Name",
    //     //         helptext = new List<string>(){
    //     //             "Usage: csgen vm -n viewmodelname",
    //     //             "",
    //     //             "Specify the name of the ViewModel.  This must be both valid in both HTML5 and C#.",
    //     //             "If no name is specified, the filename (without '.cs') is used.  If neither are supplied, the ViewModel is named 'newviewmodel'.",
    //     //         },
    //     //         paratype = ParameterType.Input,
    //     //         nextparatype = ParameterType.CsClassName
    //     //     });

    //     //    ps.Add(new ParameterSetting(){
    //     //         category = "vm",
    //     //         setting = "--namespace",
    //     //         synonym = "-p",
    //     //         description = "ViewModel Model Namespace",
    //     //         helptext = new List<string>(){
    //     //             "Usage: csgen vm -p viewmodelnamespace",
    //     //             "",
    //     //             "Specify the name of the ViewModel.  This must be valid in C#.",
    //     //             "This parameter is option.  If no name is specified, the ViewModel class is created with no namespace.",
    //     //         },
    //     //         paratype = ParameterType.Input,
    //     //         nextparatype = ParameterType.CsClassName
    //     //     });

    //     //    ps.Add(new ParameterSetting(){
    //     //         category = "vm",
    //     //         setting = "--output",
    //     //         synonym = "-o",
    //     //         description = "ViewModel Output File",
    //     //         helptext = new List<string>(){
    //     //             "Usage: csgen vm -o outputfilename",
    //     //             "",
    //     //             "Specify the name of the file to be created.",
    //     //             "",
    //     //             "If no full path is specified then the current directory is used.",
    //     //             "This must be a valid filename and will be overwritten without notification.",
    //     //             "If not specified, the model name is used with '.cs' appended.  If neither are specified, the file 'newviewmodel.cs' is created in the current directory.",
    //     //         },
    //     //         paratype = ParameterType.Input,
    //     //         nextparatype = ParameterType.File
    //     //     });

    //     //    ps.Add(new ParameterSetting(){
    //     //         category = "vm",
    //     //         setting = "--source",
    //     //         synonym = "-s",
    //     //         description = "ViewModel Field Information Source File",
    //     //         helptext = new List<string>(){
    //     //             "Usage: csgen vm -s sourcefilename",
    //     //             "",
    //     //             "Specify the name of a CSV file containing fields.",
    //     //             "",
    //     //             "If no full path is specified then the current directory is checked.",
    //     //             "If the header row contains 'vmfieldname','vmfieldtype','vmfieldsize','vmfielddesc','vmfieldcap','vmfieldreq' then these will be used.",
    //     //             "",
    //     //             "The CSV can optionally have a 'vmname' field to filter records.",
    //     //             "Only records that have a matching 'vmname' the same as the specified model will be used.",
    //     //             "The order that fields are loaded is the natural order of the rows in the source file.",
    //     //             "If the -f|--fieldnames option is also used, those fields will be added after any obtained through the -s|--source     load."
    //     //         },
    //     //         paratype = ParameterType.Input,
    //     //         nextparatype = ParameterType.File
    //     //     });

    //     //    ps.Add(new ParameterSetting(){
    //     //         category = "vm",
    //     //         setting = "--fieldnames",
    //     //         synonym = "-vf",
    //     //         description = "ViewModel Field Names",
    //     //         helptext = new List<string>(){
    //     //             "Usage: csgen vm -vf fieldnames",
    //     //             "",
    //     //             "Specify a comma-separated list of fieldnames.",
    //     //             "",
    //     //             "The format of this is:",
    //     //             "vmfieldname1[,vmfieldname2][,...]",
    //     //             "",
    //     //             "If the -s|--source     option is also used, fields will be loaded from that file first and ones specified with -f are appended.",
    //     //             "Each name must be valid as a CSS/HTML name."
    //     //         },
    //     //         paratype = ParameterType.Input,
    //     //         nextparatype = ParameterType.HtmlFieldName,
    //     //         nextparaseparator = ","
    //     //     });

    //     //    ps.Add(new ParameterSetting(){
    //     //         category = "vm",
    //     //         setting = "--fieldtypes",
    //     //         synonym = "-vt",
    //     //         description = "ViewModel Field Types",
    //     //         helptext = new List<string>(){
    //     //             "Usage: csgen vm -vt fieldtypes",
    //     //             "",
    //     //             "Specify a comma-separated list of C# types associated with fields.",
    //     //             "",
    //     //             "The format of this is:",
    //     //             "vmfieldtype1[,vmfieldtype2][,...]",
    //     //             "",
    //     //             "If not specified, the default is string."
    //     //         },
    //     //         paratype = ParameterType.Input,
    //     //         nextparatype = ParameterType.CsClassName,
    //     //         nextparaseparator = ","
    //     //     });

    //     //    ps.Add(new ParameterSetting(){
    //     //         category = "vm",
    //     //         setting = "--fieldsizes",
    //     //         synonym = "-vz",
    //     //         description = "ViewModel Field Sizes",
    //     //         helptext = new List<string>(){
    //     //             "Usage: csgen vm -vz fieldsizes",
    //     //             "",
    //     //             "Specify a comma-separated list of field sizes.",
    //     //             "",
    //     //             "The format of this is:",
    //     //             "vmfieldsize1[,vmfieldsize2][,...]",
    //     //             "",
    //     //             "Where the field size is set to zero then this is ignored."
    //     //         },
    //     //         paratype = ParameterType.Input,
    //     //         nextparatype = ParameterType.Integer,
    //     //         nextparaseparator = ",",
    //     //         nextparaintmin = 0
    //     //     });


    //     //    ps.Add(new ParameterSetting(){
    //     //         category = "vm",
    //     //         setting = "--fielddescs",
    //     //         synonym = "-vd",
    //     //         description = "ViewModel Field Descriptions",
    //     //         helptext = new List<string>(){
    //     //             "Usage: csgen vm -vt fielddescs",
    //     //             "",
    //     //             "Specify a comma-separated list of field descriptions.",
    //     //             "",
    //     //             "The format of this is:",
    //     //             "vmfielddesc1[,vmfielddesc2][,...]"
    //     //         },
    //     //         paratype = ParameterType.Input,
    //     //         nextparatype = ParameterType.CsClassName,
    //     //         nextparaseparator = ","
    //     //     });

    //     //    ps.Add(new ParameterSetting(){
    //     //         category = "vm",
    //     //         setting = "--fieldcaps",
    //     //         synonym = "-va",
    //     //         description = "ViewModel Field Captions",
    //     //         helptext = new List<string>(){
    //     //             "Usage: csgen vm -va fieldcaps",
    //     //             "",
    //     //             "Specify a comma-separated list of the caption text that will appear in the View.",
    //     //             "",
    //     //             "The format of this is:",
    //     //             "vmfieldcap1[,vmfieldcap2][,...]"
    //     //         },
    //     //         paratype = ParameterType.Input,
    //     //         nextparatype = ParameterType.CsClassName,
    //     //         nextparaseparator = ","
    //     //     });

    //     //    ps.Add(new ParameterSetting(){
    //     //         category = "vm",
    //     //         setting = "--fieldreqs",
    //     //         synonym = "-vq",
    //     //         description = "ViewModel Field Required Text",
    //     //         helptext = new List<string>(){
    //     //             "Usage: csgen vm -vq fieldreqs",
    //     //             "",
    //     //             "Specify a comma-separated list of text that appears when required.",
    //     //             "Note that if blank or missing, the field is not required.",
    //     //             "",
    //     //             "The format of this is:",
    //     //             "vmfieldreq1[,vmfieldreq2][,...]"
    //     //         },
    //     //         paratype = ParameterType.Input,
    //     //         nextparatype = ParameterType.CsClassName,
    //     //         nextparaseparator = ","
    //     //     });

    //     //    ps.Add(new ParameterSetting(){
    //     //         category = "vm",
    //     //         setting = "--key",
    //     //         synonym = "-k",
    //     //         description = "ViewModel Primary Key",
    //     //         helptext = new List<string>(){
    //     //             "Usage: csgen vm -k pkeyfieldname",
    //     //             "",
    //     //             "Specify the name of the primary key field.",
    //     //             "",
    //     //             "If not specified, no pkey will be set."
    //     //         },
    //     //         paratype = ParameterType.Input,
    //     //         nextparatype = ParameterType.CsClassName
    //     //     });




    //        ps.Add(new ParameterSetting(){
    //             category = "view",
    //             setting = "--help",
    //             synonym = "-h",
    //             description = "View Creation Help",
    //             restriction = "",
    //             input = "",
    //             nextinput = "",
    //             required  = false,
    //             helptext = new List<string>(){
    //                 "Usage: csgen view [options]",
    //                 "",
    //                 "Creates a View file.",
    //                 "",
    //                 "Options:",
    //                 "  -v|--viewname     View name.",
    //                 "  -n|--name         ViewModel name.",
    //                 "  -p|--namespace    ViewModel namespace.",
    //                 "  -l|--layout       Layout cshtml file.",
    //                 "  -o|--output       Full path to output .cshtml file.",
    //                 "  -s|--source       Loads field properties from a CSV file.",
    //                 "                    The header row must contain 'vmfieldname','vmfieldtype','vmfieldsize','vmfielddesc''vmfieldreq','vmfieldcap' to be used.",
    //                 "                    The header row can also contain 'viewfclass','viewftype','viewfdclass','viewficlass','viewfrows' to be used.",
    //                 "  -vf|--fieldnames  Comma-separated list of ViewModel field names in order.",
    //                 "                    Syntax is vmfieldname1[,vmfieldname2][,...].",
    //                 "  -vt|--fieldtypes  Comma-separated list of ViewModel field types in order.",
    //                 "                    Syntax is vmfieldtype1[,vmfieldtype2][,...].",
    //                 "  -vz|--fieldsizes  Comma-separated list of ViewModel field sizes in order.",
    //                 "                    Syntax is vmfieldsize1[,vmfieldsize2][,...].",
    //                 "  -vc|--fielddescs  Comma-separated list of ViewModel field descriptions in order.",
    //                 "                    Syntax is vmfielddesc1[,vmfielddesc2][,...].",
    //                 "  -vq|--fieldreqs   Comma-separated list of ViewModel field required text in order.",
    //                 "                    Syntax is vmfieldreq1[,vmfieldreqc2][,...].",
    //                 "  -va|--fieldcaps   Comma-separated list of ViewModel field captions in order.",
    //                 "                    Syntax is vmfieldcap1[,vmfieldcap2][,...].",
    //                 "  -we|--viewfclass  Comma-separated list of View form field CSS classes in order.",
    //                 "                    Syntax is viewfclass1[,viewfclass2][,...].",
    //                 "  -wy|--viewftype   Comma-separated list of View form field HTML types in order.",
    //                 "                    Syntax is viewftype1[,viewftype2][,...].",
    //                 "  -wd|--viewfdclass Comma-separated list of colon-delimited CSS classes in order to wrap a form field in <div> tags.",
    //                 "                    Syntax is viewfdclass1a:viewfdclass1b[:viewfdclass1c][,viewfdclass2a][,...].",
    //                 "  -wi|--viewficlass Comma-separated list of a CSS class for an <i> tag that follows a form field.",
    //                 "                    Syntax is viewficlass1[,viewficlass2][,...].",
    //                 "  -wr|--viewfrows   Comma-separated list of <textarea> row size.",
    //                 "                    Syntax is viewfrows1[,viewfrows2][,...].",
    //                 "  -fk|--key         Specifies the primary key field.",
    //                 "  -fg|--fkey        Specifies the foreign key field.",
    //                 "  -fu|--user        Specifies the user key field.",
    //                 "  -fu|--message     Specifies the messaging field.",
    //                 "  -bt|--submit      Type of Submit object on the form.",
    //                 "  -ba|--subaction   Specifies the Submit action.",
    //                 "  -bd|--subdclass   Colon-delimited CSS classes for the Submit object.",
    //                 "  -bi|--subiclass   CSS class for an embedded <i> tag for the Submit object.",
    //                 "  -zt|--return      Type of Return object on the form.",
    //                 "  -za|--retaction   Specifies the Return action.",
    //                 "  -zd|--retdclass   Colon-delimited CSS classes for the Return object.",
    //                 "  -zi|--reticlass   CSS class for an embedded <i> tag for the Return object.",
    //                 "  -ma|--formaction  Specifies the Form action.",
    //                 "  -mw|--formclass   Colon-delimited CSS classes wrapping the Form object.",
    //                 "  -ms|--formsub     Colon-delimited CSS classes wrapping all objects inside the Form object.",
    //                 "  -pc|--pageclass   Specifies the CSS class wrapping the Info and Form sections.",
    //                 "  -ic|--infoclass   Colon-delimited CSS classes wrapping the Info section above form fields.",
    //                 "  -ih|--infohclass  CSS class of the heading in the Info section.",
    //                 "  -it|--infotext    Text for the information section.",
    //                 "  -lf|--layfiles    Colon-separated list of Layout cshtml files associated with --laynames.",
    //                 "  -ln|--laynames    Colon-separated list of @section names.",
    //                 "  -h|--help         Display help."
    //             },
    //             paratype = ParameterType.Switch,
    //             paraintmin = 0,
    //             paraintmax = 65535,
    //             nextparatype = ParameterType.Any,
    //             nextparaintmin = 0,
    //             nextparaintmax = 65535
    //         });

    //        ps.Add(new ParameterSetting(){
    //             category = "view",
    //             setting = "--viewname",
    //             synonym = "-v",
    //             description = "View Name",
    //             helptext = new List<string>(){
    //                 "Usage: csgen view -n viewname",
    //                 "",
    //                 "Specify the name of the View.  This is the text appearing in the <h3> tags inside the view. xModel that will be the base of this View.  This can be any text.",
    //                 "If not supplied, there will be no heading in the View.",
    //             },
    //             paratype = ParameterType.Input,
    //             nextparatype = ParameterType.Any
    //         });

    //        ps.Add(new ParameterSetting(){
    //             category = "view",
    //             setting = "--name",
    //             synonym = "-n",
    //             description = "ViewModel Name",
    //             helptext = new List<string>(){
    //                 "Usage: csgen view -n viewmodelname",
    //                 "",
    //                 "Specify the name of the ViewModel that will be the base of this View.  This must be both valid in both HTML5 and C#.",
    //                 "If no name is specified, the filename (without '.cshtml') is used.  If neither are supplied, the ViewModel is named 'newviewmodel'.",
    //             },
    //             paratype = ParameterType.Input,
    //             nextparatype = ParameterType.CsClassName
    //         });

    //        ps.Add(new ParameterSetting(){
    //             category = "view",
    //             setting = "--namespace",
    //             synonym = "-p",
    //             description = "ViewModel Namespace",
    //             helptext = new List<string>(){
    //                 "Usage: csgen view -p viewmodelnamespace",
    //                 "",
    //                 "Specify the name of the namespace of the associatedViewModel.  This must be a valid in C# name.",
    //                 "This parameter is option.  If no name is specified, the ViewModel class used in the View has no namespace.",
    //             },
    //             paratype = ParameterType.Input,
    //             nextparatype = ParameterType.CsClassName
    //         });

    //        ps.Add(new ParameterSetting(){
    //             category = "view",
    //             setting = "--output",
    //             synonym = "-o",
    //             description = "View Output File",
    //             helptext = new List<string>(){
    //                 "Usage: csgen view -o outputfilename",
    //                 "",
    //                 "Specify the name of the file to be created.",
    //                 "",
    //                 "If no full path is specified then the current directory is used.",
    //                 "This must be a valid filename and will be overwritten without notification.",
    //                 "If not specified, the view name is used with spaces removed and '.cshtml' appended.  If neither are specified, the file 'newview.cshtml' is created in the current directory.",
    //             },
    //             paratype = ParameterType.Input,
    //             nextparatype = ParameterType.File
    //         });

    //        ps.Add(new ParameterSetting(){
    //             category = "view",
    //             setting = "--source",
    //             synonym = "-s",
    //             description = "View Field Information Source File",
    //             helptext = new List<string>(){
    //                 "Usage: csgen view -s sourcefilename",
    //                 "",
    //                 "Specify the name of a CSV file containing fields.",
    //                 "",
    //                 "This is a superset of the fields in csgen vm -s.",
    //                 "",
    //                 "If no full path is specified then the current directory is checked.",
    //                 "The ViewModel fields are: 'vmfieldname','vmfieldtype','vmfieldsize','vmfielddesc''vmfieldreq','vmfieldcap' to be used.",
    //                 "Additional View fields are: 'viewfclass','viewftype','viewfdclass','viewficlass','viewfrows'.",
    //                 "",
    //                 "The CSV can optionally have a 'vmname' field to filter records.",
    //                 "Only records that have a matching 'vmname' the same as the specified model will be used.",
    //                 "The order that fields are loaded is the natural order of the rows in the source file.",
    //                 "If the -vf|--fieldnames option is also used, those fields will be added after any obtained through the -s|--source     load."
    //             },
    //             paratype = ParameterType.Input,
    //             nextparatype = ParameterType.File
    //         });

    //        ps.Add(new ParameterSetting(){
    //             category = "view",
    //             setting = "--fieldnames",
    //             synonym = "-vf",
    //             description = "View Field Names",
    //             helptext = new List<string>(){
    //                 "Usage: csgen view -vf fieldnames",
    //                 "",
    //                 "Specify a comma-separated list of ViewModel fieldnames used in the View.",
    //                 "",
    //                 "The format of this is:",
    //                 "vmfieldname1[,vmfieldname2][,...]",
    //                 "",
    //                 "If the -s|--source     option is also used, fields will be loaded from that file first and ones specified with -f are appended.",
    //                 "Each name must be valid as a CSS/HTML name."
    //             },
    //             paratype = ParameterType.Input,
    //             nextparatype = ParameterType.HtmlFieldName,
    //             nextparaseparator = ","
    //         });

    //        ps.Add(new ParameterSetting(){
    //             category = "view",
    //             setting = "--fieldtypes",
    //             synonym = "-vt",
    //             description = "View Field Types",
    //             helptext = new List<string>(){
    //                 "Usage: csgen view -vt fieldtypes",
    //                 "",
    //                 "Specify a comma-separated list of C# types associated with fields.",
    //                 "",
    //                 "The format of this is:",
    //                 "vmfieldtype1[,vmfieldtype2][,...]",
    //                 "",
    //                 "If not specified, the default is string."
    //             },
    //             paratype = ParameterType.Input,
    //             nextparatype = ParameterType.CsClassName,
    //             nextparaseparator = ","
    //         });

    //        ps.Add(new ParameterSetting(){
    //             category = "view",
    //             setting = "--fieldsizes",
    //             synonym = "-vz",
    //             description = "View Field Sizes",
    //             helptext = new List<string>(){
    //                 "Usage: csgen view -vz fieldsizes",
    //                 "",
    //                 "Specify a comma-separated list of field sizes.",
    //                 "",
    //                 "The format of this is:",
    //                 "vmfieldsize1[,vmfieldsize2][,...]",
    //                 "",
    //                 "Where the field size is set to zero then this is ignored."
    //             },
    //             paratype = ParameterType.Input,
    //             nextparatype = ParameterType.Integer,
    //             nextparaseparator = ",",
    //             nextparaintmin = 0
    //         });


    //        ps.Add(new ParameterSetting(){
    //             category = "view",
    //             setting = "--fielddescs",
    //             synonym = "-vd",
    //             description = "View Field Descriptions",
    //             helptext = new List<string>(){
    //                 "Usage: csgen view -vd fielddescs",
    //                 "",
    //                 "Specify a comma-separated list of field descriptions.",
    //                 "",
    //                 "The format of this is:",
    //                 "vmfielddesc1[,vmfielddesc2][,...]"
    //             },
    //             paratype = ParameterType.Input,
    //             nextparatype = ParameterType.Any,
    //             nextparaseparator = ","
    //         });

    //        ps.Add(new ParameterSetting(){
    //             category = "view",
    //             setting = "--fieldcaps",
    //             synonym = "-va",
    //             description = "View Field Captions",
    //             helptext = new List<string>(){
    //                 "Usage: csgen view -va fieldcaps",
    //                 "",
    //                 "Specify a comma-separated list of the caption text that will appear in the View.",
    //                 "",
    //                 "The format of this is:",
    //                 "vmfieldcap1[,vmfieldcap2][,...]"
    //             },
    //             paratype = ParameterType.Input,
    //             nextparatype = ParameterType.Any,
    //             nextparaseparator = ","
    //         });

    //        ps.Add(new ParameterSetting(){
    //             category = "view",
    //             setting = "--fieldreqs",
    //             synonym = "-vq",
    //             description = "View Field Required Text",
    //             helptext = new List<string>(){
    //                 "Usage: csgen view -vq fieldreqs",
    //                 "",
    //                 "Specify a comma-separated list of text that appears when required.",
    //                 "Note that if blank or missing, the field is not required.",
    //                 "",
    //                 "The format of this is:",
    //                 "vmfieldreq1[,vmfieldreq2][,...]"
    //             },
    //             paratype = ParameterType.Input,
    //             nextparatype = ParameterType.Any,
    //             nextparaseparator = ","
    //         });


    //        ps.Add(new ParameterSetting(){
    //             category = "view",
    //             setting = "--fieldtypes",
    //             synonym = "-vt",
    //             description = "View Field Types",
    //             helptext = new List<string>(){
    //                 "Usage: csgen view -vt fieldtypes",
    //                 "",
    //                 "Specify a comma-separated list of C# types associated with fields.",
    //                 "",
    //                 "The format of this is:",
    //                 "vmfieldtype1[,vmfieldtype2][,...]",
    //                 "",
    //                 "If not specified, the default is string."
    //             },
    //             paratype = ParameterType.Input,
    //             nextparatype = ParameterType.CsClassName,
    //             nextparaseparator = ","
    //         });




    //        ps.Add(new ParameterSetting(){
    //             category = "view",
    //             setting = "--viewftype",
    //             synonym = "-wy",
    //             description = "View Field Types",
    //             helptext = new List<string>(){
    //                 "Usage: csgen view -wy viewfieldtypes",
    //                 "",
    //                 "Comma-separated list of View form field HTML types in order.",
    //                 "",
    //                 "The format of this is:",
    //                 "viewftype1[,viewftype2][,...]]"
    //             },
    //             paratype = ParameterType.Input,
    //             nextparatype = ParameterType.HtmlFieldType,
    //             nextparaseparator = ","
    //         });

    //        ps.Add(new ParameterSetting(){
    //             category = "view",
    //             setting = "--viewfdclass",
    //             synonym = "-wd",
    //             description = "View Field Classes",
    //             helptext = new List<string>(){
    //                 "Usage: csgen view -wd cssclasses",
    //                 "",
    //                 "Specify a comma-separated list of colon-delimited CSS classes that wrap the associated form field in <div> tags.",
    //                 "",
    //                 "The format of this is:",
    //                 "viewfdclass1a:viewfdclass1b[:viewfdclass1c][,viewfdclass2a][,...]",
    //                 "",
    //                 "Each class can have spaces.  Separate divs are created with colons.",
    //                 "",
    //                 "For example:",
    //                 "<div class=\"row justify-content-center\">",
    //                 "    <div class=\"col-xxl-5 col-xl-5 col-lg-7 col-md-10\">",
    //                 "        <div class=\"section-title text-center mb-50\">",
    //                 "",
    //                 "The format of this is:",
    //                 "row justify-content-center:col-xxl-5 col-xl-5 col-lg-7 col-md-10:section-title text-center mb-50"
    //             },
    //             paratype = ParameterType.Input,
    //             nextparatype = ParameterType.Any,
    //             nextparaseparator = ","
    //         });

    //        ps.Add(new ParameterSetting(){
    //             category = "view",
    //             setting = "--fieldreqs",
    //             synonym = "-wq",
    //             description = "View Field Required Text",
    //             helptext = new List<string>(){
    //                 "Usage: csgen view -wq fieldreqs",
    //                 "",
    //                 "Specify a comma-separated list of text that appears when required.",
    //                 "Note that if blank or missing, the field is not required.",
    //                 "",
    //                 "The format of this is:",
    //                 "vmfieldreq1[,vmfieldreq2][,...]"
    //             },
    //             paratype = ParameterType.Input,
    //             nextparatype = ParameterType.Any,
    //             nextparaseparator = ","
    //         });


    //        ps.Add(new ParameterSetting(){
    //             category = "view",
    //             setting = "--fieldtypes",
    //             synonym = "-wt",
    //             description = "View Field Types",
    //             helptext = new List<string>(){
    //                 "Usage: csgen view -wt fieldtypes",
    //                 "",
    //                 "Specify a comma-separated list of C# types associated with fields.",
    //                 "",
    //                 "The format of this is:",
    //                 "vmfieldtype1[,vmfieldtype2][,...]",
    //                 "",
    //                 "If not specified, the default is string."
    //             },
    //             paratype = ParameterType.Input,
    //             nextparatype = ParameterType.CsClassName,
    //             nextparaseparator = ","
    //         });




    //        ps.Add(new ParameterSetting(){
    //             category = "view",
    //             setting = "--key",
    //             synonym = "-k",
    //             description = "View Primary Key",
    //             helptext = new List<string>(){
    //                 "Usage: csgen vm -k pkeyfieldname",
    //                 "",
    //                 "Specify the name of the primary key field.",
    //                 "",
    //                 "If not specified, no pkey will be set."
    //             },
    //             paratype = ParameterType.Input,
    //             nextparatype = ParameterType.CsClassName
    //         });

    //     }
    }
}