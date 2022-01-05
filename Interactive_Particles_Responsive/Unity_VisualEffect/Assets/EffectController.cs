/////////////////////////////////////////////////////////////////
/*
  Interactive Particles Responsive Made With ESP32 + INMP441 & Unity
  For More Information: https://youtu.be/lRj01J-cxew
  Created by Eric N. (ThatProject)
*/
/////////////////////////////////////////////////////////////////

using UnityEngine;
using UnityEngine.VFX;

public class EffectController : MonoBehaviour
{
    VisualEffect visualEffect;
 
    
    // Start is called before the first frame update
    void Start()
    {
        visualEffect = GetComponent<VisualEffect>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void setForceX(float x)
    {
        visualEffect.SetFloat("ForceX", x * -1); 
    }
}
