# Upgrade guide 1.x to 2.x

## Breaking changes
##### Bio.FileReplaceInvalidChars

...has been renamed to `PathReplaceInvalidChars`.

A third parameter `isDirectory` is now required, which decides whether or not to replace characters that are only invalid in file names, but allowed in paths (such as `:` or `?`).

See `GetSafeOutputPath` for another way to ensure valid paths.


##### Bio.CopyStream

Removed. Use the extension method instead:

```C#
using BioLib.Streams;
myStream.Copy(otherStream);
```

The parameter `keepPosition` has been removed to keep function complexity low. Use a closure instead:

``` c#
myStream.KeepPosition(() => myStream.Copy(otherStream));
```


##### Bio.Seperator

Renamed. Use the correctly spelled Bio.Separator instead.


##### Public constants

Removed. Prompt-related constants have been moved to `PromptInput` class.