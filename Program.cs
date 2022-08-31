
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
            bool testfparameters = false;
            bool testwparameters = false;
            bool testcparameters = false;
            bool testcontroller = false; // 
            bool testmodel = false;
            bool testview = false;
            bool testvm = false;


            if(testfparameters)
            {

// TO DO: come back and fix this after the facade
                string outputfile = @"c:\SNJW\code\xo\Models\facade\ModelFacade.cs";
                cg.parameters.Clear();
                cg.parameters.Add("facade");

                cg.parameters.Add("--fname");
                cg.parameters.Add("ModelFacade");
                cg.parameters.Add("--vname");
                cg.parameters.Add("model");
                cg.parameters.Add("--mname");
                cg.parameters.Add("Model");
                cg.parameters.Add("--fnamespace");
                cg.parameters.Add("Seamlex.MyEdApps");
                cg.parameters.Add("--fdbsave");
                cg.parameters.Add("--source");
                cg.parameters.Add(@"C:\SNJW\code\scriptloader\scriptloader-small.csv");
                cg.parameters.Add("--output");
                cg.parameters.Add(outputfile);
                // cg.parameters.Add("--cparent");
                // cg.parameters.Add("Controller");
                // cg.parameters.Add("--croute");
                // cg.parameters.Add("Model/Create");
                cg.parameters.Add("--cdcontext");
                cg.parameters.Add("xo.Data.ApplicationDbContext");
                // cg.parameters.Add("--cdpropname");
                // cg.parameters.Add("db");
                cg.parameters.Add("--vpkey");
                cg.parameters.Add("id");
                cg.parameters.Add("--vfkey");
                cg.parameters.Add("userid");
                cg.parameters.Add("--vftable");
                cg.parameters.Add("AspNetUsers");
                cg.parameters.Add("--vuserkey");
                cg.parameters.Add("userid");
                cg.parameters.Add("--vmessage");
                cg.parameters.Add("message");
                cg.parameters.Add("--mpkey");
                cg.parameters.Add("Id");
                cg.parameters.Add("--mfkey");
                cg.parameters.Add("Id");
                cg.parameters.Add("--mparent");
                cg.parameters.Add("AspNetUsers");
                cg.parameters.Add("--mparkey");
                cg.parameters.Add("Id");

                // cg.parameters.Add("--cacthttps");
                // cg.parameters.Add("GET:GET/POST:GET/POST:GET/POST:GET");
                // cg.parameters.Add("--cactnames");
                // cg.parameters.Add("Create:Index:Edit:Delete:Details");
                // cg.parameters.Add("--cacttypes");
                // cg.parameters.Add("Create:Index:Edit:Delete:Details");
                // cg.parameters.Add("--cvnames");
                // cg.parameters.Add("model");
                // cg.parameters.Add("--cmnames");
                // cg.parameters.Add("Model");
                // cg.parameters.Add("--cwnames");
                // cg.parameters.Add("Create:Index:Edit:Delete:Details");
                // cg.parameters.Add("--cfnames");
                // cg.parameters.Add("ModelFacade");

                cg.parameters.Add("--cvpkeys");
                cg.parameters.Add("id");
                cg.parameters.Add("--cvfkeys");
                cg.parameters.Add("userid");
                cg.parameters.Add("--cmpkeys");
                cg.parameters.Add("ModelId");
                cg.parameters.Add("--cmfkeys");
                cg.parameters.Add("ModelUserId");
                cg.parameters.Add("--cmparents");
                cg.parameters.Add("AspNetUsers");
                cg.parameters.Add("--cmparkeys");
                cg.parameters.Add("Id");

                cg.parameters.Add("--cvmkeys");
                cg.parameters.Add("Id");
                cg.parameters.Add("--cvukeys");
                cg.parameters.Add("userid");
                cg.parameters.Add("--cvmsgs");
                cg.parameters.Add("message");

                cg.parameters.Add("--ftype");
                cg.parameters.Add("insert");

                cg.parameters.Add("--fillempty");
                // cg.parameters.Add("--cnofacade");


                cg.Run();
                if(System.IO.File.Exists(outputfile))
                    System.Diagnostics.Process.Start("notepad.exe",outputfile);
                return;                
            }



/*
c:\SNJW\code\shared\csgen.exe controller --cname ModelController --source "C:\SNJW\code\scriptloader\scriptloader-small.csv" --output c:\SNJW\code\xo\Controllers\ModelController.cs --cparent Controller --croute Model/Create --ccontext xo.Data.ApplicationDbContext --vpkey id --vfkey userid --vftable AspNetUsers --vuserkey userid --vmessage message  --mpkey id --mfkey userid --mftable AspNetUsers --chttps GET:GET/POST:GET/POST:GET/POST:GET --cactnames Index:Create:Edit:Delete:Details --cacttypes Index:Create:Edit:Delete:Details --cvnames model --cmnames Model --cwnames Index:Create:Edit:Delete:Details --cvpkeys id --cvfkeys userid --cmpkeys ModelId --cmfkeys ModelUserId --cmparents AspNetUsers --cvukeys userid --cvmsgs message --fillempty
*/

            if(testcparameters)
            {

// TO DO: come back and fix this after the facade
                string outputfile = @"c:\SNJW\code\xo\Controllers\ModelController2.cs";
                cg.parameters.Clear();
                cg.parameters.Add("controller");

                cg.parameters.Add("--cname");
                cg.parameters.Add("ModelController");
                // cg.parameters.Add("--cnamespace");
                // cg.parameters.Add("Seamlex.MyEdApps");
                cg.parameters.Add("--source");
                cg.parameters.Add(@"C:\SNJW\code\scriptloader\scriptloader-small.csv");
                cg.parameters.Add("--output");
                cg.parameters.Add(outputfile);
                cg.parameters.Add("--cparent");
                cg.parameters.Add("Controller");
                cg.parameters.Add("--croute");
                cg.parameters.Add("Model/Create");
                cg.parameters.Add("--cdcontext");
                cg.parameters.Add("xo.Data.ApplicationDbContext");
                cg.parameters.Add("--cdpropname");
                cg.parameters.Add("db");
                cg.parameters.Add("--vpkey");
                cg.parameters.Add("id");
                cg.parameters.Add("--vfkey");
                cg.parameters.Add("userid");
                cg.parameters.Add("--vftable");
                cg.parameters.Add("AspNetUsers");
                cg.parameters.Add("--vuserkey");
                cg.parameters.Add("userid");
                cg.parameters.Add("--vmessage");
                cg.parameters.Add("message");
                cg.parameters.Add("--mpkey");
                cg.parameters.Add("id");
                cg.parameters.Add("--mfkey");
                cg.parameters.Add("userid");
                cg.parameters.Add("--mftable");
                cg.parameters.Add("AspNetUsers");
                cg.parameters.Add("--cacthttps");
                cg.parameters.Add("GET:GET/POST:GET/POST:GET/POST:GET");
                cg.parameters.Add("--cactnames");
                cg.parameters.Add("Create:Index:Edit:Delete:Details");
                cg.parameters.Add("--cacttypes");
                cg.parameters.Add("Create:Index:Edit:Delete:Details");
                cg.parameters.Add("--cvnames");
                cg.parameters.Add("model");
                cg.parameters.Add("--cmnames");
                cg.parameters.Add("Model");
                cg.parameters.Add("--cwnames");
                cg.parameters.Add("Create:Index:Edit:Delete:Details");
                cg.parameters.Add("--cfnames");
                cg.parameters.Add("ModelFacade");
                cg.parameters.Add("--cvpkeys");
                cg.parameters.Add("id");
                cg.parameters.Add("--cvfkeys");
                cg.parameters.Add("userid");
                cg.parameters.Add("--cmpkeys");
                cg.parameters.Add("ModelId");
                cg.parameters.Add("--cmfkeys");
                cg.parameters.Add("ModelUserId");
                cg.parameters.Add("--cmparents");
                cg.parameters.Add("AspNetUsers");
                cg.parameters.Add("--cmparkeys");
                cg.parameters.Add("Id");

                cg.parameters.Add("--cvmkeys");
                cg.parameters.Add("Id");
                cg.parameters.Add("--cvukeys");
                cg.parameters.Add("userid");
                cg.parameters.Add("--cvmsgs");
                cg.parameters.Add("message");

                cg.parameters.Add("--fillempty");
                cg.parameters.Add("--cnofacade");


                cg.Run();
                if(System.IO.File.Exists(outputfile))
                    System.Diagnostics.Process.Start("notepad.exe",outputfile);
                return;                
            }


/*

c:\SNJW\code\shared\csgen.exe view --wname "Create Model" --sourcefile xoload-small.csv --output c:\SNJW\code\xo\Views\Model\Create.cshtml --vpkey id --vfkey userid --vftable AspNetUsers --vuserkey userid --vmessage message --wsubmit Submit --wsubaction Create --wreturn Index --wfrmaction Submit --winfohead Create a new Model" --winfotext "Enter the new model details below and click 'Create'" --wlayfiles _MainHeadPartial.cshtml:_MainStylesPartial.cshtml:_MainPreloadPartial.cshtml:_MainHeaderPartial.cshtml:_MainClientPartial.cshtml:_MainFooterPartial.cshtml:_MainScriptsPartial.cshtml --laynames Head:Styles:Preload:Header:Client:Footer:Scripts --wlayout Layout

*/

            if(testwparameters)
            {

                string outputfile = @"c:\SNJW\code\xo\Views\Model\Create.cshtml";
                cg.parameters.Clear();
                cg.parameters.Add("view");

                cg.parameters.Add("--wname");
                cg.parameters.Add("Create Model");
                cg.parameters.Add("--waction");
                cg.parameters.Add("Create");
                // cg.parameters.Add("--source");
                // cg.parameters.Add(@"C:\SNJW\code\scriptloader\scriptloader-small.csv");
                cg.parameters.Add("--output");
                cg.parameters.Add(outputfile);
                cg.parameters.Add("--vname");
                cg.parameters.Add("model");
                cg.parameters.Add("--vfnames");
                cg.parameters.Add("id,code,name,desc,message");
                cg.parameters.Add("--vftypes");
                cg.parameters.Add("string");
                cg.parameters.Add("--vpkey");
                cg.parameters.Add("id");
                cg.parameters.Add("--vfkey");
                cg.parameters.Add("userid");
                cg.parameters.Add("--vftable");
                cg.parameters.Add("AspNetUsers");
                cg.parameters.Add("--vuserkey");
                cg.parameters.Add("userid");
                cg.parameters.Add("--vmessage");
                cg.parameters.Add("message");
                cg.parameters.Add("--wsubmit");
                cg.parameters.Add("Submit");
                cg.parameters.Add("--wsubaction");
                cg.parameters.Add("Create");
                cg.parameters.Add("--wreturn");
                cg.parameters.Add("Index");
                cg.parameters.Add("--wfrmaction");
                cg.parameters.Add("Submit");
                cg.parameters.Add("--winfohead");
                cg.parameters.Add("Create a new Model");
                cg.parameters.Add("--winfotext");
                cg.parameters.Add("Enter the new model details below and click 'Create'");
                cg.parameters.Add("--wlayfiles");
                cg.parameters.Add("_MainHeadPartial.cshtml:_MainStylesPartial.cshtml:_MainPreloadPartial.cshtml:_MainHeaderPartial.cshtml:_MainClientPartial.cshtml:_MainFooterPartial.cshtml:_MainScriptsPartial.cshtml");
                cg.parameters.Add("--wlaynames");
                cg.parameters.Add("Head:Styles:Preload:Header:Client:Footer:Scripts");
                cg.parameters.Add("--wlayout");
                cg.parameters.Add("Layout");

                cg.parameters.Add("--wfdclasses");
                cg.parameters.Add("col-md-6:single-input");
                cg.parameters.Add("--wficlasses");
                cg.parameters.Add(",,lni lni-user,lni lni-phone,lni lni-format,lni lni-comments-alt,lni lni-envelope");
                cg.parameters.Add("--wfclasses");
                cg.parameters.Add("form-input");


                cg.parameters.Add("--fillempty");

    
                cg.Run();
                if(System.IO.File.Exists(outputfile))
                    System.Diagnostics.Process.Start("notepad.exe",outputfile);
                return;                
            }

        



/*
        public string cname {get;set;} = "";  // Name of the Controller
        public string cnamespace {get;set;} = "";  // Namespace of the Controller
        public string cparent {get;set;} = "";  // Name of the parentClass
        public string ccontext {get;set;} = "";  // Class name of the ApplicationContext
        public string cfacade {get;set;} = "";  // Does Controller use a façade?
        public bool cidentity {get;set;} = true;  // Does Controller use Identity?
        public bool casync {get;set;} = true;  // Does Controller use asynchronous methods?
        public bool cdbsave {get;set;} = true;  // Does Controller issue a dbsave?
        public bool cbinding {get;set;} = true;  // Does Controller bind individual fields?
        public string chttp {get;set;} = "";  // GET/SET action properties
        public string cactname {get;set;} = "";  // action name
        public string cactsyn {get;set;} = "";  // action synonyms
        public string cacttype {get;set;} = "";  // type (Create/Delete/Edit/Index/Details)
        public string cactvm {get;set;} = "";  // ViewModel names
        public string cactmodel {get;set;} = "";  // Model name
        public string cactview {get;set;} = "";  // View name
        public string cactpkey {get;set;} = "";  // ViewModel primary key field
        public string cactfkey {get;set;} = "";  // ViewModel foreign key field
        public string cactukey {get;set;} = "";  // ViewModel user key field
        public string cactmsg {get;set;} = "";  // ViewModel message field

*/

            if(testcontroller)
            {

                string output = @"c:\SNJW\code\xp\Areas\Election\Controllers\ElectionController.cs";

                cg.parameters.Clear();
                cg.parameters.Add("controller");
                cg.parameters.Add("--cname");
                cg.parameters.Add("ElectionController");
                cg.parameters.Add("--cnamespace");
                cg.parameters.Add("Seamlex.MyEdApps");
                cg.parameters.Add("--cparent");
                cg.parameters.Add("Controller");
                cg.parameters.Add("--cdcontext");
                cg.parameters.Add("xp.Data.ApplicationDbContext");
                cg.parameters.Add("--careaname");
                cg.parameters.Add("Election");

                cg.parameters.Add("--cnobinding");

                cg.parameters.Add("--source");
                cg.parameters.Add(@"C:\SNJW\code\scriptloader\scriptloader-election.csv");

                cg.parameters.Add("--output");
                cg.parameters.Add(output);

                cg.parameters.Add("--cacthttps");
                cg.parameters.Add("GET/POST:GET:GET/POST:GET/POST:GET");
                cg.parameters.Add("--cactnames");
                cg.parameters.Add("Create:Index:Edit:Delete:Details");
                cg.parameters.Add("--cacttypes");
                cg.parameters.Add("Create:Index:Edit:Delete:Details");


                cg.parameters.Add("--vname");  //  Colon-delimited ViewModel names
                cg.parameters.Add("election");

                cg.parameters.Add("--cvnames");  //  Colon-delimited ViewModel names
                cg.parameters.Add("election");
                cg.parameters.Add("--cmnames");  // Colon-delimited Model names
                cg.parameters.Add("Election");
                cg.parameters.Add("--cwnames");  // Colon-delimited View names
                cg.parameters.Add("Create Election");
                cg.parameters.Add("--cvpkeys");  // Colon-delimited ViewModel primary key fields
                cg.parameters.Add("id");
                cg.parameters.Add("--cvfkeys");  // Colon-delimited ViewModel foreign key fields
                cg.parameters.Add("userid");
                cg.parameters.Add("--cmpkeys");  // Colon-delimited Model primary key fields
                cg.parameters.Add("Id");
                cg.parameters.Add("--cmfkeys");  // Colon-delimited Model foreign key fields
                cg.parameters.Add("ElectionUserId");
                cg.parameters.Add("--cmparents");  // Colon-delimited Model parent table names
                cg.parameters.Add("User");
                cg.parameters.Add("--cvukeys");  // Colon-delimited action ViewModel user key fields
                cg.parameters.Add("userid");
                cg.parameters.Add("--cvmsgs");  // Colon-delimited action ViewModel message fields
                cg.parameters.Add("message");

                cg.parameters.Add("--cfnames");  // Colon-delimited Facade names
                cg.parameters.Add("enfacade");

                cg.parameters.Add("--fillempty");

//                cg.parameters.Add("--cnofacade");

/*

                // cg.parameters.Add("--source");
                // cg.parameters.Add("c:\\temp\\sourcefile.csv");
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
                cg.parameters.Add("--vuserkey");
                cg.parameters.Add("userid");
                cg.parameters.Add("--vmessage");
                cg.parameters.Add("message");

                cg.parameters.Add("--mfnames");
                cg.parameters.Add("Id,foreigntableId,Code,Name,Desc,Start,Message");
                cg.parameters.Add("--mftypes");
                cg.parameters.Add("Guid,Guid,string,string,string,DateTime,string");
                // cg.parameters.Add("--mfsizes");
                // cg.parameters.Add("0,0,10,0,0,0,100");
                // cg.parameters.Add("--mfsizes");
                // cg.parameters.Add("string,string,string,string,string,DateTime,string");

                */

                cg.Run();
                System.Diagnostics.Process.Start("notepad.exe",output);
                return;                
            }


/*

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using lm.Models;
using lm.Models.vm;
using lm.Data.Context;

// disable compiler warnings: some methods are synchronous but will be asynchronous later, and others I have left in as placeholders
#pragma warning disable 1998

namespace lm.Controllers
{
    public class ModelController : Controller
    {
        private readonly lmdbContext db;

        public ModelController(lmdbContext context)
        {
            db = context;
        }

        // GET: Model
        public async Task<IActionResult> Index()
        {

*/
            if(testview)
            {
                cg.parameters.Clear();
                cg.parameters.Add("view");
                cg.parameters.Add("--wname");
                cg.parameters.Add("Election List");
                cg.parameters.Add("--vname");
                cg.parameters.Add("election");
                cg.parameters.Add("--vnamespace");
                cg.parameters.Add("Seamlex.MyEdApps");
                cg.parameters.Add("--output");
                cg.parameters.Add("c:\\temp\\list.cshtml");
                // cg.parameters.Add("--source");
                // cg.parameters.Add("c:\\temp\\sourcefile.csv");
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
                cg.parameters.Add("--vuserkey");
                cg.parameters.Add("userid");
                cg.parameters.Add("--vmessage");
                cg.parameters.Add("message");

                cg.parameters.Add("--wftypes");
                cg.parameters.Add("hidden,hidden,text,text,textarea,date,hidden");

                cg.parameters.Add("--wfdclasses");
                cg.parameters.Add("col-md-6:single-input,col-md-6:single-input,col-md-6:single-input,col-md-6:single-input,col-md-12:single-input,col-md-6:single-input,col-md-6:single-input");
                cg.parameters.Add("--wficlasses");
                cg.parameters.Add(",,lni lni-user,lni lni-phone,lni lni-format,lni lni-comments-alt,lni lni-envelope");
                cg.parameters.Add("--wfclasses");
                cg.parameters.Add("form-input,form-input,form-input,form-input,form-input,form-input,form-input");


                // cg.parameters.Add("--vfkey");
                // cg.parameters.Add("parentid");
                // cg.parameters.Add("--vftable");
                // cg.parameters.Add("parenttable");
                //cg.parameters.Add("--help");

                cg.parameters.Add("--wsubmit");
                cg.parameters.Add("button");
                cg.parameters.Add("--wsubaction");
                cg.parameters.Add("post");
                cg.parameters.Add("--wsubdclass");
                cg.parameters.Add("col-md-12:form-button");
                cg.parameters.Add("--wsubiclass");
                cg.parameters.Add("lni lni-telegram-original");

                cg.parameters.Add("--wreturn");
                cg.parameters.Add("a");
                cg.parameters.Add("--wretaction");
                cg.parameters.Add("link");
                cg.parameters.Add("--wretdclass");
                cg.parameters.Add("col-md-12:form-button");
                cg.parameters.Add("--wreticlass");
                cg.parameters.Add("lni lni-telegram-original");

                cg.parameters.Add("--wfrmaction");
                cg.parameters.Add("Create");
                cg.parameters.Add("--wfrmclass");
                cg.parameters.Add("row:col-lg-8:contact-form-wrapper");
                cg.parameters.Add("--wfrmsub");
                cg.parameters.Add("row");


                cg.parameters.Add("--wpageclass");
                cg.parameters.Add("container");
                cg.parameters.Add("--winfoclass");
                cg.parameters.Add("row justify-content-center:col-xxl-5 col-xl-5 col-lg-7 col-md-10:section-title text-center mb-50");
                cg.parameters.Add("--winfohclass");
                cg.parameters.Add("mb-15");
                cg.parameters.Add("--winfohead");
                cg.parameters.Add("Manage Elections");
                cg.parameters.Add("--winfotext");
                cg.parameters.Add("Below is a list of all elections that you manage");

                cg.parameters.Add("--wlayfiles");
                cg.parameters.Add("_MainHeadPartial.cshtml:_MainStylesPartial.cshtml:_MainPreloadPartial.cshtml:_MainHeaderPartial.cshtml:_MainClientPartial.cshtml:_MainFooterPartial.cshtml:_MainScriptsPartial.cshtml");
                cg.parameters.Add("--wlaynames");
                cg.parameters.Add("Head:Styles:Preload:Header:Client:Footer:Scripts");
                cg.parameters.Add("--wlayout");
                cg.parameters.Add("_Main");

                cg.parameters.Add("--waction");
                cg.parameters.Add("create");

                cg.Run();
                System.Diagnostics.Process.Start("notepad.exe","c:\\temp\\list.cshtml");
                return;   
            }

            if(testmodel)
            {


 // c:\SNJW\code\shared\csgen.exe model --mname Ballot --source "C:\SNJW\code\scriptloader\scriptloader-election.csv" --output c:\SNJW\code\xp\Areas\Election\Models\data\Ballot.cs --mpkey BallotId --mfkey BallotElectionId --mparent Election
                string output = @"c:\SNJW\code\xp\Areas\Election\Models\data\Ballot.cs";               
                cg.parameters.Clear();
                cg.parameters.Add("model");
                cg.parameters.Add("--mname");
                cg.parameters.Add("Ballot");
                //   cg.parameters.Add("--mnamespace");
                //   cg.parameters.Add("Seamlex.MyEdApps");
                cg.parameters.Add("--source");
                cg.parameters.Add(@"C:\SNJW\code\scriptloader\scriptloader-election.csv");
                cg.parameters.Add("--output");
                cg.parameters.Add(output);
                cg.parameters.Add("--mpkey");
                cg.parameters.Add("BallotId");
                cg.parameters.Add("--mfkey");
                cg.parameters.Add("BallotElectionId");
                cg.parameters.Add("--mparent");
                cg.parameters.Add("Election");


                cg.Run();
                System.Diagnostics.Process.Start("notepad.exe",output);
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
                cg.parameters.Add("--source");
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