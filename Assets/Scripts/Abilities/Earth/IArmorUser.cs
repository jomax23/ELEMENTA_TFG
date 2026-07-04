// Decouples the armor system from specific movement scripts.
// Any entity (player or enemy) can implement this to receive speed penalties from the armor.
public interface IArmorUser
{
    // Applies a movement speed multiplier. 
    // 1.0f = normal speed, < 1.0f = slowed down.
    void SetArmorSpeedMultiplier(float multiplier);
}