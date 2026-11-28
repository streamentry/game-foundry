#!/bin/zsh
# Validate and upload each catalog IPA to App Store Connect with API-key auth.
# Needs ASC_KEY_ID, ASC_ISSUER_ID, STORE_OUT. Skips games already marked uploaded.
# Usage: tools/store_catalog/upload_ios.sh [slug ...]
set -u
ROOT=${0:A:h:h:h}
slugs=("$@")
(( ${#slugs} )) || slugs=(${(f)"$(cd $ROOT/games && ls -d */ | tr -d / | grep -v '^evergrove$')"})
for s in $slugs; do
  IPA=$STORE_OUT/$s/ios/$s.ipa
  [[ -f $IPA ]] || { print "$s no-ipa"; continue }
  [[ -f $STORE_OUT/$s/ios.uploaded.${BUILD:-1} ]] && { print "$s already"; continue }
  xcrun altool --upload-app -f $IPA -t ios --apiKey $ASC_KEY_ID --apiIssuer $ASC_ISSUER_ID >$STORE_OUT/$s/altool.log 2>&1
  r=$?
  (( r == 0 )) && shasum -a 256 $IPA > $STORE_OUT/$s/ios.uploaded.${BUILD:-1}
  print "$s upload=$r $(grep -o 'UPLOAD SUCCEEDED\|UPLOAD FAILED.*' $STORE_OUT/$s/altool.log | head -1)"
done
