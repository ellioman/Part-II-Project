#! /bin/sh

# This script runs all the Project Unit tests, outputting them to a result file

while read LINE
do
  if [ ! $FIRST_LINE ]; then
    ROOT_PATH=${LINE%?}
    FIRST_LINE="DONE"
  else
    UNITY_LOCATION=${LINE%?}
  fi
done <".testsettings"

echo "Your '.testsettings' file tells us that:"
echo "Your project root is at: $ROOT_PATH"
echo "Your Unity installation is at: $UNITY_LOCATION"

PROJECT_PATH="$ROOT_PATH"'/project/Wave Particles'
RESULT_LOCATION="$ROOT_PATH"'/testOutput'

source ./scripts/runTests.sh "$UNITY_LOCATION" "$PROJECT_PATH" "$RESULT_LOCATION" python
