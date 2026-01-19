# Plain Text Input Formatter

By default asp.net core sends text back as json string, it would be more efficient to send back plain text with the content type of `text/html`.  The plain text input formatter does just that.

It is enabled by default.  To disable it, overwrite the `PlainTextFormatter` property of the `Startup` class and set it to `false`.