using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Instantly teleports the malicious entity back to the level's starting point, applying a digital dissolve effect.
/// </summary>
public class IDS_DPI : EffectBase
{
    [Header("Teleport Visuals")]
    [SerializeField] 
    private ParticleSystem _teleportParticles;
    [SerializeField] 
    private float _teleportAnimationTime = 1f;

    public override void ApplyEffect(EnemyBase target)
    {
        if ( LevelManager.Instance.CurrentLevel != null && GameBoard.Instance != null )
        {
            target.StartCoroutine(TeleportRoutine(target));
        }
    }

    // Refactored to use MaterialPropertyBlock and sharedMaterials to avoid garbage collection spikes
    private IEnumerator TeleportRoutine(EnemyBase target)
    {
        float originalSpeed = target.GetBaseSpeed();
        target.SetSpeed(0f);
        target.IsTargetable = false;

        Renderer[] allRenderers = target.GetComponentsInChildren<Renderer>();
        Dictionary<Renderer, Material[]> originalMaterials = new Dictionary<Renderer, Material[]>();

        if ( allRenderers.Length > 0 && _dissolveMaterialTemplate != null )
        {
            for ( int i = 0; i < allRenderers.Length; i++ )
            {
                Renderer rend = allRenderers[i];
                
                if ( rend != null )
                {
                    originalMaterials[rend] = rend.sharedMaterials;

                    Material[] sharedDyingMats = new Material[rend.sharedMaterials.Length];
                    for ( int j = 0; j < sharedDyingMats.Length; j++ )
                    {
                        sharedDyingMats[j] = _dissolveMaterialTemplate;
                    }
                    rend.sharedMaterials = sharedDyingMats;
                }
            }

            if ( _teleportParticles != null )
            {
                Instantiate(_teleportParticles, target.transform.position, Quaternion.identity);
            }

            MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
            float elapsed = 0f;
            
            while ( elapsed < _teleportAnimationTime / 2f )
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / ( _teleportAnimationTime / 2f );

                for ( int k = 0; k < allRenderers.Length; k++ )
                {
                    Renderer rend = allRenderers[k];
                    
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

        Vector3 startPosition = new Vector3(
            GameBoard.Instance.StartPoint.Coordinates.x,
            0f,
            GameBoard.Instance.StartPoint.Coordinates.y);

        target.transform.position = startPosition;
        target.BuildNewRoute();

        if ( originalMaterials.Count > 0 )
        {
            if ( _teleportParticles != null )
            {
                Instantiate(_teleportParticles, target.transform.position, Quaternion.identity);
            }

            MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
            float elapsed = 0f;
            
            while ( elapsed < _teleportAnimationTime / 2f )
            {
                elapsed += Time.deltaTime;
                float progress = 1f - ( elapsed / ( _teleportAnimationTime / 2f ) );

                for ( int k = 0; k < allRenderers.Length; k++ )
                {
                    Renderer rend = allRenderers[k];
                    
                    if ( rend != null )
                    {
                        rend.GetPropertyBlock(propertyBlock);
                        propertyBlock.SetFloat(_shaderDissolveParameter, progress);
                        rend.SetPropertyBlock(propertyBlock);
                    }
                }
                
                yield return null;
            }

            foreach ( var mat in originalMaterials )
            {
                if ( mat.Key != null )
                {
                    mat.Key.sharedMaterials = mat.Value;
                }
            }
        }

        target.SetSpeed(originalSpeed);
        target.IsTargetable = true;
    }

    public override void RemoveEffect(EnemyBase target)
    {
        // The instant effect does not require cancellation
    }
}