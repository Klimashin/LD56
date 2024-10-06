using UnityEngine;

public abstract class StatusEffect<T> : ScriptableObject where T : StatusEffect
{
    public abstract T Create();
}

public enum StatusEffectTrigger
{
    None,
    EndOfRound,
}

public abstract class StatusEffect
{
    public abstract void Apply(Character character);
}
