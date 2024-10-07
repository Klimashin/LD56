using UnityEngine;

[CreateAssetMenu(menuName = "Statuses/IncHp")]
public class IncMaxHpEffect : StatusEffectData
{
    [SerializeField] private int value;
    
    public override StatusEffect Create()
    {
        return new IncMaxHp(value);
    }
}

public class IncMaxHp : StatusEffect
{
    public int Value { get; private set; }

    public IncMaxHp(int value)
    {
        Value = value;
    }
    
    public override void Apply(Character character)
    {
        character.SetHpBonus(Value);
    }
    
    public override void Tick(Character character)
    {
        if (Value == 0)
        {
            Expire();
        }
    }

    public override void Merge(StatusEffect newEffect)
    {
        Value += (newEffect as IncMaxHp).Value;
    }
}
