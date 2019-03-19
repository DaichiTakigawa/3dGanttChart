using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

public class WelcomePageController : MonoBehaviour
{
    public TMP_InputField inputFileTextMesh;
    public TMP_InputField outputFileTextMesh;
    private string inputFilePath;
    private string outputFilePath;
    private string homePath;
    // Start is called before the first frame update
    void Start() {
        homePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
    }

    public void onStartButtonClick() {
        if (inputFilePath == null || outputFilePath == null) return;
        if (inputFilePath == "" || outputFilePath == "") return;
        saveFile();
        EditorUtility.DisplayDialog("", "save file", "ok", "cancel");
        return;
    }

    public void onInputFileTextSelected() {
        inputFilePath = EditorUtility.OpenFilePanel("select input file", homePath, "txt");
        inputFileTextMesh.text = inputFilePath;
        return;
    }
    
    public void onOutputFileTextSelected() {
        outputFilePath = EditorUtility.OpenFilePanel("select output file", homePath, "txt");
        outputFileTextMesh.text = outputFilePath;
        return;
    }

    private void saveFile() {
        if (File.Exists(Application.dataPath + "/input.txt")) {
            File.Delete(Application.dataPath + "/input.txt");
        }
        if (File.Exists(Application.dataPath + "/output.txt")) {
            File.Delete(Application.dataPath + "/output.txt");
        }
        File.Copy(inputFilePath, Application.dataPath + "/input.txt");
        File.Copy(outputFilePath, Application.dataPath + "/output.txt");
    }
}
