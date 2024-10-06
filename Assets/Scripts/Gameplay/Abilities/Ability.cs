using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

public abstract class Ability : ScriptableObject
{
    [SerializeField] public Target target = Target.Self; 
    
    public enum Target
    {
        Self,
        FirstEnemyOnTheFloor,
        LastEnemyOnTheFloor,
        AllEnemiesOnTheFloor,
        FirstAllyOnTheFloor,
        LastAllyOnTheFloor,
        AllAlliesOnTheFloor
    }

    public List<Character> SelectTargets(List<Character> allUnitsOnTheFloor, Character self)
    {
        switch (target)
        {
            case Target.Self:
                return new List<Character>() { self };
            case Target.FirstEnemyOnTheFloor:
            {
                var target = self.IsEnemy
                    ? allUnitsOnTheFloor.Where(c => !c.IsEnemy)
                        .OrderBy(c => c.LayoutPos.zoneIndex)
                        .FirstOrDefault()
                    : allUnitsOnTheFloor.Where(c => c.IsEnemy)
                        .OrderBy(c => c.LayoutPos.zoneIndex)
                        .FirstOrDefault();
                return target == null ? new List<Character>() : new List<Character>() {target};
            }
            case Target.AllEnemiesOnTheFloor:
                return self.IsEnemy
                    ? allUnitsOnTheFloor.Where(c => !c.IsEnemy)
                        .OrderBy(c => c.LayoutPos.zoneIndex).ToList()
                    : allUnitsOnTheFloor.Where(c => c.IsEnemy)
                        .OrderBy(c => c.LayoutPos.zoneIndex).ToList();
            case Target.LastEnemyOnTheFloor:
            {
                var target = self.IsEnemy
                    ? allUnitsOnTheFloor.Where(c => !c.IsEnemy)
                        .OrderByDescending(c => c.LayoutPos.zoneIndex)
                        .FirstOrDefault()
                    : allUnitsOnTheFloor.Where(c => c.IsEnemy)
                        .OrderByDescending(c => c.LayoutPos.zoneIndex)
                        .FirstOrDefault();
                return target == null ? new List<Character>() : new List<Character>() {target};
            }
            case Target.FirstAllyOnTheFloor:
            {
                var target = self.IsEnemy
                    ? allUnitsOnTheFloor.Where(c => c.IsEnemy)
                        .OrderBy(c => c.LayoutPos.zoneIndex)
                        .FirstOrDefault()
                    : allUnitsOnTheFloor.Where(c => !c.IsEnemy)
                        .OrderBy(c => c.LayoutPos.zoneIndex)
                        .FirstOrDefault();
                return target == null ? new List<Character>() : new List<Character>() {target};
            }
            case Target.LastAllyOnTheFloor:
            {
                var target = self.IsEnemy
                    ? allUnitsOnTheFloor.Where(c => c.IsEnemy)
                        .OrderByDescending(c => c.LayoutPos.zoneIndex)
                        .FirstOrDefault()
                    : allUnitsOnTheFloor.Where(c => !c.IsEnemy)
                        .OrderByDescending(c => c.LayoutPos.zoneIndex)
                        .FirstOrDefault();
                return target == null ? new List<Character>() : new List<Character>() {target};
            }
            case Target.AllAlliesOnTheFloor:
            {
                return self.IsEnemy
                    ? allUnitsOnTheFloor.Where(c => c.IsEnemy)
                        .OrderBy(c => c.LayoutPos.zoneIndex).ToList()
                    : allUnitsOnTheFloor.Where(c => !c.IsEnemy)
                        .OrderBy(c => c.LayoutPos.zoneIndex).ToList();
            }
            default:
                throw new Exception("Unhandled target setting");
        }
    }
    
    public abstract UniTask Apply(List<Character> targets, Character self);
}
