using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaggerController : TargetController
{
    private GameObject target;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(ChooseTarget());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator ChooseTarget()
    {
        for (; ; )
        {
            SetTarget(GetClosest());

            yield return new WaitForSeconds(GameController.targetSwitchTimer);
        }
    }
}
