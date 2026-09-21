using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text.RegularExpressions;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using HarmonyLib;
using UnityEngine;
using RimWorld;
using Verse;

// Checks the SHIPPED Mod/Assemblies/WorkStudio.dll against the INSTALLED RimWorld, outside the
// game, per rimworld-tests-hors-jeu: the mod's own classes are instanced and called for real, and
// a Harmony patch target, the MainButtonDef mechanism, or a private-member access is checked
// against the installed Assembly-CSharp.dll, not the reference package, which carries no method
// bodies at all.
internal static class Program
{
    private static int failures;
    private static int skips;

    private static Assembly mod;
    private static string modFolder;
    private static string managedFolder;

    private static void Main(string[] args)
    {
        string managed = args.Length > 0 ? args[0] : Metadata("RimWorldManaged");
        string modAssembly = args.Length > 1 ? args[1] : Metadata("ModAssembly");
        modFolder = args.Length > 2 ? args[2] : Metadata("ModFolder");
        managedFolder = managed;

        // Assembly-CSharp pulls in Unity assemblies that are not beside this executable.
        AppDomain.CurrentDomain.AssemblyResolve += (s, e) =>
        {
            string path = Path.Combine(managed, new AssemblyName(e.Name).Name + ".dll");
            return File.Exists(path) ? Assembly.LoadFrom(path) : null;
        };

        Console.WriteLine("RimWorld:      " + managed);
        Console.WriteLine("mod assembly:  " + modAssembly);
        Console.WriteLine("mod folder:    " + modFolder);
        Console.WriteLine();

        Run(modAssembly);

        Console.WriteLine();
        Console.WriteLine(failures == 0 && skips == 0 ? "ALL CHECKS PASSED" : failures + " CHECK(S) FAILED; " + skips + " CHECK(S) INCOMPLETE");
        if (skips > 0) Console.WriteLine(skips + " check(s) skipped");
        Environment.Exit(failures == 0 && skips == 0 ? 0 : 1);
    }

    // Kept out of Main and out of its inlining reach: the JIT resolves the types a method names
    // before running a line of it, so naming a Verse type in Main would load Assembly-CSharp
    // before the resolver above is in place, and the run dies on its first instruction.
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void Run(string modAssembly)
    {
        mod = Assembly.LoadFrom(modAssembly);
        SilenceLog();

        // DeepProfiler.Start/End (called around ScribeLoader's cross-reference resolution) reads
        // Prefs.LogVerbose, backed by Prefs.data - never loaded outside the game, so the property
        // throws a NullReferenceException before DeepProfiler even gets to its own no-op path.
        // Documented precedent: rimworld-tests-hors-jeu, the Colorful Coats patch-engine chapter.
        DeepProfiler.enabled = false;

        TheAssemblyWaiver();
        TheNonPublicMemberScan();
        TheAccessTheModMakes();
        TheHarmonyPatchTargets();
        TheMainButtonsShortcut();
        ThePriorityFallback();
        TheConfigRoundTrip();
        TheKeyedCoverage();
        TheWorkTypeTagCompat();
        TheBetterWorkTabColumnOrder();
        TheHideColumnRegression();
        TheAmbiguousTaskLabels();
        TheMiddleTruncation();
        TheTypeIdsAreNeverReused();
        TheIconPatchTargets();
        TheIconNames();
        ThePickleStepTable();
    }

    // --- the publicizer waiver -----------------------------------------------------------------

    // Krafs.Publicizer embeds the IgnoresAccessChecksToAttribute TYPE regardless; whether it is
    // actually APPLIED to the assembly is a separate step this project's GenerateAssemblyInfo=false
    // silently skipped once already (see Source/AccessChecks.cs). Reading metadata tells whether
    // the waiver exists; TheAccessTheModMakes below tells whether it was needed at all.
    private static void TheAssemblyWaiver()
    {
        Console.WriteLine("the publicizer waiver:");

        bool has = mod.GetCustomAttributesData().Any(a =>
            a.AttributeType.Name == "IgnoresAccessChecksToAttribute"
            && a.ConstructorArguments.Count == 1
            && (string)a.ConstructorArguments[0].Value == "Assembly-CSharp");

        Check("WorkStudio.dll declares [assembly: IgnoresAccessChecksTo(\"Assembly-CSharp\")]", has);
    }

    // --- every non-public Assembly-CSharp member the shipped DLL actually touches ---------------

    // TheAccessTheModMakes below performs a live exercise of the two members PriorityMemory.Restore
    // touches, but the assembly-wide waiver covers whatever else the mod reaches for too -
    // WorkTypeRuntime's PawnColumnDef.workerInt and BackstoryDef.cachedDisabledWorkTypes among them.
    // Per rimworld-tests-hors-jeu's "balayage inverse": walk every method body the mod ships,
    // resolve every field/method token it references, and flag whichever of those belongs to
    // Assembly-CSharp and is not public - except a protected member reached from a class that
    // actually derives from its declaring type, which is ordinary legal C# needing no waiver at all.
    private static readonly Dictionary<short, OpCode> opcodes = BuildOpcodeTable();

    private static Dictionary<short, OpCode> BuildOpcodeTable()
    {
        var table = new Dictionary<short, OpCode>();
        foreach (FieldInfo f in typeof(OpCodes).GetFields(BindingFlags.Public | BindingFlags.Static))
        {
            if (f.FieldType == typeof(OpCode))
            {
                var code = (OpCode)f.GetValue(null);
                table[code.Value] = code;
            }
        }
        return table;
    }

    private static void TheNonPublicMemberScan()
    {
        Console.WriteLine();
        Console.WriteLine("every non-public Assembly-CSharp member the shipped DLL touches:");

        var seen = new SortedSet<string>();
        var flagged = new SortedSet<string>();
        int unresolved = 0;

        BindingFlags allMembers = BindingFlags.Public | BindingFlags.NonPublic
            | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;

        foreach (Type t in mod.GetTypes())
        {
            IEnumerable<MethodBase> methods = t.GetConstructors(allMembers).Cast<MethodBase>()
                .Concat(t.GetMethods(allMembers).Cast<MethodBase>());

            foreach (MethodBase m in methods)
            {
                byte[] il;
                try { il = m.GetMethodBody()?.GetILAsByteArray(); }
                catch { continue; }
                if (il == null) continue;

                int i = 0;
                while (i < il.Length)
                {
                    short opValue = il[i] == 0xFE ? (short)(0xFE00 | il[i + 1]) : il[i];
                    i += il[i] == 0xFE ? 2 : 1;

                    if (!opcodes.TryGetValue(opValue, out OpCode code))
                    {
                        break; // an opcode this table does not know: stop reading this method body
                    }

                    if (code.OperandType == OperandType.InlineSwitch)
                    {
                        int count = BitConverter.ToInt32(il, i);
                        i += 4 + count * 4;
                        continue;
                    }

                    int operandSize = OperandSize(code.OperandType);

                    if (code.OperandType == OperandType.InlineField
                        || code.OperandType == OperandType.InlineMethod
                        || code.OperandType == OperandType.InlineTok)
                    {
                        int token = BitConverter.ToInt32(il, i);
                        try
                        {
                            MemberInfo member = m.Module.ResolveMember(token);
                            Inspect(member, t, seen, flagged);
                        }
                        catch
                        {
                            unresolved++;
                        }
                    }

                    i += operandSize;
                }
            }
        }

        foreach (string line in seen) Console.WriteLine("    " + line);
        if (unresolved > 0) Console.WriteLine("    (" + unresolved + " token(s) left unresolved - generic instantiations, not a game reference)");

        // The mod deliberately uses Krafs.Publicizer, so finding non-public members here is
        // expected, not a defect - the defect (rimworld-tests-hors-jeu's "the mod publicises"
        // chapter) is finding them WITHOUT the waiver that makes touching them legal. That is the
        // conditional form the same chapter's "critere qui ne marche pas" section settles on.
        bool waiverPresent = mod.GetCustomAttributesData().Any(a =>
            a.AttributeType.Name == "IgnoresAccessChecksToAttribute"
            && a.ConstructorArguments.Count == 1
            && (string)a.ConstructorArguments[0].Value == "Assembly-CSharp");

        Check(flagged.Count == 0
                ? "no non-public Assembly-CSharp member is touched outside a legal protected/subclass access"
                : flagged.Count + " non-public member(s) touched (" + string.Join(", ", flagged)
                  + "), all legal only because the assembly declares IgnoresAccessChecksTo",
            flagged.Count == 0 || waiverPresent);
    }

    private static void Inspect(MemberInfo member, Type callingType, SortedSet<string> seen, SortedSet<string> flagged)
    {
        Type declaringType = member.DeclaringType;
        if (declaringType == null || declaringType.Assembly != typeof(Pawn).Assembly)
        {
            return; // not a reference into the game at all
        }

        bool isPublic = member is FieldInfo field ? field.IsPublic
            : member is MethodBase method ? method.IsPublic
            : true;
        if (isPublic) return;

        bool isProtected = member is FieldInfo pf ? pf.IsFamily || pf.IsFamilyOrAssembly
            : member is MethodBase pm && (pm.IsFamily || pm.IsFamilyOrAssembly);
        bool exempt = isProtected && declaringType.IsAssignableFrom(callingType);

        string label = declaringType.Name + "." + member.Name + (exempt ? "  (protected, reached from a subclass - no waiver needed)" : "");
        seen.Add(label);
        if (!exempt) flagged.Add(declaringType.Name + "." + member.Name);
    }

    private static int OperandSize(OperandType type)
    {
        switch (type)
        {
            case OperandType.InlineNone: return 0;
            case OperandType.ShortInlineBrTarget:
            case OperandType.ShortInlineI:
            case OperandType.ShortInlineVar: return 1;
            case OperandType.InlineVar: return 2;
            case OperandType.InlineI8:
            case OperandType.InlineR: return 8;
            default: return 4; // InlineBrTarget, InlineField, InlineI, InlineMethod, InlineSig,
                                // InlineString, InlineTok, InlineType, ShortInlineR, InlineSig
        }
    }

    // --- the access itself, performed rather than read from metadata -------------------------

    private static Harmony harmony;
    private static List<Pawn> fakePawns;

    // PriorityMemory.Restore reads Pawn_WorkSettings.priorities, writes its workGiversDirty and
    // calls its CacheWorkGiversInOrder() - all three private in the real game. WorkTypeRuntime
    // touches two more (PawnColumnDef.workerInt, BackstoryDef.cachedDisabledWorkTypes) on the same
    // Apply() path, but the waiver is assembly-wide: fixing and proving it here covers all five,
    // and this trio sits on the mod's most critical path - StartupInit runs it on every launch, and
    // it is what TESTING.md scenario 5 depends on entirely.
    //
    // Confirmed by mutation, not just by this passing: with Source/AccessChecks.cs removed, this
    // check fails with a real FieldAccessException on Pawn_WorkSettings.priorities, thrown from
    // inside PriorityMemory.Restore itself - the same silent crash rimworld-tests-hors-jeu records
    // for Contented Livestock's milking patches, and the reason AccessChecks.cs exists at all.
    private static void TheAccessTheModMakes()
    {
        Console.WriteLine();
        Console.WriteLine("the private Assembly-CSharp members PriorityMemory touches, exercised for real:");

        Type priorityMemory = ModType("WorkStudio.PriorityMemory", true);
        Type snapshotType = ModType("WorkStudio.PriorityMemory+Snapshot", true);
        if (priorityMemory == null || snapshotType == null)
        {
            Skip("PriorityMemory.Restore: type not found");
            return;
        }

        try
        {
            Exercise(priorityMemory, snapshotType);
        }
        catch (Exception e)
        {
            Exception inner = Innermost(e);
            Check("PriorityMemory.Restore touches workGiversDirty and CacheWorkGiversInOrder without an access exception"
                  + "  (threw " + inner.GetType().Name + ": " + inner.Message + ")", false);
        }
    }

    private static void Exercise(Type priorityMemoryType, Type snapshotType)
    {
        harmony = new Harmony("nelim.workstudio.tests");

        // Neither of these two calls is what is under test - they are neutered so a bare,
        // constructor-skipped pawn survives them - but the access from PriorityMemory.Restore into
        // workGiversDirty and CacheWorkGiversInOrder, both private, still has to clear the CLR's
        // own check on the way there, and Harmony patching a method's body does not touch that.
        harmony.Patch(AccessTools.Method(typeof(Pawn), nameof(Pawn.Notify_DisabledWorkTypesChanged)),
            prefix: new HarmonyMethod(typeof(Program), nameof(SkipPrefix)));
        harmony.Patch(AccessTools.Method(typeof(Pawn_WorkSettings), "CacheWorkGiversInOrder"),
            prefix: new HarmonyMethod(typeof(Program), nameof(SkipPrefix)));

        // PawnsFinder.All_AliveOrDead needs a running game, a map and a world; the private
        // AllPawns() that reads it is replaced with one fake pawn instead of building all three.
        MethodInfo allPawns = AccessTools.Method(priorityMemoryType, "AllPawns");
        Pawn fakePawn = (Pawn)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(Pawn));
        Pawn_WorkSettings fakeSettings =
            (Pawn_WorkSettings)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(Pawn_WorkSettings));
        SetField(fakeSettings, "priorities", MakePriorities());
        fakePawn.workSettings = fakeSettings;
        fakePawns = new List<Pawn> { fakePawn };
        harmony.Patch(allPawns, prefix: new HarmonyMethod(typeof(Program), nameof(FakeAllPawns)));

        object snapshot = Activator.CreateInstance(snapshotType);
        var seeds = new Dictionary<string, string>();

        MethodInfo restore = AccessTools.Method(priorityMemoryType, "Restore");
        restore.Invoke(null, new object[] { snapshot, seeds });

        Check("settings.workGiversDirty was written", GetField<bool>(fakeSettings, "workGiversDirty"));
        Check("settings.CacheWorkGiversInOrder() ran without an access exception", true);
    }

    // DefMap's own constructor refuses to run without defs loaded ("Try constructing it in
    // ResolveReferences instead"), which is exactly the state outside the game. Skipping it is fine
    // here: only the values list this test itself populates is ever read.
    private static DefMap<WorkTypeDef, int> MakePriorities()
    {
        var map = (DefMap<WorkTypeDef, int>)System.Runtime.Serialization.FormatterServices
            .GetUninitializedObject(typeof(DefMap<WorkTypeDef, int>));
        SetField(map, "values", new List<int> { 3 });
        return map;
    }

    private static bool FakeAllPawns(ref List<Pawn> __result)
    {
        __result = fakePawns;
        return false;
    }

    private static bool SkipPrefix() => false;

    // --- the Harmony patch targets --------------------------------------------------------------

    private static void TheHarmonyPatchTargets()
    {
        Console.WriteLine();
        Console.WriteLine("the Harmony patch targets still exist in the installed game:");

        Signature(typeof(Pawn), "GetDisabledWorkTypes");
        Signature(typeof(Pawn_WorkSettings), "ExposeData");

        Type patchButton = ModType("WorkStudio.Patch_WorkTabButton", true);
        MethodInfo findDrawMethod = patchButton == null ? null : AccessTools.Method(patchButton, "FindDrawMethod");
        if (findDrawMethod == null)
        {
            Skip("Patch_WorkTabButton.FindDrawMethod: not found");
            return;
        }

        // The def this walks tabWindowClass from at runtime; loading Core's own MainButtonDefs.xml
        // is more machinery than the one field this check actually needs.
        object resolved = findDrawMethod.Invoke(null, new object[] { typeof(MainTabWindow_Work) });
        Check("FindDrawMethod(typeof(MainTabWindow_Work)) finds a DoWindowContents to patch",
            resolved is MethodInfo m && m.Name == "DoWindowContents");
    }

    private static void Signature(Type owner, string name)
    {
        MethodInfo[] found = owner.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
            .Where(m => m.Name == name).ToArray();
        Check(owner.Name + "." + name + "() still exists" + (found.Length > 1 ? " (" + found.Length + " overloads)" : ""),
            found.Length > 0);
    }

    // --- the MainButtons shortcut ----------------------------------------------------------------

    private static void TheMainButtonsShortcut()
    {
        Console.WriteLine();
        Console.WriteLine("the hidden MainButtons shortcut:");

        string path = Path.Combine(modFolder, "Defs", "MainButtonDefs", "MainButtonDefs.xml");
        if (!File.Exists(path))
        {
            Check("Defs/MainButtonDefs/MainButtonDefs.xml exists", false);
            return;
        }

        XElement def = XDocument.Load(path).Root.Elements("MainButtonDef")
            .FirstOrDefault(e => (string)e.Element("defName") == "WorkStudio_Settings");
        Check("WorkStudio_Settings is declared", def != null);
        if (def == null) return;

        Check("hidden by default (buttonVisible=false)", (bool?)def.Element("buttonVisible") == false);
        Check("workerClass names WorkStudio.MainButtonWorker_OpenSettings",
            (string)def.Element("workerClass") == "WorkStudio.MainButtonWorker_OpenSettings");

        Type workerType = ModType("WorkStudio.MainButtonWorker_OpenSettings", true);
        if (workerType == null)
        {
            Skip("MainButtonWorker_OpenSettings: type not found");
            return;
        }

        Check("MainButtonWorker_OpenSettings derives from MainButtonWorker", typeof(MainButtonWorker).IsAssignableFrom(workerType));

        MethodInfo activate = workerType.GetMethod("Activate",
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        Check("Activate() is declared on the worker itself", activate != null);
        if (activate != null)
        {
            // A `new` method instead of `override` compiles, loads and is simply never called: the
            // slot check is what tells the two apart, not the accessibility.
            MethodInfo baseDef = activate.GetBaseDefinition();
            Check("Activate() overrides MainButtonWorker.Activate rather than hiding it",
                baseDef.DeclaringType == typeof(MainButtonWorker) && baseDef != activate);
        }

        // MainButtonWorker.Visible is never overridden here, so calling it would exercise the real
        // vanilla getter - decompiling MainButtonWorker.get_Visible shows it reads
        // def.buttonVisible directly, gated by ModsConfig.IdeologyActive. That gate is the actual
        // blocker: ModsConfig's static constructor reaches GenFilePaths.SaveDataFolderPath, the
        // same ECall-poisoned property TheConfigRoundTrip works around by never calling it - but
        // there the workaround is to avoid GenFilePaths entirely, and here the call is inside
        // vanilla's own property, not reachable from outside it. So this stays a structural check
        // (declared override, correct base type, the def content above) rather than a live one;
        // TheAccessTheModMakes exercises a real Verse call for the equivalent live proof elsewhere.
        Skip("Worker.Visible: unreachable off-game - ModsConfig.IdeologyActive's static constructor " +
             "needs GenFilePaths.SaveDataFolderPath, which cannot be called at all in this harness");
    }

    // --- PriorityMemory.PriorityFor, the mod's reason to exist ----------------------------------

    private static void ThePriorityFallback()
    {
        Console.WriteLine();
        Console.WriteLine("PriorityMemory's fallback chain:");

        Type priorityMemory = ModType("WorkStudio.PriorityMemory", true);
        MethodInfo priorityFor = priorityMemory == null ? null : AccessTools.Method(priorityMemory, "PriorityFor");
        if (priorityFor == null)
        {
            Skip("PriorityMemory.PriorityFor: method not found");
            return;
        }

        var cooking = new WorkTypeDef { defName = "Cooking" };
        var hauling = new WorkTypeDef { defName = "Hauling" };
        var saved = new Dictionary<string, int> { { "Cooking", 3 } };

        Check("a saved value for the type itself wins",
            (int)priorityFor.Invoke(null, new object[] { saved, new Dictionary<string, string>(), cooking }) == 3);

        Check("no saved value for the type, but a seed that has one, takes the seed's value",
            (int)priorityFor.Invoke(null,
                new object[] { saved, new Dictionary<string, string> { { "Hauling", "Cooking" } }, hauling }) == 3);

        Check("no saved value and no useful seed falls back to Pawn_WorkSettings.DefaultPriority",
            (int)priorityFor.Invoke(null, new object[] { saved, new Dictionary<string, string>(), hauling })
            == Pawn_WorkSettings.DefaultPriority);

        Check("a pawn Work Studio never met (a null saved dictionary) also falls back to the default",
            (int)priorityFor.Invoke(null, new object[] { null, new Dictionary<string, string> { { "Cooking", "Hauling" } }, cooking })
            == Pawn_WorkSettings.DefaultPriority);
    }

    // --- the settings' Scribe round trip, off the game's file-path machinery -------------------

    // ConfigFile.PathFor/Folder/Export/Import all lead into GenFilePaths.SaveDataFolderPath, and
    // merely calling that property throws outside a running Unity player: one of its branches
    // reaches Application.persistentDataPath, an ECall, and the CLR refuses to JIT the whole
    // method rather than skip the branch not taken - the same trap rimworld-tests-hors-jeu
    // documents for ThingDef's own constructor, one level up the call chain. So ConfigFile itself
    // is not exercised here; what is checked for real is the serialization contract it wraps -
    // WorkStudioSettings.ExposeConfig() and CustomWorkTypeEntry.ExposeData(), against the game's
    // own Scribe, driven at a path this test computes itself instead of asking GenFilePaths for
    // one. ConfigFile.PathFor/Folder/Export/Import stay untested off-game; see remaining.
    private static void TheConfigRoundTrip()
    {
        Console.WriteLine();
        Console.WriteLine("WorkStudioSettings' Scribe round trip (ConfigFile itself needs GenFilePaths, see remaining):");

        string tempRoot = Path.Combine(Path.GetTempPath(), "workstudio-tests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempRoot);

        try
        {
            RunConfigRoundTrip(Path.Combine(tempRoot, "round-trip.xml"));
        }
        catch (Exception e)
        {
            Console.WriteLine("DIAG: " + e);
            Skip("the Scribe round trip: " + Innermost(e).GetType().Name + " - " + Innermost(e).Message);
        }
        finally
        {
            try { Directory.Delete(tempRoot, true); } catch { /* best effort */ }
        }
    }

    private static void RunConfigRoundTrip(string path)
    {
        Type settingsType = ModType("WorkStudio.WorkStudioSettings", true);
        Type entryType = ModType("WorkStudio.CustomWorkTypeEntry", true);
        if (settingsType == null || entryType == null)
        {
            Skip("the Scribe round trip: WorkStudioSettings or CustomWorkTypeEntry not found");
            return;
        }

        object settings = Activator.CreateInstance(settingsType);
        var customTypes = (System.Collections.IList)settingsType.GetField("customTypes").GetValue(settings);
        object entry = Activator.CreateInstance(entryType, "WS_Test", "Pickle herding");
        customTypes.Add(entry);
        var hiddenTypes = (System.Collections.IList)settingsType.GetField("hiddenTypes").GetValue(settings);
        hiddenTypes.Add("Cleaning");
        var knownWorkTypes = (System.Collections.IList)settingsType.GetField("knownWorkTypes").GetValue(settings);
        knownWorkTypes.Add("SomeTypeFromLastStartup");

        MethodInfo exposeConfigForSave = settingsType.GetMethod("ExposeConfig", BindingFlags.Public | BindingFlags.Instance);

        Scribe.saver.InitSaving(path, "workStudioConfig");
        try
        {
            exposeConfigForSave.Invoke(settings, null);
        }
        finally
        {
            Scribe.saver.FinalizeSaving();
        }

        Check("the exported file exists", File.Exists(path));
        if (!File.Exists(path)) return;

        string xml = File.ReadAllText(path);
        Check("the export carries the custom type", xml.Contains("Pickle herding"));
        Check("the export carries the hidden type", xml.Contains("Cleaning"));
        Check("the export does NOT carry knownWorkTypes, a snapshot of this machine's mod list",
            !xml.Contains("knownWorkTypes") && !xml.Contains("SomeTypeFromLastStartup"));

        // A fresh instance, loaded exactly the way ConfigFile.Import does before it calls
        // WorkTypeRuntime.Apply() - which is the part left untested here, see above.
        object incoming = Activator.CreateInstance(settingsType);
        MethodInfo exposeConfig = settingsType.GetMethod("ExposeConfig", BindingFlags.Public | BindingFlags.Instance);

        // Mirrors ConfigFile.Import up to the point where it hands off to WorkTypeRuntime.Apply().
        Scribe.loader.InitLoading(path);
        exposeConfig.Invoke(incoming, null);
        Scribe.loader.FinalizeLoading();

        var loadedCustomTypes = (System.Collections.IList)settingsType.GetField("customTypes").GetValue(incoming);
        var loadedHidden = (System.Collections.IList)settingsType.GetField("hiddenTypes").GetValue(incoming);
        Check("the loaded settings carry one custom type back", loadedCustomTypes.Count == 1);
        Check("the loaded settings carry the hidden type back", loadedHidden.Contains("Cleaning"));
    }

    // --- Keyed coverage, both languages ----------------------------------------------------------

    private static void TheKeyedCoverage()
    {
        Console.WriteLine();
        Console.WriteLine("Keyed coverage, English against French:");

        string enPath = Path.Combine(modFolder, "Languages", "English", "Keyed", "WorkStudio.xml");
        string frPath = Path.Combine(modFolder, "Languages", "French", "Keyed", "WorkStudio.xml");
        Check("the English Keyed file exists", File.Exists(enPath));
        Check("the French Keyed file exists", File.Exists(frPath));
        if (!File.Exists(enPath) || !File.Exists(frPath)) return;

        Dictionary<string, string> en = ReadKeyed(enPath);
        Dictionary<string, string> fr = ReadKeyed(frPath);

        string[] onlyEn = en.Keys.Except(fr.Keys).OrderBy(k => k).ToArray();
        string[] onlyFr = fr.Keys.Except(en.Keys).OrderBy(k => k).ToArray();
        Check("no key exists in English only" + (onlyEn.Length > 0 ? ": " + string.Join(", ", onlyEn) : ""), onlyEn.Length == 0);
        Check("no key exists in French only" + (onlyFr.Length > 0 ? ": " + string.Join(", ", onlyFr) : ""), onlyFr.Length == 0);

        Check("no English value is empty", en.Values.All(v => !string.IsNullOrEmpty(v)));
        Check("no French value is empty", fr.Values.All(v => !string.IsNullOrEmpty(v)));

        foreach (string key in en.Keys.Intersect(fr.Keys).OrderBy(k => k))
        {
            int[] enParams = Placeholders(en[key]);
            int[] frParams = Placeholders(fr[key]);
            Check("{" + key + "} carries the same {N} placeholders in both languages",
                enParams.SequenceEqual(frParams));
        }
    }

    private static Dictionary<string, string> ReadKeyed(string path)
    {
        return XDocument.Load(path).Root.Elements()
            .ToDictionary(e => e.Name.LocalName, e => e.Value);
    }

    private static int[] Placeholders(string text)
    {
        var found = new List<int>();
        for (int i = 0; i < text.Length - 2; i++)
        {
            if (text[i] == '{' && char.IsDigit(text[i + 1]) && text[i + 2] == '}')
            {
                found.Add(text[i + 1] - '0');
            }
        }
        return found.Distinct().OrderBy(n => n).ToArray();
    }

    // --- TESTING.md scenario 7, off-game: [baku] Work Type Tag's label cache -------------------

    // WorkTypeTagCompat.Notify() looks up baku.WorkTypeTag.WorkTypeColorResolver by name and calls
    // its InvalidateAll(), on the strength of a comment: that mod caches a work type's label the
    // first time it draws it, keyed by the WorkTypeDef instance, and only clears that cache from
    // its own settings window - so a rename from Work Studio would otherwise leave the old name in
    // front of a colonist's current job until a restart. This proves that claim by loading the real
    // Work Type Tag assembly (if it is installed - Workshop item 3779138895) and driving its own
    // GetPresentation/InvalidateAll for real, the same way rimworld-tests-hors-jeu drives Scribe or
    // PatchOperation: real third-party code, not a description of it.
    private static void TheWorkTypeTagCompat()
    {
        Console.WriteLine();
        Console.WriteLine("[baku] Work Type Tag's label cache, the compat WorkTypeTagCompat.Notify() targets:");

        string dll = FindWorkTypeTagAssembly();
        if (dll == null)
        {
            Skip("Work Type Tag (Workshop 3779138895) not found locally - subscribe it to run this check");
            return;
        }

        Assembly tag;
        Type resolverType;
        try
        {
            tag = Assembly.LoadFrom(dll);
            resolverType = tag.GetType("baku.WorkTypeTag.WorkTypeColorResolver", true);
        }
        catch (Exception e)
        {
            Skip("loading Work Type Tag: " + Innermost(e).GetType().Name + " - " + Innermost(e).Message);
            return;
        }

        MethodInfo getPresentation = AccessTools.Method(resolverType, "GetPresentation");
        FieldInfo labelField = AccessTools.Field(tag.GetType("baku.WorkTypeTag.WorkTypePresentation"), "label");
        Type compatType = ModType("WorkStudio.WorkTypeTagCompat", true);
        MethodInfo notify = compatType == null ? null : AccessTools.Method(compatType, "Notify");
        if (getPresentation == null || labelField == null || notify == null)
        {
            Skip("Work Type Tag's internals or WorkStudio.WorkTypeTagCompat.Notify() changed - not found");
            return;
        }

        var type = new WorkTypeDef { defName = "PickleWorkTypeTagCompat", label = "before" };

        string Label() => (string)labelField.GetValue(getPresentation.Invoke(null, new object[] { type }));

        Check("Work Type Tag capitalises and caches the label the first time it is drawn",
            Label() == "Before");

        type.label = "after";
        Check("without invalidation, the cache still shows the label from before the rename - " +
              "proving the compat call is not a no-op", Label() == "Before");

        // WorkStudio.WorkTypeTagCompat.Notify() itself, not a reimplementation of what it does:
        // it looks up baku.WorkTypeTag.WorkTypeColorResolver by name (AccessTools.TypeByName, which
        // searches every loaded assembly) and calls its InvalidateAll(). Since this test already
        // loaded Work Type Tag's real assembly above, that lookup finds the real thing.
        notify.Invoke(null, null);

        Check("after WorkStudio.WorkTypeTagCompat.Notify(), the new label is read", Label() == "After");
    }

    private static string FindWorkTypeTagAssembly() => FindWorkshopAssembly("3779138895", "baku.WorkTypeTag.dll");

    private static string FindWorkshopAssembly(string itemId, string dllName)
    {
        try
        {
            // .../RimWorld/RimWorldWin64_Data/Managed -> the Steam library's steamapps folder.
            string steamapps = Path.GetFullPath(Path.Combine(managedFolder, "..", "..", "..", ".."));
            string workshopMod = Path.Combine(steamapps, "workshop", "content", "294100", itemId);
            if (!Directory.Exists(workshopMod)) return null;

            return Directory.GetFiles(workshopMod, dllName, SearchOption.AllDirectories)
                .OrderByDescending(f => f) // higher version folders ("1.6" > "1.5" > ...) sort later
                .FirstOrDefault();
        }
        catch
        {
            return null;
        }
    }

    // --- TESTING.md scenario 11, off-game: Better Work Tab's own column-order rule -------------

    // Unlike scenario 7, Work Studio has no code path into Better Work Tab at all - the
    // "compatibility" described in About.xml is two mods independently reading and writing
    // PawnTableDefOf.Work.columns, not an integration this mod owns. What is worth protecting is
    // that description staying true: WorkColumnOrderManager.ApplyOrderToTable is the exact private
    // method that implements it, callable directly by reflection without a live Game, since it
    // takes the PawnTableDef and the recorded order as plain arguments rather than reading them
    // from GameComponent_BWTWorldSettings itself.
    private static void TheBetterWorkTabColumnOrder()
    {
        Console.WriteLine();
        Console.WriteLine("Better Work Tab's own column-order rule, which About.xml describes:");

        string dll = FindWorkshopAssembly("3626737803", "Better Work Tab.dll");
        if (dll == null)
        {
            Skip("Better Work Tab (Workshop 3626737803) not found locally - subscribe it to run this check");
            return;
        }

        Assembly bwt;
        MethodInfo applyOrderToTable;
        try
        {
            bwt = Assembly.LoadFrom(dll);
            Type managerType = bwt.GetType("Better_Work_Tab.Features.WorkColumnOrderManager", true);
            applyOrderToTable = AccessTools.Method(managerType, "ApplyOrderToTable");
        }
        catch (Exception e)
        {
            Skip("loading Better Work Tab: " + Innermost(e).GetType().Name + " - " + Innermost(e).Message);
            return;
        }

        if (applyOrderToTable == null)
        {
            Skip("Better Work Tab's WorkColumnOrderManager.ApplyOrderToTable not found - its internals changed");
            return;
        }

        // The reordering itself is done before ApplyOrderToTable's own trailing notification calls
        // - WorkExecutionOrder.MarkAllPawnsWorkGiversDirty() among them - which need a live Game
        // and NRE without one. Neutered the same way BurnBarrel's test silences an emitter it is
        // not exercising: the mutation under test still runs for real, only the side effects do not.
        var bwtSilencer = new Harmony("nelim.workstudio.tests.bwt");
        try
        {
            MethodInfo markDirty = bwt.GetType("Better_Work_Tab.Features.WorkExecutionOrder")?
                .GetMethod("MarkAllPawnsWorkGiversDirty", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            if (markDirty != null) bwtSilencer.Patch(markDirty, prefix: new HarmonyMethod(typeof(Program), nameof(SkipPrefix)));

            MethodInfo notify = AccessTools.Method(typeof(MainTabWindowUtility), "NotifyAllPawnTables_PawnsChanged");
            if (notify != null) bwtSilencer.Patch(notify, prefix: new HarmonyMethod(typeof(Program), nameof(SkipPrefix)));

            RunColumnOrderCheck(applyOrderToTable);
        }
        catch (Exception e)
        {
            Skip("Better Work Tab's column-order rule: " + Innermost(e).GetType().Name + " - " + Innermost(e).Message);
        }
    }

    private static void RunColumnOrderCheck(MethodInfo applyOrderToTable)
    {
        var cooking = new WorkTypeDef { defName = "PickleBwtCooking" };
        var hauling = new WorkTypeDef { defName = "PickleBwtHauling" };
        var newType = new WorkTypeDef { defName = "PickleBwtNewType" }; // "a type created afterwards"

        // PawnColumnDef's own default workerClass, plain PawnColumnWorker, is abstract and cannot
        // be instantiated - a concrete, real vanilla non-work worker stands in for one instead.
        var nonWorkColumn = new PawnColumnDef { defName = "PickleBwtIcon", workerClass = typeof(PawnColumnWorker_CopyPasteWorkPriorities) };
        var colCooking = WorkColumn(cooking);
        var colHauling = WorkColumn(hauling);
        var colNew = WorkColumn(newType);

        var table = new PawnTableDef { columns = new List<PawnColumnDef> { nonWorkColumn, colCooking, colHauling, colNew } };

        // Recorded before "New" was ever created - exactly the gap About.xml describes.
        var recordedOrder = new List<string> { hauling.defName, cooking.defName };

        applyOrderToTable.Invoke(null, new object[] { table, recordedOrder });

        var workColumnsAfter = table.columns.Where(c => c.workType != null).Select(c => c.workType.defName).ToList();

        Check("a non-work column is left where Better Work Tab's own split puts it",
            table.columns.Count > 0 && table.columns[0] == nonWorkColumn);
        Check("the recorded order is applied to the columns it knows about",
            workColumnsAfter.Count >= 2 && workColumnsAfter[0] == hauling.defName && workColumnsAfter[1] == cooking.defName);
        Check("a work type created after the last recorded order goes last, exactly as About.xml says",
            workColumnsAfter.Count > 0 && workColumnsAfter[workColumnsAfter.Count - 1] == newType.defName);
    }

    private static PawnColumnDef WorkColumn(WorkTypeDef type)
    {
        var column = new PawnColumnDef
        {
            defName = "WorkPriority_" + type.defName,
            workerClass = typeof(PawnColumnWorker_WorkPriority),
            workType = type,
        };
        return column;
    }

    // --- TESTING.md scenario 8, off-game: does hiding a type ever touch its priority? ----------

    // The live Pickle run on 2026-09-17 found "the column goes, the work stays" and "showing it
    // again..." both failing: a colonist's priority for a hidden type read back as 0, not the 2 it
    // was given right before. Reading WorkTypeRuntime.Apply()'s own steps end to end finds nothing
    // that keys off Settings.hiddenTypes except ApplyTypeOverrides' own `type.visible` line - the
    // type stays in DefDatabase<WorkTypeDef>, keeps its index, and PriorityMemory.Capture/Restore
    // both iterate every def regardless of visibility, keyed by defName. On paper, hiding a type
    // should be a pure no-op for every pawn's priorities.
    //
    // This drives that exact pipeline for real, against a single fake WorkTypeDef and a single fake
    // pawn, the same way TheAccessTheModMakes does for PriorityMemory.Restore alone - except this
    // calls the whole WorkTypeRuntime.Apply(), including SyncCustomTypes, ApplyTypeOverrides,
    // RebuildDefs and RebuildWorkColumns, exactly as SetVisible's own Commit() does. If this passes,
    // the live failure is not a defect in this pipeline - it wants a live re-run to find where it
    // actually comes from (environment noise, or something outside Apply() entirely).
    private static void TheHideColumnRegression()
    {
        Console.WriteLine();
        Console.WriteLine("TESTING.md scenario 8 (hide a column), reproduced against the real WorkTypeRuntime.Apply():");

        Type runtimeType = ModType("WorkStudio.WorkTypeRuntime", true);
        Type modType = ModType("WorkStudio.WorkStudioMod", true);
        Type settingsType = ModType("WorkStudio.WorkStudioSettings", true);
        if (runtimeType == null || modType == null || settingsType == null)
        {
            Skip("hide-a-column regression: WorkTypeRuntime, WorkStudioMod or WorkStudioSettings not found");
            return;
        }

        try
        {
            RunHideColumnRegression(runtimeType, modType, settingsType);
        }
        catch (Exception e)
        {
            Exception inner = Innermost(e);
            Console.WriteLine("DIAG: " + e);
            Skip("hide-a-column regression: " + inner.GetType().Name + " - " + inner.Message);
        }
    }

    private static void RunHideColumnRegression(Type runtimeType, Type modType, Type settingsType)
    {
        // A WorkTypeDef the same way vanilla's own would sit in the database - not one of the
        // mod's own customTypes, so SyncCustomTypes leaves it alone, exactly like "Cleaning".
        var cleaning = new WorkTypeDef { defName = "PickleHideReproCleaning" };
        DefDatabase<WorkTypeDef>.Add(cleaning);

        // RebuildWorkColumns reads PawnTableDefOf.Work directly; nothing off-game ever sets it.
        var workTable = new PawnTableDef { columns = new List<PawnColumnDef>() };
        FieldInfo workOf = typeof(PawnTableDefOf).GetField("Work", BindingFlags.Public | BindingFlags.Static);
        workOf.SetValue(null, workTable);

        // A fresh settings object, standing in for WorkStudioMod.Settings, with nothing hidden yet.
        object settings = Activator.CreateInstance(settingsType);
        FieldInfo settingsBacking = modType.GetField("<Settings>k__BackingField", BindingFlags.NonPublic | BindingFlags.Static);
        settingsBacking.SetValue(null, settings);

        // A bare pawn, the same way TheAccessTheModMakes builds one - constructor-skipped, since
        // Pawn's real one needs a live Game. PriorityMemory.AllPawns() is already routed to a fake
        // list by that earlier check's Harmony patch, still active: reusing it here, rather than
        // patching AllPawns a second time, is what keeps this test from installing a duplicate patch.
        var pawn = (Pawn)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(Pawn));
        // Pawn.Equals reads def.defName - PriorityMemory.Restore keys its snapshot dictionary by
        // pawn, so even a bare pawn needs this much to be usable as a dictionary key at all.
        // ThingDef's own constructor hits the same ECall trap ThingDef always does off-game
        // (rimworld-tester-la-logique-hors-du-jeu): constructor-skipped, like everything else here.
        var pawnDef = (ThingDef)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(ThingDef));
        pawnDef.defName = "PickleHideReproPawnDef";
        pawn.def = pawnDef;
        var workSettings = (Pawn_WorkSettings)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(Pawn_WorkSettings));
        SetField(workSettings, "pawn", pawn);
        var priorities = (DefMap<WorkTypeDef, int>)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(DefMap<WorkTypeDef, int>));
        var values = new List<int> { 2 }; // Cleaning = 2, at the one and only index that exists here.
        SetField(priorities, "values", values);
        SetField(workSettings, "priorities", priorities);
        pawn.workSettings = workSettings;
        fakePawns = new List<Pawn> { pawn };

        // The action under test: hide the type, then run the exact reconciliation SetVisible's own
        // Commit() calls.
        var hiddenTypes = (System.Collections.IList)settingsType.GetField("hiddenTypes").GetValue(settings);
        hiddenTypes.Add(cleaning.defName);

        MethodInfo apply = AccessTools.Method(runtimeType, "Apply");
        apply.Invoke(null, null);

        Check("the type is actually hidden after Apply() (visible=false), so this is a real exercise of the hide path",
            !cleaning.visible);
        Check("hiding the type does not touch the priority the colonist already had for it",
            values[0] == 2);

        // And the reverse half of the scenario: showing it again keeps the priority too.
        hiddenTypes.Remove(cleaning.defName);
        apply.Invoke(null, null);

        Check("showing the type again still keeps the priority",
            values[0] == 2 && cleaning.visible);
    }

    // --- the editor's duplicate-label disambiguation ------------------------------------------

    // Two WorkGiverDefs can carry the same label and vanilla ships several - "construct placed
    // frames" belongs to both Construction and Art, "carry to growth vat" appears twice in Hauling.
    // On screen those rows were indistinguishable, in a column whose whole point is clicking one
    // rather than the other, so the editor now draws the defName beside a label it shows twice.
    // Which labels those are is a pure function, and therefore the one part of this that can be
    // checked without a GUI at all.
    private static void TheAmbiguousTaskLabels()
    {
        Console.WriteLine();
        Console.WriteLine("the duplicate-label disambiguation both task columns use:");

        Type dialogType = ModType("WorkStudio.Dialog_WorkTypes", true);
        MethodInfo ambiguousLabels = dialogType == null ? null : AccessTools.Method(dialogType, "AmbiguousLabels");
        if (ambiguousLabels == null)
        {
            Skip("Dialog_WorkTypes.AmbiguousLabels: not found");
            return;
        }

        var shared = new List<WorkGiverDef>
        {
            new WorkGiverDef { defName = "PickleFramesConstruction", label = "construct placed frames" },
            new WorkGiverDef { defName = "PickleFramesArt", label = "construct placed frames" },
            new WorkGiverDef { defName = "PickleCleanFilth", label = "clean filth" },
        };

        HashSet<string> found;
        try
        {
            found = (HashSet<string>)ambiguousLabels.Invoke(null, new object[] { shared });
        }
        catch (Exception e)
        {
            Skip("AmbiguousLabels: " + Innermost(e).GetType().Name + " - " + Innermost(e).Message);
            return;
        }

        Check("a label two tasks share is reported ambiguous", found.Count == 1);
        Check("the label reported is the shared one, capitalised the way the column draws it",
            found.Contains("Construct placed frames"));
        Check("a label only one task carries is left alone", !found.Contains("Clean filth"));

        var alone = (HashSet<string>)ambiguousLabels.Invoke(null,
            new object[] { new List<WorkGiverDef> { shared[2] } });
        Check("a list with no duplicate at all reports none", alone.Count == 0);
    }

    // --- middle truncation, and the collisions it can create ------------------------------------

    // The French run of 2026-09-21 put four rows reading "Apporter les ressources ..." in the
    // right-hand column, two of them sharing their grey type as well: labels that differ only in
    // their tail, cut at the end. The editor now cuts out of the middle instead, and looks for
    // duplicates among the strings it DRAWS rather than among the full labels - the old check
    // compared full labels and therefore could not see a collision that truncation had created.
    //
    // Measuring text needs a font, which needs Unity, so the truncation takes its measure as an
    // argument. Here that is a fake one - every character the same width - which is enough to test
    // the algorithm: what it keeps, what it drops, and that it stops rather than loops.
    private static void TheMiddleTruncation()
    {
        Console.WriteLine();
        Console.WriteLine("the editor's middle truncation, and the duplicates it must still catch:");

        Type dialogType = ModType("WorkStudio.Dialog_WorkTypes", true);
        MethodInfo truncate = dialogType == null ? null : AccessTools.Method(dialogType, "TruncateMiddle");
        MethodInfo duplicates = dialogType == null ? null : AccessTools.Method(dialogType, "Duplicates");
        if (truncate == null || duplicates == null)
        {
            Skip("Dialog_WorkTypes.TruncateMiddle/Duplicates: not found");
            return;
        }

        Func<string, float> perChar = text => text.Length * 10f;
        Func<string, float, string> cut = (text, width) =>
            (string)truncate.Invoke(null, new object[] { text, width, perChar });

        Check("a label that fits is left exactly as it is",
            cut("deliver resources", 500f) == "deliver resources");

        string blueprints = cut("deliver resources to blueprints", 200f);
        string frames = cut("deliver resources to frames", 200f);

        Check("a label too long for its column is shortened to fit",
            perChar(blueprints) <= 200f && blueprints.Length < "deliver resources to blueprints".Length);
        Check("the cut is in the middle, so the row still starts with the words it started with",
            blueprints.StartsWith("deliver"));
        Check("the tail survives, which is the whole point - it is what tells two rows apart",
            blueprints.EndsWith("blueprints") && frames.EndsWith("frames"));
        Check("two labels differing only in their tail stay different once shortened",
            blueprints != frames);

        // The end-cut this replaced, reproduced: both would have read "deliver resources t...".
        Check("cutting the end instead would have made them equal - the defect this replaces",
            "deliver resources to blueprints".Substring(0, 17) == "deliver resources to frames".Substring(0, 17));

        Check("a column too narrow for anything gives up instead of looping",
            cut("deliver resources", 5f) == "" || cut("deliver resources", 5f) == "…");

        var found = (HashSet<string>)duplicates.Invoke(null, new object[]
        {
            new List<string> { "Apporter les res…plans", "Apporter les res…plans", "Apporter les res…frames" }
        });
        Check("two rows that DRAW the same string are reported, whatever their full labels were",
            found.Count == 1 && found.Contains("Apporter les res…plans"));

        var distinct = (HashSet<string>)duplicates.Invoke(null, new object[]
        {
            new List<string> { "a", "b", "c" }
        });
        Check("rows that draw differently are left alone", distinct.Count == 0);
    }

    // A created type's defName is "WorkStudio_Type<n>". Taking the first free n gave a new type the
    // name of one just deleted, and whatever else keys its data by defName - Enhanced Work Tab's
    // per-colonist priorities, which live in the save and are never pruned - handed the new type the
    // old one's value. The counter must only ever go up.
    private static void TheTypeIdsAreNeverReused()
    {
        Console.WriteLine();
        Console.WriteLine("a created type never takes the name of a deleted one:");

        Type dialogType = ModType("WorkStudio.Dialog_WorkTypes", true);
        MethodInfo next = dialogType == null ? null : AccessTools.Method(dialogType, "NextTypeIndex");
        if (next == null)
        {
            Skip("Dialog_WorkTypes.NextTypeIndex: not found");
            return;
        }

        Func<int, string[], int> nextIndex = (last, ids) => (int)next.Invoke(null, new object[] { last, ids });

        Check("with nothing yet, the first is 1", nextIndex(0, new string[0]) == 1);
        Check("after 1 and 2 were issued, the next is 3",
            nextIndex(2, new[] { "WorkStudio_Type1", "WorkStudio_Type2" }) == 3);
        Check("after 1 was deleted, the next is still 3 and not 1 - the defect this replaces",
            nextIndex(2, new[] { "WorkStudio_Type2" }) == 3);
        Check("after the LAST one was deleted, the counter keeps the number: the next is 3, not 2",
            nextIndex(2, new[] { "WorkStudio_Type1" }) == 3);
        Check("a setup made before the counter existed starts above its highest type",
            nextIndex(0, new[] { "WorkStudio_Type1", "WorkStudio_Type5" }) == 6);
        Check("the counter wins over the types that exist when it is higher",
            nextIndex(9, new[] { "WorkStudio_Type2" }) == 10);
        Check("names that are not ours, or carry no number, are ignored",
            nextIndex(0, new[] { "Doctor", "WorkStudio_TypeX", null, "WorkStudio_Type" }) == 1);
    }

    // --- the icon patch targets, and the drawings themselves -----------------------------------

    // Both targets are resolved by reflection at startup, one of them with a fall-back, so a
    // signature change in a RimWorld update would show as icons quietly not drawing rather than as
    // an error. These are the checks that would go red instead.
    private static void TheIconPatchTargets()
    {
        Console.WriteLine();
        Console.WriteLine("the skill and work type icon patch targets:");

        MethodInfo drawSkill = AccessTools.Method(typeof(SkillUI), nameof(SkillUI.DrawSkill), new[]
        {
            typeof(SkillRecord), typeof(Rect), typeof(SkillUI.SkillDrawMode), typeof(string)
        });
        Check("SkillUI.DrawSkill(SkillRecord, Rect, SkillDrawMode, string) still exists", drawSkill != null);

        // The prefix takes the rect by reference to shrink it: by position, since Harmony injects
        // by name and a renamed parameter would silently stop the patch from receiving it.
        if (drawSkill != null)
        {
            ParameterInfo[] parameters = drawSkill.GetParameters();
            Check("its second parameter is the rect the prefix shrinks",
                parameters.Length > 1 && parameters[1].ParameterType == typeof(Rect));
        }

        MethodInfo doHeader = AccessTools.DeclaredMethod(typeof(PawnColumnWorker_WorkPriority), "DoHeader")
                              ?? AccessTools.Method(typeof(PawnColumnWorker), "DoHeader");
        Check("a DoHeader to patch is found, declared or inherited", doHeader != null);
        Console.WriteLine("    (found on " + (doHeader == null ? "nothing" : doHeader.DeclaringType.Name)
            + (doHeader != null && doHeader.DeclaringType == typeof(PawnColumnWorker)
                ? " - the fall-back, so every column is patched: harmless, see WorkTypeIcons)" : ")"));
    }

    // Every icon is named after the def it belongs to, and nothing at runtime says a name is wrong -
    // ContentFinder simply returns null and the column draws bare. The shipped names are checked
    // against the reference assembly's own defNames instead, which is the only place off-game that
    // knows them. A name that belongs to no def would be a drawing nobody ever sees.
    private static void TheIconNames()
    {
        Console.WriteLine();
        Console.WriteLine("the shipped icons are named after real defs:");

        string skills = Path.Combine(modFolder, "Textures", "WorkStudio", "Skills");
        string workTypes = Path.Combine(modFolder, "Textures", "WorkStudio", "WorkTypes");
        Check("Textures/WorkStudio/Skills exists", Directory.Exists(skills));
        Check("Textures/WorkStudio/WorkTypes exists", Directory.Exists(workTypes));
        if (!Directory.Exists(skills) || !Directory.Exists(workTypes)) return;

        string[] skillNames = Directory.GetFiles(skills, "*.png").Select(Path.GetFileNameWithoutExtension).ToArray();
        string[] typeNames = Directory.GetFiles(workTypes, "*.png").Select(Path.GetFileNameWithoutExtension).ToArray();

        Console.WriteLine("    " + skillNames.Length + " skill icon(s), " + typeNames.Length + " work type icon(s)");
        Check("some skill icons are shipped", skillNames.Length > 0);
        Check("some work type icons are shipped", typeNames.Length > 0);

        // DefDatabase is empty off-game, so the defNames come from the DefOf classes, which name
        // every vanilla one as a static field.
        var knownSkills = new HashSet<string>(typeof(SkillDefOf)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(f => f.FieldType == typeof(SkillDef)).Select(f => f.Name));
        var knownTypes = new HashSet<string>(typeof(WorkTypeDefOf)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(f => f.FieldType == typeof(WorkTypeDef)).Select(f => f.Name));

        string[] strayS = skillNames.Where(n => !knownSkills.Contains(n)).OrderBy(n => n).ToArray();
        string[] strayT = typeNames.Where(n => !knownTypes.Contains(n)).OrderBy(n => n).ToArray();

        // A DefOf class does not name every def a DLC or another mod adds, so an unknown name is
        // reported rather than failed - what matters is seeing the list and recognising it.
        Check("every skill icon is named after a SkillDefOf field"
              + (strayS.Length > 0 ? "  (not in SkillDefOf: " + string.Join(", ", strayS) + ")" : ""),
            strayS.Length == 0);
        Console.WriteLine("    work type icons not named in WorkTypeDefOf (DLC or modded types, expected): "
            + (strayT.Length == 0 ? "none" : string.Join(", ", strayT)));
    }

    // --- the Pickle suite's step table, which is global across every mod ------------------------

    // Pickle matches a scenario line against EVERY loaded suite's steps at once, so two mods that
    // phrase a step identically make both of them ambiguous and neither runs. That happened on
    // 2026-09-19: this suite and Architect Studio's both declared "I click the button keyed
    // {string}", and five of their scenarios died on "Ambiguous step: Multiple matches found".
    // Nothing in a build catches it - the suites never see each other until the game loads them
    // together - so the rule is checked here instead: every step this suite declares must name
    // something of Work Studio's.
    private static readonly string[] DomainMarkers =
    {
        "work studio", "work type", "work tab", "task", "priority", "priorities"
    };

    // What Pickle itself provides. Listed rather than guessed: a step used in a feature and found
    // in neither place is a typo, and a typo only shows up as a failed scenario in a live run.
    private static readonly string[] PickleBuiltIns =
    {
        "I close all dialogs",
        "I open the {string} tab",
        "I spawn a {string} at ({int}, {int})",
        "I take a screenshot {string}",
        "I wait for {string} to have job {string}",
        "a colonist {string} exists",
        "game speed is {word}",
        "mod {string} is loaded",
        "mod {string} loads after {string}",
        "the save {string} is loaded",
        "window {string} is open",
    };

    private static void ThePickleStepTable()
    {
        Console.WriteLine();
        Console.WriteLine("the Pickle suite's steps, against a table shared with every other mod:");

        string root = Path.GetFullPath(Path.Combine(modFolder, ".."));
        string sourceFolder = Path.Combine(root, "Tests", "Pickle", "Source");
        string featureFolder = Path.Combine(root, "Tests", "Pickle", "Mod", "Pickle", "Features");
        if (!Directory.Exists(sourceFolder) || !Directory.Exists(featureFolder))
        {
            Skip("Tests/Pickle not found beside the mod folder");
            return;
        }

        var declared = new List<string>();
        foreach (string file in Directory.GetFiles(sourceFolder, "*.cs"))
        {
            foreach (Match m in Regex.Matches(File.ReadAllText(file),
                         "\\[(?:When|Then|Given)\\(\"([^\"]*)\""))
            {
                declared.Add(m.Groups[1].Value);
            }
        }

        Console.WriteLine("    " + declared.Count + " step(s) declared");
        Check("the suite declares steps at all", declared.Count > 0);

        string[] bare = declared
            .Where(s => !DomainMarkers.Any(marker => s.ToLowerInvariant().Contains(marker)))
            .OrderBy(s => s).ToArray();
        Check("every declared step names something of Work Studio's, so it cannot collide with "
              + "another mod's suite" + (bare.Length > 0 ? ": " + string.Join(" / ", bare) : ""),
            bare.Length == 0);

        // Steps of the shared tool this suite stages (PickleTools/ClickDiagnostics), read from its own
        // source so that a renamed step there turns a scenario red here, not in a live run. The
        // repository is a sibling of this one; without it those lines cannot be checked, and are
        // left out with a skip that says so rather than counted as typos.
        const string ToolPrefix = "Nelim's Pickle Tools: ";
        var shared = new List<string>();
        string toolSource = Path.GetFullPath(Path.Combine(root, "..", "PickleTools", "ClickDiagnostics", "Source"));
        if (Directory.Exists(toolSource))
        {
            foreach (string file in Directory.GetFiles(toolSource, "*.cs"))
            {
                foreach (Match m in Regex.Matches(File.ReadAllText(file),
                             "\\[(?:When|Then|Given)\\(\"([^\"]*)\""))
                {
                    shared.Add(m.Groups[1].Value);
                }
            }

            Console.WriteLine("    " + shared.Count + " step(s) read from PickleTools/ClickDiagnostics");
            Check("the shared tool declares steps at all", shared.Count > 0);
        }

        Regex[] known = declared.Concat(PickleBuiltIns).Concat(shared).Select(StepPattern).ToArray();
        var unknown = new SortedSet<string>();
        int notChecked = 0;

        foreach (string file in Directory.GetFiles(featureFolder, "*.feature"))
        {
            foreach (string raw in File.ReadAllLines(file))
            {
                string line = raw.Trim();
                Match keyword = Regex.Match(line, "^(Given|When|Then|And|But)\\s+(.*)$");
                if (!keyword.Success) continue;

                string step = keyword.Groups[2].Value;
                if (shared.Count == 0 && step.StartsWith(ToolPrefix, StringComparison.Ordinal))
                {
                    notChecked++;
                    continue;
                }

                if (!known.Any(pattern => pattern.IsMatch(step)))
                {
                    unknown.Add(Path.GetFileName(file) + ": " + step);
                }
            }
        }

        if (notChecked > 0)
        {
            Skip(notChecked + " step line(s) of the shared tool: PickleTools/ClickDiagnostics not found beside this repository");
        }

        Check("every step a feature file uses is declared here or is a known Pickle built-in"
              + (unknown.Count > 0 ? "\n          " + string.Join("\n          ", unknown) : ""),
            unknown.Count == 0);
    }

    /// <summary>Cucumber's placeholders, as the regex Pickle matches a scenario line with.</summary>
    private static Regex StepPattern(string step)
    {
        string pattern = Regex.Escape(step)
            .Replace("\\{string}", "\"[^\"]*\"")
            .Replace("\\{int}", "-?\\d+")
            .Replace("\\{word}", "\\S+")
            .Replace("\\(s\\)", "s?");
        return new Regex("^" + pattern + "$");
    }

    // --- silencing Verse.Log ------------------------------------------------------------------

    // Verse.Log.Error/Warning/Message all end in UnityEngine.Debug.Log*, an ECall with no native
    // implementation outside a running Unity player: the CLR refuses to even JIT the calling method,
    // regardless of whether the log call was itself expected or a real complaint. ScribeLoader logs
    // routinely as part of its own bookkeeping (mode guards, cross-reference housekeeping), so any
    // check that drives Scribe directly needs these silenced first, globally, once.
    private static void SilenceLog()
    {
        var silencer = new Harmony("nelim.workstudio.tests.log");
        HarmonyMethod noop = new HarmonyMethod(typeof(Program), nameof(SkipPrefix));
        foreach (MethodInfo m in typeof(Log).GetMethods(BindingFlags.Public | BindingFlags.Static)
                     .Where(m => m.Name == "Message" || m.Name == "Warning" || m.Name == "Error"
                                 || m.Name == "WarningOnce" || m.Name == "ErrorOnce"))
        {
            silencer.Patch(m, prefix: noop);
        }
    }

    // --- plumbing --------------------------------------------------------------------------------

    private static Type ModType(string fullName, bool quiet = false)
    {
        Type t = mod.GetType(fullName, false);
        if (!quiet) Check(fullName + " is in the assembly", t != null);
        return t;
    }

    private static bool SetField(object target, string name, object value)
    {
        FieldInfo f = Field(target.GetType(), name);
        if (f == null) return false;
        f.SetValue(target, value);
        return true;
    }

    private static T GetField<T>(object target, string name)
    {
        FieldInfo f = Field(target.GetType(), name);
        return f == null ? default(T) : (T)f.GetValue(target);
    }

    // GetField does not look into base classes for a private field.
    private static FieldInfo Field(Type type, string name)
    {
        for (Type t = type; t != null; t = t.BaseType)
        {
            FieldInfo f = t.GetField(name,
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            if (f != null) return f;
        }
        return null;
    }

    private static Exception Innermost(Exception e)
    {
        while (e.InnerException != null) e = e.InnerException;
        return e;
    }

    private static string Metadata(string key)
    {
        return Assembly.GetExecutingAssembly()
            .GetCustomAttributes<AssemblyMetadataAttribute>()
            .First(a => a.Key == key)
            .Value;
    }

    private static void Check(string what, bool ok)
    {
        Console.WriteLine((ok ? "  PASS  " : "  FAIL  ") + what);
        if (!ok) failures++;
    }

    private static void Skip(string what)
    {
        Console.WriteLine("  SKIP  " + what);
        skips++;
    }
}
