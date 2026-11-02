using Dawnsbury.Core;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.CharacterBuilder.FeatsDb;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.TrueFeatDb;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.TrueFeatDb.Ancestries;
using Dawnsbury.Core.CharacterBuilder.Selections.Options;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Targeting.TargetingRequirements;
using Dawnsbury.Display.Illustrations;
using Dawnsbury.Modding;
using HarmonyLib;

namespace VersatileHeritages;
public class Loader
{
    [DawnsburyDaysModMainMethod]
    public static void LoadMod()
    {
        new Harmony("VersatileHeritages").PatchAll();

        Dhampir.Dhampir.Load();
    }
}
