using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public abstract class Ability : ScriptableObject
{
    [SerializeField] public Target target = Target.Self; 
    
    public enum Target
    {
        Self,
        AllAlliesOnTheFloor,
        AllEnemiesOnTheFloor,
        FirstEnemyOnTheFloor,
        LastEnemyOnTheFloor,
        AttackTarget
    }
    
    public abstract UniTask Apply(List<Character> targets);
}
