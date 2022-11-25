using System;
using Photon.Pun;
using UnityEngine;
using Random = UnityEngine.Random;

public class TempCard : MonoBehaviour
{
    public Action<TempCard> OnDestroy = delegate {};
    
    [SerializeField] private Vector2 _startPosition;
    [SerializeField] private Vector2 _startRotation;
    [Range(1,10)][SerializeField] private float _translationTime = 3f;
    [SerializeField] private Vector2 _destination;
    
    private Transform _middlePosition;
    private bool _travel;
    private bool _destroyOnArrival;
    private float _timer = 0f;
    
    private float _rotationDest;
    
    
    private void Awake()
    {
        var pos = transform.position;
        _startPosition = new Vector2(pos.x, pos.y);
    }

    public void SetMiddlePosition(Transform middlePosition)
    {
        _middlePosition = middlePosition;
    }

    public void TravelToMiddle(bool destroyOnArrival = false)
    {
        Debug.Log("Traveling to middle");
        _travel = true;
        _destination = new Vector2(_middlePosition.position.x, _middlePosition.position.y);
        _timer = 0;
        _destroyOnArrival = destroyOnArrival;
        _rotationDest = Random.Range(0f, 360f);
    }

    public void TravelToCharacter(Transform character, bool destroyOnArrival = false)
    {
        Debug.Log("Traveling to character");
        _travel = true;
        _destination = new Vector2(character.position.x, character.position.y);
        _timer = 0;
        _destroyOnArrival = destroyOnArrival;
        _rotationDest = Random.Range(0f, 360f);
    }

    private void Update()
    {
        if (!_travel) return;
        if (_timer < _translationTime)
        {
            transform.position = Vector2.Lerp(_startPosition, _destination, _timer/_translationTime);
            transform.Rotate(0,0, Mathf.Lerp(0, _rotationDest, _timer/_translationTime));
            _timer += Time.deltaTime;
        }
        else if (_timer >= _translationTime)
        {
            Debug.Log("Arrived to destination");
            transform.position = _middlePosition.position;
            _travel = false;
            if (_destroyOnArrival)
            {
                Debug.Log("Destroying card");
                OnDestroy.Invoke(this);
            }
        }
    }
}