using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using com.kleberswf.lib.core;
using Newtonsoft.Json;
#if !DISABLESTEAMWORKS
using Steamworks;
#endif
using UnityEngine;

public class PlayerDataHandler : Singleton<PlayerDataHandler>
{
    private Dictionary<string, string> playerData;

    private string savePath;
    [SerializeField] 
    private string saveFileName;
    [SerializeField] 
    private string saveFileFormat;

    private void Start()
    {
        savePath = InitSavePath();
        LoadData();
    }

    private void OnDestroy()
    {
        SaveData();
    }

    private string InitSavePath()
    {
        StringBuilder savePathBuilder = new StringBuilder(Application.persistentDataPath);
        savePathBuilder.Append('\\');
        
#if !DISABLESTEAMWORKS
        if (SteamManager.Initialized)
        {
            CSteamID steamID = SteamUser.GetSteamID();
            if (steamID != CSteamID.Nil)
            {
                savePathBuilder.Append("STEAM");
                savePathBuilder.Append(steamID.ToString());
                savePathBuilder.Append('\\');
            }
        }
        savePathBuilder.Append(saveFileName);
        savePathBuilder.Append('.');
        savePathBuilder.Append(saveFileFormat);
        
#elif !DISABLESTOVE
        savePathBuilder.Append("STOVE");
        string userId = StoveManager.Instance.User.GameUserId;
        savePathBuilder.Append(userId);
        
        savePathBuilder.Append(saveFileName);
        savePathBuilder.Append('.');
        savePathBuilder.Append(saveFileFormat);
#endif

        return savePathBuilder.ToString();
    }
    
    private void LoadData()
    {
        try
        {
            if (File.Exists(savePath))
            {
                Debug.Log($"게임 로드 완료: {savePath}");
                // 파일에서 JSON 데이터 읽기
                string jsonData = File.ReadAllText(savePath);
                
                // JSON 문자열을 SaveData 객체로 변환
                playerData = JsonConvert.DeserializeObject<Dictionary<string,string>>(jsonData);
            }
            else if (File.Exists(LoadOldSavePath()))
            {
                string jsonData = File.ReadAllText(LoadOldSavePath());
                playerData = JsonConvert.DeserializeObject<Dictionary<string,string>>(jsonData);
            }
            else
            {
                playerData = new Dictionary<string, string>();
            }

        }
        catch (Exception e)
        {
            Debug.LogError($"로드 중 에러 발생: {e.Message}");
            playerData = new Dictionary<string, string>();
        }
    }

    /// <summary>
    /// 저장 경로의 변경으로 이전 버전 호환을 위해 사용.
    /// </summary>
    /// <returns></returns>
    private string LoadOldSavePath()
    {
        StringBuilder savePathBuilder = new StringBuilder(Application.persistentDataPath);
        
#if !DISABLESTEAMWORKS
        if (SteamManager.Initialized)
        {
            CSteamID steamID = SteamUser.GetSteamID();
            if (steamID != CSteamID.Nil)
            {
                savePathBuilder.Append(steamID.ToString());
            }
        }
        savePathBuilder.Append(saveFileName);
        savePathBuilder.Append('.');
        savePathBuilder.Append(saveFileFormat);
        
#elif !DISABLESTOVE
        string userId = StoveManager.Instance.User.GameUserId;
        savePathBuilder.Append(userId);
        
        savePathBuilder.Append(saveFileName);
        savePathBuilder.Append('.');
        savePathBuilder.Append(saveFileFormat);
#endif

        return savePathBuilder.ToString();
    }

    public void SaveData()
    {
        try 
        {
            // Dictionary를 포함한 데이터를 JSON 문자열로 변환
            string jsonData = JsonConvert.SerializeObject(playerData);
            
            // 파일에 JSON 데이터 쓰기
            File.WriteAllText(savePath, jsonData);
            Debug.Log($"게임 저장 완료: {savePath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"저장 중 에러 발생: {e.Message}");
        }
    }

    #region Methods for get data

    public bool HasKey(string key)
    {
        return playerData.ContainsKey(key);
    }

    public int GetInt(string key, int defaultValue = 0)
    {
        return playerData.ContainsKey(key) ? int.Parse(playerData[key]) : defaultValue;
    }
    
    public bool SetInt(string key, int value)
    {
        try
        {
            if (playerData.ContainsKey(key))
            {
                playerData[key] = value.ToString();
            }
            else
            {
                playerData.Add(key, value.ToString());
            }

            return true;
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            return false;
        }
    }
    
    #endregion
}
