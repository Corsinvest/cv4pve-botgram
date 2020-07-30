# cv4pve-botgram

[![License](https://img.shields.io/github/license/Corsinvest/cv4pve-botgram.svg)](LICENSE.md) [![AppVeyor branch](https://img.shields.io/appveyor/ci/franklupo/cv4pve-botgram/master.svg)](https://ci.appveyor.com/project/franklupo/cv4pve-botgram)

[ProxmoxVE Api](https://pve.proxmox.com/pve-docs/api-viewer/)

```text
    ______                _                      __
   / ____/___  __________(_)___ _   _____  _____/ /_
  / /   / __ \/ ___/ ___/ / __ \ | / / _ \/ ___/ __/
 / /___/ /_/ / /  (__  ) / / / / |/ /  __(__  ) /_
 \____/\____/_/  /____/_/_/ /_/|___/\___/____/\__/

Telegram bot for Proxmox VE         (Made in Italy)

Usage: cv4pve-botgram [options]

Options:
  -?|-h|--help      Show help information
  --version         Show version information
  --host            The host name host[:port],host1[:port],host2[:port]
  --api-token       Api token format 'USER@REALM!TOKENID=UUID'. Require Proxmox VE 6.2 or later
  --username        User name <username>@<realm>
  --password        The password. Specify 'file:path_file' to store password in file.
  --token           Telegram API token bot
  --chatsId         Telegram Chats Id valid for communication (comma separated)

Commands:
  app-check-update  Check update application
  app-upgrade       Upgrade application

Run 'cv4pve-botgram [command] --help' for more information about a command.

cv4pve-botgram is a part of suite cv4pve-tools.
For more information visit https://www.cv4pve-tools.com
```

## Copyright and License

Copyright: Corsinvest Srl
For licensing details please visit [LICENSE.md](LICENSE.md)

## Commercial Support

This software is part of a suite of tools called cv4pve-tools. If you want commercial support, visit the [site](https://www.cv4pve-tools.com)

## Introduction

Telegram for Proxmox VE.

this software aims to simplify the management of Proxmox VE single node or cluster using. The reasons are:

* do not use graphical interface (GUI)
* access to simplified operations
* using [Telegram](https://telegram.org/) application
* access using chat mobile
* Support multiple host for HA in --host parameter es. host[:port],host1[:port],host2[:port]
* Check-Update and Upgrade application

## Main features

* Completely written in C#
* Use native api REST Proxmox VE (library C#)
* Independent os (Windows, Linux, Macosx)
* Installation unzip file extract binary
* Not require installation in Proxmox VE
* Execute out side Proxmox VE
* Call Api Get,Set,Create,Delete
* Alias command
* Documentation generated at runtime from server
* Complex output to document in chat
* Valid ChatsId for communication
* Use Api token --api-token parameter

## Api token

From version 6.2 of Proxmox VE is possible to use [Api token](https://pve.proxmox.com/pve-docs/pveum-plain.html).
This feature permit execute Api without using user and password.
If using **Privilege Separation** when create api token remember specify in permission.

## Set up your bot in Telegram

You don’t need to write any code for this. In fact, you don’t even need your computer! Go to the telegram app on your phone and…

* Search for the “botfather” telegram bot (he’s the one that’ll assist you with creating and managing your bot)

![0](/Images/0.png)

* Type **/help** to see all possible commands the botfather can handle

![1](/Images/1.png)

* Click on or type **/newbot** to create a new bot.

![2](/Images/2.png)

Follow instructions and make a new name for your bot. If you are making a bot just for experimentation, it can be useful to namespace your bot by placing your name before it in its username, since it has to be a unique name. Although, its screen name can be whatever you like.
I have chosen “Frank Test PVE Bot” as the screen name and “frank_test_pve_bot” as its username.

* Congratulations! You have created your first bot. You should see a new API token generated for it (for example, in the previous picture, you can see my newly generated token is **707587383:AAHD9DVwXrBxz8wyOl0DWLdbzl8skkffhRo**). Now you can search for your newly created bot on telegram :

## Configuration

E.g. install on linux 64

Download last package e.g. Debian cv4pve-botgram-linux-x64.zip, on your os and install:

```sh
root@debian:~# unzip cv4pve-botgram-linux-x64.zip
```

This tool need basically no configuration.

```sh
root@debian:~# cv4pve-botgram --host=192.168.0.100 --username=root@pam --password=fagiano --token 707587383:AAHD9DVwXrBxz8wyOl0DWLdbzl8skkffhRo
```

## Use

In telegram open chat and execute **/help** command.
[Web Chat Telegram](https://web.telegram.org/#/)
