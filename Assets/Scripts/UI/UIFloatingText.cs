using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIFloatingText : MonoBehaviour
{
    const float FT_MARGIN = 10f;
    public RectTransform Frame;
    public TMP_Text Text;
    public Canvas canvas;

    bool _fitHorizontal = false;
    bool _fitVertical = false;

    RectTransform _target;
    
    void Update()
    {
        gameObject.SetActive(_target != null);
    }

    public void Show(RectTransform icon, string text, bool fitHorizontal, bool fitVertical)
    {
        gameObject.SetActive(true);
        _target = icon;
        Text.text = text;
        Vector2 ft_size = new Vector2(Text.preferredWidth + FT_MARGIN * 2f, Text.preferredHeight + FT_MARGIN * 2f);
        Frame.sizeDelta = ft_size;
        _fitHorizontal = fitHorizontal;
        _fitVertical = fitVertical;
        PositionFloatingText(icon);
    }

    public void Hide()
    {
        _target = null;
    }

    void PositionFloatingText(RectTransform icon)
    {
        Vector2 icon_size = icon.rect.size;
        Vector2 ft_size = Frame.rect.size;

        // Calculate potential positions
        List<Vector3> positions = new List<Vector3>();
        float scaleFactor = canvas.scaleFactor;

        // Calculate positions relative to the icon's position
        if(_fitVertical)
        {
            // Up
            positions.Add(icon.transform.position.ToV2() + new Vector2(0f, icon_size.y / 2 + ft_size.y / 2) * scaleFactor);
            // Down
            positions.Add(icon.transform.position.ToV2() + new Vector2(0f, -icon_size.y / 2 - ft_size.y / 2) * scaleFactor);
        }
        
        if(_fitHorizontal)
        {
            // Right
            positions.Add(icon.transform.position.ToV2() + new Vector2(icon_size.x / 2 + ft_size.x / 2, 0f) * scaleFactor);
            // Left
            positions.Add(icon.transform.position.ToV2() + new Vector2(-icon_size.x / 2 - ft_size.x / 2, 0f) * scaleFactor);
        }
        
        // Find the position closest to the center of the screen
        Vector3 screenCenter = new Vector3(Screen.width / 2, Screen.height / 2, 0);
        int closestIndex = 0;
        float minDistance = Vector3.Distance(positions[0], screenCenter);

        for (int i = 1; i < positions.Count; i++)
        {
            float distance = Vector3.Distance(positions[i], screenCenter);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestIndex = i;
            }
        }

        // Set the position of the floating text
        Frame.transform.position = positions[closestIndex]; // Use anchoredPosition for RectTransform positioning

        FitInScreen();
    }

    void FitInScreen()
    {
        Vector3 pos = transform.position;
        Vector2 ft_size = Frame.rect.size * canvas.scaleFactor;
        
        float marginBottom = transform.position.y - ft_size.y / 2f;
        if(marginBottom < 0)
        {
            pos.y -= marginBottom;
        }
        
        float marginTop = Screen.height - (transform.position.y + ft_size.y / 2f);
        if(marginTop < 0)
        {
            pos.y += marginTop;
        }

        float marginLeft = transform.position.x - ft_size.x / 2f;
        if(marginLeft < 0)
        {
            pos.x -= marginLeft;
        }

        float marginRight = Screen.width - (transform.position.x + ft_size.x / 2f);
        if(marginRight < 0f)
        {
            pos.x += marginRight;
        }

        transform.position = pos;
    }
}
