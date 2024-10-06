using UnityEngine;

[CreateAssetMenu(menuName = "Statuses/IncPower")]
public class IncAttackPowerEffect : StatusEffectData
{
    [SerializeField] private int value;
    
    public override StatusEffect Create()
    {
        return new IncAttackPower(value);
    }
}

public class IncAttackPower : StatusEffect
{
    public int Value { get; private set; }

    public IncAttackPower(int value)
    {
        Value = value;
    }
    
    public override void Apply(Character character)
    {
        character.SetPowerBonus(Value);
        if (Value == 0)
        {
            Expire();
        }
    }

    public override void Merge(StatusEffect newEffect)
    {
        Value += (newEffect as IncAttackPower).Value;
    }
}
