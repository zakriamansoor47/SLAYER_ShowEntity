using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Cvars;
using System.Drawing;
using Microsoft.Extensions.Logging;
using System.Runtime.InteropServices;

// Used these to remove compile warnings
#pragma warning disable CS8600
#pragma warning disable CS8602
#pragma warning disable CS8603
#pragma warning disable CS8604
#pragma warning disable CS8619

namespace SLAYER_ShowEntity;

public partial class SLAYER_ShowEntity : BasePlugin
{
    // ---------------------------------------
    // Useful Funtions
    // ---------------------------------------
    public CBeam DrawLaserBetween(Vector startPos, Vector endPos, Color color, float life, float width)
    {
        if (startPos == null || endPos == null)
            return null;

        CBeam beam = Utilities.CreateEntityByName<CBeam>("beam");

        if (beam == null)
        {
            Logger.LogError($"Failed to create beam...");
            return null;
        }

        beam.Render = color;
        beam.Width = width;

        beam.Teleport(startPos, QAngle.Zero, Vector.Zero);
        beam.EndPos.X = endPos.X;
        beam.EndPos.Y = endPos.Y;
        beam.EndPos.Z = endPos.Z;
        beam.DispatchSpawn();

        if(life != -1) AddTimer(life, () => {if(beam != null && beam.IsValid) beam.Remove(); }); // destroy beam after specific time

        return beam;
    }
    private void DeleteLaserBeams(List<CBeam> LaserBeams)
    {
        foreach (var beam in LaserBeams)
        {
            if (beam != null && beam.IsValid)
            {
                beam.Remove();
            }
        }
        LaserBeams.Clear(); // Clear the list after deleting
    }
    public static CCSGameRules? GetGameRules()
    {
        var rules = Utilities.FindAllEntitiesByDesignerName<CCSGameRulesProxy>("cs_gamerules").First().GameRules!;
		if(rules == null)return null;
		else return rules;
    }
    
    public static CDynamicProp SetGlowOnEntity(CDynamicProp? entity, Color GlowColor, int GlowRangeMin = 0, int GlowRnageMax = 5000, int teamNum = -1)
    {
        if (entity == null || !entity.IsValid)return null;

        CDynamicProp Glow;
        if(entity.DesignerName.Contains($"weapon_"))Glow = Utilities.CreateEntityByName<CDynamicProp>("prop_physics")!;
        else Glow = Utilities.CreateEntityByName<CDynamicProp>("prop_dynamic")!;

        Glow.Spawnflags = 256;
        Glow.Render = Color.Transparent;

        Glow.CBodyComponent!.SceneNode!.Owner!.Entity!.Flags = (uint)(Glow.CBodyComponent!.SceneNode!.Owner!.Entity!.Flags & ~(1 << 2));
        Glow.SetModel(entity.CBodyComponent!.SceneNode!.GetSkeletonInstance().ModelState.ModelName);
        Glow.DispatchSpawn();
        Glow.Teleport(entity.AbsOrigin, entity.AbsRotation, entity.AbsVelocity);

        Glow.Glow.GlowColorOverride = GlowColor;
        Glow.Glow.GlowRange = GlowRnageMax;
        Glow.Glow.GlowRangeMin = GlowRangeMin;
        Glow.Glow.GlowTeam = teamNum;
        Glow.Glow.GlowType = 3;

        
        Glow.AcceptInput("SetParent", entity, Glow, "!activator");
        return Glow;
    }
    public CCSPlayerController? GetPlayerFromIndex(int index)
    {
        foreach(var player in Utilities.GetPlayers().Where(p => p != null && p.IsValid && !p.IsHLTV))
        {
            if(player.PlayerPawn.Value.Index == index)return player;
        }
        return null;
    }
    public List<CDynamicProp> SetGlowOnPlayer(CCSPlayerController player, Color color)
    {
        CDynamicProp? modelGlow = Utilities.CreateEntityByName<CDynamicProp>("prop_dynamic");
        CDynamicProp? modelRelay = Utilities.CreateEntityByName<CDynamicProp>("prop_dynamic");
        if (modelGlow == null || modelRelay == null)
        {
            return null;
        }

        string modelName = player.PlayerPawn.Value.CBodyComponent!.SceneNode!.GetSkeletonInstance().ModelState.ModelName;

        modelRelay.SetModel(modelName);
        modelRelay.Spawnflags = 256u;
        modelRelay.RenderMode = RenderMode_t.kRenderNone;
        modelRelay.DispatchSpawn();

        modelGlow.SetModel(modelName);
        modelGlow.Spawnflags = 256u;
        modelGlow.DispatchSpawn();

        modelGlow.Glow.GlowColorOverride = color;
        modelGlow.Glow.GlowRange = 5000;
        modelGlow.Glow.GlowTeam = -1;
        modelGlow.Glow.GlowType = 3;
        modelGlow.Glow.GlowRangeMin = 100;

        modelRelay.AcceptInput("FollowEntity", player.PlayerPawn.Value, modelRelay, "!activator");
        modelGlow.AcceptInput("FollowEntity", modelRelay, modelGlow, "!activator");

        var models = new List<CDynamicProp>();
        models.Add(modelGlow);
        models.Add(modelRelay);

        return models;
    }
	private QAngle ConvertVectorToQAngle(Vector vector)
    {
        if(vector == null)return null;
        return new QAngle(vector.X, vector.Y, vector.Z);
    }
	private Vector ConvertQAngleToVector(QAngle qAngle)
    {
        if(qAngle == null)return null;
        return new Vector(qAngle.X, qAngle.Y, qAngle.Z);
    }
	private Vector ConvertVector3ToVector(System.Numerics.Vector3 vector)
    {
        return new Vector(vector.X, vector.Y, vector.Z);
    }
	private System.Numerics.Vector3 ConvertVectorToVector3(Vector vector)
    {
        return new System.Numerics.Vector3(vector.X, vector.Y, vector.Z);
    }
	private Vector CreateNewVector(Vector vector)
    {
        if(vector == null)return null;
        return new Vector(vector.X, vector.Y, vector.Z);
    }
    private static string ConvertVectorToString(Vector vector)
    {
        if(vector == null)return null;
        return $"{vector.X} {vector.Y} {vector.Z}";
    }
    private Vector ConvertStringToVector(string vectorString)
    {
        if (string.IsNullOrWhiteSpace(vectorString))
            return null;
        
        // Remove commas and " from the string
        vectorString = vectorString.Replace(",", "");
        vectorString = vectorString.Replace("\"", "");

        // Split the string by spaces
        string[] components = vectorString.Split(' ');

        if (components.Length != 3)
            return null; // Return null or handle error if not exactly 3 components

        // Try to parse each component to float
        if (float.TryParse(components[0], out float x) &&
            float.TryParse(components[1], out float y) &&
            float.TryParse(components[2], out float z))
        {
            return new Vector(x, y, z);
        }

        return null; // Return null or handle parsing error
    }
    private QAngle ConvertStringToQAngle(string vectorString)
    {
        if (string.IsNullOrWhiteSpace(vectorString))
            return null;
        
        // Remove commas and " from the string
        vectorString = vectorString.Replace(",", "");
        vectorString = vectorString.Replace("\"", "");

        // Split the string by spaces
        string[] components = vectorString.Split(' ');

        if (components.Length != 3)
            return null; // Return null or handle error if not exactly 3 components

        // Try to parse each component to float
        if (float.TryParse(components[0], out float x) &&
            float.TryParse(components[1], out float y) &&
            float.TryParse(components[2], out float z))
        {
            return new QAngle(x, y, z);
        }

        return null; // Return null or handle parsing error
    }
    public static CBaseEntity? GetClientAimTarget(CCSPlayerController player)
    {
        var GameRules = Utilities.FindAllEntitiesByDesignerName<CCSGameRulesProxy>("cs_gamerules").FirstOrDefault()?.GameRules;

        if (GameRules is null)
            return null;

        VirtualFunctionWithReturn<IntPtr, IntPtr, IntPtr> findPickerEntity = new(GameRules.Handle, 27);

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) findPickerEntity = new(GameRules.Handle, 28);

        var target = new CBaseEntity(findPickerEntity.Invoke(GameRules.Handle, player.Handle));
        if(target != null && target.IsValid)return target;

        return null;
    }
   
    public QAngle CalculateAngle(Vector origin1, Vector origin2)
    {
        if(origin1 == null || origin2 == null)return null;
        // Calculate the direction vector from origin1 to origin2
        Vector direction = new Vector(
            origin2.X - origin1.X,
            origin2.Y - origin1.Y,
            origin2.Z - origin1.Z
        );

        // Calculate the yaw angle
        float yaw = (float)(Math.Atan2(direction.Y, direction.X) * (180.0 / Math.PI));

        // Calculate the pitch angle
        float hypotenuse = (float)Math.Sqrt(direction.X * direction.X + direction.Y * direction.Y);
        float pitch = (float)(Math.Atan2(-direction.Z, hypotenuse) * (180.0 / Math.PI));

        // Create and return the QAngle with the calculated pitch and yaw
        return new QAngle(pitch, yaw, 0); // Roll is usually set to 0
    }
   
}
