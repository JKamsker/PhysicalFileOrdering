# How physical ordering works

The library answers a deliberately narrow question: “what is the first usable
storage position the operating system reports for this file?” It does not try
to reconstruct platter geometry, which modern drives do not expose reliably.

## Ordering algorithm

For every input path, `PhysicalFileOrderer`:

1. Converts the path to an absolute path.
2. Resolves a provider for the path's host filesystem.
3. Asks that provider for a volume identity and first usable extent position.
4. Groups entries by volume in first-seen order.
5. Within each volume, sorts known positions ascending.
6. Places unknown positions afterward without changing their relative order.

Provider failures are isolated to the affected path. That path receives the
fallback volume identity and an unknown position, allowing the remaining input
to be ordered normally.

## Why the first extent?

Querying one extent is inexpensive and provides a useful starting-location
heuristic for mostly contiguous files. Modeling every extent would require a
read schedule rather than a file order, and would make the API and runtime cost
substantially more complex.

## Volume identity

Positions are comparable only within the same volume. Windows uses the stable
volume name when available, Linux uses the mount's major/minor device ID, and
macOS uses its mount point. The fallback provider uses the most specific mount
root reported by `DriveInfo`.

## Error behavior

Invalid input collections and invalid path syntax are reported to the caller.
Errors encountered while resolving filesystem metadata are treated as an
unknown placement for that individual path. Sorting is deterministic for the
same placements and input order.
