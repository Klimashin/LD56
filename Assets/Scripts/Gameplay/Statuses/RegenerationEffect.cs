using UnityEngine;

[CreateAssetMenu(menuName = "Statuses/Regeneration")]
public class RegenerationEffect : StatusEffect<Regeneration>
{
    [SerializeField] private int value;
    
    public override Regeneration Create()
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
    }
}
