using Il2CppSystem.IO;
using MelonLoader;
using NRPFarmod;
using NRPFarmod.MelonCall;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

[assembly: MelonInfo(typeof(NRPFarMod), "NRPFarModLib", "1.0.3", "Farliam & iLollek")]
[assembly: MelonGame("PLANET JEM SOFTWARE", "NIGHT-RUNNERS PROLOGUE")]
[assembly: MelonGame("PLANET JEM SOFTWARE", "NIGHT-RUNNERS PROLOGUE PATREON")]

namespace NRPFarmod {
    public class NRPFarMod : MelonMod {

        private readonly IList<MelonCaller> subModules;

        public NRPFarMod() {
            subModules = new List<MelonCaller>();
            Assembly assembly = Assembly.GetExecutingAssembly();
            var mods = assembly.GetTypes().Where(type => type.IsAssignableTo(typeof(MelonCaller)) && type.BaseType == typeof(MelonCaller)).ToList();
            MelonLogger.Msg($"Gefundene Module: \u001b[32m{mods.Count}\u001b[0m");
            foreach (var type in mods) {
                try {
                    var subModul = Activator.CreateInstance(type);
                    if(subModul != null) subModules.Add(subModul as MelonCaller ?? throw new Exception("Cast Exception"));
                    MelonLogger.Msg($"CreateInstance: {type}");
                } catch (Exception ex) {
                    MelonLogger.Error($"Fehler beim Instanziieren von {type.Name}: {ex.Message}");
                }
            }
            foreach(var module in subModules) {
                module.OnMelonCallerLoaded();
            }
        }

        public override void OnApplicationLateStart() {
            foreach(var modul in subModules)
                modul.OnApplicationLateStart();
        }

        public override void OnUpdate() {
            foreach (var modul in subModules)
                modul.OnUpdate();
        }

        public override void OnFixedUpdate() {
            foreach (var modul in subModules)
                modul.OnFixedUpdate();
        }

        public override void OnLateUpdate() {
            foreach (var modul in subModules)
                modul.OnLateUpdate();
        }

        public override void OnApplicationQuit() {
            foreach (var modul in subModules)
                modul.OnApplicationQuit();
        }

        public override void OnApplicationStart() {
            foreach (var modul in subModules)
                modul.OnApplicationStart();
        }

        public override void OnDeinitializeMelon() {
            foreach (var modul in subModules)
                modul.OnDeinitializeMelon();
        }

        public override void OnEarlyInitializeMelon() {
            foreach (var modul in subModules)
                modul.OnEarlyInitializeMelon();
        }

        public override void OnGUI() {
            foreach (var modul in subModules)
                modul.OnGUI();
        }

        public override void OnInitializeMelon() {
            foreach (var modul in subModules)
                modul.OnInitializeMelon();
        }

        public override void OnLateInitializeMelon() {
            foreach (var modul in subModules)
                modul.OnLateInitializeMelon();
        }

        public override void OnLevelWasInitialized(int level) {
            foreach (var modul in subModules)
                modul.OnLevelWasInitialized(level);
        }
        public override void OnLevelWasLoaded(int level) {
            foreach (var modul in subModules)
                modul.OnLevelWasLoaded(level);
        }

        public override void OnModSettingsApplied() {
            foreach (var modul in subModules)
                modul.OnModSettingsApplied();
        }

        public override void OnPreferencesLoaded() {
            base.OnPreferencesLoaded();
            foreach (var modul in subModules)
                modul.OnPreferencesLoaded();
        }

        public override void OnPreferencesLoaded(string filepath) {
            base.OnPreferencesLoaded(filepath);
            foreach (var modul in subModules)
                modul.OnPreferencesLoaded(filepath);
        }

        public override void OnPreferencesSaved() {
            base.OnPreferencesSaved();
            foreach (var modul in subModules)
                modul.OnPreferencesSaved();
        }

        public override void OnPreferencesSaved(string filepath) {
            base.OnPreferencesSaved(filepath);
            foreach (var modul in subModules)
                modul.OnPreferencesSaved(filepath);
        }

        public override void OnPreSupportModule() {
            base.OnPreSupportModule();
            foreach (var modul in subModules)
                modul.OnPreSupportModule();
        }

        public override void OnSceneWasInitialized(int buildIndex, string sceneName) {
            base.OnSceneWasInitialized(buildIndex, sceneName);
            foreach (var modul in subModules)
                modul.OnSceneWasInitialized(buildIndex, sceneName);
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName) {
            base.OnSceneWasLoaded(buildIndex, sceneName);
            foreach (var modul in subModules)
                modul.OnSceneWasLoaded(buildIndex, sceneName);
        }

        public override void OnSceneWasUnloaded(int buildIndex, string sceneName) {
            base.OnSceneWasUnloaded(buildIndex, sceneName);
            foreach (var modul in subModules)
                modul.OnSceneWasUnloaded(buildIndex, sceneName);
        }
    }
}
