using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChartController : MonoBehaviour
{
    [SerializeField]
    private float paddingRatio = 0.01f;
    [SerializeField]
    private  float verticalLabelWidthRatio = 0.05f;
    [SerializeField]
    private float lineWidth = 0.3f;
    [SerializeField]
    private int fontSize = 200;
    [SerializeField]
    private float chartY = 5;
    [SerializeField]
    private float chartDepthPerI = 10;
    [SerializeField]
    private GameObject ChartDayText;
    [SerializeField]
    private GameObject ChartMachineText;
    [SerializeField]
    private GameObject canvas;
    [SerializeField]
    private GameObject OperationCube;
    [SerializeField]
    private Shader shader;
    private float planeWidth, planeHeight;
    private float chartWidth, chartHeight;
    private float chartX, chartZ;
    private float padding;
    private float verticalLabelWidth;
    private float dayWidth;
    private float machineWidth;
    private int maxCLabel;
    public DataFrame dataFrame;


    public void init(DataFrame _dataFrame) {
        dataFrame = _dataFrame;
        setLayout();
        drawChart();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private Vector3 mtr(Vector3 vector) {
        vector -= new Vector3(planeWidth/2.0f, 0, planeHeight/2.0f);
        return transform.rotation * vector + transform.position;
    }

    private void setLayout() {
        /* レイアウトに必要な数値計算 */
        planeWidth = transform.localScale.x * 10;
        planeHeight = transform.localScale.z * 10;
        padding = planeWidth * paddingRatio;
        verticalLabelWidth = planeWidth * verticalLabelWidthRatio;
        chartHeight = planeHeight - 3 * padding;
        chartWidth = planeWidth - 2 * padding - verticalLabelWidth;
        chartX = padding + verticalLabelWidth;
        chartZ = planeHeight - (2*padding);

        /* 日付の軸を描写 */
        GameObject go = new GameObject("dayline");
        go.AddComponent(typeof(LineRenderer));
        go.transform.SetParent(transform);
        LineRenderer lr = go.GetComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.startWidth = lineWidth; 
        lr.endWidth = lineWidth;
        lr.sharedMaterial = Resources.Load("LineMaterial", typeof(Material)) as Material;
        lr.SetPosition(0, mtr(new Vector3(chartX, 0.01f, chartZ)));
        lr.SetPosition(1, mtr(new Vector3(chartX+chartWidth, 0.01f, chartZ)));
        for (int d = 1; d <= dataFrame.MAX_D; ++d) {
            go = new GameObject("dayline" + d);
            go.AddComponent(typeof(LineRenderer));
            go.transform.SetParent(transform);
            lr = go.GetComponent<LineRenderer>();
            lr.positionCount = 2;
            lr.startWidth = lineWidth; 
            lr.endWidth = lineWidth;
            lr.sharedMaterial = Resources.Load("LineMaterial", typeof(Material)) as Material;
            lr.SetPosition(0, mtr(new Vector3(chartX+(chartWidth/dataFrame.MAX_D)*d - 0.01f, 0.01f, chartZ+padding)));
            lr.SetPosition(1, mtr(new Vector3(chartX+(chartWidth/dataFrame.MAX_D)*d - 0.01f, 0.01f, chartZ-chartHeight)));
        }

        /* 設備の軸を描写 */
        go = new GameObject("machineline");
        go.AddComponent(typeof(LineRenderer));
        go.transform.SetParent(transform);
        lr = go.GetComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.startWidth = lineWidth; 
        lr.endWidth = lineWidth;
        lr.sharedMaterial = Resources.Load("LineMaterial", typeof(Material)) as Material;
        lr.SetPosition(0, mtr(new Vector3(chartX, 0.01f, chartZ)));
        lr.SetPosition(1, mtr(new Vector3(chartX, 0.01f, chartZ-chartHeight)));
        for (int m = 0; m < dataFrame.M; ++m) {
            go = new GameObject("machineline" + m);
            go.AddComponent(typeof(LineRenderer));
            go.transform.SetParent(transform);
            lr = go.GetComponent<LineRenderer>();
            lr.positionCount = 2;
            lr.startWidth = lineWidth;
            lr.endWidth = lineWidth;
            lr.sharedMaterial = Resources.Load("LineMaterial", typeof(Material)) as Material;
            lr.SetPosition(0, mtr(new Vector3(chartX, 0.01f, chartZ-(chartHeight/dataFrame.M)*(m+1) - 0.01f)));
            lr.SetPosition(1, mtr(new Vector3(chartX-padding, 0.01f, chartZ-(chartHeight/dataFrame.M)*(m+1) - 0.01f)));
        }

        /* 日付のテキストを配置 */
        for (int d = 1; d <= dataFrame.MAX_D; ++d) {
           go = Instantiate(ChartDayText);
           go.transform.SetParent(canvas.transform);
           Text text = go.GetComponent<Text>();
           text.text = d.ToString() + " ";
           text.fontSize = fontSize;
           RectTransform rect = go.GetComponent<RectTransform>();
           rect.anchoredPosition3D = new Vector3((chartX+(chartWidth/dataFrame.MAX_D)*d)/planeWidth*100, chartZ/planeHeight*100, -0.1f);
           rect.localRotation = Quaternion.Euler(0, 0, 0);
           rect.localScale = new Vector3(0.01f, 0.01f, 0.01f);
        }

        /* 設備のテキストを配置 */
        for (int m = 0; m < dataFrame.M; ++m) {
           go = Instantiate(ChartMachineText);
           go.transform.SetParent(canvas.transform);
           Text text = go.GetComponent<Text>();
           text.text = "設備  \nm: " + (m+1) + "  \n能力増加コスト  \nCm: " + dataFrame.C[m] + "  \n製造時間係数  \nDm: " + dataFrame.D[m] + "  ";
           text.fontSize = fontSize/2;
           RectTransform rect = go.GetComponent<RectTransform>();
           rect.anchoredPosition3D = new Vector3(chartX/planeWidth*100, (chartZ-(chartHeight/dataFrame.M)*(m+1))/planeHeight*100 - 1f, -0.1f);
           rect.localRotation = Quaternion.Euler(0, 0, 0);
           rect.localScale = new Vector3(0.01f, 0.01f, 0.01f);
        }
    }

    private void drawChart() {
        /* 各工程に対応するCubeを作成 */
        GameObject chart = GameObject.Find("Chart");
        if (dataFrame.MAX_C < 1000) {
            maxCLabel = (dataFrame.MAX_C + 10) / 10 * 10;
        } else {
            maxCLabel = (dataFrame.MAX_C + 100) / 100 * 100;
        }
        for (int r = 0; r < dataFrame.R; ++r) {
            Operation ope = dataFrame.operations[r];
            Bom bom = ope.bom;
            Material material = new Material(shader);
            float[] rgb = new float[3];
            for (int i = 0; i < 3; ++i) {
                    rgb[i] = ((r%2)*200+r*10+(r+1)*(i+1)*100)%255/255f;
                    rgb[i] = Mathf.Max(rgb[i], 0f);
                    rgb[i] = Mathf.Min(rgb[i], 1f);
            }
            for (int p = 0; p < bom.p; ++p) {
                int t1 = ope.t1[p];
                int t2 = ope.t2[p];
                while (t1 != t2) {
                    GameObject operationCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    int nDay = (t1+DataFrame.SECONDS_A_DAY)/DataFrame.SECONDS_A_DAY;
                    int capa = dataFrame.mdToC[bom.pTom[p]][nDay];
                    float height = chartHeight/dataFrame.M/maxCLabel*capa;
                    float width = (float)(Mathf.Min(t2-t1, nDay*DataFrame.SECONDS_A_DAY - t1))/DataFrame.SECONDS_A_DAY*chartWidth/dataFrame.MAX_D;
                    float depth = chartDepthPerI/2;
                    float x = chartX + ((float)(t1)/DataFrame.SECONDS_A_DAY)*(chartWidth/dataFrame.MAX_D);
                    float z = chartZ - (bom.pTom[p]+1)*(chartHeight/dataFrame.M);
                    float y = chartY + bom.i*chartDepthPerI;
                    operationCube.transform.SetParent(chart.transform);
                    operationCube.transform.position = mtr(new Vector3(x+width/2, y+depth/2, z+height/2));
                    operationCube.transform.localScale = new Vector3(width, depth, height);
                    operationCube.transform.rotation = this.transform.rotation;
                    Renderer renderer = operationCube.GetComponent<Renderer>();
                    renderer.material = material;
                    renderer.material.color = new Color(rgb[0], rgb[1], rgb[2]);
                    t1 = Mathf.Min(t2, nDay*DataFrame.SECONDS_A_DAY);
                }
            }
        }
    }
    
}
