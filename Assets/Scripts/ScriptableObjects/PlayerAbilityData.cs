using UnityEngine;

[CreateAssetMenu]
public class PlayerAbilityData : ScriptableObject
{
    [SerializeField] public int Cost = 1;
    [SerializeField] public PlayerAbilityTarget target;
    [SerializeField] public PlayerAbilityType type;
    [SerializeField] public int Power = 1;
    [SerializeField, TextArea] public string Description;
}

public enum PlayerAbilityTarget
{
    PlayerUnit,
    EnemyUnit
}

public enum PlayerAbilityType
{
    Damage,
    Heal
}
