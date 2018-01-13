using System.Collections;
using UnityEngine;
using WWFramework.Util;

public class Test: MonoBehaviour
{
    private void Start()
    {
        StartCoroutine(Coroutine());
        StartCoroutine(Coroutine1());

        for (int i = 0; i < 999; i++)
        {
            ThreadManager.Instance.AddTask(Log, null);
        }
    }


    private void Log()
    {
        for (int i = 0; i < 999; i++)
        {
            Debug.Log(i);
        }
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