using System;
using Microsoft.AspNetCore.Http;

namespace n3q.Web
{
    public interface ICommandline
    {
        HttpContext HttpContext { get; set; }

        Commandline.HandlerMap GetHandlers();
        string Run(string script, Commandline.ICommandlineUser user);
        string CheckRole(Commandline.Handler handler, Commandline.ICommandlineUser user);
    }
}