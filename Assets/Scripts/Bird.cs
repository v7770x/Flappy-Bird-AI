using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CSML;

public class Bird : MonoBehaviour {

    private bool is_dead = false;
    private Rigidbody2D rb2d;
    public float up_force = 200f;
    private Animator anim;

    //to keep track of the network matricies
    public Matrix th1, th2;
    public ColumnPool cp;
    public int id;

    //keep tract of score
    public float score = 0;

	// Use this for initialization
	void Start () {
        rb2d = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        cp = GameObject.FindObjectOfType<ColumnPool>();
	}

    private void FixedUpdate()
    {
        
    }

    // Update is called once per frame
    void Update () {
        //if (!is_dead)
        //{
        //    if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.UpArrow))
        //    {
        //        flap();
        //    }
        //}
        Vector2 currPos = transform.position;
        Matrix inputs = cp.getInputs(currPos.x, currPos.y);
        float output = (float)forwardProp(inputs)[1,1].Re;
        Debug.Log(output);
        if (output > 0.5)
        {
            flap();
        }
    }

    void flap()
    {
        rb2d.velocity = Vector2.zero;
        rb2d.AddForce(new Vector2(0, up_force));
        anim.SetTrigger("Flap");
    }


    Matrix forwardProp(Matrix inputs)
    {
        applyActivationFunction(ref inputs);

        inputs.InsertRow(Matrix.Identity(1), 1);
        Matrix hiddenLayer1 = (th1 * inputs);
        applyActivationFunction(ref hiddenLayer1);

        hiddenLayer1.InsertRow(Matrix.Identity(1), 1);
        Matrix output = th2 * hiddenLayer1;
        applyActivationFunction(ref output);

        return output;
    }

    void applyActivationFunction(ref Matrix m)
    {
        for(int i=1; i<= m.RowCount; i++)
        {
            for(int j=1; j<= m.ColumnCount; j++)
            {
                m[i, j] = new Complex(sigmoid(m[i, j].Re));
            }
        }
    }

    double sigmoid(double val)
    {
        return 1 / (1 + Mathf.Exp(-(float)val));
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        //Debug.Log(other.gameObject.tag);
        if (other.gameObject.tag != "Bird")
        {
            rb2d.velocity = Vector2.zero;
            is_dead = true;
            anim.SetTrigger("Die");
            GameController.instance.BirdDied(id, score +Time.time);
            Destroy(gameObject);
        }
        else
        {
            Physics2D.IgnoreCollision(other.gameObject.GetComponent<Collider2D>(), GetComponent<Collider2D>());
        }
        
    }
    public void score_big()
    {
        score += 3;
    }

}
