using UnityEngine;

[CreateAssetMenu]
public class CharacterData : ScriptableObject
{
    [SerializeField] public Character CharacterPrefab;
    [SerializeField] public ToyBoxDraggableItem CharacterUiDraggablePrefab;
    [SerializeField] public int Hp;
    [SerializeField] public int AttackPower;
}
