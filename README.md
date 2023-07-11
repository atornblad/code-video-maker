# Code Video Maker

Get a full git log from an adjacent repository:

```bash
git log --reverse -p -U99999 > ../code-video-maker/git-log-p-output.txt
```

Render a video of a git repo:

```bash
dotnet run -- -w 512 -f 16 -r ./git-log-p-output.txt -o   lowres.mp4
dotnet run -- -w 1920 -f 60 -r ./git-log-p-output.txt -o  highres.mp4
dotnet run -- -w 3840 -f 60 -r ./git-log-p-output.txt -o  highres-4k.mp4
```


Render an opening sequence

```bash
dotnet run -- -w 512 -f 16 --opening 'Title' 'Subtitle' -o opening-lowres.mp4
dotnet run -- -w 1920 -f 60 --opening 'Title' 'Subtitle' -o opening-highres.mp4
```


Render the full video

```bash
dotnet run -- -w 512 -f 16 --opening 'js-mod-player' 'Building a JavaScript MOD player' -r ./git-log-p-output.txt -o js-mod-player-1-lowres.mp4
dotnet run -- -w 1920 -f 60 --opening 'js-mod-player' 'Building a JavaScript MOD player' -r ./git-log-p-output.txt -o js-mod-player-1-highres.mp4
```

Render a background segment of 60 seconds

```bash
dotnet run -- -w 512 -f 16 --background 60 -o background-lowres.mp4
dotnet run -- -w 1920 -f 60 --background 60 -o background-highres.mp4
```

# TO DO


