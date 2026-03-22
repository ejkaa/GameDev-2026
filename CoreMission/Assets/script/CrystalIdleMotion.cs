using UnityEngine;

public class CrystalIdleMotion : MonoBehaviour
{
    public float rotateSpeed = 60f;
    public float floatAmplitude = 0.25f;
    public float floatFrequency = 2f;

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        transform.Rotate(0f, rotateSpeed * Time.deltaTime, 0f);

        Vector3 pos = startPos;
        pos.y += Mathf.Sin(Time.time * floatFrequency) * floatAmplitude;
        transform.position = pos;
    }
}