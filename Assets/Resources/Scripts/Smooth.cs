using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class SmoothPosition
{
    public bool isCoroutineRunning { get; set; }

    public IEnumerator BeginSmoothing(Transform t,Vector2 d,float time)
    {
        Smooth.SmoothTranslationCoroutine(t,d,time);
        isCoroutineRunning = true;
        while (t.position.x != d.x || t.position.y != d.y)
        {
            isCoroutineRunning = true;
        }
        isCoroutineRunning = false;
        yield return null;
    }
}

static class Smooth{

    static public IEnumerator SmoothTranslationCoroutine(Transform transform, Vector2 destination, float duration)
    {
        float elapsedTime = 0;
        Vector3 startingposition=transform.position;

        while(elapsedTime<duration)
        {
            transform.position = Vector2.Lerp(startingposition, destination, elapsedTime/duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }
}
