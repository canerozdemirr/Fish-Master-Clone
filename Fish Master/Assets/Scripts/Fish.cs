using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Fish : MonoBehaviour
{
    private FishType _fishType;
    private CircleCollider2D _circleCollider2D;
    private SpriteRenderer _spriteRenderer;
    private float _screenSize;
    private Tweener _fishTweener;
    public Camera mainCamera;

    [Serializable]
    public class FishType
    {
        public int price;
        public float fishCount;
        public float minimumLength;
        public float maximumLength;
        public float colliderRadius;
        public Sprite sprite;
    }

    public FishType fishType
    {
        get => _fishType;
        set
        {
            _fishType = value;
            _circleCollider2D.radius = _fishType.colliderRadius;
            _spriteRenderer.sprite = _fishType.sprite;
        }
    }

    private void Awake()
    {
        _circleCollider2D = gameObject.GetComponent<CircleCollider2D>();
        _spriteRenderer = transform.GetChild(0).GetComponent<SpriteRenderer>();
        mainCamera = Camera.main;
        if (mainCamera != null) _screenSize = mainCamera.ScreenToWorldPoint(Vector3.zero).x;
    }
    public void RefreshFish()
    {
        _fishTweener?.Kill();
        var fishNumber = UnityEngine.Random.Range(_fishType.minimumLength, _fishType.maximumLength);
        _circleCollider2D.enabled = true;
        var fishPosition = transform.position;
        fishPosition.y = fishNumber;
        fishPosition.x = _screenSize;
        transform.position = fishPosition;

        const int secondFishNumber = 1;
        var y = UnityEngine.Random.Range(fishNumber - secondFishNumber, fishNumber + secondFishNumber);
        var vector = new Vector2(-fishPosition.x, y);

        const int thirdFishNumber = 3;
        var delay = UnityEngine.Random.Range(0, 2 * thirdFishNumber);
        _fishTweener = transform.DOMove(vector, thirdFishNumber).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.Linear)
            .SetDelay(delay).OnStepComplete(
                delegate
                {
                    var localScale = transform.localScale;
                    localScale.x = -localScale.x;
                    transform.localScale = localScale;
                });
    }

    public void Hooked()
    {
        _circleCollider2D.enabled = false;
        _fishTweener.Kill();
    }
}