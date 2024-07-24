using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

/*
    * This parses `packlist.dat` files. They usually contain a
    table with every single .PAK file the game should "mount"
    along with the file names, decryption keys, and a checksum
    to verify that `packlist.dat` has the correct data.

    * `packlist.dat` and .PAK files were introduced sometime
    around 2006 between the releases of `TrackMania Nations
    ESWC` and `TrackMania United`.

    * File format (sourced from https://wiki.xaseco.org/wiki/Packlist.dat)

    byte version (1)
    byte numPacks
    uint32 crc32 (0)
    uint32 salt
    PackEntry[numPacks]
    byte signature[0x10] // 16

    PackEntry:
        byte flags
        byte nameLength (must be < 32)
        byte encryptedName[nameLength]
        byte encryptedKeyString[0x20] // 32
*/


namespace OpenNGB
{
    public static class Packlist
    {
        public struct PacklistStructure
        {
            public int version; // 1
            public int numPacks;
            public uint crc32; // 0
            public uint salt;
            public PackEntry[] entries;
            public byte[] signature;
        }
        public struct PackEntry
        {
            public int flags;
            public int nameLength; // < 32
            public byte[] encryptedName;
            public byte[] encryptedKeyString;
        }
        public static PacklistStructure ParseFile(string filePath)
        {
            byte[] buffer = File.ReadAllBytes(filePath);
            PacklistStructure output = new PacklistStructure();
            output.version = buffer[0];
            output.numPacks = buffer[1];
            output.crc32 = System.BitConverter.ToUInt32(buffer, 2); // 0
            output.salt = System.BitConverter.ToUInt32(buffer, 6);
            output.entries = new PackEntry[output.numPacks];
            int position = 9;
            for (int i = 0; i < output.numPacks; i++)
            {
                PackEntry entry = new PackEntry();
                entry.flags = buffer[position + 1];
                entry.nameLength = buffer[position + 2];
                entry.encryptedName = new byte[entry.nameLength];
                Array.Copy(buffer, position + 2, entry.encryptedName, 0, entry.nameLength);
                entry.encryptedKeyString = new byte[32]; // 0x20
                Array.Copy(buffer, position + 2 + entry.nameLength, entry.encryptedKeyString, 0, 32); // 0x20
                position = position + 2 + entry.nameLength + 32;
                output.entries[i] = entry;
            }
            output.signature = new byte[16];
            Array.Copy(buffer, position + 1, output.signature, 0, 16);
            return output;
        }
    }
}
