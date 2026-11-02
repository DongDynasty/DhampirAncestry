using Dawnsbury.Core;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.CharacterBuilder.FeatsDb;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.TrueFeatDb;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.TrueFeatDb.Ancestries;
using Dawnsbury.Core.CharacterBuilder.Selections.Options;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Mods.VersatileHeritages.DhampirHeritage;
using Dawnsbury.Core.Mechanics.Targeting.TargetingRequirements;
using Dawnsbury.Display.Illustrations;
using Dawnsbury.Modding;
using HarmonyLib;

namespace Dawnsbury.Mods.VersatileHeritages;
public class Loader
{
    [DawnsburyDaysModMainMethod]
    public static void LoadMod()
    {
        new Harmony("dawnsbury.mods.versatileheritages").PatchAll();

        Dhampir.Load();
    }
}