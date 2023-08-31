using Lumina;
using Lumina.Data;
using Lumina.Excel;

namespace MogMogCheck.Sheets;

public class ExtendedSpecialShop : Lumina.Excel.GeneratedSheets.SpecialShop
{
    private const int NumItems = 60;

    [StructLayout(LayoutKind.Explicit, Size = StructSize)]
    public struct SpecialShopItem
    {
        public const int StructSize = 0x4C; // 0x50 - 0x4

        [FieldOffset(0x4)] public uint ReceiveCount1;
        [FieldOffset(0x8)] public uint ReceiveCount2;
        [FieldOffset(0xC)] public uint GiveCount1;
        [FieldOffset(0x10)] public uint GiveCount2;
        [FieldOffset(0x14)] public uint GiveCount3;
        [FieldOffset(0x18)] public int ReceiveItemId1;
        [FieldOffset(0x1C)] public int ReceiveItemId2;
        [FieldOffset(0x20)] public int ReceiveSpecialShopItemCategory1;
        [FieldOffset(0x24)] public int ReceiveSpecialShopItemCategory2;
        [FieldOffset(0x28)] public int GiveItemId1;
        [FieldOffset(0x2C)] public int GiveItemId2;
        [FieldOffset(0x30)] public int GiveItemId3;
        [FieldOffset(0x34)] public int UnlockQuest;
        [FieldOffset(0x38)] public int Unk38;
        [FieldOffset(0x3C)] public int Unk3C;
        [FieldOffset(0x40)] public ushort Unk40;
        [FieldOffset(0x42)] public ushort Unk42;
        [FieldOffset(0x44)] public ushort Unk44;
        [FieldOffset(0x46)] public ushort PatchNumber;
        [FieldOffset(0x48)] public byte Unk48;
        [FieldOffset(0x49)] public byte Unk49;
        [FieldOffset(0x4A)] public byte Unk4A;
        [FieldOffset(0x4B)] public byte Unk4B;
        [FieldOffset(0x4C)] public byte Unk4C;
        [FieldOffset(0x4D)] public byte SortKey;
        [FieldOffset(0x4E)] public bool Unk4E;
        [FieldOffset(0x4F)] public bool Unk4F;

        public unsafe void Read(int index, RowParser parser)
        {
            ReceiveCount1 = parser.ReadOffset<uint>((ushort)(0x4 + StructSize * index));
            ReceiveCount2 = parser.ReadOffset<uint>((ushort)(0x8 + StructSize * index));
            GiveCount1 = parser.ReadOffset<uint>((ushort)(0xC + StructSize * index));
            GiveCount2 = parser.ReadOffset<uint>((ushort)(0x10 + StructSize * index));
            GiveCount3 = parser.ReadOffset<uint>((ushort)(0x14 + StructSize * index));
            ReceiveItemId1 = parser.ReadOffset<int>((ushort)(0x18 + StructSize * index));
            ReceiveItemId2 = parser.ReadOffset<int>((ushort)(0x1C + StructSize * index));
            ReceiveSpecialShopItemCategory1 = parser.ReadOffset<int>((ushort)(0x20 + StructSize * index));
            ReceiveSpecialShopItemCategory2 = parser.ReadOffset<int>((ushort)(0x24 + StructSize * index));
            GiveItemId1 = parser.ReadOffset<int>((ushort)(0x28 + StructSize * index));
            GiveItemId2 = parser.ReadOffset<int>((ushort)(0x2C + StructSize * index));
            GiveItemId3 = parser.ReadOffset<int>((ushort)(0x30 + StructSize * index));
            UnlockQuest = parser.ReadOffset<int>((ushort)(0x34 + StructSize * index));
            Unk38 = parser.ReadOffset<int>((ushort)(0x38 + StructSize * index));
            Unk3C = parser.ReadOffset<int>((ushort)(0x3C + StructSize * index));
            Unk40 = parser.ReadOffset<ushort>((ushort)(0x40 + StructSize * index));
            Unk42 = parser.ReadOffset<ushort>((ushort)(0x42 + StructSize * index));
            Unk44 = parser.ReadOffset<ushort>((ushort)(0x44 + StructSize * index));
            PatchNumber = parser.ReadOffset<ushort>((ushort)(0x46 + StructSize * index));
            Unk48 = parser.ReadOffset<byte>((ushort)(0x48 + StructSize * index));
            Unk49 = parser.ReadOffset<byte>((ushort)(0x49 + StructSize * index));
            Unk4A = parser.ReadOffset<byte>((ushort)(0x4A + StructSize * index));
            Unk4B = parser.ReadOffset<byte>((ushort)(0x4B + StructSize * index));
            Unk4C = parser.ReadOffset<byte>((ushort)(0x4C + StructSize * index));
            SortKey = parser.ReadOffset<byte>((ushort)(0x4D + StructSize * index));
            Unk4E = parser.ReadOffset<bool>((ushort)(0x4E + StructSize * index));
            Unk4F = parser.ReadOffset<bool>((ushort)(0x4F + StructSize * index));
        }
    }

    public SpecialShopItem[] Items { get; set; } = null!;

    public override void PopulateData(RowParser parser, GameData gameData, Language language)
    {
        base.PopulateData(parser, gameData, language);

        Items = new SpecialShopItem[NumItems];
        for (var i = 0; i < NumItems; i++)
        {
            Items[i] = new SpecialShopItem();
            Items[i].Read(i, parser);
        }
    }
}
