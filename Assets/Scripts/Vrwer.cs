using UnityEngine;
using System.IO;

public unsafe class Vrwer : MediaPlayerCtrl {

	bool bfirst = true;

	protected override void Update() {
		if (bfirst) {
			bfirst = false;
			Debug.Log("yeah");
			m_bFirst = true;

			loadFromSD();
			base.Play();
			Call_SetLooping(m_bLoop);
		}

		base.Update();
	}

	void loadFromSD() {
		string strURL = "week5_360_TB.mp4";
		string sd_path = "file://" + Application.persistentDataPath + "/" + strURL;
//		string path = "showreel.mp4";
//		Debug.Log(sd_path);

		// Debug.Log("PAPSIAAA");
		// DownloadStreamingVideoAndLoad("http://cdn.cs50.net/2016/fall/lectures/5/week5_360_TB.mp4");
		// Debug.Log("PAPSI222");

		if (File.Exists(sd_path)) {
			Debug.Log("found "+sd_path);
		} else {
			Debug.Log("not found "+sd_path);
		}
		
		Debug.Log("trying "+sd_path);
		try {
			Call_Load(sd_path, 0);
			Debug.Log("Successfully executed Call_Load.");
		} catch (System.Exception e) {
			Debug.Log("Failed to Call_Load.");
			Debug.LogError(e);
		}

	}
}