using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Image))]
public class LibraryCard : MonoBehaviour, IPointerClickHandler
{
    [Header("Link to the entity")]
    [SerializeField]
    private GameEntity _referenceEntity;

    private Image _cardImage;

    public void OnPointerClick(PointerEventData eventData)
    {
        if ( _referenceEntity == null )
        {
            return;
        }

        if ( LibraryInfoPanel.Instance != null )
        {
            LibraryInfoPanel.Instance.ShowInfo(_referenceEntity);
        }
    }
}