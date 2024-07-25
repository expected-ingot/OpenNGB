using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
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
    
    Part of Arc.Trackmania was copied for use in `DecryptEntries()`.
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

            public byte[] nameKey;
        }
        public struct PackEntry
        {
            public int flags;
            public int nameLength; // < 32
            public byte[] encryptedName;
            public byte[] encryptedKeyString;

            public byte[] rawName;
            public string name;
            public byte[] key;
            public byte[] keyStringKey;
            public byte[] keyString;

        }
        public static void DecryptEntries(ref PacklistStructure structure)
        {
            MD5CryptoServiceProvider _md5 = new MD5CryptoServiceProvider();
            structure.nameKey = _md5.ComputeHash(Encoding.ASCII.GetBytes("6611992868945B0B59536FC3226F3FD0" + structure.salt));
            for (int i = 0; i < structure.numPacks; i++)
            {
                // Decrypt .PAK filenames
                PackEntry entry = structure.entries[i];
                entry.rawName = entry.encryptedName;
                for (int j = 0; j < entry.encryptedName.Length; j++)
                {
                    entry.rawName[j] ^= structure.nameKey[j % 16];
                }
                entry.name = Encoding.ASCII.GetString(entry.rawName);

                entry.keyStringKey = _md5.ComputeHash(Encoding.ASCII.GetBytes(entry.name + structure.salt + "B97C1205648A66E04F86A1B5D5AF9862"));
                entry.keyString = new byte[0x20];
                for (int j = 0; j < 0x20; j++)
                {
                    entry.keyString[j] = (byte)(entry.encryptedKeyString[j] ^ entry.keyStringKey[j % 16]);
                }
                entry.key = _md5.ComputeHash(Encoding.ASCII.GetBytes(entry.keyString + "NadeoPak"));

                structure.entries[i] = entry;
            }
        }
        public static bool VerifySignature(BinaryReader reader, PacklistStructure structure)
        {
            return false
        }
        public static PacklistStructure ReadPacklistStructure(string filePath)
        {
            BinaryReader binaryReader = new BinaryReader(File.OpenRead(filePath));
            PacklistStructure output = new PacklistStructure();
            output.version = binaryReader.ReadByte(); // 1
            output.numPacks = binaryReader.ReadByte();
            output.crc32 = binaryReader.ReadUInt32();
            output.salt = binaryReader.ReadUInt32();
            output.entries = new PackEntry[output.numPacks];
            for (int i = 0; i < output.numPacks; i++)
            {
                PackEntry entry = new PackEntry();
                entry.flags = binaryReader.ReadByte();
                entry.nameLength = binaryReader.ReadByte();
                entry.encryptedName = binaryReader.ReadBytes(entry.nameLength);
                entry.encryptedKeyString = binaryReader.ReadBytes(0x20); // 32
                output.entries[i] = entry;
            }
            output.signature = binaryReader.ReadBytes(0x10); // 16
            DecryptEntries(ref output);
            return output;
        }
    }
}
