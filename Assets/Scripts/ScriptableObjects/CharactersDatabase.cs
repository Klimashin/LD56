using System;
using LavaUtils;
using UnityEngine;

[CreateAssetMenu]
public class CharactersDatabase : ScriptableObject
{
    public CharactersDictionary Values;
}

[Serializable]
public class CharactersDictionary : UnitySerializedDictionary<CharacterType, CharacterData> { }