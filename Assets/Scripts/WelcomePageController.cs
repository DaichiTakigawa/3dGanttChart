using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WelcomePageController : MonoBehaviour
{
    public TMP_InputField inputFileTextMesh;
    public TMP_InputField outputFileTextMesh;
    private string input;
    private string output;

    public void onStartButtonClick() {
        if (input == null || output == null) return;
        if (input == "" || output == "") return;
        saveFile();
        return;
    }

    public void onInputFeildDeselect() {
        input = inputFileTextMesh.text;
        return;
    }

    public void onOutputFieldDeselect() {
        output = outputFileTextMesh.text;
        return;
    }

    private void saveFile() {
        if (File.Exists(Application.dataPath + "/input.txt")) {
            File.Delete(Application.dataPath + "/input.txt");
        }
        if (File.Exists(Application.dataPath + "/output.txt")) {
            File.Delete(Application.dataPath + "/output.txt");
        }

        StreamWriter sw = new StreamWriter(Application.dataPath + "/input.txt");
        sw.Write(input);
        sw.Close();
        sw = new StreamWriter(Application.dataPath + "/output.txt");
        sw.Write(output);
        sw.Close();
    }
}
