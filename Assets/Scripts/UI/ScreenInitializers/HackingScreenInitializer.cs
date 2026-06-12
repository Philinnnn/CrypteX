using UnityEngine.UIElements;
using Gameplay;
using Managers;

namespace UI.ScreenInitializers
{
    public class HackingScreenInitializer : IScreenInitializer
    {
        public void Initialize(VisualElement screen)
        {
            int level = GameManager.Instance.LevelManager.Level;
            var riddle = GameManager.Instance.LevelManager.GetLevelRiddle();

            switch (level)
            {
                case 3:
                {
                    var atbash = new Gameplay.AtbashHackMinigame(
                        screen,
                        riddle.encrypted,
                        riddle.decrypted,
                        OnHackingSuccess
                    );
                    break;
                }
                case 4:
                {
                    var rot13 = new Gameplay.Rot13HackMinigame(
                        screen,
                        riddle.encrypted,
                        riddle.decrypted,
                        OnHackingSuccess
                    );
                    break;
                }
                case 5:
                {
                    var railFence = new Gameplay.RailFenceHackMinigame(
                        screen,
                        riddle.encrypted,
                        riddle.decrypted,
                        OnHackingSuccess
                    );
                    break;
                }
                case 6:
                {
                    var polybius = new Gameplay.PolybiusHackMinigame(
                        screen,
                        riddle.encrypted,
                        riddle.decrypted,
                        riddle.key,
                        OnHackingSuccess
                    );
                    break;
                }
                case 7:
                {
                    var morseCode = new Gameplay.MorseCodeHackMinigame(
                        screen,
                        riddle.decrypted,
                        OnHackingSuccess
                    );
                    break;
                }
                case 8:
                {
                    var vigenere = new Gameplay.VigenereHackMinigame(
                        screen,
                        riddle.encrypted,
                        riddle.decrypted,
                        riddle.key,
                        OnHackingSuccess
                    );
                    break;
                }
                case 9:
                {
                    var vernam = new Gameplay.VernamHackMinigame(
                        screen,
                        riddle.encrypted,
                        riddle.decrypted,
                        riddle.key,
                        OnHackingSuccess
                    );
                    break;
                }
                case 10:
                {
                    var shamir = new Gameplay.ShamirHackMinigame(
                        screen,
                        riddle.encrypted,
                        riddle.decrypted,
                        riddle.key,
                        OnHackingSuccess
                    );
                    break;
                }
                case 11:
                {
                    var xor = new Gameplay.XorHackMinigame(
                        screen,
                        riddle.encrypted,
                        riddle.decrypted,
                        riddle.key,
                        OnHackingSuccess
                    );
                    break;
                }
                case 12:
                {
                    var sdes = new Gameplay.SDESHackMinigame(
                        screen,
                        riddle.encrypted,
                        riddle.decrypted,
                        riddle.key,
                        OnHackingSuccess
                    );
                    break;
                }
                case 13:
                {
                    var md5 = new Gameplay.MD5HackMinigame(
                        screen,
                        riddle.encrypted,
                        riddle.decrypted,
                        riddle.key,
                        OnHackingSuccess
                    );
                    break;
                }
                case 14:
                {
                    var mac = new Gameplay.MACHackMinigame(
                        screen,
                        riddle.encrypted,
                        riddle.decrypted,
                        riddle.key,
                        OnHackingSuccess
                    );
                    break;
                }
                case 15:
                {
                    var rsa = new Gameplay.RSAHackMinigame(
                        screen,
                        riddle.encrypted,
                        riddle.decrypted,
                        riddle.key,
                        OnHackingSuccess
                    );
                    break;
                }
                default:
                {
                    var minigame = new Gameplay.CaesarHackMinigame(
                        screen,
                        riddle.encrypted,
                        riddle.decrypted,
                        OnHackingSuccess
                    );
                    break;
                }
            }
        }

        private void OnHackingSuccess()
        {
            GameManager.Instance.ChangeState(GameState.InDialogue);
        }
    }
}
