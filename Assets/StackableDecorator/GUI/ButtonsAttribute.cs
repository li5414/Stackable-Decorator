using System.Linq;
using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace StackableDecorator
{
    public class ButtonsAttribute : StyledDecoratorAttribute
    {
        public string titles = string.Empty;
        public string icons = string.Empty;
        public string tooltips = string.Empty;
        public string actions = string.Empty;
        public string buttonStyles = null;
        public int column = -1;
        public int hOffset = 0;
        public int vOffset = 0;

        public float width = -1;
        public float height = -1;
        public bool indented = true;
        public bool below = false;
        public TextAlignment alignment = TextAlignment.Left;
#if UNITY_EDITOR
        protected override string m_defaultStyle { get { return "button"; } }

        private class DynamicContent
        {
            public DynamicValue<string> Title;
            public DynamicValue<string> Icon;
            public DynamicValue<string> Tooltip;
        }

        private float m_Left;
        private float m_Right;
        private float m_Top;
        private float m_Bottom;

        private DynamicContent[] m_DynamicContents = null;
        private DynamicAction[] m_DynamicActions = null;
        private GUIContent[] m_Contents = null;

        private Dictionary<string, Data> m_Data = new Dictionary<string, Data>();

        private class Data
        {
            public ButtonGroup buttonGroup = null;
            public Vector2 buttonSize;
        }
#endif
        public ButtonsAttribute(float left = 0, float right = 0, float top = 0, float bottom = 0)
        {
#if UNITY_EDITOR
            m_Left = left;
            m_Right = right;
            m_Top = top;
            m_Bottom = bottom;
#endif
        }
#if UNITY_EDITOR
        private void Update()
        {
            if (m_DynamicContents == null)
            {
                var titles = this.titles.Split(',');
                var icons = this.icons.Split(',');
                var tooltips = this.tooltips.Split(',');
                var actions = this.actions.Split(',');
                if (icons.Length < titles.Length)
                    icons = icons.Concat(Enumerable.Repeat(string.Empty, titles.Length - icons.Length)).ToArray();
                if (tooltips.Length < titles.Length)
                    tooltips = tooltips.Concat(Enumerable.Repeat(string.Empty, titles.Length - tooltips.Length)).ToArray();
                if (actions.Length < titles.Length)
                    actions = actions.Concat(Enumerable.Repeat(string.Empty, titles.Length - actions.Length)).ToArray();

                m_DynamicContents = new DynamicContent[titles.Length];
                m_DynamicActions = new DynamicAction[titles.Length];
                for (int i = 0; i < titles.Length; i++)
                {
                    m_DynamicContents[i] = new DynamicContent();
                    m_DynamicContents[i].Title = new DynamicValue<string>(titles[i], m_SerializedProperty);
                    m_DynamicContents[i].Icon = new DynamicValue<string>(icons[i], m_SerializedProperty);
                    m_DynamicContents[i].Tooltip = new DynamicValue<string>(tooltips[i], m_SerializedProperty);
                    m_DynamicActions[i] = new DynamicAction(actions[i], m_SerializedProperty);
                }

                m_Contents = titles.Select(t => new GUIContent()).ToArray();
            }

            for (int i = 0; i < m_Contents.Length; i++)
            {
                m_DynamicContents[i].Title.Update(m_SerializedProperty);
                m_DynamicContents[i].Icon.Update(m_SerializedProperty);
                m_DynamicContents[i].Tooltip.Update(m_SerializedProperty);
                m_DynamicActions[i].Update(m_SerializedProperty);

                m_Contents[i].text = m_DynamicContents[i].Title.GetValue();
                m_Contents[i].tooltip = m_DynamicContents[i].Tooltip.GetValue();
                var image = m_DynamicContents[i].Icon.GetValue();
                if (!string.IsNullOrEmpty(image))
                    m_Contents[i].image = EditorGUIUtility.IconContent(image).image;
            }
        }

        public override float GetHeight(SerializedProperty property, GUIContent label, float height)
        {
            if (!IsVisible()) return height;

            Update();
            var data = m_Data.Get(property.propertyPath);
            if (data.buttonGroup == null)
                data.buttonGroup = new ButtonGroup(m_Contents, buttonStyles == null ? m_Style.name : buttonStyles);
            data.buttonGroup.Update(m_Contents);
            data.buttonGroup.hOffset = hOffset;
            data.buttonGroup.vOffset = vOffset;
            data.buttonSize = data.buttonGroup.GetButtonSize(column);

            float button = this.height < 0 ? data.buttonSize.y : this.height;
            button += m_Top + m_Bottom;
            if (button < 0) button = 0;
            return height + button;
        }

        public override bool BeforeGUI(ref Rect position, ref SerializedProperty property, ref GUIContent label, ref bool includeChildren, bool visible)
        {
            if (!IsVisible()) return visible;
            if (!visible) return false;

            var data = m_Data.Get(property.propertyPath);

            var indent = position.indent(indented);
            var rect = new Rect(indent);

            float width = 0;
            if (this.width < 0)
                width = data.buttonSize.x;
            else if (this.width <= 1)
                width = indent.width * this.width;
            else
                width = this.width;
            rect.width = width;
            rect.height = height < 0 ? data.buttonSize.y : height;

            if (alignment == TextAlignment.Right)
            {
                rect.xMin = indent.xMax - rect.width;
                rect.xMax = indent.xMax;
            }
            else if (alignment == TextAlignment.Center)
            {
                rect.x += (indent.width - rect.width) / 2;
            }
            rect.xMin += m_Left;
            rect.xMax -= m_Right;

            if (below)
                rect.y = indent.yMax - rect.height - m_Top - m_Bottom;
            rect.y += m_Top;

            var selected = data.buttonGroup.Draw(rect, -1, column);
            if(selected !=-1)
            {
                m_DynamicActions[selected].Update(m_SerializedProperty);
                m_DynamicActions[selected].DoAction();
            }

            float button = rect.height + m_Top + m_Bottom;
            if (button < 0) button = 0;
            if (below)
                position.height -= button;
            else
                position.yMin += button;
            return true;
        }
#endif
    }
}