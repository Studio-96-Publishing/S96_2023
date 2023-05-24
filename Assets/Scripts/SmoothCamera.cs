using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmoothCamera : MonoBehaviour
{
    private Vector3 tempPos;
    private Quaternion tempRot;

    public float smooth = 1.0f; // interpolation speed, increase to accelerate, decrease to decelerate.


    private void Start()
    {
        if (smooth < 0)
            smooth = Mathf.Abs(smooth);

        //tempPos = Vuforia.VuforiaManager.Instance.ARCameraTransform.position;
        //tempRot = Vuforia.VuforiaManager.Instance.ARCameraTransform.rotation;
        tempPos = this.transform.position;
        tempRot = this.transform.rotation;
    }

    void LateUpdate()
    {
        tempPos = Vector3.Lerp(tempPos, Vuforia.VuforiaManager.Instance.ARCameraTransform.position, Time.deltaTime * smooth);
        tempRot = Quaternion.Lerp(tempRot, Vuforia.VuforiaManager.Instance.ARCameraTransform.rotation, Time.deltaTime * smooth);

        transform.position = tempPos;
        transform.rotation = tempRot;
    }
}
