using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SqlServer.Dts.Runtime;
using System.Diagnostics;
using GS.Utilities.Logger.Interfaces;
using GS.Utilities.Logger;

namespace GS.CDR.Utilities
{
    public static class PackageExecutor
    {
        private static ILogger logger = ClientLoggerFactory.GetLogger<GS.Utilities.Logger.TextLogger>();
        public static DtsExecutionResult RunPackage(string path, Dictionary<string, Object> parameters)
        {
            DtsExecutionResult result = null;
            Microsoft.SqlServer.Dts.Runtime.Application app = new Microsoft.SqlServer.Dts.Runtime.Application();
            Microsoft.SqlServer.Dts.Runtime.Package package = null;
            package = app.LoadPackage(path, null, false);
            var x = package.Variables;
            foreach (var item in x)
            {
                string p = item.Name;
                //Debug.Print(p.ToString());

                if (parameters.ContainsKey(p))
                {
                    item.Value = parameters[item.Name];
                }
            }
            Microsoft.SqlServer.Dts.Runtime.DTSExecResult results = package.Execute();
            if (results == DTSExecResult.Failure)
            {
                foreach (var item in package.Errors)
                {
                    string s = string.Format("{0}{1}{2}{1}{3}{1}{4}", item.Description, Environment.NewLine, item.ErrorCode, item.Source, item.SubComponent);
                    Exception ex = new Exception(s);
                    logger.Log(ex);
                }
            }
            result = new DtsExecutionResult(results);
            foreach (var item in package.Variables)
            {
                if (!item.SystemVariable)
                    result.Add(item.Name, item.Value);
            }
            return result;
        }
    }

    public class DtsExecutionResult
    {
        private Dictionary<string, Object> outputVariables;

        public Dictionary<string, Object> OutputVariables
        {
            get
            {
                if (outputVariables == null)
                    outputVariables = new Dictionary<string, object>();
                return outputVariables;
            }
            set { outputVariables = value; }
        }
        public DTSExecResult Result { get; set; }

        public object this[string name]
        {
            get
            {
                if (OutputVariables.ContainsKey(name))
                    return OutputVariables[name];
                return null;
            }

        }

        public DtsExecutionResult(DTSExecResult result)
        {
            this.Result = result;
        }



        public void Add(string name, object value)
        {
            OutputVariables.Add(name, value);
        }

    }
}
