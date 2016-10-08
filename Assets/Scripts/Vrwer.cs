using UnityEngine;


public unsafe class Vrwer : MediaPlayerCtrl {

	public override void onStart () {
		loadFromSD();
		Debug.Log("asdfds");
	}

	void loadFromSD() {
//		return;
		string strURL = "asdfasdf";
		string sd_path = "file://" + Application.persistentDataPath + "/" + strURL;
		string path = "showreel.mp4";
//		Debug.Log(sd_path);

		Call_Load(path, 0);
	}
}