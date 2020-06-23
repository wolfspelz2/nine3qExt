using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Orleans;

namespace n3q.Web
{
    public interface ICommandline
    {
        HttpContext HttpContext { get; set; }
        Commandline.User ActiveUser { get; set; }
        List<string> AdminTokens { get; set; }

        Commandline.HandlerMap GetHandlers();
        string Run(string script);
        string CheckRole(Commandline.Handler handler, Commandline.ICommandlineUser user);
    }
}