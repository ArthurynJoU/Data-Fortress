using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Progresses stealthily across the board, remaining completely untargetable by standard defenses until exposed by specific analytical effects or traps.
/// </summary>
public class Trojan : EnemyBase, IEffectible
{
    [Header("Mechanics")]
    [SerializeField]
    private Renderer _modelRenderer; // enemy's 3D model
    
    private bool _isVisible = false;
    public override bool IsTargetable => _isVisible;

    [Header("Activating Effects")]
    [Tooltip("List of effect types that remove invisibility from Trojan.")]
    [SerializeField] 
    private List<EffectType> _activatingEffects;

    [Header("Visuals")]
    [SerializeField] 
    private ParticleSystem _revealEffectPrefab;

    public override void Initialize()
    {
        base.Initialize();
        SetVisibility(false);
    }

    public void SetVisibility(bool state)
    {
        if ( state && !_isVisible && _revealEffectPrefab != null )
        {
            Instantiate(_revealEffectPrefab, transform.position + Vector3.up * 0.5f, Quaternion.identity);
        }

        _isVisible = state;
        if ( _modelRenderer != null )
        {
            foreach ( Material mat in _modelRenderer.materials )
            {
                if ( mat.HasProperty("_Color") )
                {
                    Color c = mat.color;
                    c.a = state ? 1f : 0.4f;
                    mat.color = c;
                }
                else if ( mat.HasProperty("_BaseColor") )
                {
                    Color c = mat.GetColor("_BaseColor");
                    c.a = state ? 1f : 0.4f;
                    mat.SetColor("_BaseColor", c);
                }
            }
        }
    }
    void IEffectible.ApplyEffect(EffectBase trigger)
    {
        if ( trigger != null && !_isVisible &&
    _activatingEffects.Contains(trigger.Type) )
        {
            SetVisibility(true);
        }
    }
}