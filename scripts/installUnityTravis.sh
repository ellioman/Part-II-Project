#! /bin/sh

# This program installs the version of Unity used to run this project in Travis CI

UNITY_VERSION='5.5.1f1'
# Use http://unity3d.com/get-unity/download/archive for up-to-date Unity links
DOWNLOAD_LOCATION='http://netstorage.unity3d.com/unity/88d00a7498cd/MacEditorInstaller/Unity-'"$UNITY_VERSION"'.pkg'
FILE_NAME='unity.pkg'

echo "Downloading Unity from $DOWNLOAD_LOCATION to $FILE_NAME"
curl -o Unity.pkg $DOWNLOAD_LOCATION

echo "Installing Unity from $FILE_NAME"
sudo installer -dumplog -package $FILE_NAME -target /
