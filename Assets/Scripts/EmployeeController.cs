using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;

public class EmployeeController : MonoBehaviour
{
    [SerializeField]
    private Animator anime;
    [SerializeField]
    private Text idText;
    private static GameObject EmployeePrehab;
    private static List<int> dToC;
    private static List<List<int>> mdToC;
    private static int M;
    private static int D;
    private static int MAX_EMP = 0;
    private static float anime_time;
    private static List<EmployeeController> employees;
    private static List<int> preids;
    private static List<int> nxtids;
    private static int lastid;
    private static Vector3 exit = new Vector3(200f, 0f, 100f);
    private static Supervisor supe; 
    private bool moveFlag = false;
    public enum State {getIn, stay, getOut};
    public State state;
    public int id;
    public int[] pos_m = new int[2];
    public int[] pos_i = new int[2];
    public float elapsed_time;

    public static void init(DataFrame dataFrame, float _anime_time, Supervisor _supe) {
        M = dataFrame.M;
        D = dataFrame.MAX_D;
        mdToC = dataFrame.mdToC;
        dToC = Enumerable.Repeat(0, D+1).ToList();
        for (int m = 0; m < M; ++m) {
            for (int d = 1; d <= D; ++d) {
                dToC[d] += dataFrame.mdToC[m][d];
            }
        }
        for (int d = 1; d <= D; ++d) {
            MAX_EMP = Mathf.Max(MAX_EMP, dToC[d]);
        }
        anime_time = _anime_time;
        supe = _supe;
        employees = new List<EmployeeController>(MAX_EMP);
        preids = new List<int>(MAX_EMP);
        nxtids = new List<int>(MAX_EMP);
        lastid = MAX_EMP-1;
        EmployeePrehab = (GameObject)Resources.Load("Prehabs/EmployeeRagdoll");
    }

    public static void employeeUpdate(int d) {
        var nxtids = new List<int>();
        for (int i = 0; i < dToC[d]; ++i) {
            int _id = (lastid+i+1)%MAX_EMP;
            nxtids.Add(_id);
        }
        nxtids.Sort();
        Dictionary<int, State> idToState = new Dictionary<int, State>();
        for (int i = 0; i < preids.Count; ++i) {
            int idx = nxtids.BinarySearch(preids[i]);
            if (idx >= 0 && nxtids[idx] == preids[i]) {
                idToState.Add(preids[i], State.stay);
            } else {
                idToState.Add(preids[i], State.getOut);
            }
        }
        List<int> newids = new List<int>();
        for (int i = 0; i < nxtids.Count; ++i) {
            int idx = preids.BinarySearch(nxtids[i]);
            if (idx < 0 || preids[idx] != nxtids[i]) {
                idToState.Add(nxtids[i], State.getIn);
                newids.Add(nxtids[i]);
            }
        }

        List<int> mToStayCount = Enumerable.Repeat(0, M).ToList();
        for (int i = 0; i < employees.Count; ++i) {
            if (idToState[employees[i].id] == State.stay) {
                ++mToStayCount[employees[i].pos_m[1]];
            }
        }

        List<int> mTodiff = Enumerable.Repeat(0, M).ToList();
        for (int m = 0; m < M; ++m) {
            mTodiff[m] = mToStayCount[m] - mdToC[m][d];
        }

        Dictionary<int, int> idToDestM = new Dictionary<int, int>();
        for (int i = 0; i < employees.Count; ++i) {
            if (idToState[employees[i].id] == State.stay) {
                int m = employees[i].pos_m[1];
                if (mTodiff[m] <= 0) {
                    idToDestM.Add(employees[i].id, m);
                } else {
                    for (int _m = 0; _m < M; ++_m) {
                        if (mTodiff[_m] < 0) {
                            idToDestM.Add(employees[i].id, _m);
                            ++mTodiff[_m];
                            --mTodiff[m];
                            break;
                        }
                    }
                }
            }
        }


        for (int i = 0; i < newids.Count; ++i) {
            for (int m = 0; m < M; ++m) {
                if (mTodiff[m] < 0) {
                    idToDestM.Add(newids[i], m);
                    ++mTodiff[m];
                    break;
                }
            }
        }

        List<EmployeeController> tmpEmployees = new List<EmployeeController>();
        List<int> mToI = Enumerable.Repeat(0, M).ToList();
        for (int i = 0; i < employees.Count; ++i) {
            if (idToState[employees[i].id] == State.getOut) {
                employees[i].state = State.getOut;
                employees[i].moveFlag = true;
                employees[i].pos_m[0] = employees[i].pos_m[1];
                employees[i].pos_i[0] = employees[i].pos_i[1];
            } else {
                employees[i].state = State.stay;
                employees[i].moveFlag = true;
                employees[i].pos_m[0] = employees[i].pos_m[1];
                employees[i].pos_i[0] = employees[i].pos_i[1];
                employees[i].pos_m[1] = idToDestM[employees[i].id];
                employees[i].pos_i[1] = mToI[idToDestM[employees[i].id]]++;
                tmpEmployees.Add(employees[i]);
            }
        }

        for (int i = 0; i < newids.Count; ++i) {
            GameObject go = Instantiate(EmployeePrehab);
            go.name = "employee" + (newids[i]+1);
            go.transform.rotation = Quaternion.Euler(0, 90f + Random.Range(-2f, 2f), 0);
            go.transform.position = new Vector3(0f, 0f, 100f + Random.Range(-10f, 10f));
            EmployeeController ec = go.GetComponent<EmployeeController>();
            ec.id = newids[i];
            ec.state = State.getIn;
            ec.moveFlag = true;
            ec.pos_m[1] = idToDestM[newids[i]];
            ec.pos_i[1] = mToI[idToDestM[newids[i]]]++;
            ec.idText.text = "Employee" + (newids[i]+1);
            tmpEmployees.Add(ec);
        }
        employees = tmpEmployees;
        preids = nxtids;
        lastid = (lastid+dToC[d])%MAX_EMP;
    }

    public void Update() {
        if (moveFlag) {
            anime.SetBool("run", true);
            elapsed_time = 0f;
            if (state == State.getIn) {
                getIn();
            } else if (state == State.stay) {
                stay();
            } else {
                getOut();
            }
            moveFlag = false;
        }
        if (anime.GetBool("run")) {
            NavMeshAgent agent = GetComponent<NavMeshAgent>();
            anime.SetFloat("speed", agent.velocity.magnitude*0.1f);
            float x = 55f + (25f/(M==1 ? 1 : M-1))*pos_m[1] + pos_i[1]%15;
            float z = 55f + (95f/(M==1 ? 1 : M-1))*pos_m[1] + pos_i[1]/15;
            Vector3 goal = new Vector3(x, 0, z);
            if (state != State.getOut && (transform.position-goal).magnitude < 2f) {
                anime.SetBool("run", false);
                anime.SetBool("idle", true);
                agent.Warp(goal);
                agent.velocity = Vector3.zero;
                transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            } 
        }
        if ((transform.position-exit).magnitude < 1f) {
            Destroy(this.gameObject);
        }
        if (elapsed_time > anime_time) {
            if (!supe.poseflag) anime.SetBool("idle", false);
        }
        elapsed_time += Time.deltaTime;
    }

    private void getIn() {
        float x = 55f + (25f/(M==1 ? 1 : M-1))*pos_m[1] + pos_i[1]%15;
        float z = 55f + (95f/(M==1 ? 1 : M-1))*pos_m[1] + pos_i[1]/15;
        GetComponent<NavMeshAgent>().destination = new Vector3(x, 0f, z);
    }
    private void stay() {
        float x = 55f + (25f/(M==1 ? 1 : M-1))*pos_m[1] + pos_i[1]%15;
        float z = 55f + (95f/(M==1 ? 1 : M-1))*pos_m[1] + pos_i[1]/15;
        GetComponent<NavMeshAgent>().destination = new Vector3(x, 0f, z);
    }
    private void getOut() {
        GetComponent<NavMeshAgent>().destination = exit;
    }
}
