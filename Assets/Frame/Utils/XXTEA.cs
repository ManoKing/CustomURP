namespace Frame.Res
{
	using System;
	using System.Text;

	public sealed class XXTEA
	{
		private const UInt32 delta = 0x9E3779B9;
		private static readonly UTF8Encoding utf8 = new UTF8Encoding();

		private XXTEA()
		{
		}

		public static Byte[] Decrypt(Byte[] data, Byte[] key)
		{
			if (data.Length == 0)
			{
				return data;
			}
			return ToByteArray(Decrypt(ToUInt32Array(data, false), ToUInt32Array(FixKey(key), false)), true);
		}

		public static Byte[] Decrypt(Byte[] data, String key)
		{
			return Decrypt(data, utf8.GetBytes(key));
		}

		public static Byte[] DecryptBase64String(String data, Byte[] key)
		{
			return Decrypt(Convert.FromBase64String(data), key);
		}

		public static Byte[] DecryptBase64String(String data, String key)
		{
			return Decrypt(Convert.FromBase64String(data), key);
		}

		public static String DecryptBase64StringToString(String data, Byte[] key)
		{
			return utf8.GetString(DecryptBase64String(data, key));
		}

		public static String DecryptBase64StringToString(String data, String key)
		{
			return utf8.GetString(DecryptBase64String(data, key));
		}

		public static String DecryptToString(Byte[] data, Byte[] key)
		{
			return utf8.GetString(Decrypt(data, key));
		}

		public static String DecryptToString(Byte[] data, String key)
		{
			return utf8.GetString(Decrypt(data, key));
		}

		public static Byte[] Encrypt(Byte[] data, Byte[] key)
		{
			if (data.Length == 0)
			{
				return data;
			}
			return ToByteArray(Encrypt(ToUInt32Array(data, true), ToUInt32Array(FixKey(key), false)), false);
		}

		public static Byte[] Encrypt(String data, Byte[] key)
		{
			return Encrypt(utf8.GetBytes(data), key);
		}

		public static Byte[] Encrypt(Byte[] data, String key)
		{
			return Encrypt(data, utf8.GetBytes(key));
		}

		public static Byte[] Encrypt(String data, String key)
		{
			return Encrypt(utf8.GetBytes(data), utf8.GetBytes(key));
		}

		public static String EncryptToBase64String(Byte[] data, Byte[] key)
		{
			return Convert.ToBase64String(Encrypt(data, key));
		}

		public static String EncryptToBase64String(String data, Byte[] key)
		{
			return Convert.ToBase64String(Encrypt(data, key));
		}

		public static String EncryptToBase64String(Byte[] data, String key)
		{
			return Convert.ToBase64String(Encrypt(data, key));
		}

		public static String EncryptToBase64String(String data, String key)
		{
			return Convert.ToBase64String(Encrypt(data, key));
		}

		private static UInt32[] Decrypt(UInt32[] v, UInt32[] k)
		{
			Int32 n = v.Length - 1;
			if (n < 1)
			{
				return v;
			}
			UInt32 z, y = v[0], sum, e;
			Int32 p, q = 6 + 52 / (n + 1);
			unchecked
			{
				sum = (UInt32)(q * delta);
				while (sum != 0)
				{
					e = sum >> 2 & 3;
					for (p = n; p > 0; p--)
					{
						z = v[p - 1];
						y = v[p] -= MX(sum, y, z, p, e, k);
					}
					z = v[n];
					y = v[0] -= MX(sum, y, z, p, e, k);
					sum -= delta;
				}
			}
			return v;
		}

		private static UInt32[] Encrypt(UInt32[] v, UInt32[] k)
		{
			Int32 n = v.Length - 1;
			if (n < 1)
			{
				return v;
			}
			UInt32 z = v[n], y, sum = 0, e;
			Int32 p, q = 6 + 52 / (n + 1);
			unchecked
			{
				while (0 < q--)
				{
					sum += delta;
					e = sum >> 2 & 3;
					for (p = 0; p < n; p++)
					{
						y = v[p + 1];
						z = v[p] += MX(sum, y, z, p, e, k);
					}
					y = v[0];
					z = v[n] += MX(sum, y, z, p, e, k);
				}
			}
			return v;
		}

		private static Byte[] FixKey(Byte[] key)
		{
			if (key.Length == 16) return key;
			Byte[] fixedkey = new Byte[16];
			if (key.Length < 16)
			{
				key.CopyTo(fixedkey, 0);
			}
			else
			{
				Array.Copy(key, 0, fixedkey, 0, 16);
			}
			return fixedkey;
		}

		private static UInt32 MX(UInt32 sum, UInt32 y, UInt32 z, Int32 p, UInt32 e, UInt32[] k)
		{
			return (z >> 5 ^ y << 2) + (y >> 3 ^ z << 4) ^ (sum ^ y) + (k[p & 3 ^ e] ^ z);
		}

		private static Byte[] ToByteArray(UInt32[] data, Boolean includeLength)
		{
			Int32 n = data.Length << 2;
			if (includeLength)
			{
				Int32 m = (Int32)data[data.Length - 1];
				n -= 4;
				if ((m < n - 3) || (m > n))
				{
					return null;
				}
				n = m;
			}
			Byte[] result = new Byte[n];
			for (Int32 i = 0; i < n; i++)
			{
				result[i] = (Byte)(data[i >> 2] >> ((i & 3) << 3));
			}
			return result;
		}

		private static UInt32[] ToUInt32Array(Byte[] data, Boolean includeLength)
		{
			Int32 length = data.Length;
			Int32 n = (((length & 3) == 0) ? (length >> 2) : ((length >> 2) + 1));
			UInt32[] result;
			if (includeLength)
			{
				result = new UInt32[n + 1];
				result[n] = (UInt32)length;
			}
			else
			{
				result = new UInt32[n];
			}
			for (Int32 i = 0; i < length; i++)
			{
				result[i >> 2] |= (UInt32)data[i] << ((i & 3) << 3);
			}
			return result;
		}
	}

	public class XXTEA_Algorithm
	{
		private const int count = 20;
		private static byte[] password = new byte[10] { 1, 3, 4, 2, 6, 7, 8, 9, 10, 5 };

		public static byte[] Decrypt(byte[] data)
		{
			byte[] partData = GetPartData(data);
			byte[] newData = XXTEA.Decrypt(partData, password);
			data = SetPartData(newData, data);
			return data;
		}

		public static byte[] Encrypt(byte[] data)
		{
			byte[] partData = GetPartData(data);
			byte[] newData = XXTEA.Encrypt(partData, password);
			data = SetPartData(newData, data);
			return data;
		}

		private static byte[] GetPartData(byte[] data)
		{
			int length = data.Length;
			if (length > count)
			{
				length = count;
			}
			byte[] partData = new byte[length];
			for (int i = 0; i < length; i++)
			{
				partData[i] = data[i];
			}
			return partData;
		}

		private static byte[] SetPartData(byte[] partData, byte[] data)
		{
			for (int i = 0; i < partData.Length; i++)
			{
				data[i] = partData[i];
			}
			return data;
		}
	}
}