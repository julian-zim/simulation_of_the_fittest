using System;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    
    #region Variables

    // Singleton pattern
    public static GameController Instance;              // instance of itself

    // constants
    private const String Graph1Caption = "Animal Count";
    private const String Graph2Caption = "Average Movement Speed (Mutatable Gene)";
    
    private const int Speed = 20;                       // the speed of everything
    private const int Mice = 75;                        // the initial mice
    private const int Plants = 100;                     // the initial plants
    private const int Lakes = 20;                       // the initial water lakes
    
    // initial genes
    private const float ViewDensity = 2f;
    private const float ViewWidth = 3f;
    private const int ViewRange = 30;
    private const int Tenacity = 2000;
    private const float Appeal = 0.3f;
    private const float MoveSpeed = 1f;
    private const float MoveSpeedMultiplyer = 1.2f;
    private const float ZigZag = 0.25f;

    // mouse variables (data that gets fetched from a newly instantiated mouse)
    private Genome mouseGenes;                          // genes
    private int mouseId;                                // the mouse id
    
    // variables
    private int currentTime;                            // time counter
    private int currentMice;                            // mouse counter
    private int currentPlants;                          // plant counter
    private Graph graph1;                               // graph 1
    private Graph graph2;                               // graph 2
    
    // public variables
    public Vector2[] lakePositions;                    // positions of the spawned lakes
    
    // unity variables
    public GameObject @this;
    public GameObject mouse;
    public GameObject waterSource;
    public GameObject plant;
    public GameObject graphWindow1;
    public GameObject graphWindow2;
    public Text promptViewDensity;
    public Text promptViewWidth;
    public Text promptViewRange;
    public Text promptTenacity;
    public Text promptAppeal;
    public Text promptMoveSpeed;
    public Text promptMoveSpeedM;
    public Text promptZigZag;

    #endregion
    
    private void Start()
    {
        
        #region Initialization
        
        UnityEngine.Random.InitState((int) DateTime.Now.Ticks);
        Instance = @this.GetComponent<GameController>();
        mouseGenes = new Genome(ViewDensity, ViewWidth, ViewRange, Tenacity, Appeal, MoveSpeed, MoveSpeedMultiplyer, ZigZag);
        mouseId = 0;
        currentTime = -1;
        currentMice = Mice;
        currentPlants = Plants;
        graph1 = new Graph(graphWindow1, Graph1Caption);
        graph2 = new Graph(graphWindow2, Graph2Caption);
        
        #endregion
        
        #region Spawn Mice

        for (int i = 0; i < Mice; i++)
        {
            Vector2 pos = new Vector2(UnityEngine.Random.value * 200 - 100, UnityEngine.Random.value * 200 - 100);
            Instantiate(Instance.mouse, new Vector3(pos.x, 0.375f, pos.y), Quaternion.identity);
        }

        #endregion

        #region Spawn Plants
        
        for (int i = 0; i < Plants; i++)
        {
            Vector2 pos = new Vector2(UnityEngine.Random.value * 200 - 100, UnityEngine.Random.value * 200 - 100);
            Instantiate(plant, new Vector3(pos.x, 0.7f, pos.y), Quaternion.identity);
        }

        #endregion
        
        #region Spawn Water
        
        lakePositions = new Vector2[Lakes];
        for (int i = 0; i < Lakes; i++)
        {
            Vector2 pos = new Vector2(UnityEngine.Random.value * 200 - 100, UnityEngine.Random.value * 200 - 100);
            GenerateWaterLake(pos);
            lakePositions[i] = pos;
        }
        
        #endregion
        
    }

    private void FixedUpdate()
    {
        currentTime++;
        if (currentTime == 0)
            return;
        
        #region Plot

        // average gene of all living mice
        float average = 0;
        foreach (GameObject currentMouse in GameObject.FindGameObjectsWithTag("Mouse"))
        {
            if (currentMouse.GetComponent<AnimalBehaviour>().GetAlive())
                average += currentMouse.GetComponent<AnimalBehaviour>().GetGenes().MoveSpeed;    // gene choice
        }
        average = currentMice > 0 ? average / currentMice : 0;
        
        graph1.AddEntry(currentTime, currentMice);
        graph2.AddEntry(currentTime, (int) Mathf.Round(average * 100));

        if (currentTime % 50 == 0)
        {
            graph1.Plot();
            graph2.Plot();
        
            #region Prompt
            
            float averageViewDensity = 0;
            float averageViewWidth = 0;
            float averageViewRange = 0;
            float averageTenacity = 0;
            float averageAppeal = 0;
            float averageMoveSpeed = 0;
            float averageMoveSpeedM = 0;
            float averageZigZag = 0;
            foreach (GameObject currentMouse in GameObject.FindGameObjectsWithTag("Mouse"))
            {
                if (currentMouse.GetComponent<AnimalBehaviour>().GetAlive())
                {
                    averageViewDensity += currentMouse.GetComponent<AnimalBehaviour>().GetGenes().ViewDensity;
                    averageViewWidth += currentMouse.GetComponent<AnimalBehaviour>().GetGenes().ViewWidth;
                    averageViewRange += currentMouse.GetComponent<AnimalBehaviour>().GetGenes().ViewRange;
                    averageTenacity += currentMouse.GetComponent<AnimalBehaviour>().GetGenes().Tenacity;
                    averageAppeal += currentMouse.GetComponent<AnimalBehaviour>().GetGenes().Appeal;
                    averageMoveSpeed += currentMouse.GetComponent<AnimalBehaviour>().GetGenes().MoveSpeed;
                    averageMoveSpeedM += currentMouse.GetComponent<AnimalBehaviour>().GetGenes().MoveSpeedMultiplyer;
                    averageZigZag += currentMouse.GetComponent<AnimalBehaviour>().GetGenes().ZigZag;
                }

            }
            averageViewDensity = currentMice > 0 ? averageViewDensity / currentMice : 0;
            averageViewWidth = currentMice > 0 ? averageViewWidth / currentMice : 0;
            averageViewRange = currentMice > 0 ? averageViewRange / currentMice : 0;
            averageTenacity = currentMice > 0 ? averageTenacity / currentMice : 0;
            averageAppeal = currentMice > 0 ? averageAppeal / currentMice : 0;
            averageMoveSpeed = currentMice > 0 ? averageMoveSpeed / currentMice : 0;
            averageMoveSpeedM = currentMice > 0 ? averageMoveSpeedM / currentMice : 0;
            averageZigZag = currentMice > 0 ? averageZigZag / currentMice : 0;

            promptViewDensity.text = "View Density: " + averageViewDensity;
            promptViewWidth.text = "View Width: " + averageViewWidth;
            promptViewRange.text = "View Range: " + averageViewRange;
            promptTenacity.text = "Tenacity: " + averageTenacity;
            promptAppeal.text = "Appeal: " + averageAppeal;
            promptMoveSpeed.text = "Move Speed: " + averageMoveSpeed;
            promptMoveSpeedM.text = "Move Speed M: " + averageMoveSpeedM;
            promptZigZag.text = "Zig Zag: " + averageZigZag;

            #endregion
            
        }
        
        #endregion

    }
    
    #region Helper Functions

    private void GenerateWaterLake(Vector2 pos)
    {
        float waterSourceSize = 10;
        Vector2 currentPos;
        for (int j = 0; j < 3; j++)
        {
            currentPos = new Vector2(pos.x - waterSourceSize, pos.y - waterSourceSize/2);
            if (currentPos.x <= 100-waterSourceSize/2 && currentPos.x >= -100+waterSourceSize/2 && currentPos.y <= 100-waterSourceSize/2 && currentPos.y >= -100+waterSourceSize/2) {
                GameObject newSource = Instantiate(waterSource, new Vector3(currentPos.x, 0.001f, currentPos.y), Quaternion.identity);
                newSource.name = "Water";
            }
            currentPos = new Vector2(pos.x, pos.y - waterSourceSize/2);
            if (currentPos.x <= 100-waterSourceSize/2 && currentPos.x >= -100+waterSourceSize/2 && currentPos.y <= 100-waterSourceSize/2 && currentPos.y >= -100+waterSourceSize/2) {
                GameObject newSource = Instantiate(waterSource, new Vector3(currentPos.x, 0.001f, currentPos.y), Quaternion.identity);
                newSource.name = "Water";
            }
            currentPos = new Vector2(pos.x + waterSourceSize, pos.y - waterSourceSize/2);
            if (currentPos.x <= 100-waterSourceSize/2 && currentPos.x >= -100+waterSourceSize/2 && currentPos.y <= 100-waterSourceSize/2 && currentPos.y >= -100+waterSourceSize/2) {
                GameObject newSource = Instantiate(waterSource, new Vector3(currentPos.x, 0.001f, currentPos.y), Quaternion.identity);
                newSource.name = "Water";
            }
        }
        for (int j = 0; j < 3; j++)
        {
            currentPos = new Vector2(pos.x - waterSourceSize, pos.y + waterSourceSize/2);
            if (currentPos.x <= 100-waterSourceSize/2 && currentPos.x >= -100+waterSourceSize/2 && currentPos.y <= 100-waterSourceSize/2 && currentPos.y >= -100+waterSourceSize/2) {
                GameObject newSource = Instantiate(waterSource, new Vector3(currentPos.x, 0.001f, currentPos.y), Quaternion.identity);
                newSource.name = "Water";
            }
            currentPos = new Vector2(pos.x, pos.y + waterSourceSize/2);
            if (currentPos.x <= 100-waterSourceSize/2 && currentPos.x >= -100+waterSourceSize/2 && currentPos.y <= 100-waterSourceSize/2 && currentPos.y >= -100+waterSourceSize/2) {
                GameObject newSource = Instantiate(waterSource, new Vector3(currentPos.x, 0.001f, currentPos.y), Quaternion.identity);
                newSource.name = "Water";
            }
            currentPos = new Vector2(pos.x + waterSourceSize, pos.y + waterSourceSize/2);
            if (currentPos.x <= 100-waterSourceSize/2 && currentPos.x >= -100+waterSourceSize/2 && currentPos.y <= 100-waterSourceSize/2 && currentPos.y >= -100+waterSourceSize/2) {
                GameObject newSource = Instantiate(waterSource, new Vector3(currentPos.x, 0.001f, currentPos.y), Quaternion.identity);
                newSource.name = "Water";
            }
        }
    }
    

    #endregion
    
    #region Public Functions
    
    public void IncMouseId()
    {
        mouseId++;
    }

    public void IncCurrentMice()
    {
        currentMice++;
    }

    public void DecCurrentMice()
    {
        currentMice--;
    }

    public void IncCurrentPlants()
    {
        currentPlants++;
    }
    
    public void DecCurrentPlants()
    {
        currentPlants--;
    }

    #endregion
    
    #region Setter

    public void SetGenes(Genome genes)
    {
        mouseGenes = genes;
    }
    
    #endregion
    
    #region Getter

    public int GetSpeed()
    {
        return Speed;
    }
    
    public int GetMouseId()
    {
        return mouseId;
    }

    public Genome GetMouseGenes()
    {
        return mouseGenes;
    }
    
    #endregion
    
    private void OnApplicationQuit()
    {
        graph1.Save( "count");
        graph2.Save( "gene");
    }
    
}
