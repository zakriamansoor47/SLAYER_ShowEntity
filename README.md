# SLAYER_ShowEntity
Print Entity on which player is aiming

## This Information will be printed in Chat, Console, and Logs (Can be enable/disable from Config file)
```c#
Console.WriteLine($"----------------------------------");
Console.WriteLine($"Entity Name: {fileNameWithoutExtension}");
Console.WriteLine($"Entity Type: {entity.DesignerName}");
Console.WriteLine($"Entity Index: {entity.Index}");
Console.WriteLine($"Entity Model: {entity.CBodyComponent!.SceneNode!.GetSkeletonInstance().ModelState.ModelName}");
Console.WriteLine($"Entity Position: {entity.AbsOrigin}");
Console.WriteLine($"Entity Rotation: {entity.AbsRotation}");
Console.WriteLine($"----------------------------------");
```
## Commands (Only For Root Admin)
```
!show_ent - Glow entity when you Aim on it (Not all entities)
!print_ent - Print the Information of entity in chat
```
