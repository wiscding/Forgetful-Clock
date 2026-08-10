using JHTGJ.Scene;
using UnityEngine;

namespace JHTGJ.Core
{
    public static class SaveManager
    {
        const string SaveExistsKey = "JHTGJ_SaveExists";
        const string DayKey = "JHTGJ_Day";
        const string RoomKey = "JHTGJ_Room";
        const string PhaseIndexKey = "JHTGJ_PhaseIndex";
        const string PhaseChoiceMadeKey = "JHTGJ_PhaseChoiceMade";
        const string StoryEndedKey = "JHTGJ_StoryEnded";
        const string PostCookingDiningKey = "JHTGJ_PostCookingDining";

        public static bool HasSave()
        {
            return PlayerPrefs.GetInt(SaveExistsKey, 0) == 1;
        }

        public static bool HasContinuableSave()
        {
            return TryLoad(out _, out _, out _, out _, out var storyEnded) && !storyEnded;
        }

        public static void SaveGame(int day, RoomType room)
        {
            SaveGame(day, room, 0, false, false);
        }

        public static void SaveGame(int day, RoomType room, int phaseIndex, bool phaseChoiceMade, bool storyEnded)
        {
            PlayerPrefs.SetInt(SaveExistsKey, 1);
            PlayerPrefs.SetInt(DayKey, day);
            PlayerPrefs.SetInt(RoomKey, (int)room);
            PlayerPrefs.SetInt(PhaseIndexKey, phaseIndex);
            PlayerPrefs.SetInt(PhaseChoiceMadeKey, phaseChoiceMade ? 1 : 0);
            PlayerPrefs.SetInt(StoryEndedKey, storyEnded ? 1 : 0);
            PlayerPrefs.Save();
        }

        public static bool IsPostCookingDiningActive() =>
            PlayerPrefs.GetInt(PostCookingDiningKey, 0) == 1;

        public static void SetPostCookingDiningActive(bool active)
        {
            PlayerPrefs.SetInt(PostCookingDiningKey, active ? 1 : 0);
            PlayerPrefs.Save();
        }

        public static bool TryLoad(out int day, out RoomType room)
        {
            return TryLoad(out day, out room, out _, out _, out _);
        }

        public static bool TryLoad(
            out int day,
            out RoomType room,
            out int phaseIndex,
            out bool phaseChoiceMade,
            out bool storyEnded)
        {
            day = 1;
            room = RoomType.FrontHall;
            phaseIndex = 0;
            phaseChoiceMade = false;
            storyEnded = false;

            if (!HasSave())
                return false;

            day = Mathf.Max(1, PlayerPrefs.GetInt(DayKey, 1));
            room = NormalizeRoom(PlayerPrefs.GetInt(RoomKey, (int)RoomType.Bedroom));
            phaseIndex = Mathf.Max(0, PlayerPrefs.GetInt(PhaseIndexKey, 0));
            phaseChoiceMade = PlayerPrefs.GetInt(PhaseChoiceMadeKey, 0) == 1;
            storyEnded = PlayerPrefs.GetInt(StoryEndedKey, 0) == 1;
            return true;
        }

        public static void DeleteSave()
        {
            PlayerPrefs.DeleteKey(SaveExistsKey);
            PlayerPrefs.DeleteKey(DayKey);
            PlayerPrefs.DeleteKey(RoomKey);
            PlayerPrefs.DeleteKey(PhaseIndexKey);
            PlayerPrefs.DeleteKey(PhaseChoiceMadeKey);
            PlayerPrefs.DeleteKey(StoryEndedKey);
            PlayerPrefs.DeleteKey(PostCookingDiningKey);
            PlayerPrefs.Save();
        }

        static RoomType NormalizeRoom(int rawValue)
        {
            return System.Enum.IsDefined(typeof(RoomType), rawValue)
                ? (RoomType)rawValue
                : RoomType.Bedroom;
        }
    }
}
