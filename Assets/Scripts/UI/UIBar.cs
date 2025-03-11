using UnityEngine;
using UnityEngine.UI;

public class UIBar : MonoBehaviour
{

    [SerializeField] Image BarBorder;
    [SerializeField] Image Bar;

    public void UpdateBar(float w) // w: float from 0 to 1
    {
        w = Mathf.Clamp01(w);
        Bar.transform.localScale = new Vector3(w, 1f, 1f);
    }
}
