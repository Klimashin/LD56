using System.Collections.Generic;
using UnityEngine;

public class BattleUI : MonoBehaviour
{
    [SerializeField] private Transform _draggablesTransform;

    private readonly List<ToyBoxDraggableItem> _draggableItems = new ();

    public void SetupDraggablesPool(List<CharacterData> characterDatas)
    {
        foreach (var toyBoxDraggableItem in _draggableItems)
        {
            Destroy(toyBoxDraggableItem.gameObject);
        }
        
        _draggableItems.Clear();

        foreach (var characterData in characterDatas)
        {
            var draggable = Instantiate(characterData.CharacterUiDraggablePrefab, _draggablesTransform);
            draggable.SetCharacterData(characterData);
            _draggableItems.Add(draggable);
        }
    }
}
