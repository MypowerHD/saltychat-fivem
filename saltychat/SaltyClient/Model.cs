﻿using System;
using System.Collections.Generic;
using System.Text;

namespace SaltyClient
{
    #region GameInstance
    /// <summary>
    /// Used for <see cref="Command.Initiate"/>
    /// </summary>
    public class GameInstance
    {
        #region Properties
        /// <summary>
        /// Unique id of the server the player must be connected to
        /// </summary>
        public string ServerUniqueIdentifier { get; set; }

        /// <summary>
        /// TeamSpeak name that should be set (max length is 30)
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Id of the TeamSpeak channel the player should be moved to
        /// </summary>
        public ulong ChannelId { get; set; }

        /// <summary>
        /// Password of the TeamSpeak channel
        /// </summary>
        public string ChannelPassword { get; set; }

        /// <summary>
        /// Foldername of the sound pack that will be used (%AppData%\TS3Client\Plugins\SaltyChat\{SoundPack}\)
        /// </summary>
        public string SoundPack { get; set; }

        /// <summary>
        /// IDs of channels which the player can join, while the game instace is running
        /// </summary>
        public ulong[] SwissChannelIds { get; set; }
        #endregion

        #region CTOR
        public GameInstance(string serverUniqueIdentifier, string name, ulong channelId, string channelPassword, string soundPack, ulong[] swissChannels)
        {
            this.ServerUniqueIdentifier = serverUniqueIdentifier;
            this.Name = name;
            this.ChannelId = channelId;
            this.ChannelPassword = channelPassword;
            this.SoundPack = soundPack;
            this.SwissChannelIds = swissChannels;
        }
        #endregion
    }
    #endregion

    #region PluginError
    public class PluginError
    {
        public Error Error { get; set; }
        public string Message { get; set; }
        public string ServerIdentifier { get; set; }

        public static PluginError Deserialize(dynamic obj)
        {
            return new PluginError()
            {
                Error = (Error)obj.Error,
                Message = obj.Message,
                ServerIdentifier = obj.ServerIdentifier
            };
        }
    }
    #endregion

    #region PluginCommand
    public class PluginCommand
    {
        #region Properties
        public Command Command { get; set; }
        public string ServerUniqueIdentifier { get; set; }
        public Newtonsoft.Json.Linq.JObject Parameter { get; set; }
        #endregion

        #region CTOR
        /// <summary>
        /// For deserialization only
        /// </summary>
        [Newtonsoft.Json.JsonConstructor]
        internal PluginCommand()
        {

        }

        /// <summary>
        /// Use this for <see cref="Command.Pong"/>
        /// </summary>
        /// <param name="command"></param>
        /// <param name="parameter"></param>
        internal PluginCommand(string serverUniqueIdentifier)
        {
            this.Command = Command.Pong;
            this.ServerUniqueIdentifier = serverUniqueIdentifier;
        }

        /// <summary>
        /// Use this with <see cref="Command.Initiate"/>
        /// </summary>
        /// <param name="command"></param>
        /// <param name="parameter"></param>
        internal PluginCommand(Command command, object parameter)
        {
            this.Command = command;
            this.Parameter = Newtonsoft.Json.Linq.JObject.FromObject(parameter);
        }

        internal PluginCommand(Command command, string serverUniqueIdentifier, object parameter)
        {
            this.Command = command;
            this.ServerUniqueIdentifier = serverUniqueIdentifier;
            this.Parameter = Newtonsoft.Json.Linq.JObject.FromObject(parameter);
        }
        #endregion

        #region Methods
        public string Serialize()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this);
        }

        public static PluginCommand Deserialize(dynamic obj)
        {
            return new PluginCommand()
            {
                Command = (Command)obj.Command,
                ServerUniqueIdentifier = obj.ServerUniqueIdentifier,
                Parameter = obj.Parameter == null ? null : Newtonsoft.Json.Linq.JObject.FromObject(obj.Parameter)
            };
        }

        public bool TryGetPayload<T>(out T payload)
        {
                try
                {
                    payload = this.Parameter.ToObject<T>();

                    return true;
                }
                catch
                {
                    // do nothing
                }

            payload = default;
            return false;
        }
        #endregion

        #region Conditional Property Serialization
        public bool ShouldSerializeParameter() => this.Parameter != null;
        #endregion
    }
    #endregion

    #region PluginState
    /// <summary>
    /// Will be received from the WebSocket if after starting a new instance
    /// </summary>
    public class PluginState
    {
        public string Version { get; set; }
        public int ActiveInstances { get; set; }
    }
    #endregion

    #region InstanceState
    /// <summary>
    /// Will be received from the WebSocket if the player (dis-)connects to the specified TeamSpeak server or channel
    /// </summary>
    public class InstanceState
    {
        public bool IsConnectedToServer { get; set; }
        public bool IsReady { get; set; }
    }
    #endregion

    #region SoundState
    /// <summary>
    /// Will be received from the WebSocket on every microphone and sound state change
    /// </summary>
    public class SoundState
    {
        public bool IsMicrophoneMuted { get; set; }
        public bool IsMicrophoneEnabled { get; set; }
        public bool IsSoundMuted { get; set; }
        public bool IsSoundEnabled { get; set; }
    }
    #endregion

    #region PlayerState
    /// <summary>
    /// Used for <see cref="Command.SelfStateUpdate"/> and <see cref="Command.PlayerStateUpdate"/>
    /// </summary>
    public class PlayerState
    {
        #region Properties
        public string Name { get; set; }
        public Vector3 Position { get; set; }
        public float? Rotation { get; set; }
        public float? VoiceRange { get; set; }
        public bool IsAlive { get; set; }
        public float? VolumeOverride { get; set; }
        public bool NoLoS { get; set; }
        public bool DistanceCulled { get; set; }
        #endregion

        #region CTOR
        /// <summary>
        /// Used for <see cref="Command.RemovePlayer"/>
        /// </summary>
        /// <param name="name"></param>
        public PlayerState(string name)
        {
            this.Name = name;
        }

        /// <summary>
        /// Used for <see cref="Command.SelfStateUpdate"/>
        /// </summary>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        public PlayerState(CitizenFX.Core.Vector3 position, float rotation)
        {
            this.Position = new Vector3(position.X, position.Y, position.Z);
            this.Rotation = rotation;
        }

        /// <summary>
        /// Used for <see cref="Command.PlayerStateUpdate"/>
        /// </summary>
        /// <param name="name"></param>
        /// <param name="position"></param>
        /// <param name="voiceRange"></param>
        /// <param name="isAlive"></param>
        /// <param name="distanceCulled"></param>
        /// <param name="noLos"></param>
        public PlayerState(string name, CitizenFX.Core.Vector3 position, float voiceRange, bool isAlive, bool distanceCulled = false, bool noLos = false)
        {
            this.Name = name;
            this.Position = new Vector3(position.X, position.Y, position.Z);
            this.VoiceRange = voiceRange;
            this.IsAlive = isAlive;
            this.NoLoS = noLos;
            this.DistanceCulled = distanceCulled;
        }

        /// <summary>
        /// Used for <see cref="Command.PlayerStateUpdate"/> with volume override
        /// </summary>
        /// <param name="name"></param>
        /// <param name="position"></param>
        /// <param name="voiceRange"></param>
        /// <param name="isAlive"></param>
        /// <param name="volumeOverride">Overrides the volume (phone, radio and proximity) - from 0 (0%) to 1.6 (160%)</param>
        public PlayerState(string name, CitizenFX.Core.Vector3 position, float voiceRange, bool isAlive, float volumeOverride)
        {
            this.Name = name;
            this.Position = new Vector3(position.X, position.Y, position.Z);
            this.VoiceRange = voiceRange;
            this.IsAlive = isAlive;

            if (volumeOverride > 1.6f)
                this.VolumeOverride = 1.6f;
            else if (volumeOverride < 0f)
                this.VolumeOverride = 0f;
            else
                this.VolumeOverride = volumeOverride;
        }
        #endregion

        #region Conditional Property Serialization
        public bool ShouldSerializeName() => !string.IsNullOrEmpty(this.Name);

        public bool ShouldSerializeRotation() => this.Rotation.HasValue;

        public bool ShouldSerializeVoiceRange() => this.VoiceRange.HasValue;

        public bool ShouldSerializeIsAlive() => this.IsAlive;

        public bool ShouldSerializeVolumeOverride() => this.VolumeOverride.HasValue;

        public bool ShouldSerializeNoLoS() => this.NoLoS;

        public bool ShouldSerializeDistanceCulled() => this.DistanceCulled;
        #endregion
    }
    #endregion

    #region BulkUpdate
    /// <summary>
    /// Used for <see cref="Command.BulkUpdate"/>
    /// </summary>
    public class BulkUpdate
    {
        public ICollection<PlayerState> PlayerStates { get; set; }
        public PlayerState SelfState { get; set; }

        public BulkUpdate(ICollection<PlayerState> playerStates, PlayerState selfState)
        {
            this.PlayerStates = playerStates;
            this.SelfState = selfState;
        }
    }
    #endregion

    #region TalkState
    public class TalkState
    {
        public string Name { get; set; }
        public bool IsTalking { get; set; }
    }
    #endregion

    #region Phone
    /// <summary>
    /// Used for <see cref="Command.PhoneCommunicationUpdate"/> and <see cref="Command.StopPhoneCommunication"/>
    /// </summary>
    public class PhoneCommunication
    {
        #region Properties
        public string Name { get; set; }
        public int? SignalStrength { get; set; }
        public float? Volume { get; set; }

        public bool Direct { get; set; }
        public string[] RelayedBy { get; set; }
        #endregion

        #region CTOR
        public PhoneCommunication(string name)
        {
            this.Name = name;
        }

        public PhoneCommunication(string name, int signalStrength)
        {
            this.Name = name;
            this.SignalStrength = signalStrength;

            this.Direct = true;
        }

        public PhoneCommunication(string name, int signalStrength, float volume)
        {
            this.Name = name;
            this.SignalStrength = signalStrength;
            this.Volume = volume;

            this.Direct = true;
        }

        public PhoneCommunication(string name, int signalStrength, bool direct, string[] relayedBy)
        {
            this.Name = name;
            this.SignalStrength = signalStrength;

            this.Direct = direct;
            this.RelayedBy = relayedBy;
        }

        public PhoneCommunication(string name, int signalStrength, float volume, bool direct, string[] relayedBy)
        {
            this.Name = name;
            this.SignalStrength = signalStrength;
            this.Volume = volume;

            this.Direct = direct;
            this.RelayedBy = relayedBy;
        }
        #endregion

        #region Conditional Property Serialization
        public bool ShouldSerializeSignalStrength() => this.SignalStrength.HasValue;

        public bool ShouldSerializeVolume() => this.Volume.HasValue;

        public bool ShouldSerializeDirect() => this.Direct;

        public bool ShouldSerializeRelayedBy() => this.RelayedBy != null && this.RelayedBy.Length > 0;
        #endregion
    }
    #endregion

    #region Radio
    /// <summary>
    /// Used for <see cref="Command.RadioTowerUpdate"/>
    /// </summary>
    public class RadioTower
    {
        public Vector3[] Towers { get; set; }

        public RadioTower(Vector3[] towers)
        {
            this.Towers = towers;
        }
    }

    /// <summary>
    /// Used for <see cref="Command.RadioCommunicationUpdate"/>
    /// </summary>
    public class RadioCommunication
    {
        #region Properties
        public string Name { get; set; }
        public RadioType SenderRadioType { get; set; }
        public RadioType OwnRadioType { get; set; }
        public bool PlayMicClick { get; set; }
        public float? Volume { get; set; }

        public bool Direct { get; set; }
        public bool Secondary { get; set; }
        public string[] RelayedBy { get; set; }
        #endregion

        #region CTOR
        public RadioCommunication(string name, RadioType senderRadioType, RadioType ownRadioType, bool playMicClick, bool isSecondary)
        {
            this.Name = name;
            this.SenderRadioType = senderRadioType;
            this.OwnRadioType = ownRadioType;
            this.PlayMicClick = playMicClick;

            this.Direct = true;
            this.Secondary = isSecondary;
        }

        public RadioCommunication(string name, RadioType senderRadioType, RadioType ownRadioType, bool playMicClick, bool direct, bool isSecondary, string[] relayedBy)
        {
            this.Name = name;
            this.SenderRadioType = senderRadioType;
            this.OwnRadioType = ownRadioType;
            this.PlayMicClick = playMicClick;

            this.Direct = direct;
            this.Secondary = isSecondary;
            this.RelayedBy = relayedBy;
        }
        #endregion

        #region Conditional Property Serialization
        public bool ShouldSerializePlayMicClick() => this.PlayMicClick;

        public bool ShouldSerializeVolume() => this.Volume.HasValue;

        public bool ShouldSerializeDirect() => this.Direct;

        public bool ShouldSerializeSecondary() => this.Secondary;

        public bool ShouldSerializeRelayedBy() => this.RelayedBy != null && this.RelayedBy.Length > 0;
        #endregion
    }

    [Flags]
    public enum RadioType
    {
        /// <summary>
        /// No radio communication
        /// </summary>
        None = 1,

        /// <summary>
        /// Short range radio communication - appx. 3 kilometers
        /// </summary>
        ShortRange = 2,

        /// <summary>
        /// Long range radio communication - appx. 8 kilometers
        /// </summary>
        LongRange = 4,

        /// <summary>
        /// Distributed radio communication, depending on <see cref="RadioTower"/> - appx. 1.8 (ultra short range), appx. 3 (short range) or 8 (long range) kilometers
        /// </summary>
        Distributed = 8,

        /// <summary>
        /// Ultra Short range radio communication - appx. 1.8 kilometers
        /// </summary>
        UltraShortRange = 16,
    }
    #endregion

    #region Megaphone
    /// <summary>
    /// Used for <see cref="Command.MegaphoneCommunicationUpdate"/>
    /// </summary>
    public class MegaphoneCommunication
    {
        #region Properties
        public string Name { get; set; }
        public float Range { get; set; }
        public float? Volume { get; set; }
        #endregion

        #region CTOR
        public MegaphoneCommunication(string name, float range)
        {
            this.Name = name;
            this.Range = range;
        }
        public MegaphoneCommunication(string name, float range, float volume)
        {
            this.Name = name;
            this.Range = range;
            this.Volume = volume;
        }
        #endregion

        #region Conditional Property Serialization
        public bool ShouldSerializeVolume() => this.Volume.HasValue;
        #endregion
    }
    #endregion

    #region Sound
    /// <summary>
    /// Used for <see cref="Command.PlaySound"/>
    /// </summary>
    public class Sound
    {
        #region Properties
        public string Filename { get; set; }
        public bool IsLoop { get; set; }
        public string Handle { get; set; }
        #endregion

        #region CTOR
        public Sound(string filename)
        {
            this.Filename = filename;
            this.Handle = filename;
        }

        public Sound(string filename, bool loop)
        {
            this.Filename = filename;
            this.IsLoop = loop;
            this.Handle = filename;
        }

        public Sound(string filename, bool loop, string handle)
        {
            this.Filename = filename;
            this.IsLoop = loop;
            this.Handle = handle;
        }
        #endregion
    }
    #endregion

    #region Command
    public enum Command
    {
        // Plugin
        PluginState = 0,

        // Instance
        Initiate = 1,
        Reset = 2,
        Ping = 3,
        Pong = 4,
        InstanceState = 5,
        SoundState = 6,
        SelfStateUpdate = 7,
        PlayerStateUpdate = 8,
        BulkUpdate = 9,
        RemovePlayer = 10,
        TalkState = 11,
        PlaySound = 18,
        StopSound = 19,

        // Phone
        PhoneCommunicationUpdate = 20,
        StopPhoneCommunication = 21,

        // Radio
        RadioCommunicationUpdate = 30,
        StopRadioCommunication = 31,
        RadioTowerUpdate = 32,

        // Megaphone
        MegaphoneCommunicationUpdate = 40,
        StopMegaphoneCommunication = 41,
    }
    #endregion

    #region Error
    public enum Error
    {
        OK,
        InvalidJson,
        NotConnectedToServer,
        AlreadyInGame,
        ChannelNotAvailable,
        NameNotAvailable
    }
    #endregion

    #region UpdateBranch
    internal enum UpdateBranch
    {
        Stable,
        Testing,
        PreBuild
    }
    #endregion

    #region VoiceClient
    public class VoiceClient
    {
        public int ServerId { get; set; }
        public CitizenFX.Core.Player Player => VoiceManager.PlayerList[this.ServerId];
        public string TeamSpeakName { get; set; }
        public float VoiceRange { get; set; }
        public bool IsAlive { get; set; }
        public CitizenFX.Core.Vector3 LastPosition { get; set; }
        public bool DistanceCulled { get; set; }

        public VoiceClient(int serverId, string teamSpeakName, float voiceRange, bool isAlive, CitizenFX.Core.Vector3 lastPosition)
        {
            this.ServerId = serverId;
            this.TeamSpeakName = teamSpeakName;
            this.VoiceRange = voiceRange;
            this.IsAlive = isAlive;
            this.LastPosition = lastPosition;
        }

        internal void SendPlayerStateUpdate(VoiceManager voiceManager)
        {
            voiceManager.ExecuteCommand(new PluginCommand(Command.PlayerStateUpdate, voiceManager.ServerUniqueIdentifier, new PlayerState(this.TeamSpeakName, this.LastPosition, this.VoiceRange, this.IsAlive)));
        }
    }
    #endregion

    #region Vector3
    public struct Vector3
    {
        public float X;
        public float Y;
        public float Z;

        public Vector3(float x, float y, float z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }
    }
    #endregion
}
