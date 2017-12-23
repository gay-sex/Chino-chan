using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chino_chan.Models.Privillages
{
    public class ServerOwner : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissions(ICommandContext Context, CommandInfo command, IServiceProvider services)
        {
            if (Global.IsServerOwnerOrHigher(Context.User.Id, Context.Guild))
            {
                return Task.Run(() => PreconditionResult.FromSuccess());
            }
            return Task.Run(() => PreconditionResult.FromError("ServerOwner"));
        }
    }
}
