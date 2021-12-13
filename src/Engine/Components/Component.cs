using Joonaxii.Engine.Core;
using System.Reflection;

namespace Joonaxii.Engine.Components
{
    public class Component : Object
    {
        private static MethodInfo _removeComp { get; } = typeof(GameObject).GetMethod("RemoveComponent", BindingFlags.NonPublic | BindingFlags.Instance);

        public bool Enabled 
        { 
            get => _enabled & _go.IsActiveInHierarchy; 
            set
            {
                bool prev = _enabled;

                _enabled = value;
                if(prev != _enabled)
                {
                    TriggerEnable();
                }
            }
        }
        protected bool _enabled = true;

        public GameObject GameObject { get => _go; }
        protected GameObject _go;

        protected virtual void SetGameObject(GameObject go) => _go = go;

        public virtual void OnEnable() { }
        public virtual void OnDisable() { }

        protected override void OnInstantiate<T>(T source)
        {
            base.OnInstantiate(source);
            if(source is Component c)
            {
                Enabled = c._enabled;
                TriggerEnable();
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _removeComp.Invoke(_go, new object[] { this });
        }

        protected void TriggerEnable()
        {
            if (_enabled) { OnEnable(); return; }
            OnDisable();
        }
    }
}
