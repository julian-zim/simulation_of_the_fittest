using System;
using UnityEngine;
using UnityEngine.iOS;
using Random = UnityEngine.Random;

public class PlantBehaviour : MonoBehaviour
{

    #region Variables
    
    // constants
    private const int MaxPlants = 500;
    private const int PlantRespawnRange = 50;
    private const float PlantRespawnProbability = 0.25f;
    private const int PlantRepawnTime = 500;
    private const int LifeTime = 5000;
    
    // variables
    private Timer plantRespawnTimer;
    private Timer lifeTimer;
    
    // unity variables
    public GameObject plant;
    
    #endregion

    private void Start()
    {
        
        #region Initialization
        
        plantRespawnTimer = new Timer((PlantRepawnTime / GameController.Instance.GetSpeed()));
        lifeTimer = new Timer(LifeTime / GameController.Instance.GetSpeed());
        
        #endregion
        
    }
    
    private void Update()
    {
        
        #region Death
        
        lifeTimer.Inc();
        if (lifeTimer.Evaluate())
        {
            Destroy(gameObject);
            GameController.Instance.DecCurrentPlants();
        }
        
        #endregion
        
        #region Spawn

        int currentPlants = GameObject.FindGameObjectsWithTag("Food").Length;
        plantRespawnTimer.Inc();
        if (plantRespawnTimer.Evaluate())
        {
            plantRespawnTimer.Reset();
            if (currentPlants < MaxPlants && Random.value <= PlantRespawnProbability)
            {
                SpawnPlant(transform.position);
            }
        }

        #endregion
        
    }
    
    #region Helper Functions
    
    private void SpawnPlant(Vector3 pos)
    {
        
        Vector3 newPos = new Vector3(0f, 0.7f, 0f);

        /*Vector2 directionToClosestLake = new Vector2();
        float minDistance = float.MaxValue;
        for (int i = 0; i < GameController.Instance.lakePositions.Length; i++)
        {
            Vector2 directionToLake = GameController.Instance.lakePositions[i] - new Vector2(pos.x, pos.z);
            float distance = directionToLake.magnitude;
            if (distance < minDistance)
            {
                minDistance = distance;
                directionToClosestLake = directionToLake;
            }
        }

        int waterQuarter;
        double angle = Utils.VectorToAngle(new Vector3(directionToClosestLake.x, 0f, directionToClosestLake.y).normalized);
        if (angle <= 90)
            waterQuarter = 0;
        else if (angle <= 180)
            waterQuarter = 1;
        else if (angle <= 270)
            waterQuarter = 2;
        else
            waterQuarter = 3;

        int spawnQuarter;
        float value = Random.value;
        if (value <= 0.39)
            spawnQuarter = waterQuarter;
        else if (value <= 0.61)
            spawnQuarter = waterQuarter + 1;
        else if (value <= 0.83)
            spawnQuarter = waterQuarter + 3;
        else
            spawnQuarter = waterQuarter + 2;
        spawnQuarter %= 4;

        float spawnAngle = 0f;
        switch (spawnQuarter)
        {
            case 0:
            {
                spawnAngle = Random.Range(0f, 89f);
                break;
            }
            case 1:
            {
                spawnAngle = Random.Range(90f, 179f);
                break;
            }
            case 2:
            {
                spawnAngle = Random.Range(180f, 269f);
                break;
            }
            case 3:
            {
                spawnAngle = Random.Range(270f, 359f);
                break;
            }
        }

        newPos = Utils.AngleToVector(spawnAngle);
        newPos *= Random.Range(0, PlantRespawnRange);
        newPos += pos;*/
        
        // TODO: Plants dont die, also why are they pushed out of the middle? Just because of the mice?

        do
        {
            newPos.x = pos.x + Random.value * 2 * PlantRespawnRange - PlantRespawnRange;
            newPos.z = pos.z + Random.value * 2 * PlantRespawnRange - PlantRespawnRange;
        } while (newPos.x < -100 || newPos.x > 100 || newPos.z < -100 || newPos.z > 100);
        
        // this would be more realistic, but the upper method creates stabilization within a population
        /*
        if (newPos.x < -100)
            newPos.x += 200;
        if (newPos.x > 100)
            newPos.x -= 200;
        
        if (newPos.z < -100)
            newPos.z += 200;
        if (newPos.z > 100)
            newPos.z -= 200;
        */
        
        GameObject newPlant = Instantiate(plant, newPos, Quaternion.identity);
        newPlant.name = "Plant";
        GameController.Instance.IncCurrentPlants();
    }
    
    #endregion
    
}
