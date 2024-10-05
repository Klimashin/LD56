using TMPro;
using UnityEngine;

public class HpDisplay : CharacterDataDisplay
{
    [SerializeField] private TMP_Text _hpText;
    
    public override void UpdateDisplay(Character character)
    {
        _hpText.text = character.Hp.ToString();
    }
}
