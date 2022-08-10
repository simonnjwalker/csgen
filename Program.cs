
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Seamlex.Utilities;
#pragma warning disable CS8602, CS8600
namespace Seamlex.Utilities
{
    public class Program
    {
        static void Main(string[] args)
        {
            CsgenLegacy csgenold = new CsgenLegacy();
            CsgenMain cg = new CsgenMain();
            // test
            bool testmodel = false;
            bool testvm = false;
            if(testmodel)
            {
                cg.parameters.Clear();
                cg.parameters.Add("model");
                cg.parameters.Add("--mname");
                cg.parameters.Add("election");
                //   cg.parameters.Add("--mnamespace");
                //   cg.parameters.Add("Seamlex.MyEdApps");
                cg.parameters.Add("--output");
                cg.parameters.Add("c:\\temp\\election.cs");
                cg.parameters.Add("--sourcefile");
                cg.parameters.Add("c:\\temp\\sourcefile.csv");
                cg.parameters.Add("--mfnames");
                cg.parameters.Add("id,userid,code,name,desc,start,message");
                cg.parameters.Add("--mftypes");
                cg.parameters.Add("string,string,string,string,string,DateTime,string");
                // cg.parameters.Add("--vfsizes");
                // cg.parameters.Add("32,32,10,0,0,0,100");
                // cg.parameters.Add("--vfdescs");
                // cg.parameters.Add("Id,UserId,Code,Name,Description,Start Date/Time,System Message");
                // cg.parameters.Add("--vfcaps");
                // cg.parameters.Add("Id,UserId,Code,Name,Description,Start Date/Time,System Message");
                cg.parameters.Add("--mpkey");
                cg.parameters.Add("id");
                // cg.parameters.Add("--vfkey");
                // cg.parameters.Add("parentid");
                // cg.parameters.Add("--vftable");
                // cg.parameters.Add("parenttable");
                //cg.parameters.Add("--help");

                cg.Run();
                return;                
            }



            if(testvm)
            {
                cg.parameters.Clear();
                cg.parameters.Add("vm");
                cg.parameters.Add("--vname");
                cg.parameters.Add("election");
                cg.parameters.Add("--vnamespace");
                cg.parameters.Add("Seamlex.MyEdApps");
                cg.parameters.Add("--output");
                cg.parameters.Add("c:\\temp\\election.cs");
                cg.parameters.Add("--sourcefile");
                cg.parameters.Add("c:\\temp\\sourcefile.csv");
                cg.parameters.Add("--vfnames");
                cg.parameters.Add("id");
                cg.parameters.Add("--vfnames");
                cg.parameters.Add("id,userid,code,name,desc,start,message");
                cg.parameters.Add("--vftypes");
                cg.parameters.Add("string,string,string,string,string,DateTime,string");
                cg.parameters.Add("--vfsizes");
                cg.parameters.Add("32,32,10,0,0,0,100");
                cg.parameters.Add("--vfdescs");
                cg.parameters.Add("Id,UserId,Code,Name,Description,Start Date/Time,System Message");
                cg.parameters.Add("--vfcaps");
                cg.parameters.Add("Id,UserId,Code,Name,Description,Start Date/Time,System Message");
                cg.parameters.Add("--vpkey");
                cg.parameters.Add("id");
                // cg.parameters.Add("--vfkey");
                // cg.parameters.Add("parentid");
                // cg.parameters.Add("--vftable");
                // cg.parameters.Add("parenttable");
                //cg.parameters.Add("--help");

                cg.Run();
                return;                
            }

                // cg.parameters.Add("-s");
                // cg.parameters.Add(@"C:\temp\vmdest.cs");
                // cg.parameters.Add("-s");
                // cg.parameters.Add(@"C:\temp\vmdest.cs");
                // cg.parameters.Add("-h");


                // Console.WriteLine();
                // Console.WriteLine("test 2:");
                // cg.parameters.Clear();
                // cg.parameters.Add("vm");
                // cg.Run();
                // Console.WriteLine();
                // Console.WriteLine("test 3:");
                // cg.parameters.Clear();
                // cg.parameters.Add("vm");
                // cg.parameters.Add("--help");
                // cg.Run();
                // Console.WriteLine();
                // Console.WriteLine("test 4:");
                // cg.parameters.Clear();
                // cg.parameters.Add("vm");
                // cg.parameters.Add("-f");
                // cg.Run();
                // return;


//             if(testreplacenerror)
//             {
//                 cg.parameters.Add("replacenth");
//                 cg.parameters.Add(@"C:\SNJW\code\xk\Areas\Identity\Pages\Account\LoginWith2fa.cshtml.cs");
//                 cg.parameters.Add("using Microsoft.AspNetCore.Mvc;");
//                 cg.parameters.Add("// using Microsoft.AspNetCore.Mvc;");
// //                cg.parameters.Add(@"1");
//                 cg.parameters.Add(@"C:\SNJW\code\xk\Areas\Identity\Pages\Account\LoginWith2fa.cshtml.cs");


//                 cg.RunLegacy();
//                 return;
//             }
//             if(testinsert)
//             {
//                 cg.parameters.Add("insert");
//                 cg.parameters.Add(@"c:\temp\test-insert-source.txt");
//                 cg.parameters.Add("");
//                 cg.parameters.Add("3");
//                 cg.parameters.Add(@"c:\temp\test-insert-output.txt");


//                 cg.RunLegacy();
//                 return;
//             }
//             if(testmodelgen)
//             {
//                 cg.parameters.Add("model");
//                 cg.parameters.Add("new");
//                 cg.parameters.Add(@"C:\SNJW\code\zc\Data\Entities\Project.cs");
//                 cg.parameters.Add("name=Project;namespace=zc.Models;fields=Id:Guid,Code:String,Name:String,Desc:String;fieldprefix=Project");
//                 // var setup = cg.GetModel(cg.parameters[1],cg.parameters[2],cg.parameters[3]);
//                 // foreach(var line in cg.GetModelText(setup).Split(System.Environment.NewLine,StringSplitOptions.None))
//                 //     cg.Message(line);

//                 cg.RunLegacy();
//                 return;
//             }

//                 // this.Message(@"csgen newmodel  ""name=Project;namespace=zc.Models;fields=Id:Guid,Code:String,Name:String,Desc:String;fieldprefix=Project""");

//             if(testreplacen)
//             {
//                 cg.parameters.Add("replacen");
//                 cg.parameters.Add(@"c:\temp\test-replacen-source.txt");
//                 cg.parameters.Add("searchfor");
//                 cg.parameters.Add("replaceme");
//                 cg.parameters.Add("2");
//                 cg.parameters.Add("1");
// //                cg.parameters.Add(@"c:\temp\test-replacen-output.txt");
//                 cg.RunLegacy();
//                 return;
//             }

            // 2022-07-27 SNJW made parameters generic
            // If this is one of the 'original' items then use the old code 
            if(args.Length>0)
            {
                string checkcommand = ","+(args[0]).ToLower().Trim()+",";
                string olditems = ",replacedq,replacewithdq,replacechar,replacechr,replaceasc,replaceascii,replacechar,replace,replacen,replacenth,insert,insertline,";
                if(!olditems.Contains(checkcommand))
                {
                    // do new stuff...
                    cg.SetParameters(args);
                    cg.Run();

                    // if(!cg.Run())
                    //     Console.WriteLine("csgen.exe did not complete.");
                    return;
                }
            }


            if(args.Length==0)
            {
                csgenold.NoParameters();
            }
            else if(args.Length==1)
            {
                csgenold.OneParameter(args[0].ToString());
            }
            else
            {
                csgenold.SetParameters(args);
                csgenold.Run();
                // Console.WriteLine("");
                // Console.WriteLine("csgen.exe completed");
            }
        }


        public void Message(string message) {Console.WriteLine(message);}
    }

}