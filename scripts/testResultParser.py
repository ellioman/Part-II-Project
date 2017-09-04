#!/usr/bin/env python
# -*- coding: utf-8 -*-

import codecs
import sys
import textwrap

import xml.etree.ElementTree as ET

if sys.platform == "win32" and sys.stdout.encoding != 'cp65001':
    sys.stdout = codecs.getwriter('cp65001')(sys.stdout.buffer, 'strict')

class bcolors:
    """
    Script used by blender to define terminal output colours

    See: https://svn.blender.org/svnroot/bf-blender/trunk/blender/build_files/scons/tools/bcolors.py
    """
    HEADER = '\033[95m'
    OKBLUE = '\033[94m'
    OKGREEN = '\033[92m'
    WARNING = '\033[93m'
    FAIL = '\033[91m'
    ENDC = '\033[0m'
    BOLD = '\033[1m'
    UNDERLINE = '\033[4m'

if __name__ == '__main__':
    """
    This script is used to parse the unit testing output produced by Unity when
    run in batchmode, and returning an error-code if any of the unit tests
    failed.
    """

    # Retrieve where the test file to be read is located
    test_result_location = sys.argv[1]
    print("Analyzing tests at: " + test_result_location + "\n")
    tree = ET.parse(test_result_location)

    root = tree.getroot()

    test_metadata = root.attrib
    unit_tests_name = test_metadata["name"]

    total_tests = int(test_metadata["total"])
    errored_tests = int(test_metadata["errors"])
    failed_tests = int(test_metadata["failures"])
    inconclusive_tests = int(test_metadata["inconclusive"])
    not_run_tests = int(test_metadata["not-run"])
    ignored_tests = int(test_metadata["ignored"])
    invalid_tests = int(test_metadata["invalid"])
    successful_tests = total_tests - (errored_tests + failed_tests + not_run_tests + inconclusive_tests + ignored_tests + invalid_tests)

    #Complete summary printed at end for ease-of-reading
    complete_summary = "{} Tests: {}, Successful: {}, Errors: {}, Failures: {}, Not Run: {}, Inconclusive: {}, Ignored: {}, Invalid: {}".format(
        unit_tests_name, total_tests, successful_tests, errored_tests, failed_tests, not_run_tests, inconclusive_tests, ignored_tests, invalid_tests
    )

    tests = root[2][0];

    for test in tests:
        test_values = test.attrib
        test_name = test_values["name"]
        test_executed =  True if test_values["executed"] == "True" else False
        test_result = test_values["result"]
        test_success = True if test_values["success"] == "True" else False
        test_summary = ""
        if(test_success):
            test_summary = bcolors.OKGREEN + "✓ " + bcolors.ENDC + test_name + " has succeeded"
            print(test_summary)
        else:
            test_summary = bcolors.FAIL + "✘ " + bcolors.ENDC + test_name + " has failed:"
            print(test_summary)
            print("\tMessage:" + textwrap.indent(test[0][0].text, "\t"))
            print("\tStack Trace:" + textwrap.indent(test[0][1].text, "\t  "))

    print(complete_summary)

    #Not all unit tests succeeded, exit with error
    if(successful_tests != total_tests):
        sys.exit(1)
    else:
        sys.exit(0)
