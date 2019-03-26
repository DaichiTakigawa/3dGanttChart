using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Supervisor : MonoBehaviour
{
    public DataFrame dataFrame;
    public ChartController chartController;
    // Start is called before the first frame update
    void Start()
    {
        dataFrame = new DataFrame();
        dataFrame.init();
        chartController.init(dataFrame);

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
