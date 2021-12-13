using Joonaxii.MathJX;
using System.Collections.Generic;
using Joonaxii.Engine.Core;

namespace Joonaxii.Engine.Components
{
    public class Transform : Component
    {
        public Transform Parent
        {
            get => _parent;
            set
            {
                var prev = _parent;
                _parent = value;

                if (prev != _parent)
                {
                    prev?._children.Remove(this);
                    UpdateWorld(true);
                    _parent?._children.Add(this);
                }
            }
        }
        private Transform _parent;

        public Vector2 LocalPosition
        {
            get => _localPosition;
            set
            {
                _localPosition = value;
                _localMatrix.SetPosition(_localPosition);
                _worldMatrix.SetPosition(_worldPosition = _parent != null ? _parent._worldMatrix.MultiplyPoint(_localPosition) : _localPosition);
                NotifyChildren();
            }
        }
        private Vector2 _localPosition;

        public float LocalRotation
        {
            get => _localRotation;
            set
            {
                _localRotation = value;

                _localMatrix.SetRotation(_localRotation);
                _worldMatrix.SetRotation(_worldRotation = _parent != null ? _parent._worldMatrix.Rotate(_localRotation) : _localRotation);

                NotifyChildren();
            }
        }
        private float _localRotation;

        public Vector2 LocalScale
        {
            get => _localScale;
            set
            {
                _localScale = value;

                _localMatrix.SetScale(_localScale);
                _worldMatrix.SetScale(_worldScale = _parent != null ? _parent._worldMatrix.ScaleVector(_localScale) : _localScale);

                NotifyChildren();
            }
        }
        private Vector2 _localScale;

        public Vector2 WorldPosition
        {
            get => _worldPosition;
            set
            {
                _worldPosition = value;

                _worldMatrix.SetPosition(_worldPosition);
                _localMatrix.SetPosition(_localPosition = _parent != null ? _worldMatrix.InversePosition() : _worldPosition);

                NotifyChildren();
            }
        }
        private Vector2 _worldPosition;

        public float WorldRotation
        {
            get => _worldRotation;
            set
            {
                _worldRotation = value;

                _worldMatrix.SetRotation(_worldRotation);
                _localMatrix.SetRotation(_localRotation = _parent != null ? _worldMatrix.InverseRotation() : _worldRotation);

                NotifyChildren();
            }
        }
        private float _worldRotation;

        public Vector2 WorldScale
        {
            get => _worldScale;

        }
        private Vector2 _worldScale;

        public Matrix3x3 LocalMatrix { get => _localMatrix; }
        public Matrix3x3 WorldMatrix { get => _worldMatrix; }

        public int ChildCount { get => _children.Count; }

        private Matrix3x3 _localMatrix;
        private Matrix3x3 _worldMatrix;

        private List<Transform> _children = new List<Transform>();

        public Transform() { LocalPosition = Vector2.zero; LocalRotation = 0; LocalScale = Vector2.one; }

        public void AddChild(Transform tr)
        {
            _children.Add(tr);
            tr.UpdateWorld(true);
            tr._parent = this;
        }

        public void RemoveChild(Transform tr)
        {
            _children.Remove(tr);
            tr.UpdateWorld(true);
            tr._parent = null;
        }

        public Transform GetChild(int index) => _children[index];

        private void UpdateWorld(bool setLocalToWorld)
        {
            if (setLocalToWorld)
            {
                LocalPosition = _worldPosition;
                LocalRotation = _worldRotation;
                LocalScale = _worldScale;

                _localMatrix.SetTRS(_localPosition, _localRotation, _localScale);
            }

            if (_parent != null)
            {
                _worldMatrix.SetPosition(_worldPosition = _parent._worldMatrix.MultiplyPoint(_localPosition));
                _worldMatrix.SetRotation(_worldRotation = _parent._worldMatrix.Rotate(_localRotation));
                _worldMatrix.SetScale(_worldScale = _parent._worldMatrix.ScaleVector(_localScale));

                NotifyChildren();
                return;
            }

            _worldMatrix.SetPosition(_worldPosition = _localPosition);
            _worldMatrix.SetRotation(_worldRotation = _localRotation);
            _worldMatrix.SetScale(_worldScale = _localScale);

            NotifyChildren();
        }

        private void NotifyChildren()
        {
            for (int i = 0; i < _children.Count; i++)
            {
                _children[i].UpdateWorld(false);
            }
        }

        protected override void OnInstantiate<T>(T other)
        {       
            if (!(other is Transform tr)) { return; }
            base.OnInstantiate(tr);

            tr._localMatrix = _localMatrix;
            tr._worldMatrix = _worldMatrix;

            tr._localPosition = _localPosition;
            tr._localRotation = _localRotation;
            tr._localScale = _localScale;

            tr._worldPosition = _worldPosition;
            tr._worldRotation = _worldRotation;
            tr._worldScale = _worldScale;

            tr._children = new List<Transform>();
            for (int i = 0; i < _children.Count; i++)
            {
                tr._children[i] = Instantiate(_children[i]);
            }
        }

        protected override void OnDestroy()
        { 
            Destroy(ref _go);

            base.OnDestroy();
            for (int i = 0; i < _children.Count; i++)
            {
                Object tr = _children[i];
                Object.Destroy(ref tr);
            }
            _children.Clear();
        }
    }
}
