using UnityEngine;

/// <summary>
/// Data provider contract for the UI system (TooltipUI).
/// </summary>
public interface IInfoProvider
{
    string GetTitle();
    string GetDescription();
    Sprite GetIcon();
}