using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class WavesConfig : ScriptableObject
{
    [SerializeField] public List<WaveConfig> Waves;
}
