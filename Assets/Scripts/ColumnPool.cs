using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CSML;

public class ColumnPool : MonoBehaviour {

    public static ColumnPool instance;

    public int columnPoolSize = 5;
    public GameObject columnPrefab;
    public float spawnRate = 4f;
    public float columnMin = -0.5f;
    public float columnMax = 2f;

    public GameObject[] columns;
    private Vector2 objectPoolPosition = new Vector2(-15f, -25f);
    private float timeSinceLastSpawned, spawnXPosition = 4f, spawnYPosition;
    private int currColumn = 0;

    private float colTopPos, colBottomPos;

	// Use this for initialization
	void Start () {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }
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
        if (GameController.instance.gen_pause == false && timeSinceLastSpawned >= spawnRate)
        {
            spawnColumn();
        }
	}

    public Matrix getInputs(float xPos, float yPos)
    {
        Matrix inputs =  new Matrix(GameController.instance.num_inputs,1);
        int col_num = currColumn - 1;
        if (col_num == -1)
        {
            col_num = 4;
        }
        Vector2 columnPos = columns[col_num].gameObject.transform.position;
        float dy = normalize_y(yPos-(columnPos.y + (colTopPos + colBottomPos) / 2 )),
                dx = normalize_x(columnPos.x + 1 - xPos),
                y = normalize_y(yPos);
        inputs[1, 1] = new Complex(dx );
        inputs[2,1] = new Complex(dy);
        //inputs[3, 1] = new Complex(y);
        //Debug.Log(inputs);
        return inputs;
    }
    float normalize_y(float y)
    {
        return y / (9.7f / 2);
    }
    float normalize_x(float x)
    {
        return x / (spawnXPosition+1);
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
        //Debug.Log("here");
        timeSinceLastSpawned = 0f;
        for (int i = 0; i < columnPoolSize; i++)
        {
            columns[i].transform.position = new Vector2(-15f, 10f);
        }
        spawnColumn();
    }

}
