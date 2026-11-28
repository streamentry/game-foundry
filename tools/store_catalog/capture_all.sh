#!/bin/zsh
# Capture App Store (1242x2688) and Google Play (1080x1920) screenshots per game.
# Usage: tools/store_catalog/capture_all.sh [slug ...]   (needs GODOT, STORE_OUT)
set -u
ROOT=${0:A:h:h:h}; CAP=${0:A:h}/capture.gd
slugs=("$@")
(( ${#slugs} )) || slugs=(${(f)"$(cd $ROOT/games && ls -d */ | tr -d / | grep -v '^evergrove$')"})
for s in $slugs; do
  G=$ROOT/games/$s; O=$STORE_OUT/$s
  $GODOT --path $G --windowed --resolution 200x200 --script $CAP -- $O/shots/apple 1242 2688 414 896 >$O/capture.log 2>&1
  $GODOT --path $G --windowed --resolution 200x200 --script $CAP -- $O/shots/play 1080 1920 450 800 >>$O/capture.log 2>&1
  print -r -- "$s apple=$(ls $O/shots/apple 2>/dev/null | wc -l | tr -d ' ') play=$(ls $O/shots/play 2>/dev/null | wc -l | tr -d ' ')"
done
