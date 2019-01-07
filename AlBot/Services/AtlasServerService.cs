using Discord;
using Discord.WebSocket;
using SteamServerQuery.SteamServerQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace IntelBot.Services
{
    public enum PlayerState : int
    {
        Unknown = 0,
        LeftGridByBoat = 1,
        LeftGridByBed = 2,
        LeftGridByDisconnect = 3,
        StillInGrid,
        EnteredGridByBoat,
        EnteredGridByBed,
        EnteredGridByConnect
    }

    public enum PlayerDirection : int
    {
        Unknown = 0,
        North = 1,
        Northeast = 2,
        East = 3,
        SouthEast,
        South,
        SouthWest,
        West,
        NorthWest
    }

    public class AtlasServerService
    {
        private Dictionary<string, IPEndPoint> atlasServers = new Dictionary<string, IPEndPoint>();
        private Dictionary<Tuple<string, int>, string> playerGridMap = new Dictionary<Tuple<string, int>, string>();
        private Dictionary<string, A2S_PLAYER> gridResultMap = new Dictionary<string, A2S_PLAYER>();
        private Dictionary<string, Dictionary<string, bool>> playerIgnoreMap = new Dictionary<string, Dictionary<string, bool>>();
        private string grid = "G5";

        private DiscordSocketClient _discord;

        public AtlasServerService( DiscordSocketClient discord )
        {
            atlasServers.Add( "G5", new IPEndPoint( IPAddress.Parse( "46.251.238.90" ), 57557 ) );
            atlasServers.Add( "G6", new IPEndPoint( IPAddress.Parse( "46.251.238.90" ), 57559 ) );
            atlasServers.Add( "G4", new IPEndPoint( IPAddress.Parse( "46.251.238.90" ), 57555 ) );
            atlasServers.Add( "F5", new IPEndPoint( IPAddress.Parse( "46.251.238.85" ), 57557 ) );
            atlasServers.Add( "F6", new IPEndPoint( IPAddress.Parse( "46.251.238.85" ), 57559 ) );
            atlasServers.Add( "F4", new IPEndPoint( IPAddress.Parse( "46.251.238.85" ), 57561 ) );
            atlasServers.Add( "H5", new IPEndPoint( IPAddress.Parse( "46.251.238.95" ), 57557 ) );
            atlasServers.Add( "H6", new IPEndPoint( IPAddress.Parse( "46.251.238.95" ), 57559 ) );
            atlasServers.Add( "H4", new IPEndPoint( IPAddress.Parse( "46.251.238.95" ), 57555 ) );

            this._discord = discord;
        }

        public Task StartService()
        {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            var task = Task.Run( async () =>
            {
                while( true )
                {
                    var newPlayerGridMap = new Dictionary<Tuple<string, int>, string>();
                    var newGridResultMap = new Dictionary<string, A2S_PLAYER>();
                    foreach( var server in atlasServers )
                    {
                        var playerQuery = new A2S_PLAYER( server.Value );
                        if( playerQuery.Players != null )
                        {
                            var uniquePlayers = playerQuery.Players.Where( x => !string.IsNullOrEmpty( x.Name ) );
                            playerQuery.Players = uniquePlayers.ToArray();
                            newGridResultMap.Add( server.Key, playerQuery );

                            foreach( var player in uniquePlayers )
                            {
                                newPlayerGridMap.TryAdd( new Tuple<string, int>( player.Name, player.Duration ), server.Key );
                            }
                        }
                    }

                    if( gridResultMap.Count <= 0 )
                    {
                        var foundGroups = newPlayerGridMap.GroupBy( x => new { x.Key.Item2, x.Value } );
                        var cou = foundGroups.Count();
                        var i = 0;
                        foreach( var groups in foundGroups )
                        {
                            foreach( var player in groups )
                            {
                                var dict = new Dictionary<string, bool>();

                                foreach( var player2 in groups )
                                {
                                    if( player2.Key == player.Key )
                                        continue;

                                    dict.Add( player2.Key.Item1, true );
                                }

                                this.playerIgnoreMap.TryAdd( player.Key.Item1, dict );
                            }
                            Console.WriteLine( ++i + " / "  + cou );
                        }
                        this.gridResultMap = newGridResultMap;
                        this.playerGridMap = newPlayerGridMap;
                        continue;
                    }

                    var gridResult = gridResultMap[ grid ];
                    var players = newGridResultMap[ grid ].Players;

                    var services = Program.Services;
#if DEBUG
                    var channel = _discord.GetGuild( 209247707951267840 ).TextChannels.First( x => x.Id == 378275172966465536 );
#else
                var channel = discord.GetGuild( 188763107584114688 ).TextChannels.First( x => x.Id == 503248406597206016 );
#endif

                    var builder = new EmbedBuilder();
                    builder.WithTitle( $"Grid population of {grid}" );


                    var timeoutTest = 119;
                    var changedPlayers = players.Select( x =>
                    {
                        //var searchingFor = gridResult.Players.Select( y => Tuple.Create( y.Name, y.Duration ) );
                        string result = "";
                        for( int i = 0; i < timeoutTest; i++ )
                        {
                            var searchingFor = Tuple.Create( x.Name, x.Duration + i );

                            playerGridMap.TryGetValue( searchingFor, out result );
                        }

                        PlayerState state = PlayerState.Unknown;
                        char[] split = result.ToCharArray();
                        char[] gridSplit = grid.ToCharArray();
                        if( string.IsNullOrEmpty( result ) )
                            state = PlayerState.Unknown;
                        /*else if( split[ 0 ] != grid[ 0 ] || split[ 1 ] != grid[ 1 ] )
                        {

                        }*/

                        return new
                        {
                            Player = x,
                            Grid = result,
                            State = state
                        };
                    }
                    );

                    var newPlayerCount = gridResult.Players.Count() - players.Count();
                    var boats = players.GroupBy( x => x.Duration ).Where( x => x.Count() >= 3 ).Where( x =>
                    {
                        /*bool res = true;
                        for( int i = 0; i < timeoutTest; i++ )
                        {
                            var searchingFor = Tuple.Create( x.Name, x.Duration + i );

                            if( playerIgnoreMap.TryGetValue( searchingFor, out res ) )
                                return false;
                        }*/

                        /*for( int i = 0; i < x.Count(); i++ )
                        {
                            var player = x.ElementAt( i );
                            Dictionary<string, bool> playerDict;

                            if( !playerIgnoreMap.TryGetValue( player.Name, out playerDict ) )
                                return true;

                            for( int j = 0; j < x.Count(); j++ )
                            {
                                if( i == j )
                                    continue;

                                var otherPlayer = x.ElementAt( j );

                                bool result = false;
                                playerDict.TryGetValue( otherPlayer.Name, out result );
                                if( result )
                                   return false;
                            }
                        }*/

                        return true;
                    } );

                    builder.WithDescription( $"**Players ({players.Count()}/150)**" +
                        $"\n" +
                        $"{string.Join( "\n", players.Select( x => x.Name ) )}" +
                        $"\n\n" +
                        $"**Boats in the area: {boats.Count()}**" +
                        $"\n" +
                        $"{string.Join( "\n\n", boats.Select( (x, i) => "__Boat " + (i + 1) + "__\n" + string.Join( "\n", x.Select( y => y.Name ) ) ) )}" +
                        $"\n\n" +
                        $"**Changed players since last update: {newPlayerCount}**" +
                        $"\n" +
                        $"{string.Join( "\n", gridResult.Players.Select( x => x.Name ).Except( players.Select( x => x.Name ) ) )}"
                        );

                    await channel.SendMessageAsync( "", embed: builder.Build() );

                    this.gridResultMap = newGridResultMap;
                    this.playerGridMap = newPlayerGridMap;

                    await Task.Delay( TimeSpan.FromSeconds( 60 ) );
                }
            } );

            return Task.CompletedTask;
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }

        /*public static async Task ScheduleGridUpdate()
        {
            if( string.IsNullOrEmpty( ip ) )
                throw new ArgumentException( "ERROR! ip cannot be null or empty" );

            if( string.IsNullOrEmpty( grid ) )
                throw new ArgumentException( "ERROR! grid cannot be null or empty" );

            var splitIP = ip.Split( ":" );

            var playerQuery = new A2S_PLAYER( new IPEndPoint( IPAddress.Parse( splitIP[ 0 ] ), int.Parse( splitIP[ 1 ] ) ) );
            var uniquePlayers = playerQuery.Players.Where( x => !string.IsNullOrEmpty( x.Name ) );
            playerQuery.Players = uniquePlayers.ToArray();

            var newPlayerGridMap = new Dictionary<Tuple<string, int>, string>();
            foreach( var player in uniquePlayers )
            {

            }

            var services = Program.Services;
            var discord = services.GetRequiredService<DiscordSocketClient>();
#if DEBUG
            var channel = discord.GetGuild( 209247707951267840 ).TextChannels.First( x => x.Id == 378275172966465536 );
#else
                var channel = discord.GetGuild( 188763107584114688 ).TextChannels.First( x => x.Id == 503248406597206016 );
#endif

            var builder = new EmbedBuilder();
            builder.WithTitle( $"Grid population of {grid}" );

            builder.WithDescription( $"**Players ({uniquePlayers.Count()}/150**" +
                $"{string.Concat( uniquePlayers.Select( x => x.Name ), "\n" )}" +
                $"\n" +
                $"**New since last update: {uniquePlayers.Count() - activeQueries[ grid ].Players.Count()}"
                );

            await channel.SendMessageAsync( "", embed: builder.Build() );
            activeQueries[ grid ] = playerQuery;
        }*/
    }
}
