using UnityEngine;
using System.Collections;
using System;
using UnityEngine;
using VRStandardAssets.Utils;
using UnityEngine.SceneManagement;


public class PlayPause : MonoBehaviour {

	public event Action<PlayPause> OnButtonSelected;                   // This event is triggered when the selection of the button has finished.
	public MediaPlayerCtrl scrMedia;

	private bool paused = false;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetButtonDown ("Fire1")) {
			if (!paused) {
				// pause the video
				scrMedia.Pause ();
			} else {
				scrMedia.Play ();
			}
			paused = !paused;
		}
		if (Input.GetKeyDown (KeyCode.Escape)) {
			SceneManager.LoadScene ("MainMenu", LoadSceneMode.Single);
		}
	}


}
