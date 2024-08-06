using Il2Cpp;
using MelonLoader;
using System;
using System.Diagnostics;
using UnityEngine;

namespace NRPFarmod.ContentManager {

    public class ManagedContent<T> : IDisposable where T : UnityEngine.Object {

        public List<double> MemoryInformation { get; private set; } = new();

        private T? content = null;

        public T? Value { get => content; }

        public readonly Type type = typeof(T);

        #region Load&Unload
        public virtual void LoadContent(Func<T> Loading) {
            if (content != null) {
                UnloadContent();
            }
            content = Loading?.Invoke();
        }

        public virtual void SetContet(T content) {
            if (content != null) {
                UnloadContent();
            }
            this.content = content;
        }

        public virtual void UnloadContent() {
            try {
                using (var proc = Process.GetCurrentProcess())
                    MemoryInformation.Add(proc.WorkingSet64 / (1024.0d * 1024.0d));
                if (content is AudioClip clip) {
                    GodConstant.Instance.musicSource.Stop();
                    Logger(GameObject.DestroyImmediate,clip, true);
                    Logger(Resources.UnloadAsset, clip);
                }
                using(var proc  = Process.GetCurrentProcess()) 
                    MemoryInformation.Add(proc.WorkingSet64 / (1024.0d * 1024.0d));
                ResetPreserveMinMax();
            } catch(Exception) {
                MelonLogger.Error($"Object null");
            }
        }
        #endregion

        public virtual void MemorySnapshot() {
            using (var proc = Process.GetCurrentProcess())
                MemoryInformation.Add(proc.WorkingSet64 / (1024.0d * 1024.0d));
            ResetPreserveMinMax();
        }

        public virtual void ResetPreserveMinMax() {
            if (MemoryInformation.Count > 200) {
                var range = MemoryInformation.GetRange(0, 50);
                var min = range.Min();
                var max = range.Max();
                MemoryInformation.RemoveRange(0, 50);
                MemoryInformation.Insert(0,min);
                MemoryInformation.Insert(0,max);
            }
        }

        #region Interface implementierung
        public void Dispose() => UnloadContent();
        #endregion

        #region Logging
        private void Logger<C>(Action<C> action, C param) {
            Exception? ex = null;
            try {
                action(param);
            } catch (Exception e) {
                ex = e;
            }
#if DEBUG
            if (ex == null) {
                MelonLogger.Msg($"Execute {action.Method.Name} -> \u001b[32m(void)ok\u001b[0m");
            } else {
                MelonLogger.Error(ex);
            }
#endif
        }

        private void Logger<C,D>(Action<C,D> action, C param, D param_) {
            Exception? ex = null;
            try {
                action(param, param_);
            } catch (Exception e) {
                ex = e;
            }
#if DEBUG
            if (ex == null) {
                MelonLogger.Msg($"Execute {action.Method.Name} -> \u001b[32m(void)ok\u001b[0m");
            } else {
                MelonLogger.Error(ex);
            }
#endif
        }

        private bool? Logger(Func<bool> func) {
            bool? result = null;
            Exception? ex = null;
            try {
                result = func();
            } catch (Exception e) {
                result = null;
                ex = e;
            }
#if DEBUG
            if (ex != null) {
                MelonLogger.Error($"{func.Method.Name} throw {ex.Message}");
            } else if (result is true) {
                MelonLogger.Msg($"{func.Method.Name} result \u001b[32m{result}\u001b[0m");
            } else if (result is false) {
                MelonLogger.Msg($"{func.Method.Name} result \u001b[31m{result}\u001b[0m");
            }
#endif
            return result;
        }
        #endregion

    }
}
