#!/bin/sh

version="1.0.0"
SOURCE_PATH="/Users/$USER/Desktop/MSpace/UBlockly/Source"
TARGET_PATH="/Users/$USER/Desktop/PTProjects/Blockly/PTBlockly/Blockly/Assets/PTUGame/PTBlockly"

rm -rf "$SOURCE_PATH"/*
cp -a "$TARGET_PATH/." "$SOURCE_PATH/"

rm -rf "$SOURCE_PATH"/Script/Test/CodeTest
rm "$SOURCE_PATH"/Script/Test/CodeTest.meta

rm -rf "$SOURCE_PATH"/Script/Test/ViewTest
rm "$SOURCE_PATH"/Script/Test/ViewTest.meta

#sync data
SOURCE_DATA_PATH="/Users/$USER/Desktop/MSpace/UBlockly/UserData"
TARGET_DATA_PATH="/Users/$USER/Desktop/PTProjects/Blockly/PTBlockly/Blockly/Assets/PTGameData/PTBlockly"

rm -rf "$SOURCE_DATA_PATH"/I18n/*
cp -a "$TARGET_DATA_PATH"/I18n/. "$SOURCE_DATA_PATH"/I18n

cd "$SOURCE_PATH"
find . -type f -name "*.cs" | while read filename; do
    sed -i '' 's/PTGame\.Blockly/UBlockly/g' "$filename"
    sed -i '' 's/Blockly\.Blockly/Blockly/g' "$filename"
    sed -i '' 's/PTBlockly/UBlockly/g' "$filename"
done

git add .
if [[ $1 = "-m" ]]; then
    git commit -m $2
else
    git commit -m "$version auto sync"
fi
git push
