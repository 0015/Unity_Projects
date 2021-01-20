using UnityEngine;

public class ReelController : MonoBehaviour
{
    private float waitTime = 5.0f;
    private float timer = 0.0f;
    private float direction = 1f;
    private float speed = 40f;

    private void Start()
    {
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer > waitTime)
        {
            direction *= -1f;
            timer = 0;
            speed = Random.Range(30.0f, 60.0f);
        }

        float rot = Time.deltaTime * speed;
        transform.Rotate(new Vector3(rot * direction, 0f, 0f));
    }
}