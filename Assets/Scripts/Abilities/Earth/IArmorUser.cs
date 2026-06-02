/// <summary>
/// Interface for entities that can have their movement speed modified by an armor system.
/// This decouples the PlayerArmor script from PlayerMovement, allowing it to be reused 
/// on enemies or other entities that implement this interface.
/// </summary>
public interface IArmorUser
{
    /// <summary>
    /// Applies a movement speed multiplier. 
    /// 1.0f = normal speed, < 1.0f = slowed down.
    /// </summary>
    void SetArmorSpeedMultiplier(float multiplier);
}