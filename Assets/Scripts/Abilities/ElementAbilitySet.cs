using UnityEngine;

/// <summary>
/// ScriptableObject that maps a specific ElementType to its 4 corresponding abilities.
/// </summary>
[CreateAssetMenu(fileName = "ElementAbilitySet", menuName = "Abilities/Element Ability Set")]
public class ElementAbilitySet : ScriptableObject
{
    [Header("Element")]
    public ElementType element;

    [Header("Abilities")]
    public AbilityData ability1;
    public AbilityData ability2;
    public AbilityData ability3;
    public AbilityData ability4;
}