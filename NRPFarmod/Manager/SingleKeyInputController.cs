using MelonLoader;
using NRPFarmod.MelonCall;

namespace NRPFarmod {

    

    public class SingleKeyInputController : MelonCaller {

        #region Instanz
        private static object _lock = new object();
        private static SingleKeyInputController? _instanz = null;

        public static SingleKeyInputController? Instanz {
            get => LockMe();
        }

        private static SingleKeyInputController? LockMe() {
            lock (_lock) {
                return _instanz;
            }
        }

        public SingleKeyInputController() {
            _instanz = this;
        }
        #endregion

        /// <summary>
        /// Last pressed Keys
        /// </summary>
        private UnityEngine.KeyCode lastKey = UnityEngine.KeyCode.None;
        /// <summary>
        /// Indicates whether a key is pressed
        /// </summary>
        private bool IsKeyDown = false;
        /// <summary>
        /// Void
        /// </summary>
        private Dictionary<UnityEngine.KeyCode, Action> inputActions = new();

        /// <summary>
        /// Adds a new action
        /// </summary>
        /// <param name="key"></param>
        /// <param name="action"></param>
        public void AddKeyCallback(UnityEngine.KeyCode key, Action action) {
            inputActions.Add(key, action);
        }
        /// <summary>
        /// Removes an already existing action
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool Remove(UnityEngine.KeyCode key) {
            return inputActions.Remove(key);
        }

        public override void OnUpdate() {
            if (!IsKeyDown) {
                foreach (var pair in inputActions) {
                    if (UnityEngine.Input.GetKeyDown(pair.Key)) {
                        IsKeyDown = true;
                        lastKey = pair.Key;
                        return;
                    }
                }
            }
            if (!UnityEngine.Input.GetKeyDown(lastKey) && lastKey != UnityEngine.KeyCode.None) {
                inputActions[lastKey]?.Invoke();
                IsKeyDown = false;
                lastKey = UnityEngine.KeyCode.None;
            }
        }
    }
}

