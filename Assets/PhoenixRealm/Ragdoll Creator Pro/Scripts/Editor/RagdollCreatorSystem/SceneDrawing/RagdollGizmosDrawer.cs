#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;

namespace PhoenixRealm.RagdollCreatorPro
{
    internal static class RagdollGizmosDrawer
    {
        #region Vars + Properties
        private static Mesh s_sphereMesh;
        private static Mesh s_cubeMesh;
        private static Mesh s_capsuleMesh;
        private static Material s_solidMat;
        #endregion

        #region Custom Functions
        public static void EnsureInit()
        {
            if (s_solidMat == null)
            {
                var shader = Shader.Find("Hidden/Internal-Colored");
                s_solidMat = new Material(shader)
                {
                    hideFlags = HideFlags.HideAndDontSave
                };
                s_solidMat.SetInt("_ZTest", (int)CompareFunction.LessEqual);
                s_solidMat.SetInt("_Cull", (int)CullMode.Back);
                s_solidMat.SetInt("_ZWrite", 0);
            }

            if (s_sphereMesh == null) s_sphereMesh = GetPrimitiveMesh(PrimitiveType.Sphere);
            if (s_cubeMesh == null) s_cubeMesh = GetPrimitiveMesh(PrimitiveType.Cube);
            if (s_capsuleMesh == null) s_capsuleMesh = GetPrimitiveMesh(PrimitiveType.Capsule);
        }

        public static void DrawSphereSolid(Vector3 position, Quaternion rotation, float radius, Color color, bool xray = false)
        {
            if (radius <= 0f) return;
            EnsureInit();
            var matrix = Matrix4x4.TRS(position, rotation, Vector3.one * (radius * 2f));
            DrawMesh(s_sphereMesh, matrix, color, xray);
        }

        public static void DrawBoxSolid(Vector3 position, Quaternion rotation, Vector3 size, Color color, bool xray = false)
        {
            if (size.sqrMagnitude <= 1e-6f) return;
            EnsureInit();
            var matrix = Matrix4x4.TRS(position, rotation, size);
            DrawMesh(s_cubeMesh, matrix, color, xray);
        }

        public static void DrawCapsuleSolid(Vector3 position, Quaternion rotation, float radius, float height, Color color, bool xray = false)
        {
            if (radius <= 0f || height <= 0f) return;
            EnsureInit();

            float totalHeight = Mathf.Max(height + radius * 2f, radius * 2f);
            var scale = new Vector3(radius * 2f, totalHeight * 0.5f, radius * 2f);
            var matrix = Matrix4x4.TRS(position, rotation, scale);
            DrawMesh(s_capsuleMesh, matrix, color, xray);
        }

        /// Unity 6 replacement for the wire capsule outline (Y-up). Uses arcs + lines.
        public static void DrawCapsuleWire(Vector3 position, Quaternion rotation, float radius, float straightHeight, Color color, float alphaMultiplier = 1f)
        {
            if (radius <= 0f) return;

            Color drawColor = new Color(color.r, color.g, color.b, Mathf.Clamp01(color.a * alphaMultiplier));

            using (new Handles.DrawingScope(drawColor, Matrix4x4.TRS(position, rotation, Vector3.one)))
            {
                float h = Mathf.Max(0f, straightHeight);
                Vector3 top = Vector3.up * (h * 0.5f);
                Vector3 bottom = -top;

                Handles.DrawWireDisc(top, Vector3.up, radius);
                Handles.DrawWireDisc(bottom, Vector3.up, radius);

                Handles.DrawLine(bottom + Vector3.right * radius, top + Vector3.right * radius);
                Handles.DrawLine(bottom - Vector3.right * radius, top - Vector3.right * radius);
                Handles.DrawLine(bottom + Vector3.forward * radius, top + Vector3.forward * radius);
                Handles.DrawLine(bottom - Vector3.forward * radius, top - Vector3.forward * radius);

                Handles.DrawWireArc(top, Vector3.right, Vector3.forward, 180f, radius);
                Handles.DrawWireArc(top, Vector3.forward, Vector3.right, 180f, radius);
                Handles.DrawWireArc(bottom, Vector3.right, -Vector3.forward, 180f, radius);
                Handles.DrawWireArc(bottom, Vector3.forward, -Vector3.right, 180f, radius);
            }
        }

        private static void DrawMesh(Mesh mesh, Matrix4x4 matrix, Color color, bool xray)
        {
            if (mesh == null) return;
            EnsureInit();

            // Temporarily override ZTest for X-Ray draw
            int prevZ = s_solidMat.GetInt("_ZTest");
            if (xray) s_solidMat.SetInt("_ZTest", (int)CompareFunction.Always);

            s_solidMat.SetColor("_Color", color);
            s_solidMat.SetPass(0);
            Graphics.DrawMeshNow(mesh, matrix);

            if (xray) s_solidMat.SetInt("_ZTest", prevZ);
        }

        private static Mesh GetPrimitiveMesh(PrimitiveType type)
        {
            GameObject temp = GameObject.CreatePrimitive(type);
            var mesh = temp.GetComponent<MeshFilter>().sharedMesh;
            Object.DestroyImmediate(temp);
            return mesh;
        }

        #endregion

        #region Custom Functions (Joint visuals)
        public static void DrawAxisArrow(Vector3 origin, Vector3 dir, float size)
        {
            Vector3 tip = origin + dir.normalized * size;
            Handles.DrawLine(origin, tip);
            Handles.ConeHandleCap(0, tip, Quaternion.LookRotation(dir), size * 0.18f, EventType.Repaint);
        }

        /// <summary>Simple wire cone preview for swing context.</summary>
        public static void DrawWireCone(Vector3 origin, Vector3 axis, float angleDeg, float radius, int segments = 24)
        {
            angleDeg = Mathf.Clamp(angleDeg, 0f, 89.9f);
            float h = radius / Mathf.Tan(angleDeg * Mathf.Deg2Rad);
            Vector3 tip = origin + axis.normalized * h;

            // Rim circle
            Handles.DrawWireDisc(origin, axis, radius);

            // Spokes
            for (int i = 0; i < segments; i++)
            {
                float t0 = (i / (float)segments) * Mathf.PI * 2f;
                Vector3 rim = (Quaternion.AngleAxis(Mathf.Rad2Deg * t0, axis) * Vector3.right) * radius + origin;
                Handles.DrawLine(tip, rim);
            }
        }
        #endregion
    }
}
#endif
