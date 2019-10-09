# cv4pve-botgram

[![License](https://img.shields.io/github/license/Corsinvest/cv4pve-botgram.svg)](https://www.gnu.org/licenses/gpl-3.0.en.html)
[![AppVeyor branch](https://img.shields.io/appveyor/ci/franklupo/cv4pve-botgram/master.svg)](https://ci.appveyor.com/project/franklupo/cv4pve-botgram) [![Donate to this project using Palpal](https://img.shields.io/badge/paypal-donate-yellow.svg)](https://www.paypal.com/cgi-bin/webscr?cmd=_donations&business=PPM9JHLQLRV2S&item_name=Open+Source+Project&currency_code=EUR&source=url)

ProxmoxVE Client API .Net

[ProxmoxVE Api](https://pve.proxmox.com/pve-docs/api-viewer/)

# **Donations**

If you like my work and want to support it, then please consider to deposit a donation through **Paypal** by clicking on the next button:

[![paypal](https://www.paypalobjects.com/en_US/IT/i/btn/btn_donateCC_LG.gif)](https://www.paypal.com/cgi-bin/webscr?cmd=_donations&business=PPM9JHLQLRV2S&item_name=Open+Source+Project&currency_code=EUR&source=url)

```text
    ______                _                      __
   / ____/___  __________(_)___ _   _____  _____/ /_
  / /   / __ \/ ___/ ___/ / __ \ | / / _ \/ ___/ __/
 / /___/ /_/ / /  (__  ) / / / / |/ /  __(__  ) /_
 \____/\____/_/  /____/_/_/ /_/|___/\___/____/\__/

Telegram bot for Proxmox VE         (Made in Italy)

Usage: cv4pve-botgram [options]

Options:
  -?|-h|--help  Show help information
  --host        The host name host[:port]
  --username    User name <username>@<realm>
  --password    The password. Specify 'file:path_file' to store password in file.
  --token       Telegram API token bot

Report bugs to support@corsinvest.it
```

## Introduction

Telegram for Proxmox VE.

this software aims to simplify the management of Proxmox VE single node or cluster using. The reasons are:

* do not use graphical interface (GUI)
* access to simplified operations
* using [Telegram](https://telegram.org/) application
* access using chat mobile

## Main features

* Completely written in C#
* Use native api REST Proxmox VE (library C#)
* Independent os (Windows, Linux, Macosx)
* Installation unzip file extract binary
* Not require installation in Proxmox VE
* Execute out side Proxmox VE
* Alias command
* Documentation generated at runtime from server
* Complex output to document in chat

## Set up your bot in Telegram

You don’t need to write any code for this. In fact, you don’t even need your computer! Go to the telegram app on your phone and…

* Search for the “botfather” telegram bot (he’s the one that’ll assist you with creating and managing your bot)

![0](/images/0.png)

* Type **/help** to see all possible commands the botfather can handle

![1](/images/1.png)

* Click on or type **/newbot** to create a new bot.

![2](/images/2.png)

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
