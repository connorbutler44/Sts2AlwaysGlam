using HarmonyLib;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Models.Events;
using MegaCrit.Sts2.Core.Models.Relics;

namespace Sts2AlwaysGlam.Sts2AlwaysGlamCode;

[HarmonyPatch(typeof(Neow), "GenerateInitialOptions")]
internal static class NeowPatch
{
    [HarmonyPostfix]
    private static void Postfix(Neow __instance, ref IReadOnlyList<EventOption> __result)
    {
        // get neow's private CurseOptions
        var curseOptions =
            Traverse
                .Create(__instance)
                .Property("CurseOptions")
                .GetValue<IEnumerable<EventOption>>()
                ?.ToList()
            ?? throw new InvalidOperationException("Could not access Neow.CurseOptions");

        // identify the cursed relic that was added by Neow.GenerateInitialOptions
        var curseOptionIndex = __result
            .Select((option, index) => (option, index))
            .Where(x => x.option.Relic is not null)
            .Where(x =>
                curseOptions.Any(curseOption =>
                    curseOption.Relic?.GetType() == x.option.Relic!.GetType()
                )
            )
            .Select(x => x.index)
            .DefaultIfEmpty(-1)
            .First();

        if (curseOptionIndex < 0)
        {
            throw new InvalidOperationException(
                "Could not identify the cursed relic option in Neow.GenerateInitialOptions"
            );
        }

        // re-use the existing SilkenTress EventOption
        var silkenTressOption = curseOptions.FirstOrDefault(option => option.Relic is SilkenTress);

        if (silkenTressOption is null)
        {
            throw new InvalidOperationException("Silken Tress was not found in Neow.CurseOptions.");
        }

        var updatedOptions = __result.ToList();
        updatedOptions[curseOptionIndex] = silkenTressOption;

        __result = updatedOptions;
    }
}
