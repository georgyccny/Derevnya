using UnityEngine;
using UnityEngine.UI;

public class Tile : MonoBehaviour
{
    public enum TileType
    {
        Ground,
        Water,
        Forest,
        House
    }

    public TileType type;
    public int resources = 0;
    public int maxResources = 5; // New variable for maximum resources

    private SpriteRenderer spriteRenderer;
    private Text resourceText;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        CreateResourceText();
    }

    void CreateResourceText()
    {
        GameObject textObj = new GameObject("ResourceText");
        textObj.transform.SetParent(transform);
        textObj.transform.localPosition = new Vector3(0, 0, -1);
        resourceText = textObj.AddComponent<Text>();
        resourceText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        resourceText.fontSize = 14;
        resourceText.alignment = TextAnchor.MiddleCenter;
        resourceText.color = Color.white;
        resourceText.text = "";
        RectTransform rectTransform = resourceText.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(1, 1);
        rectTransform.localScale = new Vector3(0.01f, 0.01f, 1);
    }

    public void SetType(TileType newType)
    {
        type = newType;
        UpdateAppearance();
        if (type == TileType.Forest)
        {
            resources = Random.Range(1, maxResources + 1);
        }
        else
        {
            resources = 0;
        }
        UpdateResourceText();
    }

    void UpdateAppearance()
    {
        switch (type)
        {
            case TileType.Ground:
                spriteRenderer.color = new Color(0.76f, 0.70f, 0.50f); // Beige
                break;
            case TileType.Water:
                spriteRenderer.color = new Color(0.0f, 0.5f, 1.0f); // Blue
                break;
            case TileType.Forest:
                spriteRenderer.color = new Color(0.0f, 0.5f, 0.0f); // Green
                break;
            case TileType.House:
                spriteRenderer.color = new Color(0.8f, 0.8f, 0.8f); // Light Gray
                break;
        }
    }

    public bool HarvestResource()
    {
        if (resources > 0)
        {
            resources--;
            UpdateResourceText();

            // Check if resources are depleted
            if (resources == 0 && type == TileType.Forest)
            {
                ConvertToGround();
            }

            return true;
        }
        return false;
    }

    void ConvertToGround()
    {
        SetType(TileType.Ground);
        UpdateAppearance();
        UpdateResourceText();
    }

    void UpdateResourceText()
    {
        if (type == TileType.Forest && resources > 0)
        {
            resourceText.text = resources.ToString();
        }
        else
        {
            resourceText.text = "";
        }
    }
}