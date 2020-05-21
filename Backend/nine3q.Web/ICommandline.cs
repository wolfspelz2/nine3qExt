using System;

namespace nine3q.Web
{
    public interface ICommandline
    {
        Guid CommandlineId { get; }

        Commandline.HandlerMap GetHandlers();
        string Run(string script, Commandline.ICommandlineUser user);
        string CheckRole(Commandline.Handler handler, Commandline.ICommandlineUser user);
    }

    public interface ICommandlineSingletonInstance : ICommandline
    {
    }
}