using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MachineController : MonoBehaviour
{
    [SerializeField]
    private Text[] machineLabels;

    public void init(int m, int M) {
        float x = 50f + (25f/(M == 1 ? 1 : M-1))*m;
        float z = 50f + (95f/(M == 1 ? 1 : M-1))*m;
        transform.position = new Vector3(x, 0, z);
        for (int i = 0; i < 2; ++i) {
            machineLabels[i].text = (m+1).ToString();
        }
    }
}
