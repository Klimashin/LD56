using UnityEngine;

[CreateAssetMenu]
public class CharacterData : ScriptableObject
{
    [SerializeField] public Character CharacterPrefab;
    [SerializeField] public ToyBoxDraggableItem CharacterUiDraggablePrefab;
    [SerializeField] public int Hp;
    [SerializeField] public int AttackPower;
    [SerializeField] public bool Haste;
    [SerializeField] public Ability AttackAbility;
    [SerializeField] public Ability OnDeathAbility;
    [SerializeField] public Ability OnRoundEndAbility;
}
