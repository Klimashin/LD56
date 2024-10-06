using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/ApplyStatus")]
public class ApplyStatusEffect : Ability
{
    [SerializeField] private StatusEffectData _statusEffectData;

    public override void Apply(List<Character> targets, Character self)
    {
        foreach (var character in targets)
        {
            var statusEffect = _statusEffectData.Create();
            character.ApplyStatusEffect(statusEffect);
        }
    }
}
