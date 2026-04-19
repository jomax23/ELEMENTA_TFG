using UnityEngine;

[CreateAssetMenu(
    fileName = "ElementAbilitySet",
    menuName = "Abilities/Element Ability Set"
)]
public class ElementAbilitySet : ScriptableObject
{
    public ElementType element;

    public AbilityData ability1;
    public AbilityData ability2;
    public AbilityData ability3;
    public AbilityData ability4;
}