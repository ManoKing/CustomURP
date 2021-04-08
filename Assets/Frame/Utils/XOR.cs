namespace DH.Frame.Res
{
	public class XOR
	{
		private const int count = 20;
		private const string password = "dh123456";

		public static byte[] Decrypt(byte[] data)
		{
			byte[] tmp = data;

			int length = tmp.Length;
			if (length > count)
			{
				length = count;
			}
			packXor(tmp, length, password);
			return tmp;
		}

		public static byte[] Encrypt(byte[] data)
		{
			int length = data.Length;
			if (length > count)
			{
				length = count;
			}
			packXor(data, length, password);
			return data;
		}

		private static void packXor(byte[] _data, int _len, string _pstr)
		{
			int length = _len;
			int strCount = 0;

			for (int i = 0; i < length; ++i)
			{
				if (strCount >= _pstr.Length)
					strCount = 0;
				_data[i] ^= (byte)_pstr[strCount++];
			}
		}
	}
}