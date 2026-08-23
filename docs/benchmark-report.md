# Anonymized benchmark report

This report measures whether reading complete files in the order returned by
`PhysicalFileOrdering` is faster than reading them in a random order. Identifying
paths, user names, file names, directory names, and volume names have been
omitted.

The benchmark was run in August 2026. It is a result from one system and one
dataset, not a general performance guarantee.

## Result

Physical ordering delivered **1.0777x the throughput** of random ordering and
reduced normalized read time by **7.21%**.

| Order | Runs | Median throughput | Normalized time per 16 GiB |
| --- | ---: | ---: | ---: |
| Random | 3 | 242.55 MiB/s | 67.549 s |
| Physical | 3 | 261.39 MiB/s | 62.681 s |

The median saving was 4.868 seconds per 16 GiB. All three physical-order runs
were faster than all three random-order runs.

## Environment

- macOS on ARM64
- Large external SATA rotating-disk array
- Journaled HFS+ filesystem
- .NET 8 runtime
- Media-file workload with 34,523 non-empty files totaling 237.74 GiB

The operating system exposed the array as one logical volume. Consequently,
reported HFS+ device offsets describe that volume and might not correspond
directly to individual platter positions behind the storage controller.

## Method

1. Enumerate the source tree and record each non-empty file's size.
2. Exclude every file read during preliminary benchmark runs.
3. Deterministically shuffle the remaining files and partition them into six
   disjoint groups, stopping each group after it reaches at least 16 GiB.
4. Assign the groups to the schedule random, physical, physical, random,
   random, physical. This distributes the two treatments through the run while
   keeping every measured pass independent of filesystem data-cache reuse.
5. For a random pass, reshuffle that group's paths with a separate seed. For a
   physical pass, order its paths with `PhysicalFileOrderers.CreateDefault()`.
6. Open every file with `FileAccess.Read`, a 1 MiB buffer, and sequential-scan
   behavior. Read each file completely before opening the next one.
7. Set macOS `F_NOCACHE` on every file descriptor as an additional cache
   safeguard. Verify that the number of bytes read equals the sum of the input
   file sizes.

No file appeared in more than one measured pass. The six passes read 14,123
unique files and 103,100,894,920 bytes, or approximately 96.02 GiB. The source
files were opened read-only and were not modified.

## Raw results

| Trial | Order | Files | Data | Time | Throughput |
| ---: | --- | ---: | ---: | ---: | ---: |
| 1 | Random | 2,315 | 16.000 GiB | 67.550 s | 242.55 MiB/s |
| 2 | Physical | 2,334 | 16.007 GiB | 62.745 s | 261.24 MiB/s |
| 3 | Physical | 2,393 | 16.003 GiB | 62.218 s | 263.38 MiB/s |
| 4 | Random | 2,379 | 16.001 GiB | 68.468 s | 239.30 MiB/s |
| 5 | Random | 2,320 | 16.004 GiB | 67.082 s | 244.30 MiB/s |
| 6 | Physical | 2,382 | 16.005 GiB | 62.702 s | 261.39 MiB/s |

Times in the summary are normalized by measured throughput because each group
slightly exceeded 16 GiB by the size of its final file.

## Placement and ordering checks

All 7,109 files used by the physical-order trials returned known HFS+ positions.
Each resulting order was monotonic by reported position, with zero violations.

Sorting and an additional placement-verification pass together took 42-51 ms
per physical trial and were measured separately from read time. In a separate
metadata-only check, sorting the complete 34,523-file dataset took 0.436
seconds, and every file returned a known position.

## Limitations

- Different files were deliberately used in every pass to prevent data-cache
  reuse. Randomized grouping and three runs per treatment reduce, but cannot
  eliminate, differences in file size, fragmentation, and disk location
  between groups.
- `F_NOCACHE` controls the macOS filesystem cache but cannot disable storage
  controller or drive caches. Disjoint datasets prevent direct reuse of file
  contents across measured passes.
- The array's mapping from logical volume offsets to its member disks is not
  known. This library orders the positions reported by the filesystem.
- The benchmark covers one filesystem, device, host, and mostly sequential
  whole-file workload. SSDs, other HDDs, other filesystems, fragmented files,
  concurrent I/O, and different RAID layouts can produce different results.
- Three runs per treatment show consistency here but are not enough for a
  broad statistical performance claim.

On this particular workload, the improvement was repeatable and substantially
larger than the ordering overhead. Applications should benchmark their own
storage and access pattern before relying on a similar gain.
