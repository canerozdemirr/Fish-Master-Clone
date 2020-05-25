using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Hook : MonoBehaviour
{
    public Transform hookEnd;

    private CapsuleCollider2D _capsuleCollider2D;
    private Camera _mainCamera;

    private int _limit, _distance, _fishCount;
    private bool _canMove;

    private Tweener _cameraTween;
    private List<Fish> _hookedFishes;

    private void Awake()
    {
        _mainCamera = Camera.main;
        _capsuleCollider2D = gameObject.GetComponent<CapsuleCollider2D>();
        hookEnd = transform.GetChild(1);
        _hookedFishes = new List<Fish>();
    }

    private void Update()
    {
        if (_canMove && Input.GetMouseButton(0))
        {
            var touchPosition = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
            var hookPosition = transform.position;
            hookPosition.x = touchPosition.x;
            transform.position = hookPosition;
        }
    }

    public void StartFishing()
    {
        _distance = PowerUpManager.Instance.length;
        _limit = PowerUpManager.Instance.strength;
        _fishCount = 0;
        var time = -_distance * 0.1f;
        _cameraTween = _mainCamera.transform.DOMoveY(_distance, 1 + time * 0.25f).OnUpdate(delegate
        {
            if (_mainCamera.transform.position.y <= -11) transform.SetParent(_mainCamera.transform);
        }).OnComplete(delegate
        {
            _capsuleCollider2D.enabled = true;
            _cameraTween = _mainCamera.transform.DOMoveY(0, time * 5).OnUpdate(delegate
            {
                if (_mainCamera.transform.position.y >= -25f)
                    StopFishing();
            });
        });
        UIManager.Instance.ChangeScreen(Screens.Play);
        _capsuleCollider2D.enabled = false;
        _canMove = true;
        _hookedFishes.Clear();
    }

    private void StopFishing()
    {
        _canMove = false;
        _cameraTween.Kill();
        _cameraTween = _mainCamera.transform.DOMoveY(0, 2).OnUpdate(delegate
        {
            if (_mainCamera.transform.position.y >= -11)
            {
                transform.SetParent(null);
                transform.position = new Vector2(transform.position.x, -6);
            }
        }).OnComplete(delegate
        {
            transform.position = Vector2.down * 6;
            _capsuleCollider2D.enabled = true;
            var totalFishNumber = 0;
            foreach (var t in _hookedFishes)
            {
                t.transform.SetParent(null);
                t.RefreshFish();
                totalFishNumber += t.fishType.price;
            }

            PowerUpManager.Instance.totalGain = totalFishNumber;
            UIManager.Instance.ChangeScreen(Screens.End);
        });
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Fish") && _fishCount != _limit)
        {
            _fishCount++;
            var caughtFish = other.GetComponent<Fish>();
            caughtFish.Hooked();
            _hookedFishes.Add(caughtFish);
            other.transform.SetParent(transform);
            other.transform.position = hookEnd.transform.position;
            other.transform.rotation = hookEnd.rotation;
            other.transform.localScale = Vector3.one;
            other.transform.DOShakeRotation(5f, Vector3.forward * 45f).SetLoops(1,LoopType.Yoyo).OnComplete(delegate
            {
                other.transform.rotation = Quaternion.identity;
            });
            if(_fishCount >= _limit) StopFishing();
        }
    }
}