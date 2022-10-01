﻿using HarmonyLib;
using SimpleJSON;
using System.Collections;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Permissions;
using TrombLoader.Data;
using TrombLoader.Helpers;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.PostProcessing;

[assembly: SecurityPermission(System.Security.Permissions.SecurityAction.RequestMinimum, SkipVerification = true)]

namespace TrombLoader.Class_Patches
{
    [HarmonyPatch(typeof(GameController))]
	[HarmonyPatch("Start")] // if possible use nameof() here
	class GameControllerStartPatch
	{
		//rewrite of the original
		static bool Prefix(GameController __instance)
		{
			//.... Dont even ask why
			__instance.toot_keys.Add(KeyCode.Space);
			__instance.toot_keys.Add(KeyCode.A);
			__instance.toot_keys.Add(KeyCode.B);
			__instance.toot_keys.Add(KeyCode.C);
			__instance.toot_keys.Add(KeyCode.D);
			__instance.toot_keys.Add(KeyCode.E);
			__instance.toot_keys.Add(KeyCode.F);
			__instance.toot_keys.Add(KeyCode.G);
			__instance.toot_keys.Add(KeyCode.H);
			__instance.toot_keys.Add(KeyCode.I);
			__instance.toot_keys.Add(KeyCode.J);
			__instance.toot_keys.Add(KeyCode.K);
			__instance.toot_keys.Add(KeyCode.L);
			__instance.toot_keys.Add(KeyCode.M);
			__instance.toot_keys.Add(KeyCode.N);
			__instance.toot_keys.Add(KeyCode.O);
			__instance.toot_keys.Add(KeyCode.P);
			__instance.toot_keys.Add(KeyCode.Q);
			__instance.toot_keys.Add(KeyCode.R);
			__instance.toot_keys.Add(KeyCode.S);
			__instance.toot_keys.Add(KeyCode.T);
			__instance.toot_keys.Add(KeyCode.U);
			__instance.toot_keys.Add(KeyCode.V);
			__instance.toot_keys.Add(KeyCode.W);
			__instance.toot_keys.Add(KeyCode.X);
			__instance.toot_keys.Add(KeyCode.Y);
			__instance.toot_keys.Add(KeyCode.Z);
			
			__instance.latency_offset = (float)GlobalVariables.localsettings.latencyadjust * 0.001f;

			Debug.Log("latency_offset: " + __instance.latency_offset);
			Application.targetFrameRate = 144;
			Cursor.lockState = CursorLockMode.Confined;
			__instance.retrying = false;
			__instance.notescoreaverage = -1f;
			__instance.notescoresamples = 1f;
			__instance.curtains.SetActive(true);
			__instance.curtainc = __instance.curtains.GetComponent<CurtainController>();
			__instance.level_finshed = false;
			__instance.scores_A = 0;
			__instance.scores_B = 0;
			__instance.scores_C = 0;
			__instance.scores_D = 0;
			__instance.scores_F = 0;

			if (GlobalVariables.scene_destination == "freeplay")
			{
				__instance.freeplay = true;
				__instance.backbtn.SetActive(true);
				__instance.champcontroller.hideChampText();
			}
			else
			{
				__instance.backbtn.SetActive(false);
			}
			__instance.puppetnum = GlobalVariables.chosen_character;
			__instance.textureindex = GlobalVariables.chosen_trombone;
			__instance.levelnum = GlobalVariables.chosen_track_index;
			__instance.soundset = GlobalVariables.chosen_soundset;
			__instance.popuptextobj.transform.localScale = new Vector3(0f, 1f, 1f);
			__instance.multtextobj.transform.localScale = new Vector3(0f, 1f, 1f);
			__instance.ui_savebtn.onClick.AddListener(new UnityAction(__instance.tryToSaveLevel));
			__instance.ui_loadbtn.onClick.AddListener(new UnityAction(__instance.loadFromEditor));
			BloomModel.Settings settings = __instance.gameplayppp.bloom.settings;
			settings.bloom.intensity = 0f;
			__instance.gameplayppp.bloom.settings = settings;
			if (!__instance.freeplay)
			{
				__instance.songtitle.text = GlobalVariables.data_tracktitles[__instance.levelnum][0];
				__instance.songtitleshadow.text = GlobalVariables.data_tracktitles[__instance.levelnum][0];
			}
			else if (__instance.freeplay)
			{
				__instance.songtitle.text = "";
				__instance.songtitleshadow.text = "";
				__instance.ui_score.text = "";
				__instance.ui_score_shadow.text = "";
				__instance.highestcombo.text = "";
				__instance.highestcomboshad.text = "";
			}
			string baseGameChartPath = "/StreamingAssets/trackassets/";
			string trackReference = GlobalVariables.data_trackrefs[__instance.levelnum];
			string customTrackReference = trackReference;
			bool isCustomTrack = false;
			if (!__instance.freeplay)
			{
				baseGameChartPath += trackReference;
			}
			else if (__instance.freeplay)
			{
				baseGameChartPath += "freeplay";
			}
			if (!File.Exists(Application.dataPath + baseGameChartPath))
			{
				Plugin.LogDebug("Nyx: Cant load asset bundle, must be a custom song, hijacking Ball game!");
				baseGameChartPath = "/StreamingAssets/trackassets/ballgame";
				trackReference = "ballgame";
				isCustomTrack = true;
			}
			__instance.myLoadedAssetBundle = AssetBundle.LoadFromFile(Application.dataPath + baseGameChartPath);
			if (__instance.myLoadedAssetBundle == null)
			{
				Plugin.LogDebug("Failed to load AssetBundle!");
				return false;
			}
			Plugin.LogDebug("LOADED ASSETBUNDLE: " + Application.dataPath + baseGameChartPath);
			if (!__instance.freeplay)
			{
				AudioSource component = __instance.myLoadedAssetBundle.LoadAsset<GameObject>("music_" + trackReference).GetComponent<AudioSource>();
				__instance.musictrack.clip = component.clip;
				__instance.musictrack.volume = component.volume;
				if (isCustomTrack)
				{
					Plugin.LogDebug("Nyx: Trying to load ogg from file!");

					var songPath = Globals.GetCustomSongsPath() + customTrackReference + "/song.ogg";
					IEnumerator e = Plugin.Instance.GetAudioClipSync(songPath);

					//Worst piece of code I have ever seen, but it does the job, I guess
					//Unity has forced my hand once again
					//Forces a coroutine to be held manually, basically removing the point of it being a coroutine
					while (e.MoveNext())
					{
						if (e.Current != null)
						{
							if (e.Current is string)
							{
								Plugin.LogError("Couldnt Load OGG FILE!!");
							}
							else
							{
								__instance.musictrack.clip = e.Current as AudioClip;
							}
						}
					}

					//AudioClip clip = WavUtility.ToAudioClip(Globals.GetCustomSongsPath() + customTrackReference + "/song.wav");
					//Debug.Log(__instance.musictrack.clip == null);

				}
			}
			__instance.StartCoroutine(__instance.loadAssetBundleResources());
			__instance.bgcontroller.songname = customTrackReference;
			GameObject gameObject = new GameObject();
			if (!__instance.freeplay)
			{
				gameObject = __instance.myLoadedAssetBundle.LoadAsset<GameObject>("BGCam_" + trackReference);

                if (isCustomTrack)
                {
					__instance.bgcontroller.tickontempo = false;

					if (File.Exists(Globals.GetCustomSongsPath() + customTrackReference + "/bg.trombackground"))
                    {
						var gameObjectOld = gameObject;
						gameObject = AssetBundleHelper.LoadObjectFromAssetBundlePath<GameObject>(Globals.GetCustomSongsPath() + customTrackReference + "/bg.trombackground");
						UnityEngine.Object.DontDestroyOnLoad(gameObject);

						// very scuffed and temporary, this could probably be completely done on export

						// handle foreground objects
						while (gameObject.transform.GetChild(1).childCount < 8)
						{
							var fillerObject = new GameObject("Filler");
							fillerObject.transform.SetParent(gameObject.transform.GetChild(1));
						}

						// handle two background images
						while (gameObject.transform.GetChild(0).GetComponentsInChildren<SpriteRenderer>().Length < 2)
						{
							var fillerObject = new GameObject("Filler");
							fillerObject.AddComponent<SpriteRenderer>();
							fillerObject.transform.SetParent(gameObject.transform.GetChild(0));
						}

						// move confetti
						gameObjectOld.transform.GetChild(2).SetParent(gameObject.transform);

						var managers = gameObject.GetComponentsInChildren<TromboneEventManager>();
						foreach (var manager in managers) manager.DeserializeAllGenericEvents();

						var invoker = gameObject.AddComponent<TromboneEventInvoker>();
						invoker.InitializeInvoker(__instance, managers);
						UnityEngine.Object.DontDestroyOnLoad(invoker);
					}
				}
			}
			else if (__instance.freeplay)
			{
				gameObject = __instance.myLoadedAssetBundle.LoadAsset<GameObject>("BGCam_freeplay");
			}
			if (gameObject != null)
			{
				GameObject gameObject2 = UnityEngine.Object.Instantiate<GameObject>(gameObject, new Vector3(0f, 0f, 0f), Quaternion.identity, __instance.bgholder.transform);
				gameObject2.transform.localPosition = new Vector3(0f, 0f, 0f);
				__instance.bgcontroller.fullbgobject = gameObject2;
			}
			if (__instance.soundset > 0)
			{
				__instance.currentnotesound.volume = 0.25f;
			}
			if (__instance.soundset == 4)
			{
				__instance.currentnotesound.volume = 0.75f;
			}
			string[] array = new string[]
			{
			"default",
			"slidewhistle",
			"eightbit",
			"club",
			"fart"
			};
			__instance.mySoundAssetBundle = AssetBundle.LoadFromFile(Application.dataPath + "/StreamingAssets/soundpacks/soundpack" + array[__instance.soundset]);
			if (__instance.mySoundAssetBundle == null)
			{
				Plugin.LogDebug("Failed to load sound pack AssetBundle!");
				return false;
			}
			Plugin.LogDebug("LOADED <<<sound pack>>> ASSETBUNDLE");
			UnityEngine.Object.Instantiate<GameObject>(__instance.mySoundAssetBundle.LoadAsset<GameObject>("soundpack" + array[__instance.soundset]), new Vector3(0f, 0f, 0f), Quaternion.identity, __instance.soundSets.transform);
			__instance.StartCoroutine(__instance.loadSoundBundleResources());
			__instance.puppet_human = UnityEngine.Object.Instantiate<GameObject>(__instance.playermodels[__instance.puppetnum], new Vector3(0f, 0f, 0f), Quaternion.identity, __instance.modelparent.transform);
			__instance.puppet_human.transform.localPosition = new Vector3(0.7f, -0.38f, 1.3f);
			if (!__instance.leveleditor && !__instance.freeplay)
			{
				LeanTween.scaleY(__instance.puppet_human, 0.01f, 0.01f);
			}
			if (__instance.freeplay)
			{
				LeanTween.moveLocalX(__instance.puppet_human, 0.4f, 0.01f);
			}
			__instance.puppet_humanc = __instance.puppet_human.GetComponent<HumanPuppetController>();
			if (GlobalVariables.localsave.cardcollectionstatus[36] > 9)
			{
				__instance.puppet_humanc.show_rainbow = true;
			}
			__instance.puppet_humanc.setTromboneTex(__instance.textureindex);
			__instance.topbreathr = __instance.topbreath.GetComponent<RectTransform>();
			__instance.bottombreathr = __instance.bottombreath.GetComponent<RectTransform>();
			__instance.noteholderr = __instance.noteholder.GetComponent<RectTransform>();
			__instance.lyricsholderr = __instance.lyricsholder.GetComponent<RectTransform>();
			__instance.noteparticlesrect = __instance.noteparticles.transform.GetComponent<RectTransform>();
			__instance.leftboundsglow.transform.localScale = new Vector3(0.01f, 1f, 1f);
			for (int i = 0; i < 15; i++)
			{
				__instance.notelines[i] = __instance.notelinesholder.transform.GetChild(i).gameObject;
			}
			for (int j = 0; j < 8; j++)
			{
				GameObject gameObject3 = __instance.notelines[j].gameObject;
				float num = __instance.vbounds / 12f;
				if (j == 0)
				{
					gameObject3.transform.localPosition = new Vector3(0f, __instance.vbounds, 0f);
				}
				else if (j == 1)
				{
					gameObject3.transform.localPosition = new Vector3(0f, __instance.vbounds - num, 0f);
				}
				else if (j == 2)
				{
					gameObject3.transform.localPosition = new Vector3(0f, __instance.vbounds - num * 3f, 0f);
				}
				else if (j == 3)
				{
					gameObject3.transform.localPosition = new Vector3(0f, __instance.vbounds - num * 5f, 0f);
				}
				else if (j == 4)
				{
					gameObject3.transform.localPosition = new Vector3(0f, __instance.vbounds - num * 7f, 0f);
				}
				else if (j == 5)
				{
					gameObject3.transform.localPosition = new Vector3(0f, __instance.vbounds - num * 8f, 0f);
				}
				else if (j == 6)
				{
					gameObject3.transform.localPosition = new Vector3(0f, __instance.vbounds - num * 10f, 0f);
				}
				else if (j == 7)
				{
					gameObject3.transform.localPosition = new Vector3(0f, __instance.vbounds - num * 12f, 0f);
				}
			}
			for (int k = 0; k < 7; k++)
			{
				GameObject gameObject4 = __instance.notelines[k + 8].gameObject;
				float num2 = __instance.vbounds / 12f;
				if (k == 0)
				{
					gameObject4.transform.localPosition = new Vector3(0f, -num2, 0f);
				}
				else if (k == 1)
				{
					gameObject4.transform.localPosition = new Vector3(0f, num2 * -3f, 0f);
				}
				else if (k == 2)
				{
					gameObject4.transform.localPosition = new Vector3(0f, num2 * -5f, 0f);
				}
				else if (k == 3)
				{
					gameObject4.transform.localPosition = new Vector3(0f, num2 * -7f, 0f);
				}
				else if (k == 4)
				{
					gameObject4.transform.localPosition = new Vector3(0f, num2 * -8f, 0f);
				}
				else if (k == 5)
				{
					gameObject4.transform.localPosition = new Vector3(0f, num2 * -10f, 0f);
				}
				else if (k == 6)
				{
					gameObject4.transform.localPosition = new Vector3(0f, num2 * -12f, 0f);
				}
			}
			for (int l = 0; l < 15; l++)
			{
				__instance.notelinepos[l] = __instance.notelines[l].gameObject.transform.localPosition.y;
				__instance.notelines[l] = null;
				UnityEngine.Object.Destroy(__instance.notelines[l]);
			}
			__instance.pointerrect = __instance.pointer.GetComponent<RectTransform>();
			__instance.pointerrect.anchoredPosition3D = new Vector3(__instance.zeroxpos - (float)__instance.dotsize * 0.5f, 0f, 0f);
			__instance.noteparticlesrect.anchoredPosition3D = new Vector3(__instance.zeroxpos, 0f, 0f);
			__instance.leftbounds.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(__instance.zeroxpos, 60f, 0f);
			if (!__instance.leveleditor && !__instance.freeplay)
			{
				__instance.buildLevel(__instance.levelnum);
				__instance.trackmovemult = __instance.tempo / 60f * (float)__instance.defaultnotelength;
				float num3 = __instance.zeroxpos - __instance.noteoffset * -__instance.trackmovemult;
				LeanTween.value(num3 + 1000f, num3, 1.5f).setEaseInOutQuad().setOnUpdate(delegate (float val)
				{
					__instance.noteholderr.anchoredPosition3D = new Vector3(val, 0f, 0f);
					__instance.lyricsholderr.anchoredPosition3D = new Vector3(val, 0f, 0f);
				});
			}
			if (!__instance.leveleditor)
			{
				__instance.editorcanvas.SetActive(false);
			}
			else if (__instance.leveleditor)
			{
				__instance.readytoplay = true;
				__instance.editorcanvas.SetActive(true);
				__instance.buildEditorGUI();
			}
			if (__instance.freeplay)
			{
				__instance.editorcanvas.SetActive(false);
				__instance.healthobj.SetActive(false);
			}
			__instance.pointer.transform.SetAsLastSibling();
			if (!__instance.freeplay && !__instance.leveleditor)
			{
				__instance.musictrack = __instance.musicref.GetComponent<AudioSource>();
				__instance.startSong();
				return false;
			}
			if (__instance.freeplay)
			{
				__instance.tempo = 40f;
				__instance.Invoke("startDance", 1f);
			}

			return false;
		}

		static void Postfix(GameController __instance)
		{
			
		}

	}

	[HarmonyPatch(typeof(GameController))]
	[HarmonyPatch("tryToLoadLevel")] // if possible use nameof() here
	class GameControllerTryToLoadLevelPatch
	{
		//rewrite of the original
		static bool Prefix(GameController __instance, ref string filename)
		{
			bool isCustomTrack = false;
			string customChartPath = Globals.GetCustomSongsPath() + filename + "/song.tmb";
			string baseChartName;
			if (filename == "EDITOR")
			{
				baseChartName = Application.streamingAssetsPath + "/leveldata/" + __instance.levelnamefield.text + ".tmb";
			}
			else
			{
				baseChartName = Application.streamingAssetsPath + "/leveldata/" + filename + ".tmb";
			}
			if (!File.Exists(baseChartName))
			{
				Plugin.LogError("File doesnt exist!! Try to load custom song, hijacking Ballgame!!!!!");
				baseChartName = Application.streamingAssetsPath + "/leveldata/ballgame.tmb";
				Plugin.LogDebug("Loading Chart:" + baseChartName);
				Plugin.LogDebug("NYX: HERE WE HOOK OUR CUSTOM CHART!!!!!!!!!!!");
				isCustomTrack = true;
			}
			if (File.Exists(baseChartName))
			{
				Plugin.LogDebug("found level");
				BinaryFormatter binaryFormatter = new BinaryFormatter();
				FileStream fileStream = File.Open(baseChartName, FileMode.Open);
				SavedLevel savedLevel = (SavedLevel)binaryFormatter.Deserialize(fileStream);
				fileStream.Close();
				if (!isCustomTrack)
				{
					Plugin.LogDebug("NYX: Printing Ingame Chart!!!!");
					//Plugin.LogDebug(savedLevel.Serialize().ToString());
				}

				CustomSavedLevel customLevel = new CustomSavedLevel(savedLevel);
				if (isCustomTrack)
				{
					Plugin.LogDebug("Loading Chart from:" + customChartPath);
					string jsonString = File.ReadAllText(customChartPath);
					var jsonObject = JSON.Parse(jsonString);
					Plugin.LogDebug(jsonObject.ToString());
					customLevel.Deserialize(jsonObject);
				}
				__instance.bgdata.Clear();
				__instance.bgdata = customLevel.bgdata;
				__instance.leveldata.Clear();
				__instance.leveldata = customLevel.savedleveldata;
				__instance.lyricdata_pos = customLevel.lyricspos;
				__instance.lyricdata_txt = customLevel.lyricstxt;

				//Debug.Log("Nyx: Serialize Custom level to get lyrics");
				//File.WriteAllText(customChartPath+".withlyrics", customLevel.Serialize().ToString());

				if (customLevel.note_color_start == null)
				{
					Plugin.LogDebug("no color data :-(");
				}
				else
				{
					__instance.note_c_start = customLevel.note_color_start;
					__instance.note_c_end = customLevel.note_color_end;
					__instance.col_r_1.text = __instance.note_c_start[0].ToString();
					__instance.col_g_1.text = __instance.note_c_start[1].ToString();
					__instance.col_b_1.text = __instance.note_c_start[2].ToString();
					__instance.col_r_2.text = __instance.note_c_end[0].ToString();
					__instance.col_g_2.text = __instance.note_c_end[1].ToString();
					__instance.col_b_2.text = __instance.note_c_end[2].ToString();
					Plugin.LogDebug(__instance.col_r_1.text + __instance.col_g_1.text + __instance.col_b_1.text);
				}
				__instance.levelendpoint = customLevel.endpoint;
				__instance.editorendpostext.text = "end: " + __instance.levelendpoint;
				__instance.tempo = customLevel.tempo;
				__instance.defaultnotelength = customLevel.savednotespacing;
				__instance.defaultnotelength = Mathf.FloorToInt((float)__instance.defaultnotelength * GlobalVariables.gamescrollspeed);
				__instance.beatspermeasure = customLevel.timesig;
				if (__instance.leveleditor)
				{
					__instance.buildAllBGNodes();
				}
				__instance.buildNotes();
				__instance.buildAllLyrics();
				__instance.changeEditorTempo(0);
				__instance.moveTimeline(0);
				__instance.changeTimeSig(0);
				__instance.levelendtime = 60f / __instance.tempo * __instance.levelendpoint;
				
				BGControllerPatch.BGEffect = customLevel.backgroundMovement;

				var modelCam = GameObject.Find("3dModelCamera")?.GetComponent<Camera>();
				if (modelCam != null) modelCam.clearFlags = CameraClearFlags.Depth;

				Plugin.LogDebug("level end TIME: " + __instance.levelendtime);
				Plugin.LogDebug("Game Loaded");

				return false;
			}
			Plugin.LogDebug("No file exists at that filename!");
			return false;
		}
	}
}
