using System.Collections.Generic;
using HideAndSeek.GameManagement;
using OWML.Common;
using QSB;
using QSB.Messaging;
using QSB.Player;
using UnityEngine;

namespace HideAndSeek{
    public static class PlayerManager{

        public static HashSet<PlayerInfo> hiders = new();
        public static HashSet<PlayerInfo> seekers = new();
        public static HashSet<PlayerInfo> spectators = new();

        public static Dictionary<PlayerInfo, HideAndSeekInfo> playerInfo = new();
        public static Dictionary<PlayerInfo, DeathType> PlayerDeathTypes = new();

        public static void Init(){
            QSBPlayerManager.OnAddPlayer += SetupPlayer;
            QSBPlayerManager.OnRemovePlayer += RemovePlayer;
        }

        public static void RemovePlayer(PlayerInfo playerInfo){
            PlayerManager.CleanUpPlayer(PlayerManager.playerInfo[playerInfo]);
            PlayerManager.playerInfo.Remove(playerInfo);
            PlayerManager.hiders.Remove(playerInfo);
            PlayerManager.seekers.Remove(playerInfo);
            PlayerManager.spectators.Remove(playerInfo);
        }

        public static void ResetAllPlayerStates() {
            foreach (var info in playerInfo.Values) {
                info.Reset();
            }
        }

        public static void SetPlayerState(PlayerInfo playerInfo, PlayerState state){
            switch (state){
                case PlayerState.Hiding:
                    hiders.Add(playerInfo);
                    seekers.Remove(playerInfo);
                    spectators.Remove(playerInfo);
                    SetupHider(PlayerManager.playerInfo[playerInfo]);
                    break;
                case PlayerState.Seeking:
                    hiders.Remove(playerInfo);
                    seekers.Add(playerInfo);
                    spectators.Remove(playerInfo);
                    SetupSeeker(PlayerManager.playerInfo[playerInfo]);
                    break;
                case  PlayerState.Spectating:
                    hiders.Remove(playerInfo);
                    seekers.Remove(playerInfo);
                    spectators.Add(playerInfo);
                    SetupSpectator(PlayerManager.playerInfo[playerInfo]);
                    break;
                case PlayerState.None:
                    Utils.WriteLine("Player state is None", MessageType.Error);
                    Reset(PlayerManager.playerInfo[playerInfo]);
                    break;
            }
        }

        //This should run once every loop to initialize everything needed for Hide and Seek
        public static void SetupPlayer(PlayerInfo playerInfo){
            HideAndSeek.instance.ModHelper.Events.Unity.RunWhen(() => playerInfo.Body != null, () => {
                Utils.WriteLine("Setting up " + playerInfo.Name + ": ", MessageType.Debug);
                HideAndSeekInfo info = playerInfo.IsLocalPlayer ? new LocalInfo() : new RemoteInfo();
                info.SetupInfo(playerInfo);
                
                if (!PlayerManager.playerInfo.ContainsKey(playerInfo)){
                    PlayerManager.playerInfo[playerInfo] =  info;
                }
                
                //Make sure each player gets the proper settings
                if (QSBCore.IsHost){ new SharedSettingsMessage(){To = info.Info.PlayerId}.Send(); }

                SetPlayerState(playerInfo, PlayerManager.playerInfo[playerInfo].State);
            });
        }

        
        public static void SetupHider(HideAndSeekInfo info){
            info.SetupHider();
        }
        
        public static void SetupSeeker(HideAndSeekInfo info){
            info.SetupSeeker();
        }
        
        public static void SetupSpectator(HideAndSeekInfo info){
            info.SetupSpectator();
        }

        public static void Reset(HideAndSeekInfo info){
            info.Reset();
        }

        public static void CleanUpPlayer(HideAndSeekInfo info){
            info.CleanUp();
        }

        public static void SetPlayerSignalSize(HideAndSeekInfo info, float size){
            //PlayerTransformSync.LocalInstance?.ReferenceSector?.AttachedObject.GetRootSector();
            //TODO :: WHEN ADDED TO QSB
        }
    }

    public enum PlayerState{
        Hiding,
        Seeking,
        Spectating,
        None
    }
}