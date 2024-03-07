using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Modules.Entities;
using Timer = CounterStrikeSharp.API.Modules.Timers.Timer;
using CounterStrikeSharp.API;

namespace HighPingKicker;
[MinimumApiVersion(189)]

public class HighPingKickerPlugin : BasePlugin, IPluginConfig<HighPingKickerConfig>
{
    public override string ModuleName => "High Ping Kicker";
    public override string ModuleVersion => "0.0.6";
    public override string ModuleAuthor => "conch";
    public override string ModuleDescription => "Kicks users with high ping";

    public HighPingKickerConfig Config { get; set; } = new();
    public Dictionary<int, PlayerInfo> Slots = new();
    public class PlayerInfo
    {
        public Timer? Timer { get; set; }
        public bool IsInGracePeriod { get; set; } = true;
        public bool IsAdmin { get; set; } = false;
        public int WarningsGiven { get; set; } = 0;
        public bool IsImmune
        {
            get => this.IsAdmin || this.IsInGracePeriod;
        }
    }

    public void OnConfigParsed(HighPingKickerConfig config)
    {
        Config = config;
    }

    public override void Load(bool hotReload)
    {
        if (hotReload) GetPlayers().ForEach(Reset);

        AddTimer(Config.CheckInterval, CheckPings, TimerFlags.REPEAT | TimerFlags.STOP_ON_MAPCHANGE);

        RegisterListener<Listeners.OnMapStart>(OnMapStartHandler);
    }
    private List<CCSPlayerController> GetPlayers()
    {
        return Utilities.GetPlayers().FindAll(p => p is { 
            IsValid: true, 
            IsBot: false, 
            IsHLTV: false, 
            Connected: PlayerConnectedState.PlayerConnected 
        });
    }

    private void OnMapStartHandler(string mapName)
    {
        // server grace period start
        AddTimer(Config.GracePeriod, () =>
        {
            // server grace period end
            AddTimer(Config.CheckInterval, CheckPings, TimerFlags.REPEAT | TimerFlags.STOP_ON_MAPCHANGE);
        });
    }
    public override void Unload(bool hotReload)
    {
        foreach (var slot in Slots)
        {
            slot.Value?.Timer?.Kill();
        }
    }

    [GameEventHandler]
    public HookResult OnPlayerConnectFull(EventPlayerConnectFull @event, GameEventInfo info)
    {
        Reset(@event.Userid);
        return HookResult.Continue;
    }

    public void Reset(CCSPlayerController player)
    {
        if (!Slots.TryGetValue(player.Slot, out PlayerInfo? playerInfo))
        {
            playerInfo = new();
            Slots.Add(player.Slot, playerInfo);
        }
        playerInfo.IsInGracePeriod = true;
        playerInfo.WarningsGiven = 0;
        playerInfo.Timer?.Kill();
        playerInfo.Timer = new Timer(Config.GracePeriod, () => playerInfo.IsInGracePeriod = false);
        var adminManager = AdminManager.GetPlayerAdminData(new SteamID(player.SteamID));
        playerInfo.IsAdmin = (adminManager?.Groups.Count ?? 0) + (adminManager?.Flags.Count ?? 0) + AdminManager.GetPlayerImmunity(player) > 0;
    }

    private void CheckPings()
    {
        if (Config.DevMode)
        {
            Logger.LogInformation("-------------------------------");
            Logger.LogInformation("Checking player's pings");
            Logger.LogInformation("-------------------------------");
        }
        GetPlayers().ForEach(CheckPing);
        if (Config.DevMode)
        { 
            Logger.LogInformation("-------------------------------");
        }
    }

    private void CheckPing(CCSPlayerController player)
    { 
        if (Config.DevMode)
            Logger.LogInformation("Name: {name}, Ping: {ping}, SteamID: {steamid}, Slot: {slot}", player.PlayerName, player.Ping, player.SteamID, player.Slot);

        if (!Slots.TryGetValue(player.Slot, out var playerInfo))
        {
            if (Config.DevMode)
            {
                Logger.LogError("Player {player} ({steamid}) PlayerInfo slot not found.", player.PlayerName, player.SteamID);
                Logger.LogInformation("Existing PlayerInfo slots...");
                foreach (var slot in Slots)
                {
                    Logger.LogInformation("      {slot}. ", slot.Key);
                }
            }
            return;
        }

        if (playerInfo.IsImmune)
        { 
            return;
        }

        if (player.Ping > Config.MaxPing)
            HandleExcessivePing(player, playerInfo);

    }
    
    public void HandleExcessivePing(CCSPlayerController player, PlayerInfo playerInfo)
    { 
        playerInfo.WarningsGiven++; 
        if (playerInfo.WarningsGiven > Config.MaxWarnings)
        {
            Server.ExecuteCommand($"kickid {player.UserId}");
            if (Config.ShowPublicKickMessage)
            {
                var kickMessage = ParseMessageTemplate(player, playerInfo, Config.KickMessage); 
                Server.PrintToChatAll(kickMessage);
            }
        } else
        { 
            if (Config.ShowWarnings)
            {
                var warningMessage = ParseMessageTemplate(player, playerInfo, Config.WarningMessage);
                player.PrintToChat(warningMessage);
            }
        }
    }

    public string ParseMessageTemplate(CCSPlayerController player, PlayerInfo playerInfo,  string message)
    {
        return message
            .Replace("{NAME}", player.PlayerName)
            .Replace("{WARN}", playerInfo.WarningsGiven.ToString())
            .Replace("{MAXWARN}", Config.MaxWarnings.ToString())
            .Replace("{PING}", player.Ping.ToString())
            .Replace("{MAXPING}", Config.MaxPing.ToString());
    }
}

public class HighPingKickerConfig : BasePluginConfig
{ 
    [JsonPropertyName("max_ping")] public int MaxPing { get; set; } = 150;
    [JsonPropertyName("max_warnings")] public int MaxWarnings { get; set; } = 5;
    [JsonPropertyName("check_interval")] public float CheckInterval { get; set; } = 20;
    [JsonPropertyName("show_warnings")] public bool ShowWarnings { get; set; } = true; 
    [JsonPropertyName("show_public_kick_message")] public bool ShowPublicKickMessage { get; set; } = true; 
    [JsonPropertyName("warning_message")] public string WarningMessage { get; set; } = "You will be kicked for excessive ping. You have {WARN} out of {MAXWARN} warnings.";
    [JsonPropertyName("kick_message")] public string KickMessage { get; set; } = "{NAME} has been kicked due to excessive ping.";
    [JsonPropertyName("grace_period_seconds")] public float GracePeriod { get; set; } = 90;
    [JsonPropertyName("dev")] public bool DevMode { get; set; } = false;
}