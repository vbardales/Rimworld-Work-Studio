# Why this folder exists

This folder is not a translation. It exists so that RimWorld stops logging

    Mod Work Studio - Pickle tests did not load any content.
    Following load folders were used: ...

at every start.

`LoadedModManager.LoadModContent` (1.6) logs that error for any active mod whose
`ModContentPack.AnyContentLoaded()` returns false. That method is satisfied by exactly one of:
a loaded texture, audio clip, `Strings/` entry, assembly, asset bundle, a `Patches/` operation,
a Def — or, through `AnyTranslationsLoaded()`, *any file at all* under a `Languages/` folder:

```csharp
public bool AnyTranslationsLoaded()
{
    foreach (string item in foldersToLoadDescendingOrder)
    {
        string path = Path.Combine(item, "Languages");
        if (Directory.Exists(path) && Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories).Any())
            return true;
    }
    return false;
}
```

This companion mod ships only `About/`, feature files and a steps assembly under `Pickle/`.
RimWorld loads none of those itself — Pickle does — so none of them count, hence the error.

Every other way to satisfy the check puts something into a running game: a Def, a patch
operation, a texture, an assembly loaded a second time by vanilla on top of the one Pickle
already loads. A file under `Languages/` puts nothing anywhere. `LanguageDatabase.InitAllMetadata`
and `LoadedLanguage.AllDirectories` both enumerate *directories* under `Languages/`
(`SearchOption.TopDirectoryOnly`), never loose files, so this file is never opened, never parsed,
and no translation key enters any database. It is read once, as a directory entry, by the
`EnumerateFiles` call above.

Do not add a language subfolder here. The moment `Languages/<Language>/` exists, its keyed and
DefInjected files are loaded for real, and this companion stops being inert.
