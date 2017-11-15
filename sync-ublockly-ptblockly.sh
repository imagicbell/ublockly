#!/bin/sh

SOURCE_PATH="/Users/$USER/Desktop/MSpace/UBlockly/Source"
TARGET_PATH="/Users/$USER/Desktop/PTProjects/Blockly/PTBlockly/Blockly/Assets/PTUGame/PTBlockly"

mv "$TARGET_PATH"/Script/Test "$TARGET_PATH"/../

rm -rf "$TARGET_PATH"/*
cp -a "$SOURCE_PATH/." "$TARGET_PATH/"

rm -rf "$TARGET_PATH"/Script/Test
mv "$TARGET_PATH"/../Test "$TARGET_PATH"/Script

cd "$TARGET_PATH"
find . -type f -name "*.cs" | while read filename; do
    sed -i '' 's/\"UBlockly/\"PTBlockly/g' "$filename"
    sed -i '' 's/UBlockly/PTGame\.Blockly/g' "$filename"
done

git add .
if [[ $1 = "-m" ]]; then
    git commit -m $2
    git push
fi




