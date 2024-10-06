using UnityEngine;

[CreateAssetMenu(menuName = "Statuses/Poison")]
public class PoisonEffect : StatusEffectData
{
    [SerializeField] private int value;
    
    public override StatusEffect Create()
    {
        return new Poison(value);
    }
}

public class Poison : StatusEffect
{
    public int Value { get; private set; }

    public Poison(int value)
    {
        Value = value;
    }
    
    public override void Apply(Character character)
    {
        character.Damage(Value);
        Value--;
        if (Value == 0)
        {
            Expire();
        }
    }

    public override void Merge(StatusEffect newEffect)
    {
        Value += (newEffect as Poison).Value;
    }
}
