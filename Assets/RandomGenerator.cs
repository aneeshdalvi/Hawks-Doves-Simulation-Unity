using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Linq;
#if UNITY_EDITOR            
    using UnityEditor;
#endif
    
public class RandomGenerator : MonoBehaviour
{

    public Vector3 center;
    public GameObject dovePrefab;
    public GameObject hawkPrefab;
    public GameObject foodPrefab;
    private Vector3 min;
    private Vector3 max;
    private float _xAxis;
    private float _yAxis;
    private float _zAxis;
    private Vector3 _randomPosition;
    public bool _canInstantiate;

    public TextMeshProUGUI hawksInputText;
    public TextMeshProUGUI hawksPlaceholder;

    public TextMeshProUGUI dovesInputText;
    public TextMeshProUGUI dovesPlaceholder;

    public TextMeshProUGUI foodInputText;
    public TextMeshProUGUI foodPlaceholder;

    //populationgeneration 
    public Dropdown populationgenerationText;

    public TextMeshProUGUI energyValueFoodInputText;
    public TextMeshProUGUI energyValueFoodPlaceholder;


    public TextMeshProUGUI energyLossInjuryInputText;
    public TextMeshProUGUI energyLossInjuryPlaceholder;

    public TextMeshProUGUI energyLossBluffInputText;
    public TextMeshProUGUI energyLossBluffPlaceholder;

    public TextMeshProUGUI baseEnergyReqInputText;
    public TextMeshProUGUI baseEnergyReqPlaceholder;

    public TextMeshProUGUI deathThresholdInputText;
    public TextMeshProUGUI deathThresholdPlaceholder;

    public TextMeshProUGUI reproductionThresholdInputText;
    public TextMeshProUGUI reproductionThresholdPlaceholder;

    public TextMeshProUGUI foodExpirationTimeInputText;
    public TextMeshProUGUI foodExpirationTimePlaceholder;

    public Button startButton;
    public Button nextStepButton;
    public Button stopButton;
    public Button animateButton;

    private int stopAfter100generations = 0;

    public DD_DataDiagram dataDiagram;


    int hawks;
    int doves;
    int food;
    //String populationGeneration
    int energyValueFood;
    int energyLossInjury;
    int energyLossBluff;
    int baseEnergyReq;
    static int deathThreshold;
    static int reproductionThreshold;
    int foodExpirationTime;

    private int generationCount = 0;

    private float h = 0.0f;
    private GameObject lineHawk;
    private GameObject lineDove;

    private bool isAnimate = false;

    List<GameObject> lineList = new List<GameObject>();

    private String directoryPath;

    private int distributionType;
    private bool isDistributionUniform;
    private bool isStartBtnClicked;

    void Start()
    {
        // set area for game
        SetRange();

        // initial setup for data diagram    
        colorSetupForGraph();

        // intialize directoryPath for new files
        directoryPath = Application.dataPath + "/CSVData/";

        // add button listners here
        handleButtonListener();

        // delete all folders and start new
        deleteAllCreatedFiles();

    }

    private void colorSetupForGraph()
    {
        GameObject dd = GameObject.Find("DataDiagram");
        dataDiagram = dd.GetComponent<DD_DataDiagram>();

// red 1.0f, 1.0f, 1.0f        
        Color colorHawk = Color.HSVToRGB((h += 0.1f) > 1 ? (h - 1) : 0.0f, 1.0f, 1.0f);
        Color colorDove = Color.HSVToRGB((h += 0.1f) > 1 ? (h - 1) : 0.0f, 0.0f, 1.0f);
        lineHawk = dataDiagram.AddLine(colorHawk.ToString(), colorHawk);
        lineDove = dataDiagram.AddLine(colorDove.ToString(), colorDove);
    }

    private void handleButtonListener()
    {
        startButton.onClick.AddListener(start);
        nextStepButton.onClick.AddListener(nextStep);
        stopButton.onClick.AddListener(stop);
        animateButton.onClick.AddListener(animate);
    }

    // reading all input values  
    private void initializeAllUserInputs()
    {

        hawks = initialzeEachInput(hawksInputText.text, hawksPlaceholder.text);
        doves = initialzeEachInput(dovesInputText.text, dovesPlaceholder.text);
        food = initialzeEachInput(foodInputText.text, foodPlaceholder.text);
        energyValueFood = initialzeEachInput(energyValueFoodInputText.text, energyValueFoodPlaceholder.text);
        energyLossInjury = initialzeEachInput(energyLossInjuryInputText.text, energyLossInjuryPlaceholder.text);
        energyLossBluff = initialzeEachInput(energyLossBluffInputText.text, energyLossBluffPlaceholder.text);
        baseEnergyReq = initialzeEachInput(baseEnergyReqInputText.text, baseEnergyReqPlaceholder.text);
        deathThreshold = initialzeEachInput(deathThresholdInputText.text, deathThresholdPlaceholder.text);
        reproductionThreshold = initialzeEachInput(reproductionThresholdInputText.text, reproductionThresholdPlaceholder.text);
        foodExpirationTime = initialzeEachInput(foodExpirationTimeInputText.text, foodExpirationTimePlaceholder.text);

        //Reading populationgenerationText Dropdown input
        checkandSetIsDistributionUniform();

    }

    //Reading each user input
    private int initialzeEachInput(String inputText, String placeholder)
    {
        String temp = inputText.Replace("\u200B", "");
        if (temp.Length == 0)
        {
            temp = placeholder;
        }
        return int.Parse(temp.Trim());
    }

    // runs on animate button click
    private void animate()
    {
        // this is button to see full game animation
        stop();
        isAnimate = true;
        start();
    }

    // runs on start button click
    private void start()
    {
        isStartBtnClicked = true;
        nextStepButton.interactable = true;
        // if user clicks on the start after animate
        if (!isAnimate)
            stop();

        stopAfter100generations = 0;
        initializeAllUserInputs();

        for (int i = 0; i < hawks; i++)
            SpawnPrefab(hawkPrefab, "Hawk", baseEnergyReq);

        for (int i = 0; i < doves; i++)
            SpawnPrefab(dovePrefab, "Dove", baseEnergyReq);

        for (int i = 0; i < food; i++)
            SpawnPrefab(foodPrefab, "Food", energyValueFood);
    }

    // runs on nextStep button click
    private void nextStep()
    {
        // this btn runs only if start is clicked
        if (!isStartBtnClicked)
        {
            nextStepButton.interactable = false;
            return;
        }


        // checks the dropdown distribution to be processed
        checkandSetIsDistributionUniform();

        // gets all the latest user inputs
        initializeAllUserInputs();

        // one generation is processed 
        runOneGenerationSimulation();

        // creates new folder for input files
        createNewDirectory();

        // writing data to files at each generation..
        writeDataToFile();

        // food generated food times after each generation
        populateNewFoodAfterOneGeneration();
    }

    private void populateNewFoodAfterOneGeneration()
    {
        for (int i = 0; i < food; i++)
            SpawnPrefab(foodPrefab, "Food", energyValueFood);
    }

    private void writeDataToFile()
    {
        string filename = "run" + DateTime.Now.ToString("ddmmyyhhmmss");

        // call for writing to file
        WriteToCSV(directoryPath + filename + ".txt",
        GameObject.FindGameObjectsWithTag("Hawk").Length,
        GameObject.FindGameObjectsWithTag("Dove").Length,
        GameObject.FindGameObjectsWithTag("Food").Length);
    }

    private void runOneGenerationSimulation()
    {
        GameObject[] foods = GameObject.FindGameObjectsWithTag("Food");
        GameObject[] hawksList = GameObject.FindGameObjectsWithTag("Hawk");
        GameObject[] dovesList = GameObject.FindGameObjectsWithTag("Dove");


        List<Collider> toBeDestroyed = new List<Collider>();
        List<Collider> toBeReproduced = new List<Collider>();
        List<GameObject> foodToBeDestroyed = new List<GameObject>();

        // for debug
        int dovesFight = 0;
        int hawksFight = 0;
        int hawkDoveFight = 0;


        foreach (GameObject fd in foods)
        {
            Collider[] colliders = Physics.OverlapSphere(fd.transform.position, 5);

            // get location of each food
            Vector3 fdLocation = fd.transform.position;

            String[] parameters = fd.name.Split(',');
            String name = parameters[0];
            int currEnergy = int.Parse(parameters[1]);
            float currTime = float.Parse(parameters[2]);
            int age = int.Parse(parameters[3]);
            age += 1;

            // update time and age (food expires after 5 generation)
            fd.name = name + "," + currEnergy + "," + Time.timeSinceLevelLoad + "," + age;

            if (age <= foodExpirationTime)
            {
                Collider collider1 = null;
                Collider collider2 = null;

                foreach (Collider otherObj in colliders)
                {
                    if (!otherObj.tag.Equals("Food"))
                    {
                        if (collider1 == null)
                        {
                            collider1 = otherObj;
                        }
                        else
                        {
                            collider2 = otherObj;
                            break;
                        }
                    }
                }

                if (collider1 != null && collider2 != null)
                {
                    foodToBeDestroyed.Add(fd);
                    // both hawks
                    if (collider1.tag.Equals("Hawk") && collider2.tag.Equals("Hawk"))
                    {
                        hawksFight += 1;
                        // loser
                        int collider1Energy = int.Parse(collider1.name.Split(',')[1]);

                        if (collider1Energy - energyLossInjury < deathThreshold)
                        {
                            toBeDestroyed.Add(collider1);
                        }
                        else
                        {
                            currEnergy = collider1Energy - energyLossInjury;
                            changeEnergy(collider1, currEnergy);
                            MoveToNewPlace(collider1, collider1.tag, currEnergy, toBeDestroyed);
                        }

                        // winner
                        int collider2Energy = int.Parse(collider2.name.Split(',')[1]);

                        if (collider2Energy + energyValueFood > reproductionThreshold)
                        {
                            // sending intial energy for reproduction   
                            currEnergy = collider2Energy;
                            changeEnergy(collider2, currEnergy);
                            toBeReproduced.Add(collider2);
                        }
                        else
                        {
                            currEnergy = collider2Energy + energyValueFood;
                            changeEnergy(collider2, currEnergy);
                            MoveToNewPlace(collider2, collider2.tag, currEnergy, toBeDestroyed);
                        }
                    }
                    else if (collider1.tag.Equals("Dove") && collider2.tag.Equals("Dove"))
                    {
                        dovesFight += 1;
                        // loser
                        int collider1Energy = int.Parse(collider1.name.Split(',')[1]);

                        if (collider1Energy - energyLossBluff < deathThreshold)
                        {
                            toBeDestroyed.Add(collider1);
                        }
                        else
                        {
                            currEnergy = collider1Energy - energyLossBluff;
                            changeEnergy(collider1, currEnergy);
                            MoveToNewPlace(collider1, collider1.tag, currEnergy, toBeDestroyed);
                        }

                        // winner
                        int collider2Energy = int.Parse(collider2.name.Split(',')[1]);

                        if ((collider2Energy + energyValueFood - energyLossBluff) > reproductionThreshold)
                        {
                            // sending intial energy for reproduction   
                            currEnergy = collider2Energy - energyLossBluff;
                            changeEnergy(collider2, currEnergy);
                            toBeReproduced.Add(collider2);

                        }
                        else
                        {
                            currEnergy = collider2Energy + energyValueFood - energyLossBluff;
                            changeEnergy(collider2, currEnergy);
                            MoveToNewPlace(collider2, collider2.tag, currEnergy, toBeDestroyed);
                        }

                    }
                    // mix hawk and dove
                    else
                    {
                        hawkDoveFight += 1;
                        if (!collider2.tag.Equals("Hawk"))
                        {
                            collider2 = collider1;
                        }

                        // winner
                        int collider2Energy = int.Parse(collider2.name.Split(',')[1]);

                        if ((collider2Energy + energyValueFood) > reproductionThreshold)
                        {
                            // sending intial energy for reproduction
                            currEnergy = collider2Energy;
                            changeEnergy(collider2, currEnergy);
                            toBeReproduced.Add(collider2);
                        }
                        else
                        {
                            currEnergy = collider2Energy + energyValueFood;
                            changeEnergy(collider2, currEnergy);
                            MoveToNewPlace(collider2, collider2.tag, currEnergy, toBeDestroyed);
                        }
                    }

                    // after the energies are changed 
                    // need to change the location of both game objects
                    MoveToNewPlace(collider1, collider1.tag, currEnergy, toBeDestroyed);

                }
            }
            else
            {
                //food expires here so remove food
                foodToBeDestroyed.Add(fd);
            }
        }// for end

        // Debug fights
        /*Debug.Log("Hawk fights hawk - " + hawksFight);
        Debug.Log("dove fights dove - " + dovesFight);
        Debug.Log("dove fights hawk - " + hawkDoveFight);*/

        // updating energies for all gObjs after one generation        
        updateEnergyAfterOneGeneration(dovesList, foodToBeDestroyed);
        updateEnergyAfterOneGeneration(hawksList, foodToBeDestroyed);

        // reproduce all objects in list generated
        reproduceObjects(toBeReproduced);

        // destroy all objects in lists generated
        destroyObjectsInLists(toBeDestroyed, foodToBeDestroyed);

    }

    // changes the energy of gameobject and place at another random position
    private void changeEnergy(Collider collider, int newEnergy)
    {
        collider.name = collider.name.Split(',')[0] + "," + newEnergy + "," + collider.name.Split(',')[2] + "," + collider.name.Split(',')[3];

    }

    // create new gameobjects at random location
    private void MoveToNewPlace(Collider collider, string tag, int newEnergy, List<Collider> toBeDestroyed)
    {
        toBeDestroyed.Add(collider);
        _xAxis = UnityEngine.Random.Range(min.x, max.x);
        _yAxis = UnityEngine.Random.Range(min.y, max.y);
        Vector3 newpos = center + new Vector3(_xAxis, _yAxis, 0);

        GameObject gameObj = Instantiate(collider.gameObject, newpos, Quaternion.identity);
        gameObj.tag = tag;
        gameObj.name = tag + "," + newEnergy + "," + Time.timeSinceLevelLoad + "," + generationCount;
    }


    private void destroyObjectsInLists(List<Collider> toBeDestroyed, List<GameObject> foodToBeDestroyed)
    {
        foreach (Collider c in toBeDestroyed)
        {
            if (c != null)
                Destroy(c.gameObject);
        }

        foreach (GameObject gObj in foodToBeDestroyed)
        {
            if (gObj != null)
                Destroy(gObj);
        }

        // create new lists for next generation
        toBeDestroyed = new List<Collider>();
        foodToBeDestroyed = new List<GameObject>();

    }

    private void checkandSetIsDistributionUniform()
    {
        distributionType = populationgenerationText.value;
        if (distributionType == 0)
        {
            isDistributionUniform = true;
        }
        else
        {
            isDistributionUniform = false;
        }
    }

    private void createNewDirectory()
    {
        if (!System.IO.Directory.Exists(directoryPath))
        {
            System.IO.Directory.CreateDirectory(directoryPath);
        }
    }

    private void reproduceObjects(List<Collider> toBeReproduced)
    {
        foreach (Collider gObj in toBeReproduced)
        {
            String[] parameters = gObj.name.Split(',');
            String name = parameters[0];
            int currEnergy = int.Parse(parameters[1]);
            float currTime = float.Parse(parameters[2]);
            int age = int.Parse(parameters[3]);

            //updateGameObjectColor(gObj, currEnergy);            
            gObj.name = gObj.name.Split(',')[0] + "," + (currEnergy / 2) + "," + gObj.name.Split(',')[2] + "," + age;


            // instantiate child 
            if (gObj.tag.Equals("Hawk"))
            {
                SpawnPrefab(hawkPrefab, "Hawk", currEnergy / 2);
            }
            else if (gObj.tag.Equals("Dove"))
            {
                SpawnPrefab(dovePrefab, "Dove", currEnergy / 2);
            }
        }
    }

    // runs on stop button click
    private void stop()
    {
        isAnimate = false;
        stopAfter100generations = 0;

        // create GameObject[] for each prehab        
        GameObject[] foods = GameObject.FindGameObjectsWithTag("Food");
        GameObject[] hawksList = GameObject.FindGameObjectsWithTag("Hawk");
        GameObject[] dovesList = GameObject.FindGameObjectsWithTag("Dove");

        // destroy all Lists
        destroyGameObjectList(foods);
        destroyGameObjectList(hawksList);
        destroyGameObjectList(dovesList);

        // delete all files created
        deleteAllCreatedFiles();


        // refreshing folders
        #if UNITY_EDITOR            
            AssetDatabase.Refresh();
        #endif
        
    }


    private void deleteAllCreatedFiles()
    {
        if (System.IO.Directory.Exists(directoryPath))
        {
            System.IO.Directory.Delete(directoryPath, true);
        }
    }

    private void destroyGameObjectList(GameObject[] objectList)
    {
        foreach (GameObject gObj in objectList)
        {
            Destroy(gObj);
        }
    }

    private void SetRange()
    {
        min = new Vector3(400, -25, 0);
        max = new Vector3(800, 540, 0);
    }


    // change color as the object losses energy
    private void updateGameObjectColor(GameObject gObj, int currEnergy)
    {
        Material gObj_material = gObj.GetComponent<Renderer>().material;
        Color gObj_color = gObj_material.color;

        if (currEnergy >= reproductionThreshold)
        {
            gObj_color.a += (float)0.1f;
        }
        else if (currEnergy < 0.5 * reproductionThreshold && currEnergy > 0.2 * reproductionThreshold)
        {
            gObj_color.a += (float)0.7f;
        }
        else if (currEnergy < 0.2 * reproductionThreshold && currEnergy < 0)
        {
            gObj_color.a += (float)0.99f;
        }
    }


    private void updateEnergyAfterOneGeneration(GameObject[] gameObjList, List<GameObject> objToBeDestroyed)
    {
        // reducing energy for all objs after each generation
        foreach (GameObject gObj in gameObjList)
        {
            String[] parameters = gObj.name.Split(',');
            String name = parameters[0];
            int currEnergy = int.Parse(parameters[1]);
            float currTime = float.Parse(parameters[2]);
            int age = int.Parse(parameters[3]);

            currEnergy = currEnergy - baseEnergyReq;

            if (currEnergy <= deathThreshold)
            {
                objToBeDestroyed.Add(gObj);
                return;
            }

            updateGameObjectColor(gObj, currEnergy);
            gObj.name = gObj.name.Split(',')[0] + "," + currEnergy + "," + gObj.name.Split(',')[2] + "," + (age + 1);

        }
    }


    void Update()
    {
        runFor100Generations();
    }

    private void runFor100Generations()
    {
        if (isAnimate && stopAfter100generations < 100)
        {
            stopAfter100generations++;
            nextStep();


        }
        else if (stopAfter100generations == 100)
        {
            stopAfter100generations++;
            String[] files = System.IO.Directory.GetFiles(directoryPath, "*.txt", System.IO.SearchOption.AllDirectories);

            List<int> allHawksList = new List<int>();
            List<int> allDovesList = new List<int>();
            List<int> allFoodList = new List<int>();


            foreach (String filepath in files)
            {
                List<int> hawksList = new List<int>();
                List<int> dovesList = new List<int>();
                List<int> foodList = new List<int>();


                ReadToCSV(filepath, hawksList, dovesList, foodList);
                allHawksList.AddRange(hawksList);
                allDovesList.AddRange(dovesList);
                allFoodList.AddRange(foodList);

            }

            // plot values to the graph
            plotGraph(allHawksList, allDovesList);

            // make it as it was at the start of the game
            isAnimate = false;

            
            #if UNITY_EDITOR            
                AssetDatabase.Refresh();
            #endif
            Debug.Log("Simulation Ended.");
        }
    }

    private void plotGraph(List<int> allHawksList, List<int> allDovesList)
    {

        // all hawks
        foreach (int hawkCount in allHawksList)
        {
            dataDiagram.InputPoint(lineHawk, new Vector2(1, hawkCount / 10f));
        }

        // all doves
        foreach (int doveCount in allDovesList)
        {
            dataDiagram.InputPoint(lineDove, new Vector2(1, doveCount / 10f));
        }
    }

    // returns normalised energy for gameobject in range(d,r)
    private int getNormalizedDistribution()
    {
        int minValue = deathThreshold;
        int maxValue = reproductionThreshold;
        int mean = (minValue + maxValue) / 2;
        int sigma = (maxValue - mean) / 3;
        return (int)UnityEngine.Random.Range(mean, sigma);

    }


    // create new gameobjects at random location
    public void SpawnPrefab(GameObject prefab, String tag, int energy)
    {

        _xAxis = UnityEngine.Random.Range(min.x, max.x);
        _yAxis = UnityEngine.Random.Range(min.y, max.y);
        Vector3 pos = center + new Vector3(_xAxis, _yAxis, 0);

        GameObject gameObj = Instantiate(prefab, pos, Quaternion.identity);
        gameObj.tag = tag;

        // check distribution is Uniform or normal distribution
        if (isDistributionUniform)
        {
            gameObj.name = tag + "," + energy + "," + Time.timeSinceLevelLoad + "," + generationCount;
        }
        else
        {
            gameObj.name = tag + "," + getNormalizedDistribution() + "," + Time.timeSinceLevelLoad + "," + generationCount;
        }


    }

    // Writing simulation data to file
    void WriteToCSV(String filepath, int hawks, int doves, int food)
    {
        try
        {
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(@filepath, true))
            {
                file.WriteLine(hawks + "," + doves + "," + food);
            }
        }
        catch (Exception)
        {
            Debug.Log("Error: Unable to write to file.");
        }
    }

    // Reading simulation data from file
    void ReadToCSV(String filepath, List<int> hawksList, List<int> dovesList, List<int> foodList)
    {
        try
        {
            using (System.IO.StreamReader file = new System.IO.StreamReader(filepath))
            {
                String data = file.ReadLine();
                while (data != null)
                {
                    String[] values = data.Split(',');
                    hawksList.Add(Int32.Parse(values[0]));
                    dovesList.Add(Int32.Parse(values[1]));
                    foodList.Add(Int32.Parse(values[2]));

                    data = file.ReadLine();
                }
            }
        }
        catch (Exception)
        {
            Debug.Log("Error: Unable to read to file.");
        }
    }

}
