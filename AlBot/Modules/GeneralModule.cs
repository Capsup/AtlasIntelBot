using Discord.Commands;
using Hangfire;
using SteamServerQuery.SteamServerQuery;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using IntelBot.Utils;

namespace IntelBot.Modules
{
    [Group( "general" )]
    public class GeneralModule : ModuleBase<SocketCommandContext>
    {
        private readonly CommandService _service;

        public GeneralModule( CommandService service )
        {
            _service = service;


        }

        [Command(), Summary( "" )]
        public async Task AddGridAsync( string ip, string grid )
        {
            //RecurringJob.AddOrUpdate( grid, () => ScheduleUtils.BackgroundFuncJob.ScheduleGridUpdate( ip, grid ), Cron.Minutely );

            await ReplyAsync( $"Added grid with name {grid} to the IntelBot" );

            return;
        }
    }
}
