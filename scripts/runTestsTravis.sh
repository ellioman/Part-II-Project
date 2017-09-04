#! /bin/bash
ROOT_PROJECT_PATH="$TRAVIS_BUILD_DIR"

UNITY_LOCATION='/Applications/Unity/Unity.app/Contents/MacOS/Unity'
PROJECT_PATH=$ROOT_PROJECT_PATH'/project/Wave Particles'
RESULT_LOCATION=$ROOT_PROJECT_PATH

echo "Calling Unit Test script..."
echo 'source ./scripts/runTests.sh' "$UNITY_LOCATION" "$PROJECT_PATH" "$RESULT_LOCATION"
source ./scripts/runTests.sh "$UNITY_LOCATION" "$PROJECT_PATH" "$RESULT_LOCATION" python3
