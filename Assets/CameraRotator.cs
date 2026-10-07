using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRotator : MonoBehaviour
{

    float rotationAroundCircle=0;
    int rotating;
    float rotationSpeed = 10f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        rotationAroundCircle += Time.deltaTime*rotating* rotationSpeed;
        updateCameraTransform();
    }

    public void OnRotate()
    {
        rotating += 1;
        if (rotating == 6) rotating = 0;
    }
    void updateCameraTransform()
    {
        float radians = Mathf.Deg2Rad * rotationAroundCircle;
        transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, -rotationAroundCircle-90, transform.localEulerAngles.z);
        transform.localPosition = new Vector3(1.9f*Mathf.Cos(radians), transform.localPosition.y, 1.9f*Mathf.Sin(radians));
    }
}
