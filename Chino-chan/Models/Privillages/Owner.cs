using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace Chino_chan.Models.Privillages
{
    public class Owner : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissions(ICommandContext Context, CommandInfo command, IServiceProvider services)
        {
            if (Global.IsOwner(Context.User.Id))
            {
                return Task.Run(() => PreconditionResult.FromSuccess());
            }
            return Task.Run(() => PreconditionResult.FromError("Owner"));
        }
    }
}
