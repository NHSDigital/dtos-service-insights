# Guide: Set up local settings

- [Guide: Set up local settings](#guide-set-up-local-settings)
  - [Overview](#overview)
  - [Key files](#key-files)
  - [Steps](#steps)

## Overview

To run the Service Insights application on your local machine you will need a `local.settings.json` file for each function project. These file are kept out of source control as they may contain sensitive values.

Instead `local.settings.json.template` files are included that omit sensitive values. These template files are included to be used as the basis for setting up your own `local.settings.json` files.

## Key files

- [`create_local_settings.sh`](../../scripts/local-settings/create_local_settings.sh)

## Steps

1. Run the following script to automatically create the `local.settings.json` files (remember to use a terminal that can run shell scripts e.g. bash or zsh)

    ```shell
    cd scripts/local-settings
    ./create_local_settings.sh
    ```

2. Update the `local.settings.json` files to include any sensitive values that were omitted from the templates.

    e.g. The value of the 'ServiceInsightsDbConnectionString'
