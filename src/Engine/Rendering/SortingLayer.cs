using System;
using System.Runtime.InteropServices;

namespace Joonaxii.Engine.Rendering
{
    [StructLayout(LayoutKind.Explicit, Size = 4)]
    public struct SortingLayer : IComparable<SortingLayer>
    {
        private static readonly string[] LAYERS = new string[] { "Default", "Background", "Foreground", "UI" };

        public uint Union { get => _union; }

        [FieldOffset(0)] private uint _union;

        [FieldOffset(0)] private ushort _orderInLayer;
        [FieldOffset(2)] private ushort _layerIndex;

        public SortingLayer(int order, ushort layer)
        {
            _union = 0;
            _orderInLayer = (ushort)(order - short.MinValue);
            _layerIndex = layer;
        }

        public void SetOrder(int order) => _orderInLayer = (ushort)(order - short.MinValue);
        public void SetLayer(string name) => _layerIndex = LayerNameToIndex(name);

        public int CompareTo(SortingLayer other) => other._union.CompareTo(_union);

        public static string LayerIndexToName(ushort index)
        {
            if (index < 0 || index >= LAYERS.Length) { return string.Empty; }
            return LAYERS[index];
        }
        public static ushort LayerNameToIndex(string name)
        {
            for (ushort i = 0; i < LAYERS.Length; i++)
            {
                if (LAYERS[i] == name) { return i; }
            }
            return 0;
        }

    }
}
