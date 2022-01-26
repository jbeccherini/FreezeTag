using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(100)]
public class GameController : MonoBehaviour
{
    public static GameObject[] allCharacters;

    //Arena Variables
    [SerializeField] public static float arenaLength = 4.5f;
    public static GameObject tagger;

    //Character Variables
    [SerializeField] public static float maxSpeed = 1;
    [SerializeField] public static float taggerMaxSpeed = 2;
    [SerializeField] public static float timeToTarget = 0.25f;
    [SerializeField] public static float arriveRadius = 0.1f;
    [SerializeField] public static float tagRadius = .5f;
    [SerializeField] public static float targetSwitchTimer = 1.0f;

    // Start is called before the first frame update
    void Start()
    {
        allCharacters = GameObject.FindGameObjectsWithTag("Character");

        tagger = allCharacters[Random.Range(0, allCharacters.Length)];
        tagger.GetComponent<TargetController>().SetTagger(true);
    }

    public static GameObject GetTagger() 
    {
        return tagger;
    }

    public static void SetTagger(GameObject tagObject)
    {
        tagger.GetComponent<TargetController>().SetTagger(false);

        tagger = tagObject;

        tagger.GetComponent<TargetController>().SetTagger(true);
    }


}
