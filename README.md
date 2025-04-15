![](https://img.shields.io/github/downloads/zakriamansoor47/SLAYER_ShowEntity/total?style=for-the-badge)

# SLAYER_ShowEntity
Print the Entity Info on which the player is aiming.

## This Information will be printed in Chat, Console, and Logs (Can be enabled/disable from the Config file)
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
```c#
!show_ent - Glow entity when you Aim at it (Not all entities)
!print_ent - Print the Information of the entity in chat
!move_ent <x> <y> <z> OR !move_ent <x> <y> <z> <pitch> <yaw> <roll> - Move the remembered entity to the given location
```
