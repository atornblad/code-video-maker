# Code Video Maker

Get a full git log from an adjacent repository:

```bash
git log --reverse -p -U99999 > ../code-video-maker/git-log-p-output.txt
```

Render a quick video:

```bash
dotnet run -- -w 512 -f 16 -o ld.mp4
```

Render a high quality video:

```bash
dotnet run -- -w 1920 -f 30 -o hd.mp4
```

Render a 4K video:

```bash
dotnet run -- -w 3840 -f 60 -o hd4k.mp4
```

# TO DO

Utilize the fact that -U99999 is recommended to automatically build initial lines for videos that don't start with empty files!
