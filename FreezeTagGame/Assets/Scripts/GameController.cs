using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    private static GameObject[] allCharacters;

    [SerializeField] private float targetSwitchTimer = 0.5f;
    public float maxSpeed = 1f;
    public float unfreezeMaxSpeed = 1.5f;
    public float taggerMaxSpeed = 3;
    public float tagRadius = 0.75f;
    public float changeTargetRadius = 1.5f;

    public static bool changeTarget = true;

    public static GameObject tagger;

    public static GameObject center;

    // Start is called before the first frame update
    void Start()
    {
        allCharacters = GameObject.FindGameObjectsWithTag("Character");

        center = GameObject.FindGameObjectWithTag("center");

        Restart();

        StartCoroutine(ChangeTargetTimer());
    }

    public static void Restart()
    {
        foreach (var character in allCharacters)
        {
            character.GetComponent<TargetController>().SetTagged(false);
            character.GetComponent<TargetController>().SetTagger(false);
        }
        tagger = allCharacters[Random.Range(0, allCharacters.Length)];

        tagger.GetComponent<TargetController>().SetTagger(true);
        foreach (var character in allCharacters)
        {
            if (!character.GetComponent<TargetController>().GetIsTagger()) 
            {
                character.GetComponent<TargetController>().SetTarget(tagger);
            }
        }
    }

    IEnumerator ChangeTargetTimer()
    {
        for (; ; )
        {
            if (!changeTarget)
            {
                yield return new WaitForSeconds(targetSwitchTimer);
                changeTarget = true;
            }
            yield return new WaitForSeconds(0.1f);
        }
    }
}
