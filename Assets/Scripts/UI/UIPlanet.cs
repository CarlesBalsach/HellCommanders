using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class UIPlanet : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public event Action<Image, PlanetDataNetwork> OnFocused;
    public event Action OnDefocused;

    [SerializeField] Image PlanetBackground;
    [SerializeField] Image PlanetBorder;
    [SerializeField] Image PlanetBottomLeft;
    [SerializeField] Image PlanetBottomRight;
    [SerializeField] Image PlanetCenter;
    [SerializeField] Image PlanetIslands;
    [SerializeField] Image PlanetTopLeft;
    [SerializeField] Image PlanetTopRight;
    [SerializeField] Image SelectedCircle;

    float _rotation;
    int _direction;

    PlanetDataNetwork _planet;

    public void Init(PlanetDataNetwork planet)
    {
        _planet = planet;

        Color water = Color.HSVToRGB(planet.ColorH / 360f, 1f, 0.3f);
        Color ground = Color.HSVToRGB(planet.ColorH / 360f, 1f, 0.8f);

        PlanetBackground.color = water;
        PlanetBorder.color = ground;
        PlanetBottomLeft.color = ground;
        PlanetBottomRight.color = ground;
        PlanetCenter.color = ground;
        PlanetIslands.color = ground;
        PlanetTopLeft.color = ground;
        PlanetTopRight.color = ground;

        PlanetBottomLeft.gameObject.SetActive(planet.Layout[0]);
        PlanetBottomRight.gameObject.SetActive(planet.Layout[1]);
        PlanetCenter.gameObject.SetActive(planet.Layout[2]);
        PlanetIslands.gameObject.SetActive(planet.Layout[3]);
        PlanetTopLeft.gameObject.SetActive(planet.Layout[4]);
        PlanetTopRight.gameObject.SetActive(planet.Layout[5]);

        if(SelectedCircle != null)
        {
            SelectedCircle.gameObject.SetActive(false);
        }

        _rotation = UnityEngine.Random.Range(0, 360f);
        _direction = UnityEngine.Random.Range(0,2) * 2 - 1;
    }

    void Update()
    {
        _rotation = _rotation + Time.deltaTime * _direction;
        transform.localRotation = Quaternion.Euler(0f, 0f, _rotation);

        if(SelectedCircle != null)
        {
            SelectedCircle.transform.localRotation = Quaternion.Euler(0f, 0f, _rotation * 5f);
        }
    }

    public void SetSelected(bool selected)
    {
        SelectedCircle.gameObject.SetActive(selected);
    }

    public void OnPointerEnter (PointerEventData eventData)
    {
        OnFocused?.Invoke(PlanetBackground, _planet);
    }

    public void OnPointerExit (PointerEventData eventData)
    {
        OnDefocused?.Invoke();
    }
}
