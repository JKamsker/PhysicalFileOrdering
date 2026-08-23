# PhysicalFileOrdering

[![CI](https://github.com/JKamsker/PhysicalFileOrdering/actions/workflows/ci.yml/badge.svg)](https://github.com/JKamsker/PhysicalFileOrdering/actions/workflows/ci.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)

PhysicalFileOrdering is a small .NET library that orders files by the first
usable on-disk extent reported by the operating system. Reading files in that
order can reduce seek overhead on a mechanical hard drive.

The library supports .NET 8 and .NET 10 and runs on Windows, Linux, and macOS.
It has no runtime package dependencies.

> This is an HDD optimization heuristic. SSDs and NVMe drives normally do not
> benefit, and layered, virtual, encrypted, RAID, or copy-on-write storage may
> not expose offsets that match a physical disk exactly.

## Quick start

Reference `src/PhysicalFileOrdering/PhysicalFileOrdering.csproj` from your
application, then create the platform-appropriate orderer:

```csharp
using PhysicalFileOrdering;

IPhysicalFileOrderer orderer = PhysicalFileOrderers.CreateDefault();
IReadOnlyList<string> orderedFiles = orderer.Sort(files);

foreach (string path in orderedFiles)
{
    using var stream = new FileStream(
        path,
        FileMode.Open,
        FileAccess.Read,
        FileShare.Read,
        bufferSize: 1024 * 1024,
        FileOptions.SequentialScan);

    // Consume the entire file before advancing to the next one.
}
```

`Sort` returns normalized absolute paths. It groups files by volume in the
order each volume first appears, then orders known positions from low to high.
Files whose position cannot be determined retain a stable input order and come
after known positions on the same volume.

## Platform support

| OS | Automatically selected filesystems | Native mechanism |
| --- | --- | --- |
| Windows | NTFS, ReFS, FAT/FAT32, exFAT, UDF | `FSCTL_GET_RETRIEVAL_POINTERS` |
| Linux | ext2/3/4, XFS, Btrfs, F2FS | `FS_IOC_FIEMAP` |
| macOS | APFS, HFS+, FAT/FAT32, exFAT | `F_LOG2PHYS_EXT` |

Unsupported, network, and layered filesystems fall back safely to stable input
ordering. Btrfs, APFS, and the FAT variants on macOS are explicitly marked as
approximate because filesystem offsets can be less representative of physical
placement there.

See [how it works](docs/how-it-works.md) for the algorithm and tradeoffs, and
[platform notes](docs/platform-support.md) for provider-specific details.

## Build and test

Install the .NET 10 SDK, then run:

```bash
dotnet restore
dotnet build --configuration Release --no-restore
dotnet test --configuration Release --no-build
dotnet pack src/PhysicalFileOrdering/PhysicalFileOrdering.csproj \
  --configuration Release --no-build --output artifacts
```

The test suite includes unit coverage for deterministic ordering and native
metadata parsing, plus a filesystem smoke test. GitHub Actions runs the same
build and tests on the current free hosted Ubuntu, Windows, and macOS runners.

## Scope and limitations

- Only the first usable extent is considered; fragmented files are not modeled
  in full.
- Ordering offsets from different volumes would be meaningless, so volumes are
  grouped instead of compared.
- Sparse or unallocated extents are skipped. If no allocated extent is found,
  the file uses stable fallback ordering.
- Files can move between locating and reading them. No placement result is a
  permanent guarantee.
- The library only reads filesystem metadata and file handles; it does not move
  or modify files.

## License

PhysicalFileOrdering is available under the [MIT License](LICENSE).
