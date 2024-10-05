
using TMPro;
using UnityEngine;

public class AttackPowerDisplay : CharacterDataDisplay
{
    [SerializeField] private TMP_Text _attackPowerText;
    
    public override void UpdateDisplay(Character character)
    {
        _attackPowerText.text = character.AttackPower.ToString();
    }
}
