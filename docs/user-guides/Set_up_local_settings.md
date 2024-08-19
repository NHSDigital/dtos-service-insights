# Guide: Set up local settings

- [Guide: Set up local settings](#guide-set-up-local-settings)
  - [Overview](#overview)
  - [Key files](#key-files)
  - [Steps](#steps)

## Overview

To run the Service Insights application on your local machine you will need a `local.settings.json` file in each function project folder.

These file are kept out of source control as they may contain sensitive values.

## Key files

- [`create_local_settings.sh`](../../scripts/local-settings/create_local_settings.sh)

## Steps

To create the `local.settings.json` files based off of the `local.settings.json.template` files, run the following commands in your terminal:

```shell
cd scripts/local-settings
./create_local_settings.sh
```
