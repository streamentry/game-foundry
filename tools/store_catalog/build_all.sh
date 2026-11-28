#!/bin/zsh
# Build signed iOS IPA and Android AAB for each catalog game.
# Needs: GODOT, STORE_OUT, and the puzzle upload keystore password in Keychain.
# Usage: tools/store_catalog/build_all.sh [slug ...]
set -u
ROOT=${0:A:h:h:h}
KS="$HOME/Library/Application Support/GameFoundry/signing/puzzle-catalog-upload.jks"
export GODOT_ANDROID_KEYSTORE_RELEASE_PATH="$KS"
export GODOT_ANDROID_KEYSTORE_RELEASE_USER=puzzle-upload
export GODOT_ANDROID_KEYSTORE_RELEASE_PASSWORD=$(security find-generic-password -a game-foundry -s game-foundry-puzzle-upload-keystore -w)
slugs=("$@")
if (( ${#slugs} == 0 )); then
  slugs=(${(f)"$(cd $ROOT/games && ls -d */ | tr -d / | grep -v '^evergrove$')"})
fi
for s in $slugs; do
  G=$ROOT/games/$s; O=$STORE_OUT/$s; mkdir -p $O/ios
  $GODOT --headless --path $G --editor --import --quit >$O/import.log 2>&1
  $GODOT --headless --path $G --install-android-build-template --export-release Android $O/$s.aab >$O/android.log 2>&1
  a=$?
  $GODOT --headless --path $G --export-release iOS $O/ios/$s.ipa >$O/ios.log 2>&1
  i=$?
  print -r -- "$s android=$a ios=$i aab=$([[ -f $O/$s.aab ]] && echo y || echo n) ipa=$([[ -f $O/ios/$s.ipa ]] && echo y || echo n)"
done
