using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetController : MonoBehaviour
{
    private static GameController gc;

    private MeshRenderer meshRenderer;
    private Vector3 direction;
    private GameObject[] allCharacters;
    private GameObject[] characters;

    [SerializeField]  public Material normalMaterial;
    [SerializeField]  public Material taggerMaterial;
    [SerializeField]  public Material taggedMaterial;

    [SerializeField] private float arenaLength = 4.5f;

    [SerializeField] private GameObject oldTarget;
    [SerializeField] private GameObject target;
    [SerializeField] private float speed;
    [SerializeField] private float timeToTarget = 0.25f;
    [SerializeField] private float arriveRadius = 0.1f;
    [SerializeField] private bool isTagged;
    [SerializeField] private bool isTagger;
    [SerializeField] private bool isTargeted;

    private float maxSpeed;
    private float unfreezeMaxSpeed;
    private float taggerMaxSpeed;
    private float tagRadius;
    private float changeTargetRadius;

    // Start is called before the first frame update
    void Start()
    {
       if (gc == null) 
        {
            gc = new GameController();
        }

        maxSpeed = gc.maxSpeed;
        taggerMaxSpeed = gc.taggerMaxSpeed;
        tagRadius = gc.tagRadius;
        changeTargetRadius = gc.changeTargetRadius;
        unfreezeMaxSpeed = gc.unfreezeMaxSpeed;

        allCharacters = GameObject.FindGameObjectsWithTag("Character");
        characters = new GameObject[allCharacters.Length-1];

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

        target = gc.tagger;
    }

    // Update is called once per frame
    void Update()
    {

        if (isTagger)
        {
            foreach (var character in allCharacters)
            {
                if (character.gameObject != this.gameObject) 
                {
                    character.GetComponent<TargetController>().SetTarget(this.gameObject);
                }
            }

            SetTarget(GetClosest());
        }

        Vector3 movement = Vector3.zero;

        if (isTagger)
        {
            movement = KinematicPursue();
        }
        else if (isTagged)
        {
            speed = 0;
            meshRenderer.material = taggedMaterial;
        }
        else if (isTargeted)
        {
            int numUntagged = 0;
            foreach (var character in characters)
            {
                if (character.GetComponent<TargetController>().isTagger || character.GetComponent<TargetController>().isTagged)
                {
                    continue;
                }

                numUntagged++;
            }
            if (numUntagged == 0)
            {
                target = GetClosestFrozen();
                if (target != null)
                {
                    speed = unfreezeMaxSpeed;
                    movement = KinematicSeek(target.transform.position);
                }
            }
            else 
            {
                movement = KinematicFlee();
            }
        }
        else 
        {
            target = GetClosestFrozen();
            if (target != null) 
            {
                speed = unfreezeMaxSpeed;
                movement = KinematicSeek(target.transform.position);
            }
        }

         Vector3 newPosition = movement + transform.position;

        if (newPosition.x > arenaLength)
        {
            movement = new Vector3(-(transform.position.x - (-arenaLength + (newPosition.x - arenaLength))), movement.y, movement.z);
        }
        else if (newPosition.x < -arenaLength)
        {
            movement = new Vector3(((transform.position.x + movement.x) + arenaLength * 2) - transform.position.x, movement.y, movement.z);
        }
        if (newPosition.z > arenaLength)
        {
            movement = new Vector3(movement.x, movement.y, -(transform.position.z - (-arenaLength + (newPosition.z - arenaLength))));
        }
        else if (newPosition.z < -arenaLength)
        {
            movement = new Vector3(movement.x, movement.y, ((transform.position.z + movement.z) + arenaLength * 2) - transform.position.z);
        }

        transform.Translate(movement);

        if (this.isTagger && !target.GetComponent<TargetController>().GetIsTagged()) 
        {
            if(GetDistance(target) < tagRadius) 
            {
                target.GetComponent<TargetController>().SetTagged(true);
                GameController.changeTarget = true;

                SetTarget(GetClosest());
            }
        }

        if (!this.isTagged && !isTagger) 
        {
            foreach (var character in characters)
            {
                if (character.GetComponent<TargetController>().isTagged) 
                {
                    if (GetDistance(character) < tagRadius && GetDistance(gc.tagger) > tagRadius) 
                    {
                        character.GetComponent<TargetController>().SetTagged(false);
                    }
                }
            }
        }
    }

    public Vector3 KinematicArrive() 
    {
        direction = GetDirection(target);
        float distance = GetDistance(target);
        speed = taggerMaxSpeed;

        if (distance < arriveRadius)
        {
            speed = 0;
        }
        else
        {
            speed = Mathf.Min(taggerMaxSpeed, distance / timeToTarget);
        }

        Vector3 movement = direction * speed * timeToTarget * Time.deltaTime;
        
        return movement;
    }

    public Vector3 KinematicFlee()
    {
        direction = GetDirection(target);
        speed = maxSpeed;

        Vector3 movement = -direction * speed * Time.deltaTime;

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

        Vector3 velocity = target.GetComponent<TargetController>().GetVelocity();

        Vector3 targetPosition = target.transform.position + velocity;
        Vector3 inputPosition = targetPosition;

        if (targetPosition.x > arenaLength)
        {
            inputPosition = new Vector3(-(transform.position.x - (-arenaLength + (inputPosition.x - arenaLength))), targetPosition.y, targetPosition.z);
        }
        else if (targetPosition.x < -arenaLength)
        {
            inputPosition = new Vector3(((transform.position.x + targetPosition.x) + arenaLength * 2) - transform.position.x, targetPosition.y, targetPosition.z);
        }
        if (targetPosition.z > arenaLength)
        {
            inputPosition = new Vector3(targetPosition.x, targetPosition.y, -(transform.position.z - (-arenaLength + (inputPosition.z - arenaLength))));
        }
        else if (targetPosition.z < -arenaLength)
        {
            inputPosition = new Vector3(targetPosition.x, targetPosition.y, ((transform.position.z + targetPosition.z) + arenaLength * 2) - transform.position.z);
        }

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
            GameController.Restart();
        }

        return closest;
    }

    public GameObject GetClosestFrozen()
    {
        GameObject closest = null;

        float distance = -1;

        foreach (var character in characters)
        {
            if (!character.GetComponent<TargetController>().GetIsTagged())
            {
                continue;
            }
            if (distance == -1)
            {
                closest = character;
                distance = GetDistance(character);
            }
            if (GetDistance(character) < distance)
            {
                closest = character;
                distance = GetDistance(character);
            }
        }

        if (closest == null)
        {
            return null;
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

        if (true)
        {
            GetComponent<MeshRenderer>().material = taggerMaterial;
            gc.tagger = this.gameObject;
        }
    }
    
    public void SetTargeted(bool value)
    {
       isTargeted = value;

        if (value) 
        {
            target = GetTagger();
        }
        else
        {
            target = null;
        }
    }

    public GameObject GetTagger() 
    {
        foreach (var character in allCharacters)
        {
            if (character.GetComponent<TargetController>().GetIsTagger()) 
            {
                return character;
            }
        }

        return null;    
    }

    public float GetSpeed() 
    {
        return speed;
    }

    public Vector3 GetVelocity() 
    {
        return KinematicFlee();
    }

    public void SetTarget(GameObject obj) 
    {
        if (this.isTagger)
        {
            //if (GetDistance(target) < 3f) 
            //{
                if (GameController.changeTarget)
                {
                    GameController.changeTarget = false;

                    if (target != null) 
                    {
                        target.GetComponent<TargetController>().isTargeted = false;
                    }
                    
                    target = obj;
                    target.GetComponent<TargetController>().isTargeted = true;

                    Debug.Log("Log is now "+ target.name);
                }
                else
                {
                    foreach (var character in characters)
                    {
                        if (character.GetComponent<TargetController>().isTagged)
                        {
                            continue;
                        }

                        if (GetDistance(character) < changeTargetRadius)
                        {
                            //GameController.changeTarget = true;
                        }
                    }
                }
            //}
        }
        else 
        {
            target = obj;
        }
    }
    
    public GameObject GetTarget() 
    {
        return target;
    }

    


}
