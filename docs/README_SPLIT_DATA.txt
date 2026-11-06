GitHub Pages split Unity data build

Original data file:
  Build/docs.data.unityweb
  Size: 155,291,533 bytes
  SHA-256: 70bf65f849797207abf011c4afddd4fe87a78bbfb4a28b5f33b9c5bbc317efb5

The original >100 MiB file has been removed and split into:
  Build/docs.data.part00.unityweb  20,971,520 bytes
  Build/docs.data.part01.unityweb  20,971,520 bytes
  Build/docs.data.part02.unityweb  20,971,520 bytes
  Build/docs.data.part03.unityweb  20,971,520 bytes
  Build/docs.data.part04.unityweb  20,971,520 bytes
  Build/docs.data.part05.unityweb  20,971,520 bytes
  Build/docs.data.part06.unityweb  20,971,520 bytes
  Build/docs.data.part07.unityweb  8,490,893 bytes

Part count: 8
Largest part: 20,971,520 bytes
All parts are below 25 MiB, so they are also suitable for GitHub's browser file upload limit.

How it works:
- index.html now sets config.dataUrl to the virtual URL Build/docs.data.split.
- Before Unity loader starts, window.fetch is wrapped.
- Requests for docs.data.split are answered with a ReadableStream.
- The stream downloads docs.data.part00..., part01..., etc. in order.
- The resulting byte stream is byte-for-byte identical to the original
  docs.data.unityweb file.
- Unity's generated loader then handles the normal .unityweb decompression.

Verification:
  Original SHA-256:  70bf65f849797207abf011c4afddd4fe87a78bbfb4a28b5f33b9c5bbc317efb5
  Rejoined SHA-256:  70bf65f849797207abf011c4afddd4fe87a78bbfb4a28b5f33b9c5bbc317efb5
  Match: YES

Upload the entire docs folder to the GitHub Pages source.
Do NOT upload the old Build/docs.data.unityweb file.

Important:
If the old >100 MiB file was already COMMITTED locally (even if push failed),
remove it from that commit/history before pushing; deleting it only from the
working folder may not be enough.
