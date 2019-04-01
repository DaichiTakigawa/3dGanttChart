using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BarrelController : MonoBehaviour
{
    private static List<List<BarrelController>> MtoStoredOrders;
    private static int M = 0;
    private float padding_bottom = 1f;
    [SerializeField]
    private Text barrelLabel;
    [SerializeField]
    private Image background;
    private int p = -1;
    private Supervisor supe;
    private Operation ope;
    private int MAX_D;
    enum State {show, undergo, fade, store}
    private State state;

    public bool barrelUpdate() {
        int time = supe.currentTime;
        if (p == ope.bom.p)  return false;
        if (ope.t2[ope.bom.p-1] < time) {
            StartCoroutine("fade");
            p = ope.bom.p;
            return false;
        }

        int np = p;
        while (np+1 < ope.bom.p && ope.t1[np+1] <= time) ++np;
        

        if (np == p) {
            if (state == State.undergo && ope.t2[np] < time) {
                store();
                state = State.store;
            }
        } else {
            p = np;
            updateLabel();
            if (ope.t1[p] <= time && time <= ope.t2[p]) {
                StartCoroutine("show");
                state = State.undergo;
            } else {
                store();
                state = State.store;
            }
        }

        return true;
    }

    public void init(Supervisor _supe, Operation _ope, int _M) {
        supe = _supe;
        ope = _ope;
        M = _M;
        state = State.store;
        MAX_D = _supe.dataFrame.MAX_D;
        float[] rgb = new float[3];
        int r = ope.r;
        for (int i = 0; i < 3; ++i) {
                rgb[i] = ((r%2)*200+r*10+(r+1)*(i+1)*100)%255/255f;
                rgb[i] = Mathf.Max(rgb[i], 0f);
                rgb[i] = Mathf.Min(rgb[i], 1f);
        }
        background.color = new Color(rgb[0], rgb[1], rgb[2], 0.8f);
        if (MtoStoredOrders == null) {
            MtoStoredOrders = Enumerable.Range(0, M).Select((x) => new List<BarrelController>()).ToList();
        }
    }

    private void updateLabel() {
        string str = "オーダ番号: " + (ope.r+1) + "\n";
        str += "品目番号: " + (ope.bom.i+1) + "\n";
        str += "工程番号: " + (p+1) + "\n";
        str += "工程開始時刻: " + ope.t1[p] + "s\n";
        str += "工程終了時刻: " + ope.t2[p] + "s\n";
        barrelLabel.text = str;
    }

    private IEnumerator show() {
        int m = ope.pTom[p];
        float sx = 50f + (25f/(M == 1 ? 1 : M-1))*m + 4f;
        float ex = sx + 20f - 4f;
        float z = 50f + (95f/(M == 1 ? 1 : M-1))*m + 2f;
        float y = padding_bottom; 
        Vector3 startpos = new Vector3(sx, y, z);
        Vector3 endpos = new Vector3(ex, y, z);
        int start_time = ope.t1[p];
        int end_time = ope.t2[p];
        while (true) {
            int time = supe.currentTime;
            if (time >= end_time) break;
            transform.position = endpos - (endpos-startpos)*((float)end_time-time)/((float)end_time-start_time);
            yield return null;
        }
        yield break;
    }
    private void store() {
        int m = ope.pTom[p];
        MtoStoredOrders[m].Add(this);
    }

    private IEnumerator fade() {
        int m = ope.pTom[p];
        float x = 50f + (25f/(M == 1 ? 1 : M-1))*m + 20f + 20f;
        float z = 50f + (95f/(M == 1 ? 1 : M-1))*m + 2f;
        float y = padding_bottom;
        transform.position = new Vector3(x, y, z);
        Vector3 speed = new Vector3(0, 25f, 0);
        float elapsed_time = 0;
        while (true) {
            if (elapsed_time > 2f) break;
            elapsed_time += Time.deltaTime;
            transform.position += speed*elapsed_time*Time.deltaTime;
            transform.rotation *= Quaternion.Euler(0f, 1000f*Time.deltaTime*elapsed_time, 0f);
            yield return null;
        }
        Destroy(this.gameObject);
        yield break;
    }

    public static void updateStoredOrders() {
        for (int m = 0; m < M; ++m) {
            var nStoredOrders = new List<BarrelController>();
            foreach (var bc in MtoStoredOrders[m]) {
                if (bc.p == bc.ope.bom.p || bc.ope.pTom[bc.p] != m) continue;
                nStoredOrders.Add(bc);
            }
            MtoStoredOrders[m] = nStoredOrders;
            float x = 50f + (25f/(M == 1 ? 1 : M-1))*m + 20f + 10f;
            float z = 50f + (95f/(M == 1 ? 1 : M-1))*m + 2f;
            Vector3 pos = new Vector3(x, 0, z);
            float between = 5f;
            foreach (var storedOrder in MtoStoredOrders[m]) {
                storedOrder.transform.position = pos;
                pos += new Vector3(between, 0f, 0f);
            }
        }
    }
}
