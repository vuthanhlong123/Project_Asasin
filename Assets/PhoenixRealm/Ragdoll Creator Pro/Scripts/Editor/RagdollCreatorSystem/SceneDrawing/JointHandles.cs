#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace PhoenixRealm.RagdollCreatorPro
{
    public static class JointHandles
    {
        private const float kDialR = 0.38f;
        private const float kThin = 2.0f;
        private const float kThick = 3.5f;

        #region Anchor
        public static bool DrawAnchorHandle(Transform bone, ref Vector3 anchorLocal)
        {
            if (bone == null) return false;
            Vector3 world = bone.TransformPoint(anchorLocal);
            float size = HandleUtility.GetHandleSize(world) * 0.09f;

            EditorGUI.BeginChangeCheck();
            Quaternion rot = Tools.pivotRotation == PivotRotation.Local ? bone.rotation : Quaternion.identity;
            Vector3 newWorld = Handles.PositionHandle(world, rot);
            // glyph
            DrawTargetGlyph(newWorld, size);
            bool changed = EditorGUI.EndChangeCheck();

            if (changed)
            {
                anchorLocal = bone.InverseTransformPoint(newWorld);
            }
            return changed;
        }
        #endregion

        #region Axis Dial
        public static bool DrawAxisDial(Transform bone, ref Vector3 axisLocal)
        {
            if (bone == null) return false;
            if (axisLocal.sqrMagnitude < 1e-6f) axisLocal = Vector3.right;

            Vector3 origin = bone.position;
            float handleSize = HandleUtility.GetHandleSize(origin);
            float coneLength = handleSize * 0.4f;
            float coneRadius = handleSize * 0.09f;

            // Transform axis to world space using bone's rotation
            Vector3 twistAxisWorld = bone.TransformDirection(axisLocal).normalized;

            // Calculate swing axis using EXACT same logic as baking code
            Vector3 swingAxisLocal = Vector3.Cross(axisLocal, Vector3.up);
            if (swingAxisLocal.sqrMagnitude < 1e-6f)
                swingAxisLocal = Vector3.Cross(axisLocal, Vector3.forward);
            swingAxisLocal = swingAxisLocal.normalized;

            Vector3 swingAxisWorld = bone.TransformDirection(swingAxisLocal).normalized;
            Vector3 swing2AxisWorld = Vector3.Cross(twistAxisWorld, swingAxisWorld).normalized;

            // Build rotation from current axis - use swing2 as up to avoid gimbal lock
            Quaternion currentRotation = Quaternion.LookRotation(twistAxisWorld, swing2AxisWorld);

            EditorGUI.BeginChangeCheck();
            Quaternion newRotation = Handles.RotationHandle(currentRotation, origin);

            bool changed = false;
            if (EditorGUI.EndChangeCheck())
            {
                // Calculate rotation delta and apply it to the axis
                Quaternion delta = newRotation * Quaternion.Inverse(currentRotation);
                Vector3 newTwistAxisWorld = delta * twistAxisWorld;

                // Convert back to local space
                axisLocal = bone.InverseTransformDirection(newTwistAxisWorld).normalized;
                changed = true;

                // Recalculate for drawing
                twistAxisWorld = newTwistAxisWorld.normalized;
                swingAxisLocal = Vector3.Cross(axisLocal, Vector3.up);
                if (swingAxisLocal.sqrMagnitude < 1e-6f)
                    swingAxisLocal = Vector3.Cross(axisLocal, Vector3.forward);
                swingAxisLocal = swingAxisLocal.normalized;
                swingAxisWorld = bone.TransformDirection(swingAxisLocal).normalized;
                swing2AxisWorld = Vector3.Cross(twistAxisWorld, swingAxisWorld).normalized;
            }

            // Draw twist axis cone (orange - non-interactive, just visual reference)
            Vector3 twistTip = origin + twistAxisWorld * coneLength;
            Handles.color = new Color(1f, 0.5f, 0.1f, 0.9f);
            Handles.DrawLine(origin, twistTip, kThick);
            Quaternion twistRotationViz = Quaternion.LookRotation(twistAxisWorld);
            Handles.ConeHandleCap(0, twistTip, twistRotationViz, coneRadius, EventType.Repaint);
            DrawAxisLabel(twistTip + twistAxisWorld * (handleSize * 0.1f), "Twist", new Color(1f, 0.5f, 0.1f));

            // Draw swing axis cone (green - non-interactive, just visual reference)
            Vector3 swingTip = origin + swingAxisWorld * coneLength;
            Handles.color = new Color(0.3f, 1f, 0.3f, 0.9f);
            Handles.DrawLine(origin, swingTip, kThick);
            Quaternion swingRotationViz = Quaternion.LookRotation(swingAxisWorld);
            Handles.ConeHandleCap(0, swingTip, swingRotationViz, coneRadius, EventType.Repaint);
            DrawAxisLabel(swingTip + swingAxisWorld * (handleSize * 0.1f), "Swing", new Color(0.3f, 1f, 0.3f));

            return changed;
        }

        private static void DrawAxisLabel(Vector3 worldPos, string text, Color color)
        {
            Handles.BeginGUI();
            Vector2 screenPos = HandleUtility.WorldToGUIPoint(worldPos);

            GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.normal.textColor = color;
            labelStyle.fontStyle = FontStyle.Bold;
            labelStyle.fontSize = 10;

            Vector2 labelSize = labelStyle.CalcSize(new GUIContent(text));
            Rect labelRect = new Rect(screenPos.x - labelSize.x * 0.5f, screenPos.y - labelSize.y * 0.5f, labelSize.x, labelSize.y);

            GUI.Label(labelRect, text, labelStyle);
            Handles.EndGUI();
        }
        #endregion

        #region Twist Dial (sector + ticks + badges)
        public static bool DrawTwistDial(Transform bone, Vector3 axisLocal, ref SoftJointLimit low, ref SoftJointLimit high)
        {
            if (bone == null) return false;

            Vector3 axisW = bone.TransformDirection(axisLocal.sqrMagnitude > 1e-6f ? axisLocal.normalized : Vector3.right);
            Vector3 origin = bone.position;
            float r = HandleUtility.GetHandleSize(origin) * kDialR;

            // CRITICAL FIX: Use the SAME swing axis calculation as baking code
            // This ensures twist zero angle matches Unity's CharacterJoint visualization
            Vector3 swingAxisLocal = Vector3.Cross(axisLocal, Vector3.up);
            if (swingAxisLocal.sqrMagnitude < 1e-6f)
                swingAxisLocal = Vector3.Cross(axisLocal, Vector3.forward);
            swingAxisLocal = swingAxisLocal.normalized;

            Vector3 refDir = bone.TransformDirection(swingAxisLocal).normalized;

            // Base ring + ticks
            Handles.color = new Color(1, 1, 1, 0.65f);
            Handles.DrawWireDisc(origin, axisW, r);
            DrawTickRing(origin, axisW, r, 15f, 0.018f, refDir);

            // Sector fill (low..high) using refDir
            DrawSector(origin, axisW, r, low.limit, high.limit, new Color(1f, 0.6f, 0.2f, 0.18f), refDir);

            bool changed = false;

            float lowNew = AngleHandle(origin, axisW, r, low.limit, "L", refDir);
            if (!Mathf.Approximately(lowNew, low.limit))
            {
                low.limit = Mathf.Min(lowNew, high.limit - 0.1f);
                changed = true;
            }

            float highNew = AngleHandle(origin, axisW, r, high.limit, "H", refDir);
            if (!Mathf.Approximately(highNew, high.limit))
            {
                high.limit = Mathf.Max(highNew, low.limit + 0.1f);
                changed = true;
            }

            // Draw swing axis reference to show where twist zero is
            Vector3 swingTip = origin + refDir * (r * 1.15f);
            Handles.color = new Color(0.3f, 1f, 0.3f, 0.6f);
            Handles.DrawDottedLine(origin, swingTip, 2f);
            Handles.SphereHandleCap(0, swingTip, Quaternion.identity, HandleUtility.GetHandleSize(swingTip) * 0.04f, EventType.Repaint);

            return changed;
        }
        #endregion

        #region Swing Envelope
        public static bool DrawSwingEnvelope(Transform bone, Vector3 axisLocal, ref SoftJointLimit s1, ref SoftJointLimit s2)
        {
            if (bone == null) return false;

            Vector3 twistW = bone.TransformDirection(axisLocal.sqrMagnitude > 1e-6f ? axisLocal.normalized : Vector3.right);

            // CRITICAL FIX: Use the SAME swing axis calculation as baking code
            // This ensures swing visualization matches Unity's CharacterJoint
            Vector3 swingAxisLocal = Vector3.Cross(axisLocal, Vector3.up);
            if (swingAxisLocal.sqrMagnitude < 1e-6f)
                swingAxisLocal = Vector3.Cross(axisLocal, Vector3.forward);
            swingAxisLocal = swingAxisLocal.normalized;

            // u = swing axis (Swing1)
            Vector3 u = bone.TransformDirection(swingAxisLocal).normalized;

            // v = swing2 axis (Swing2) - perpendicular to both twist and swing1
            Vector3 v = Vector3.Cross(twistW, u).normalized;

            Vector3 origin = bone.position;
            float r = HandleUtility.GetHandleSize(origin) * (kDialR * 1.1f);

            // Base rings + ticks
            Handles.color = new Color(1, 1, 1, 0.65f);
            Handles.DrawWireDisc(origin, u, r);
            Handles.DrawWireDisc(origin, v, r);
            DrawTickRing(origin, u, r, 15f, 0.014f, twistW);
            DrawTickRing(origin, v, r, 15f, 0.014f, twistW);

            // Wedges
            DrawSwingWedge(origin, u, twistW, r, s1.limit, new Color(0.45f, 1f, 0.45f, 0.18f));
            DrawSwingWedge(origin, v, twistW, r, s2.limit, new Color(0.35f, 0.95f, 0.95f, 0.18f));

            bool changed = false;

            // S1 handle (Swing 1 Limit - around swing axis 'u')
            float ns1 = RadialAngleHandle(origin, u, twistW, r, s1.limit, "S1");
            ns1 = Mathf.Clamp(ns1, 0f, 179f);
            if (!Mathf.Approximately(ns1, s1.limit))
            {
                s1.limit = ns1;
                changed = true;
            }

            // S2 handle (Swing 2 Limit - around swing2 axis 'v')
            float ns2 = RadialAngleHandle(origin, v, twistW, r, s2.limit, "S2");
            ns2 = Mathf.Clamp(ns2, 0f, 179f);
            if (!Mathf.Approximately(ns2, s2.limit))
            {
                s2.limit = ns2;
                changed = true;
            }

            // Center glyph
            DrawCenterGlyph(origin, twistW, r * 0.32f);

            // Draw axis references to show which is which
            Vector3 uTip = origin + u * (r * 0.85f);
            Vector3 vTip = origin + v * (r * 0.85f);

            Handles.color = new Color(0.45f, 1f, 0.45f, 0.7f);
            Handles.DrawDottedLine(origin, uTip, 2f);
            Handles.SphereHandleCap(0, uTip, Quaternion.identity, HandleUtility.GetHandleSize(uTip) * 0.04f, EventType.Repaint);

            Handles.color = new Color(0.35f, 0.95f, 0.95f, 0.7f);
            Handles.DrawDottedLine(origin, vTip, 2f);
            Handles.SphereHandleCap(0, vTip, Quaternion.identity, HandleUtility.GetHandleSize(vTip) * 0.04f, EventType.Repaint);

            return changed;
        }
        #endregion

        #region Primitives
        private static void DrawTickRing(Vector3 origin, Vector3 normal, float r, float stepDeg, float lenScale, Vector3 refDir)
        {
            Handles.color = new Color(1, 1, 1, 0.35f);
            for (float a = 0; a < 360f; a += stepDeg)
            {
                Vector3 d = Quaternion.AngleAxis(a, normal) * refDir;
                Vector3 p0 = origin + d * (r * (1f - lenScale));
                Vector3 p1 = origin + d * (r * (1f - lenScale * 0.2f));
                Handles.DrawAAPolyLine(kThin, new[] { p0, p1 });
            }
        }

        private static void DrawSector(Vector3 origin, Vector3 normal, float r, float low, float high, Color fill, Vector3 refDir)
        {
            float sweep = Mathf.DeltaAngle(low, high);
            if (sweep < 0f) sweep += 360f;
            sweep = Mathf.Clamp(sweep, 0f, 359f);

            Handles.color = fill;
            Handles.DrawSolidArc(origin, normal, Quaternion.AngleAxis(low, normal) * refDir, sweep, r);
            Handles.color = new Color(fill.r, fill.g, fill.b, 0.8f);
            Handles.DrawWireArc(origin, normal, Quaternion.AngleAxis(low, normal) * refDir, sweep, r);
        }

        private static float AngleHandle(Vector3 origin, Vector3 normal, float r, float angleDeg, string label, Vector3 refDir)
        {
            Vector3 dir = Quaternion.AngleAxis(angleDeg, normal) * refDir;
            Vector3 tip = origin + dir * r;
            float size = HandleUtility.GetHandleSize(tip) * 0.055f;

            EditorGUI.BeginChangeCheck();
            Vector3 moved = Handles.FreeMoveHandle(tip, size, Vector3.zero, Handles.SphereHandleCap);
            if (EditorGUI.EndChangeCheck())
            {
                Vector3 onPlane = Vector3.ProjectOnPlane(moved - origin, normal).normalized;
                float signed = Vector3.SignedAngle(refDir, onPlane, normal);
                angleDeg = Mathf.Repeat(signed + 540f, 360f) - 180f;
            }

            Handles.DrawAAPolyLine(kThick, new[] { origin, tip });
            DrawBadge(tip, $"{label} {angleDeg:0}°");
            return angleDeg;
        }

        private static float RadialAngleHandle(Vector3 origin, Vector3 planeNormal, Vector3 axisW, float r, float angle, string label)
        {
            Vector3 dir = Vector3.Normalize(Vector3.Cross(planeNormal, axisW));
            if (dir.sqrMagnitude < 1e-6f) dir = Vector3.right;

            Vector3 tip = origin + (Quaternion.AngleAxis(angle, planeNormal) * dir) * r;
            float sz = HandleUtility.GetHandleSize(tip) * 0.038f;

            EditorGUI.BeginChangeCheck();
            Vector3 moved = Handles.FreeMoveHandle(tip, sz, Vector3.zero, Handles.SphereHandleCap);
            if (EditorGUI.EndChangeCheck())
            {
                Vector3 onPlane = Vector3.ProjectOnPlane(moved - origin, planeNormal).normalized;
                float signed = Vector3.SignedAngle(dir, onPlane, planeNormal);
                angle = Mathf.Abs(signed);
            }

            Handles.DrawAAPolyLine(kThick, new[] { origin, tip });
            DrawBadge(tip, $"{label} {angle:0}°");
            return angle;
        }

        private static void DrawBadge(Vector3 worldPos, string text)
        {
            Handles.BeginGUI();
            Vector2 p = HandleUtility.WorldToGUIPoint(worldPos);
            Rect r = new Rect(p.x + 6, p.y - 12, 80, 18);
            GUI.Box(r, text);
            Handles.EndGUI();
        }

        private static void DrawTargetGlyph(Vector3 worldPos, float size)
        {
            Vector3 r = Vector3.right * size;
            Vector3 u = Vector3.up * size;
            Handles.DrawWireDisc(worldPos, SceneView.currentDrawingSceneView ? SceneView.currentDrawingSceneView.camera.transform.forward : Vector3.forward, size * 0.9f);
            Handles.DrawLine(worldPos - r, worldPos + r);
            Handles.DrawLine(worldPos - u, worldPos + u);
        }

        private static void DrawSwingWedge(Vector3 origin, Vector3 planeNormal, Vector3 axisW, float r, float angle, Color fill)
        {
            Vector3 dir = Vector3.Normalize(Vector3.Cross(planeNormal, axisW));
            if (dir.sqrMagnitude < 1e-6f) dir = Vector3.right;

            Handles.color = fill;
            Handles.DrawSolidArc(origin, planeNormal, Quaternion.AngleAxis(-angle, planeNormal) * dir, angle * 2f, r);
            Handles.color = new Color(fill.r, fill.g, fill.b, 0.8f);
            Handles.DrawWireArc(origin, planeNormal, Quaternion.AngleAxis(-angle, planeNormal) * dir, angle * 2f, r);
        }

        private static void DrawCenterGlyph(Vector3 origin, Vector3 twistW, float r)
        {
            Vector3 u = Vector3.Normalize(Vector3.Cross(twistW, Vector3.up));
            if (u.sqrMagnitude < 1e-6f) u = Vector3.right;
            Vector3 v = Vector3.Normalize(Vector3.Cross(twistW, u));

            Handles.color = new Color(1, 1, 1, 0.4f);
            Handles.DrawWireDisc(origin, twistW, r * 0.9f);
            Handles.DrawAAPolyLine(kThin, new[] { origin - u * r, origin + u * r });
            Handles.DrawAAPolyLine(kThin, new[] { origin - v * r, origin + v * r });
        }
        #endregion

    }
}
#endif
