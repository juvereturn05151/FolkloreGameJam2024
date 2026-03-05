

public interface IRotModifier
{
    float Multiplier { get; }   // e.g. 2.0f means rot twice as fast
    int Priority { get; }       // optional if you want ordering
}