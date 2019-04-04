using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class EmployeeController : MonoBehaviour
{
    [SerializeField]
    private Animator anime;
    private static GameObject EmployeePrehab;
    private static List<int> dToC;
    private static List<List<int>> mdToC;
    private static int M;
    private static int D;
    private static int MAX_EMP = 0;
    private static float anime_time;
    private const int phase_size = 5;
    private static float[] anime_phase_time = new float[phase_size];
    private static List<EmployeeController> employees;
    private static List<int> preids;
    private static List<int> nxtids;
    private static int lastid;
    private static float start_time = 0f;
    public bool animeFlag = false;
    public enum State {getIn, stay, getOut};
    public int id;
    public int[] pos_m = new int[2];
    public int[] pos_i = new int[2];
    public State animState;
    private int phase = 0;

    public static void init(DataFrame dataFrame, float _anime_time) {
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
        for (int i = 0; i < phase_size; ++i) {
            anime_phase_time[i] = anime_time/phase_size*(i+1);
        }
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
                employees[i].animState = State.getOut;
                employees[i].animeFlag = true;
                employees[i].pos_m[0] = employees[i].pos_m[1];
                employees[i].pos_i[0] = employees[i].pos_i[1];
            } else {
                employees[i].animState = State.stay;
                employees[i].animeFlag = true;
                employees[i].pos_m[0] = employees[i].pos_m[1];
                employees[i].pos_i[0] = employees[i].pos_i[1];
                employees[i].pos_m[1] = idToDestM[employees[i].id];
                employees[i].pos_i[1] = mToI[idToDestM[employees[i].id]]++;
                tmpEmployees.Add(employees[i]);
            }
        }

        for (int i = 0; i < newids.Count; ++i) {
            GameObject go = Instantiate(EmployeePrehab);
            go.name = "employee" + newids[i];
            go.transform.rotation = Quaternion.Euler(0, 90f, 0);
            go.transform.position = new Vector3(0f, 0f, 100f);
            EmployeeController ec = go.GetComponent<EmployeeController>();
            ec.id = newids[i];
            ec.animState = State.getIn;
            ec.animeFlag = true;
            ec.pos_m[1] = idToDestM[newids[i]];
            ec.pos_i[1] = mToI[idToDestM[newids[i]]]++;
        }
        employees = tmpEmployees;
        preids = nxtids;
        start_time = Time.time;
    }

    public void Update() {
        if (animeFlag) {
            if (animState == State.getIn) {
                StartCoroutine("getIn");
            } else if (animState == State.getOut) {
                StartCoroutine("getOut");
            } else {
                StartCoroutine("stay");
            }
        }
    }

    private IEnumerator getIn() {
        animeFlag = false;
        float rot_time = 0f;
        float move_time = 0f;
        Quaternion start_rot = transform.rotation;
        Vector3 start_pos = transform.position;
        Quaternion aim_rot;
        Vector3 aim_pos;
        if (phase == 0) {
            rot_time = 0;
            move_time = anime_phase_time[0];
            aim_rot = transform.rotation;
            aim_pos = transform.position + (new Vector3(5f, 0f, 0f));
        } else if (phase == 1) {
            rot_time = (anime_phase_time[1]-anime_phase_time[0])*0.3f;
            move_time = (anime_phase_time[1]-anime_phase_time[0])*0.7f;
            bool foward = (pos_m[1] >= M/2f);
            aim_rot = Quaternion.Euler(0f, (foward ? 0f : 180f), 0f);
            float z = 50f + (95f/(M == 1 ? 1 : M-1))*pos_m[1];
            aim_pos = new Vector3(transform.position.x, 0f, z+6f);
        } else if (phase == 2) {
            rot_time = (anime_phase_time[2]-anime_phase_time[1])*0.3f;
            move_time = (anime_phase_time[2]-anime_phase_time[1])*0.7f;
            aim_rot = Quaternion.Euler(0f, 90f, 0f);
            float x = 55f + (25f/(M == 1 ? 1 : M-1))*pos_m[1];
            float z = 50f + (95f/(M == 1 ? 1 : M-1))*pos_m[1];
            aim_pos = new Vector3((x+pos_i[1]*0.5f-5f)/2f, 0f, transform.position.z);
        } else if (phase == 3) {
            rot_time = (anime_phase_time[3]-anime_phase_time[2])*0f;
            move_time = (anime_phase_time[3]-anime_phase_time[2]);
            aim_rot = Quaternion.Euler(0f, 90f, 0f);
            float x = 55f + (25f/(M == 1 ? 1 : M-1))*pos_m[1];
            float z = 50f + (95f/(M == 1 ? 1 : M-1))*pos_m[1];
            aim_pos = new Vector3((x+pos_i[1]*0.5f), 0f, transform.position.z);
        } else {
            rot_time = (anime_phase_time[4]-anime_phase_time[4])*0.3f;
            move_time = (anime_phase_time[4]-anime_phase_time[4])*0.7f;
            aim_rot = Quaternion.Euler(0f, 180f, 0f);
            float x = 55f + (25f/(M == 1 ? 1 : M-1))*pos_m[1];
            float z = 50f + (95f/(M == 1 ? 1 : M-1))*pos_m[1];
            aim_pos = new Vector3((x+pos_i[1]*0.5f), 0f, transform.position.z-1);
        }
        while (true) {
            float elapsed_time = Time.time - start_time;
            if (elapsed_time > anime_phase_time[phase]) break;
            if (elapsed_time < anime_phase_time[phase]-move_time) {
                transform.rotation = Quaternion.Lerp(start_rot, aim_rot, 1f-(anime_phase_time[phase]-move_time-elapsed_time)/(rot_time+0.0001f));
            } else {
                transform.position = start_pos + (aim_pos-start_pos)/(move_time+0.0001f)*(move_time-anime_phase_time[phase]+elapsed_time);
            }
            yield return null;
        }
        transform.rotation = aim_rot; transform.position = aim_pos;
        ++phase;
        if (phase != phase_size) animeFlag = true;
        yield break;
    }

    private IEnumerator stay() {
        animeFlag = false;
        float rot_time = 0f;
        float move_time = 0f;
        Quaternion start_rot = transform.rotation;
        Vector3 start_pos = transform.position;
        Quaternion aim_rot;
        Vector3 aim_pos;
        if (phase == 0) {
            if (pos_m[0] == pos_m[1]) {
                rot_time = anime_phase_time[0];
                move_time = 0f;
                bool foward = (pos_i[0] >= pos_i[1]);
                aim_rot = Quaternion.Euler(0f, (foward ? -90f : 90f), 0f);
                aim_pos = transform.position;
            } else {
                rot_time = anime_phase_time[0]*0.4f;
                move_time = anime_phase_time[0]*0.6f;
                aim_rot = Quaternion.Euler(0f, 0f, 0f);
                aim_pos = transform.position + new Vector3(0f, 0f, 1f);
            }
        } else if (phase == 1) {
            if (pos_m[0] == pos_m[1]) {
                rot_time = 0f;
                move_time = (anime_phase_time[1] - anime_phase_time[0]);
                aim_rot = transform.rotation;
                aim_pos = transform.position + new Vector3((pos_i[1]-pos_i[0])*0.1f/3, 0f, 0f);
            } else {
                rot_time = (anime_phase_time[1]-anime_phase_time[0])*0.3f;
                move_time = (anime_phase_time[1]-anime_phase_time[0])*0.7f;
                float x = 20f;
                aim_rot = Quaternion.Euler(0f, -90f, 0f);
                aim_pos = new Vector3(x, 0f, transform.position.z);
            }
        } else if (phase == 2) {
            if (pos_m[0] == pos_m[1]) {
                rot_time = 0f;
                move_time = (anime_phase_time[2] - anime_phase_time[1]);
                aim_rot = transform.rotation;
                aim_pos = transform.position + new Vector3((pos_i[1]-pos_i[0])*0.1f/3, 0f, 0f);
            } else {
                rot_time = (anime_phase_time[2]-anime_phase_time[1])*0.3f;
                move_time = (anime_phase_time[2]-anime_phase_time[1])*0.7f;
                bool foward = (pos_m[1] > pos_m[0]);
                float z = 50f + (95f/(M == 1 ? 1 : M-1))*pos_m[1];
                aim_rot = Quaternion.Euler(0f, (foward ? 0f : 180f), 0f);
                aim_pos = new Vector3(transform.position.x, 0f, z+1f);
            }
        } else if (phase == 3) {
            if (pos_m[0] == pos_m[1]) {
                rot_time = 0f;
                move_time = (anime_phase_time[3] - anime_phase_time[2]);
                aim_rot = transform.rotation;
                float x = 50f + (25f/(M == 1 ? 1 : M-1))*pos_m[1];
                aim_pos = new Vector3(x+pos_i[1]*0.1f, 0f, 0f);
            } else {
                rot_time = (anime_phase_time[3]-anime_phase_time[2])*0.3f;
                move_time = (anime_phase_time[3]-anime_phase_time[2])*0.7f;
                aim_rot = Quaternion.Euler(0f, 90f, 0f);
                float x = 50f + (25f/(M == 1 ? 1 : M-1))*pos_m[1];
                float z = 50f + (95f/(M == 1 ? 1 : M-1))*pos_m[1];
                aim_pos = new Vector3(x+pos_i[1]*0.1f, 0f, z+1f);
            }
        } else {
            if (pos_m[0] == pos_m[1]) {
                rot_time = anime_phase_time[4]-anime_phase_time[3];
                move_time = 0f;
                aim_rot = Quaternion.Euler(0f, 180f, 0f);
                aim_pos = transform.position;
            } else {
                rot_time = (anime_phase_time[4]-anime_phase_time[3])*0.3f;
                move_time = (anime_phase_time[4]-anime_phase_time[3])*0.7f;
                aim_rot = Quaternion.Euler(0f, 90f, 0f);
                float x = 50f + (25f/(M == 1 ? 1 : M-1))*pos_m[1];
                float z = 50f + (95f/(M == 1 ? 1 : M-1))*pos_m[1];
                aim_pos = new Vector3((x+pos_i[1]*0.1f), 0f, transform.position.z-1f);
            }
        }
        while (true) {
            float elapsed_time = Time.time -start_time;
            if (elapsed_time > anime_phase_time[phase]) break;
            if (elapsed_time < anime_phase_time[phase]-move_time) {
                transform.rotation = Quaternion.Lerp(start_rot, aim_rot, 1f-(anime_phase_time[phase]-move_time-elapsed_time)/(rot_time+0.0001f));
            } else {
                transform.position = (aim_pos-start_pos)/(move_time+0.0001f)*(move_time-anime_phase_time[phase]+elapsed_time);
            }
            yield return null;
        }
        transform.rotation = aim_rot; transform.position = aim_pos;
        ++phase;
        if (phase != phase_size) animeFlag = true;
        yield break;
    }

    private IEnumerator getOut() {
        animeFlag = false;
        float rot_time = 0f;
        float move_time = 0f;
        Quaternion start_rot = transform.rotation;
        Vector3 start_pos = transform.position;
        Quaternion aim_rot;
        Vector3 aim_pos;
        if (phase == 0) {
            rot_time = anime_phase_time[0]*0.3f;
            move_time = anime_phase_time[0]*0.7f;
            aim_rot = Quaternion.Euler(0f, 0f, 0f);
            aim_pos = transform.position + new Vector3(0f, 0f, 1f);
        } else if (phase == 1) {
            rot_time = (anime_phase_time[1]-anime_phase_time[0])*0.3f;
            move_time = (anime_phase_time[1]-anime_phase_time[0])*0.7f;
            aim_rot = Quaternion.Euler(0f, 90f, 0f);
            float x = 180f;
            aim_pos = transform.position + new Vector3(x-transform.position.x, 0f, 0f)*0.5f; 
        } else if (phase == 2) {
            rot_time = 0f;
            move_time = (anime_phase_time[2]-anime_phase_time[1]);
            aim_rot = transform.rotation;
            float x = 180f;
            aim_pos = new Vector3(x, 0f, transform.position.z); 
        } else if (phase == 3) {
            rot_time = (anime_phase_time[3]-anime_phase_time[2])*0.3f;
            move_time = (anime_phase_time[3]-anime_phase_time[2])*0.7f;
            bool foward = (pos_m[0] <= M*0.5f);
            aim_rot = Quaternion.Euler(0f, (foward ? 0f : 180f), 0f);
            float x = 180f;
            aim_pos = new Vector3(x, 0, 100f);
        } else {
            rot_time = (anime_phase_time[4]-anime_phase_time[4])*0.3f;
            move_time = (anime_phase_time[4]-anime_phase_time[4])*0.7f;
            aim_rot = Quaternion.Euler(0f, 90f, 0f);
            aim_pos = new Vector3(200f, 0f, 100f);
        }
        while (true) {
            float elapsed_time = Time.time - start_time;
            if (elapsed_time > anime_phase_time[phase]) break;
            if (elapsed_time < anime_phase_time[phase]-move_time) {
                transform.rotation = Quaternion.Lerp(start_rot, aim_rot, 1f-(anime_phase_time[phase]-move_time-elapsed_time)/(rot_time+0.0001f));
            } else {
                transform.position = (aim_pos-start_pos)/(move_time+0.0001f)*(move_time-anime_phase_time[phase]+elapsed_time);
            }
            yield return null;
        }
        transform.rotation = aim_rot; transform.position = aim_pos;
        ++phase;
        if (phase != phase_size) animeFlag = true;
        if (phase == phase_size) Destroy(this.gameObject);
        yield break;
    }

}