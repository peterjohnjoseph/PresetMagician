﻿using System.IO;
using MessagePack;

namespace PresetMagician.NKS
{
    public class PluginIdChunk : AbstractMessagePack
    {
        public const string RiffTypeID = "PLID";

        public PluginIdChunk()
        {
            TypeID = RiffTypeID;
            Version = 1;
            pluginId = new PluginId();
        }

        public override void Read(Stream source)
        {
            base.ReadData(source, RiffTypeID);
        }

        public override void Write(Stream target)
        {
            base.WriteData(target);
        }

        public override void DeserializeMessagePack(byte[] buffer)
        {
            pluginId = MessagePackSerializer.Deserialize<PluginId>(buffer);
        }

        public override byte[] SerializeMessagePack()
        {
            byte[] b = MessagePackSerializer.Serialize(pluginId);

            return b;
        }

        public PluginId pluginId;

        public string getJSON()
        {
            return MessagePackSerializer.ToJson(MessagePackSerializer.Serialize(pluginId));
        }

        public override string ChunkDescription
        {
            get { return "Native Instruments Plugin ID"; }
        }
    }
}