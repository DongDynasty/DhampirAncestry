using Dawnsbury.Core.CharacterBuilder;
using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Common;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Spellbook;
using Dawnsbury.Core.CharacterBuilder.Spellcasting;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core.Creatures;
using Dawnsbury.Core.Creatures.Parts;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Core.Mechanics.Core;
using Dawnsbury.Core.Mechanics.Damage;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Core.Mechanics.Targeting.TargetingRequirements;
using Dawnsbury.Core.Mechanics.Targeting.Targets;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Core.Roller;
using Dawnsbury.Display.Illustrations;
using Dawnsbury.Modding;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace VersatileHeritages.Dhampir;

public class Dhampir
{
    //Ancestry Variables
    //-------------------------------------------------------------

    public static Trait DhampirTrait;
    public static FeatName DhampirFeatName;
    public static List<Trait> traits = [];
    internal record PendingVoidHarm(CombatAction? Source, DiceFormula Formula);


    //Main Loader
    //-------------------------------------------------------------

    public static void Load()
    {

        //registers the DhampirTrait as an ancestry trait
        DhampirTrait = ModManager.RegisterTrait("Dhampir",
            new TraitProperties("Dhampir", true, "", false)
            {
                IsAncestryTrait = true,
            });

        //add traits
        traits.Add(DhampirTrait);
        traits.Add(Trait.Uncommon);
        traits.Add(Trait.VersatileHeritage);

        AddDhampirHeritageFeats(CreateDhampirHeritageFeats());

        DhampirFeatName = ModManager.RegisterFeatName("DhampirHeritage", "Dhampir");

        Feat DhampirHeritage = new Feat(DhampirFeatName,
            "The mortal offspring of undead vampires are caught between life and undeath, mortality and immortality, mundane existence and powerful magic.",
            "You gain the dhampir trait, in addition to the traits from your ancestry. You have the void healing ability, which means you are harmed by vitality damage and healed by void effects as if you were undead. You also gain low-light vision, or you gain darkvision if your ancestry already has low-light vision. You can select from dhampir feats and feats from your ancestry whenever you gain an ancestry feat.",
            traits, null).WithOnSheet((Action<CalculatedCharacterSheetValues>)(sheet => sheet.Ancestries.Add(DhampirTrait)))
            .WithOnCreature(delegate (Creature creature)
            {
                creature.Traits.Add(DhampirTrait);
            })
            .WithPermanentQEffect("You have the void healing ability, which means you are harmed by vitality damage and healed by void effects as if you were undead.",
            QEffect =>
            {
                QEffect.AdjustDiceFormulaForSelfHealing = (self, action, formula) =>
                {
                    if (action == null || !action.HasTrait(Trait.Positive))
                    {
                        return formula;
                    }

                    if (action.SpellId == SpellId.Heal)
                    {
                        int die = (int) DifficultSpells.GetHealHarmDiceValue(action.Owner, true);
                        formula = DiceFormula.FromText($"{action.SpellLevel}d{die}", "Void Healing");
                    }

                    self.Tag = new PendingVoidHarm(action, formula);
                    return DiceFormula.FromText("0", "Void Healing");
                };

                QEffect.StateCheckWithVisibleChanges = async self =>
                {
                    if (self.Tag is not PendingVoidHarm pending)
                    {
                        return;
                    }

                    self.Tag = null;

                    await CommonSpellEffects.DealDirectDamage(
                        pending.Source,
                        pending.Formula,
                        self.Owner,
                        CheckResult.Success,
                        DamageKind.Untyped);
                };

                QEffect.YouAreDealtDamageEvent = async (self, damageEvent) =>
                {
                    int voidDamage = 0;

                    foreach (KindedDamage part in damageEvent.KindedDamages)
                    {
                        if (part.DamageKind == DamageKind.Negative && part.ResolvedDamage > 0)
                        {
                            voidDamage += part.ResolvedDamage;
                            part.ResolvedDamage = 0;
                        }
                    }

                    if (voidDamage == 0)
                    {
                        return;
                    }

                    CombatAction? action = damageEvent.CombatAction;

                    if (action?.SpellId == SpellId.Harm && action.SpentActions == 2)
                    {
                        voidDamage += 8 * action.SpellLevel;
                    }

                    await self.Owner.HealAsync(
                        DiceFormula.FromText(voidDamage.ToString(), "Void Healing"),
                        action ?? CombatAction.CreateSimple(self.Owner, "Void Healing"));
                };

                //QEffect.AdjustSavingThrowCheckResult = (self, defense, action, result) =>
                //{
                //    if (action?.SpellId == SpellId.Harm && result > CheckResult.Failure)
                //    {
                //        return CheckResult.Failure;
                //    }

                //    return result;
                //};

                QEffect.AfterYouAreHealed = (self, action, amount) =>
                {
                    if (amount > 0 && self.Tag is PendingVoidHarm pending && pending.Source == action)
                    {
                        self.Tag = pending with
                        {
                            Formula = pending.Formula.Add(DiceFormula.FromText(amount.ToString(), "Reverse bonus Healing"))
                        };
                    }

                    return Task.CompletedTask;
                };
            });

        //ModManager.RegisterActionOnEachSpell(spell =>
        //{
        //    if (spell.SpellId != SpellId.Harm) return;

        //    foreach (CreatureTarget target in AllCreatureTargets(spell.Target))
        //    {
        //        var requirements = target.CreatureTargetingRequirements;

        //        for (int i = 0; i < requirements.Count; i++)
        //        {
        //            if (requirements[i] is LegacyCreatureTargetingRequirement)
        //            {
        //                requirements[i] = new DhampirHarmTargeting(requirements[i]);
        //            }
        //        }
        //    }
        //});

        ModManager.AddFeat(DhampirHeritage);
    }

    private static void AddDhampirHeritageFeats(IEnumerable<Feat> feats)
    {
        foreach (var feat in feats)
        {
            ModManager.AddFeat(feat);
        }
    }

    private static IEnumerable<Feat> CreateDhampirHeritageFeats()
    {
        return new List<Feat>
        {
        };
    }

    private static IEnumerable<CreatureTarget> AllCreatureTargets(Target? target) => target switch
    {
        CreatureTarget creature => [creature],
        DependsOnActionsSpentTarget dependant => dependant.Targets.SelectMany(AllCreatureTargets),
        _ => []
    };

    internal class DhampirHarmTargeting(CreatureTargetingRequirement original) : CreatureTargetingRequirement
    {
        public override Usability Satisfied(Creature source, Creature target) =>
            target.HasTrait(DhampirTrait) ? Usability.Usable : original.Satisfied(source, target);
    }


    //Harmony
    //-------------------------------------------------------------

    [HarmonyPatch(typeof(DifficultSpells), nameof(DifficultSpells.WouldHealHarmSpellHeal))]
    internal static class DhampirWouldHealPatch
    {
        static bool Prefix(Creature target, bool isHeal, ref bool __result)
        {
            if (!target.HasTrait(Dhampir.DhampirTrait))
                return true;

            __result = !isHeal;
            return false;
        }
    }

    [HarmonyPatch(typeof(DifficultSpells), nameof(DifficultSpells.WouldHealHarmSpellHarm))]
    internal static class DhampirWouldHarmPatch
    {
        static bool Prefix(Creature target, bool isHeal, ref bool __result)
        {
            if (!target.HasTrait(Dhampir.DhampirTrait))
                return true;

            __result = isHeal;
            return false;
        }
    }

    //private static bool IsVitalityEffect(CombatAction? action)
    //{
    //    if (action == null)
    //    {
    //        return false;
    //    }

    //    if (action.HasTrait(Trait.Positive))
    //    {
    //        return true;
    //    }

    //    if (ModManager.TryParse("Vitality", out Trait vitality) && action.HasTrait(vitality))
    //    {

    //        return true;
    //    }

    //    return false;
    //}

    //private static bool IsVoidEffect(CombatAction? action)
    //{
    //    if (action == null)
    //    {
    //        return false;
    //    }

    //    if (action.HasTrait(Trait.Negative))
    //    {
    //        return true;
    //    }

    //    if (ModManager.TryParse("Void", out Trait voidTrait) && action.HasTrait(voidTrait))
    //    {

    //        return true;
    //    }

    //    return false;
    //}
}
