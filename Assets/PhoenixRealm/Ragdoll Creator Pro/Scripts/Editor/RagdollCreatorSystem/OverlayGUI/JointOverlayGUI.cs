#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace PhoenixRealm.RagdollCreatorPro.Editor
{
    internal class JointOverlayUI : IOverlayTabUI
    {
        #region Vars + Properties

        private Vector3Field m_anchorField;
        private Foldout m_advancedAxisFoldout;
        private Vector3Field m_axisVectorField;
        private Toggle m_projectionToggle;
        private Toggle m_preprocessingToggle;

        // Individual limit sliders and fields
        private Slider m_lowTwistSlider;
        private FloatField m_lowTwistField;
        private Slider m_highTwistSlider;
        private FloatField m_highTwistField;
        private Slider m_swing1Slider;
        private FloatField m_swing1Field;
        private Slider m_swing2Slider;
        private FloatField m_swing2Field;

        // State tracking
        private CustomNode m_currentNode;
        private Transform m_currentBone;
        private bool m_isUpdating = false;
        private bool m_isInitialized = false;
        private bool m_callbacksRegistered = false;

        public bool IsInitialized => m_isInitialized;

        #endregion

        #region IOverlayTabUI Implementation

        public void Initialize(VisualElement tabPanel)
        {
            if (tabPanel == null)
            {
                Debug.LogError("[JointOverlayUI] Initialize: tabPanel is null!");
                return;
            }

            m_anchorField = tabPanel.Q<Vector3Field>("anchor-field");
            m_advancedAxisFoldout = tabPanel.Q<Foldout>("advanced-axis-foldout");
            m_axisVectorField = tabPanel.Q<Vector3Field>("axis-vector-field");
            m_projectionToggle = tabPanel.Q<Toggle>("projection-toggle");
            m_preprocessingToggle = tabPanel.Q<Toggle>("preprocessing-toggle");

            m_lowTwistSlider = tabPanel.Q<Slider>("low-twist-slider");
            m_lowTwistField = tabPanel.Q<FloatField>("low-twist-field");
            m_highTwistSlider = tabPanel.Q<Slider>("high-twist-slider");
            m_highTwistField = tabPanel.Q<FloatField>("high-twist-field");
            m_swing1Slider = tabPanel.Q<Slider>("swing1-slider");
            m_swing1Field = tabPanel.Q<FloatField>("swing1-field");
            m_swing2Slider = tabPanel.Q<Slider>("swing2-slider");
            m_swing2Field = tabPanel.Q<FloatField>("swing2-field");

            m_isInitialized = (m_anchorField != null);
        }

        public void UpdateContent(VisualElement tabPanel, RagdollMakerContext ctx, CustomNode node)
        {
            if (!m_isInitialized || m_anchorField == null)
            {
                Initialize(tabPanel);
                if (!m_isInitialized || m_anchorField == null)
                {
                    Debug.LogError("[JointOverlayUI] UpdateContent: Failed to initialize UI elements. Cannot update content.");
                    return;
                }
            }

            if (node == null || node.Transform == null)
            {
                Debug.LogError($"[JointOverlayUI] UpdateContent: Missing required elements - node: {node != null}, bone: {node?.Transform != null}");
                return;
            }

            m_isUpdating = true;
            m_callbacksRegistered = false;
            m_currentNode = node;
            m_currentBone = node.Transform;

            m_anchorField.SetValueWithoutNotify(node.JointAnchorLocal);

            if (m_axisVectorField != null)
            {
                m_axisVectorField.SetValueWithoutNotify(node.JointAxisLocal);
            }

            UpdateLimitControls(node, node.Transform);

            if (m_projectionToggle != null)
            {
                m_projectionToggle.SetValueWithoutNotify(node.JointEnableProjection);
            }

            if (m_preprocessingToggle != null)
            {
                m_preprocessingToggle.SetValueWithoutNotify(node.JointEnablePreprocessing);
            }

            m_isUpdating = false;
            RegisterCallbacks(node, node.Transform);
            m_callbacksRegistered = true;
        }

        public void Cleanup()
        {
            m_callbacksRegistered = false;
            m_isInitialized = false;
            m_isUpdating = false;
            m_currentNode = null;
            m_currentBone = null;
        }

        #endregion

        #region Private Methods

        private void RegisterCallbacks(CustomNode node, Transform bone)
        {
            if (m_callbacksRegistered) return;

            if (m_anchorField != null)
            {
                m_anchorField.RegisterValueChangedCallback(evt =>
                {
                    if (!m_isUpdating && m_currentNode == node && m_currentBone == bone)
                    {
                        Undo.RecordObject(bone, "Edit Joint Anchor");
                        node.JointAnchorLocal = evt.newValue;
                    }
                });
            }

            if (m_axisVectorField != null)
            {
                m_axisVectorField.RegisterValueChangedCallback(evt =>
                {
                    if (!m_isUpdating && m_currentNode == node && m_currentBone == bone)
                    {
                        Undo.RecordObject(bone, "Edit Joint Axis");
                        node.JointAxisLocal = evt.newValue;
                    }
                });
            }

            if (m_projectionToggle != null)
            {
                m_projectionToggle.RegisterValueChangedCallback(evt =>
                {
                    if (!m_isUpdating && m_currentNode == node && m_currentBone == bone)
                    {
                        Undo.RecordObject(bone, "Edit Joint Projection");
                        node.JointEnableProjection = evt.newValue;
                    }
                });
            }

            if (m_preprocessingToggle != null)
            {
                m_preprocessingToggle.RegisterValueChangedCallback(evt =>
                {
                    if (!m_isUpdating && m_currentNode == node && m_currentBone == bone)
                    {
                        Undo.RecordObject(bone, "Edit Joint Preprocessing");
                        node.JointEnablePreprocessing = evt.newValue;
                    }
                });
            }
        }

        private void UpdateLimitControls(CustomNode node, Transform bone)
        {
            var limits = node.JointLimits;

            // Low Twist Limit
            if (m_lowTwistSlider != null && m_lowTwistField != null)
            {
                float lowTwistValue = limits.lowTwistLimit.Limit;
                m_lowTwistSlider.SetValueWithoutNotify(lowTwistValue);
                m_lowTwistField.SetValueWithoutNotify(lowTwistValue);

                if (!m_callbacksRegistered)
                {
                    m_lowTwistSlider.RegisterValueChangedCallback(evt =>
                    {
                        if (!m_isUpdating && m_currentNode == node && m_currentBone == bone)
                        {
                            float newValue = Mathf.Min(evt.newValue, m_currentNode.JointLimits.highTwistLimit.Limit - 0.1f);
                            UpdateJointLimit(bone, node, "lowTwist", newValue);
                            m_lowTwistField.SetValueWithoutNotify(newValue);
                        }
                    });

                    m_lowTwistField.RegisterValueChangedCallback(evt =>
                    {
                        if (!m_isUpdating && m_currentNode == node && m_currentBone == bone)
                        {
                            float newValue = Mathf.Clamp(evt.newValue, -180f, 0f);
                            newValue = Mathf.Min(newValue, m_currentNode.JointLimits.highTwistLimit.Limit - 0.1f);
                            UpdateJointLimit(bone, node, "lowTwist", newValue);
                            m_lowTwistSlider.SetValueWithoutNotify(newValue);
                        }
                    });
                }
            }

            // High Twist Limit
            if (m_highTwistSlider != null && m_highTwistField != null)
            {
                float highTwistValue = limits.highTwistLimit.Limit;
                m_highTwistSlider.SetValueWithoutNotify(highTwistValue);
                m_highTwistField.SetValueWithoutNotify(highTwistValue);

                if (!m_callbacksRegistered)
                {
                    m_highTwistSlider.RegisterValueChangedCallback(evt =>
                    {
                        if (!m_isUpdating && m_currentNode == node && m_currentBone == bone)
                        {
                            float newValue = Mathf.Max(evt.newValue, m_currentNode.JointLimits.lowTwistLimit.Limit + 0.1f);
                            UpdateJointLimit(bone, node, "highTwist", newValue);
                            m_highTwistField.SetValueWithoutNotify(newValue);
                        }
                    });

                    m_highTwistField.RegisterValueChangedCallback(evt =>
                    {
                        if (!m_isUpdating && m_currentNode == node && m_currentBone == bone)
                        {
                            float newValue = Mathf.Clamp(evt.newValue, 0f, 180f);
                            newValue = Mathf.Max(newValue, m_currentNode.JointLimits.lowTwistLimit.Limit + 0.1f);
                            UpdateJointLimit(bone, node, "highTwist", newValue);
                            m_highTwistSlider.SetValueWithoutNotify(newValue);
                        }
                    });
                }
            }

            // Swing 1 Limit
            if (m_swing1Slider != null && m_swing1Field != null)
            {
                float swing1Value = limits.swing1Limit.Limit;
                m_swing1Slider.SetValueWithoutNotify(swing1Value);
                m_swing1Field.SetValueWithoutNotify(swing1Value);

                if (!m_callbacksRegistered)
                {
                    m_swing1Slider.RegisterValueChangedCallback(evt =>
                    {
                        if (!m_isUpdating && m_currentNode == node && m_currentBone == bone)
                        {
                            float newValue = Mathf.Clamp(evt.newValue, 0f, 179f);
                            UpdateJointLimit(bone, node, "swing1", newValue);
                            m_swing1Field.SetValueWithoutNotify(newValue);
                        }
                    });

                    m_swing1Field.RegisterValueChangedCallback(evt =>
                    {
                        if (!m_isUpdating && m_currentNode == node && m_currentBone == bone)
                        {
                            float newValue = Mathf.Clamp(evt.newValue, 0f, 179f);
                            UpdateJointLimit(bone, node, "swing1", newValue);
                            m_swing1Slider.SetValueWithoutNotify(newValue);
                        }
                    });
                }
            }

            // Swing 2 Limit
            if (m_swing2Slider != null && m_swing2Field != null)
            {
                float swing2Value = limits.swing2Limit.Limit;
                m_swing2Slider.SetValueWithoutNotify(swing2Value);
                m_swing2Field.SetValueWithoutNotify(swing2Value);

                if (!m_callbacksRegistered)
                {
                    m_swing2Slider.RegisterValueChangedCallback(evt =>
                    {
                        if (!m_isUpdating && m_currentNode == node && m_currentBone == bone)
                        {
                            float newValue = Mathf.Clamp(evt.newValue, 0f, 179f);
                            UpdateJointLimit(bone, node, "swing2", newValue);
                            m_swing2Field.SetValueWithoutNotify(newValue);
                        }
                    });

                    m_swing2Field.RegisterValueChangedCallback(evt =>
                    {
                        if (!m_isUpdating && m_currentNode == node && m_currentBone == bone)
                        {
                            float newValue = Mathf.Clamp(evt.newValue, 0f, 179f);
                            UpdateJointLimit(bone, node, "swing2", newValue);
                            m_swing2Slider.SetValueWithoutNotify(newValue);
                        }
                    });
                }
            }
        }

        private void UpdateJointLimit(Transform bone, CustomNode node, string limitType, float value)
        {
            Undo.RecordObject(bone, $"Edit {limitType} Limit");
            var newLimits = node.JointLimits.Clone();

            switch (limitType)
            {
                case "lowTwist":
                    newLimits.lowTwistLimit.Limit = value;
                    break;
                case "highTwist":
                    newLimits.highTwistLimit.Limit = value;
                    break;
                case "swing1":
                    newLimits.swing1Limit.Limit = value;
                    break;
                case "swing2":
                    newLimits.swing2Limit.Limit = value;
                    break;
            }

            node.JointLimits = newLimits;
        }

        #endregion
    }
}
#endif
