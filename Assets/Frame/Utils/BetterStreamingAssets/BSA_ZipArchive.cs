using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Better.StreamingAssets.ZipArchive
{
	internal struct Zip64EndOfCentralDirectoryLocator
	{
		public const uint SignatureConstant = 0x07064B50;
		public const int SizeOfBlockWithoutSignature = 16;

		public uint NumberOfDiskWithZip64EOCD;
		public ulong OffsetOfZip64EOCD;
		public uint TotalNumberOfDisks;

		public static bool TryReadBlock(BinaryReader reader, out Zip64EndOfCentralDirectoryLocator zip64EOCDLocator)
		{
			zip64EOCDLocator = new Zip64EndOfCentralDirectoryLocator();

			if (reader.ReadUInt32() != SignatureConstant)
				return false;

			zip64EOCDLocator.NumberOfDiskWithZip64EOCD = reader.ReadUInt32();
			zip64EOCDLocator.OffsetOfZip64EOCD = reader.ReadUInt64();
			zip64EOCDLocator.TotalNumberOfDisks = reader.ReadUInt32();
			return true;
		}
	}

	internal struct Zip64EndOfCentralDirectoryRecord
	{
		public uint NumberOfDiskWithStartOfCD;
		public ulong NumberOfEntriesOnThisDisk;
		public ulong NumberOfEntriesTotal;
		public uint NumberOfThisDisk;
		public ulong OffsetOfCentralDirectory;
		public ulong SizeOfCentralDirectory;
		public ulong SizeOfThisRecord;
		public ushort VersionMadeBy;
		public ushort VersionNeededToExtract;
		private const ulong NormalSize = 0x2C;
		private const uint SignatureConstant = 0x06064B50;

		public static bool TryReadBlock(BinaryReader reader, out Zip64EndOfCentralDirectoryRecord zip64EOCDRecord)
		{
			zip64EOCDRecord = new Zip64EndOfCentralDirectoryRecord();

			if (reader.ReadUInt32() != SignatureConstant)
				return false;

			zip64EOCDRecord.SizeOfThisRecord = reader.ReadUInt64();
			zip64EOCDRecord.VersionMadeBy = reader.ReadUInt16();
			zip64EOCDRecord.VersionNeededToExtract = reader.ReadUInt16();
			zip64EOCDRecord.NumberOfThisDisk = reader.ReadUInt32();
			zip64EOCDRecord.NumberOfDiskWithStartOfCD = reader.ReadUInt32();
			zip64EOCDRecord.NumberOfEntriesOnThisDisk = reader.ReadUInt64();
			zip64EOCDRecord.NumberOfEntriesTotal = reader.ReadUInt64();
			zip64EOCDRecord.SizeOfCentralDirectory = reader.ReadUInt64();
			zip64EOCDRecord.OffsetOfCentralDirectory = reader.ReadUInt64();

			return true;
		}
	}

	internal struct Zip64ExtraField
	{
		public const int OffsetToFirstField = 4;
		private const ushort TagConstant = 1;

		private long? _compressedSize;
		private long? _localHeaderOffset;
		private ushort _size;
		private int? _startDiskNumber;
		private long? _uncompressedSize;

		public long? CompressedSize
		{
			get { return _compressedSize; }
			set { _compressedSize = value; UpdateSize(); }
		}

		public long? LocalHeaderOffset
		{
			get { return _localHeaderOffset; }
			set { _localHeaderOffset = value; UpdateSize(); }
		}

		public int? StartDiskNumber { get { return _startDiskNumber; } }

		public long? UncompressedSize
		{
			get { return _uncompressedSize; }
			set { _uncompressedSize = value; UpdateSize(); }
		}

		public static Zip64ExtraField GetJustZip64Block(Stream extraFieldStream,
			bool readUncompressedSize, bool readCompressedSize,
			bool readLocalHeaderOffset, bool readStartDiskNumber)
		{
			Zip64ExtraField zip64Field;
			using (BinaryReader reader = new BinaryReader(extraFieldStream))
			{
				ZipGenericExtraField currentExtraField;
				while (ZipGenericExtraField.TryReadBlock(reader, extraFieldStream.Length, out currentExtraField))
				{
					if (TryGetZip64BlockFromGenericExtraField(currentExtraField, readUncompressedSize,
								readCompressedSize, readLocalHeaderOffset, readStartDiskNumber, out zip64Field))
					{
						return zip64Field;
					}
				}
			}

			zip64Field = new Zip64ExtraField();

			zip64Field._compressedSize = null;
			zip64Field._uncompressedSize = null;
			zip64Field._localHeaderOffset = null;
			zip64Field._startDiskNumber = null;

			return zip64Field;
		}

		private static bool TryGetZip64BlockFromGenericExtraField(ZipGenericExtraField extraField,
			bool readUncompressedSize, bool readCompressedSize,
			bool readLocalHeaderOffset, bool readStartDiskNumber,
			out Zip64ExtraField zip64Block)
		{
			zip64Block = new Zip64ExtraField();

			zip64Block._compressedSize = null;
			zip64Block._uncompressedSize = null;
			zip64Block._localHeaderOffset = null;
			zip64Block._startDiskNumber = null;

			if (extraField.Tag != TagConstant)
				return false;

			MemoryStream ms = null;
			try
			{
				ms = new MemoryStream(extraField.Data);
				using (BinaryReader reader = new BinaryReader(ms))
				{
					ms = null;

					zip64Block._size = extraField.Size;

					ushort expectedSize = 0;

					if (readUncompressedSize) expectedSize += 8;
					if (readCompressedSize) expectedSize += 8;
					if (readLocalHeaderOffset) expectedSize += 8;
					if (readStartDiskNumber) expectedSize += 4;

					if (expectedSize != zip64Block._size)
						return false;

					if (readUncompressedSize) zip64Block._uncompressedSize = reader.ReadInt64();
					if (readCompressedSize) zip64Block._compressedSize = reader.ReadInt64();
					if (readLocalHeaderOffset) zip64Block._localHeaderOffset = reader.ReadInt64();
					if (readStartDiskNumber) zip64Block._startDiskNumber = reader.ReadInt32();

					if (zip64Block._uncompressedSize < 0) throw new ZipArchiveException("FieldTooBigUncompressedSize");
					if (zip64Block._compressedSize < 0) throw new ZipArchiveException("FieldTooBigCompressedSize");
					if (zip64Block._localHeaderOffset < 0) throw new ZipArchiveException("FieldTooBigLocalHeaderOffset");
					if (zip64Block._startDiskNumber < 0) throw new ZipArchiveException("FieldTooBigStartDiskNumber");

					return true;
				}
			}
			finally
			{
				if (ms != null)
					ms.Dispose();
			}
		}

		private void UpdateSize()
		{
			_size = 0;
			if (_uncompressedSize != null) _size += 8;
			if (_compressedSize != null) _size += 8;
			if (_localHeaderOffset != null) _size += 8;
			if (_startDiskNumber != null) _size += 4;
		}
	}

	internal struct ZipCentralDirectoryFileHeader
	{
		public const uint SignatureConstant = 0x02014B50;
		public long CompressedSize;
		public ushort CompressionMethod;
		public uint Crc32;
		public int DiskNumberStart;
		public uint ExternalFileAttributes;
		public ushort ExtraFieldLength;
		public List<ZipGenericExtraField> ExtraFields;
		public byte[] FileComment;
		public ushort FileCommentLength;
		public byte[] Filename;
		public ushort FilenameLength;
		public ushort GeneralPurposeBitFlag;
		public ushort InternalFileAttributes;
		public uint LastModified;
		public long RelativeOffsetOfLocalHeader;

		public long UncompressedSize;

		public byte VersionMadeByCompatibility;
		public byte VersionMadeBySpecification;
		public ushort VersionNeededToExtract;

		public static bool TryReadBlock(BinaryReader reader, out ZipCentralDirectoryFileHeader header)
		{
			header = new ZipCentralDirectoryFileHeader();

			if (reader.ReadUInt32() != SignatureConstant)
				return false;
			header.VersionMadeBySpecification = reader.ReadByte();
			header.VersionMadeByCompatibility = reader.ReadByte();
			header.VersionNeededToExtract = reader.ReadUInt16();
			header.GeneralPurposeBitFlag = reader.ReadUInt16();
			header.CompressionMethod = reader.ReadUInt16();
			header.LastModified = reader.ReadUInt32();
			header.Crc32 = reader.ReadUInt32();
			uint compressedSizeSmall = reader.ReadUInt32();
			uint uncompressedSizeSmall = reader.ReadUInt32();
			header.FilenameLength = reader.ReadUInt16();
			header.ExtraFieldLength = reader.ReadUInt16();
			header.FileCommentLength = reader.ReadUInt16();
			ushort diskNumberStartSmall = reader.ReadUInt16();
			header.InternalFileAttributes = reader.ReadUInt16();
			header.ExternalFileAttributes = reader.ReadUInt32();
			uint relativeOffsetOfLocalHeaderSmall = reader.ReadUInt32();

			header.Filename = reader.ReadBytes(header.FilenameLength);

			bool uncompressedSizeInZip64 = uncompressedSizeSmall == ZipHelper.Mask32Bit;
			bool compressedSizeInZip64 = compressedSizeSmall == ZipHelper.Mask32Bit;
			bool relativeOffsetInZip64 = relativeOffsetOfLocalHeaderSmall == ZipHelper.Mask32Bit;
			bool diskNumberStartInZip64 = diskNumberStartSmall == ZipHelper.Mask16Bit;

			Zip64ExtraField zip64;

			long endExtraFields = reader.BaseStream.Position + header.ExtraFieldLength;
			using (Stream str = new SubReadOnlyStream(reader.BaseStream, reader.BaseStream.Position, header.ExtraFieldLength, leaveOpen: true))
			{
				header.ExtraFields = null;
				zip64 = Zip64ExtraField.GetJustZip64Block(str,
						uncompressedSizeInZip64, compressedSizeInZip64,
						relativeOffsetInZip64, diskNumberStartInZip64);
			}

			reader.BaseStream.AdvanceToPosition(endExtraFields);

			reader.BaseStream.Position += header.FileCommentLength;
			header.FileComment = null;

			header.UncompressedSize = zip64.UncompressedSize == null
													? uncompressedSizeSmall
													: zip64.UncompressedSize.Value;
			header.CompressedSize = zip64.CompressedSize == null
													? compressedSizeSmall
													: zip64.CompressedSize.Value;
			header.RelativeOffsetOfLocalHeader = zip64.LocalHeaderOffset == null
													? relativeOffsetOfLocalHeaderSmall
													: zip64.LocalHeaderOffset.Value;
			header.DiskNumberStart = zip64.StartDiskNumber == null
													? diskNumberStartSmall
													: zip64.StartDiskNumber.Value;

			return true;
		}
	}

	internal struct ZipEndOfCentralDirectoryBlock
	{
		public const uint SignatureConstant = 0x06054B50;
		public const int SizeOfBlockWithoutSignature = 18;
		public byte[] ArchiveComment;
		public ushort NumberOfEntriesInTheCentralDirectory;
		public ushort NumberOfEntriesInTheCentralDirectoryOnThisDisk;
		public ushort NumberOfTheDiskWithTheStartOfTheCentralDirectory;
		public ushort NumberOfThisDisk;
		public uint OffsetOfStartOfCentralDirectoryWithRespectToTheStartingDiskNumber;
		public uint Signature;
		public uint SizeOfCentralDirectory;

		public static bool TryReadBlock(BinaryReader reader, out ZipEndOfCentralDirectoryBlock eocdBlock)
		{
			eocdBlock = new ZipEndOfCentralDirectoryBlock();
			if (reader.ReadUInt32() != SignatureConstant)
				return false;

			eocdBlock.Signature = SignatureConstant;
			eocdBlock.NumberOfThisDisk = reader.ReadUInt16();
			eocdBlock.NumberOfTheDiskWithTheStartOfTheCentralDirectory = reader.ReadUInt16();
			eocdBlock.NumberOfEntriesInTheCentralDirectoryOnThisDisk = reader.ReadUInt16();
			eocdBlock.NumberOfEntriesInTheCentralDirectory = reader.ReadUInt16();
			eocdBlock.SizeOfCentralDirectory = reader.ReadUInt32();
			eocdBlock.OffsetOfStartOfCentralDirectoryWithRespectToTheStartingDiskNumber = reader.ReadUInt32();

			ushort commentLength = reader.ReadUInt16();
			eocdBlock.ArchiveComment = reader.ReadBytes(commentLength);

			return true;
		}
	}

	internal struct ZipGenericExtraField
	{
		private const int SizeOfHeader = 4;

		private byte[] _data;
		private ushort _size;
		private ushort _tag;
		public byte[] Data { get { return _data; } }

		public ushort Size { get { return _size; } }

		public ushort Tag { get { return _tag; } }

		public static bool TryReadBlock(BinaryReader reader, long endExtraField, out ZipGenericExtraField field)
		{
			field = new ZipGenericExtraField();

			if (endExtraField - reader.BaseStream.Position < 4)
				return false;

			field._tag = reader.ReadUInt16();
			field._size = reader.ReadUInt16();

			if (endExtraField - reader.BaseStream.Position < field._size)
				return false;

			field._data = reader.ReadBytes(field._size);
			return true;
		}
	}

	internal struct ZipLocalFileHeader
	{
		public const uint DataDescriptorSignature = 0x08074B50;
		public const int OffsetToBitFlagFromHeaderStart = 6;
		public const int OffsetToCrcFromHeaderStart = 14;
		public const uint SignatureConstant = 0x04034B50;
		public const int SizeOfLocalHeader = 30;

		public static bool TrySkipBlock(BinaryReader reader)
		{
			const int OffsetToFilenameLength = 22;

			if (reader.ReadUInt32() != SignatureConstant)
				return false;

			if (reader.BaseStream.Length < reader.BaseStream.Position + OffsetToFilenameLength)
				return false;

			reader.BaseStream.Seek(OffsetToFilenameLength, SeekOrigin.Current);

			ushort filenameLength = reader.ReadUInt16();
			ushort extraFieldLength = reader.ReadUInt16();

			if (reader.BaseStream.Length < reader.BaseStream.Position + filenameLength + extraFieldLength)
				return false;

			reader.BaseStream.Seek(filenameLength + extraFieldLength, SeekOrigin.Current);

			return true;
		}
	}

	public static class ZipArchiveUtils
	{
		public static void ReadEndOfCentralDirectory(Stream stream, BinaryReader reader, out long expectedNumberOfEntries, out long centralDirectoryStart)
		{
			try
			{
				stream.Seek(-ZipEndOfCentralDirectoryBlock.SizeOfBlockWithoutSignature, SeekOrigin.End);
				if (!ZipHelper.SeekBackwardsToSignature(stream, ZipEndOfCentralDirectoryBlock.SignatureConstant))
					throw new ZipArchiveException("SignatureConstant");

				long eocdStart = stream.Position;

				ZipEndOfCentralDirectoryBlock eocd;
				bool eocdProper = ZipEndOfCentralDirectoryBlock.TryReadBlock(reader, out eocd);
				Debug.Assert(eocdProper);

				if (eocd.NumberOfThisDisk != eocd.NumberOfTheDiskWithTheStartOfTheCentralDirectory)
					throw new ZipArchiveException("SplitSpanned");

				centralDirectoryStart = eocd.OffsetOfStartOfCentralDirectoryWithRespectToTheStartingDiskNumber;
				if (eocd.NumberOfEntriesInTheCentralDirectory != eocd.NumberOfEntriesInTheCentralDirectoryOnThisDisk)
					throw new ZipArchiveException("SplitSpanned");
				expectedNumberOfEntries = eocd.NumberOfEntriesInTheCentralDirectory;

				if (eocd.NumberOfThisDisk == ZipHelper.Mask16Bit ||
					eocd.OffsetOfStartOfCentralDirectoryWithRespectToTheStartingDiskNumber == ZipHelper.Mask32Bit ||
					eocd.NumberOfEntriesInTheCentralDirectory == ZipHelper.Mask16Bit)
				{
					stream.Seek(eocdStart - Zip64EndOfCentralDirectoryLocator.SizeOfBlockWithoutSignature, SeekOrigin.Begin);

					if (ZipHelper.SeekBackwardsToSignature(stream, Zip64EndOfCentralDirectoryLocator.SignatureConstant))
					{
						Zip64EndOfCentralDirectoryLocator locator;
						bool zip64eocdLocatorProper = Zip64EndOfCentralDirectoryLocator.TryReadBlock(reader, out locator);
						Debug.Assert(zip64eocdLocatorProper);

						if (locator.OffsetOfZip64EOCD > long.MaxValue)
							throw new ZipArchiveException("FieldTooBigOffsetToZip64EOCD");
						long zip64EOCDOffset = (long)locator.OffsetOfZip64EOCD;

						stream.Seek(zip64EOCDOffset, SeekOrigin.Begin);

						Zip64EndOfCentralDirectoryRecord record;
						if (!Zip64EndOfCentralDirectoryRecord.TryReadBlock(reader, out record))
							throw new ZipArchiveException("Zip64EOCDNotWhereExpected");

						if (record.NumberOfEntriesTotal > long.MaxValue)
							throw new ZipArchiveException("FieldTooBigNumEntries");
						if (record.OffsetOfCentralDirectory > long.MaxValue)
							throw new ZipArchiveException("FieldTooBigOffsetToCD");
						if (record.NumberOfEntriesTotal != record.NumberOfEntriesOnThisDisk)
							throw new ZipArchiveException("SplitSpanned");

						expectedNumberOfEntries = (long)record.NumberOfEntriesTotal;
						centralDirectoryStart = (long)record.OffsetOfCentralDirectory;
					}
				}

				if (centralDirectoryStart > stream.Length)
				{
					throw new ZipArchiveException("FieldTooBigOffsetToCD");
				}
			}
			catch (EndOfStreamException ex)
			{
				throw new ZipArchiveException("CDCorrupt", ex);
			}
			catch (IOException ex)
			{
				throw new ZipArchiveException("CDCorrupt", ex);
			}
		}
	}

	public class ZipArchiveException : Exception
	{
		public ZipArchiveException(string msg) : base(msg)
		{ }

		public ZipArchiveException(string msg, Exception inner)
			: base(msg, inner)
		{
		}
	}

	internal static class ZipHelper
	{
		internal const ushort Mask16Bit = 0xFFFF;
		internal const uint Mask32Bit = 0xFFFFFFFF;
		private const int BackwardsSeekingBufferSize = 32;

		internal static void AdvanceToPosition(this Stream stream, long position)
		{
			long numBytesLeft = position - stream.Position;
			Debug.Assert(numBytesLeft >= 0);
			while (numBytesLeft != 0)
			{
				const int throwAwayBufferSize = 64;
				int numBytesToSkip = (numBytesLeft > throwAwayBufferSize) ? throwAwayBufferSize : (int)numBytesLeft;
				int numBytesActuallySkipped = stream.Read(new byte[throwAwayBufferSize], 0, numBytesToSkip);
				if (numBytesActuallySkipped == 0)
					throw new IOException();
				numBytesLeft -= numBytesActuallySkipped;
			}
		}

		internal static void ReadBytes(Stream stream, byte[] buffer, int bytesToRead)
		{
			int bytesLeftToRead = bytesToRead;

			int totalBytesRead = 0;

			while (bytesLeftToRead > 0)
			{
				int bytesRead = stream.Read(buffer, totalBytesRead, bytesLeftToRead);
				if (bytesRead == 0) throw new IOException();

				totalBytesRead += bytesRead;
				bytesLeftToRead -= bytesRead;
			}
		}

		internal static bool SeekBackwardsToSignature(Stream stream, uint signatureToFind)
		{
			int bufferPointer = 0;
			uint currentSignature = 0;
			byte[] buffer = new byte[BackwardsSeekingBufferSize];

			bool outOfBytes = false;
			bool signatureFound = false;

			while (!signatureFound && !outOfBytes)
			{
				outOfBytes = SeekBackwardsAndRead(stream, buffer, out bufferPointer);

				Debug.Assert(bufferPointer < buffer.Length);

				while (bufferPointer >= 0 && !signatureFound)
				{
					currentSignature = (currentSignature << 8) | ((uint)buffer[bufferPointer]);
					if (currentSignature == signatureToFind)
					{
						signatureFound = true;
					}
					else
					{
						bufferPointer--;
					}
				}
			}

			if (!signatureFound)
			{
				return false;
			}
			else
			{
				stream.Seek(bufferPointer, SeekOrigin.Current);
				return true;
			}
		}

		private static bool SeekBackwardsAndRead(Stream stream, byte[] buffer, out int bufferPointer)
		{
			if (stream.Position >= buffer.Length)
			{
				stream.Seek(-buffer.Length, SeekOrigin.Current);
				ReadBytes(stream, buffer, buffer.Length);
				stream.Seek(-buffer.Length, SeekOrigin.Current);
				bufferPointer = buffer.Length - 1;
				return false;
			}
			else
			{
				int bytesToRead = (int)stream.Position;
				stream.Seek(0, SeekOrigin.Begin);
				ReadBytes(stream, buffer, bytesToRead);
				stream.Seek(0, SeekOrigin.Begin);
				bufferPointer = bytesToRead - 1;
				return true;
			}
		}
	}
}