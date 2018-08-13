using System.Collections;

namespace EnhancedMapServerNetCore.Managers
{
    public static class SaveManager
    {
        public static bool IsSaving { get; private set; }

        public static void Init()
        {
            CoroutineManager.StartCoroutine(ISave());
        }

        private static IEnumerator ISave()
        {
            while (Core.IsRunning)
            {
                yield return new WaitForSeconds(IsSaving ? .5f : 5 * 60);
                Save();
            }
        }

        public static void Save()
        {
            if (IsSaving)
                return;

            IsSaving = true;
            Core.Server.Pause();

            // backup
            RoomManager.Save(true);
            AccountManager.Save(true);
            SettingsManager.Save(true);

            RoomManager.Save();
            AccountManager.Save();
            SettingsManager.Save();

            Core.Server.Resume();
            IsSaving = false;
        }
    }
}