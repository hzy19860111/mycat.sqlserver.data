using System;
namespace zlib
{
	internal sealed class Inflate
	{
		private const int MAX_WBITS = 15;
		private const int PRESET_DICT = 32;
		internal const int Z_NO_FLUSH = 0;
		internal const int Z_PARTIAL_FLUSH = 1;
		internal const int Z_SYNC_FLUSH = 2;
		internal const int Z_FULL_FLUSH = 3;
		internal const int Z_FINISH = 4;
		private const int Z_DEFLATED = 8;
		private const int Z_OK = 0;
		private const int Z_STREAM_END = 1;
		private const int Z_NEED_DICT = 2;
		private const int Z_ERRNO = -1;
		private const int Z_STREAM_ERROR = -2;
		private const int Z_DATA_ERROR = -3;
		private const int Z_MEM_ERROR = -4;
		private const int Z_BUF_ERROR = -5;
		private const int Z_VERSION_ERROR = -6;
		private const int METHOD = 0;
		private const int FLAG = 1;
		private const int DICT4 = 2;
		private const int DICT3 = 3;
		private const int DICT2 = 4;
		private const int DICT1 = 5;
		private const int DICT0 = 6;
		private const int BLOCKS = 7;
		private const int CHECK4 = 8;
		private const int CHECK3 = 9;
		private const int CHECK2 = 10;
		private const int CHECK1 = 11;
		private const int DONE = 12;
		private const int BAD = 13;
		internal int mode;
		internal int method;
		internal long[] was = new long[1];
		internal long need;
		internal int marker;
		internal int nowrap;
		internal int wbits;
		internal InfBlocks blocks;
		private static byte[] mark = new byte[]
		{
			0,
			0,
			(byte)SupportClass.Identity(255L),
			(byte)SupportClass.Identity(255L)
		};
		internal int inflateReset(ZStream z)
		{
			if (z == null || z.istate == null)
			{
				return -2;
			}
			z.total_in = (z.total_out = 0L);
			z.msg = null;
			z.istate.mode = ((z.istate.nowrap != 0) ? 7 : 0);
			z.istate.blocks.reset(z, null);
			return 0;
		}
		internal int inflateEnd(ZStream z)
		{
			if (this.blocks != null)
			{
				this.blocks.free(z);
			}
			this.blocks = null;
			return 0;
		}
		internal int inflateInit(ZStream z, int w)
		{
			z.msg = null;
			this.blocks = null;
			this.nowrap = 0;
			if (w < 0)
			{
				w = -w;
				this.nowrap = 1;
			}
			if (w < 8 || w > 15)
			{
				this.inflateEnd(z);
				return -2;
			}
			this.wbits = w;
			z.istate.blocks = new InfBlocks(z, (z.istate.nowrap != 0) ? null : this, 1 << w);
			this.inflateReset(z);
			return 0;
		}
		internal int inflate(ZStream z, int f)
		{
			if (z == null || z.istate == null || z.next_in == null)
			{
				return -2;
			}
			f = ((f == 4) ? -5 : 0);
			int num = -5;
			int next_in_index;
			while (true)
			{
				switch (z.istate.mode)
				{
				case 0:
				{
					if (z.avail_in == 0)
					{
						return num;
					}
					num = f;
					z.avail_in--;
					z.total_in += 1L;
					Inflate arg_BC_0 = z.istate;
					byte[] arg_B9_0 = z.next_in;
					next_in_index = z.next_in_index;
					z.next_in_index = next_in_index + 1;
					if (((arg_BC_0.method = arg_B9_0[next_in_index]) & 15) != 8)
					{
						z.istate.mode = 13;
						z.msg = "unknown compression method";
						z.istate.marker = 5;
						continue;
					}
					if ((z.istate.method >> 4) + 8 > z.istate.wbits)
					{
						z.istate.mode = 13;
						z.msg = "invalid window size";
						z.istate.marker = 5;
						continue;
					}
					z.istate.mode = 1;
					goto IL_142;
				}
				case 1:
					goto IL_142;
				case 2:
					goto IL_1EA;
				case 3:
					goto IL_252;
				case 4:
					goto IL_2C2;
				case 5:
					goto IL_331;
				case 6:
					goto IL_3AB;
				case 7:
					num = z.istate.blocks.proc(z, num);
					if (num == -3)
					{
						z.istate.mode = 13;
						z.istate.marker = 0;
						continue;
					}
					if (num == 0)
					{
						num = f;
					}
					if (num != 1)
					{
						return num;
					}
					num = f;
					z.istate.blocks.reset(z, z.istate.was);
					if (z.istate.nowrap != 0)
					{
						z.istate.mode = 12;
						continue;
					}
					z.istate.mode = 8;
					goto IL_45C;
				case 8:
					goto IL_45C;
				case 9:
					goto IL_4C5;
				case 10:
					goto IL_536;
				case 11:
					goto IL_5A6;
				case 12:
					return 1;
				case 13:
					return -3;
				}
				break;
				IL_142:
				if (z.avail_in == 0)
				{
					return num;
				}
				num = f;
				z.avail_in--;
				z.total_in += 1L;
				byte[] arg_182_0 = z.next_in;
				next_in_index = z.next_in_index;
				z.next_in_index = next_in_index + 1;
				int num2 = arg_182_0[next_in_index] & 255;
				if (((z.istate.method << 8) + num2) % 31 != 0)
				{
					z.istate.mode = 13;
					z.msg = "incorrect header check";
					z.istate.marker = 5;
					continue;
				}
				if ((num2 & 32) == 0)
				{
					z.istate.mode = 7;
					continue;
				}
				goto IL_1DE;
				IL_5A6:
				if (z.avail_in == 0)
				{
					return num;
				}
				num = f;
				z.avail_in--;
				z.total_in += 1L;
				Inflate expr_5D5 = z.istate;
				long arg_5FB_0 = expr_5D5.need;
				byte[] arg_5F2_0 = z.next_in;
				next_in_index = z.next_in_index;
				z.next_in_index = next_in_index + 1;
				expr_5D5.need = arg_5FB_0 + (long)(arg_5F2_0[next_in_index] & 255uL);
				if ((int)z.istate.was[0] != (int)z.istate.need)
				{
					z.istate.mode = 13;
					z.msg = "incorrect data check";
					z.istate.marker = 5;
					continue;
				}
				goto IL_646;
				IL_536:
				if (z.avail_in == 0)
				{
					return num;
				}
				num = f;
				z.avail_in--;
				z.total_in += 1L;
				Inflate expr_565 = z.istate;
				long arg_593_0 = expr_565.need;
				byte[] arg_582_0 = z.next_in;
				next_in_index = z.next_in_index;
				z.next_in_index = next_in_index + 1;
				expr_565.need = arg_593_0 + ((arg_582_0[next_in_index] & 255L) << 8 & 65280L);
				z.istate.mode = 11;
				goto IL_5A6;
				IL_4C5:
				if (z.avail_in == 0)
				{
					return num;
				}
				num = f;
				z.avail_in--;
				z.total_in += 1L;
				Inflate expr_4F4 = z.istate;
				long arg_523_0 = expr_4F4.need;
				byte[] arg_511_0 = z.next_in;
				next_in_index = z.next_in_index;
				z.next_in_index = next_in_index + 1;
				expr_4F4.need = arg_523_0 + ((arg_511_0[next_in_index] & 255L) << 16 & 16711680L);
				z.istate.mode = 10;
				goto IL_536;
				IL_45C:
				if (z.avail_in == 0)
				{
					return num;
				}
				num = f;
				z.avail_in--;
				z.total_in += 1L;
				Inflate arg_4B3_0 = z.istate;
				byte[] arg_4A2_0 = z.next_in;
				next_in_index = z.next_in_index;
				z.next_in_index = next_in_index + 1;
				arg_4B3_0.need = (long)((arg_4A2_0[next_in_index] & 255) << 24 & -16777216);
				z.istate.mode = 9;
				goto IL_4C5;
			}
			return -2;
			IL_1DE:
			z.istate.mode = 2;
			IL_1EA:
			if (z.avail_in == 0)
			{
				return num;
			}
			num = f;
			z.avail_in--;
			z.total_in += 1L;
			Inflate arg_241_0 = z.istate;
			byte[] arg_230_0 = z.next_in;
			next_in_index = z.next_in_index;
			z.next_in_index = next_in_index + 1;
			arg_241_0.need = (long)((arg_230_0[next_in_index] & 255) << 24 & -16777216);
			z.istate.mode = 3;
			IL_252:
			if (z.avail_in == 0)
			{
				return num;
			}
			num = f;
			z.avail_in--;
			z.total_in += 1L;
			Inflate expr_281 = z.istate;
			long arg_2B0_0 = expr_281.need;
			byte[] arg_29E_0 = z.next_in;
			next_in_index = z.next_in_index;
			z.next_in_index = next_in_index + 1;
			expr_281.need = arg_2B0_0 + ((arg_29E_0[next_in_index] & 255L) << 16 & 16711680L);
			z.istate.mode = 4;
			IL_2C2:
			if (z.avail_in == 0)
			{
				return num;
			}
			num = f;
			z.avail_in--;
			z.total_in += 1L;
			Inflate expr_2F1 = z.istate;
			long arg_31F_0 = expr_2F1.need;
			byte[] arg_30E_0 = z.next_in;
			next_in_index = z.next_in_index;
			z.next_in_index = next_in_index + 1;
			expr_2F1.need = arg_31F_0 + ((arg_30E_0[next_in_index] & 255L) << 8 & 65280L);
			z.istate.mode = 5;
			IL_331:
			if (z.avail_in == 0)
			{
				return num;
			}
			z.avail_in--;
			z.total_in += 1L;
			Inflate expr_360 = z.istate;
			long arg_386_0 = expr_360.need;
			byte[] arg_37D_0 = z.next_in;
			next_in_index = z.next_in_index;
			z.next_in_index = next_in_index + 1;
			expr_360.need = arg_386_0 + (long)(arg_37D_0[next_in_index] & 255uL);
			z.adler = z.istate.need;
			z.istate.mode = 6;
			return 2;
			IL_3AB:
			z.istate.mode = 13;
			z.msg = "need dictionary";
			z.istate.marker = 0;
			return -2;
			IL_646:
			z.istate.mode = 12;
			return 1;
		}
		internal int inflateSetDictionary(ZStream z, byte[] dictionary, int dictLength)
		{
			int start = 0;
			int num = dictLength;
			if (z == null || z.istate == null || z.istate.mode != 6)
			{
				return -2;
			}
			if (z._adler.adler32(1L, dictionary, 0, dictLength) != z.adler)
			{
				return -3;
			}
			z.adler = z._adler.adler32(0L, null, 0, 0);
			if (num >= 1 << z.istate.wbits)
			{
				num = (1 << z.istate.wbits) - 1;
				start = dictLength - num;
			}
			z.istate.blocks.set_dictionary(dictionary, start, num);
			z.istate.mode = 7;
			return 0;
		}
		internal int inflateSync(ZStream z)
		{
			if (z == null || z.istate == null)
			{
				return -2;
			}
			if (z.istate.mode != 13)
			{
				z.istate.mode = 13;
				z.istate.marker = 0;
			}
			int num;
			if ((num = z.avail_in) == 0)
			{
				return -5;
			}
			int num2 = z.next_in_index;
			int num3 = z.istate.marker;
			while (num != 0 && num3 < 4)
			{
				if (z.next_in[num2] == Inflate.mark[num3])
				{
					num3++;
				}
				else if (z.next_in[num2] != 0)
				{
					num3 = 0;
				}
				else
				{
					num3 = 4 - num3;
				}
				num2++;
				num--;
			}
			z.total_in += (long)(num2 - z.next_in_index);
			z.next_in_index = num2;
			z.avail_in = num;
			z.istate.marker = num3;
			if (num3 != 4)
			{
				return -3;
			}
			long total_in = z.total_in;
			long total_out = z.total_out;
			this.inflateReset(z);
			z.total_in = total_in;
			z.total_out = total_out;
			z.istate.mode = 7;
			return 0;
		}
		internal int inflateSyncPoint(ZStream z)
		{
			if (z == null || z.istate == null || z.istate.blocks == null)
			{
				return -2;
			}
			return z.istate.blocks.sync_point();
		}
	}
}
