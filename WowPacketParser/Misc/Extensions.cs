using System;
using WowPacketParser.DBC.DBCStructures;

namespace WowPacketParser.Misc
{
    public static class Extensions
    {
        public static byte ToByte(this bool boolean)
        {
            return (byte)(boolean ? 1 : 0);
        }

        public static bool HasAnyFlag(this Enum value, Enum toTest)
        {
            var val = ((IConvertible)value).ToUInt64(null);
            var test = ((IConvertible)toTest).ToUInt64(null);

            return (val & test) != 0;
        }

        public static string GetExistingSpellName(int spellId)
        {
            if (!DBC.DBCStore.DBC.Enabled()) // Could use a more general solution here
                return string.Empty;
            SpellEntry spell;
            if (spellId <= 0)
                return string.Empty;
            return DBC.DBCStore.DBC.Spell.TryGetValue((uint)spellId, out spell)
                ? spell.GetSpellName()
                : "-Unknown-";
        }

        public static string SpellLine(int spellId)
        {
            if (spellId == 0)
                return "0";
            var name = GetExistingSpellName(spellId);
            if (!String.IsNullOrEmpty(name))
                return spellId + " (" + name + ")";
            return spellId.ToString();
        }
    }
}