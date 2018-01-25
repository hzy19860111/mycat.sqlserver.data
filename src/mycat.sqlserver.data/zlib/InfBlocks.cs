using System;
namespace zlib
{
    internal sealed class InfBlocks
    {
        private const int MANY = 1440;
        private static readonly int[] inflate_mask = new int[]
		{
			0,
			1,
			3,
			7,
			15,
			31,
			63,
			127,
			255,
			511,
			1023,
			2047,
			4095,
			8191,
			16383,
			32767,
			65535
		};
        internal static readonly int[] border = new int[]
		{
			16,
			17,
			18,
			0,
			8,
			7,
			9,
			6,
			10,
			5,
			11,
			4,
			12,
			3,
			13,
			2,
			14,
			1,
			15
		};
        private const int Z_OK = 0;
        private const int Z_STREAM_END = 1;
        private const int Z_NEED_DICT = 2;
        private const int Z_ERRNO = -1;
        private const int Z_STREAM_ERROR = -2;
        private const int Z_DATA_ERROR = -3;
        private const int Z_MEM_ERROR = -4;
        private const int Z_BUF_ERROR = -5;
        private const int Z_VERSION_ERROR = -6;
        private const int TYPE = 0;
        private const int LENS = 1;
        private const int STORED = 2;
        private const int TABLE = 3;
        private const int BTREE = 4;
        private const int DTREE = 5;
        private const int CODES = 6;
        private const int DRY = 7;
        private const int DONE = 8;
        private const int BAD = 9;
        internal int mode;
        internal int left;
        internal int table;
        internal int index;
        internal int[] blens;
        internal int[] bb = new int[1];
        internal int[] tb = new int[1];
        internal InfCodes codes;
        internal int last;
        internal int bitk;
        internal int bitb;
        internal int[] hufts;
        internal byte[] window;
        internal int end;
        internal int read;
        internal int write;
        internal object checkfn;
        internal long check;
        internal InfBlocks(ZStream z, object checkfn, int w)
        {
            this.hufts = new int[4320];
            this.window = new byte[w];
            this.end = w;
            this.checkfn = checkfn;
            this.mode = 0;
            this.reset(z, null);
        }
        internal void reset(ZStream z, long[] c)
        {
            if (c != null)
            {
                c[0] = this.check;
            }
            if (this.mode == 4 || this.mode == 5)
            {
                this.blens = null;
            }
            if (this.mode == 6)
            {
                this.codes.free(z);
            }
            this.mode = 0;
            this.bitk = 0;
            this.bitb = 0;
            this.read = (this.write = 0);
            if (this.checkfn != null)
            {
                z.adler = (this.check = z._adler.adler32(0L, null, 0, 0));
            }
        }
        internal int proc(ZStream z, int r)
        {
            int num = z.next_in_index;
            int num2 = z.avail_in;
            int num3 = this.bitb;
            int i = this.bitk;
            int num4 = this.write;
            int num5 = (num4 < this.read) ? (this.read - num4 - 1) : (this.end - num4);
            int num6;
            while (true)
            {
                switch (this.mode)
                {
                    case 0:
                        while (i < 3)
                        {
                            if (num2 == 0)
                            {
                                goto IL_8C;
                            }
                            r = 0;
                            num2--;
                            num3 |= (int)(z.next_in[num++] & 255) << i;
                            i += 8;
                        }
                        num6 = (num3 & 7);
                        this.last = (num6 & 1);
                        switch (SupportClass.URShift(num6, 1))
                        {
                            case 0:
                                num3 = SupportClass.URShift(num3, 3);
                                i -= 3;
                                num6 = (i & 7);
                                num3 = SupportClass.URShift(num3, num6);
                                i -= num6;
                                this.mode = 1;
                                continue;
                            case 1:
                                {
                                    int[] array = new int[1];
                                    int[] array2 = new int[1];
                                    int[][] array3 = new int[1][];
                                    int[][] array4 = new int[1][];
                                    InfTree.inflate_trees_fixed(array, array2, array3, array4, z);
                                    this.codes = new InfCodes(array[0], array2[0], array3[0], array4[0], z);
                                    num3 = SupportClass.URShift(num3, 3);
                                    i -= 3;
                                    this.mode = 6;
                                    continue;
                                }
                            case 2:
                                num3 = SupportClass.URShift(num3, 3);
                                i -= 3;
                                this.mode = 3;
                                continue;
                            case 3:
                                goto IL_1CD;
                            default:
                                continue;
                        }
                        break;
                    case 1:
                        while (i < 32)
                        {
                            if (num2 == 0)
                            {
                                goto IL_23D;
                            }
                            r = 0;
                            num2--;
                            num3 |= (int)(z.next_in[num++] & 255) << i;
                            i += 8;
                        }
                        if ((SupportClass.URShift(~num3, 16) & 65535) != (num3 & 65535))
                        {
                            goto Block_8;
                        }
                        this.left = (num3 & 65535);
                        i = (num3 = 0);
                        this.mode = ((this.left != 0) ? 2 : ((this.last != 0) ? 7 : 0));
                        continue;
                    case 2:
                        if (num2 == 0)
                        {
                            goto Block_11;
                        }
                        if (num5 == 0)
                        {
                            if (num4 == this.end && this.read != 0)
                            {
                                num4 = 0;
                                num5 = ((num4 < this.read) ? (this.read - num4 - 1) : (this.end - num4));
                            }
                            if (num5 == 0)
                            {
                                this.write = num4;
                                r = this.inflate_flush(z, r);
                                num4 = this.write;
                                num5 = ((num4 < this.read) ? (this.read - num4 - 1) : (this.end - num4));
                                if (num4 == this.end && this.read != 0)
                                {
                                    num4 = 0;
                                    num5 = ((num4 < this.read) ? (this.read - num4 - 1) : (this.end - num4));
                                }
                                if (num5 == 0)
                                {
                                    goto Block_21;
                                }
                            }
                        }
                        r = 0;
                        num6 = this.left;
                        if (num6 > num2)
                        {
                            num6 = num2;
                        }
                        if (num6 > num5)
                        {
                            num6 = num5;
                        }
                        Array.Copy(z.next_in, num, this.window, num4, num6);
                        num += num6;
                        num2 -= num6;
                        num4 += num6;
                        num5 -= num6;
                        if ((this.left -= num6) == 0)
                        {
                            this.mode = ((this.last != 0) ? 7 : 0);
                            continue;
                        }
                        continue;
                    case 3:
                        while (i < 14)
                        {
                            if (num2 == 0)
                            {
                                goto IL_515;
                            }
                            r = 0;
                            num2--;
                            num3 |= (int)(z.next_in[num++] & 255) << i;
                            i += 8;
                        }
                        num6 = (this.table = (num3 & 16383));
                        if ((num6 & 31) > 29 || (num6 >> 5 & 31) > 29)
                        {
                            goto IL_5A3;
                        }
                        num6 = 258 + (num6 & 31) + (num6 >> 5 & 31);
                        this.blens = new int[num6];
                        num3 = SupportClass.URShift(num3, 14);
                        i -= 14;
                        this.index = 0;
                        this.mode = 4;
                        goto IL_6E2;
                    case 4:
                        goto IL_6E2;
                    case 5:
                        goto IL_7BB;
                    case 6:
                        goto IL_B66;
                    case 7:
                        goto IL_C2F;
                    case 8:
                        goto IL_CC4;
                    case 9:
                        goto IL_D0B;
                }
                break;
            IL_6E2:
                while (this.index < 4 + SupportClass.URShift(this.table, 10))
                {
                    while (i < 3)
                    {
                        if (num2 == 0)
                        {
                            goto IL_646;
                        }
                        r = 0;
                        num2--;
                        num3 |= (int)(z.next_in[num++] & 255) << i;
                        i += 8;
                    }
                    int[] arg_6D5_0 = this.blens;
                    int[] arg_6D1_0 = InfBlocks.border;
                    int num7 = this.index;
                    this.index = num7 + 1;
                    arg_6D5_0[arg_6D1_0[num7]] = (num3 & 7);
                    num3 = SupportClass.URShift(num3, 3);
                    i -= 3;
                }
                while (this.index < 19)
                {
                    int[] arg_71C_0 = this.blens;
                    int[] arg_71A_0 = InfBlocks.border;
                    int num7 = this.index;
                    this.index = num7 + 1;
                    arg_71C_0[arg_71A_0[num7]] = 0;
                }
                this.bb[0] = 7;
                num6 = InfTree.inflate_trees_bits(this.blens, this.bb, this.tb, this.hufts, z);
                if (num6 != 0)
                {
                    goto Block_33;
                }
                this.index = 0;
                this.mode = 5;
            IL_7BB:
                while (true)
                {
                    num6 = this.table;
                    if (this.index >= 258 + (num6 & 31) + (num6 >> 5 & 31))
                    {
                        break;
                    }
                    num6 = this.bb[0];
                    while (i < num6)
                    {
                        if (num2 == 0)
                        {
                            goto IL_7F2;
                        }
                        r = 0;
                        num2--;
                        num3 |= (int)(z.next_in[num++] & 255) << i;
                        i += 8;
                    }
                    int arg_868_0 = this.tb[0];
                    num6 = this.hufts[(this.tb[0] + (num3 & InfBlocks.inflate_mask[num6])) * 3 + 1];
                    int num8 = this.hufts[(this.tb[0] + (num3 & InfBlocks.inflate_mask[num6])) * 3 + 2];
                    if (num8 < 16)
                    {
                        num3 = SupportClass.URShift(num3, num6);
                        i -= num6;
                        int[] arg_8D4_0 = this.blens;
                        int num7 = this.index;
                        this.index = num7 + 1;
                        arg_8D4_0[num7] = num8;
                    }
                    else
                    {
                        int num9 = (num8 == 18) ? 7 : (num8 - 14);
                        int num10 = (num8 == 18) ? 11 : 3;
                        while (i < num6 + num9)
                        {
                            if (num2 == 0)
                            {
                                goto IL_902;
                            }
                            r = 0;
                            num2--;
                            num3 |= (int)(z.next_in[num++] & 255) << i;
                            i += 8;
                        }
                        num3 = SupportClass.URShift(num3, num6);
                        i -= num6;
                        num10 += (num3 & InfBlocks.inflate_mask[num9]);
                        num3 = SupportClass.URShift(num3, num9);
                        i -= num9;
                        num9 = this.index;
                        num6 = this.table;
                        if (num9 + num10 > 258 + (num6 & 31) + (num6 >> 5 & 31) || (num8 == 16 && num9 < 1))
                        {
                            goto IL_9CC;
                        }
                        num8 = ((num8 == 16) ? this.blens[num9 - 1] : 0);
                        do
                        {
                            this.blens[num9++] = num8;
                        }
                        while (--num10 != 0);
                        this.index = num9;
                    }
                }
                this.tb[0] = -1;
                int[] array5 = new int[1];
                int[] array6 = new int[1];
                int[] array7 = new int[1];
                int[] array8 = new int[1];
                array5[0] = 9;
                array6[0] = 6;
                num6 = this.table;
                num6 = InfTree.inflate_trees_dynamic(257 + (num6 & 31), 1 + (num6 >> 5 & 31), this.blens, array5, array6, array7, array8, this.hufts, z);
                if (num6 != 0)
                {
                    goto Block_47;
                }
                this.codes = new InfCodes(array5[0], array6[0], this.hufts, array7[0], this.hufts, array8[0], z);
                this.blens = null;
                this.mode = 6;
            IL_B66:
                this.bitb = num3;
                this.bitk = i;
                z.avail_in = num2;
                z.total_in += (long)(num - z.next_in_index);
                z.next_in_index = num;
                this.write = num4;
                if ((r = this.codes.proc(this, z, r)) != 1)
                {
                    goto Block_49;
                }
                r = 0;
                this.codes.free(z);
                num = z.next_in_index;
                num2 = z.avail_in;
                num3 = this.bitb;
                i = this.bitk;
                num4 = this.write;
                num5 = ((num4 < this.read) ? (this.read - num4 - 1) : (this.end - num4));
                if (this.last != 0)
                {
                    goto IL_C28;
                }
                this.mode = 0;
            }
            r = -2;
            this.bitb = num3;
            this.bitk = i;
            z.avail_in = num2;
            z.total_in += (long)(num - z.next_in_index);
            z.next_in_index = num;
            this.write = num4;
            return this.inflate_flush(z, r);
        IL_8C:
            this.bitb = num3;
            this.bitk = i;
            z.avail_in = num2;
            z.total_in += (long)(num - z.next_in_index);
            z.next_in_index = num;
            this.write = num4;
            return this.inflate_flush(z, r);
        IL_1CD:
            num3 = SupportClass.URShift(num3, 3);
            i -= 3;
            this.mode = 9;
            z.msg = "invalid block type";
            r = -3;
            this.bitb = num3;
            this.bitk = i;
            z.avail_in = num2;
            z.total_in += (long)(num - z.next_in_index);
            z.next_in_index = num;
            this.write = num4;
            return this.inflate_flush(z, r);
        IL_23D:
            this.bitb = num3;
            this.bitk = i;
            z.avail_in = num2;
            z.total_in += (long)(num - z.next_in_index);
            z.next_in_index = num;
            this.write = num4;
            return this.inflate_flush(z, r);
        Block_8:
            this.mode = 9;
            z.msg = "invalid stored block lengths";
            r = -3;
            this.bitb = num3;
            this.bitk = i;
            z.avail_in = num2;
            z.total_in += (long)(num - z.next_in_index);
            z.next_in_index = num;
            this.write = num4;
            return this.inflate_flush(z, r);
        Block_11:
            this.bitb = num3;
            this.bitk = i;
            z.avail_in = num2;
            z.total_in += (long)(num - z.next_in_index);
            z.next_in_index = num;
            this.write = num4;
            return this.inflate_flush(z, r);
        Block_21:
            this.bitb = num3;
            this.bitk = i;
            z.avail_in = num2;
            z.total_in += (long)(num - z.next_in_index);
            z.next_in_index = num;
            this.write = num4;
            return this.inflate_flush(z, r);
        IL_515:
            this.bitb = num3;
            this.bitk = i;
            z.avail_in = num2;
            z.total_in += (long)(num - z.next_in_index);
            z.next_in_index = num;
            this.write = num4;
            return this.inflate_flush(z, r);
        IL_5A3:
            this.mode = 9;
            z.msg = "too many length or distance symbols";
            r = -3;
            this.bitb = num3;
            this.bitk = i;
            z.avail_in = num2;
            z.total_in += (long)(num - z.next_in_index);
            z.next_in_index = num;
            this.write = num4;
            return this.inflate_flush(z, r);
        IL_646:
            this.bitb = num3;
            this.bitk = i;
            z.avail_in = num2;
            z.total_in += (long)(num - z.next_in_index);
            z.next_in_index = num;
            this.write = num4;
            return this.inflate_flush(z, r);
        Block_33:
            r = num6;
            if (r == -3)
            {
                this.blens = null;
                this.mode = 9;
            }
            this.bitb = num3;
            this.bitk = i;
            z.avail_in = num2;
            z.total_in += (long)(num - z.next_in_index);
            z.next_in_index = num;
            this.write = num4;
            return this.inflate_flush(z, r);
        IL_7F2:
            this.bitb = num3;
            this.bitk = i;
            z.avail_in = num2;
            z.total_in += (long)(num - z.next_in_index);
            z.next_in_index = num;
            this.write = num4;
            return this.inflate_flush(z, r);
        IL_902:
            this.bitb = num3;
            this.bitk = i;
            z.avail_in = num2;
            z.total_in += (long)(num - z.next_in_index);
            z.next_in_index = num;
            this.write = num4;
            return this.inflate_flush(z, r);
        IL_9CC:
            this.blens = null;
            this.mode = 9;
            z.msg = "invalid bit length repeat";
            r = -3;
            this.bitb = num3;
            this.bitk = i;
            z.avail_in = num2;
            z.total_in += (long)(num - z.next_in_index);
            z.next_in_index = num;
            this.write = num4;
            return this.inflate_flush(z, r);
        Block_47:
            if (num6 == -3)
            {
                this.blens = null;
                this.mode = 9;
            }
            r = num6;
            this.bitb = num3;
            this.bitk = i;
            z.avail_in = num2;
            z.total_in += (long)(num - z.next_in_index);
            z.next_in_index = num;
            this.write = num4;
            return this.inflate_flush(z, r);
        Block_49:
            return this.inflate_flush(z, r);
        IL_C28:
            this.mode = 7;
        IL_C2F:
            this.write = num4;
            r = this.inflate_flush(z, r);
            num4 = this.write;
            int arg_C69_0 = (num4 < this.read) ? (this.read - num4 - 1) : (this.end - num4);
            if (this.read != this.write)
            {
                this.bitb = num3;
                this.bitk = i;
                z.avail_in = num2;
                z.total_in += (long)(num - z.next_in_index);
                z.next_in_index = num;
                this.write = num4;
                return this.inflate_flush(z, r);
            }
            this.mode = 8;
        IL_CC4:
            r = 1;
            this.bitb = num3;
            this.bitk = i;
            z.avail_in = num2;
            z.total_in += (long)(num - z.next_in_index);
            z.next_in_index = num;
            this.write = num4;
            return this.inflate_flush(z, r);
        IL_D0B:
            r = -3;
            this.bitb = num3;
            this.bitk = i;
            z.avail_in = num2;
            z.total_in += (long)(num - z.next_in_index);
            z.next_in_index = num;
            this.write = num4;
            return this.inflate_flush(z, r);
        }
        internal void free(ZStream z)
        {
            this.reset(z, null);
            this.window = null;
            this.hufts = null;
        }
        internal void set_dictionary(byte[] d, int start, int n)
        {
            Array.Copy(d, start, this.window, 0, n);
            this.write = n;
            this.read = n;
        }
        internal int sync_point()
        {
            if (this.mode != 1)
            {
                return 0;
            }
            return 1;
        }
        internal int inflate_flush(ZStream z, int r)
        {
            int num = z.next_out_index;
            int num2 = this.read;
            int num3 = ((num2 <= this.write) ? this.write : this.end) - num2;
            if (num3 > z.avail_out)
            {
                num3 = z.avail_out;
            }
            if (num3 != 0 && r == -5)
            {
                r = 0;
            }
            z.avail_out -= num3;
            z.total_out += (long)num3;
            if (this.checkfn != null)
            {
                z.adler = (this.check = z._adler.adler32(this.check, this.window, num2, num3));
            }
            Array.Copy(this.window, num2, z.next_out, num, num3);
            num += num3;
            num2 += num3;
            if (num2 == this.end)
            {
                num2 = 0;
                if (this.write == this.end)
                {
                    this.write = 0;
                }
                num3 = this.write - num2;
                if (num3 > z.avail_out)
                {
                    num3 = z.avail_out;
                }
                if (num3 != 0 && r == -5)
                {
                    r = 0;
                }
                z.avail_out -= num3;
                z.total_out += (long)num3;
                if (this.checkfn != null)
                {
                    z.adler = (this.check = z._adler.adler32(this.check, this.window, num2, num3));
                }
                Array.Copy(this.window, num2, z.next_out, num, num3);
                num += num3;
                num2 += num3;
            }
            z.next_out_index = num;
            this.read = num2;
            return r;
        }
    }
}
