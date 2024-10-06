using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class HealAbility : Ability
{
    [SerializeField] private int _amount;
    
    public async override UniTask Apply(List<Character> targets)
    {
        foreach (var character in targets)
        {
            character.Heal(_amount);
        }
    }
}
