using UnityEngine;

public class PlayerScript : MonoBehaviour
{

    public float speed;
    private Rigidbody2D rb2d;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        float moveHorizontal = Input.GetAxis("Horizontal");
        Debug.Log(moveHorizontal);
        float moveVertical = Input.GetAxis("Vertical");

        rb2d.linearVelocity = new Vector2(moveHorizontal * speed, moveVertical * speed);
    }
}
