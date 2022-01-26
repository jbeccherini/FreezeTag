using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    private GameObject[] allCharacters;

    // Start is called before the first frame update
    void Start()
    {
        allCharacters = GameObject.FindGameObjectsWithTag("Character");

        allCharacters[Random.Range(0, allCharacters.Length)].GetComponent<TargetController>().SetTagger(true);


    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
