# Platform support

All platform implementations compile into one managed assembly. The default
factory selects only the resolver for the current operating system.

## Windows

Windows providers open the file and call `FSCTL_GET_RETRIEVAL_POINTERS`. Sparse
or unallocated ranges are skipped until the first non-negative logical cluster
number is found. The volume GUID is used to prevent positions from unrelated
volumes being compared.

Supported automatically: NTFS, ReFS, FAT/FAT32, exFAT, and UDF.

## Linux

Linux providers call `FS_IOC_FIEMAP` and scan returned extents until they find
one that is allocated and usable for ordering. Mount metadata comes from
`/proc/self/mountinfo`; `DriveInfo` is used only if mountinfo is unavailable.

Supported automatically: ext2, ext3, ext4, XFS, Btrfs, and F2FS. The public
`GenericFiemapFilePlacementProvider` can be selected explicitly for another
filesystem known to implement FIEMAP. It is not selected automatically because
network and layered filesystems may return misleading positions.

Btrfs results are marked approximate, particularly for multi-device profiles.

## macOS

macOS providers use `F_LOG2PHYS_EXT` to translate the first logical file offset
to a device offset. Drive information identifies the filesystem and mount.

Supported automatically: APFS, HFS+, FAT/FAT32, and exFAT. The public
`GenericLog2PhysFilePlacementProvider` is available for explicit use with other
filesystems that support the same operation.

APFS and FAT-family results are marked approximate. Copy-on-write clones,
compression, sparse files, and fragmentation can reduce how well the first
reported offset predicts the best complete-file read order.
