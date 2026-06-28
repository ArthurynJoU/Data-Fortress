using UnityEngine;

/// <summary>
/// The root of all entities in the game. 
/// </summary>
public abstract class GameEntity : MonoBehaviour, IProduct, IInfoProvider
{
    [Header("Base Characteristics")]
    [SerializeField]
    protected float MaxHealth = 100f;
    [SerializeField]
    protected float Strength = 20f;

    public float CurrentHealth { get; protected set; }

    [Header("UI Visuals")]
    [SerializeField]
    protected string EntityName;
    [TextArea] [SerializeField] 
    protected string Description;
    [SerializeField] 
    protected Sprite Icon;

    public GameContentFactory OriginFactory { get; set; }

    public virtual void Initialize()
    {
        gameObject.SetActive(true);
        CurrentHealth = MaxHealth;
    }

    public virtual void Recycle()
    {
        if ( OriginFactory != null )
        {
            OriginFactory.Reclaim(this);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    public string GetTitle()
    {
        return EntityName;
    }
    public string GetDescription()
    {
        return Description;
    }
    public Sprite GetIcon()
    {
        return Icon;
    }
}