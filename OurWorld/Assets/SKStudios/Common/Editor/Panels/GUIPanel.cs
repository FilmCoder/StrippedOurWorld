using UnityEngine;

namespace SKStudios.Common.Editor {
    public abstract class GuiPanel {
        public abstract string Title { get; }
        public abstract void OnGui(Rect position);
        public virtual void OnEnable() { }
        public virtual void OnDisable() { }

        protected Rect ApplySettingsPadding(Rect position)
        {
            position.width -= 30;
            position.x += 10;
            position.y += 10;
            position.height -= 30;

            return position;
        }
    }
}