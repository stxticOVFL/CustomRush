using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using MelonLoader;
using UnityEngine;

namespace CustomRush
{
    public class CustomRush : MelonMod
    {
        internal static MelonLogger.Instance Logger { get; private set; }
        internal static Game Game { get { return Singleton<Game>.Instance; } }

        internal static AssetBundle bundle;
        internal static event Action<AssetBundle> OnBundleLoad;

        static CustomRush i;

        public override void OnEarlyInitializeMelon() => Logger = LoggerInstance;
        public override void OnInitializeMelon()
        {
            i = this;
            NeonLite.Settings.AddHolder("CustomRush");
            NeonLite.NeonLite.LoadModules(MelonAssembly);

#if DEBUG
            NeonLite.Modules.Anticheat.Register(MelonAssembly);
#endif
        }


        public override void OnLateInitializeMelon()
        {
            var bundleLoading = AssetBundle.LoadFromMemoryAsync(Resources.crush_prefabs);
            bundleLoading.completed += _ =>
            {
                Logger.Msg("AssetBundle loading done!");
                bundle = bundleLoading.assetBundle;
                OnBundleLoad.Invoke(bundle);
            };
        }
    }

    static class Helpers
    {
        [Conditional("DEBUG")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void DebugMsg(this MelonLogger.Instance log, string msg)
        {
            log.Msg(msg);
            UnityEngine.Debug.Log(msg);
        }

        [Conditional("DEBUG")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void DebugMsg(this MelonLogger.Instance log, object msg)
        {
            log.Msg(msg);
            UnityEngine.Debug.Log(msg);
        }
    }

    internal static class Constants
    {
        public const LevelRush.LevelRushType CUSTOM_RUSHTYPE = (LevelRush.LevelRushType)100;
    }
}
