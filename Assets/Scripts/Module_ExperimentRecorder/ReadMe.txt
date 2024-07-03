Attach DepthRecordings.cs to Camera. The depth map of this specific camera will be recorded.

Downscaling factor

nearClip and farClip are used to define the depth texture range: 
- pixel value 0 correpsonds to nearClip
- pixel value 1 correpsonds to farClip
- pixel value scales linearly with distance between nearClip and farClip

TODO:
arbitrary resolution of recordings
save additionally RGB?
reduce depth resolution (bit depth)