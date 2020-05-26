using System;
using Microsoft.AspNetCore.Http;

namespace n3q.Web
{
    public interface ICommandline
    {
        Commandline.HandlerMap GetHandlers();
        //Commandline.Runner NewRunner(HttpContext httpContext);
        string Run(string script, Commandline.ICommandlineUser user);
        string CheckRole(Commandline.Handler handler, Commandline.ICommandlineUser user);
    }

    public interface ICommandlineSingletonInstance : ICommandline
    {
    }
}