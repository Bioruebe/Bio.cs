# Changelog

## 2.3.0

- Added `CreateFile` and `FileSetTimes`
- Added `Concatenate` overload accepting an array of streams to `StreamExtensions`
- Changed `Tout`: the function can now be called without parameters

## 2.2.0

- Added options `CoutPrintTime` and `CoutKeepLog` to allow global console output configuration
- Added `RandomInt`, `HexToBytes`, `PrintTime` and `CoutGetLog` functions to main class
- Added I/O helper functions `PathContainsInvalidChars`, `PathExists`, `PathGetDirectory`, `FileDelete`, `CreateDirectoryStructure` and `IsDirectory`

## 2.1.0

- Added `ConcatenatedStream` and `OffsetStream` classes
- Added `Concatenate` and `ToByteArray` functions to `StreamExtensions`
- Added `Tout` (`Cout` with prepended timestamp)
- Added `Debug` overload which allows a dynamic amount of parameters
- Fixed RNG initialization in `RandomArray` (affects `RandomStream` as well)
- Fixed `StreamExtensions.Copy` not copying the whole stream if the position is not zero
- Fixed `StreamExtensions.Copy` returning `Stream` instead of `MemoryStream`

## 2.0.0

[Breaking changes](UPGRADE_V1_V2.md)

- Added many new extension methods for `Stream` and `BinaryReader` classes (namespace `BioLib.Streams`)
- Added file/path helper functions:
  - `GetSafeOutputPath`
  - `PathRemoveRelativeParts`
  - `FileOpen`
  - `FileMove`
  - `DirectoryIsEmpty`
- Added `HexDump` helper function and overloads for `Cout` to pretty-print streams and byte arrays automatically
- Added `RandomStream` function
- Added `ProgressWithoutDuplicates`
- Added support for .NET Framework 4.0
- Fixed `FileReplaceInvalidChars`: invalid characters in file names (such as `?`) were not replaced

## 1.0.0

Initial release