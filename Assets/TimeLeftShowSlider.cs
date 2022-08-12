using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimeLeftShowSlider : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public GameObject Background;

    // Update is called once per frame
    void Update()
    {
        float value = gameObject.GetComponent<Slider>().value;
        if (value != 0)
        {
            Background.SetActive(true);
        }
        else
        {
            Background.SetActive(false);
        }
    }
}
