using UnityEngine;


public unsafe class Vrwer : MediaPlayerCtrl {

	bool bfirst = true;

	void Start() {
		m_bFirst = true;
	}

	void Update() {
		if (bfirst) {
			bfirst = false;
			Debug.Log("yeah");

			loadFromSD();
            Call_SetLooping(m_bLoop);
		}

		// if (true) {
			base.Update();
			base.Play();
		// }

		// if (string.IsNullOrEmpty(m_strFileName))
		// {
		// 	return;
		// }

		// if (checkNewActions) {
		// 	checkNewActions = false;
		// 	CheckThreading();
		// }

		// if (false)
		// {

		// 	string strName = m_strFileName.Trim();

		// 	#if UNITY_IPHONE  || UNITY_TVOS
		// 	/*if (strName.StartsWith("http",StringComparison.OrdinalIgnoreCase))
		// 	{
		// 		StartCoroutine( DownloadStreamingVideoAndLoad(strName) );
		// 	}
		// 	else*/
		// 	{
		// 		Call_Load(strName,0);
		// 	}
			
		// 	#elif UNITY_ANDROID 
		// 	if (m_bSupportRockchip)
		// 	{
		// 		Call_SetRockchip(m_bSupportRockchip);
				
		// 		if (strName.Contains("://")) {
		// 			Call_Load(strName,0);
		// 		} else {
		// 			//Call_Load(strName,0);
		// 			StartCoroutine( CopyStreamingAssetVideoAndLoad(strName));
		// 		}
		// 	} else {
		// 		Call_Load(strName,0);
		// 	}
		// 	#elif UNITY_STANDALONE
		// 	Call_Load(strName,0);
		// 	#endif
			
		// 	Call_SetLooping(m_bLoop);
		// 	m_bFirst = true;
		// }


		return;

		// if (m_CurrentState == MEDIAPLAYER_STATE.PLAYING ||
		// 	m_CurrentState == MEDIAPLAYER_STATE.PAUSED) {
		// 	if (m_bCheckFBO == false) {
		// 		if (Call_GetVideoWidth () <= 0 || Call_GetVideoHeight () <= 0) {
		// 			return;
		// 		}

		// 		m_iWidth = Call_GetVideoWidth ();
		// 		m_iHeight = Call_GetVideoHeight ();

		// 		Resize();

		// 		if (m_VideoTexture != null) {
		// 			//Destroy(m_VideoTexture);
		// 			if (m_VideoTextureDummy != null) {
		// 				Destroy (m_VideoTextureDummy);
		// 				m_VideoTextureDummy = null;
		// 			}

		// 			m_VideoTextureDummy = m_VideoTexture;
		// 			m_VideoTexture = null;
		// 		}

		// 		#if UNITY_ANDROID || UNITY_EDITOR || UNITY_STANDALONE
		// 		if (m_bSupportRockchip) {
		// 			m_VideoTexture = new Texture2D (Call_GetVideoWidth (), Call_GetVideoHeight (), TextureFormat.RGB565, false);
					
		// 		} else {
		// 			m_VideoTexture = new Texture2D (Call_GetVideoWidth (), Call_GetVideoHeight (), TextureFormat.RGBA32, false);
		// 		}
				
		// 		m_VideoTexture.filterMode = FilterMode.Bilinear;
		// 		m_VideoTexture.wrapMode = TextureWrapMode.Clamp;

		// 		m_texPtr = m_VideoTexture.GetNativeTexturePtr ();

		// 		#if UNITY_5_2 || UNITY_5_3 || UNITY_5_4 || UNITY_5_5
		// 		Call_SetUnityTexture((int)m_VideoTexture.GetNativeTexturePtr());
		// 		#else
		// 		Call_SetUnityTexture (m_VideoTexture.GetNativeTextureID ());
		// 		#endif
		// 		#endif
		// 		Call_SetWindowSize ();
		// 		m_bCheckFBO = true;
		// 		if (OnResize != null)
		// 			OnResize ();
		// 	} else {
		// 		if (Call_GetVideoWidth () != m_iWidth || Call_GetVideoHeight () != m_iHeight) {
		// 			m_iWidth = Call_GetVideoWidth ();
		// 			m_iHeight = Call_GetVideoHeight ();

		// 			ResizeTexture ();
		// 		}
		// 	}

		// 	Call_UpdateVideoTexture();

		// 	m_iCurrentSeekPosition = Call_GetSeekPosition();
		// }

		// if (m_CurrentState != Call_GetStatus()) {
		// 	m_CurrentState = Call_GetStatus();

		// 	if (m_CurrentState == MEDIAPLAYER_STATE.READY) {

		// 		if (OnReady != null)
		// 			OnReady();

		// 		if (m_bAutoPlay)
		// 			Call_Play(0);

		// 		SetVolume(m_fVolume);
		// 	} else if (m_CurrentState == MEDIAPLAYER_STATE.END) {
		// 		if (OnEnd != null) {
		// 			OnEnd();
		// 		}

		// 		if (m_bLoop == true)
		// 		{
		// 			Call_Play(0);
		// 		}
		// 	}
		// 	else if (m_CurrentState == MEDIAPLAYER_STATE.ERROR)
		// 	{
		// 		OnError((MEDIAPLAYER_ERROR)Call_GetError(), (MEDIAPLAYER_ERROR)Call_GetErrorExtra());
		// 	}
		// }

		// GL.InvalidateState();

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