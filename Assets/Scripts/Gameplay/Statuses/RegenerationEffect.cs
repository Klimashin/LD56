using UnityEngine;

[CreateAssetMenu(menuName = "Statuses/Regeneration")]
public class RegenerationEffect : StatusEffectData
{
    [SerializeField] private int value;
    
    public override StatusEffect Create()
    {
        return new Regeneration(value);
    }
}

public class Regeneration : StatusEffect
{
    public int Value { get; private set; }

    public Regeneration(int value)
    {
        Value = value;
    }
    
    public override void Apply(Character character)
    {
        character.Heal(Value);
        Value--;
        if (Value == 0)
        {
            Expire();
        }
    }

    public override void Merge(StatusEffect newEffect)
    {
        Value += (newEffect as Regeneration).Value;
    }
}
