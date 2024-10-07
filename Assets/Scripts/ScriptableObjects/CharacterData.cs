using UnityEngine;

[CreateAssetMenu]
public class CharacterData : ScriptableObject
{
    [SerializeField] public Character CharacterPrefab;
    [SerializeField] public ToyBoxDraggableItem CharacterUiDraggablePrefab;
    [SerializeField] public int Hp;
    [SerializeField] public int Power;
    [SerializeField] public int Cost = 1;
    [SerializeField] public Ability MainAbility;
    [SerializeField] public Ability OnDeathAbility;
    [SerializeField] public Ability OnFloorFightEndAbility;
    [SerializeField, TextArea] public string Description;
}
