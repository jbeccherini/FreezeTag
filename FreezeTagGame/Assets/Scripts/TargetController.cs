using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(200)]
public class TargetController : MonoBehaviour
{
    private MeshRenderer meshRenderer;
    private Vector3 direction;
    private GameObject[] allCharacters;
    private GameObject[] characters;

    [SerializeField]  public Material normalMaterial;
    [SerializeField]  public Material taggerMaterial;
    [SerializeField]  public Material taggedMaterial;

    [SerializeField] private GameObject target;

    [SerializeField] private float speed;
    [SerializeField] private Vector3 movement;

    [SerializeField] private bool isTagged;
    [SerializeField] private bool isTagger;
    [SerializeField] private bool isTargeted;


    private static float arenaLength = 4.5f;
    public static GameObject tagger;

    //Character Variables
    private static float maxSpeed;
    private static float taggerMaxSpeed;
    private static float timeToTarget;
    private static float arriveRadius;
    private static float tagRadius;
    private static float targetSwitchTimer;

    // Start is called before the first frame update
    void Start()
    {
        maxSpeed = GameController.maxSpeed;
        taggerMaxSpeed = GameController.taggerMaxSpeed;
        timeToTarget = GameController.timeToTarget;
        arriveRadius = GameController.arriveRadius;
        tagRadius = GameController.tagRadius;
        targetSwitchTimer = GameController.targetSwitchTimer;
        allCharacters = allCharacters = GameObject.FindGameObjectsWithTag("Character");

        characters = new GameObject[allCharacters.Length-1];

        GetComponent<TaggerController>().enabled = false;

        int index = 0;

        foreach (var character in allCharacters)
        {
            if (character != this.gameObject) 
            {
                characters[index] = character;

                index++;
            }
        }
        
        meshRenderer = GetComponent<MeshRenderer>();

        if (isTagger)
        {
            target = GetClosest();
        }
        else 
        {
            target = GameController.GetTagger();
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (isTagger)
        {
            movement = KinematicPursue();
        }
        else if (isTagged)
        {
            speed = 0;

            SetMaterial("tagged");
        }
        else if (isTargeted)
        {
            movement = KinematicFlee();
        }

        movement = NormalizeMovement(movement);

        transform.Translate(movement);

        if (this.isTagger && !target.GetComponent<TargetController>().GetIsTagged())
        {
            if (GetDistance(target) < tagRadius)
            {
                target.GetComponent<TargetController>().SetTagged(true);
            }
        }
    }

    private Vector3 NormalizeMovement(Vector3 input)
    {
        Vector3 newPosition = input + transform.position;

        if (newPosition.x > arenaLength)
        {
            input = new Vector3(-(transform.position.x - (-arenaLength + (newPosition.x - arenaLength))), input.y, input.z);
        }
        else if (newPosition.x < -arenaLength)
        {
            input = new Vector3(((transform.position.x + input.x) + arenaLength * 2) - transform.position.x, input.y, input.z);
        }
        if (newPosition.z > arenaLength)
        {
            input = new Vector3(input.x, input.y, -(transform.position.z - (-arenaLength + (newPosition.z - arenaLength))));
        }
        else if (newPosition.z < -arenaLength)
        {
            input = new Vector3(input.x, input.y, ((transform.position.z + input.z) + arenaLength * 2) - transform.position.z);
        }

        return input;
    }

    public Vector3 KinematicArrive() 
    {
        direction = GetDirection(target);
        float distance = GetDistance(target);
        speed = taggerMaxSpeed;

        speed = Mathf.Min(taggerMaxSpeed, distance / timeToTarget);

        Vector3 movement = direction * speed * timeToTarget * Time.deltaTime;
        
        return movement;
    }

    public Vector3 KinematicFlee()
    {
        direction = GetDirection(target);
        speed = maxSpeed;

        Vector3 movement = -(direction) * speed * Time.deltaTime;

        return movement;
    }
    
    public Vector3 KinematicWander()
    {
        //Vector3 wanderTarget = new Vector3(Random.Range(-arenaLength,arenaLength), transform.position.y, Random.Range(-arenaLength, arenaLength));

        //direction = wanderTarget - transform.position;
        //speed = maxSpeed;

        //Vector3 movement = direction * speed/2 * Time.deltaTime;

        //return movement;
        return Vector3.zero;
    }

    public Vector3 KinematicSeek(Vector3 targetPosition) 
    {
        Vector3 directrion = targetPosition - this.transform.position;

        Vector3 normalizedvelocityDirection = directrion / (directrion.magnitude);

        Vector3 movement = speed * normalizedvelocityDirection * Time.deltaTime;

        return movement;
    }

    public Vector3 KinematicPursue()
    {
        speed = taggerMaxSpeed;

        float distance = GetDistance(target);
        float timeToTarget = distance / speed;

        Vector3 velocity = target.GetComponent<TargetController>().GetVelocity() / Time.deltaTime;

        Vector3 targetPosition = target.transform.position + velocity * timeToTarget;
        
        targetPosition = NormalizeMovement(targetPosition);

        return KinematicSeek(targetPosition);
    }

    private float GetDistance(GameObject target) 
    {
        float distance;

        float length1 = Mathf.Abs(transform.position.x - target.transform.position.x);
        float height1 = Mathf.Abs(transform.position.z - target.transform.position.z);

        float length2 = Mathf.Abs(arenaLength*2 - length1);
        float height2 = Mathf.Abs(arenaLength*2 - height1);

        Vector2 distance1 = new Vector2(length1, height1);
        Vector2 distance2 = new Vector2(length2, height1);
        Vector2 distance3 = new Vector2(length1, height2);
        Vector2 distance4 = new Vector2(length2, height2);

        distance = Mathf.Min(distance1.magnitude, distance2.magnitude, distance3.magnitude, distance4.magnitude);

        return distance;
    }

    private Vector3 GetDirection(GameObject target)
    {
        float distance;

        float length1 = (target.transform.position.x - transform.position.x);
        float height1 = (target.transform.position.z - transform.position.z);

        float length2 = Mathf.Abs(2.0f*arenaLength - Mathf.Abs(length1));
        float height2 = Mathf.Abs(2.0f*arenaLength - Mathf.Abs(height1));

        if (length1 > 0) 
        {
            length2 *= -1.0f;
        }

        if (height1 > 0)
        {
            height2 *= -1.0f;
        }

        Vector3[] directions = new Vector3[4];
        directions[0] = new Vector3(length1, 0, height1);
        directions[1] = new Vector3(length2, 0, height1);
        directions[2] = new Vector3(length1, 0, height2);
        directions[3] = new Vector3(length2, 0, height2);

        float[] magnitudes = new float[4];

        magnitudes[0] = directions[0].magnitude;
        magnitudes[1] = directions[1].magnitude;
        magnitudes[2] = directions[2].magnitude;
        magnitudes[3] = directions[3].magnitude;

        distance = Mathf.Min(magnitudes);

        for (int i = 0; i < magnitudes.Length; i++)
        {
            if (magnitudes[i] == distance) 
            {
                return directions[i];
            }
        }

        return directions[0];
    }

    public GameObject GetClosest() 
    {
        GameObject closest = null;

        float distance = -1;

        foreach (var character in characters)
        {
            if (character.GetComponent<TargetController>().GetIsTagged()) 
            {
                continue;
            }
            if (distance == -1) 
            {
                closest = character;
                distance = GetDistance(character);
            }
            if( GetDistance(character) < distance)
            {
                closest = character;
                distance = GetDistance(character);
            }
        }

        if (closest == null)
        {
            Debug.Log("Game Over!");
        }

        return closest;
    }

    public bool GetIsTagged() 
    {
        return isTagged;
    }
    
    public bool GetIsTagger() 
    {
        return isTagger;
    }
    
    public bool GetIsTargeted() 
    {
        return isTargeted;
    }

    public void SetTagged(bool value)
    {
       isTagged = value;

        if (isTagged)
        {
            meshRenderer.material = taggedMaterial;
        }
        else 
        {
            meshRenderer.material = normalMaterial;
        }
    }
    
    public void SetTagger(bool value)
    {
       isTagger = value;

        GetComponent<TaggerController>().enabled =value;

        if (true)
        {
            SetMaterial("tagger");
        }
    }
    
    public void SetTargeted(bool value)
    {
       isTargeted = value;

        if (value) 
        {
            target = GameController.GetTagger();
        }
        else
        {
            target = null;
        }
    }

    public float GetSpeed() 
    {
        return speed;
    }

    public Vector3 GetVelocity() 
    {
        return KinematicFlee();
    }

    public GameObject GetTarget() 
    {
        return target;
    }
    
    public void SetTarget(GameObject targetObject) 
    {
        target = targetObject;
    }

    public void SetMaterial(string type) 
    {
        switch (type) 
        {
            case "tagger": meshRenderer.material = taggerMaterial; break;
            case "tagged": meshRenderer.material = taggedMaterial; break;
            default: meshRenderer.material = normalMaterial; break;
        }
    }

}
