using UnityEngine.UIElements;

namespace Gameplay
{
    public class AtbashUIElements
    {
        public Label EncryptedText { get; private set; }
        public Label DecryptedText { get; private set; }
        public Label TopAlphabet { get; private set; }
        public Label BottomAlphabet { get; private set; }
        public Label CipherLabel { get; private set; }
        public Label AnswerText { get; private set; }
        public Button SubmitButton { get; private set; }
        public Button DeleteButton { get; private set; }
        public VisualElement LettersPanel { get; private set; }
        public VisualElement DemoLetterPairContainer { get; private set; }
        public VisualElement InputLabel { get; private set; }
        public VisualElement ButtonsContainer { get; private set; }

        public AtbashUIElements(VisualElement root)
        {
            EncryptedText = root.Q<Label>("EncryptedText");
            DecryptedText = root.Q<Label>("DecryptedText");
            TopAlphabet = root.Q<Label>("TopAlphabet");
            BottomAlphabet = root.Q<Label>("BottomAlphabet");
            CipherLabel = root.Q<Label>("CipherLabel");
            AnswerText = root.Q<Label>("AnswerText");
            SubmitButton = root.Q<Button>("SubmitButton");
            DeleteButton = root.Q<Button>("DeleteButton");
            LettersPanel = root.Q<VisualElement>("LettersPanel");
            DemoLetterPairContainer = root.Q<VisualElement>("DemoLetterPairContainer");
            InputLabel = root.Q<VisualElement>("InputLabel");
            ButtonsContainer = root.Q<VisualElement>("ButtonsContainer");
        }
    }
}

