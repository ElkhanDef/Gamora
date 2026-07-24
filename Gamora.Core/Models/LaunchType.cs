namespace Gamora.Core.Models;

// "Battlenet" (BattleNet değil) yazılıyor çünkü JSON'a camelCase ile yazılınca "battlenet" olsun diye.
public enum LaunchType
{
    Exe,
    Steam,
    Riot,
    Battlenet,
    Epic
}
