#!/usr/bin/env bash


docker-compose build  --no-cache --pull retrieve-mesh-file

# 1) Create an array of image names
list=(
#"acrukshubdevserins.azurecr.io/service-insights-create-ps-episode-data"
# "service-insights-get-organisation-data"
# "service-insights-update-participant"
# "service-insights-get-participant"
"service-insights-retrieve-mesh-file"
# "service-insights-get-episode-ref-data"
# "service-insights-get-episode-mgmt"
# "service-insights-create-update-episode"
# "service-insights-receive-data"
# "service-insights-update-episode"
# "service-insights-retrieve-episode-ref-data"
# "service-insights-get-episode"
# "service-insights-create-episode"
# "service-insights-get-demographics-data"
# "service-insights-get-ps-episode"
# "service-insights-get-ps-profile"
# "service-insights-create-ps-profile"
# "service-insights-create-ps-episode"
# "service-insights-get-ps-episode-data"
# "service-insights-get-ps-profile-data"
# "service-insights-create-ps-profile-data"
# "service-insights-create-ps-episode-data"
# "service-insights-azurite-setup"
# "service-insights-database-setup"
# "service-insights-base-image"
)

# 2) Loop over the list and print each name
for file in "${list[@]}"
do
  echo "$file"
  export CHECK_DOCKER_IMAGE="$file"
  bash /Users/alastairlock/git/dtos-devops-templates/scripts/reports/create-sbom-report.sh
  bash /Users/alastairlock/git/dtos-devops-templates/scripts/reports/scan-vulnerabilities.sh
done
