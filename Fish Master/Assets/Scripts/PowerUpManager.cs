using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class PowerUpManager : MonoBehaviour
{
    public static PowerUpManager Instance;
    /*[HideInInspector]*/ public int length, strength, offlineEarnings, lengthCost, strengthCost, offlineEarningCost, wallet, totalGain;

    private int[] _cost = new int[] // yeah I am lazy to put up an actual number increase algorithm so I manually added them to a script, sorry :/
    {
        120,
        140,
        180,
        250,
        350,
        500,
        650,
        850,
        1000,
        1500,
        2250,
        3500,
        5000,
        7000,
        9000,
        10000,
        13000,
        15000,
        20000
    };

    private void Awake()
    {
        if (Instance) Destroy(gameObject);
        else Instance = this;
        length = -PlayerPrefs.GetInt("Length", 30);
        strength = PlayerPrefs.GetInt("Strength", 3);
        offlineEarnings = PlayerPrefs.GetInt("OfflineEarning", 3);
        lengthCost = _cost[-length / 10 - 3];
        strengthCost = _cost[strength - 3];
        offlineEarningCost = _cost[offlineEarnings - 3];
        wallet = PlayerPrefs.GetInt("Wallet", 0);
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if(pauseStatus)
        {
            DateTime currentTime;
            currentTime = DateTime.Now;
            PlayerPrefs.SetString("Date", currentTime.ToString());
        }
        else
        {
            var @string = PlayerPrefs.GetString("Date", string.Empty);
            if (@string != string.Empty)
            {
                var dateTime = DateTime.Parse(@string);
                totalGain = (int) ((DateTime.Now - dateTime).TotalMinutes * offlineEarnings + 1.0);
                UIManager.Instance.ChangeScreen(Screens.Return);
            }
        }
    }

    private void OnApplicationQuit()
    {
        OnApplicationPause(true);
    }

    public void BuyLength()
    {
        length -= 10;
        wallet -= lengthCost;
        lengthCost = _cost[-length / 10 - 3];
        PlayerPrefs.SetInt("Length", -length);
        PlayerPrefs.SetInt("Wallet", wallet);
        UIManager.Instance.ChangeScreen(Screens.Main);
    }
    
    public void BuyStrength()
    {
        strength++;
        wallet -= strengthCost;
        strengthCost = _cost[strength - 3];
        PlayerPrefs.SetInt("Strength", strength);
        PlayerPrefs.SetInt("Wallet", wallet);
        UIManager.Instance.ChangeScreen(Screens.Main);
    }
    
    public void BuyOfflineEarning()
    {
        offlineEarnings++;
        wallet -= offlineEarningCost;
        offlineEarningCost = _cost[offlineEarnings - 3];
        PlayerPrefs.SetInt("OfflineEarning", offlineEarnings);
        PlayerPrefs.SetInt("Wallet", wallet);
        UIManager.Instance.ChangeScreen(Screens.Main);
    }

    public void CollectMoney()
    {
        wallet += totalGain;
        PlayerPrefs.SetInt("Wallet", wallet);
        UIManager.Instance.ChangeScreen(Screens.Main);
    }
    
    public void CollectDoubleMoney()
    {
        wallet += totalGain * 2;
        PlayerPrefs.SetInt("Wallet", wallet);
        UIManager.Instance.ChangeScreen(Screens.Main);
    }
}
