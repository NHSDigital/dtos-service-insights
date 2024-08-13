# Guide: Set up local settings

- [Guide: Set up local settings](#guide-set-up-local-settings)
  - [Overview](#overview)
  - [Prerequisites](#prerequisites)
  - [Key files](#key-files)
  - [Steps](#steps)
  - [Troubleshooting](#troubleshooting)

## Overview

To run the Service Insights application on your local machine you will need a `local.settings.json` file in each function project folder.

These file are kept out of source control as they may contain sensitive values.

To create your `local.settings.json` files based off of the `local.settings.json.template` files, run the following commands in your terminal:

1. `cd scripts/local-settings`
2. `./create_local_settings.sh`
