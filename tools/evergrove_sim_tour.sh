#!/bin/zsh
# Dev QA tour on any iOS simulator, in either orientation and any text size (I121).
# usage: tools/evergrove_sim_tour.sh <udid> <app> <tag> <orientation:portrait|landscape> [textscale]
UDID=$1; APP=$2; TAG=$3; ORI=$4; SCALE=${5:-100}
BID=$(/usr/libexec/PlistBuddy -c 'Print CFBundleIdentifier' "$APP/Info.plist")
O=${EVERGROVE_TOUR_ROOT:-/Volumes/SSD/mosselora-builds/painted}
OUT=$O/tour-$TAG; LOG=$O/tour-$TAG.stdout
xcrun simctl terminate $UDID $BID 2>/dev/null
xcrun simctl uninstall $UDID $BID 2>/dev/null
xcrun simctl install $UDID "$APP"
C=$(xcrun simctl get_app_container $UDID $BID data)
mkdir -p "$C/Documents/tour"; rm -f $LOG
SIMCTL_CHILD_mosseloraQaTour="$C/Documents/tour" SIMCTL_CHILD_mosseloraOrientation=$ORI SIMCTL_CHILD_mosseloraTextScale=$SCALE \
  xcrun simctl launch --stdout=$LOG --stderr=$O/tour-$TAG.stderr $UDID $BID >/dev/null
for i in {1..120}; do grep -q "MOSSELORA_QA_TOUR_DONE\|Exception\|No visible element" $LOG 2>/dev/null && break; sleep 2; done
sleep 3
rm -rf $OUT; cp -R "$C/Documents/tour" $OUT
echo "$(ls $OUT | wc -l | tr -d ' ') shots"; grep -o "MOSSELORA_QA_TOUR_DONE\|No visible element[^\"]*\|Exception[^\"]*" $LOG | head -2
