using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    private GameObject _currentScreen;
    public GameObject mainUi, playUi, endUi, returnUi;

    public Button lengthButton, strengthButton, offlineEarningButton;

    public TextMeshProUGUI gameScreenMoney,
        lengthCostText,
        lengthValueText,
        strengthCostText,
        strengthValueText,
        offlineEarningCostText,
        offlineEarningValueText,
        endScreenMoney, returnScreenMoney;

    private int _gameCount;

    private void Awake()
    {
        if (Instance) Destroy(gameObject);
        else Instance = this;
        _currentScreen = mainUi;
    }

    private void Start()
    {
        CheckPowerUps();
        UpdateTexts();
    }

    public void ChangeScreen(Screens screens)
    {
        _currentScreen.SetActive(false);
        switch (screens)
        {
            case Screens.Main:
                _currentScreen = mainUi;
                UpdateTexts();
                CheckPowerUps();
                break;
            case Screens.Play:
                _currentScreen = playUi;
                _gameCount++;
                break;
            case Screens.End:
                _currentScreen = endUi;
                _gameCount++;
                SetEndScreenMoney();
                break;
            case Screens.Return:
                _currentScreen = returnUi;
                SetReturnScreenMoney();
                break;
        }
        _currentScreen.SetActive(true);
    }

    private void SetEndScreenMoney()
    {
        endScreenMoney.text = "$" + PowerUpManager.Instance.totalGain;
    }

    private void SetReturnScreenMoney()
    {
        returnScreenMoney.text = "YOU GAINED $" + PowerUpManager.Instance.totalGain + " WHILE YOU WERE OUTSIDE!";
    }

    private void UpdateTexts()
    {
        gameScreenMoney.text = "$" + PowerUpManager.Instance.wallet;
        lengthCostText.text = "$" + PowerUpManager.Instance.lengthCost;
        lengthValueText.text = -PowerUpManager.Instance.length + "M";
        strengthCostText.text = "$" + PowerUpManager.Instance.strengthCost;
        strengthValueText.text = PowerUpManager.Instance.strength + " FISHES";
        offlineEarningCostText.text = "$" + PowerUpManager.Instance.offlineEarningCost;
        offlineEarningValueText.text = "$" + PowerUpManager.Instance.offlineEarnings + "/min";
    }

    private void CheckPowerUps()
    {
        var lengthCost = PowerUpManager.Instance.lengthCost;
        var strengthCost = PowerUpManager.Instance.strengthCost;
        var offlineEarningCost = PowerUpManager.Instance.offlineEarningCost;
        var wallet = PowerUpManager.Instance.wallet;

        lengthButton.interactable = wallet >= lengthCost;
        strengthButton.interactable = wallet >= strengthCost;
        offlineEarningButton.interactable = wallet >= offlineEarningCost;
    }
}