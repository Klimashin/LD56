using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Damage")]
public class DamageAbility : Ability
{
    [SerializeField] private bool _usePower = true;
    [SerializeField, HideIf("_usePower")] private int _amount;
    
    public async override UniTask Apply(List<Character> targets, Character self)
    {
        foreach (var character in targets)
        {
            var amount = _usePower ? self.Power : _amount;
            character.Damage(amount);
        }
    }
}
