using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace Chino_chan.Models.Privillages
{
    public class Owner : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext Context, CommandInfo Command, IServiceProvider Services)
        {
            if (Global.IsOwner(Context.User.Id))
            {
                return Task.Run(() => PreconditionResult.FromSuccess());
            }
            return Task.Run(() => PreconditionResult.FromError("Owner"));
        }
    }
}
