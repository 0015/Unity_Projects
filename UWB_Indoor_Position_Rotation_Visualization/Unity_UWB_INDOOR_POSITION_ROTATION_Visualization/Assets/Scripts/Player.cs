/////////////////////////////////////////////////////////////////
/*
  ESP32 + UWB + IMU | Indoor Position & Rotation + Unity Visualization
  For More Information: https://youtu.be/fPuxcjHsfpc
  Created by Eric N. (ThatProject)
*/
/////////////////////////////////////////////////////////////////

using UnityEngine;

public class Player : MonoBehaviour
{
    // My Offsets
    [SerializeField] int meterToSpace = 20;
    [SerializeField] int x_offset = 20;
    [SerializeField] int z_offset = -20;
    private Quaternion inverseQt;
    private Quaternion rawQt;
    
    void Start()
    {
        inverseQt = transform.rotation;
    }
    
    public void resetRotation()
    {
        inverseQt = Quaternion.Inverse(rawQt);
    }
        
    public void movePlayer(double x, double y)
    {
        if (double.IsNaN(x))
            x = 0;
        if (double.IsNaN(y))
            y = 0;

        transform.position = new Vector3((float)y * meterToSpace * -1 + x_offset, 0, (float)x * meterToSpace + z_offset);
    }

    public void rotateCap(Quaternion q)
    {
        rawQt = q;
        Vector3 eulerAngles = (rawQt * inverseQt).eulerAngles;
        Quaternion yAxisRotation = Quaternion.Euler(0, eulerAngles.y * -1, 0);
        transform.rotation = yAxisRotation;
    }
}