using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UILoading : MonoBehaviour
{

    const float LADNPAGE_TIME = 1f;

    float _chrono = 0f;

    void Update()
    {
        _chrono += Time.deltaTime;
        if(_chrono > LADNPAGE_TIME)
        {
            Loader.Load(Loader.Scene.MainScene);
        }
    }
}
