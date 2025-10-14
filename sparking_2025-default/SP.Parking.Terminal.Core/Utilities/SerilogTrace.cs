//using PostSharp.Aspects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog;
//using PostSharp.Serialization;

namespace SP.Parking.Terminal.Core.Utilities
{

    //[PSerializable]
    //public class SerilogTraceAttribute : OnMethodBoundaryAspect
    //{
    //    public static Serilog.ILogger Logger = Log.Logger;

    //    /// <summary>  
    //    /// On Method Entry  
    //    /// </summary>  
    //    /// <param name="args"></param>  
    //    public override void OnEntry(MethodExecutionArgs args)
    //    {
    //        //Logger.Information($"OnEntry : {(args.Method != null ? args.Method.Name : "")}");

    //        //string className = "", methodName = "", arguments = "";
    //        //if (args.Method != null)
    //        //    if (args.Method.DeclaringType != null)
    //        //        className = $"{args.Method.DeclaringType.Namespace}.{args.Method.DeclaringType.Name}";
    //        //if (args.Method != null)
    //        //{
    //        //    methodName = args.Method.Name;
    //        //    arguments = args.Arguments.ToString();
    //        //}

    //        //Logger.Information($"className: {className}; methodName:{methodName};arguments:{arguments}");
    //    }

    //    /// <summary>  
    //    /// On Method success  
    //    /// </summary>  
    //    /// <param name="args"></param>  
    //    public override void OnSuccess(MethodExecutionArgs args)
    //    {
    //        //Logger.Information($"OnSuccess : {(args.Method != null ? args.Method.Name : "")}");
    //        //var returnValue = args.ReturnValue;
    //        //Logger.Information($"ReturnValue : {returnValue}");
    //    }


    //    /// <summary>  
    //    /// On Method Exception  
    //    /// </summary>  
    //    /// <param name="args"></param>  
    //    public override void OnException(MethodExecutionArgs args)
    //    {
    //        //if (args.Exception != null)
    //        //    Logger.Information($"OnException : {(!string.IsNullOrEmpty(args.Exception.Message) ? args.Exception.Message : "")}");


    //        //var Message = args.Exception.Message;
    //        //var StackTrace = args.Exception.StackTrace;

    //        //Logger.Information($"Application has got exception in method-{args.Method.Name} and message is {Message}");

    //        // or you can send email notification         
    //    }

    //    /// <summary>  
    //    /// On Method Exit  
    //    /// </summary>  
    //    /// <param name="args"></param>  
    //    public override void OnExit(MethodExecutionArgs args)
    //    {
    //    }
    //}
}
