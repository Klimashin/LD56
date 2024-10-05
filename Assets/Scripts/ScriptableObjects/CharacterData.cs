using UnityEngine;

[CreateAssetMenu]
public class CharacterData : ScriptableObject
{
    [SerializeField] public CharacterType CharacterType;
    [SerializeField] public Character CharacterPrefab;
    [SerializeField] public ToyBoxDraggableItem CharacterUiDraggablePrefab;
}
