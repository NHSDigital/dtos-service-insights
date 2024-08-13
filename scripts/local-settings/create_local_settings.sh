#!/bin/bash

# Root directory relative to the script location (up one level, then into 'src')
ROOT_DIR="$(dirname "$0")/../../src"

# Find all files named 'local.settings.json.template' and copy them
find "$ROOT_DIR" -type f -name "local.settings.json.template" | while read -r template_file; do
  # Determine the directory of the template file
  file_dir=$(dirname "$template_file")

  # Define the target file path
  target_file="$file_dir/local.settings.json"

  # Copy the template file to the new location
  cp "$template_file" "$target_file"

  echo "Copied $template_file to $target_file"
done
