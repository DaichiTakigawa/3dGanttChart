using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Supervisor : MonoBehaviour
{
    public float timeSpeed = 1f;
    private float floatTime = 0f;
    private int maxTime;
    public DataFrame dataFrame;
    public ChartController chartController;
    public Text timeText;
    public Text currentDataText;
    public Text capacityText;
    public int currentTime = 0;
    public List<int> currentMtoR;
    public bool poseflag = false;
    [SerializeField]
    private GameObject MachinePrehab;
    [SerializeField]
    private GameObject BarrelPrehab;
    private List<BarrelController> barrels;
    private int nextR = 0;
    [SerializeField]
    // private float anime_time = 100f;
    // private float anime_elapsed_time = 0f;
    // private int day_count = 0;
    // private bool animeFlag = false;


    // Start is called before the first frame update
    void Start() {
        dataFrame = new DataFrame();
        dataFrame.init();
        init();
        chartController.init(dataFrame);
        timeText.text = "1日目00時00分00秒";
        currentDataText.text = currentDataOfTime(currentTime);
        capacityText.text = currentCapaOfTime(currentTime);
    }

    void FixedUpdate() {
        if (!poseflag) floatTime += Time.fixedDeltaTime * timeSpeed;
        floatTime = Mathf.Min(floatTime, maxTime);
    }

    // Update is called once per frame
    void Update() {
        if (!poseflag) {
            int nTime = (int)floatTime;
            if (nTime >= maxTime) {
                nTime = maxTime;
                poseflag = true;
            }
            if (nTime != currentTime) {
                currentTime = nTime;
                updateSituation(currentTime);
                currentDataText.text = currentDataOfTime(currentTime);
                capacityText.text = currentCapaOfTime(currentTime);
                timeText.text = timeFormatter(currentTime, true);
                chartController.updateTimeLine(currentTime);
                while (nextR < dataFrame.R && dataFrame.operations[nextR].t1[0] <= nTime) {
                    createBarrel(nextR++);
                }
                var nbarrels = new List<BarrelController>();
                foreach (var bc in barrels) {
                    if (bc.barrelUpdate()) {
                        nbarrels.Add(bc);
                    }
                }
                barrels = nbarrels;
                BarrelController.updateStoredOrders();
            }
            /*
            int nday = (currentTime+DataFrame.SECONDS_A_DAY)/DataFrame.SECONDS_A_DAY;
            if (nday != day_count) {
                day_count = nday;
                animeFlag = true;
                timeText.text = timeFormatter((day_count-1)*DataFrame.SECONDS_A_DAY, true);
                EmployeeController.employeeUpdate(day_count);
                anime_elapsed_time = 0f;
            }
            */
        }
        /*
        if (animeFlag) {
            if (anime_elapsed_time > anime_time) animeFlag = false;
            anime_elapsed_time += Time.deltaTime;
        }
        */
    }

    void init() {
        currentMtoR = new List<int>(dataFrame.M);
        for (int m = 0; m < dataFrame.M; ++m) {
            for (int r = 0; r < dataFrame.R; ++r) {
                if (dataFrame.operations[r].mTop[m] != -1) {
                    currentMtoR.Add(r);
                    break;
                }
            }
        }
        maxTime = dataFrame.MAX_D*DataFrame.SECONDS_A_DAY;
        for (int m = 0; m < dataFrame.M; ++m) {
            GameObject go = Instantiate(MachinePrehab);
            go.GetComponent<MachineController>().init(m, dataFrame.M);
            go.name = "machine" + m;
        }
        barrels = new List<BarrelController>();
        // EmployeeController.init(dataFrame, anime_time);
    }
    void updateSituation(int time) {
        for (int m = 0; m < dataFrame.M; ++m) {
            int r = currentMtoR[m];
            if (r == dataFrame.R) continue;
            Operation ope = dataFrame.operations[r];
            int p = ope.mTop[m];
            if (time > ope.t2[p]) {
                int _r;
                for (_r = r+1; _r < dataFrame.R; ++_r) {
                    if (dataFrame.operations[_r].mTop[m] != -1) {
                        int _p = dataFrame.operations[_r].mTop[m];
                        if (dataFrame.operations[_r].t2[_p] >= time) break;
                    } 
                }
                currentMtoR[m] = _r;
            }
        }
    }

    private string currentDataOfTime(int time) {
        string currentData = "入力データ\n";
        currentData += "\t設備数: " + dataFrame.M + ", ";
        currentData += "品目数: " + dataFrame.I + ", ";
        currentData += "最大工程数: " + dataFrame.P + ", ";
        currentData += "オーダ数: " + dataFrame.R + "\n";
        currentData += "\tBOM\n";
        for (int i = 0; i < dataFrame.I; ++i) {
            currentData += "\t\t品目: " + (i+1) + "\n";
            for (int p = 0; p < dataFrame.boms[i].p; ++p) {
                currentData += "\t\t\t工程番号: " + (p+1) + ", ";
                currentData += "設備番号: " + (dataFrame.boms[i].pTom[p]+1) + ", ";
                currentData += "製造スピード: " + dataFrame.boms[i].times[p] + "\n";
            }
        }

        currentData += "スコア\n";
        currentData += "\t粗利合計: " + dataFrame.gross_profit + ", ";
        currentData += "コスト合計: " + dataFrame.cost_sum + ", ";
        currentData += "利益プロフィット: " + dataFrame.total_profit + "\n";

        currentData += "納期遅れオーダ\n";
        if (dataFrame.deadLineViolationOrders.Count == 0) {
            currentData += "\tなし\n";
        } else {
            currentData += "\tオーダ番号: ";
            for (int i = 0; i < dataFrame.deadLineViolationOrders.Count; ++i) {
                currentData += (dataFrame.deadLineViolationOrders[i]+1).ToString();
                if (i+1 != dataFrame.deadLineViolationOrders.Count) currentData += ", ";
                else currentData += "\n";
            }
        }

       currentData += "\n現在の設備状況\n";
        for (int m = 0; m < dataFrame.M; ++m) {
            currentData += "\t設備番号: " + (m+1) + "\n";
            int r = currentMtoR[m];
            if (r == dataFrame.R) {
                currentData += "\t\tなし\n\n";
                continue;
            }
            Operation ope = dataFrame.operations[r];
            int p = ope.mTop[m];
            if (time < ope.t1[p]) {
                currentData += "\t\tなし\n\n";
            } else {
                currentData += "\t\tオーダ番号: " + (r+1) + ", ";
                currentData += "品目番号: " + (ope.bom.i+1) + ", ";
                currentData += "工程番号: " + (p+1) + "\n";
                currentData += "\t\t製造開始時間: " + timeFormatter(ope.t1[p], true) + "(" + ope.t1[p] + "s), ";
                currentData += "製造終了時間: " + timeFormatter(ope.t2[p], true) + "(" + ope.t2[p] + "s), ";
                currentData += "製造終了までの残り時間: " + timeFormatter(ope.t2[p]-time, false) + "(" + (ope.t2[p]-time) +  "s)\n";
            }
        }

        return currentData;
    }

    private string timeFormatter(int time, bool date) {
        int day = (time+DataFrame.SECONDS_A_DAY)/DataFrame.SECONDS_A_DAY;
        int hour = (time-DataFrame.SECONDS_A_DAY*(day-1))/3600;
        int minute = (time-DataFrame.SECONDS_A_DAY*(day-1)-3600*hour)/60;
        int second = (time-DataFrame.SECONDS_A_DAY*(day-1)-3600*hour-60*minute);
        string res;
        if (date) {
            res = day + "日目" + (hour < 10 ? "0" : "") + hour + "時" + (minute < 10 ? "0" : "") + minute + "分" + (second < 10 ? "0" : "") + second + "秒";
        } else {
            res = (--day) + "日" + (hour < 10 ? "0" : "") + hour + "時間" + (minute < 10 ? "0" : "") + minute + "分" + (second < 10 ? "0" : "") + second + "秒";
        }
        return res;
    }

    private string currentCapaOfTime(int time) {
        int day = (time+DataFrame.SECONDS_A_DAY)/DataFrame.SECONDS_A_DAY;
        string currentCapaData = "現在の能力値データ\n";
        for (int m = 0; m < dataFrame.M; ++m) {
            currentCapaData += "\t設備番号: " + (m+1) + ", 能力値: " + dataFrame.mdToC[m][day] + "\n";
        }
        return currentCapaData;
    }

    private void createBarrel(int r) {
        GameObject barrel = Instantiate(BarrelPrehab);
        barrel.GetComponent<BarrelController>().init(this, dataFrame.operations[r], dataFrame.M);
        barrel.name = "order" + r;
        barrels.Add(barrel.GetComponent<BarrelController>());
    }
}