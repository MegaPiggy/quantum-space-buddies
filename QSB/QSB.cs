﻿using OWML.Common;
using OWML.ModHelper;
using QSB.Events;
using QSB.Utility;
using UnityEngine;
using UnityEngine.Networking;

namespace QSB
{
    public class QSB : ModBehaviour
    {
        public static IModHelper Helper;
        public static string DefaultServerIP;
        public static bool DebugMode;
        public static bool WokenUp;

        private void Awake()
        {
            Application.runInBackground = true;
        }

        private void Start()
        {
            Helper = ModHelper;

            gameObject.AddComponent<DebugLog>();
            gameObject.AddComponent<QSBNetworkManager>();
            gameObject.AddComponent<NetworkManagerHUD>();
            gameObject.AddComponent<DebugActions>();
            gameObject.AddComponent<PlayerStatesRequest>();
            gameObject.AddComponent<UnityHelper>();

            GlobalMessenger.AddListener("RestartTimeLoop", OnLoopStart);
            GlobalMessenger.AddListener("WakeUp", OnWakeUp);
        }

        private void OnWakeUp()
        {
            WokenUp = true;
            PlayerStatesRequest.Instance.Request();
        }

        private void OnLoopStart()
        {
            WokenUp = false;
        }

        public override void Configure(IModConfig config)
        {
            DefaultServerIP = config.GetSettingsValue<string>("defaultServerIP");
            DebugMode = config.GetSettingsValue<bool>("debugMode");
        }
    }
}
