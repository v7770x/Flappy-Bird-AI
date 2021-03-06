﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using CSML;

public class GameController : MonoBehaviour {

    public static GameController instance;
    public GameObject gameOverText;
    public Text scoreText, topScoreText, generationText;

    public bool gameOver = false;
    private float score = 0;
    public float scrollSpeed = -1.5f;

    public int numBirds = 50, numBirdsAlive;
    public Bird birdObject;
    public int generation=0, gen_converge = -1;


    //variables to keep track of the thetas, scores
    private List<Matrix> th1List;
    private List<Matrix> th2List;
    public float[] scores;
    public GameObject colTop, colBottom;

    //keep track of mutations:
    private int mutation_min = 5, mutation_max = 7;

    //looping through generations:
    public bool gen_pause = false, auto_gen=true;

    //keep track of nueral net layer units
    public int num_inputs, num_h1_units, num_outputs = 1;

    //keep track of top score:
    private float top_score=-1, curr_avg_top=0, curr_top = -1;
    private float waitTime = 0;

    //columns

    // Use this for initialization
    void Start () {
        num_inputs = 2;
        num_h1_units = 7;
        numBirds = 50;
        //num_players = numBirds;
        th1List = new List<Matrix>(numBirds);
        th2List = new List<Matrix>(numBirds);
        if (instance == null)
        {
            instance = this;
        }else if(instance != this)
        {
            Destroy(gameObject);
            return;
        }
        initialize_matricies();
        //cp = GameObject.FindObjectOfType<ColumnPool>();
    }

    void initialize_matricies()
    {
        for (int i = 0; i < numBirds; i++)
        {
            Vector3 initialSpawnPos = new Vector3(0, 0, 0);
            Quaternion initialSpawnRotation = Quaternion.identity;
            Bird clone = Instantiate<Bird>(birdObject, initialSpawnPos, initialSpawnRotation);
            //Debug.Log(num_inputs + "  " + num_h1_units + "  " + num_outputs);
            clone.th1 = getRandomMatrix(num_h1_units, num_inputs + 1, 1.0f);
            clone.th2 = getRandomMatrix(num_outputs, num_h1_units + 1, 1.0f);
            //clone.th3 = getRandomMatrix(num_outputs, num_hidden_units_2 + 1, 0.1f);
            th1List.Add(clone.th1);
            th2List.Add(clone.th2);
            //theta3list.Add(clone.th3);
            clone.id = i;
        }

        

        generation = 1;
        numBirdsAlive = numBirds;
        //gen_pause = false;
        scores = new float[numBirds];
    }

    Matrix getRandomMatrix(int m, int n, float scale)
    {
        double[,] mat = new double[m, n];
        for (int i = 0; i < m; i++)
        {
            for (int j = 0; j < n; j++)
            {
                mat[i, j] = (double)Random.Range(-scale, scale);
            }
        }
        Matrix randMatrix = new Matrix(mat);
        return randMatrix;
    }

    // Update is called once per frame
    void Update () {
        //if(gameOver==true && Input.GetKeyDown(KeyCode.UpArrow))
        //      {
        //          SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        //      }
        if (Input.GetKeyDown(KeyCode.R))
        {
            //Application.LoadLevel(Application.loadedLevel);
        }
        else if (Input.GetKeyDown(KeyCode.N) && gen_pause)
        {
            start_next_generation();
            gen_pause = false;
        }
        else if (Input.GetKeyDown(KeyCode.P))
        {
            auto_gen = !auto_gen;
        }

        if (auto_gen&&gen_pause&&waitTime<=0)
        {
            start_next_generation();
            gen_pause = false;
        }else if (auto_gen && gen_pause)
        {
            waitTime -= Time.deltaTime;
        }
    }

    //record score when bird scores
    public void BirdScored()
    {
        if (gameOver)
        {
            return;
        }
        score++;
        scoreText.text = "Score: " + score;
    }

    public void BirdDied(int id, float score)
    {
        //gameOverText.SetActive(true);
        //gameOver = true;
        numBirdsAlive -= 1;
        scores[id] = score;
        if (numBirdsAlive <= 0)
        {
            procreate();
        }
    }

    public void procreate()
    {
        
        int[] fittest = selectFittest();
        reproduce(fittest);
        gen_pause = true;
        waitTime = 1f;
    }

    public void reproduce(int[] fittest)
    {
        //sets a randomized mutation rate
        int mutation_rate = Random.Range(mutation_min, mutation_max),
             curr_num = 0;//keeps track of how many values it has iterated through

        //new list of thetas for the next generation
        List<Matrix> next_generation_th1 = new List<Matrix>();
        List<Matrix> next_generation_th2 = new List<Matrix>();
        //List<Matrix> next_generation_th3 = new List<Matrix>();

        if (top_score < 3)
        {
            for (int i = 0; i < numBirds; i++)
            {
                next_generation_th1.Add(getRandomMatrix(th1List[0].RowCount, th1List[0].ColumnCount, 1.0f));
                next_generation_th2.Add(getRandomMatrix(th2List[0].RowCount, th2List[0].ColumnCount, 1.0f));
            }
        }
        else
        {

            //find the total weight and set up the list for the roulette wheel
            int total_weight = 0;
            List<int> rouletteWheel = new List<int>();
            int curr_ind = 0;
            int num_no_cross = (int)(numBirds * 0.5);
            for (int i = 0; i < fittest.Length; i++)
            {
                int weight = (int)scores[fittest[1]];
                total_weight += weight;
                for (int j = 0; j < weight; j++)
                {
                    rouletteWheel.Add(i);
                }
                next_generation_th1.Add(th1List[fittest[i]]);
                next_generation_th2.Add(th2List[fittest[i]]);
                curr_ind += 1;
                //next_generation_th1.Add(randomMergeMatricies(th1List[fittest[i]], th1List[fittest[i]], fittest[i], fittest[i]));
                //next_generation_th1.Add(randomMergeMatricies(th2List[fittest[i]], th2List[fittest[i]], fittest[i], fittest[i]));
                for (int k = 0; k < num_no_cross / fittest.Length; k++)
                {
                    next_generation_th1.Add(mutate(th1List[fittest[i]].Clone(), mutation_rate, ref curr_num, 0.0000f, false));
                    next_generation_th2.Add(mutate(th2List[fittest[i]].Clone(), mutation_rate, ref curr_num, 0.0000f, false));
                    next_generation_th1.Add(super_mutate(th1List[fittest[i]].Clone(), 0.05f));
                    next_generation_th2.Add(super_mutate(th2List[fittest[i]].Clone(), 0.05f));
                    //next_generation_th1.Add(mutate(th1List[fittest[i]].Clone(), mutation_rate, ref curr_num, 2.0f));
                    //next_generation_th2.Add(mutate(th2List[fittest[i]].Clone(), mutation_rate, ref curr_num, 2.0f));
                    next_generation_th1.Add(super_mutate(th1List[fittest[i]].Clone(), 1f));
                    next_generation_th2.Add(super_mutate(th2List[fittest[i]].Clone(), 1f));
                    curr_ind += 3;
                }

            }
            //int num_random = (int)(numBirds * 0.15);
            //for (int i = curr_ind; i < curr_ind + num_random; i++)
            //{
            //    next_generation_th1.Add(getRandomMatrix(th1List[0].RowCount, th1List[0].ColumnCount, 1.0f));
            //    next_generation_th2.Add(getRandomMatrix(th2List[0].RowCount, th2List[0].ColumnCount, 1.0f));
            //}
            //curr_ind += num_random;

            for (int i = curr_ind; i < numBirds; i++)
            {
                //select 2 from wheel
                int a = rouletteWheel[Random.Range(0, total_weight)];
                int b = a;
                if (fittest.Length != 1)
                {
                    while (b == a)
                    {
                        b = rouletteWheel[Random.Range(0, total_weight)];
                    }
                }

                //reproduce by merging matricies
                Matrix new_th1 = randomMergeMatricies(th1List[a], th1List[b], a, b);
                Matrix new_th2 = randomMergeMatricies(th2List[a], th2List[b], a, b);
                //Matrix new_th3 = randomMergeMatricies(theta3list[a], theta3list[b], a, b);
                new_th1 = mutate(new_th1, mutation_rate, ref curr_num, 2.0f);
                new_th2 = mutate(new_th2, mutation_rate, ref curr_num, 2.0f);
                //mutate(ref new_th3, mutation_rate, ref curr_num);
                next_generation_th1.Add(new_th1);
                next_generation_th2.Add(new_th2);
                //next_generation_th3.Add(new_th3);
            }
        }
        th1List = next_generation_th1;
        th2List = next_generation_th2;
        //theta3list = next_generation_th3;
        if (gen_converge == -1 && top_score > 100)
        {
            gen_converge = generation;
        }
        generation += 1;
        Debug.Log("mopface" + next_generation_th1.Count);

        generationText.text = "Gen: " + generation;
    }
    public Matrix randomMergeMatricies(Matrix a, Matrix b, int a_ind, int b_ind)
    {
        Matrix baby = new Matrix(a.RowCount, a.ColumnCount);

        //find total and choose where to split it
        int total_count = a.RowCount * b.ColumnCount;
        int curr_count = 0;
        int split_point = Random.Range(0, total_count);

        for (int i = 1; i <= a.RowCount; i++)
        {
            for (int j = 1; j <= b.ColumnCount; j++)
            {
                int rand_unit = Random.Range(0, 2);
                if (rand_unit==1)
                {
                    baby[i, j] = b[i, j];
                }
                else
                {
                    baby[i, j] = a[i, j];

                }
                //curr_count++;
            }
        }
        return baby;
    }

    public Matrix mutate(Matrix baby, int mutation_rate, ref int curr_num, float mutation_range, bool posneg)
    {
        Matrix mutant_baby = new Matrix(baby.RowCount, baby.ColumnCount);
        for (int i = 1; i <= baby.RowCount; i++)
        {
            for (int j = 1; j <= baby.ColumnCount; j++)
            {
                curr_num += 1;
                if (curr_num % mutation_rate == 0)
                {
                    int sign = 1;
                    if(posneg)
                        sign = Random.Range(0, 2) == 1 ? 1 : -1;
                    mutant_baby[i, j] = sign* baby[i, j] + Random.Range(-mutation_range, mutation_range);
                }
            }
        }
        return mutant_baby;
    }

    public Matrix mutate(Matrix baby, int mutation_rate, ref int curr_num, float mutation_range)
    {
        return mutate(baby, mutation_rate, ref curr_num, mutation_range, true);
    }

    public Matrix super_mutate(Matrix baby, float mutation_range)
    {
        Matrix super_mutant_baby = new Matrix(baby.RowCount, baby.ColumnCount);
        for (int i = 1; i <= baby.RowCount; i++)
        {
            for (int j = 1; j <= baby.ColumnCount; j++)
            {
                super_mutant_baby[i, j] = baby[i, j] + Random.Range(-mutation_range, mutation_range);
                
            }
        }
        return super_mutant_baby;
    }





    public int[] selectFittest()
    {
        int n_survivors = 4;
        int[] survivors_list = new int[n_survivors];
        float[] top_scores = new float[n_survivors];
        for (int i = 0; i < n_survivors; i++)
        {
            int max_score_ind = 0;
            for (int j = 1; j < numBirds; j++)
            {
                if (scores[j] > scores[max_score_ind])
                {
                    max_score_ind = j;
                }
            }
            survivors_list[i] = max_score_ind;
            top_scores[i] = scores[max_score_ind];
            scores[max_score_ind] = -1;
            Debug.Log("player: " + max_score_ind + " score: " + top_scores[i]);
        }
        curr_avg_top = 0;
        curr_top = -1;
        for (int i = 0; i < n_survivors; i++)
        {
            curr_avg_top += top_scores[i];
            if (top_scores[i] > top_score)
            {
                top_score = top_scores[i];
            }
            if (top_scores[i] > curr_top)
            {
                curr_top = top_scores[i];
            }
        }
        curr_avg_top /= n_survivors;
        //if (curr_avg_top > top_score)
        //{
        //    reward_mutation();
        //}
        //else
        //{
        //    penalize_mutation();
        //}
        weigh_tops(top_scores, survivors_list);
        scoreText.text = "Score: "+curr_top;
        topScoreText.text = "Top: " + top_score;
        return survivors_list;
    }

    public void weigh_tops(float[] top_scores, int[] survivors)
    {
        //set up roulette wheel scores in score[]
        //float[] rel_weights = new float[survivors.Length];
        //float min_score = Mathf.Min(top_scores);
        float total_score = 0;
        for (int i = 0; i < survivors.Length; i++)
        {
            total_score += top_scores[i];
        }
        for (int i = 0; i < survivors.Length; i++)
        {
            scores[survivors[i]] = 100 * top_scores[i] / total_score;
        }

    }

    public void start_next_generation()
    {
        for (int i = 0; i < numBirds; i++)
        {
            Vector3 initialSpawnPos = new Vector3(0, 0, 0);
            Quaternion initialSpawnRotation = Quaternion.identity;
            Bird clone = Instantiate<Bird>(birdObject, initialSpawnPos, initialSpawnRotation);
            clone.th1 = th1List[i];
            clone.th2 = th2List[i];
            //clone.th3 = theta3list[i];
            clone.id = i;
        }
        numBirdsAlive = numBirds;
        reset_field();
    }

    public void start_next_generation(bool auto_call)
    {
        if (auto_gen)
        {
            start_next_generation();
        }
    }

    public void reset_field()
    {
        ColumnPool.instance.reset_columns();
    }



}
