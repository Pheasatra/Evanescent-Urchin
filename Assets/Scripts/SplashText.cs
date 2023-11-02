using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplashText : MonoBehaviour
{
    public float timeLeft = 3.0f;

    // Start is called before the first frame update
    void Start()
    {
        GetComponent<Renderer>().sortingLayerName = "2";
        Destroy(gameObject, timeLeft);
    }

    // Update is called once per frame
    void Update()
    {
        // Move our text upwards during the process
        transform.Translate(new Vector2(0, 2.5f * Time.deltaTime), Space.World);

        // Keep our object always rotated up
        transform.rotation = Quaternion.Euler(0.0f, 0.0f, transform.parent.rotation.z * -1.0f);
    }

    public void UpdateShader()
    {

    }
}
