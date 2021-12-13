namespace Joonaxii.Physics.Demo.Rendering
{
    [System.Flags]
    public enum TransformConstraints
    {
        None,

        Position,
        Rotation,
        Scale,

        RotScale = Rotation | Scale,
        PosScale = Position | Scale,
        PosRotation = Position | Rotation,

        All = Position | Rotation | Scale,
    }
}