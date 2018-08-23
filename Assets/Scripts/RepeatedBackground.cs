using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RepeatedBackground : MonoBehaviour {

    private BoxCollider2D groundCollider;
    private float groundHorizontalLength;
    private Vector2 initialPos;

	// Use this for initialization
	void Awake () {
        groundCollider = GetComponent<BoxCollider2D>();
        groundHorizontalLength = groundCollider.size.x;
        initialPos = transform.position;
	}
	
	// Update is called once per frame
	void Update () {
        if (transform.position.x < -groundHorizontalLength &&!GameController.instance.gen_pause)
        {
            RepositionBackground();
        }
        if (GameController.instance.gen_pause)
        {
            reset_background();
        }
	}

    private void RepositionBackground()
    {
        Vector2 groundOffset = new Vector2(groundHorizontalLength * 2f, 0);
        transform.position = (Vector2)transform.position + groundOffset;
    }

    private void reset_background()
    {
        transform.position = initialPos;
    }

}
