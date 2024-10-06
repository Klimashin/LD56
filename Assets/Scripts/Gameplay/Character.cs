using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class Character : MonoBehaviour
{
    [SerializeField] private CharacterDataDisplay[] _dataDisplays;
    [SerializeField] private SpriteRenderer _sprite;
    
    private CharacterData _characterData;
    private int _maxHp;

    public int MaxHp { get; private set; }
    public int Hp { get; private set; }
    public int AttackPower { get; private set; }
    public bool IsDead => Hp <= 0;

    public Action<Character> OnLethalDamage;

    public CharacterPosition LayoutPos { get; private set; }

    public void Initialize(CharacterData characterData, CharacterPosition layoutPos)
    {
        _characterData = characterData;
        LayoutPos = layoutPos;
        _sprite.flipX = layoutPos.zoneType == ZoneType.Right;

        MaxHp = _characterData.Hp;
        Hp = MaxHp;
        AttackPower = characterData.AttackPower;

        UpdateDisplays();
    }

    public void SetLayoutPos(CharacterPosition position)
    {
        LayoutPos = position;
    }

    public void Heal(int amount)
    {
        if (IsDead)
        {
            return;
        }

        Hp = Mathf.Min(MaxHp, Hp + amount);
    }

    private void UpdateDisplays()
    {
        foreach (var characterDataDisplay in _dataDisplays)
        {
            characterDataDisplay.UpdateDisplay(this);
        }
    }

    public async UniTask HandleAttack(List<Character> targets)
    {
        for (int i = 0; i < targets.Count; i++)
        {
            var target = targets[i];
            if (target.Hp <= 0)
            {
                continue;
            }

            await AttackAnimation();

            target.Damage(AttackPower);

            break;
        }
    }

    private void Damage(int damage)
    {
        Hp = Mathf.Max(Hp - damage, 0);
        UpdateDisplays();
        if (IsDead)
        {
            OnLethalDamage?.Invoke(this);
            DeathRoutine().Forget();
        }
    }

    private async UniTaskVoid DeathRoutine()
    {
        gameObject.SetActive(false);
    }

    private async UniTask AttackAnimation()
    {
        transform.DOPunchScale(new Vector3(0.1f, 0.1f, 0.1f), 0.4f, 1);
        await UniTask.Delay(TimeSpan.FromSeconds(0.5f), DelayType.DeltaTime, PlayerLoopTiming.Update, destroyCancellationToken);
    }
}
