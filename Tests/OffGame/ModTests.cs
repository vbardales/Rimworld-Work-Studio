using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using HarmonyLib;
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

    private static void Main(string[] args)
    {
        string managed = args.Length > 0 ? args[0] : Metadata("RimWorldManaged");
        string modAssembly = args.Length > 1 ? args[1] : Metadata("ModAssembly");
        modFolder = args.Length > 2 ? args[2] : Metadata("ModFolder");

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
        TheAccessTheModMakes();
        TheHarmonyPatchTargets();
        TheMainButtonsShortcut();
        ThePriorityFallback();
        TheConfigRoundTrip();
        TheKeyedCoverage();
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
