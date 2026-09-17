// Krafs.Publicizer publicises the reference assembly, which is what lets WorkTypeRuntime and
// PriorityMemory read Pawn_WorkSettings.priorities and write column.workerInt,
// settings.workGiversDirty, settings.CacheWorkGiversInOrder() and backstory.cachedDisabledWorkTypes
// at all. In the real Assembly-CSharp those are private: the compiler emits a plain cross-assembly
// ldfld/callvirt either way, and the CLR allows that instruction only when this assembly declares
// the waiver below.
//
// Publicizer defines the attribute type for us and normally applies it through the SDK's
// generated AssemblyInfo — which this project switches off with GenerateAssemblyInfo=false. The
// type was therefore embedded and the waiver was not. Nothing said so: the build stayed clean, and
// the very first Apply() at startup would have thrown on its first pawn, silently taking priority
// protection down with it.
//
// Tests/ModTests.cs catches this by performing the access rather than by reading metadata, which
// is how it was found without launching the game.

[assembly: System.Runtime.CompilerServices.IgnoresAccessChecksTo("Assembly-CSharp")]
