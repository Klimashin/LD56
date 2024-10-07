using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class UpgradePackageConfig : ScriptableObject
{
    [SerializeField] public UpgradeSlotUi UpgradeSlotUi;
    [SerializeField] public List<string> CharactersInPackage;
}
