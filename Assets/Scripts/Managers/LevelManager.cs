using UnityEngine;
using UnityEngine.UIElements;

namespace Managers
{
    [System.Serializable]
    public struct RiddleData
    {
        public string encrypted;
        public string decrypted;
        public string key;
    }

    public class LevelManager : BaseManager
    {
        public int Level { get; private set; } = 1;
        
        [Header("Settings")]
        [SerializeField] private VisualTreeAsset[] levelCiphers;
        [SerializeField] private RiddleData[] levelRiddles;
        [SerializeField] private Sprite[] morseLampSprites;

        public Sprite[] GetMorseLampSprites() => morseLampSprites;

        public override void Init() { }

        public VisualTreeAsset GetCurrentLevelUI()
        {
            var index = Level - 1;
            return (levelCiphers != null && index >= 0 && index < levelCiphers.Length) ? levelCiphers[index] : null;
        }

        public (string encrypted, string decrypted, string key) GetLevelRiddle()
        {
            var index = Level - 1;
            
            if (levelRiddles != null && index >= 0 && index < levelRiddles.Length)
            {
                var e = levelRiddles[index].encrypted;
                var d = levelRiddles[index].decrypted;
                var k = levelRiddles[index].key;

                if (!string.IsNullOrEmpty(e) && !string.IsNullOrEmpty(d))
                {
                    return (e, d, k);
                }
            }

            return Level switch
            {
                1 => ("ТЭПЯЭ ЮЭХОЪЭРОБК Р ОСУЬБАБРЭ", "ДОБРО ПОЖАЛОВАТЬ В АГЕНТСТВО", ""),
                2 => ("ЕНЯАЬАН Ц ЁЦВЮ ПИСНЛА ЯТШЮТАИ", "ЧАСТОТА И ШИФР ВЫДАЮТ СЕКРЕТЫ", ""),
                3 => ("СРЭДХ ЛОРЭЪСГ ЭЯН ПОЦЭЪМНМЭЛЪМ", "НОВЫЙ УРОВЕНЬ ВАС ПРИВЕТСТВУЕТ", ""),
                4 => ("GUR CUNAGBZ FRRF LBH", "THE PHANTOM SEES YOU", ""),
                5 => ("РЙВИАОСЫЛРНСРЛДАИТХКЕБА", "РАСКРОЙСЛЕДЫВЛАБИРИНТАХ", ""),
                6 => ("34 36 36 15 24 14 16 12 21 55", "КООРДИНАТЫ", ""),
                7 => ("... .. --. -. .- .-..", "СИГНАЛ", ""),
                8 => ("FIP NHUFY", "KEY FOUND", "VERITAS"),
                9 => ("XEX KRQHY", "THE TRUTH", "EXTRAWORD"),
                10 => ("44, 21, 5", "RSA_PREP", "MATH_LOCK"),
                11 => ("10101010", "11110000", "01011010"),
                12 => ("SDES", "SDES_SOLVED", "1010000010"),
                13 => ("MD5_HASH", "pX9#vL2!mR8*Q", ""),
                14 => ("A3F8B2C4", "DATA_PACKET_#42", "84926"),
                15 => ("N=143,E=7", "PROJECT_PHANTOM_SOURCE", "103"),
                _ => ("AAAA", "BBB", "")
            };
        }

        public void SetLevel(int level)
        {
            if (level < 1 || level > 15) return;
            Level = level;
        }
    }
}