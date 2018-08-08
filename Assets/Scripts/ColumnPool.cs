using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CSML;

public class ColumnPool : MonoBehaviour {

    public int columnPoolSize = 5;
    public GameObject columnPrefab;
    public float spawnRate = 4f;
    public float columnMin = -0.5f;
    public float columnMax = 2f;

    public GameObject[] columns;
    private Vector2 objectPoolPosition = new Vector2(-15f, -25f);
    private float timeSinceLastSpawned, spawnXPosition = 10f, spawnYPosition;
    private int currColumn = 0;

    private float colTopPos, colBottomPos;

	// Use this for initialization
	void Start () {
        columns = new GameObject[columnPoolSize];
        for (int i=0; i<columnPoolSize; i++)
        {
            columns[i] = (GameObject)Instantiate(columnPrefab, objectPoolPosition, Quaternion.identity);
        }
        spawnColumn();
        colTopPos = GameObject.FindGameObjectWithTag("ColTop").gameObject.transform.position.y;
        colBottomPos = GameObject.FindGameObjectWithTag("ColBottom").gameObject.transform.position.y;
    }
	
	// Update is called once per frame
	void Update () {
        timeSinceLastSpawned += Time.deltaTime;
        if (GameController.instance.gameOver == false && timeSinceLastSpawned >= spawnRate)
        {
            spawnColumn();
        }
	}

    public Matrix getInputs(float xPos, float yPos)
    {
        Matrix inputs =  new Matrix(2,1);
        Vector2 columnPos = columns[currColumn - 1].gameObject.transform.position;
        inputs[1, 1] = new Complex(columnPos.x+1 - xPos);
        inputs[2,1] = new Complex(yPos - (columnPos.y + (colTopPos+colBottomPos)/2));
        return inputs;
    }

    void spawnColumn()
    {
        timeSinceLastSpawned = 0f;
        timeSinceLastSpawned = 0;
        spawnYPosition = Random.Range(columnMin, columnMax);
        columns[currColumn].transform.position = new Vector2(spawnXPosition, spawnYPosition);
        currColumn = (currColumn + 1) % columnPoolSize;
    }

    public void reset_columns()
    {

        //private float timeSinceLastSpawned, spawnXPosition = 10f, spawnYPosition;
        //private int currColumn = 0;
        timeSinceLastSpawned = 0f;
        for (int i = 0; i < columnPoolSize; i++)
        {
            Destroy(columns[i].gameObject);
            columns[i] = (GameObject)Instantiate(columnPrefab, objectPoolPosition, Quaternion.identity);
        }
        //spawnColumn();
    }

}
