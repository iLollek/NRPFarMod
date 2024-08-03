using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRPFarmod.MelonCall {
    public abstract class  MelonCaller {
        public virtual void OnApplicationLateStart() { }
        public virtual void OnUpdate(){}
        public virtual void OnFixedUpdate(){}
        public virtual void OnLateUpdate(){}
        public virtual void OnApplicationQuit(){}
        public virtual void OnApplicationStart(){}
        public virtual void OnDeinitializeMelon(){}
        public virtual void OnEarlyInitializeMelon(){}
        public virtual void OnGUI(){}
        public virtual void OnInitializeMelon(){}
        public virtual void OnLateInitializeMelon(){}
        public virtual void OnLevelWasInitialized(int level){}
        public virtual void OnLevelWasLoaded(int level){}
        public virtual void OnModSettingsApplied(){}
        public virtual void OnPreferencesLoaded(){}
        public virtual void OnPreferencesLoaded(string filepath){}
        public virtual void OnPreferencesSaved(){}
        public virtual void OnPreferencesSaved(string filepath){}
        public virtual void OnPreSupportModule(){}
        public virtual void OnSceneWasInitialized(int buildIndex, string sceneName){}
        public virtual void OnSceneWasLoaded(int buildIndex, string sceneName){}
        public virtual void OnSceneWasUnloaded(int buildIndex, string sceneName){}
        public virtual void OnMelonCallerLoaded() { }

    }
}
