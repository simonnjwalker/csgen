
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
    public class CsgenMain
    {
        public List<string> parameters = new List<string>();

        public bool Run()
        {
            // this performed a generic analysis of parameters
            ParameterHander ph = new ParameterHander();

            // this is an app-specific setup of parameter-info
            CsgenParameters csgenparainfo = new CsgenParameters();
            csgenparainfo.SetParameterInfo();

            ph.ps.AddRange(csgenparainfo.ps);
            if(!ph.SetParameters(parameters))
            {
                this.Message(ph.lastmessage);
            }
            else if(ph.IsHelpRequested())
            {
                foreach(string helpline in ph.GetHelp())
                    this.Message(helpline);
            }
            else
            {

                var mh = new CsgenMvvm();
                if(!mh.SetFullModel(ph.ps))
                {
                    this.Message(mh.lastmessage);
                    return false;
                }

                if(!mh.CreateFile())
                {
                    this.Message(mh.lastmessage);
                    return false;
                }
            }
            //this.Message("Completed.");
            return true;
        }

        public void SetParameters(string[] args)
        {
            parameters.Clear();
            parameters.AddRange(args.ToList());
        }

        public void Message(string message) {Console.WriteLine(message);}
    }
}