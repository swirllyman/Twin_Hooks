using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine;
public class Aimer2D : MonoBehaviour
{
    public Transform crosshair;
    [SerializeField] Transform playerCameraFollow;
    [SerializeField] Transform lookAtObject;
    public float cameraOffsetDistance = .35f;
    [SerializeField] float camTargetSpeed = .25f;
    internal Vector3 aimDirection;
    internal float aimDistance = 1.0f;

    
    float mouseDistance;

    private void Start()
    {
        //playerCameraFollow.transform.parent = null;
    }

    public Vector3 GetAimDirection()
    {
        return aimDirection;
    }

    // Update is called once per frame
    void Update()
    {
        aimDirection = Vector3.zero;
        float aimAngle = 0;
        var worldMousePosition = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0f));
        mouseDistance = Vector2.Distance(worldMousePosition, transform.position);
        var facingDirection = worldMousePosition - transform.position;
        aimAngle = Mathf.Atan2(facingDirection.y, facingDirection.x);
        if (aimAngle < 0f)
        {
            aimAngle = Mathf.PI * 2 + aimAngle;
        }

        aimDirection = Quaternion.Euler(0, 0, aimAngle * Mathf.Rad2Deg) * Vector2.right;
        SetCrosshairPosition(aimAngle);
        playerCameraFollow.transform.position = transform.position;
        lookAtObject.localPosition = Vector3.Lerp(lookAtObject.localPosition, aimDirection * cameraOffsetDistance, Time.deltaTime * camTargetSpeed);
    }

    /// <summary>
    /// Move the aiming crosshair based on aim angle
    /// </summary>
    /// <param name="aimAngle">The mouse aiming angle</param>
    public void SetCrosshairPosition(float aimAngle)
    {

        if (mouseDistance > aimDistance)
        {
            mouseDistance = aimDistance;
        }

        var x = transform.position.x + 1f * Mathf.Cos(aimAngle) * mouseDistance;
        var y = transform.position.y + 1f * Mathf.Sin(aimAngle) * mouseDistance;

        var crossHairPosition = new Vector3(x, y, 0);
        crosshair.transform.position = crossHairPosition;
    }
}
