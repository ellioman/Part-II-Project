
#! /bin/sh

# This script runs all the Project Unit tests, outputting them to a result file
# and displaying those unit tests using a python3 parser
# The arguments are:
#   1. The location of Unity in the file system
#   2. The path to the project you want to Unit Test
#   3. Where you want to output the results
#   4. The path to your locally installed version of python 3

DATE=`date +%Y-%m-%d\ %H.%M.%S`

echo "Running Unity Unit Test on $DATE"

UNITY_LOCATION="$1"
PROJECT_PATH="$2"
RESULT_LOCATION="$3"
PYTHON_PATH="$4"
PYTHON_SCRIPT_LOCATION=$PROJECT_PATH"/../../scripts/testResultParser.py"

echo "UNITY_LOCATION=$UNITY_LOCATION"
echo "PROJECT_PATH=$PROJECT_PATH"
echo "RESULT_LOCATION=$RESULT_LOCATION"


RESULT_FILE="$RESULT_LOCATION/$DATE.xml"

"$UNITY_LOCATION" \
  -batchmode \
  -nographics \
  -runEditorTests \
  -projectPath "$PROJECT_PATH" \
  -logFile "$RESULT_LOCATION/unity.log" \
  -editorTestsResultFile "$RESULT_FILE"


  if [ -f "$RESULT_FILE" ]; then
    echo 'Testing Complete, output:'
    # Don't run anything after this, as we want to keep exit code!
    # (Or store exit code somewhere useful!)
    "$PYTHON_PATH" "$PYTHON_SCRIPT_LOCATION" "$RESULT_FILE"
  else
    echo 'The test output has not appeared, Unity log:'
    echo "$(cat "$RESULT_LOCATION/unity.log")"
    exit 1
  fi
