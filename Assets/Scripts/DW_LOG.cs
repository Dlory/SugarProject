using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public static class DebugExtensions
{
	public static void LogGUI(this Object obj, string text) {
		Camera camera = GameObject.FindObjectOfType<Camera> ();
		DW_LOG logScript = camera.GetComponent<DW_LOG> ();
		if (logScript != null) {
			logScript.LogText = text;
		}
	}
}

public class DW_LOG : MonoBehaviour {
    public Rect startRect = new Rect(25, 25, 85, 40); // The rect the window is initially displayed at.
    private GUIStyle style; // The style the text will be displayed at, based en defaultSkin.label.
    private Rect realRect;
	public string LogText;

    private void OnGUI() {
        // Copy the default label skin, change the color and the alignement
        if (style == null) {
            style = new GUIStyle(GUI.skin.label);
            style.normal.textColor = Color.white;
            style.alignment = TextAnchor.MiddleCenter;
        }

		GUI.color = Color.white;
		realRect = GUI.Window(998, startRect, DoMyWindow, "");
    }

    private void DoMyWindow(int windowID) {
		GUI.Label(new Rect(0, 0, realRect.width, realRect.height), LogText , style);
    }
}