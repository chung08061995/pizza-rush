using UnityEngine;

public class GameConstain
{
    public class RuntimeStorage
    {
        public const string StartBooterItems = nameof(StartBooterItems);
    }
    public static class PlayerPrefsKey
    {
        public const string Level = "Level";
        public const string Star = "Star";
        public const string Gold = "Gold";
        public const string PlayerName = "PlayerName";
        public const string CurrentAvatar = "CurrentAvatar";
        public const string MusicVolume = "MusicVolume";
        public const string SfxVolume = "SfxVolume";
        public const string Vibrate = "Vibrate";
        public const string VibrationEnabled = "VibrationEnabled";
        public const string DailyChallenge_StreakDays = "DailyChallenge_StreakDays";
        public const string DailyChallenge_LastPlayedDate = "DailyChallenge_LastPlayedDate";
        public const string DailyChallenge_PlayedDates = nameof(DailyChallenge_PlayedDates);
        public const string LimitedLivesData = nameof(LimitedLivesData);
        public const string UnlimitedLivesData = nameof(UnlimitedLivesData);
        public const string SkillAddTile = nameof(SkillAddTile);
        public const string SkillDestroyContainer = nameof(SkillDestroyContainer);
        public const string Skill_FreezeTime = nameof(Skill_FreezeTime);
        public const string Skill_SplitContainer = nameof(Skill_SplitContainer);
        public const string BooterCoffeeTime = nameof(BooterCoffeeTime);
        public const string BooterMagic = nameof(BooterMagic);
        public const string LevelUpRewardClaimedPrefix = "LevelUpRewardClaimed_";
    }

    public static class SenceName
    {
        public const string LevelRunner = "LevelRunner";
        public const string Main = "Main";
        public const string Empty = "Empty";
        public const string Init = "Init";
    }
    /// <summary>
    /// Chứa các mẫu định dạng chuỗi được sử dụng trong toàn bộ dự án.
    /// </summary>
    public static class StringFormats
    {
        /// <summary>
        /// Mẫu đường dẫn cho tài nguyên cấp độ.
        /// Định dạng: "LevelData/0001", "LevelData/0002", v.v.
        /// <para>Tham số truyền vào: (int) levelIndex</para>
        /// </summary>
        public const string LevelDataPath = "LevelData/";
        public const string LevelDataFileNameFormat = LevelDataPath + "{0:D4}"; // Ví dụ: LevelData/0001

        public const string LevelDisplayFormat = "Level {0}";

        public const string BonusTimePopupLose = "+{0}s";
        public const string DescriptionBonusTimePopupLose = "Add {0} seconds to keep playing!";

        public const string DateFormat = "yyyy-MM-dd";
    }

    public class ScriptableObjectsPath
    {
        public const string Root = "CoffeeRunPuzzle/";
        public const string Item = Root + "Item/";
        public const string StringItem = Item + "StringItem/";
        public const string SpriteItem = Item + "SpriteItem/";
        public const string IntItem = Item + "IntItem/";
        public const string AudioClipItem = Item + "AudioClipItem/";
        public const string ColorMaterial = Root + "ColorMaterial/";
        public const string Color = Root + "Color/";
        public const string Skill = Root + "Skill/";
        public const string ParametterGameConfig = Root + "ParametterGameConfig/";
        public const string LevelUpReward = Root + "LevelUpReward/";
    }

}
