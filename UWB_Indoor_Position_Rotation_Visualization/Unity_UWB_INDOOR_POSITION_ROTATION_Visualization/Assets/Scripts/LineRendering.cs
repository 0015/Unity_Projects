/////////////////////////////////////////////////////////////////
/*
  ESP32 + UWB + IMU | Indoor Position & Rotation + Unity Visualization
  For More Information: https://youtu.be/fPuxcjHsfpc
  Created by Eric N. (ThatProject)
*/
/////////////////////////////////////////////////////////////////

using UnityEngine;

public class LineRendering : MonoBehaviour
{
    public LineRenderer line;
    public Transform targetPos;

    // Start is called before the first frame update
    void Start()
    {
        line.positionCount = 2;
    }

    // Update is called once per frame
    void Update()
    {
        line.SetPosition(0, transform.position);
        line.SetPosition(1, targetPos.position);
    }
}