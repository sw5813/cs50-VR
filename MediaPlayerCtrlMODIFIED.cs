
using UnityEngine;
using System.Runtime.InteropServices;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

#if !UNITY_WEBPLAYER && !UNITY_WEBGL && !UNITY_WP8 && !UNITY_WP8_1
using FFmpeg.AutoGen;
using System.Threading;
#endif

#if UNITY_EDITOR || UNITY_STANDALONE
public unsafe class MediaPlayerCtrl : MonoBehaviour {
#else
public class MediaPlayerCtrl : MonoBehaviour
{
#endif

	public string m_strFileName;
	public GameObject[] m_TargetMaterial = null;
	private Texture2D m_VideoTexture = null;
	private Texture2D m_VideoTextureDummy = null;
	private MEDIAPLAYER_STATE m_CurrentState;
	private int m_iCurrentSeekPosition;
	private float m_fVolume = 1.0f;
	private int m_iWidth;
	private int m_iHeight;
	private float m_fSpeed = 1.0f;

	public bool m_bFullScreen = false; // Please use only in FullScreen prefab.
	// Using a device support Rochchip or Low-end devices
	// (Reason 1: Not directly play in StreamingAssets)
	// (Reason 2: Video buffer is RGB565 only supported)
	public bool m_bSupportRockchip = true;

	public delegate void VideoEnd();
	public delegate void VideoReady();
	public delegate void VideoError(MEDIAPLAYER_ERROR errorCode, MEDIAPLAYER_ERROR errorCodeExtra);
	public delegate void VideoFirstFrameReady();
	public delegate void VideoResize ();

	public VideoResize OnResize;
	public VideoReady OnReady;
	public VideoEnd OnEnd;
	public VideoError OnVideoError;
	public VideoFirstFrameReady OnVideoFirstFrameReady;

	private IntPtr m_texPtr;

#if UNITY_IPHONE || UNITY_TVOS
	private int m_iPauseFrame;
#endif

#if UNITY_ANDROID && !UNITY_EDITOR && UNITY_5
	[DllImport("BlueDoveMediaRender")]
	private static extern void InitNDK();

#if UNITY_5_2 
	[DllImport("BlueDoveMediaRender")]
	private static extern IntPtr EasyMovieTextureRender();
#endif
#endif

#if (UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN)
	[DllImport("EasyMovieTexture")]
	private static extern void SetTextureFromUnity(System.IntPtr texture, int w, int h,byte[] data);

	[DllImport("EasyMovieTexture")]
	private static extern IntPtr GetRenderEventFunc();
	
	private delegate void DebugCallback(string message);
	
	[DllImport("EasyMovieTexture")]
	private static extern void RegisterDebugCallback(DebugCallback callback);	
	
#else

	#if (UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX)
		[DllImport("EasyMovieTextureRender")]
		private static extern void SetTextureFromUnity(System.IntPtr texture, int w, int h,byte[] data);

		[DllImport("EasyMovieTextureRender")]
		private static extern IntPtr GetRenderEventFunc();

		private delegate void DebugCallback(string message);

		[DllImport("EasyMovieTextureRender")]
		private static extern void RegisterDebugCallback(DebugCallback callback);
	#endif

#endif

	private int m_iAndroidMgrID;
	private bool m_bIsFirstFrameReady;

	public enum MEDIAPLAYER_ERROR
	{
		MEDIA_ERROR_NOT_VALID_FOR_PROGRESSIVE_PLAYBACK = 200,
		MEDIA_ERROR_IO = -1004,
		MEDIA_ERROR_MALFORMED = -1007,
		MEDIA_ERROR_TIMED_OUT = -110,
		MEDIA_ERROR_UNSUPPORTED = -1010,
		MEDIA_ERROR_SERVER_DIED = 100,
		MEDIA_ERROR_UNKNOWN = 1
	}

	public enum MEDIAPLAYER_STATE
	{
		NOT_READY = 0,
		READY = 1,
		END = 2,
		PLAYING = 3,
		PAUSED = 4,
		STOPPED = 5,
		ERROR = 6
	}

	public enum MEDIA_SCALE
	{
		SCALE_X_TO_Y = 0,
		SCALE_X_TO_Z = 1,
		SCALE_Y_TO_X = 2,
		SCALE_Y_TO_Z = 3,
		SCALE_Z_TO_X = 4,
		SCALE_Z_TO_Y = 5,
		SCALE_X_TO_Y_2 = 6,
	}

	bool m_bFirst = false;

	public MEDIA_SCALE m_ScaleValue;
	public GameObject[] m_objResize = null;
	public bool m_bLoop = false;
	public bool m_bAutoPlay = true;
	private bool m_bStop = false;
	public bool m_bInit = false;

	#if !UNITY_WEBPLAYER && !UNITY_WEBGL && !UNITY_WP8 && !UNITY_WP8_1

	public virtual void onStart() { Console.WriteLine("A.G"); }

	static MediaPlayerCtrl()
	{
	#if UNITY_EDITOR
		String currentPath = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Process);

		String dllPath = Environment.CurrentDirectory + Path.DirectorySeparatorChar + "Assets" + Path.DirectorySeparatorChar + "Plugins";
	
		if (currentPath.Contains(dllPath) == false) {
			Environment.SetEnvironmentVariable("PATH", currentPath + Path.PathSeparator + dllPath, EnvironmentVariableTarget.Process);
		}

		dllPath = Environment.CurrentDirectory + Path.DirectorySeparatorChar + "Assets" + Path.DirectorySeparatorChar + "Plugins"  + Path.DirectorySeparatorChar + "x64";

		if (currentPath.Contains(dllPath) == false) {
			Environment.SetEnvironmentVariable("PATH", currentPath + Path.PathSeparator + dllPath, EnvironmentVariableTarget.Process);
		}

		dllPath = Environment.CurrentDirectory + Path.DirectorySeparatorChar + "Assets" + Path.DirectorySeparatorChar + "Plugins"  + Path.DirectorySeparatorChar + "x86";

		if (currentPath.Contains(dllPath) == false) {
			Environment.SetEnvironmentVariable("PATH", currentPath + Path.PathSeparator + dllPath, EnvironmentVariableTarget.Process);
		}
	#endif
	}

	void Awake()
	{
	#if UNITY_STANDALONE
		String currentPath = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Process);

		String dllPath = Application.dataPath + Path.DirectorySeparatorChar + "Plugins";

		if (currentPath.Contains(dllPath) == false) {
			Environment.SetEnvironmentVariable("PATH", currentPath + Path.PathSeparator + dllPath, EnvironmentVariableTarget.Process);
		}
	#endif

		if (SystemInfo.deviceModel.Contains("rockchip")) {
			m_bSupportRockchip = true;
		} else {
			m_bSupportRockchip = false;
		}

#if UNITY_IPHONE || UNITY_TVOS || UNITY_EDITOR || UNITY_STANDALONE
		
		if (m_TargetMaterial!=null) {

			for (int iIndex = 0; iIndex < m_TargetMaterial.Length; iIndex++)
			{
				if (m_TargetMaterial[iIndex] != null) {
					if (m_TargetMaterial[iIndex].GetComponent<MeshFilter>() != null) {
						Vector2 [] vec2UVs= m_TargetMaterial[iIndex].GetComponent<MeshFilter>().mesh.uv;
						
						for (int i = 0; i < vec2UVs.Length; i++)
						{
							vec2UVs[i] = new Vector2(vec2UVs[i].x, 1.0f -vec2UVs[i].y);
						}
						
						m_TargetMaterial[iIndex].GetComponent<MeshFilter>().mesh.uv = vec2UVs;
					}
					
					if (m_TargetMaterial[iIndex].GetComponent<RawImage>() != null) {
						m_TargetMaterial[iIndex].GetComponent<RawImage>().uvRect = new Rect(0,1,1,-1);
					}
				}
			}
		}	
#endif

	}
	void Start()
	{

#if UNITY_STANDALONE || UNITY_EDITOR
		// RegisterDebugCallback(new DebugCallback(DebugMethod));
		// threadVideo = new Thread(ThreadUpdate);
		// threadVideo.Start();
#endif

#if UNITY_ANDROID && !UNITY_EDITOR
		
#if UNITY_5
		if (SystemInfo.graphicsMultiThreaded == true) {
			InitNDK();
		}
#endif
		m_iAndroidMgrID = Call_InitNDK();
#endif
		Call_SetUnityActivity();

#if UNITY_ANDROID
		if (Application.dataPath.Contains(".obb")) {
			Call_SetSplitOBB(true,Application.dataPath);
		} else {
			Call_SetSplitOBB(false, null);
		}
#endif

		m_bInit = true;

	}

	void OnApplicationQuit()
	{
		//if (System.IO.Directory.Exists(Application.persistentDataPath + "/Data") == true)
		//	 System.IO.Directory.Delete(Application.persistentDataPath + "/Data", true);
	}

	bool m_bCheckFBO = false;

	void OnDisable()
	{
		if (GetCurrentState() == MEDIAPLAYER_STATE.PLAYING) {
			Pause();
		}
	}

	void OnEnable()
	{
		if (GetCurrentState() == MEDIAPLAYER_STATE.PAUSED) {
			Play();
		}
	}

	void Update()
	{
		if (string.IsNullOrEmpty(m_strFileName)) {
			return;
		}

		if (checkNewActions) {			
			checkNewActions = false;
			CheckThreading ();
		}

		if (m_bFirst == false) {
			string strName = m_strFileName.Trim();
			// onStart();

			#if UNITY_IPHONE  || UNITY_TVOS || UNITY_ANDROID
			if (strName.StartsWith("http", StringComparison.OrdinalIgnoreCase)) {
				Debug.Log("oiem");
				StartCoroutine(DownloadStreamingVideoAndLoad(strName));
			} else {
				Call_Load(strName,0);
			}
			
			// #elif UNITY_ANDROID 
			
			// if (m_bSupportRockchip)
			// {
			// 	Call_SetRockchip(m_bSupportRockchip);
				
			// 	if (strName.Contains("://"))
			// 	{
			// 		Call_Load(strName,0);
			// 	}
			// 	else
			// 	{
			// 		//Call_Load(strName,0);
			// 		StartCoroutine( CopyStreamingAssetVideoAndLoad(strName));
			// 	}
				
			// }
			// else
			// {
			// 	Call_Load(strName,0);
			// }
			#elif UNITY_STANDALONE
			// if (strName.StartsWith("http", StringComparison.OrdinalIgnoreCase))
			// {
			// 	Debug.Log("oiem");
			// 	StartCoroutine(DownloadStreamingVideoAndLoad(strName));
			// } else
			// {
				Call_Load(strName,0);
			// }
			
			#endif
			
			Call_SetLooping(m_bLoop);
			m_bFirst = true;
		}

		if (m_CurrentState == MEDIAPLAYER_STATE.PLAYING || m_CurrentState == MEDIAPLAYER_STATE.PAUSED) {
			if (m_bCheckFBO == false) {

				if (Call_GetVideoWidth () <= 0 || Call_GetVideoHeight () <= 0) {
					return;
				}

				m_iWidth = Call_GetVideoWidth ();
				m_iHeight = Call_GetVideoHeight ();

				Resize ();

				if (m_VideoTexture != null) {

					//Destroy(m_VideoTexture);

					if (m_VideoTextureDummy != null) {
						Destroy (m_VideoTextureDummy);
						m_VideoTextureDummy = null;
					}

					m_VideoTextureDummy = m_VideoTexture;
					m_VideoTexture = null;

				}

				#if UNITY_ANDROID || UNITY_EDITOR || UNITY_STANDALONE
				if (m_bSupportRockchip) {
					m_VideoTexture = new Texture2D (Call_GetVideoWidth (), Call_GetVideoHeight (), TextureFormat.RGB565, false);
					
				} else {
					m_VideoTexture = new Texture2D (Call_GetVideoWidth (), Call_GetVideoHeight (), TextureFormat.RGBA32, false);
				}
				
				m_VideoTexture.filterMode = FilterMode.Bilinear;
				m_VideoTexture.wrapMode = TextureWrapMode.Clamp;

				m_texPtr = m_VideoTexture.GetNativeTexturePtr ();

				#if UNITY_5_2 || UNITY_5_3 || UNITY_5_4 || UNITY_5_5
				Call_SetUnityTexture((int)m_VideoTexture.GetNativeTexturePtr());
				#else
				Call_SetUnityTexture (m_VideoTexture.GetNativeTextureID ());
				#endif
				#endif
				Call_SetWindowSize ();
				m_bCheckFBO = true;
				if (OnResize != null)
					OnResize ();
			} else {
				if (Call_GetVideoWidth () != m_iWidth || Call_GetVideoHeight () != m_iHeight) {
					m_iWidth = Call_GetVideoWidth ();
					m_iHeight = Call_GetVideoHeight ();

					ResizeTexture ();
				}
			}

			Call_UpdateVideoTexture();

			m_iCurrentSeekPosition = Call_GetSeekPosition();
		}

		if (m_CurrentState != Call_GetStatus()) {

			m_CurrentState = Call_GetStatus();

			if (m_CurrentState == MEDIAPLAYER_STATE.READY) {

				if (OnReady != null)
					OnReady();

				if (m_bAutoPlay)
					Call_Play(0);

				SetVolume(m_fVolume);

			} else if (m_CurrentState == MEDIAPLAYER_STATE.END) {
				if (OnEnd != null)
					OnEnd();

				if (m_bLoop == true) {
					Call_Play(0);
				}
			} else if (m_CurrentState == MEDIAPLAYER_STATE.ERROR) {
				OnError((MEDIAPLAYER_ERROR)Call_GetError(), (MEDIAPLAYER_ERROR)Call_GetErrorExtra());
			}
		}

		GL.InvalidateState ();

	}

	public void ResizeTexture()
	{
		Debug.Log("ResizeTexture " + m_iWidth + " " + m_iHeight);

		if (m_iWidth == 0 || m_iHeight == 0)
			return;
		
		if (m_VideoTexture != null) {

			//Destroy(m_VideoTexture);

			if (m_VideoTextureDummy != null) {
				Destroy (m_VideoTextureDummy);
				m_VideoTextureDummy = null;
			}

			m_VideoTextureDummy = m_VideoTexture;
			m_VideoTexture = null;

		}

		#if UNITY_ANDROID || UNITY_EDITOR || UNITY_STANDALONE
		if (m_bSupportRockchip) {
			m_VideoTexture = new Texture2D (Call_GetVideoWidth (), Call_GetVideoHeight (), TextureFormat.RGB565, false);

		} else {
			m_VideoTexture = new Texture2D (Call_GetVideoWidth (), Call_GetVideoHeight (), TextureFormat.RGBA32, false);
		}

		m_VideoTexture.filterMode = FilterMode.Bilinear;
		m_VideoTexture.wrapMode = TextureWrapMode.Clamp;

		m_texPtr = m_VideoTexture.GetNativeTexturePtr ();

		#if UNITY_5_2 || UNITY_5_3 || UNITY_5_4 || UNITY_5_5
		Call_SetUnityTexture((int)m_VideoTexture.GetNativeTexturePtr());
		#else
		Call_SetUnityTexture (m_VideoTexture.GetNativeTextureID ());
		#endif
		#endif

		Call_SetWindowSize ();
	}

	public void Resize()
	{
		if (m_CurrentState != MEDIAPLAYER_STATE.PLAYING)
			return;

		if (Call_GetVideoWidth() <= 0 || Call_GetVideoHeight() <= 0) {
			return;
		}

		if (m_objResize != null) {
			int iScreenWidth = Screen.width;
			int iScreenHeight = Screen.height;

			float fRatioScreen = (float)iScreenHeight / (float)iScreenWidth;
			int iWidth = Call_GetVideoWidth();
			int iHeight = Call_GetVideoHeight();

			float fRatio = (float)iHeight / (float)iWidth;
			float fRatioResult = fRatioScreen / fRatio;

			for (int i = 0; i < m_objResize.Length; i++)
			{
				if (m_objResize[i] == null)
					continue;

				if (m_bFullScreen) {

					m_objResize[i].transform.localScale = new Vector3(20.0f / fRatioScreen, 20.0f / fRatioScreen, 1.0f);
					if (fRatio < 1.0f) {
						if (fRatioScreen < 1.0f) {
							if (fRatio > fRatioScreen) {
								m_objResize[i].transform.localScale *= fRatioResult;
							}
						}

						m_ScaleValue = MEDIA_SCALE.SCALE_X_TO_Y;
					} else {
						if (fRatioScreen > 1.0f) {
							if (fRatio >= fRatioScreen) {
								
								m_objResize [i].transform.localScale *= fRatioResult;
							}
						} else {
							m_objResize [i].transform.localScale *= fRatioResult;
							
						}

						m_ScaleValue = MEDIA_SCALE.SCALE_X_TO_Y;
					}
				}

				if (m_ScaleValue == MEDIA_SCALE.SCALE_X_TO_Y) {
					m_objResize[i].transform.localScale
						= new Vector3(m_objResize[i].transform.localScale.x
									  , m_objResize[i].transform.localScale.x * fRatio
									  , m_objResize[i].transform.localScale.z);
				} else if (m_ScaleValue == MEDIA_SCALE.SCALE_X_TO_Y_2) {
					m_objResize[i].transform.localScale
						= new Vector3(m_objResize[i].transform.localScale.x
									  , m_objResize[i].transform.localScale.x * fRatio / 2.0f
									  , m_objResize[i].transform.localScale.z);
				} else if (m_ScaleValue == MEDIA_SCALE.SCALE_X_TO_Z) {
					m_objResize[i].transform.localScale
						= new Vector3(m_objResize[i].transform.localScale.x
									  , m_objResize[i].transform.localScale.y
									  , m_objResize[i].transform.localScale.x * fRatio);
				} else if (m_ScaleValue == MEDIA_SCALE.SCALE_Y_TO_X) {
					m_objResize[i].transform.localScale
						= new Vector3(m_objResize[i].transform.localScale.y / fRatio
									  , m_objResize[i].transform.localScale.y
									  , m_objResize[i].transform.localScale.z);
				} else if (m_ScaleValue == MEDIA_SCALE.SCALE_Y_TO_Z) {
					m_objResize[i].transform.localScale
						= new Vector3(m_objResize[i].transform.localScale.x
									  , m_objResize[i].transform.localScale.y
									  , m_objResize[i].transform.localScale.y / fRatio);
				} else if (m_ScaleValue == MEDIA_SCALE.SCALE_Z_TO_X) {
					m_objResize[i].transform.localScale
						= new Vector3(m_objResize[i].transform.localScale.z * fRatio
									  , m_objResize[i].transform.localScale.y
									  , m_objResize[i].transform.localScale.z);
				} else if (m_ScaleValue == MEDIA_SCALE.SCALE_Z_TO_Y) {
					m_objResize[i].transform.localScale
						= new Vector3(m_objResize[i].transform.localScale.x
									  , m_objResize[i].transform.localScale.z * fRatio
									  , m_objResize[i].transform.localScale.z);
				} else {
					m_objResize[i].transform.localScale
						= new Vector3(m_objResize[i].transform.localScale.x, m_objResize[i].transform.localScale.y, m_objResize[i].transform.localScale.z);
				}
			}

		}
	}

	//The error code is the following sites related documents.
	//http://developer.android.com/reference/android/media/MediaPlayer.OnErrorListener.html 
	void OnError(MEDIAPLAYER_ERROR iCode, MEDIAPLAYER_ERROR iCodeExtra)
	{
		string strError = "";

		switch (iCode)
		{
			case MEDIAPLAYER_ERROR.MEDIA_ERROR_NOT_VALID_FOR_PROGRESSIVE_PLAYBACK:
				strError = "MEDIA_ERROR_NOT_VALID_FOR_PROGRESSIVE_PLAYBACK";
				break;
			case MEDIAPLAYER_ERROR.MEDIA_ERROR_SERVER_DIED:
				strError = "MEDIA_ERROR_SERVER_DIED";
				break;
			case MEDIAPLAYER_ERROR.MEDIA_ERROR_UNKNOWN:
				strError = "MEDIA_ERROR_UNKNOWN";
				break;
			default:
				strError = "Unknown error " + iCode;
				break;
		}

		strError += " ";

		switch (iCodeExtra)
		{
			case MEDIAPLAYER_ERROR.MEDIA_ERROR_IO:
				strError += "MEDIA_ERROR_IO";
				break;
			case MEDIAPLAYER_ERROR.MEDIA_ERROR_MALFORMED:
				strError += "MEDIA_ERROR_MALFORMED";
				break;
			case MEDIAPLAYER_ERROR.MEDIA_ERROR_TIMED_OUT:
				strError += "MEDIA_ERROR_TIMED_OUT";
				break;
			case MEDIAPLAYER_ERROR.MEDIA_ERROR_UNSUPPORTED:
				strError += "MEDIA_ERROR_UNSUPPORTED";
				break;
			default:
				strError = "Unknown error " + iCode;
				break;
		}

		Debug.LogError(strError);

		if (OnVideoError != null) {
			OnVideoError(iCode, iCodeExtra);
		}
	}
}
