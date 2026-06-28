using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The basic logic for all supporting programmes through analysis.
/// </summary>
public abstract class EffectBase : GameEntity, IEffector, ITickable
{
    [Header("Settings")]
    [Tooltip("The lifetime of a effect on the field in seconds. If 0 or less, it is infinite.")]
    [SerializeField]
    protected float AttritionRate = 10f;

    [Header("Targeting")]
    [Tooltip("List of enemy types that this node is effective against")]
    [SerializeField]
    protected List<EnemyType> AffectedCategories;

    [Header("Effect Type")]
    [Tooltip("Type of this effect for interaction with enemies")]
    public EffectType Type;

    [Header("Expiration Visuals")]
    [SerializeField]
    protected Material _dissolveMaterialTemplate;
    [SerializeField]
    protected float _expireAnimationTime = 1f;
    [SerializeField]
    protected string _shaderDissolveParameter = "_DissolveAmount";
    [SerializeField]
    protected ParticleSystem _expirationVFX;

    [Header("Contact Visuals")]
    [SerializeField]
    protected ParticleSystem _contactVFX;

    [Header("Audio")]
    [SerializeField]
    protected AudioClip AttackSound;
    [SerializeField]
    protected AudioClip DeathSound;
    [SerializeField]
    protected AudioClip BirthSound;
    [Range(0f, 1f)]
    [SerializeField]
    protected float Volume = 0.6f;

    // Internal States
    protected bool _isExpiring = false;
    protected float _currentLifetime;
    protected List<EnemyBase> _affectedEnemies = new List<EnemyBase>();

    private Dictionary<Renderer, Material[]> _originalMaterials = new Dictionary<Renderer, Material[]>();
    private bool _materialsSaved = false;

    public override void Initialize()
    {
        base.Initialize();

        SaveAndRestoreMaterials();

        _isExpiring = false;
        _affectedEnemies.Clear();

        Collider col = GetComponent<Collider>();
        
        if ( col != null )
        {
            col.enabled = true;
            col.isTrigger = true;

            Collider[] hits = Physics.OverlapBox(col.bounds.center, col.bounds.extents);
            
            foreach ( Collider hit in hits )
            {
                OnTriggerEnter(hit);
            }
        }

        if ( GameSessionController.Instance != null )
        {
            GameSessionController.Instance.RegisterEffect(this);
        }

        if ( BirthSound != null )
        {
            AudioSource.PlayClipAtPoint(BirthSound, transform.position, Volume);
        }
    }

    public override void Recycle()
    {
        List<EnemyBase> activeEnemies = new List<EnemyBase>(_affectedEnemies);

        foreach ( EnemyBase enemy in activeEnemies )
        {
            if ( enemy != null )
            {
                RemoveEffect(enemy);
            }
        }

        _affectedEnemies.Clear();

        if ( GameSessionController.Instance != null )
        {
            GameSessionController.Instance.UnregisterEffect(this);
        }

        base.Recycle();
    }

    private void SaveAndRestoreMaterials()
    {
        if ( !_materialsSaved )
        {
            Renderer[] allRenderers = GetComponentsInChildren<Renderer>(true);
            
            foreach ( Renderer r in allRenderers )
            {
                _originalMaterials[r] = r.sharedMaterials;
            }
            
            _materialsSaved = true;
        }

        foreach ( var mat in _originalMaterials )
        {
            if ( mat.Key != null )
            {
                mat.Key.materials = mat.Value;
            }
        }
    }

    public virtual void Tick(float deltaTime)
    {
        if ( !_isExpiring )
        {
            TakeDamage(AttritionRate * deltaTime);
        }

        int currentIndex = _affectedEnemies.Count - 1;
        while ( currentIndex >= 0 )
        {
            EnemyBase enemy = _affectedEnemies[currentIndex];

            if ( enemy == null || !enemy.gameObject.activeInHierarchy || enemy.CurrentHealth <= 0 )
            {
                _affectedEnemies.RemoveAt(currentIndex);
                RemoveEffect(enemy);
            }
            
            currentIndex--;
        }
    }

    public virtual void TakeDamage(float amount)
    {
        if ( !gameObject.activeInHierarchy )
        {
            return;
        }

        if ( CurrentHealth <= 0 )
        {
            return;
        }

        if ( _isExpiring )
        {
            return;
        }

        CurrentHealth -= amount;

        if ( CurrentHealth <= 0 )
        {
            Die();
        }
    }

    public virtual void Die()
    {
        if ( !gameObject.activeInHierarchy )
        {
            return;
        }

        if ( _isExpiring )
        {
            return;
        }

        if ( DeathSound != null )
        {
            AudioSource.PlayClipAtPoint(DeathSound, transform.position, Volume);
        }

        StartCoroutine(ExpireRoutine());
    }

    // Optimised using MaterialPropertyBlock to prevent excessive garbage collection
    protected virtual IEnumerator ExpireRoutine()
    {
        _isExpiring = true;

        Collider col = GetComponent<Collider>();
        if ( col != null )
        {
            col.enabled = false;
        }

        if ( _expirationVFX != null )
        {
            Vector3 vfxPos = transform.position + new Vector3(0, 0.2f, 0);
            Instantiate(_expirationVFX, vfxPos, Quaternion.identity);
        }

        Renderer[] allRenderers = GetComponentsInChildren<Renderer>();

        if ( allRenderers.Length > 0 && _dissolveMaterialTemplate != null )
        {
            for ( int i = 0; i < allRenderers.Length; i++ )
            {
                Renderer rend = allRenderers[i];
                Material[] sharedDyingMats = new Material[rend.sharedMaterials.Length];
                
                for ( int j = 0; j < sharedDyingMats.Length; j++ )
                {
                    sharedDyingMats[j] = _dissolveMaterialTemplate;
                }
                
                rend.sharedMaterials = sharedDyingMats;
            }

            MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
            float elapsed = 0f;

            while ( elapsed < _expireAnimationTime )
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / _expireAnimationTime;

                foreach ( Renderer rend in allRenderers )
                {
                    if ( rend != null )
                    {
                        rend.GetPropertyBlock(propertyBlock);
                        propertyBlock.SetFloat(_shaderDissolveParameter, progress);
                        rend.SetPropertyBlock(propertyBlock);
                    }
                }
                
                yield return null;
            }
        }
        else
        {
            yield return new WaitForSeconds(_expireAnimationTime);
        }

        Recycle();
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[EffectBase] An object has entered the effect area of {gameObject.name}: {other.gameObject.name}");
        EnemyBase enemy = other.GetComponentInParent<EnemyBase>();

        if ( enemy != null && enemy.GetHealth() > 0 && !enemy.IsImmune && AffectedCategories.Contains(enemy.Category) )
        {
            if ( !_affectedEnemies.Contains(enemy) )
            {
                Debug.Log($"[EffectBase] Applying the {Type} effect to the enemy {enemy.name}");
                
                if ( _contactVFX != null )
                {
                    Vector3 vfxPos = enemy.transform.position + new Vector3(0, 0.5f, 0);
                    Instantiate(_contactVFX, vfxPos, Quaternion.identity);
                }

                IEffectible effectible = enemy as IEffectible;
                
                if ( effectible != null )
                {
                    effectible.ApplyEffect(this);
                }
                
                if ( AttackSound != null )
                {
                    AudioSource.PlayClipAtPoint(AttackSound, transform.position, Volume);
                }
                
                ApplyEffect(enemy);
                _affectedEnemies.Add(enemy);
            }
        }
        else
        {
            Debug.Log($"[EffectBase] The object {other.gameObject.name} has been ignored. (Category does not match or HP=0)");
        }
    }

    protected virtual void OnTriggerExit(Collider other)
    {
        EnemyBase enemy = other.GetComponentInParent<EnemyBase>();

        if ( enemy != null )
        {
            if ( _affectedEnemies.Contains(enemy) )
            {
                _affectedEnemies.Remove(enemy);
                RemoveEffect(enemy);
            }
        }
    }

    public abstract void ApplyEffect(EnemyBase target);
    public abstract void RemoveEffect(EnemyBase target);
}