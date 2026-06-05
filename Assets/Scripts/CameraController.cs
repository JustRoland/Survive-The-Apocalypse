using System;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private PlayerController _playerController;
    private Transform _target;
    [SerializeField] private Vector3 offset;
    [SerializeField] private Vector2 deadZoneRect;
    [SerializeField] private float easingStart;
    [SerializeField] private float easingEnd;
    Vector3 _velocity = Vector3.zero;

    private void Start()
    {
        _playerController = GameManager.Instance.Player;
        _target = _playerController.transform;
        transform.position = _target.position + offset;
    }


    private void FixedUpdate()
    {
        var distanceFromCenter = _target.position - transform.position + offset;
        
        var boundsX = Mathf.Abs(distanceFromCenter.x) > deadZoneRect.x;
        var boundsY = Mathf.Abs(distanceFromCenter.y) > deadZoneRect.y;

        var targetPosition = transform.position;
        
        if (boundsX)
        {
            targetPosition.x += distanceFromCenter.x - deadZoneRect.x * (distanceFromCenter.x/Mathf.Abs(distanceFromCenter.x));
        }

        if (boundsY)
        {
            targetPosition.y += distanceFromCenter.y - deadZoneRect.y * (distanceFromCenter.y/Mathf.Abs(distanceFromCenter.y));
        }

        transform.position = _playerController.VelocityX is >= -1f and <= 1f
            ? Vector3.SmoothDamp(transform.position, _target.position + offset, ref _velocity, easingEnd)
            : Vector3.SmoothDamp(transform.position, targetPosition, ref _velocity, easingStart);

    }
}
