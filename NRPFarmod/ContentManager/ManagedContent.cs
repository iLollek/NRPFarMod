using MelonLoader;
using System;
using UnityEngine;

namespace NRPFarmod.ContentManager {

    public class ManagedContent<T> : IDisposable where T : UnityEngine.Object {

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
            if (content is AudioClip clip) {
                Logger(clip.UnloadAudioData);
                Logger(UnityEngine.Object.DestroyImmediate, clip);
            }
            Logger(Resources.UnloadAsset, content);
            Logger(UnityEngine.Object.DestroyImmediate, content);
            GC.Collect();
            GC.WaitForPendingFinalizers();
            content = null;
        }
        #endregion

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
