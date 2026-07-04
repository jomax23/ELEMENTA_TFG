using UnityEngine;

// Maps a specific element to its 4 corresponding abilities.
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