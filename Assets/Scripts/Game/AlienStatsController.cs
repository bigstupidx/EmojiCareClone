﻿using System;
using System.Collections;
using UnityEngine;

public class AlienStatsController : MonoBehaviour {
	private static AlienStatsController instance = null;
	public static AlienStatsController Instance{
		get{ return instance; }
	}

	const string KEY_LASTTIMEPLAY = "LastTimePlayed";
	const string KEY_TIMEONPAUSE = "TimeOnPause";

	bool hasDoneInit = false;
	public bool isStatsDepletingStats = false;
	public bool isStartIncreasingGrowth = false;

	void Awake()
	{
		if(instance != null && instance != this) { 
			Destroy(gameObject);
			return; 
		}
		else instance = this;
		DontDestroyOnLoad(gameObject);
	}

	public void InitStatsController()
	{
		if(PlayerData.Instance.playerAlienID == -1){
			PlayerPrefs.DeleteKey(KEY_LASTTIMEPLAY);
			return;
		}else{
			if(!hasDoneInit){
				CalculateAlienStatsData();
				hasDoneInit = true;
			}
		}
	}

	void Start()
	{
		for (int i = 1; i <= 5; i++) {
			try {
				LocalNotification.CancelNotification(i);
			} catch (Exception e) {
				Debug.LogWarning("Caught an exception from LocalNotification."+e.ToString());
			}
		}	
	}

	void CalculateAlienStatsData()
	{
		if(PlayerPrefs.HasKey(KEY_LASTTIMEPLAY)){
			if(DateTime.Now.CompareTo(LastTimePlay) < 0) return;
			else if(DateTime.Now.CompareTo(LastTimePlay) > 0 ){
				
				Alien playerAlien = PlayerData.Instance.PlayerAlien;
				int totalTicks = getTotalTicks(DateTime.Now - LastTimePlay);

				for(int i = 0; i<totalTicks;i++){
					if( playerAlien.alienHungerMod <= 0 && playerAlien.alienHygeneMod <= 0 && playerAlien.alienHappinessMod <= 0){
						playerAlien.DepleteAlienHealth();
						if(PlayerData.Instance.alienDead) break;
					}else{
						playerAlien.DepleteAlienStats();
					}
				}
			}
		}

		if(PlayerData.Instance.playerAlienID != -1 && !isStatsDepletingStats) StartCoroutine(CoroutineStartDepletingStats());
		PlayerPrefs.DeleteKey(KEY_LASTTIMEPLAY);
	}

	void CalculateAlienStatsDataAfterPause(){
		if(PlayerData.Instance.PlayerAlien != null && PlayerPrefs.HasKey(KEY_TIMEONPAUSE)){
			if(DateTime.Now.CompareTo(TimeOnPause) < 0) return;
			else if(DateTime.Now.CompareTo(TimeOnPause) > 0) {
				
				Alien playerAlien = PlayerData.Instance.PlayerAlien;
				int totalTicks = getTotalTicks(DateTime.Now - TimeOnPause);

				for(int i = 0; i<totalTicks;i++){
					if( playerAlien.alienHungerMod <= 0 && playerAlien.alienHygeneMod <= 0 && playerAlien.alienHappinessMod <= 0) {
						playerAlien.DepleteAlienHealth();
						if(PlayerData.Instance.alienDead) break;
					} else { 
						playerAlien.DepleteAlienStats();

						if( playerAlien.alienHungerMod > 0 && playerAlien.alienHygeneMod > 0 && playerAlien.alienHappinessMod > 0) {
							if(playerAlien.alienGrowthMod >= playerAlien.alienGrowth){
								playerAlien.LevelUp();
								playerAlien.IncreaseGrowth();
							}else{
								playerAlien.IncreaseGrowth();
							}
						}
					}
				}
			}

			if(PlayerData.Instance.playerAlienID != -1 && !isStatsDepletingStats) StartCoroutine(CoroutineStartDepletingStats());
			PlayerPrefs.DeleteKey(KEY_TIMEONPAUSE);
		}
	}

	int getTotalTicks(TimeSpan duration)
	{
		int dayToMin = duration.Days * 24 * 60;
		int hourToMin = duration.Hours * 60;
		int min = duration.Minutes;
		int sec = duration.Seconds;

		int tempSec = ((dayToMin + hourToMin + min)* 60 ) + sec;
		int tempTick = tempSec / PlayerData.Instance.PlayerAlien.alienStatsDepletionDuration;
		return tempTick;
	}

	int getTotalTicksForGrowth(TimeSpan duration)
	{
		int dayToMin = duration.Days * 24 * 60;
		int hourToMin = duration.Hours * 60;
		int min = duration.Minutes;
		int sec = duration.Seconds;

		int tempSec = ((dayToMin + hourToMin + min)* 60 ) + sec;
		int tempTick = tempSec / PlayerData.Instance.PlayerAlien.alienGrowthDuration;
		return tempTick;
	}

	public DateTime LastTimePlay{
		get{ return DateTime.Parse(PlayerPrefs.GetString(KEY_LASTTIMEPLAY)); }
		set{ PlayerPrefs.SetString(KEY_LASTTIMEPLAY,value.ToString()); }
	}

	public DateTime TimeOnPause{
		get{ return DateTime.Parse(PlayerPrefs.GetString(KEY_TIMEONPAUSE)); }
		set{ PlayerPrefs.SetString(KEY_TIMEONPAUSE,value.ToString()); }
	}

	public void CheckAlienStatsForGrowth()
	{
		if((PlayerData.Instance.PlayerAlien.alienHungerMod > 0 && 
			PlayerData.Instance.PlayerAlien.alienHygeneMod > 0 && 
			PlayerData.Instance.PlayerAlien.alienHappinessMod > 0) &&
			!isStartIncreasingGrowth
		){
			if(!isStartIncreasingGrowth) StartCoroutine(CoroutineStartIncreasingGrowth());
		}
	}

	IEnumerator CoroutineStartIncreasingGrowth()
	{
		isStartIncreasingGrowth = true;
		while(true){
			yield return new WaitForSeconds(PlayerData.Instance.PlayerAlien.alienGrowthDuration);
			if(PlayerData.Instance.PlayerAlien.alienGrowthMod >= PlayerData.Instance.PlayerAlien.alienGrowth){
				PlayerData.Instance.PlayerAlien.LevelUp();
				PlayerData.Instance.PlayerAlien.IncreaseGrowth();
			}else{
				PlayerData.Instance.PlayerAlien.IncreaseGrowth();
			}
		}
	}

	IEnumerator CoroutineStartDepletingStats()
	{
		isStatsDepletingStats = true;
		while(true){
			yield return new WaitForSeconds(PlayerData.Instance.PlayerAlien.alienStatsDepletionDuration);
			if( PlayerData.Instance.PlayerAlien.alienHungerMod <= 0 || PlayerData.Instance.PlayerAlien.alienHygeneMod <= 0 || PlayerData.Instance.PlayerAlien.alienHappinessMod <= 0){
				StopCoroutine(CoroutineStartIncreasingGrowth());
				isStartIncreasingGrowth = false;
				PlayerData.Instance.PlayerAlien.DepleteAlienHealth();
			}else{
				PlayerData.Instance.PlayerAlien.DepleteAlienStats();
			}
		}
	}

	void OnApplicationPause(bool pauseStatus)
	{
		if(pauseStatus){
			FireNotifications();
			StopAllCoroutines();
			isStatsDepletingStats = false;
			isStartIncreasingGrowth = false;
			if(PlayerData.Instance.playerAlienID != -1) TimeOnPause = DateTime.Now;
		}else{
			if(PlayerData.Instance.playerAlienID != -1 && hasDoneInit) CalculateAlienStatsDataAfterPause();
		}
	}

	void OnApplicationQuit()
	{
		StopAllCoroutines();
		isStatsDepletingStats = false;
		isStartIncreasingGrowth = false;
		if(PlayerData.Instance.playerAlienID != -1) LastTimePlay = DateTime.Now;
		PlayerPrefs.Save();
	}

	void FireNotifications ()
	{
		if (PlayerData.Instance.playerAlienID != -1) {
			long statDurMS = PlayerData.Instance.PlayerAlien.alienStatsDepletionDuration * 1000;
			float threshold = 0.25f;
			float happinessTimer = PlayerData.Instance.PlayerAlien.alienHappinessMod - PlayerData.Instance.PlayerAlien.alienHappiness * threshold;
			float healthTimer = PlayerData.Instance.PlayerAlien.alienHealthMod - PlayerData.Instance.PlayerAlien.alienHealth * threshold;
			float hungerTimer = PlayerData.Instance.PlayerAlien.alienHungerMod - PlayerData.Instance.PlayerAlien.alienHunger * threshold;
			float hygieneTimer = PlayerData.Instance.PlayerAlien.alienHygeneMod - PlayerData.Instance.PlayerAlien.alienHygene * threshold;
			float deathTimer = PlayerData.Instance.PlayerAlien.alienHealthMod * statDurMS;

			if (PlayerData.Instance.PlayerAlien.alienType == AlienType.Active) {
				if (hygieneTimer > 0) {
					LocalNotification.SendNotification (3, (long)(hygieneTimer * statDurMS + 2000), "Emoji Care", "Come clean your emoji!", new Color32 (0xff, 0x44, 0x44, 255));
					//LocalNotification.SendNotification (3, 7000, "Emoji Care", "Come clean with your emoji!", new Color32 (0xff, 0x44, 0x44, 255));
				} else if (hygieneTimer <= 0) {
					LocalNotification.SendNotification (3, 7000, "Emoji Care", "Come clean your emoji!", new Color32 (0xff, 0x44, 0x44, 255));
				}
			} else if (PlayerData.Instance.PlayerAlien.alienType == AlienType.Greedy) {
				if (hungerTimer > 0) {
					LocalNotification.SendNotification (2, (long)(hungerTimer * statDurMS + 1000), "Emoji Care", "Come feed your emoji!", new Color32 (0xff, 0x44, 0x44, 255));
					//LocalNotification.SendNotification (2, 6000, "Emoji Care", "Come feed your emoji!", new Color32 (0xff, 0x44, 0x44, 255));
				} else if (hungerTimer <= 0) {
					LocalNotification.SendNotification (2, 6000, "Emoji Care", "Come feed your emoji!", new Color32 (0xff, 0x44, 0x44, 255));
				}
			} else if (PlayerData.Instance.PlayerAlien.alienType == AlienType.Sad) {
				if (happinessTimer > 0) {
					LocalNotification.SendNotification (1, (long)(happinessTimer * statDurMS), "Emoji Care", "Come play with your emoji!", new Color32 (0xff, 0x44, 0x44, 255));
					//LocalNotification.SendNotification (1, 5000, "Emoji Care", "Come play with your emoji!", new Color32 (0xff, 0x44, 0x44, 255));
				} else if (happinessTimer <= 0) {
					LocalNotification.SendNotification (1, 5000, "Emoji Care", "Come play with your emoji!", new Color32 (0xff, 0x44, 0x44, 255));
				}
			}

			if (healthTimer > 0) {
				Debug.Log ("health1");
				LocalNotification.SendNotification (4, (long)(healthTimer * statDurMS + 3000), "Emoji Care", "Your emoji is dying!", new Color32 (0xff, 0x44, 0x44, 255));
				//LocalNotification.SendNotification (4, 8000, "Emoji Care", "Your emoji is dying!", new Color32 (0xff, 0x44, 0x44, 255));
			} else if (healthTimer <= 0) {
				Debug.Log ("health2");
				LocalNotification.SendNotification (5, (long)(deathTimer+4000), "Emoji Care", "Your emoji is dead!", new Color32 (0xff, 0x44, 0x44, 255));
			}
		}
	}
}