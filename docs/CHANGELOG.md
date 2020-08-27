# Changelog
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