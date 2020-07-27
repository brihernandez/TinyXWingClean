using UnityEngine;

public static class GunMaths
{
    public static Vector3 ComputeGunLead(Vector3 targetPos, Vector3 targetVel, Vector3 ownPos, Vector3 ownVel, float muzzleVelocity)
    {
        if (muzzleVelocity < 1f)
            return targetPos;

        // Figure out ETA for bullets to reach target.
        Vector3 predictedTargetPos = targetPos + targetVel;
        Vector3 predictedOwnPos = ownPos + ownVel;

        float range = Vector3.Distance(predictedOwnPos, predictedTargetPos);
        float timeToHit = range / muzzleVelocity;

        // Project velocity of target using the TimeToHit.
        Vector3 leadMarker = (targetVel - ownVel) * timeToHit + targetPos;

        //Debug.DrawLine(tform.position, leadMarker, Color.yellow);
        return leadMarker;
    }
}
