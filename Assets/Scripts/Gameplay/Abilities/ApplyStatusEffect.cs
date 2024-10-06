using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/ApplyStatus")]
public class ApplyStatusEffect : Ability
{
    [SerializeField] private StatusEffectData _statusEffectData;

    public async override UniTask Apply(List<Character> targets, Character self)
    {
        foreach (var character in targets)
        {
            var statusEffect = _statusEffectData.Create();
            character.ApplyStatusEffect(statusEffect);
        }
    }
}
