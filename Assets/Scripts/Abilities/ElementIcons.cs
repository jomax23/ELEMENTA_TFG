using UnityEngine;

public class ElementIcons : MonoBehaviour
{
    [Header("Rotation")]
    [SerializeField] private float rotationSpeed = 60f;

    [Header("Icons")]
    [SerializeField] private GameObject fireIcon;
    [SerializeField] private GameObject waterIcon;
    [SerializeField] private GameObject earthIcon;
    [SerializeField] private GameObject airIcon;

    private void Update()
    {
        Rotate();
    }

    private void Rotate()
    {
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
    }

    public void SetElement(ElementType element)
    {
        // Apagar todos
        fireIcon.SetActive(false);
        waterIcon.SetActive(false);
        earthIcon.SetActive(false);
        airIcon.SetActive(false);

        // Encender el correcto
        switch (element)
        {
            case ElementType.Fire:
                fireIcon.SetActive(true);
                break;

            case ElementType.Water:
                waterIcon.SetActive(true);
                break;

            case ElementType.Earth:
                earthIcon.SetActive(true);
                break;

            case ElementType.Air:
                airIcon.SetActive(true);
                break;
        }
    }
}