using System.Collections;
using UnityEngine;

public class Test: MonoBehaviour
{
    private void Start()
    {
        StartCoroutine(Coroutine());
        StartCoroutine(Coroutine1());
    }


    private void Update()
    {
    }


    private IEnumerator Coroutine()
    {
        yield return null;
    }

    private IEnumerator Coroutine1()
    {
        yield return null;
    }

}