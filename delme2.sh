# declare -A docker_functions_map=()
# Parse the compose.yaml file and populate the docker_functions_map
# for service in $(yq eval '.services[].name' "compose.yaml"); do
#     for task in $(yq eval ".services[?has($service)]|.services[?has($service)].tasks[].name" compose.yaml); do
#         # docker_functions_map["$service/$task"]="run-$service-$task"
#         echo ["$service/$task"]="run-$service-$task"
#     done
# done

# for service in $(yq eval '.services[] | select(.container_name != "azurite" and .container_name != "azurite-setup" and .container_name != "sql-database" and .container_name != "database-setup") | .container_name' compose.yaml); do
#   dockerfile=$(yq eval --arg service "$service" '.services[] | select(.container_name == $service) | .build.dockerfile' compose.yaml)
#   echo "Service: $service, Dockerfile: $dockerfile"
# done

# for service in $(yq eval '.services[] | select(.container_name != "azurite" and .container_name != "azurite-setup" and .container_name != "sql-database" and .container_name != "database-setup") | .container_name' compose.yaml); do
#   dockerfile=$(yq eval ".services[] | select(.container_name == \"$service\") | .build.dockerfile" compose.yaml)
#   echo "Service: $service, Dockerfile: $dockerfile"get-docker-
# done

declare -A docker_functions_map=()


for service in $(yq eval '.services[] | select(.container_name != "azurite" and .container_name != "azurite-setup" and .container_name != "sql-database" and .container_name != "database-setup") | .container_name' compose.yaml); do
  dockerfile=$(yq eval ".services[] | select(.container_name == \"$service\") | .build.dockerfile" compose.yaml | sed 's#.\/##')
  servicename=$(yq eval ".services[] | select(.container_name == \"$service\") | .container_name" compose.yaml)
  echo ["\"${dockerfile}\""]="\"${servicename}\""
  docker_functions_map[${dockerfile}]="${servicename}"
done



# for service in $(yq eval '.services[] | select(.container_name != "azurite" and .container_name != "azurite-setup" and .container_name != "sql-database" and .container_name != "database-setup") | .container_name' compose.yaml); do
#   echo $service
#   # for task in $(yq eval ".services[?has($service)]|.services[?has($service)].tasks[].name" compose.yaml); do
#         # docker_functions_map["$service/$task"]="run-$service-$task"
#         # echo ["$service/$task"]="run-$service-$task"
#     # done
# done
