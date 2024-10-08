using UnityEngine;

public abstract class StatusEffectData : ScriptableObject
{
    [SerializeField] public StatusEffectTrigger TickTrigger = StatusEffectTrigger.None;
    public abstract StatusEffect Create();
}

public enum StatusEffectTrigger
{
    None,
    EndOfRound,
}

public abstract class StatusEffect
{
    public bool IsExpired { get; private set; }

    public abstract void Tick(Character character);
    
    public abstract void Merge(StatusEffect newEffect);

    public abstract void Apply(Character character);

    public virtual string GetText() => string.Empty;

    public void Expire()
    {
        IsExpired = true;
    }
}
