using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatusEffectsDisplay : CharacterDataDisplay
{
    [SerializeField] private Transform _statusEffectsTransform;
    [SerializeField] private GameObject _effectPrefab;
    [SerializeField] private Sprite _poisonSprite;
    [SerializeField] private Sprite _regenSprite;

    private readonly List<GameObject> _effects = new ();
    
    public override void UpdateDisplay(Character character)
    {
        for (var i = 0; i < _effects.Count; i++)
        {
            Destroy(_effects[i]);
        }
        _effects.Clear();

        foreach (var characterStatusEffect in character.StatusEffects)
        {
            if (characterStatusEffect.Value == null)
            {
                continue;
            }

            var effectText = characterStatusEffect.Value.GetText();
            if (string.IsNullOrEmpty(effectText))
            {
                continue;
            }

            var effect = Instantiate(_effectPrefab, _statusEffectsTransform);
            effect.GetComponentInChildren<TMP_Text>().text = effectText;
            if (characterStatusEffect.Key.ToString() == "Poison")
            {
                effect.GetComponentInChildren<Image>().sprite = _poisonSprite;
            }
            else if (characterStatusEffect.Key.ToString() == "Regeneration")
            {
                effect.GetComponentInChildren<Image>().sprite = _regenSprite;
            }
            
            _effects.Add(effect);
        }
    }
}
