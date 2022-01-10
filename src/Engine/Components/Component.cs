using Joonaxii.Engine.Core;
using System.Reflection;

namespace Joonaxii.Engine.Components
{
    public class Component : Object
    {
        public override string Name 
        { 
            get => _go.Name;
            set { }
        }

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

        public Transform Transform { get => _go.Transform; }

        protected virtual void SetGameObject(GameObject go) => _go = go;

        public virtual void OnEnable() { }
        public virtual void OnDisable() { }

        public virtual void Update(float delta) { }

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

        public override string ToString() => $"{GetType().Name} {typeof(GameObject).Name}({Name})";
    }
}
