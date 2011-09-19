using System;
using System.Collections.Generic;
using System.Linq;
using WowPacketParser.Enums;
using WowPacketParser.Misc;
using WowPacketParser.SQL;
using Guid=WowPacketParser.Misc.Guid;

namespace WowPacketParser.Parsing.Parsers
{
    public static class CharacterHandler
    {
        public static readonly Dictionary<Guid, CharacterInfo> Characters =
            new Dictionary<Guid, CharacterInfo>();

        public static readonly List<StartInfo> StartInfos = new List<StartInfo>();

        [Parser(Opcode.CMSG_STANDSTATECHANGE)]
        public static void HandleStandStateChange(Packet packet)
        {
            packet.ReadInt32("Standstate");
        }

        [Parser(Opcode.SMSG_STANDSTATE_UPDATE)]
        public static void HandleStandStateUpdate(Packet packet)
        {
            packet.ReadByte("Standstate");
        }

        [Parser(Opcode.CMSG_CHAR_CREATE)]
        public static void HandleClientCharCreate(Packet packet)
        {
            packet.ReadCString("Name");

            packet.ReadEnum<Race>("Race", TypeCode.Byte);

            packet.ReadEnum<Class>("Class", TypeCode.Byte);

            packet.ReadEnum<Gender>("Gender", TypeCode.Byte);

            packet.ReadByte("Skin");

            packet.ReadByte("Face");

            packet.ReadByte("Hair Style");

            packet.ReadByte("Hair Color");

            packet.ReadByte("Facial Hair");

            packet.ReadByte("Outfit ID");
        }

        [Parser(Opcode.CMSG_CHAR_DELETE)]
        public static void HandleClientCharDelete(Packet packet)
        {
            packet.ReadGuid("GUID");
        }

        [Parser(Opcode.CMSG_CHAR_RENAME)]
        public static void HandleClientCharRename(Packet packet)
        {
            packet.ReadGuid("GUID");

            packet.ReadCString("New Name");
        }

        [Parser(Opcode.SMSG_CHAR_RENAME)]
        public static void HandleServerCharRename(Packet packet)
        {
            var result = packet.ReadEnum<ResponseCode>("Race", TypeCode.Byte);

            if (result != ResponseCode.RESPONSE_SUCCESS)
                return;

            packet.ReadGuid("GUID");

            packet.ReadCString("Name");
        }

        [Parser(Opcode.SMSG_CHAR_CREATE)]
        [Parser(Opcode.SMSG_CHAR_DELETE)]
        public static void HandleCharResponse(Packet packet)
        {
            packet.ReadEnum<ResponseCode>("Response", TypeCode.Byte);
        }

        [Parser(Opcode.CMSG_ALTER_APPEARANCE)]
        public static void HandleAlterAppearance(Packet packet)
        {
            packet.ReadByte("Hair Style");

            packet.ReadByte("Hair Color");

            packet.ReadByte("Facial Hair");
        }

        [Parser(Opcode.SMSG_BARBER_SHOP_RESULT)]
        public static void HandleBarberShopResult(Packet packet)
        {
            packet.ReadEnum<BarberShopResult>("Result", TypeCode.Int32);
        }

        [Parser(Opcode.CMSG_CHAR_CUSTOMIZE)]
        public static void HandleClientCharCustomize(Packet packet)
        {
            packet.ReadGuid("GUID");

            packet.ReadCString("New Name");

            packet.ReadEnum<Gender>("Gender", TypeCode.Byte);

            packet.ReadByte("Skin");

            packet.ReadByte("Face");

            packet.ReadByte("Hair Style");

            packet.ReadByte("Hair Color");

            packet.ReadByte("Facial Hair");
        }

        [Parser(Opcode.SMSG_CHAR_CUSTOMIZE)]
        public static void HandleServerCharCustomize(Packet packet)
        {
            var response = packet.ReadEnum<ResponseCode>("Response", TypeCode.Byte);

            if (response != ResponseCode.RESPONSE_SUCCESS)
                return;

            packet.ReadGuid("GUID");

            packet.ReadCString("Name");

            packet.ReadEnum<Gender>("Gender", TypeCode.Byte);

            packet.ReadByte("Skin");

            packet.ReadByte("Face");

            packet.ReadByte("Hair Style");

            packet.ReadByte("Hair Color");

            packet.ReadByte("Facial Hair");
        }

        [Parser(Opcode.SMSG_CHAR_ENUM)]
        public static void HandleCharEnum(Packet packet)
        {
            Characters.Clear();

            var count = packet.ReadByte("Count");

            for (var i = 0; i < count; i++)
            {
                var guid = packet.ReadGuid("GUID");

                var name = packet.ReadCString("Name");

                var race = packet.ReadEnum<Race>("Race", TypeCode.Byte);

                var clss = packet.ReadEnum<Class>("Class", TypeCode.Byte);

                packet.ReadEnum<Gender>("Gender", TypeCode.Byte);

                packet.ReadByte("Skin");

                packet.ReadByte("Face");

                packet.ReadByte("Hair Style");

                packet.ReadByte("Hair Color");

                packet.ReadByte("Facial Hair");

                var level = packet.ReadByte("Level");

                var zone = packet.ReadInt32("Zone ID");

                var mapId = packet.ReadInt32("Map ID");

                var pos = packet.ReadVector3("Position");

                packet.ReadInt32("Guild ID");

                packet.ReadEnum<CharacterFlag>("Character Flags", TypeCode.Int32);

                packet.ReadEnum<CustomizationFlag>("Customization Flags", TypeCode.Int32);

                var firstLogin = packet.ReadBoolean("First Login");

                packet.ReadInt32("Pet Display ID");

                packet.ReadInt32("Pet Level");

                packet.ReadEnum<CreatureFamily>("Pet Family", TypeCode.Int32);

                for (var j = 0; j < 19; j++)
                {
                    packet.ReadInt32("Equip Display ID");

                    packet.ReadEnum<InventoryType>("Equip Inventory Type", TypeCode.Byte);

                    packet.ReadInt32("Equip Aura ID");
                }

                for (var j = 0; j < 4; j++)
                {
                    packet.ReadInt32("Bag Display ID");

                    packet.ReadEnum<InventoryType>("Bag Inventory Type", TypeCode.Byte);

                    packet.ReadInt32("Bag Aura ID");
                }

                if (firstLogin)
                    if (StartInfos.All(item => item.Race != race && item.Class != clss))
                    {
                        var startInfo = new StartInfo
                                            {
                                                Race = race,
                                                Class = clss,
                                                Position = pos,
                                                Map = mapId,
                                                Zone = zone
                                            };

                        StartInfos.Add(startInfo);
                        SQLStore.WriteData(SQLStore.StartPositions.GetCommand(race, clss, mapId, zone, pos));
                    }

                var chInfo = new CharacterInfo
                                 {
                                     Guid = guid,
                                     Race = race,
                                     Class = clss,
                                     Name = name,
                                     FirstLogin = firstLogin,
                                     Level = level
                                 };

                Characters.Add(guid, chInfo);
            }
        }

        [Parser(Opcode.SMSG_SET_FACTION_STANDING)]
        public static void HandleSetFactionStanding(Packet packet)
        {
            packet.ReadSingle("Unk Float");

            packet.ReadByte("Unk UInt8");

            var amount = packet.ReadInt32("Count");

            for (int i = 0; i < amount; i++)
            {
                packet.ReadInt32("Faction List ID");

                packet.ReadInt32("Standing");
            }
        }

        [Parser(Opcode.SMSG_PLAYER_VEHICLE_DATA)]
        public static void HandlePlayerVehicleData(Packet packet)
        {
            packet.ReadPackedGuid("GUID");

            packet.ReadInt32("Vehicle ID");
        }
    }
}