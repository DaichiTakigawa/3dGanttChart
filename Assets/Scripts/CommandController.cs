using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CommandController : MonoBehaviour
{
    [SerializeField]
    private Text text; 
    [SerializeField]
    private Image plane;
    [SerializeField]
    private Supervisor supe;
    private bool commandMode = false;
    private bool searchFlag = false;
    private GameObject searchObject;
    private string tmpCommand;
    private Camera mainCamea;

    void Start() {
        plane.color = new Color(0f, 0f, 0f, 0f);
        mainCamea = Camera.main;
    }

    void Update() {
        if (Input.inputString == ":") {
            commandMode = true;
            text.text = ":";
            plane.color = new Color(0f, 0f, 0f, 1f);
        }
        if (commandMode && Input.GetKeyDown(KeyCode.Escape)) {
            commandMode = false;
            text.text = "";
            tmpCommand = "";
            plane.color = new Color(0f, 0f, 0f, 0f);
        }

        if (commandMode && Input.anyKeyDown) {
            string str = Input.inputString;
            bool judge = true;
            foreach (char c in str) {
                if (!char.IsDigit(c) && !('a' <= c && c <= 'z') && c != ' ' && c != '.') {
                    judge = false;
                }
            }
            if (judge) {
                tmpCommand += str;
                text.text = ":" + tmpCommand;
            }
            
        }
        
        if (commandMode &&  Input.GetKeyDown(KeyCode.Backspace)) {
            tmpCommand = tmpCommand.Substring(0, (tmpCommand.Length-1 >= 0 ? tmpCommand.Length-1 : 0));
            text.text = ":" + tmpCommand;
        }

        if (commandMode && Input.GetKeyDown(KeyCode.Return)) {
            string[] strs = tmpCommand.Split(' ');
            tmpCommand = "";
            text.text = "";
            if (strs[0] == "s" || strs[0] == "start") {
                supe.poseflag = false;
            } else if (strs[0] == "p" || strs[0] == "pose") {
                supe.poseflag = true;
            } else if (strs[0] == "time") {
                float d;
                if (float.TryParse(strs[1], out d)) {
                    supe.timeSpeed = supe.defaultTimeSpeed * d;
                } else {
                    text.text = "unexpedted command: " + strs[0] + " " + strs[1];
                }
            } else if (strs[0] == "camera") {
                if (strs[1] == "main") {
                    searchFlag = false;
                } else {
                    if (strs.Length <= 1) {
                    text.text = "unexpedted command: " + strs[0];
                    } else {
                        searchObject = GameObject.Find(strs[1]);
                        if (searchObject == null) {
                            text.text = "not here";
                        } else {
                            searchFlag = true;
                        }
                    }
                }
            } else {
                text.text = "unexpedted command: " + strs[0];
            }
        }

        if (searchFlag && searchObject != null) {
            if (searchObject.name.Substring(0, 5) == "order") {
                mainCamea.transform.position = searchObject.transform.position + new Vector3(-2f, 5f, -5f);
                mainCamea.transform.LookAt(searchObject.transform, new Vector3(0f, 1f, 0f));
            } else if (searchObject.name.Substring(0, 7) == "machine") {
                mainCamea.transform.position = searchObject.transform.position + new Vector3(12f, 10f, -12f);
                mainCamea.transform.LookAt(searchObject.transform.position + new Vector3(12f, 0f, 0f), new Vector3(0f, 1f, 0f));
            } else {
                mainCamea.transform.position = searchObject.transform.position + new Vector3(0f, 4f, -5f);
                mainCamea.transform.LookAt(searchObject.transform, new Vector3(0f, 1f, 0f));
            }
        }
    }
}
