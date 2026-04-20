using System;

namespace AI
{
    internal enum GameDifficulty
    {
        Easy,
        Medium,
        Hard
    }

    internal static class AppState
    {
        public static bool IsMuted { get; set; } = false;
        public static GameDifficulty Difficulty { get; set; } = GameDifficulty.Easy;
        public static bool GoHome { get; set; } = false;

        public static string GetDifficultyText()
        {
            return Difficulty switch
            {
                GameDifficulty.Medium => "Máy (Trung bình)",
                GameDifficulty.Hard => "Máy (Khó)",
                _ => "Máy (Dễ)"
            };
        }

        public static void PlayUiClickSound()
        {
            if (IsMuted)
            {
                return;
            }

            try
            {
                SoundManager.PlayClick();
            }
            catch
            {
            }
        }

        public static void PlayToggleOnSound()
        {
            try
            {
                SoundManager.PlayClick();
            }
            catch
            {
            }
        }
    }
}
