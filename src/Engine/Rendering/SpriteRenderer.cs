﻿using Joonaxii.Engine.Components;
using Joonaxii.Engine.Core;
using Joonaxii.MathJX;
using System;

namespace Joonaxii.Physics.Demo.Rendering
{
    public class SpriteRenderer : Component, IComparable<SpriteRenderer>
    {
        public Action onBecomeVisible;
        public Action onBecomeInvisible;

        public bool FlipX { get; set; }
        public bool FlipY { get; set; }

        public Rect Bounds { get => _bounds; }

        public bool IsVisible { get; private set; }

        public SortingLayer layer;
        public Sprite sprite;

        private Rect _bounds;

        private Transform _transform;
        private TransformConstraints _constraints;

        public SpriteRenderer(Transform transform)
        {
            _transform = transform;
            _constraints = TransformConstraints.None;
            TXTRenderer.Instance.RegisterRenderer(this);
        }

        public void SetTransformConstraints(TransformConstraints constraints)
        {
            _constraints = constraints;
        }

        public int CompareTo(SpriteRenderer other)
        {
            return other.layer.CompareTo(layer);
        }

        protected override void OnInstantiate<T>(T source)
        {
            base.OnInstantiate(source);
            if(source is SpriteRenderer sp)
            {
                FlipX = sp.FlipX;
                FlipY = sp.FlipY;

                _bounds = sp._bounds;

                IsVisible = sp.IsVisible;

                layer = sp.layer;
                sprite = sp.sprite;

                _constraints = sp._constraints;
            }
        }

        protected override void SetGameObject(GameObject go)
        {
            base.SetGameObject(go);
            _transform = go.Transform;
        }

        public void Draw(char[] buffer, uint[] depthBuffer)
        {
            if (!_enabled || sprite == null || _transform == null) { return; }

            var mat = _transform.WorldMatrix;
            _bounds = sprite.Bounds;

            var sprtCent = mat.MultiplyPoint(_bounds.Center);
            var sprtSize = mat.MultiplyAbsVector(_bounds.Size);

            _bounds.Set(sprtCent, sprtSize);
            if (!_bounds.Overlaps(TXTRenderer.Instance.Bounds))
            {
                if (IsVisible)
                {
                    IsVisible = false;
                    onBecomeInvisible?.Invoke();
                }
                return;
            }

            if (!IsVisible)
            {
                IsVisible = true;
                onBecomeVisible?.Invoke();
            }

            sprite.Draw(_transform, _constraints, buffer, depthBuffer, layer.Union, FlipX, FlipY);
        }
    }
}
