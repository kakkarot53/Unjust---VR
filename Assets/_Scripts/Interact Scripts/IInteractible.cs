using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInteractible
{
    void Interact();
    bool CanInteract();
}
public static class InteractibleExtensions
{
    public static bool IsWithinViewport(this IInteractible interactible, Camera camera, Transform objectTransform, float minDistance = 0.1f, float verticalTolerance = 80f)
    {
        Vector3 viewportPoint = camera.WorldToViewportPoint(objectTransform.position);
        float distanceToObject = Vector3.Distance(camera.transform.position, objectTransform.position);

        // Check if the object is within the minimum distance to automatically consider it "in view"
        if (distanceToObject <= minDistance)
        {
            return true;
        }

        // Define a slightly more generous range for the vertical axis (y-axis) tolerance
        bool isWithinHorizontalBounds = viewportPoint.x > 0 && viewportPoint.x < 1;
        bool isWithinVerticalBounds = viewportPoint.y > -verticalTolerance && viewportPoint.y < 1 + verticalTolerance;
        bool isInFrontOfCamera = viewportPoint.z > 0;

        return isInFrontOfCamera && isWithinHorizontalBounds && isWithinVerticalBounds;
    }
}
