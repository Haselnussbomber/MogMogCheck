using Lumina;
using Lumina.Data;
using Lumina.Excel;

namespace MogMogCheck.Sheets;

public class SpecialShop : Lumina.Excel.GeneratedSheets.SpecialShop
{
    private const int NumItems = 60;

    [StructLayout(LayoutKind.Explicit, Size = StructSize)]
    public struct SpecialShopItem
    {
        public const int StructSize = 0x4C; // 0x50 - 0x4

        [FieldOffset(0x4)] public uint StackSize;
        [FieldOffset(0x8)] public uint Unk8;
        [FieldOffset(0xC)] public uint RequiredCount;
        [FieldOffset(0x10)] public uint Unk10;
        [FieldOffset(0x14)] public uint Unk14;
        [FieldOffset(0x18)] public int ItemId;
        [FieldOffset(0x1C)] public int Unk1C;
        [FieldOffset(0x20)] public int Unk20;
        [FieldOffset(0x24)] public int Unk24;
        [FieldOffset(0x28)] public int RequiredItem;
        [FieldOffset(0x2C)] public int Unk2C;
        [FieldOffset(0x30)] public int Unk30;
        [FieldOffset(0x34)] public int RequiredQuest;
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
            StackSize = parser.ReadOffset<uint>((ushort)(0x4 + StructSize * index));
            Unk8 = parser.ReadOffset<uint>((ushort)(0x8 + StructSize * index));
            RequiredCount = parser.ReadOffset<uint>((ushort)(0xC + StructSize * index));
            Unk10 = parser.ReadOffset<uint>((ushort)(0x10 + StructSize * index));
            Unk14 = parser.ReadOffset<uint>((ushort)(0x14 + StructSize * index));
            ItemId = parser.ReadOffset<int>((ushort)(0x18 + StructSize * index));
            Unk1C = parser.ReadOffset<int>((ushort)(0x1C + StructSize * index));
            Unk20 = parser.ReadOffset<int>((ushort)(0x20 + StructSize * index));
            Unk24 = parser.ReadOffset<int>((ushort)(0x24 + StructSize * index));
            RequiredItem = parser.ReadOffset<int>((ushort)(0x28 + StructSize * index));
            Unk2C = parser.ReadOffset<int>((ushort)(0x2C + StructSize * index));
            Unk30 = parser.ReadOffset<int>((ushort)(0x30 + StructSize * index));
            RequiredQuest = parser.ReadOffset<int>((ushort)(0x34 + StructSize * index));
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
