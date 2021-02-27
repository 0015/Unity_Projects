using TMPro;
using UnityEngine;

public class ReelController : MonoBehaviour
{
    [SerializeField] private TMP_Text stateText;
    [SerializeField] private Transform reel;
    [SerializeField] private Transform body;
    [SerializeField] private GameObject endCube;
    private Quaternion rawQuat;
    private Quaternion quatLookAtCam;
    private float rotatedAroundX;
    private Vector3 lastUp;
    private Vector3 lastForward;
    private float previousBodyY;
    private float totalAmountAngeY;
    private Rigidbody endCubeRB;
    private float gravityScale = 10.0f; // 10 times gravity
    private static float globalGravity = -9.81f;


    private void Start()
    {
        endCubeRB = endCube.GetComponent<Rigidbody>();
        quatLookAtCam = Quaternion.identity;
        previousBodyY = 0;
        totalAmountAngeY = 0;
    }

    public void updateQuaternion(Quaternion _rawQuat)
    {
        rawQuat = _rawQuat;
        var calcRotation = quatLookAtCam * rawQuat;

        transform.rotation = calcRotation;
        var rotationDifferenceX = Vector3.SignedAngle(transform.up, lastUp, transform.right);
        rotatedAroundX += rotationDifferenceX;

        var rotationDifferenceY = Vector3.SignedAngle(transform.forward, lastForward, transform.up);

        if (Mathf.Abs(rotationDifferenceX) < 0.1 && Mathf.Abs(rotationDifferenceY) < 0.1)
        {
            setStateText("Idle");
        }
        else if (Mathf.Abs(rotationDifferenceX) * 2 > Mathf.Abs(rotationDifferenceY))
        {
            setStateText("Rotating Reel");
            reel.transform.localEulerAngles = new Vector3(rotatedAroundX, 0, 0);
        }
        else
        {
            setStateText("Rotating Body");
            float amountMove = previousBodyY - transform.eulerAngles.y;
            float amountMoveWithSign = Mathf.Abs(amountMove) * rotationDifferenceY > 0 ? -1 : 1;

            if (Mathf.Abs(amountMove) >= 0.1f)
            {
                if (endCubeRB != null)
                {
                    endCubeRB.velocity = endCubeRB.velocity * 0.9f;
                    endCubeRB.angularVelocity = endCubeRB.angularVelocity * 0.9f;
                    Vector3 gravity = globalGravity * gravityScale * Vector3.up;
                    endCubeRB.AddForce(gravity, ForceMode.Acceleration);
                }

                var bodyY = body.eulerAngles.y + amountMoveWithSign;
                body.localEulerAngles = new Vector3(-90, bodyY, 0);
            }

            previousBodyY = transform.eulerAngles.y;
        }


        lastUp = transform.up;
        lastForward = transform.forward;
    }

    public void ResetRotation()
    {
        quatLookAtCam = Quaternion.Inverse(rawQuat);
        totalAmountAngeY = 0;
        previousBodyY = 0;
        body.localEulerAngles = new Vector3(-90, 0, 0);
    }

    void setStateText(string text)
    {
        if (stateText == null) return;
        stateText.text = "Status: " + text;
    }
}