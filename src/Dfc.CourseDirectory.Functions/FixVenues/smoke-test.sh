#!/bin/bash
set -e # exit on error

# This script is just for developer convenience to be run locally to hit the endpoints
# after code changes to make sure all the DI etc is wired up correctly.

echo "======================================================"
echo "FindNextCorruption..."
curl -i http://localhost:7071/api/FindNextCorruption -d '{}' -H "Content-Type:application/json"
echo # curl -i doesn't add a newline, add one now
echo

echo "======================================================"
echo "FindNextFixableCorruption..."
id=`curl http://localhost:7071/api/FindNextFixableCorruption -d '{}' -H "Content-Type:application/json"`
echo
echo "next id: $id"
echo

idArray="[""$id""]" 
echo
echo "======================================================"
echo "AnalyseSpecificApprenticeshipVenueReferences[""$id""]..."
curl -v http://localhost:7071/api/AnalyseSpecificApprenticeshipVenueReferences -d "$idArray" -H "Content-Type:application/json" | jq
echo
echo

echo "======================================================"
echo "FixSpecificApprenticeshipVenueReferences[""$id""]..."
curl -i http://localhost:7071/api/FixSpecificApprenticeshipVenueReferences -d "$idArray" -H "Content-Type:application/json"
echo
echo

echo "======================================================"
echo "AnalyseAllApprenticeshipVenueReferences..."
curl -i -v http://localhost:7071/api/AnalyseAllApprenticeshipVenueReferences -d '{}' -H "Content-Type:application/json"
echo
echo

# you only get one shot at this then you have to reset the whole collection, so left here commented out for reference:
# you can leave it to run for a bit then kill the function host if you want to partially test this endpoint
#echo "======================================================"
#echo "FixAllApprenticeshipVenueReferences..."
#curl -i -v http://localhost:7071/api/FixAllApprenticeshipVenueReferences -d '{}' -H "Content-Type:application/json"
#echo
#echo
echo "Skipping fix-all test. Test manually with:"
echo $'curl -i -v http://localhost:7071/api/FixAllApprenticeshipVenueReferences -d \'{}\' -H "Content-Type:application/json"'

echo "Done"
