using JHTGJ.Scene;
using JHTGJ.Story;
using UnityEngine;

namespace JHTGJ.Core
{
    public static class GameSession
    {
        public enum StartMode
        {
            None,
            NewGame,
            Continue
        }

        const string PendingModeKey = "JHTGJ_PendingStartMode";
        const string PendingRoomKey = "JHTGJ_PendingRoom";
        const string PendingOpeningCgKey = "JHTGJ_PendingOpeningCg";

        public static void RequestNewGame()
        {
            SaveManager.DeleteSave();
            PlayerPrefs.SetInt(PendingModeKey, (int)StartMode.NewGame);
            PlayerPrefs.SetInt(PendingRoomKey, (int)RoomType.Bedroom);
            PlayerPrefs.SetInt(PendingOpeningCgKey, 1);
            PlayerPrefs.Save();
        }

        public static bool HasPendingOpeningCg() =>
            PlayerPrefs.GetInt(PendingOpeningCgKey, 0) == 1;

        public static bool TryConsumeOpeningCg()
        {
            if (!HasPendingOpeningCg())
                return false;

            PlayerPrefs.DeleteKey(PendingOpeningCgKey);
            PlayerPrefs.Save();
            return true;
        }

        public static bool RequestContinue()
        {
            if (!SaveManager.HasContinuableSave())
                return false;

            if (!SaveManager.TryLoad(out _, out var room, out _, out _, out _))
                return false;

            PlayerPrefs.SetInt(PendingModeKey, (int)StartMode.Continue);
            PlayerPrefs.SetInt(PendingRoomKey, (int)room);
            PlayerPrefs.DeleteKey(PendingOpeningCgKey);
            PlayerPrefs.Save();
            return true;
        }

        public static RoomType ResolveStartRoom(RoomType defaultRoom)
        {
            if (TryConsumeStart(out var pendingRoom))
                return pendingRoom;

            if (SaveManager.TryLoad(out _, out var savedRoom, out _, out _, out var storyEnded) && !storyEnded)
                return savedRoom;

            return defaultRoom;
        }

        public static bool ShouldWaitForOpeningCg()
        {
            return HasPendingOpeningCg()
                || (OpeningCgPlayer.Instance != null && OpeningCgPlayer.Instance.IsPlaying);
        }

        public static bool TryConsumeStart(out RoomType room)
        {
            room = RoomType.FrontHall;
            var mode = (StartMode)PlayerPrefs.GetInt(PendingModeKey, (int)StartMode.None);
            if (mode == StartMode.None)
                return false;

            room = (RoomType)PlayerPrefs.GetInt(PendingRoomKey, (int)RoomType.FrontHall);
            PlayerPrefs.DeleteKey(PendingModeKey);
            PlayerPrefs.DeleteKey(PendingRoomKey);
            PlayerPrefs.Save();
            return true;
        }
    }
}
