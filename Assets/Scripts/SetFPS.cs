using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetFPS : MonoBehaviour
{
    public int _fps = 30;

    void Update()
    {
        QualitySettings.vSyncCount = 1;
	Application.targetFrameRate = _fps;
    }
}
