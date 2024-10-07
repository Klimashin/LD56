using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

[CreateAssetMenu]
public class GameSettings : ScriptableObject
{
    [SerializeField] public int CoreBaseHp = 30;
    [SerializeField] public int BaseManaPoints = 3;
    [SerializeField] public int KingDamage = 5;
    [SerializeField] private List<string> InitialCharactersPool;
    [SerializeField] private List<WavesConfig> Stage1Waves;
    [SerializeField] private List<WavesConfig> Stage2Waves;
    [SerializeField] private List<WavesConfig> Stage3Waves;
    [SerializeField] private List<WavesConfig> Stage4Waves;
    [SerializeField] private List<UpgradePackageConfig> UpgradePackages;

    public const int MAX_STAGE_INDEX = 3;

    public List<string> GetInitialCharacters() => InitialCharactersPool;

    public WavesConfig GetWaveConfigForStage(int stageIndex)
    {
        List<WavesConfig> stageWaves;
        switch (stageIndex)
        {
            case 0:
                stageWaves = Stage1Waves;
                break;
            case 1:
                stageWaves = Stage2Waves;
                break;
            case 2:
                stageWaves = Stage3Waves;
                break;
            case 3:
                stageWaves = Stage4Waves;
                break;
            default:
                throw new Exception("Stage index out of range!");
        }

        var randomIndex = Random.Range(0, stageWaves.Count);
        return stageWaves[randomIndex];
    }

    public List<UpgradePackageConfig> GetRandomUpgradePackages(int count)
    {
        var maxIndex = UpgradePackages.Count;
        if (count - 1 > maxIndex)
        {
            throw new Exception("Requested more upgrade packages than there is in DB at all");
        }

        HashSet<int> randomIndexes = new();
        while (randomIndexes.Count < count)
        {
            var randomIndex = Random.Range(0, maxIndex);
            if (!randomIndexes.Contains(randomIndex))
            {
                randomIndexes.Add(randomIndex);
            }
        }

        List<UpgradePackageConfig> result = new();
        foreach (var i in randomIndexes.ToList())
        {
            result.Add(UpgradePackages[i]);
        }

        return result;
    }
}
