using UnityEngine;

public class CamShaker : MonoBehaviour 
{

    Vector3 originalCameraPosition;

    float shakeAmt = 0;

    public Camera mainCamera;

    void Start(){
        originalCameraPosition = mainCamera.transform.position;
    }

    public void DoShake() 
    {

        shakeAmt = .05f;
        InvokeRepeating("CameraShake", 0, .01f);
        Invoke("StopShaking", 0.3f);

    }

    public void TinyShake() 
    {

        shakeAmt = .015f;
        InvokeRepeating("CameraShake", 0, .02f);
        Invoke("StopShaking", 0.21f);

    }

    void CameraShake()
    {
        if(shakeAmt>0) 
        {
            float quakeAmt = Random.value*shakeAmt*2 - shakeAmt;
            Vector3 pp = mainCamera.transform.position;
            pp.x+= quakeAmt;
            pp.y+= quakeAmt; // can also add to x and/or z
            mainCamera.transform.position = pp;
        }
    }

    void StopShaking()
    {
        CancelInvoke("CameraShake");
        mainCamera.transform.position = originalCameraPosition;
    }

}