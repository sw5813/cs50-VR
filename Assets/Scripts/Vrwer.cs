using UnityEngine;


public unsafe class Vrwer : MediaPlayerCtrl {

	bool bfirst = true;

	// void Start() {
	// 	// m_bFirst = false;
	// }

	protected override void Update() {
		if (bfirst) {
			bfirst = false;
			Debug.Log("yeah");
			m_bFirst = true;

			loadFromSD();
			base.Play();
			Call_SetLooping(m_bLoop);
		}

		Debug.Log("Update child");

		base.Update();
	}

	void loadFromSD() {
//		return;
		string strURL = "week5_360_TB.mp4";
		string sd_path = "file://" + Application.persistentDataPath + "/" + strURL;
//		string path = "showreel.mp4";
//		Debug.Log(sd_path);

		Call_Load(sd_path, 0);
	}
}