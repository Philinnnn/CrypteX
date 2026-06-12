using UnityEngine.UIElements;

namespace UI.ScreenInitializers
{
    public interface IScreenInitializer
    {
        void Initialize(VisualElement screen);
    }
}