#!/bin/bash
echo "chris: - "

declare -A docker_functions_map=()

# Parse the compose.yaml file and populate the docker_functions_map
services=$(jq -r '.services | keys[]' "compose.yaml")  # Extract service names
for service in $services; do
    tasks=$(jq -r --arg service "$service" '.services[$service].tasks[].name' compose.yaml)  # Extract task names for each service
    for task in $tasks; do
        docker_functions_map["$service/$task"]="run-$service-$task"
    done
done

exit


echo "alastair:-"

grep container_name compose.yaml | cut -d: -f2 | sed 's/^ //g' | grep -Ev 'azurite|sql-database|database-setup' | while read -r service; do
  dockerfile=$(grep -A4 "${service}" compose.yaml | grep dockerfile | cut -d: -f2 | sed 's/^ .\///g')
  echo ["${dockerfile}"]="${service}"
done


# for service in $(grep container_name  compose.yaml | cut -d: -f2 | sed 's/^ //g' | grep -v azurite | grep -v sql-database | grep -v database-setup);
# do
#   dockerfile=$(grep -a4 ${service}  compose.yaml | grep dockerfile | cut -d: -f2 | sed 's/^ .\///g')
#   echo ["${dockerfile}"]="${service}"
# done

exit



declare -A docker_functions_map=()
# Parse the compose.yaml file and populate the docker_functions_map
for service in $(yq eval '.services[].name' "../../compose-mac.yaml"); do
    for task in $(yq eval ".services[?has($service)]|.services[?has($service)].tasks[].name" compose.yaml); do
        docker_functions_map["$service/$task"]="run-$service-$task"
    done
done
#changed_functions=""

