using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class AnimalBehaviour : MonoBehaviour
{

    #region Variables

    // constants
    private const int Turn = 50;                     // how fast the mouse changes direction
    private const int Thirst = 2000;                 // how long it is statisfied with water
    private const int Parch = 2000;                  // how long it doesnt parch
    private const int Hunger = 3000;                 // how long it is statisfied with food
    private const int Starve = 3000;                 // how long it doesnt starve
    private const int Libido = 4000;                 // how long the mouse is statisfied with a partner
    private const int Adult = 1000;                  // how long the mouse cannot mate yet
    private const int LifeTime = 50000;              // how long the mouse lives at max
    
    // gene mutations ranges
    private const float ViewDensityMutationRange = 0.5f;
    private const float ViewWidthMutationRange = 0.5f;
    private const int ViewRangeMutationRange = 5;
    private const int TenacityMutationRange = 100;
    private const float AppealMutationRange = 0.05f;
    private const float MoveSpeedMutationRange = 1.0f;
    private const float MoveSpeedMultiplyerMutationRange = 0.1f;
    private const float ZigZagMutationRange = 0.05f;
    
    // game controller constants (data thats fetched from the game controller)
    private int id;
    private Genome genes;

    // gene value copies (needed cause those values get modified)
    private float viewDensity;
    private float viewWidth;
    private float moveSpeed;

    // variables
    private bool waterSearch;        // currently in search for water
    private bool waterFound;         // found water now and moves towards it
    private bool drinking;           // currently in the coroutine "Drink"
    private bool foodSearch;         // currently in search for food
    private bool foodFound;          // found food and now moves towards it
    private bool eating;             // currently in the coroutine "Eat"
    private bool partnerSearch;      // currently in serach for a partner
    private bool partnerFound;       // found a partner and tries to get its attention
    private bool mating;             // currently in the coroutine "Mate"
    private bool grown;              // grown up
    
    private bool alive;                                             // if the mouse is alive
    private bool collidingBorder;
    private bool collidingBorderN;
    private bool collidingBorderS;
    private bool collidingBorderW;
    private bool collidingBorderE;
    private float turnTime;                                         // the time counter for the slerp turn animation
    private float deathTime;                                        // the time counter for the slerp death animation
    private Priority priority;                                      // priority of water or food
    private Vector3 direction;                                      // the move direction
    private Quaternion lastRotation;                                // the last rotation before starting a slerp rotate
    private Transform nearestWater;                                 // the selected water
    private Transform nearestPlant;                                 // the selected food
    private GameObject partner;                                     // the selected partner
    private List<Transform> waterInSight;                           // the water the mouse sees at the moment
    private List<Transform> foodInSight;                            // the food the mouse sees at the moment
    private List<GameObject> partnerInSight;                        // the mice he mouse sees at the moment
    private AudioSource bingSound;                                  // the bing (or alternatively bang) sound source
    private System.Random random;                                   // a system random

    private Timer lifeTimer;            // getting old
    private Timer waterTimer;           // developing thirst
    private Timer parchTimer;           // parching
    private Timer foodTimer;            // developing hunger
    private Timer starveTimer;          // starving
    private Timer partnerTimer;         // developing libido
    private Timer courtshipTimer;       // getting frustrated
    private Timer growTimer;            // growing up
    private Timer turnTimer;            // changing direction while idle moving
    
    // unity variables
    public GameObject mouse;
    public Material standardMaterial;
    public Material glowMaterial;
    
    #endregion
    
    private void Start()
    {
        
        #region Initializiation
        
        // game controller constants
        id = GameController.Instance.GetMouseId();
        GameController.Instance.IncMouseId();
        genes = GameController.Instance.GetMouseGenes();
        
        // gene value copies
        viewDensity = genes.ViewDensity;
        viewWidth = genes.ViewWidth;
        moveSpeed = genes.MoveSpeed;
        
        // variables
        waterSearch = false;
        waterFound = false;
        drinking = false;
        foodSearch = false;
        foodFound = false;
        eating = false;
        partnerSearch = false;
        partnerFound = false;
        mating = false;
        grown = false;
        
        alive = true;
        collidingBorder = false;
        collidingBorderN = false;
        collidingBorderS = false;
        collidingBorderW = false;
        collidingBorderE = false;
        turnTime = 0.0f;
        deathTime = 0.0f;
        priority = Priority.Idle;
        foodInSight = new List<Transform>();
        waterInSight = new List<Transform>();
        partnerInSight = new List<GameObject>();
        bingSound = GetComponent<AudioSource>();
        random = new System.Random(Guid.NewGuid().GetHashCode());
        UnityEngine.Random.InitState((int) DateTime.Now.Ticks);
        
        lifeTimer = new Timer(LifeTime / GameController.Instance.GetSpeed());
        waterTimer = new Timer(Thirst / GameController.Instance.GetSpeed());
        parchTimer = new Timer(Parch / GameController.Instance.GetSpeed());
        foodTimer = new Timer(Hunger / GameController.Instance.GetSpeed());
        starveTimer = new Timer(Starve / GameController.Instance.GetSpeed());
        partnerTimer = new Timer(Libido / GameController.Instance.GetSpeed());
        courtshipTimer = new Timer(genes.Tenacity / GameController.Instance.GetSpeed());
        growTimer = new Timer(Adult / GameController.Instance.GetSpeed());
        turnTimer = new Timer(Turn / GameController.Instance.GetSpeed());
        
        // initial transform (NextDouble returns a random value in [0.0, 1.0) so 1 - NextDouble returns a value in (0.0, 1.0])
        direction = Vector3.Normalize(new Vector3(1 - (float) random.NextDouble(), 0, 1 - (float) random.NextDouble()));
        if (random.Next(2) == 1)
            direction *= -1;
        transform.Rotate(transform.rotation.x, (float) -VectorToAngle(direction), transform.rotation.z);

        #endregion
    
    }

    private void FixedUpdate()
    {
        
        #region Death
        
        lifeTimer.Inc();
        if (lifeTimer.Evaluate() && !mating)
            Die();
        
        if (!alive)
        {
            waterFound = false;
            waterSearch = false;
            foodFound = false;
            foodSearch = false;
            partnerSearch = false;
            partnerFound = false;
            waterTimer.Reset();
            foodTimer.Reset();
            partnerTimer.Reset();
            waterTimer.Deactivate();
            parchTimer.Deactivate();
            foodTimer.Deactivate();
            starveTimer.Deactivate();
            partnerTimer.Deactivate();
            foreach (GameObject currentMouse in GameObject.FindGameObjectsWithTag("Mouse"))
            {
                if (currentMouse.GetComponent<AnimalBehaviour>().GetPartner() != null && currentMouse.GetComponent<AnimalBehaviour>().GetPartner().GetComponent<AnimalBehaviour>().GetId() == id)
                    currentMouse.GetComponent<AnimalBehaviour>().ClearPartner();
            }
        }

        #endregion
        
        #region Drink
        
        waterTimer.Inc();
        if (waterTimer.Evaluate() && !drinking && !waterFound && !waterSearch)
        {
            waterSearch = true;
            SetPriority();
        }

        if (waterSearch)
        {
            parchTimer.Inc();
            if (priority == Priority.Water)
            {
                
                MoveRandom();
                ScanArea(priority);

                // find water
                if (waterInSight.Count > 0)
                {
                    nearestWater = waterInSight.First();
                    foreach (Transform water in waterInSight)
                    {
                        if ((water.position - transform.position).magnitude < (nearestWater.position - transform.position).magnitude)
                            nearestWater = water;
                    }
                    waterInSight.Clear();
                    waterSearch = false;
                    waterFound = true;
                }
                
            }
        }
        
        if (waterFound)
        {
            parchTimer.Inc();
            direction = Vector3.Normalize(new Vector3(nearestWater.position.x - transform.position.x, 0, nearestWater.position.z - transform.position.z));
            MoveAlong(direction);
            
            if (!drinking && (nearestWater.position - transform.position).magnitude < 7)
            {
                waterFound = false;
                drinking = true;
                StartCoroutine(Drink());
            }
        }

        if (parchTimer.Evaluate() && !drinking)
            Die();
        
        #endregion
        
        #region Eat
        
        foodTimer.Inc();
        if (foodTimer.Evaluate() && !eating && !foodFound && !foodSearch)
        {
            foodSearch = true;
            SetPriority();
        }

        if (foodSearch)
        {
            starveTimer.Inc();
            if (priority == Priority.Food)
            {
                
                MoveRandom();
                ScanArea(priority);

                // find food
                if (foodInSight.Count > 0)
                {
                    nearestPlant = foodInSight.First();
                    foreach (Transform plant in foodInSight)
                    {
                        if ((plant.position - transform.position).magnitude < (nearestPlant.position - transform.position).magnitude)
                            nearestPlant = plant;
                    }
                    foodInSight.Clear();
                    foodSearch = false;
                    foodFound = true;
                }
                
            }
        }
        
        if (foodFound)
        {
            
            if (nearestPlant == null)
            {
                foodFound = false;
                foodSearch = true;
            }
            else
            {
                
                starveTimer.Inc();
                direction = Vector3.Normalize(new Vector3(nearestPlant.position.x - transform.position.x, 0, nearestPlant.position.z - transform.position.z));
                MoveAlong(direction);
                
                if (!eating && (nearestPlant.position - transform.position).magnitude < 1.2 + GameController.Instance.GetSpeed() / 10f)
                {
                    foodFound = false;
                    eating = true;
                    StartCoroutine(Eat(nearestPlant.gameObject));
                }
                
            }
        }

        if (starveTimer.Evaluate() && !eating)
            Die();

        #endregion

        #region Mate
        
        if (grown)
        {
            
            partnerTimer.Inc();
            if (partnerTimer.Evaluate() && !mating && !partnerFound && !partnerSearch)
            {
                partnerSearch = true;
                SetPriority();
            }
            
            if (partnerSearch && priority == Priority.Partner)
            {
                MoveRandom();
                ScanArea(priority);
                
                // find partner
                foreach (GameObject potentialPartner in partnerInSight)
                {
                    float chooseProbability = potentialPartner.GetComponent<AnimalBehaviour>().GetGenes().Appeal;
                    if (chooseProbability > 1)
                        chooseProbability = 1;
                    if (UnityEngine.Random.value <= chooseProbability * 0.02f * GameController.Instance.GetSpeed()) // Like this the probability to get chosen every SECOND is equals to Appeal, and not every FIXEDUPDATE
                    {
                        partner = potentialPartner;
                        partnerSearch = false;
                        partnerFound = true;
                        moveSpeed *= genes.MoveSpeedMultiplyer;
                        break;
                    }
                }
                
            }

            if (partnerFound)
            {
                courtshipTimer.Inc();
                if (courtshipTimer.Evaluate() || partner == null || (partner.GetComponent<AnimalBehaviour>().GetMating() && partner.GetComponent<AnimalBehaviour>().GetPartner() != null && partner.GetComponent<AnimalBehaviour>().GetPartner().GetComponent<AnimalBehaviour>().GetId() != id))
                {
                    courtshipTimer.Reset();
                    partnerSearch = true;
                    partnerFound = false;
                    partner = null;
                    moveSpeed /= genes.MoveSpeedMultiplyer;
                }
                else
                {
                    
                    direction = Vector3.Normalize(partner.transform.position + partner.GetComponent<AnimalBehaviour>().GetDirection() * 5 - transform.position);
                    MoveAlong(direction);
                    if (partner.GetComponent<AnimalBehaviour>().GetPartner() != null && partner.GetComponent<AnimalBehaviour>().GetPartner().GetComponent<AnimalBehaviour>().GetId() == id)
                    {
                        courtshipTimer.Reset();
                        moveSpeed /= genes.MoveSpeedMultiplyer;
                        partnerFound = false;
                        mating = true;
                        StartCoroutine(Mate());
                    }
                    
                }

            }
            
        }
        else
        {
            growTimer.Inc();
            if (growTimer.Evaluate())
                grown = true;
        }
        
        #endregion
        
        #region Move
        
        /* // this method would be more realistic but the one below ensures stability within a population
        if (transform.position.x > 100)
            transform.position = new Vector3(transform.position.x - 200, transform.position.y, transform.position.z);
        if (transform.position.x < -100)
            transform.position = new Vector3(transform.position.x + 200, transform.position.y, transform.position.z);
        if (transform.position.z > 100)
            transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z - 200);
        if (transform.position.z < -100)
            transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z + 200);
        */
        
        if (collidingBorder)
        {
            if (collidingBorderN)
                direction = new Vector3(0f, 0f, -1f);
            if (collidingBorderS)
                direction = new Vector3(0f, 0f, 1f);
            if (collidingBorderW)
                direction = new Vector3(1f, 0f, 0f);
            if (collidingBorderE)
                direction = new Vector3(-1f, 0f, 0f);
            MoveAlong(direction);
        }
        else
        {
            if (alive && !drinking && !waterFound && !waterSearch && !eating && !foodFound && !mating && !partnerFound && !partnerSearch)
                MoveRandom();
        }
        
        #endregion
        
        #region Color

        if (gameObject.GetComponent<Renderer>().material != standardMaterial && !mating)
        {
            Destroy(gameObject.GetComponent<Renderer>().material);
            gameObject.GetComponent<Renderer>().material = standardMaterial;
        }


        if (!alive)
        {
            gameObject.GetComponent<Renderer>().material.color = Color.black;
        }
        else
        {
            
            if (priority == Priority.Water && waterSearch)
            {
                gameObject.GetComponent<Renderer>().material.color = new Color(0.5f, 0.5f, 1f, 1);
            }
            else if (priority == Priority.Water && waterFound)
            {
                gameObject.GetComponent<Renderer>().material.color = Color.blue;
            }
            else if (drinking)
            {
                gameObject.GetComponent<Renderer>().material.color = new Color(0, 0, 0.5f, 1);
            }

            else if (priority == Priority.Food && foodSearch)
            {
                gameObject.GetComponent<Renderer>().material.color = new Color(1f, 0.5f, 0.5f, 1);
            }
            else if (priority == Priority.Food && foodFound)
            {
                gameObject.GetComponent<Renderer>().material.color = Color.red;
            }
            else if (eating)
            {
                gameObject.GetComponent<Renderer>().material.color = new Color(0.5f, 0, 0, 1);
            }
            
            else if (priority == Priority.Partner && partnerSearch)
            {
                gameObject.GetComponent<Renderer>().material.color = new Color(1f, 1f, 0.75f, 1);
            }
            else if (priority == Priority.Partner && partnerFound)
            {
                gameObject.GetComponent<Renderer>().material.color = Color.yellow;
            }
            else if (mating)
            {
                Destroy(gameObject.GetComponent<Renderer>().material);
                gameObject.GetComponent<Renderer>().material = glowMaterial;
            }
            
            else
                gameObject.GetComponent<Renderer>().material.color = Color.white;
        }

        #endregion

    }

    #region Unity Functions

    private void OnTriggerEnter(Collider other)
    {
        collidingBorder = true;
        if (other.gameObject.name.Equals("BorderNorth"))
            collidingBorderN = true;
        if (other.gameObject.name.Equals("BorderSouth"))
            collidingBorderS = true;
        if (other.gameObject.name.Equals("BorderWest"))
            collidingBorderW = true;
        if (other.gameObject.name.Equals("BorderEast"))
            collidingBorderE = true;
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.name.Equals("BorderNorth"))
            collidingBorderN = false;
        if (other.gameObject.name.Equals("BorderSouth"))
            collidingBorderS = false;
        if (other.gameObject.name.Equals("BorderWest"))
            collidingBorderW = false;
        if (other.gameObject.name.Equals("BorderEast"))
            collidingBorderE = false;
        if (!collidingBorderN && !collidingBorderS && !collidingBorderW && !collidingBorderE)
            collidingBorder = false;
    }

    #endregion
    
    #region Helper Functions
    
    private void Die()
    {
        if (alive)
        {
            alive = false;
            GameController.Instance.DecCurrentMice();
            lastRotation = transform.rotation;
        }
        StartCoroutine(DeathDelay());
        transform.rotation = Quaternion.Slerp(lastRotation, Quaternion.Euler(new Vector3(90, lastRotation.y, lastRotation.z)), deathTime * GameController.Instance.GetSpeed());
        deathTime += 0.02f;
    }
    
    private void MoveAlong(Vector3 moveDirection)
    {
        transform.position += moveDirection * (0.1f * moveSpeed * GameController.Instance.GetSpeed());
        transform.rotation = Quaternion.Slerp(lastRotation, Quaternion.Euler(new Vector3(lastRotation.x, (float) -VectorToAngle(moveDirection), lastRotation.z)), turnTime * GameController.Instance.GetSpeed());
        turnTime += 0.1f;
    }
    
    private void MoveRandom()
    {
        
        turnTimer.Inc();
        if (turnTimer.Evaluate())
        {
            turnTimer.Reset();
            lastRotation = transform.rotation;
            turnTime = 0.0f;
            
            // calculate new direction
            float deviationX = UnityEngine.Random.value * genes.ZigZag;
            if (random.Next(2) == 1)
                deviationX *= -1;
            float deviationZ = UnityEngine.Random.value * genes.ZigZag;
            if (random.Next(2) == 1)
                deviationZ *= -1;
            direction = Vector3.Normalize(new Vector3(direction.x + deviationX, 0, direction.z + deviationZ));
        }
        
        // move
        MoveAlong(direction);
        
    }
    
    private void ScanArea(Priority objective)
    {

        double angle = VectorToAngle(direction);
        int segments = Mathf.RoundToInt(2 * viewWidth * viewDensity);
        int segmentDeg = (int) ((1 / viewDensity) * 10);
        
        if (objective == Priority.Partner)
            partnerInSight.Clear();

        for (int i = 0; i <= segments; i++)
        {
            Vector3 adaptedVector = AngleToVector(angle - segments / 2 * segmentDeg + i * segmentDeg);
            if (Physics.Raycast(transform.position, adaptedVector, out var hit, genes.ViewRange))
            {
                if (objective == Priority.Food)
                {
                    if (hit.transform.gameObject.CompareTag("Food") && !foodInSight.Contains(hit.transform))
                        foodInSight.Add(hit.transform);
                }
                else if (objective == Priority.Water)
                {
                    if (hit.transform.gameObject.CompareTag("Water") && !waterInSight.Contains(hit.transform))
                        waterInSight.Add(hit.transform);
                }
                else if (objective == Priority.Partner)
                {
                    if (hit.transform.gameObject.CompareTag("Mouse") && hit.transform.gameObject.GetComponent<AnimalBehaviour>().GetId() != id && hit.transform.gameObject.GetComponent<AnimalBehaviour>().GetGrown() && (hit.transform.gameObject.GetComponent<AnimalBehaviour>().GetPartner() == null || hit.transform.gameObject.GetComponent<AnimalBehaviour>().GetPartner().GetComponent<AnimalBehaviour>().GetId() == id) && !partnerInSight.Contains(hit.transform.gameObject)) 
                        partnerInSight.Add(hit.transform.gameObject);
                }
            }
            //Debug.DrawLine(transform.position, transform.position + adaptedVector * viewRange);
        }
        
    }
    
    private void SetPriority()
    {
        if (drinking)
            priority = Priority.Water;
        else if (eating)
            priority = Priority.Food;
        else if (mating)
            priority = Priority.Partner;
        else if ((waterSearch || waterFound) && (foodSearch || foodFound))
            priority = starveTimer.GetTimeLeft() < parchTimer.GetTimeLeft() ? Priority.Food : Priority.Water;
        else if (waterSearch || waterFound)
            priority = Priority.Water;
        else if (foodSearch || foodFound)
            priority = Priority.Food;
        else if (partnerSearch || partnerFound)
            priority = Priority.Partner;
        else
            priority = Priority.Idle;

        if (priority != Priority.Water && waterFound)
        {
            waterSearch = true;
            waterFound = false;
        }
        if (priority != Priority.Food && foodFound)
        {
            foodSearch = true;
            foodFound = false;
        }
        if (priority != Priority.Partner && partnerFound)
        {
            partnerSearch = true;
            partnerFound = false;
            partner = null;
            moveSpeed /= genes.MoveSpeedMultiplyer;
        }
    }
    
    private double VectorToAngle(Vector3 vector)
    {
        
        Vector3 sinusVector = new Vector3(0, 0, vector.z);
        double sinus = sinusVector.magnitude;
        double angleRad = Math.Asin(sinus);
        double angleDeg = 180 / Math.PI * angleRad;
        if (vector.z < 0)
            angleDeg *= -1;
        if (vector.x < 0)
            angleDeg += (90 - angleDeg) * 2;
        
        // debug
        Vector3 correctedPosition = transform.position + new Vector3(0.0f, transform.localScale.y / 2 + 0.01f, 0.0f);
        Debug.DrawLine(correctedPosition - Vector3.right * 10, correctedPosition + Vector3.right * 10, Color.yellow);
        Debug.DrawLine(correctedPosition, correctedPosition + vector * 10, Color.blue);
        Debug.DrawLine(correctedPosition + new Vector3(vector.x, 0, 0) * 10, correctedPosition + new Vector3(vector.x, 0, 0) * 10 + sinusVector * 10, Color.red);
        
        return angleDeg;
    }

    private Vector3 AngleToVector(double angleDeg)
    {
        double angleRad = angleDeg * Math.PI / 180;
        double sinus = Math.Sin(angleRad);
        double cosinus = Math.Cos(angleRad);
        Vector3 sinusVector = new Vector3(0, 0, (float) sinus);
        Vector3 cosinusVector = new Vector3((float) cosinus, 0, 0);
        Vector3 vector = sinusVector + cosinusVector;
        
        // debug
        Vector3 correctedPosition = transform.position + new Vector3(0.0f, transform.localScale.y / 2 + 0.01f, 0.0f);
        Debug.DrawLine(correctedPosition, correctedPosition + cosinusVector * 10, Color.yellow);
        Debug.DrawLine(correctedPosition + cosinusVector * 10, correctedPosition + cosinusVector * 10 + sinusVector * 10, Color.blue);
        Debug.DrawLine(correctedPosition, correctedPosition + (sinusVector + cosinusVector) * 10, Color.red);
        
        return Vector3.Normalize(vector);
    }
    
    #endregion
    
    #region Coroutines
    
    private IEnumerator Drink()
    {
        yield return new WaitForSeconds((2.5f / GameController.Instance.GetSpeed()));
        parchTimer.Reset();
        waterTimer.Reset();
        drinking = false;
        SetPriority();
    }
    
    private IEnumerator Eat(GameObject food)
    {
        yield return new WaitForSeconds((5f / GameController.Instance.GetSpeed()));
        if (food != null)
        {
            Destroy(food);
            GameController.Instance.DecCurrentPlants();
        }
        starveTimer.Reset();
        foodTimer.Reset();
        eating = false;
        SetPriority();
    }

    private IEnumerator Mate()
    {
        Genome partnerGenes = partner.GetComponent<AnimalBehaviour>().GetGenes();

        yield return new WaitForSeconds((10f / GameController.Instance.GetSpeed()));
        //bingSound.Play();
        
        if (random.Next(2) == 1)
        {
            // mutations
            float viewDensityMutation = UnityEngine.Random.value * 2 * ViewDensityMutationRange - ViewDensityMutationRange;
            float viewWidthMutation = UnityEngine.Random.value * 2 * ViewWidthMutationRange - ViewWidthMutationRange;
            int viewRangeMutation = Mathf.RoundToInt(UnityEngine.Random.value * 2 * ViewRangeMutationRange - ViewRangeMutationRange);
            int tenacityMutation = Mathf.RoundToInt(UnityEngine.Random.value * 2 * TenacityMutationRange - TenacityMutationRange);
            float appealMutation = (UnityEngine.Random.value * 2 * AppealMutationRange - AppealMutationRange);
            float moveSpeedMutation = (UnityEngine.Random.value * 2 * MoveSpeedMutationRange - MoveSpeedMutationRange);
            float moveSpeedMultiplyerMutation = (UnityEngine.Random.value * 2 * MoveSpeedMultiplyerMutationRange - MoveSpeedMultiplyerMutationRange);
            float zigZagMutation = (UnityEngine.Random.value * 2 * ZigZagMutationRange - ZigZagMutationRange);
            
            // mutated genes
            float mutatedViewDensity = (genes.ViewDensity + partnerGenes.ViewDensity) / 2 + viewDensityMutation;
            float mutatedViewWidth = (genes.ViewWidth + partnerGenes.ViewWidth) / 2 + viewWidthMutation;
            int mutatedViewRange = (genes.ViewRange + partnerGenes.ViewRange) / 2 + viewRangeMutation;
            int mutatedTenacity = (genes.Tenacity + partnerGenes.Tenacity) / 2 + tenacityMutation;
            float mutatedAppeal = (genes.Appeal + partnerGenes.Appeal) / 2 + appealMutation;
            float mutatedMoveSpeed = (genes.MoveSpeed + partnerGenes.MoveSpeed) / 2 + moveSpeedMutation;
            Debug.Log("Parent1 speed: " + genes.MoveSpeed + ", Parent2 speed: " + partnerGenes.MoveSpeed + ", Resulting Base Speed: " + (genes.MoveSpeed + partnerGenes.MoveSpeed) / 2 + ", Speed Mutation: " + moveSpeedMutation + ", Resulting new speed: " + mutatedMoveSpeed);
            float mutatedMoveSpeedMultiplyer = (genes.MoveSpeedMultiplyer + partnerGenes.MoveSpeedMultiplyer) / 2 + moveSpeedMultiplyerMutation;
            float mutatedZigZag = (genes.ZigZag + partnerGenes.ZigZag) / 2 + zigZagMutation;
            
            // clamps
            if (mutatedViewDensity < 1)
                mutatedViewDensity = 1;
            else if (mutatedViewDensity > 5)
                mutatedViewDensity = 5;
            if (mutatedViewWidth < 1)
                mutatedViewWidth = 1;
            else if (mutatedViewWidth > 8)
                mutatedViewWidth = 8;
            if (mutatedViewRange < 0)
                mutatedViewRange = 0;
            else if (mutatedTenacity < 0)
                mutatedTenacity = 0;
            mutatedAppeal = Mathf.Clamp(mutatedAppeal, 0f, 1f);
            if (mutatedMoveSpeed < 0)
                mutatedMoveSpeed = 0;
            mutatedMoveSpeedMultiplyer = Mathf.Clamp(mutatedMoveSpeedMultiplyer, 1f, 2f);
            mutatedZigZag = Mathf.Clamp(mutatedZigZag, 0.1f, 1f);

            GameController.Instance.SetGenes(new Genome(mutatedViewDensity, mutatedViewWidth, mutatedViewRange, mutatedTenacity, mutatedAppeal, mutatedMoveSpeed, mutatedMoveSpeedMultiplyer, mutatedZigZag));
            GameObject newMouse = Instantiate(mouse, transform.position, Quaternion.identity);
            newMouse.name = "Mouse";
            GameController.Instance.IncCurrentMice();
        }
        
        partnerTimer.Reset();
        partner = null;
        mating = false;
        SetPriority();
    }

    private IEnumerator DeathDelay()
    {
        yield return new WaitForSeconds((2f / GameController.Instance.GetSpeed()));
        Destroy(gameObject);
    }
    
    #endregion

    #region Public Functions

    public void ClearPartner()
    {
        partner = null;
    }

    #endregion
    
    #region Getter

    public int GetId()
    {
        return id;
    }

    public bool GetAlive()
    {
        return alive;
    }

    public bool GetGrown()
    {
        return grown;
    }

    public bool GetMating()
    {
        return mating;
    }

    public GameObject GetPartner()
    {
        return partner;
    }
    
    public Vector3 GetDirection()
    {
        return direction;
    }
    
    public Genome GetGenes()
    {
        return genes;
    }
    
    #endregion
    
}
