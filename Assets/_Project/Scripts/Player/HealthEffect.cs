using Shelter.Core;
using Shelter.Player;
using UnityEngine;

public class HealthEffect : MonoBehaviour
{
    [SerializeField] private PlayerHealth _health;

    [SerializeField] private Material HealthEffectVignette;
    void Start()
    {
        HealthEffectVignette.SetFloat("_ApertureSize", 1);
    }

    private void OnEnable()
    {
        _health.Damaged += Effect;
    }

    private void OnDisable()
    {
        _health.Damaged -= Effect;
    }

    void Effect(DamageInfo damageInfo)
    {
        float _vlaue = 1 - (((_health.MaxHealth - _health.CurrentHealth) * (_health.MaxHealth -_health.CurrentHealth)) / (_health.MaxHealth * _health.MaxHealth));
        HealthEffectVignette.SetFloat("_ApertureSize", _vlaue);

    }
}
