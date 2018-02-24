using UnityEngine;
using BDArmory;
using BDArmory.Control;
using BDArmory.Misc;

namespace SimpleAI
{
    public class SimpleAI : BDGenericAIBase
    {
        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "speed"),
            UI_FloatRange(minValue = 100, maxValue = 800, stepIncrement = 10)]
        public float speed = 300;

        public override bool IsValidFixedWeaponTarget(Vessel target)
        {
            return true;
        }

        public override bool CanEngage()
        {
            return (speedController.debugThrust > 0) ;
        }

        protected override void AutoPilot(FlightCtrlState s)
        {
            speedController.targetSpeed = speed;

            Vector3 targetDirection;

            if (MissileGuidance.GetRadarAltitude(vessel) < 500)
            {
                targetDirection = Quaternion.AngleAxis(75, vessel.transform.right) * VectorUtils.GetUpDirection(vessel.CoM);
                vessel.ActionGroups.SetGroup(KSPActionGroup.Gear, true);
            }
            else
            {
                if (targetVessel != null)
                    targetDirection = targetVessel.CoM - vessel.CoM;
                else
                {
                    Vector3 worldPos = vessel.mainBody.GetWorldSurfacePosition(assignedPositionGeo.x, assignedPositionGeo.y, 0);
                    targetDirection = worldPos + VectorUtils.GetUpDirection(worldPos) * ((float)vessel.mainBody.GetAltitude(worldPos) + 1500) - vessel.CoM;
                }
                vessel.ActionGroups.SetGroup(KSPActionGroup.Gear, false);
            }

            float pitchError = VectorUtils.SignedAngle(vessel.transform.up, Vector3.ProjectOnPlane(targetDirection, vessel.transform.right), -vessel.transform.forward);
            float yawError = VectorUtils.SignedAngle(vessel.transform.up, Vector3.ProjectOnPlane(targetDirection, vessel.transform.forward), vessel.transform.right);
            float rollError = VectorUtils.SignedAngle(-vessel.transform.forward, Vector3.ProjectOnPlane(Vector3.Angle(vessel.transform.up, targetDirection) > 7 ? targetDirection : VectorUtils.GetUpDirection(vessel.CoM), vessel.transform.up), vessel.transform.right);

            Vector3 localAngVel = vessel.angularVelocity;
            s.roll = 0.04f * rollError - 1.2f * -localAngVel.y;
            s.pitch = 0.1f * pitchError - 3f * -localAngVel.x;
            s.yaw = 0.03f * yawError - 0.6f * -localAngVel.z;
        }
    }
}
