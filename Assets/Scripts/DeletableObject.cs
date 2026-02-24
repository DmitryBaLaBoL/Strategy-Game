using UnityEngine;

public class DeletableObject : MonoBehaviour
{
    public enum ObjectType
    {
        Building,
        Tree,
        Stone,
        Resource
    }

    public ObjectType objectType;
    public bool canBeDeleted = true;
    public GameObject deleteEffect;
    public AudioClip deleteSound;
    public Material highlightMaterial;
    public Color highlightColor = Color.red;

    private Material originalMaterial;
    private Renderer objectRenderer;
    private bool isHighlighted = false;
    private DeleteModeManager deleteModeManager;
    private SimpleIslandGenerator islandGenerator;

    void Start()
    {
        Debug.Log("Удален объект тип");
        objectRenderer = GetComponent<Renderer>();
        if (objectRenderer != null)
            originalMaterial = objectRenderer.material;

        deleteModeManager = FindObjectOfType<DeleteModeManager>();
        islandGenerator = FindObjectOfType<SimpleIslandGenerator>();

        if (deleteModeManager != null)
        {
            deleteModeManager.OnDeleteModeChanged += OnDeleteModeChanged;
        }
    }

    void OnDestroy()
    {
        if (deleteModeManager != null)
        {
            deleteModeManager.OnDeleteModeChanged -= OnDeleteModeChanged;
        }
    }

    void OnDeleteModeChanged(bool isActive)
    {
        Debug.Log("Уведомление о снаружи ");
        if (!isActive && isHighlighted)
        {
            Debug.Log("Уведомление о внутри");
            RemoveHighlight();
        }
    }

    void OnMouseEnter()
    {
        if (deleteModeManager != null && deleteModeManager.IsDeleteModeActive() && canBeDeleted)
        {
            Debug.Log("Обьект не был удалён");
            HighlightObject();
        }
    }

    void OnMouseExit()
    {
        if (isHighlighted)
        {
            RemoveHighlight();
        }
    }

    void OnMouseDown()
    {
        if (deleteModeManager != null && deleteModeManager.IsDeleteModeActive() && canBeDeleted)
        {
            DeleteObject();
        }
    }

    void HighlightObject()
    {
        if (objectRenderer != null)
        {
            if (highlightMaterial != null)
            {
                objectRenderer.material = highlightMaterial;
            }
            else
            {
                objectRenderer.material.color = highlightColor;
            }
            isHighlighted = true;
        }
    }

    void RemoveHighlight()
    {
        if (objectRenderer != null)
        {
            if (originalMaterial != null)
            {
                objectRenderer.material = originalMaterial;
            }
            isHighlighted = false;
            
        }
    }

    public void DeleteObject()
    {
        if (!canBeDeleted) return;

        // Эффекты при удалении
        if (deleteEffect != null)
        {
            Instantiate(deleteEffect, transform.position, Quaternion.identity);
        }

        if (deleteSound != null)
        {
            AudioSource.PlayClipAtPoint(deleteSound, transform.position);
        }

        // Уведомляем генератор острова об удалении
        if (islandGenerator != null)
        {
            islandGenerator.RemoveResource(gameObject);
        }

        // Уничтожаем объект
        Destroy(gameObject);

        Debug.Log("Удален объект тип");
    }
}