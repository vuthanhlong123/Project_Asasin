#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UIElements;

namespace PhoenixRealm.RagdollCreatorPro.Editor
{
    public class RagdollValidationManager
    {
        #region Vars + Properties

        private RagdollMakerContext m_ctx;

        #endregion

        #region Constructor

        public RagdollValidationManager(RagdollMakerContext ctx)
        {
            m_ctx = ctx;
        }

        #endregion

        #region Public API

        public void RefreshValidation()
        {
            if (m_ctx == null) return;

            if (HasChainsToValidate())
            {
                m_ctx.Validation = RagdollSystemValidator.ValidateChains(m_ctx.Chains);
            }
            else
            {
                m_ctx.Validation = new ValidationResult();
            }
        }

        public void BuildValidationUI(VisualElement validationContainer, VisualElement validationSection)
        {
            if (!ShouldShowValidation())
            {
                SetSectionVisibility(validationSection, false);
                return;
            }

            SetSectionVisibility(validationSection, true);
            AddValidationHeader(validationContainer);
            AddValidationMessages(validationContainer);
            AddValidationSummary(validationContainer);
        }

        #endregion

        #region Private Methods

        private bool HasChainsToValidate()
        {
            return m_ctx.Chains != null && m_ctx.Chains.Count > 0;
        }

        private bool ShouldShowValidation()
        {
            return m_ctx?.Validation != null && HasValidationMessages();
        }

        private bool HasValidationMessages()
        {
            return m_ctx.Validation.Errors.Count > 0 ||
                   m_ctx.Validation.Warnings.Count > 0 ||
                   m_ctx.Validation.Infos.Count > 0;
        }

        private void SetSectionVisibility(VisualElement section, bool visible)
        {
            if (section != null)
            {
                section.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }

        private void AddValidationHeader(VisualElement container)
        {
            var headerLabel = new Label("Validation Results");
            headerLabel.AddToClassList("validation-header");
            container.Add(headerLabel);
        }

        private void AddValidationMessages(VisualElement container)
        {
            AddValidationErrors(container);
            AddValidationWarnings(container);
            AddValidationInfos(container);
        }

        private void AddValidationErrors(VisualElement container)
        {
            foreach (var error in m_ctx.Validation.Errors)
            {
                var errorElement = CreateValidationLabel($"❌ {error}", "validation-item--error", new Color(0.8f, 0.2f, 0.2f));
                container.Add(errorElement);
            }
        }

        private void AddValidationWarnings(VisualElement container)
        {
            foreach (var warning in m_ctx.Validation.Warnings)
            {
                var warningElement = CreateValidationLabel($"⚠️ {warning}", "validation-item--warning", new Color(0.8f, 0.6f, 0.2f));
                container.Add(warningElement);
            }
        }

        private void AddValidationInfos(VisualElement container)
        {
            foreach (var info in m_ctx.Validation.Infos)
            {
                var infoElement = CreateValidationLabel($"ℹ️ {info}", "validation-item--info", new Color(0.3f, 0.6f, 0.8f));
                container.Add(infoElement);
            }
        }

        private Label CreateValidationLabel(string text, string cssClass, Color color)
        {
            var label = new Label(text);
            label.AddToClassList("validation-item");
            label.AddToClassList(cssClass);
            label.style.color = color;
            return label;
        }

        private void AddValidationSummary(VisualElement container)
        {
            if (m_ctx.Validation.HasErrors)
            {
                AddErrorSummary(container);
            }
            else if (m_ctx.Validation.HasWarnings)
            {
                AddWarningSummary(container);
            }
            else
            {
                AddSuccessSummary(container);
            }
        }

        private void AddErrorSummary(VisualElement container)
        {
            var summaryElement = CreateValidationSummary(
                "❌ Validation failed - Cannot bake ragdoll until errors are resolved",
                "validation-summary--error",
                new Color(0.8f, 0.2f, 0.2f)
            );
            container.Add(summaryElement);
        }

        private void AddWarningSummary(VisualElement container)
        {
            var summaryElement = CreateValidationSummary(
                "⚠️ Validation passed with warnings - RagdollCreatorPro can be baked",
                "validation-summary--warning",
                new Color(0.8f, 0.6f, 0.2f)
            );
            container.Add(summaryElement);
        }

        private void AddSuccessSummary(VisualElement container)
        {
            var summaryElement = CreateValidationSummary(
                "✅ Validation passed - Ready to bake ragdoll",
                "validation-summary--success",
                new Color(0.2f, 0.8f, 0.2f)
            );
            container.Add(summaryElement);
        }

        private Label CreateValidationSummary(string text, string cssClass, Color color)
        {
            var summaryElement = new Label(text);
            summaryElement.AddToClassList("validation-summary");
            summaryElement.AddToClassList(cssClass);
            summaryElement.style.color = color;
            summaryElement.style.unityFontStyleAndWeight = FontStyle.Bold;
            return summaryElement;
        }

        #endregion
    }
}
#endif
