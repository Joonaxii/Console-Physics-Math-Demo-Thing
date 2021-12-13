using Joonaxii.Engine.Components;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Joonaxii.Engine.Core
{
    public sealed class GameObject : Object
    {
        public bool IsActive 
        {
            get => _active;
            set
            {
                bool prev = _active;
                _active = value;

                if(_active != prev)
                {
                    for (int i = 0; i < _transform.ChildCount; i++)
                    {
                        var tr = _transform.GetChild(i);
                        tr.GameObject.RecursiveSetActiveHierarchy(_active);
                    }
                }
            }
        }
        private bool _active = true;

        public bool IsActiveInHierarchy
        {
            get => _activeInHierarchy & _active;
        }
        private bool _activeInHierarchy = true;

        private static MethodInfo _setGo { get; } = typeof(Component).GetMethod("SetGameObject", BindingFlags.NonPublic | BindingFlags.Instance);

        public Transform Transform { get => _transform; }
        private Transform _transform;

        private List<Component> _components = new List<Component>();

        public GameObject() : this("New GameObject") { }
        public GameObject(string name) : base(name)
        {
            _components = new List<Component>();
            _transform = AddComponent<Transform>();
        }

        public GameObject(string name, params Type[] components) : base(name)
        {
            _components = new List<Component>();
            _transform = AddComponent<Transform>();

            if (components != null)
            {
                for (int i = 0; i < components.Length; i++)
                {
                    var comp = components[i];
                    if (comp == null) { continue; }
                    AddComponent(comp);
                }
            }
        }

        public bool TryGetComponent<T>(out T component) where T : Component
        {
            for (int i = 0; i < _components.Count; i++)
            {
                if(_components[i] is T val) { component = val; return true; }
            }

            component = null;
            return false;
        }

        public T AddComponent<T>() where T : Component
        {
            T val = Instantiate<T>();
            _setGo.Invoke(val, new object[] { this });
            _components.Add(val);
            return val;
        }

        public Component AddComponent(Type type)
        {
            Component val = Instantiate(type) as Component;
            if (val == null) { return null; }

            _setGo.Invoke(val, new object[] { this });
            _components.Add(val);
            return val;
        }

        private bool RemoveComponent(Component component) => _components.Remove(component);

        private void RecursiveSetActiveHierarchy(bool active)
        {
            active &= _active;
            _activeInHierarchy = active;
            for (int i = 0; i < _transform.ChildCount; i++)
            {
                _transform.GameObject.RecursiveSetActiveHierarchy(active);
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            for (int i = 0; i < _components.Count; i++)
            {
                Object comp = _components[i];
                Destroy(ref comp);
            }
        }
    }
}
