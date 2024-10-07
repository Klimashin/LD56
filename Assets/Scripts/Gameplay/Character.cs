using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Reflex.Attributes;
using UnityEngine;

public class Character : MonoBehaviour
{
    [SerializeField] private CharacterDataDisplay[] _dataDisplays;
    [SerializeField] private SpriteRenderer _sprite;
    [SerializeField] private float _floatingTextOffsetY = 1.5f;
    
    private CharacterData _characterData;

    public int MaxHp => _characterData.Hp + _bonusHp;
    public int Hp { get; private set; }
    public int Power => _characterData.Power + _powerBonus;
    public bool IsDead => Hp <= 0;

    public Action<Character> OnLethalDamage;

    public CharacterPosition LayoutPos { get; private set; }
    public readonly Dictionary<Type, StatusEffect> StatusEffects = new ();
    public bool IsEnemy => LayoutPos.zoneType == ZoneType.Right;
    public bool IsHandlingInProgress => _handlesHashSet.Count > 0;

    private int _powerBonus;
    private int _bonusHp;
    private BattleController _battleController;
    private FloatingTextFactory _floatingTextFactory;
    private readonly HashSet<string> _handlesHashSet = new ();

    [Inject]
    private void Inject(BattleController battleController, FloatingTextFactory floatingTextFactory)
    {
        _battleController = battleController;
        _floatingTextFactory = floatingTextFactory;
    }

    public void Initialize(CharacterData characterData, CharacterPosition layoutPos)
    {
        _characterData = characterData;
        LayoutPos = layoutPos;
        _sprite.flipX = layoutPos.zoneType == ZoneType.Right;
        
        Hp = MaxHp;

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

        _floatingTextFactory.ShowFloatingText($"+{amount.ToString()}", transform.position + Vector3.up * _floatingTextOffsetY, FloatingTextFactory.FloatingTextType.Green);
        Hp = Mathf.Min(MaxHp, Hp + amount);
        UpdateDisplays();
    }

    private void UpdateDisplays()
    {
        foreach (var characterDataDisplay in _dataDisplays)
        {
            characterDataDisplay.UpdateDisplay(this);
        }
    }

    public async UniTask HandleMainAbility(List<Character> possibleTargets)
    {
        _handlesHashSet.Add(nameof(HandleMainAbility));
        
        possibleTargets = possibleTargets.Where(t => !t.IsDead).ToList();

        var mainAbility = _characterData.MainAbility;
        var targets = mainAbility.SelectTargets(possibleTargets, this);

        if (targets.Count > 0)
        {
            await MainAbilityAnimation();

            mainAbility.Apply(targets, this);
            
            await UniTask.Delay(TimeSpan.FromSeconds(BattleController.STANDARD_DELAY), DelayType.DeltaTime, PlayerLoopTiming.Update, destroyCancellationToken);
        }

        _handlesHashSet.Remove(nameof(HandleMainAbility));
    }
    
    public async UniTask HandleOnDeathAbility(List<Character> possibleTargets)
    {
        _handlesHashSet.Add(nameof(HandleOnDeathAbility));
        
        possibleTargets = possibleTargets.Where(t => !t.IsDead).ToList();

        var ability = _characterData.OnDeathAbility;
        var targets = ability.SelectTargets(possibleTargets, this);

        ability.Apply(targets, this);
        
        _handlesHashSet.Remove(nameof(HandleOnDeathAbility));
    }
    
    public async UniTask HandleOnFloorFightEndAbility(List<Character> possibleTargets)
    {
        _handlesHashSet.Add(nameof(HandleOnFloorFightEndAbility));
        
        possibleTargets = possibleTargets.Where(t => !t.IsDead).ToList();

        var ability = _characterData.OnFloorFightEndAbility;
        if (ability != null)
        {
            var targets = ability.SelectTargets(possibleTargets, this);
            
            transform.DOPunchScale(new Vector3(0.1f, 0.1f, 0.1f), 0.4f, 1);

            ability.Apply(targets, this);
            
            await UniTask.Delay(TimeSpan.FromSeconds(BattleController.STANDARD_DELAY), DelayType.DeltaTime, PlayerLoopTiming.Update, destroyCancellationToken);
        }
        
        StatusEffectsTick();

        _handlesHashSet.Remove(nameof(HandleOnFloorFightEndAbility));
    }

    private void StatusEffectsTick()
    {
        foreach (var keyValuePair in StatusEffects)
        {
            var statusEffect = keyValuePair.Value;
            if (statusEffect == null)
            {
                continue;
            }

            statusEffect.Tick(this);
        }

        ClearExpiredEffects();
        
        UpdateDisplays();
    }

    public void Damage(int damage)
    {
        if (IsDead)
        {
            return;
        }
        
        Hp = Mathf.Max(Hp - damage, 0);
        _floatingTextFactory.ShowFloatingText($"-{damage.ToString()}", transform.position + Vector3.up * _floatingTextOffsetY, FloatingTextFactory.FloatingTextType.Red);
        UpdateDisplays();
        if (IsDead)
        {
            OnLethalDamage?.Invoke(this);
            if (_characterData.OnDeathAbility != null)
            {
                HandleOnDeathAbility(_battleController.GetAllAliveCharactersOnTheFloor(LayoutPos.floorIndex)).Forget();
            }
            DeathRoutine().Forget();
        }
    }

    public void SetPowerBonus(int amount)
    {
        _powerBonus = amount;
    }
    
    public void SetHpBonus(int amount)
    {
        var diff = amount - _bonusHp;
        _bonusHp = amount;
        if (diff > 0)
        {
            Heal(diff);
        }
    }

    private async UniTaskVoid DeathRoutine()
    {
        await UniTask.Delay(TimeSpan.FromSeconds(BattleController.STANDARD_DELAY), DelayType.DeltaTime, PlayerLoopTiming.Update, destroyCancellationToken);
        Destroy(gameObject);
    }

    private async UniTask MainAbilityAnimation()
    {
        var defaultOrder = _sprite.sortingOrder;
        var animationSeq = DOTween.Sequence().SetAutoKill(false);
        _sprite.sortingOrder++;
        animationSeq
            .Append(transform.DOMoveX(transform.position.x + (IsEnemy ? -2f : 2f), BattleController.STANDARD_DELAY / 2f))
            .OnComplete(() =>
            {
                animationSeq
                    .SetAutoKill(true)
                    .PlayBackwards();
            });

        await UniTask.Delay(TimeSpan.FromSeconds(BattleController.STANDARD_DELAY), DelayType.DeltaTime, PlayerLoopTiming.Update, destroyCancellationToken);

        _sprite.sortingOrder = defaultOrder;
    }

    public void ApplyStatusEffect(StatusEffect statusEffect)
    {
        var effectType = statusEffect.GetType();
        if (StatusEffects.TryGetValue(effectType, out var existingEffect) && existingEffect != null)
        {
            existingEffect.Merge(statusEffect);
        }
        else
        {
            StatusEffects.Add(effectType, statusEffect);
        }
    }

    public void ClearExpiredEffects()
    {
        var expiredEffectPairs = StatusEffects.Where(pair => pair.Value.IsExpired).ToList();
        foreach (var expiredEffectPair in expiredEffectPairs)
        {
            StatusEffects.Remove(expiredEffectPair.Key);
        }
    }
}
