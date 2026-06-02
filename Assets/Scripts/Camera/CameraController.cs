using UnityEngine;

/// <summary>
/// Dynamically adjusts the camera position to keep all players in view.
/// Calculates the bounding box of all objects tagged "Player" and centers the camera,
/// adjusting the Z-axis distance based on the horizontal spread between players.
/// </summary>
public class CameraController : MonoBehaviour
{
    [Header("Camera Settings")]
    [Tooltip("Vertical offset applied to the camera's Y position.")]
    [SerializeField] private float yOffset = 2.0f;

    [Tooltip("Minimum distance (Z-axis) the camera can be from the players.")]
    [SerializeField] private float minDistance = 7.5f;

    [Tooltip("Maximum distance (Z-axis) the camera can be from the players.")]
    [SerializeField] private float maxDistance = 17.5f;

    private Transform[] playerTransforms;

    private void Start()
    {
        GameObject[] allPlayers = GameObject.FindGameObjectsWithTag("Player");
        playerTransforms = new Transform[allPlayers.Length];
        for (int i = 0; i < allPlayers.Length; i++)
        {
            playerTransforms[i] = allPlayers[i].transform;
        }
    }

    private void LateUpdate()
    {
        if (playerTransforms.Length == 0)
        {
            Debug.LogWarning("[CameraController] No players found in the scene.", this);
            return;
        }

        // Initialize bounds with the first player's position
        float xMin = playerTransforms[0].position.x;
        float xMax = xMin;
        float yMin = playerTransforms[0].position.y;
        float yMax = yMin;

        // Calculate the bounding box of all players
        for (int i = 1; i < playerTransforms.Length; i++)
        {
            Vector3 pos = playerTransforms[i].position;
            if (pos.x < xMin) xMin = pos.x;
            if (pos.x > xMax) xMax = pos.x;
            if (pos.y < yMin) yMin = pos.y;
            if (pos.y > yMax) yMax = pos.y;
        }

        float xMiddle = (xMin + xMax) * 0.5f;
        float yMiddle = (yMin + yMax) * 0.5f;
        
        // Calculate required distance based on horizontal spread, clamped to min/max
        float distance = Mathf.Clamp(xMax - xMin, minDistance, maxDistance);

        transform.position = new Vector3(xMiddle, yMiddle + yOffset, -distance);
    }
}