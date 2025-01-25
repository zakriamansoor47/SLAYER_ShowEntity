using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Core.Translations;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Menu;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Drawing;
using Microsoft.Extensions.Logging;
using System.Runtime.InteropServices;
using System.Reflection;

namespace SLAYER_ShowEntity;
public class SLAYER_ShowEntityConfig : BasePluginConfig
{
    [JsonPropertyName("Tag")]public string Tag { get; set; } = "\x10[\x02★ \x04ShowEntity \x02★\x10]";
    [JsonPropertyName("PrintEntityInfoInChat")]public bool PrintEntityInfoInChat { get; set; } = true;
    [JsonPropertyName("PrintEntityInfoInConsole")]public bool PrintEntityInfoInConsole { get; set; } = true;
    [JsonPropertyName("PrintEntityInfoInLogFile")]public bool PrintEntityInfoInLogFile { get; set; } = true;
}
public partial class SLAYER_ShowEntity : BasePlugin, IPluginConfig<SLAYER_ShowEntityConfig>
{
    public override string ModuleName => "SLAYER_ShowEntity";
    public override string ModuleVersion => "1.0";
    public override string ModuleAuthor => "SLAYER";
    public override string ModuleDescription => "Print Entity on which player is aiming";
    public required SLAYER_ShowEntityConfig Config {get; set;}
    public void OnConfigParsed(SLAYER_ShowEntityConfig config)
    {
        // Assign the validated config to the global Config property
        Config = config;
    }
    public bool[] PlayerShowingEntity = new bool[64];
    public CBaseEntity?[] PlayerLookingAtEntity = new CBaseEntity[64];
    public CounterStrikeSharp.API.Modules.Timers.Timer?[] RayTraceTimer = new CounterStrikeSharp.API.Modules.Timers.Timer?[64];
    public override void Load(bool hotReload)
    {
        RegisterEventHandler((EventPlayerSpawn @event, GameEventInfo info)=>
        {
            var player = @event.Userid;
            if(player == null || !player.IsValid || player.IsBot|| player.TeamNum < 2)return HookResult.Continue;

            PlayerShowingEntity[player.Slot] = false;
            PlayerLookingAtEntity[player.Slot] = null;
            if(RayTraceTimer[player.Slot] != null)RayTraceTimer[player.Slot]?.Kill();

            return HookResult.Continue;
        });
    }

    [ConsoleCommand("show_ent", "Show Entity")]
	[RequiresPermissions("@css/root")]
	public void ShowEntityCMD(CCSPlayerController? player, CommandInfo command)
	{
        if (player == null || !player.IsValid || player.Connected != PlayerConnectedState.PlayerConnected || player.TeamNum < 2 || player.Pawn.Value!.LifeState != (byte)LifeState_t.LIFE_ALIVE)return;
        
        if(RayTraceTimer[player.Slot] != null)RayTraceTimer[player.Slot]?.Kill(); // kill old timer if exists

        if(!PlayerShowingEntity[player.Slot])
        {
            PlayerShowingEntity[player.Slot] = true;
            RayTraceTimer[player.Slot] = AddTimer(0.2f, ()=>
            {
                var playerPosition = CreateNewVector(player.PlayerPawn.Value!.AbsOrigin!);
                var Position = TraceShape(playerPosition, player.PlayerPawn.Value!.EyeAngles, true, true, 0.2f); // Creating Beam and Saving where position where player aiming
                var entity = GetClientAimTarget(player); // Getting entity at which player is aiming
                var Distance = entity != null ? System.Numerics.Vector3.Distance(ConvertVectorToVector3(Position), ConvertVectorToVector3(entity?.AbsOrigin!)) : 10000;

                if(entity != null && !entity.DesignerName.Contains("csgo_viewmodel") && PlayerLookingAtEntity[player.Slot] == null)
                {
                    CDynamicProp? glow = null;
                    if((entity.DesignerName.Contains("player") || entity.DesignerName.Contains("hostage")) && Distance > 10f && Distance <= 150f)
                    {
                        var foundplayer = GetPlayerFromIndex((int)entity.Index);
                        if(foundplayer == null || player == foundplayer)return;
                        //glow = SetGlowOnPlayer(foundplayer, Color.Green)[1];  // Have to fix this issue on player glow
                    }
                    else 
                    {
                        if(Distance <= 10f)glow = SetGlowOnEntity(entity.As<CDynamicProp>(), Color.Green);
                    }
                    if(glow != null && glow.IsValid)PlayerLookingAtEntity[player.Slot] = glow; // save glow entity
                    else PlayerLookingAtEntity[player.Slot] = null;
                    //player.PrintToChat($"you are looking at: {ChatColors.Green}{entity.DesignerName} | {entity.Index}");
                    
                }
                else if(entity == null || (Distance > 10 && Distance < 150))
                {
                    if(PlayerLookingAtEntity[player.Slot] != null && PlayerLookingAtEntity[player.Slot]!.IsValid)
                        PlayerLookingAtEntity[player.Slot]!.Remove(); // Delete Glow-entity from entity
                    else if(PlayerLookingAtEntity[player.Slot] != null && !PlayerLookingAtEntity[player.Slot]!.IsValid)PlayerLookingAtEntity[player.Slot] = null;
                }
                

            }, TimerFlags.REPEAT);
        }
        else PlayerShowingEntity[player.Slot] = false;

    }
    [ConsoleCommand("print_ent", "Print Entity Information")]
	[RequiresPermissions("@css/root")]
	public void PrintEntityCMD(CCSPlayerController? player, CommandInfo command)
	{
        if (player == null || !player.IsValid || player.Connected != PlayerConnectedState.PlayerConnected || player.TeamNum < 2 || player.Pawn.Value!.LifeState != (byte)LifeState_t.LIFE_ALIVE)return;


        var entity = GetClientAimTarget(player);
        if(entity == null) return;
        string Type = "";
        var entities = Utilities.GetAllEntities().ToList();
        foreach(var ent in entities.Where(e => e != null))
        {
            if(ent.Index == entity.Index)
            {
                Type = $"{ent}";
            }
        }
        string fileNameWithExtension = entity.CBodyComponent!.SceneNode!.GetSkeletonInstance().ModelState.ModelName.Substring(entity.CBodyComponent!.SceneNode!.GetSkeletonInstance().ModelState.ModelName.LastIndexOf('/') + 1);
        string fileNameWithoutExtension = fileNameWithExtension.Substring(0, fileNameWithExtension.LastIndexOf('.'));

        Console.WriteLine(fileNameWithoutExtension);
        
        if(Config.PrintEntityInfoInChat)
        {
            player.PrintToChat($" {ChatColors.DarkRed}-----------------{StringExtensions.ReplaceColorTags(Config.Tag)}{ChatColors.DarkRed}-----------------");
            player.PrintToChat($" {ChatColors.DarkRed}Entity Name: {ChatColors.Green}{fileNameWithoutExtension}");
            player.PrintToChat($" {ChatColors.DarkRed}Entity Type: {ChatColors.Green}{entity.DesignerName}");
            player.PrintToChat($" {ChatColors.DarkRed}Entity Index: {ChatColors.Green}{entity.Index}");
            player.PrintToChat($" {ChatColors.DarkRed}Entity Model: {ChatColors.Green}{entity.CBodyComponent!.SceneNode!.GetSkeletonInstance().ModelState.ModelName}");
            player.PrintToChat($" {ChatColors.DarkRed}Entity Position: {ChatColors.Green}{entity.AbsOrigin}");
            player.PrintToChat($" {ChatColors.DarkRed}Entity Rotation: {ChatColors.Green}{entity.AbsRotation}");
            player.PrintToChat($" {ChatColors.DarkRed}----------------------------------");
        }
        if(Config.PrintEntityInfoInConsole)
        {
            Console.WriteLine($"----------------------------------");
            Console.WriteLine($"Entity Name: {fileNameWithoutExtension}");
            Console.WriteLine($"Entity Type: {entity.DesignerName}");
            Console.WriteLine($"Entity Index: {entity.Index}");
            Console.WriteLine($"Entity Model: {entity.CBodyComponent!.SceneNode!.GetSkeletonInstance().ModelState.ModelName}");
            Console.WriteLine($"Entity Position: {entity.AbsOrigin}");
            Console.WriteLine($"Entity Rotation: {entity.AbsRotation}");
            Console.WriteLine($"----------------------------------");
        }
        if(Config.PrintEntityInfoInLogFile)
        {
            Logger.LogInformation($"----------------------------------");
            Logger.LogInformation($"Entity Name: {fileNameWithoutExtension}");
            Logger.LogInformation($"Entity Type: {entity.DesignerName}");
            Logger.LogInformation($"Entity Index: {entity.Index}");
            Logger.LogInformation($"Entity Model: {entity.CBodyComponent!.SceneNode!.GetSkeletonInstance().ModelState.ModelName}");
            Logger.LogInformation($"Entity Position: {entity.AbsOrigin}");
            Logger.LogInformation($"Entity Rotation: {entity.AbsRotation}");
            Logger.LogInformation($"----------------------------------");
        }
    }
}